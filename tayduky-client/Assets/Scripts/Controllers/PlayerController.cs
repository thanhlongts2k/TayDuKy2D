using System;
using UnityEngine;
using TayDuKy.Network;
using TayDuKy.Managers;

namespace TayDuKy.Controllers
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Properties")]
        [SerializeField] private float speed = 5.0f;
        [SerializeField] private float gridSize = 1.0f; // Each tile is 1x1 unit

        [Header("Faction Sprite Sheets (16 Frames: Up=0-3, Right=4-7, Left=8-11, Down=12-15)")]
        [SerializeField] private Sprite[] thanTocSprites = new Sprite[16];
        [SerializeField] private Sprite[] maTocSprites = new Sprite[16];
        [SerializeField] private Sprite[] yeuTocSprites = new Sprite[16];

        [Header("Animation Settings")]
        [SerializeField] private float frameRate = 0.15f; // Frame step duration

        public static event Action<Vector3> OnPlayerMoved;

        private Vector3 targetPosition;
        private bool isMoving = false;
        private int characterId = 1024; // Mock Character ID

        private Sprite[] activeSprites;
        private float frameTimer = 0f;
        private int currentFrameIndex = 0;
        private enum CharacterDirection { Up, Right, Left, Down }
        private CharacterDirection currentDirection = CharacterDirection.Down;
        private SpriteRenderer spriteRenderer;

        public int CharacterId => characterId;
        public bool IsMoving => isMoving;


        public void SetCharacter(int id, string faction)
        {
            characterId = id;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer != null)
            {
                if (faction == "Thần Tộc")
                {
                    activeSprites = thanTocSprites;
                }
                else if (faction == "Ma Tộc")
                {
                    activeSprites = maTocSprites;
                }
                else if (faction == "Yêu Tộc")
                {
                    activeSprites = yeuTocSprites;
                }
                
                if (activeSprites != null && activeSprites.Length == 16)
                {
                    spriteRenderer.sprite = activeSprites[12]; // Default to Down idle
                }
                spriteRenderer.color = Color.white; // Clear fallback cyan color
            }
            Debug.Log($"PlayerController: Set character ID={id}, Faction={faction}");
        }

        public Sprite GetFactionDefaultSprite(string factionName)
        {
            string f = factionName.ToLower().Trim();
            if (f == "thần tộc" || f == "than_toc")
                return thanTocSprites != null && thanTocSprites.Length == 16 ? thanTocSprites[12] : null;
            if (f == "ma tộc" || f == "ma_toc")
                return maTocSprites != null && maTocSprites.Length == 16 ? maTocSprites[12] : null;
            if (f == "yêu tộc" || f == "yeu_toc")
                return yeuTocSprites != null && yeuTocSprites.Length == 16 ? yeuTocSprites[12] : null;
            return null;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            targetPosition = transform.position;
            // Initialize default activeSprites for testing if none is set yet
            if (activeSprites == null || activeSprites.Length == 0)
            {
                activeSprites = thanTocSprites;
                if (activeSprites != null && activeSprites.Length == 16 && spriteRenderer != null)
                {
                    spriteRenderer.sprite = activeSprites[12];
                }
            }
        }

        private void Update()
        {
            // 1. Handle D-pad Keyboard inputs for testing (WASD / Arrow Keys)
            if (!isMoving)
            {
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");

                if (horizontal != 0)
                {
                    MoveGrid(new Vector3(horizontal * gridSize, 0, 0));
                }
                else if (vertical != 0)
                {
                    MoveGrid(new Vector3(0, vertical * gridSize, 0));
                }
            }

            // 2. Handle Tap-to-move input (Mouse Click/Touch screen)
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                clickPosition.z = 0; // Maintain 2D plane
                
                // Align target click to grid cell coordinate
                float snapX = Mathf.Round(clickPosition.x / gridSize) * gridSize;
                float snapY = Mathf.Round(clickPosition.y / gridSize) * gridSize;
                Vector3 target = new Vector3(snapX, snapY, 0);

                if (MapManager.Instance == null || MapManager.Instance.CanWalk(target))
                {
                    SetTargetPosition(target);
                }
            }

            // 3. Perform movement smoothing & track movement direction
            if (isMoving)
            {
                Vector3 lastPos = transform.position;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                
                // Determine facing direction from movement delta
                Vector3 movement = targetPosition - lastPos;
                if (movement.magnitude > 0.001f)
                {
                    if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
                    {
                        currentDirection = movement.x > 0 ? CharacterDirection.Right : CharacterDirection.Left;
                    }
                    else
                    {
                        currentDirection = movement.y > 0 ? CharacterDirection.Up : CharacterDirection.Down;
                    }
                }

                if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
                {
                    transform.position = targetPosition;
                    isMoving = false;
                }
            }

            // 4. Update sprite animations based on state and direction
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (activeSprites == null || activeSprites.Length != 16 || spriteRenderer == null) return;

            if (isMoving)
            {
                frameTimer += Time.deltaTime;
                if (frameTimer >= frameRate)
                {
                    frameTimer = 0f;
                    currentFrameIndex = (currentFrameIndex + 1) % 4; // Cycle through 4 columns
                }
            }
            else
            {
                currentFrameIndex = 0; // Stand still (Idle Column 0)
                frameTimer = 0f;
            }

            // Calculate row offsets matching standard
            int offset = 0;
            switch (currentDirection)
            {
                case CharacterDirection.Up:    offset = 0;  break; // Row 0
                case CharacterDirection.Right: offset = 4;  break; // Row 1
                case CharacterDirection.Left:  offset = 8;  break; // Row 2
                case CharacterDirection.Down:  offset = 12; break; // Row 3
            }

            int spriteIndex = offset + currentFrameIndex;
            if (spriteIndex >= 0 && spriteIndex < activeSprites.Length)
            {
                spriteRenderer.sprite = activeSprites[spriteIndex];
            }
        }

        private void MoveGrid(Vector3 direction)
        {
            Vector3 newTarget = transform.position + direction;
            if (MapManager.Instance != null && !MapManager.Instance.CanWalk(newTarget))
            {
                Debug.Log($"Movement blocked at grid ({newTarget.x}, {newTarget.y}) by obstacle.");
                return;
            }
            SetTargetPosition(newTarget);
        }

        private void SetTargetPosition(Vector3 newPosition)
        {
            Vector3 oldPosition = targetPosition;
            targetPosition = newPosition;
            isMoving = true;

            // Notify pet / followers
            OnPlayerMoved?.Invoke(oldPosition);

            // Send movement packet to Golang Server
            SendMovePacket(targetPosition);
        }

        public void TeleportTo(Vector3 position, int mapId)
        {
            transform.position = position;
            targetPosition = position;
            isMoving = false;

            // Notify pet to snap as well
            OnPlayerMoved?.Invoke(position);

            SendMovePacket(position, mapId);
        }

        private void SendMovePacket(Vector3 target, int mapId = -1)
        {
            if (NetworkClient.Instance == null) return;

            // Convert position to integer grid tile coordinates for server validation
            int gridX = Mathf.RoundToInt(target.x / gridSize);
            int gridY = Mathf.RoundToInt(target.y / gridSize);

            // Determine direction string for server
            string dirStr = "SOUTH";
            switch (currentDirection)
            {
                case CharacterDirection.Up:    dirStr = "NORTH"; break;
                case CharacterDirection.Right: dirStr = "EAST";  break;
                case CharacterDirection.Left:  dirStr = "WEST";  break;
                case CharacterDirection.Down:  dirStr = "SOUTH"; break;
            }

            int finalMapId = mapId;
            if (finalMapId == -1)
            {
                finalMapId = MapManager.Instance != null ? MapManager.Instance.ActiveMapId : 101;
            }

            // Construct JSON Move Packet matching Action ID 1001 with map_id
            string movePayload = $"{{\"action_id\": 1001, \"character_id\": {characterId}, \"map_id\": {finalMapId}, \"target_x\": {gridX}, \"target_y\": {gridY}, \"direction\": \"{dirStr}\", \"is_riding\": false}}";
            
            NetworkClient.Instance.SendPacket(movePayload);
            Debug.Log($"Client sent move packet: Map={finalMapId}, X={gridX}, Y={gridY}, Direction={dirStr}");
        }
    }
}
