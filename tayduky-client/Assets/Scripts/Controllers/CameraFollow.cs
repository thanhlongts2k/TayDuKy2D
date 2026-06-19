using UnityEngine;

namespace TayDuKy.Controllers
{
    /// <summary>
    /// Gắn vào Main Camera để tự động theo dõi (follow) nhân vật người chơi.
    /// Dùng LateUpdate để camera cập nhật sau khi nhân vật đã di chuyển xong trong frame.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Follow Target")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 5f;

        [Header("Camera Offset (relative to target)")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        [Header("Camera Bounds (0 = vô hạn)")]
        [SerializeField] private float minX = 0f;
        [SerializeField] private float maxX = 0f;
        [SerializeField] private float minY = 0f;
        [SerializeField] private float maxY = 0f;
        [SerializeField] private bool useBounds = false;

        [Header("Zoom (Scroll Wheel)")]
        [SerializeField] private float minZoom = 3.5f; // Prevent zooming out too far (UI would break at < 3.5)
        [SerializeField] private float maxZoom = 9f;
        [SerializeField] private float scrollSensitivity = 1.5f;

        private Camera cam;
        private float lastMapWidth = 24f;
        private float lastMapHeight = 24f;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void Update()
        {
            // Scroll wheel zoom (only when camera is orthographic)
            if (cam != null && cam.orthographic)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.001f)
                {
                    cam.orthographicSize = Mathf.Clamp(
                        cam.orthographicSize - scroll * scrollSensitivity,
                        minZoom, maxZoom);
                    RecalculateBounds();
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;

            // Clamp camera within map bounds if configured
            if (useBounds)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
            }

            // Smooth lerp to desired position
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Đặt target cho camera follow từ code (dùng trong SetupTestScene hoặc NetworkClient sau login).
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            // Snap camera immediately to target on first assignment
            if (newTarget != null)
            {
                transform.position = newTarget.position + offset;
            }
            Debug.Log($"CameraFollow: Target set to '{(newTarget != null ? newTarget.name : "null")}'.");
        }

        /// <summary>
        /// Cập nhật giới hạn camera theo kích thước map hiện tại.
        /// </summary>
        public void SetMapBounds(float mapWidth, float mapHeight)
        {
            lastMapWidth = mapWidth;
            lastMapHeight = mapHeight;
            RecalculateBounds();
        }

        private void RecalculateBounds()
        {
            if (cam == null) cam = GetComponent<Camera>();
            if (cam == null) return;

            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            minX = halfWidth;
            maxX = lastMapWidth - halfWidth;
            minY = halfHeight;
            maxY = lastMapHeight - halfHeight;

            // Only use bounds if the map is large enough for clamping to make sense
            useBounds = (maxX > minX) && (maxY > minY);
            Debug.Log($"CameraFollow: Bounds updated to X[{minX:F1},{maxX:F1}] Y[{minY:F1},{maxY:F1}]. UseBounds={useBounds}");
        }
    }
}
