using UnityEngine;

namespace TayDuKy.UI
{
    /// <summary>
    /// Hiệu ứng vòng tròn cổng dịch chuyển nhấp nháy, đổi kích thước (pulse animation) 
    /// tạo cảm giác ma thuật sống động chốn bồng lai tiên cảnh Tây Du Ký.
    /// </summary>
    public class PortalDot : MonoBehaviour
    {
        private SpriteRenderer sr;
        private float baseScale = 0.4f;
        private float pulseSpeed = 3.5f;
        private float pulseRange = 0.08f;
        private float timer = 0f;

        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            timer = Random.Range(0f, 2f * Mathf.PI); // Khởi tạo lệch pha để tránh đồng loạt
        }

        private void Update()
        {
            timer += Time.deltaTime * pulseSpeed;
            float scaleOffset = Mathf.Sin(timer) * pulseRange;
            transform.localScale = new Vector3(baseScale + scaleOffset, baseScale + scaleOffset, 1f);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0.5f + Mathf.Sin(timer) * 0.2f; // Pulse độ mờ từ 0.3 đến 0.7
                sr.color = c;
            }
        }
    }
}
