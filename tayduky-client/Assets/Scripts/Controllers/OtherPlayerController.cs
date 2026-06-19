using UnityEngine;

namespace TayDuKy.Controllers
{
    public class OtherPlayerController : MonoBehaviour
    {
        [Header("Movement Properties")]
        [SerializeField] private float speed = 5.0f;
        [SerializeField] private float gridSize = 1.0f;

        [Header("Faction Sprite Sheets (16 Frames: Up=0-3, Right=4-7, Left=8-11, Down=12-15)")]
        [SerializeField] private Sprite[] thanTocSprites = new Sprite[16];
        [SerializeField] private Sprite[] maTocSprites = new Sprite[16];
        [SerializeField] private Sprite[] yeuTocSprites = new Sprite[16];

        [Header("Animation Settings")]
        [SerializeField] private float frameRate = 0.15f;

        private int characterId;
        private string characterName;
        private Vector3 targetPosition;
        private bool isMoving = false;

        private Sprite[] activeSprites;
        private float frameTimer = 0f;
        private int currentFrameIndex = 0;
        private enum CharacterDirection { Up, Right, Left, Down }
        private CharacterDirection currentDirection = CharacterDirection.Down;
        private SpriteRenderer spriteRenderer;

        // Nameplate – floating text above character head
        private GameObject nameplateObj;
        private TextMesh nameplateText;
        private int characterLevel = 1;

        public int CharacterId => characterId;

        public void SetCharacter(int id, string name, string faction, int level = 1)
        {
            characterId    = id;
            characterName  = name;
            characterLevel = level;
            gameObject.name = $"OtherPlayer_{name}_{id}";

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                string f = faction.ToLower().Trim();
                if (f == "thần tộc" || f == "than_toc") activeSprites = thanTocSprites;
                else if (f == "ma tộc"   || f == "ma_toc")  activeSprites = maTocSprites;
                else if (f == "yêu tộc"  || f == "yeu_toc") activeSprites = yeuTocSprites;

                if (activeSprites != null && activeSprites.Length == 16)
                    spriteRenderer.sprite = activeSprites[12]; // Down idle

                spriteRenderer.color = Color.white;
            }

            UpdateNameplate(name, level);
            Debug.Log($"OtherPlayerController: Setup player ID={id}, Name={name}, Faction={faction}");
        }

        public void SetSpriteSheets(Sprite[] than, Sprite[] ma, Sprite[] yeu)
        {
            thanTocSprites = than;
            maTocSprites = ma;
            yeuTocSprites = yeu;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            targetPosition = transform.position;
        }

        private void Update()
        {
            if (isMoving)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
                {
                    transform.position = targetPosition;
                    isMoving = false;
                }
            }

            UpdateAnimation();
        }

        public void MoveTo(Vector3 target, string dirStr)
        {
            targetPosition = target;
            isMoving = true;

            string d = dirStr.ToUpper().Trim();
            if (d == "NORTH") currentDirection = CharacterDirection.Up;
            else if (d == "EAST") currentDirection = CharacterDirection.Right;
            else if (d == "WEST") currentDirection = CharacterDirection.Left;
            else if (d == "SOUTH") currentDirection = CharacterDirection.Down;
        }

        public void TeleportTo(Vector3 pos)
        {
            transform.position = pos;
            targetPosition = pos;
            isMoving = false;
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
                    currentFrameIndex = (currentFrameIndex + 1) % 4;
                }
            }
            else
            {
                currentFrameIndex = 0;
                frameTimer = 0f;
            }

            int offset = 0;
            switch (currentDirection)
            {
                case CharacterDirection.Up:    offset = 0;  break;
                case CharacterDirection.Right: offset = 4;  break;
                case CharacterDirection.Left:  offset = 8;  break;
                case CharacterDirection.Down:  offset = 12; break;
            }

            int spriteIndex = offset + currentFrameIndex;
            if (spriteIndex >= 0 && spriteIndex < activeSprites.Length)
                spriteRenderer.sprite = activeSprites[spriteIndex];
        }

        /// <summary>
        /// Creates or updates the floating nameplate text above this other player.
        /// </summary>
        private void UpdateNameplate(string displayName, int level)
        {
            if (nameplateObj == null)
            {
                nameplateObj = new GameObject("_Nameplate");
                nameplateObj.transform.SetParent(transform, false);
                nameplateObj.transform.localPosition = new Vector3(0f, 1.1f, 0f);
                nameplateObj.transform.localScale     = Vector3.one;

                nameplateText = nameplateObj.AddComponent<TextMesh>();
                nameplateText.fontSize      = 28;
                nameplateText.characterSize = 0.10f;
                nameplateText.alignment     = TextAlignment.Center;
                nameplateText.anchor        = TextAnchor.MiddleCenter;
                nameplateText.fontStyle     = FontStyle.Bold;

                var mr = nameplateObj.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    mr.sortingLayerName = "Default";
                    mr.sortingOrder     = 50;
                }
            }

            if (nameplateText != null)
            {
                // Other players: cyan nameplate to distinguish from local player
                nameplateText.text  = $"{displayName}  Lv.{level}";
                nameplateText.color = new Color(0.4f, 1f, 1f);
            }
        }
    }
}
