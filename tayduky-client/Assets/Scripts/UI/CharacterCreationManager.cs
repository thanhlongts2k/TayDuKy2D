using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TayDuKy.Network;

namespace TayDuKy.UI
{
    public class CharacterCreationManager : MonoBehaviour
    {
        public static CharacterCreationManager Instance { get; private set; }

        [System.Serializable]
        public class PlayableClass
        {
            public string name;        // Kiếm Tiên, Pháp Sư...
            public string faction;     // Thần Tộc, Ma Tộc, Yêu Tộc
            public string gender;      // Nam, Nữ
            public string weapon;      // Phi Kiếm, Trượng, Đao...
            public int base_hp;
            public int base_atk;
            public int base_def;
            public string description;
        }

        [Header("UI Inputs")]
        [SerializeField] private InputField characterNameInput;
        [SerializeField] private Text statusText;

        [Header("Character Selection Display")]
        [SerializeField] private Image characterAvatarImage;
        [SerializeField] private Text characterNameLabel; // Displays class name
        [SerializeField] private Text factionLabel;       // Displays Faction and Gender
        [SerializeField] private Text rarityLabel;        // Displays Weapon type
        [SerializeField] private Text statsLabel;         // HP, ATK, DEF
        [SerializeField] private Text descriptionLabel;

        [Header("Navigation Buttons")]
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;

        [Header("Submit Button")]
        [SerializeField] private Button createButton;

        [Header("Avatar Sprites")]
        [SerializeField] private Sprite thanTocSprite;
        [SerializeField] private Sprite maTocSprite;
        [SerializeField] private Sprite yeuTocSprite;

        private List<PlayableClass> playableClasses = new List<PlayableClass>();
        private int currentIndex = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (createButton != null)
                createButton.onClick.AddListener(OnCreateCharacterClick);

            if (prevButton != null)
                prevButton.onClick.AddListener(OnPrevClick);

            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextClick);

            InitializePlayableClasses();
            UpdateCharacterDisplay();
            ShowStatus("Hãy chọn nhân vật đại diện cho người thứ 5 và đặt tên!");
        }

        private void InitializePlayableClasses()
        {
            // Thiết lập 6 nhân vật đại diện (Chủ tướng - Người thứ 5 thỉnh kinh)
            playableClasses = new List<PlayableClass>
            {
                new PlayableClass {
                    name = "Kiếm Tiên",
                    faction = "Thần Tộc",
                    gender = "Nam",
                    weapon = "Cổ Kiếm",
                    base_hp = 120,
                    base_atk = 22,
                    base_def = 12,
                    description = "Thượng tiên tu đạo nho nhã, chỉ huy ngự kiếm phi hành tầm xa, phòng ngự cực tốt."
                },
                new PlayableClass {
                    name = "Pháp Sư",
                    faction = "Thần Tộc",
                    gender = "Nữ",
                    weapon = "Ngọc Trượng",
                    base_hp = 90,
                    base_atk = 28,
                    base_def = 8,
                    description = "Tiên tử phái Dao Trì thanh khiết, sở hữu sức mạnh phép thuật Thiên Đình tấn công diện rộng."
                },
                new PlayableClass {
                    name = "Ma Đao",
                    faction = "Ma Tộc",
                    gender = "Nam",
                    weapon = "U Minh Đao",
                    base_hp = 130,
                    base_atk = 22,
                    base_def = 12,
                    description = "Ma chiến sĩ dũng mãnh kiên cường, đao pháp bá đạo càn quét u minh, HP dồi dào."
                },
                new PlayableClass {
                    name = "Sát Thủ",
                    faction = "Ma Tộc",
                    gender = "Nữ",
                    weapon = "Song Đoản Đao",
                    base_hp = 110,
                    base_atk = 24,
                    base_def = 9,
                    description = "Nữ sát thủ ma tộc thoắt ẩn thoắt hiện, tốc độ chớp nhoáng và chí mạng cực kỳ cao."
                },
                new PlayableClass {
                    name = "Yêu Vương",
                    faction = "Yêu Tộc",
                    gender = "Nam",
                    weapon = "Hổ Đầu Thương",
                    base_hp = 150,
                    base_atk = 19,
                    base_def = 16,
                    description = "Yêu vương hộ vệ cường tráng lực lưỡng, mình đồng da sắt, sinh lực trâu bò phản sát thương."
                },
                new PlayableClass {
                    name = "Yêu Pháp",
                    faction = "Yêu Tộc",
                    gender = "Nữ",
                    weapon = "Vũ Linh Phiến",
                    base_hp = 120,
                    base_atk = 21,
                    base_def = 11,
                    description = "Yêu linh rừng sâu tinh nghịch xinh đẹp, dùng quạt phép gây khống chế suy yếu đối thủ."
                }
            };
        }

        private void UpdateCharacterDisplay()
        {
            if (playableClasses == null || playableClasses.Count == 0) return;

            var charClass = playableClasses[currentIndex];

            if (characterNameLabel != null) characterNameLabel.text = charClass.name;
            if (factionLabel != null) factionLabel.text = $"Thế Lực: {charClass.faction} ({charClass.gender})";
            if (rarityLabel != null) rarityLabel.text = $"Vũ Khí: {charClass.weapon}";
            if (descriptionLabel != null) descriptionLabel.text = charClass.description;

            if (statsLabel != null)
            {
                statsLabel.text = $"Máu: {charClass.base_hp}  |  Công: {charClass.base_atk}  |  Thủ: {charClass.base_def}";
            }

            // Gán Avatar theo chủng tộc
            if (characterAvatarImage != null)
            {
                if (charClass.faction == "Thần Tộc")
                {
                    characterAvatarImage.sprite = thanTocSprite;
                }
                else if (charClass.faction == "Ma Tộc")
                {
                    characterAvatarImage.sprite = maTocSprite;
                }
                else // Yêu Tộc
                {
                    characterAvatarImage.sprite = yeuTocSprite;
                }
            }

            // Đặt tên gợi ý theo phái
            if (characterNameInput != null)
            {
                characterNameInput.text = "TieuLam_" + charClass.name;
            }
        }

        public void OnPrevClick()
        {
            if (playableClasses == null || playableClasses.Count == 0) return;
            currentIndex--;
            if (currentIndex < 0) currentIndex = playableClasses.Count - 1;
            UpdateCharacterDisplay();
        }

        public void OnNextClick()
        {
            if (playableClasses == null || playableClasses.Count == 0) return;
            currentIndex++;
            if (currentIndex >= playableClasses.Count) currentIndex = 0;
            UpdateCharacterDisplay();
        }

        public void OnCreateCharacterClick()
        {
            if (characterNameInput == null || playableClasses == null || playableClasses.Count == 0) return;

            string charName = characterNameInput.text.Trim();
            if (string.IsNullOrEmpty(charName))
            {
                ShowStatus("Tên nhân vật không được để trống!", true);
                return;
            }

            if (NetworkClient.Instance == null)
            {
                ShowStatus("Lỗi kết nối mạng!", true);
                return;
            }

            string username = NetworkClient.Instance.LoggedInUsername;
            if (string.IsNullOrEmpty(username))
            {
                username = "guest_" + UnityEngine.Random.Range(1000, 9999);
            }

            var selectedClass = playableClasses[currentIndex];
            ShowStatus("Đang khởi tạo nhân vật thứ 5...");

            // Gửi gói tin tạo nhân vật lên server (Action ID 1003)
            string createPayload = $"{{\"action_id\": 1003, \"username\": \"{username}\", \"name\": \"{charName}\", \"faction\": \"{selectedClass.faction}\", \"class\": \"{selectedClass.name}\", \"gender\": \"{selectedClass.gender}\"}}";
            NetworkClient.Instance.SendPacket(createPayload);
            Debug.Log($"Client sent character creation: Name={charName}, Faction={selectedClass.faction}, Class={selectedClass.name}, Gender={selectedClass.gender}");
        }

        public void ShowStatus(string msg, bool isError = false)
        {
            if (statusText == null) return;
            statusText.text = msg;
            statusText.color = isError ? Color.red : Color.yellow;
        }
    }
}
