using System.Collections.Generic;
using UnityEngine;

namespace TayDuKy.Controllers
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PetController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float baseSpeed = 5.0f;
        [SerializeField] private float catchUpSpeedMultiplier = 1.5f;
        
        [Header("Sprite Sheet (16 Frames)")]
        [SerializeField] private Sprite[] sprites = new Sprite[16];

        [Header("Animation Settings")]
        [SerializeField] private float frameRate = 0.15f;

        private Queue<Vector3> targetQueue = new Queue<Vector3>();
        private Vector3 currentTargetPos;
        private bool isMoving = false;
        private float frameTimer = 0f;
        private int currentFrameIndex = 0;
        private SpriteRenderer spriteRenderer;

        private enum CharacterDirection { Up, Right, Left, Down }
        private CharacterDirection currentDirection = CharacterDirection.Down;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            PlayerController.OnPlayerMoved += OnPlayerMoved;
        }

        private void OnDisable()
        {
            PlayerController.OnPlayerMoved -= OnPlayerMoved;
        }

        public void SetSprites(Sprite[] newSprites)
        {
            sprites = newSprites;
            if (sprites != null && sprites.Length == 16 && spriteRenderer != null)
            {
                spriteRenderer.sprite = sprites[12]; // Default Down Idle
            }
        }

        private void OnPlayerMoved(Vector3 oldGridPos)
        {
            // Add player's previous grid coordinate to the follow queue
            targetQueue.Enqueue(oldGridPos);
        }

        private void Start()
        {
            // Spawn pet at player's current location to start side-by-side
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                transform.position = player.transform.position;
                currentTargetPos = transform.position;
            }
        }

        private void Update()
        {
            // 1. Process follow target queue
            if (!isMoving && targetQueue.Count > 0)
            {
                currentTargetPos = targetQueue.Dequeue();
                isMoving = true;
            }

            // 2. Perform step-by-step movement interpolation
            if (isMoving)
            {
                // Dynamic speed bonus if player moves far ahead
                float currentSpeed = baseSpeed;
                if (targetQueue.Count > 2)
                {
                    currentSpeed *= catchUpSpeedMultiplier;
                }

                Vector3 lastPos = transform.position;
                transform.position = Vector3.MoveTowards(transform.position, currentTargetPos, currentSpeed * Time.deltaTime);

                // Calculate direction based on target heading
                Vector3 movement = currentTargetPos - lastPos;
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

                // Snapped arrival check
                if (Vector3.Distance(transform.position, currentTargetPos) < 0.01f)
                {
                    transform.position = currentTargetPos;
                    isMoving = false;
                }
            }

            // 3. Tick animation frames
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (sprites == null || sprites.Length != 16 || spriteRenderer == null) return;

            if (isMoving)
            {
                frameTimer += Time.deltaTime;
                if (frameTimer >= frameRate)
                {
                    frameTimer = 0f;
                    currentFrameIndex = (currentFrameIndex + 1) % 4; // Loop 4 walking steps
                }
            }
            else
            {
                currentFrameIndex = 0; // Return to idle (frame 0)
                frameTimer = 0f;
            }

            // Determine row offsets (Row 0=Up, Row 1=Right, Row 2=Left, Row 3=Down)
            int offset = 0;
            switch (currentDirection)
            {
                case CharacterDirection.Up:    offset = 0;  break;
                case CharacterDirection.Right: offset = 4;  break;
                case CharacterDirection.Left:  offset = 8;  break;
                case CharacterDirection.Down:  offset = 12; break;
            }

            int index = offset + currentFrameIndex;
            if (index >= 0 && index < sprites.Length)
            {
                spriteRenderer.sprite = sprites[index];
            }
        }
    }
}
