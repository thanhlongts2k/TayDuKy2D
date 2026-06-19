using System;
using UnityEngine;
using UnityEngine.UI;

namespace TayDuKy.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Top-Left Character Stats")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Slider mpSlider;
        [SerializeField] private Text levelText;
        [SerializeField] private Text nameText;

        [Header("Bottom Chat UI")]
        [SerializeField] private Text chatHistoryText;
        [SerializeField] private InputField chatInputField;
        [SerializeField] private GameObject chatPanel;

        [Header("Sub-Panels")]
        [SerializeField] private GameObject inventoryPanel; // Panel Bảng
        [SerializeField] private GameObject shopPanel;      // Panel Shop
        [SerializeField] private GameObject sysPanel;       // Panel Sys
        [SerializeField] private GameObject factionPanel;   // Panel Khu Phái

        [Header("Scene Navigation Panels")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject characterCreationPanel;
        [SerializeField] private GameObject worldPanel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Debug.Log($"UIManager: Start called. loginPanel={loginPanel}, characterCreationPanel={characterCreationPanel}, worldPanel={worldPanel}");
            ShowLogin();
        }

        public void ShowLogin()
        {
            Debug.Log("UIManager: ShowLogin called");
            if (loginPanel != null) loginPanel.SetActive(true);
            if (characterCreationPanel != null) characterCreationPanel.SetActive(false);
            if (worldPanel != null) worldPanel.SetActive(false);
        }

        public void ShowCharacterCreation()
        {
            Debug.Log("UIManager: ShowCharacterCreation called");
            if (loginPanel != null) loginPanel.SetActive(false);
            if (characterCreationPanel != null) characterCreationPanel.SetActive(true);
            if (worldPanel != null) worldPanel.SetActive(false);
        }

        public void ShowWorld()
        {
            Debug.Log("UIManager: ShowWorld called");
            if (loginPanel != null) loginPanel.SetActive(false);
            if (characterCreationPanel != null) characterCreationPanel.SetActive(false);
            if (worldPanel != null) worldPanel.SetActive(true);
        }

        public void UpdateCharacterStats(string name, int level, int hpCur, int hpMax, int mpCur, int mpMax)
        {
            if (nameText != null) nameText.text = name;
            if (levelText != null) levelText.text = $"Lvl {level}";
            
            if (hpSlider != null)
            {
                hpSlider.maxValue = hpMax;
                hpSlider.value = hpCur;
            }
            if (mpSlider != null)
            {
                mpSlider.maxValue = mpMax;
                mpSlider.value = mpCur;
            }
        }

        // --- Bottom Buttons Event Handlers ---
        
        // Bấm nút "Bảng"
        public void OnClickBangButton()
        {
            TogglePanel(inventoryPanel);
            CloseOtherPanels(inventoryPanel);
            Debug.Log("UI: Opened Inventory/Character Panel (Bảng)");
        }

        // Bấm nút "Nhanh" (Auto / Phím tắt)
        public void OnClickNhanhButton()
        {
            Debug.Log("UI: Toggle Auto-Battle mode (Nhanh)");
            // Trigger auto battle setup in combat manager
        }

        // Bấm nút "Khu Phái"
        public void OnClickKhuPhaiButton()
        {
            TogglePanel(factionPanel);
            CloseOtherPanels(factionPanel);
            Debug.Log("UI: Opened Faction/Teleport Menu (Khu Phái)");
        }

        // Bấm nút "Shop"
        public void OnClickShopButton()
        {
            TogglePanel(shopPanel);
            CloseOtherPanels(shopPanel);
            Debug.Log("UI: Opened Shop Panel (Shop)");
        }

        // Bấm nút "Sys"
        public void OnClickSysButton()
        {
            TogglePanel(sysPanel);
            CloseOtherPanels(sysPanel);
            Debug.Log("UI: Opened System Settings (Sys)");
        }

        // --- Chat UI Actions ---

        public void AppendChatMessage(string sender, string message)
        {
            if (chatHistoryText != null)
            {
                chatHistoryText.text += $"\\n<b>{sender}:</b> {message}";
            }
        }

        public void OnSendChatSubmit()
        {
            if (chatInputField == null || string.IsNullOrEmpty(chatInputField.text)) return;

            string chatText = chatInputField.text;
            chatInputField.text = ""; // Clear input field

            // Intercept developer debug/cheat commands
            if (chatText.StartsWith("/"))
            {
                ProcessDevCommand(chatText);
                return;
            }

            // Send through ChatManager to server
            if (Managers.ChatManager.Instance != null)
            {
                Managers.ChatManager.Instance.SendWorldChat(chatText);
            }
        }

        private void ProcessDevCommand(string commandLine)
        {
            string cleanCommand = commandLine.Trim();
            string[] parts = cleanCommand.Split(' ');
            string cmd = parts[0].ToLower();

            if (cmd == "/pet")
            {
                if (parts.Length > 1 && int.TryParse(parts[1], out int petId))
                {
                    if (petId == 0)
                    {
                        Managers.MountAndPetManager.Instance.UnsummonPet();
                        AppendChatMessage("Hệ thống", "Đã thu hồi Pet.");
                    }
                    else if (petId == 101 || petId == 102)
                    {
                        Managers.MountAndPetManager.Instance.SummonPet(petId);
                        string petName = petId == 101 ? "Thỏ Ngọc" : "Tiểu Toàn Phong";
                        AppendChatMessage("Hệ thống", $"Đã gửi lệnh triệu hồi: {petName}.");
                    }
                    else
                    {
                        AppendChatMessage("Hệ thống", "ID Pet không hợp lệ! Dùng 101 (Thỏ Ngọc) hoặc 102 (Tiểu Toàn Phong).");
                    }
                }
                else
                {
                    AppendChatMessage("Hệ thống", "Cú pháp: /pet [ID] (Ví dụ: /pet 101, hoặc /pet 0 để thu hồi)");
                }
            }
            else if (cmd == "/unpet")
            {
                Managers.MountAndPetManager.Instance.UnsummonPet();
                AppendChatMessage("Hệ thống", "Đã thu hồi Pet.");
            }
            else if (cmd == "/mount")
            {
                if (parts.Length > 1 && int.TryParse(parts[1], out int mountId))
                {
                    if (mountId == 0)
                    {
                        Managers.MountAndPetManager.Instance.UnequipMount();
                        AppendChatMessage("Hệ thống", "Đã xuống thú cưỡi.");
                    }
                    else if (mountId == 1 || mountId == 2)
                    {
                        Managers.MountAndPetManager.Instance.EquipMount(mountId);
                        string mName = mountId == 1 ? "U Minh Bạch Hổ" : "Lục Bảo Phi Kiếm";
                        AppendChatMessage("Hệ thống", $"Đã gửi lệnh cưỡi thú: {mName}.");
                    }
                    else
                    {
                        AppendChatMessage("Hệ thống", "ID Thú cưỡi không hợp lệ! Dùng 1 hoặc 2.");
                    }
                }
                else
                {
                    AppendChatMessage("Hệ thống", "Cú pháp: /mount [ID] (Ví dụ: /mount 1, hoặc /mount 0 để xuống)");
                }
            }
            else if (cmd == "/unmount")
            {
                Managers.MountAndPetManager.Instance.UnequipMount();
                AppendChatMessage("Hệ thống", "Đã xuống thú cưỡi.");
            }
            else
            {
                AppendChatMessage("Hệ thống", $"Không tìm thấy lệnh phát triển: {cmd}");
            }
        }

        // --- Helper Panel Toggles ---

        private void TogglePanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(!panel.activeSelf);
            }
        }

        private void CloseOtherPanels(GameObject activePanel)
        {
            GameObject[] panels = { inventoryPanel, shopPanel, sysPanel, factionPanel };
            foreach (var panel in panels)
            {
                if (panel != null && panel != activePanel)
                {
                    panel.SetActive(false);
                }
            }
        }
    }
}
