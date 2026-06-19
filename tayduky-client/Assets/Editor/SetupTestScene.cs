using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TayDuKy.Network;
using TayDuKy.Controllers;
using TayDuKy.Managers;
using TayDuKy.UI;

namespace TayDuKy.Editor
{
    public class SetupTestScene : MonoBehaviour
    {
        [MenuItem("Tools/Tây Du Ký/Tự Động Lắp Ráp Scene Thử Nghiệm")]
        public static void BuildScene()
        {
            // 1. Create a new empty scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 2. Create the _NetworkManager and attach manager scripts
            GameObject networkManagerObj = new GameObject("_NetworkManager");
            var netClient = networkManagerObj.AddComponent<NetworkClient>();
            var chatMgr = networkManagerObj.AddComponent<ChatManager>();
            var questMgr = networkManagerObj.AddComponent<QuestManager>();
            var mountPetMgr = networkManagerObj.AddComponent<MountAndPetManager>();
            var combatMgr = networkManagerObj.AddComponent<CombatManager>();
            var configMgr = networkManagerObj.AddComponent<ConfigManager>();
            var mapMgr = networkManagerObj.AddComponent<MapManager>();

            Debug.Log("Editor: Created _NetworkManager with all Manager scripts attached.");

            // 0. Ensure textures are imported as Sprites
            ConfigureTextureAsSprite("Assets/Sprites/Characters/than_toc.png");
            ConfigureTextureAsSprite("Assets/Sprites/Characters/ma_toc.png");
            ConfigureTextureAsSprite("Assets/Sprites/Characters/yeu_toc.png");

            // 0.5. Ensure spritesheets are imported and sliced as Multiple Sprites
            ConfigureAndSliceSpriteSheet("Assets/Sprites/Characters/than_toc_sheet.png", 4, 4);
            ConfigureAndSliceSpriteSheet("Assets/Sprites/Characters/ma_toc_sheet.png", 4, 4);
            ConfigureAndSliceSpriteSheet("Assets/Sprites/Characters/yeu_toc_sheet.png", 4, 4);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            // Load all avatar sprites for CC Manager setup
            Sprite thanPortrait = LoadSprite("Assets/Sprites/Characters/than_toc.png");
            Sprite maPortrait = LoadSprite("Assets/Sprites/Characters/ma_toc.png");
            Sprite yeuPortrait = LoadSprite("Assets/Sprites/Characters/yeu_toc.png");

            // Load all world sprites sheets (16 frames) for Player setup
            Sprite[] thanSheetSprites = LoadAllSheetSprites("Assets/Sprites/Characters/than_toc_sheet.png");
            Sprite[] maSheetSprites = LoadAllSheetSprites("Assets/Sprites/Characters/ma_toc_sheet.png");
            Sprite[] yeuSheetSprites = LoadAllSheetSprites("Assets/Sprites/Characters/yeu_toc_sheet.png");

            Sprite defaultWorldSprite = thanSheetSprites != null && thanSheetSprites.Length == 16 ? thanSheetSprites[12] : null;

            // 3. Create the Player GameObject
            GameObject playerObj = new GameObject("Player");
            var playerController = playerObj.AddComponent<PlayerController>();
            
            // Add a simple SpriteRenderer for visual representation
            var spriteRenderer = playerObj.AddComponent<SpriteRenderer>();
            spriteRenderer.color = Color.white;

            if (defaultWorldSprite != null)
            {
                spriteRenderer.sprite = defaultWorldSprite;
                Debug.Log("Editor: Assigned than_toc_sheet_down_0 sprite to Player.");
            }
            else
            {
                // Fallback to dynamic texture if asset not imported yet
                Texture2D tex = new Texture2D(16, 16);
                for (int y = 0; y < tex.height; y++)
                {
                    for (int x = 0; x < tex.width; x++)
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                }
                tex.Apply();
                spriteRenderer.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 16.0f);
                spriteRenderer.color = Color.cyan;
                Debug.Log("Editor: Created Player with fallback cyan square.");
            }

            // Bind faction sprites to PlayerController (using world spritesheets)
            SerializedObject serPlayer = new SerializedObject(playerController);
            AssignSpriteArray(serPlayer.FindProperty("thanTocSprites"), thanSheetSprites);
            AssignSpriteArray(serPlayer.FindProperty("maTocSprites"), maSheetSprites);
            AssignSpriteArray(serPlayer.FindProperty("yeuTocSprites"), yeuSheetSprites);
            serPlayer.ApplyModifiedProperties();

            // Bind pet sprites to MountAndPetManager (using placeholders)
            SerializedObject serMP = new SerializedObject(mountPetMgr);
            AssignSpriteArray(serMP.FindProperty("thoNgocSprites"), thanSheetSprites); // Thỏ Ngọc -> Thần Tộc placeholder
            AssignSpriteArray(serMP.FindProperty("tieuToanPhongSprites"), yeuSheetSprites); // Tiểu Toàn Phong -> Yêu Tộc placeholder
            serMP.ApplyModifiedProperties();

            Debug.Log("Editor: Created Player GameObject with PlayerController and bound Faction & Pet Sprites.");

            // 4. Create UI Canvas and UI Objects
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create EventSystem if not exists
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            // Create UI Manager Component on Canvas
            var uiManager = canvasObj.AddComponent<UIManager>();

            // ==========================================
            // A. CREATE LOGIN PANEL
            // ==========================================
            GameObject loginPanelObj = new GameObject("LoginPanel");
            loginPanelObj.transform.SetParent(canvasObj.transform, false);
            var loginRect = loginPanelObj.AddComponent<RectTransform>();
            loginRect.anchorMin = Vector2.zero;
            loginRect.anchorMax = Vector2.one;
            loginRect.sizeDelta = Vector2.zero;
            var loginBg = loginPanelObj.AddComponent<Image>();
            loginBg.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            var loginMgr = loginPanelObj.AddComponent<LoginManager>();

            // Title Text
            GameObject titleObj = new GameObject("GameTitle");
            titleObj.transform.SetParent(loginPanelObj.transform, false);
            var titleTxt = titleObj.AddComponent<Text>();
            titleTxt.text = "TÂY DU KÝ 2D";
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.fontSize = 28;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = Color.yellow;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 150);
            titleRect.sizeDelta = new Vector2(300, 50);

            // Username Input Field
            GameObject userObj = CreateInputField("UsernameInput", loginPanelObj.transform, "Nhập tài khoản...", new Vector2(0, 50));
            InputField userField = userObj.GetComponent<InputField>();

            // Password Input Field
            GameObject passObj = CreateInputField("PasswordInput", loginPanelObj.transform, "Nhập mật khẩu...", new Vector2(0, 0));
            InputField passField = passObj.GetComponent<InputField>();
            passField.contentType = InputField.ContentType.Password;

            // Login Button
            GameObject btnLoginObj = CreateUIButton("BtnLogin", loginPanelObj.transform, "Đăng Nhập", new Color(0.8f, 0.6f, 0.2f), new Vector2(-55, -65), new Vector2(90, 35));
            Button loginBtn = btnLoginObj.GetComponent<Button>();

            // Register Button
            GameObject btnRegObj = CreateUIButton("BtnRegister", loginPanelObj.transform, "Đăng Ký", new Color(0.4f, 0.4f, 0.4f), new Vector2(55, -65), new Vector2(90, 35));
            Button regBtn = btnRegObj.GetComponent<Button>();

            // Status Text
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(loginPanelObj.transform, false);
            var statusTxt = statusObj.AddComponent<Text>();
            statusTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusTxt.fontSize = 14;
            statusTxt.color = Color.yellow;
            statusTxt.alignment = TextAnchor.MiddleCenter;
            var statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.anchoredPosition = new Vector2(0, -120);
            statusRect.sizeDelta = new Vector2(280, 40);

            // Bind references in LoginManager
            SerializedObject serLogin = new SerializedObject(loginMgr);
            serLogin.FindProperty("usernameInput").objectReferenceValue = userField;
            serLogin.FindProperty("passwordInput").objectReferenceValue = passField;
            serLogin.FindProperty("statusText").objectReferenceValue = statusTxt;
            serLogin.FindProperty("loginButton").objectReferenceValue = loginBtn;
            serLogin.FindProperty("registerButton").objectReferenceValue = regBtn;
            serLogin.ApplyModifiedProperties();

            // ==========================================
            // B. CREATE CHARACTER CREATION PANEL (TƯỚNG ĐỒNG HÀNH)
            // ==========================================
            GameObject ccPanelObj = new GameObject("CharacterCreationPanel");
            ccPanelObj.transform.SetParent(canvasObj.transform, false);
            var ccRect = ccPanelObj.AddComponent<RectTransform>();
            ccRect.anchorMin = Vector2.zero;
            ccRect.anchorMax = Vector2.one;
            ccRect.sizeDelta = Vector2.zero;
            var ccBg = ccPanelObj.AddComponent<Image>();
            ccBg.color = new Color(0.15f, 0.1f, 0.15f, 1f);

            var ccMgr = ccPanelObj.AddComponent<CharacterCreationManager>();

            // Faction Select Title
            GameObject ccTitleObj = new GameObject("CCTitle");
            ccTitleObj.transform.SetParent(ccPanelObj.transform, false);
            var ccTitleTxt = ccTitleObj.AddComponent<Text>();
            ccTitleTxt.text = "CHỌN HỆ PHÁI HẠ PHÀM";
            ccTitleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ccTitleTxt.fontSize = 24;
            ccTitleTxt.fontStyle = FontStyle.Bold;
            ccTitleTxt.color = Color.cyan;
            ccTitleTxt.alignment = TextAnchor.MiddleCenter;
            var ccTitleRect = ccTitleObj.GetComponent<RectTransform>();
            ccTitleRect.anchoredPosition = new Vector2(0, 160);
            ccTitleRect.sizeDelta = new Vector2(300, 40);

            // Avatar Display
            GameObject avatarObj = new GameObject("CharacterAvatar");
            avatarObj.transform.SetParent(ccPanelObj.transform, false);
            Image avatarImg = avatarObj.AddComponent<Image>();
            avatarImg.sprite = thanPortrait; // Default starting
            var avatarRect = avatarObj.GetComponent<RectTransform>();
            avatarRect.anchoredPosition = new Vector2(0, 50);
            avatarRect.sizeDelta = new Vector2(80, 80);

            // Character Name Label (Tên hệ phái)
            GameObject charNameLblObj = new GameObject("CharNameLabel");
            charNameLblObj.transform.SetParent(ccPanelObj.transform, false);
            Text charNameLbl = charNameLblObj.AddComponent<Text>();
            charNameLbl.text = "Tề Thiên Đại Thánh";
            charNameLbl.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            charNameLbl.fontSize = 18;
            charNameLbl.fontStyle = FontStyle.Bold;
            charNameLbl.color = Color.yellow;
            charNameLbl.alignment = TextAnchor.MiddleCenter;
            var charNameLblRect = charNameLblObj.GetComponent<RectTransform>();
            charNameLblRect.anchoredPosition = new Vector2(0, -10);
            charNameLblRect.sizeDelta = new Vector2(250, 25);

            // Faction Label
            GameObject factionLblObj = new GameObject("FactionLabel");
            factionLblObj.transform.SetParent(ccPanelObj.transform, false);
            Text factionLbl = factionLblObj.AddComponent<Text>();
            factionLbl.text = "Thế Lực: Thần Tộc";
            factionLbl.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            factionLbl.fontSize = 12;
            factionLbl.color = Color.white;
            factionLbl.alignment = TextAnchor.MiddleCenter;
            var factionLblRect = factionLblObj.GetComponent<RectTransform>();
            factionLblRect.anchoredPosition = new Vector2(0, -30);
            factionLblRect.sizeDelta = new Vector2(250, 20);

            // Rarity Label
            GameObject rarityLblObj = new GameObject("RarityLabel");
            rarityLblObj.transform.SetParent(ccPanelObj.transform, false);
            Text rarityLbl = rarityLblObj.AddComponent<Text>();
            rarityLbl.text = "Phẩm Chất: Thần Thoại";
            rarityLbl.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rarityLbl.fontSize = 12;
            rarityLbl.color = Color.red;
            rarityLbl.alignment = TextAnchor.MiddleCenter;
            var rarityLblRect = rarityLblObj.GetComponent<RectTransform>();
            rarityLblRect.anchoredPosition = new Vector2(0, -50);
            rarityLblRect.sizeDelta = new Vector2(250, 20);

            // Stats Label (HP, ATK, DEF)
            GameObject statsLblObj = new GameObject("StatsLabel");
            statsLblObj.transform.SetParent(ccPanelObj.transform, false);
            Text statsLbl = statsLblObj.AddComponent<Text>();
            statsLbl.text = "Máu: 650  |  Công: 85  |  Thủ: 45";
            statsLbl.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statsLbl.fontSize = 12;
            statsLbl.color = Color.green;
            statsLbl.alignment = TextAnchor.MiddleCenter;
            var statsLblRect = statsLblObj.GetComponent<RectTransform>();
            statsLblRect.anchoredPosition = new Vector2(0, -70);
            statsLblRect.sizeDelta = new Vector2(250, 20);

            // Description Label
            GameObject descLblObj = new GameObject("DescriptionLabel");
            descLblObj.transform.SetParent(ccPanelObj.transform, false);
            Text descLbl = descLblObj.AddComponent<Text>();
            descLbl.text = "Mô tả nhân vật...";
            descLbl.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descLbl.fontSize = 11;
            descLbl.fontStyle = FontStyle.Italic;
            descLbl.color = Color.gray;
            descLbl.alignment = TextAnchor.MiddleCenter;
            var descLblRect = descLblObj.GetComponent<RectTransform>();
            descLblRect.anchoredPosition = new Vector2(0, -95);
            descLblRect.sizeDelta = new Vector2(240, 30);

            // Name Input Field (Tên người chơi đặt)
            GameObject nameInputObj = CreateInputField("CharNameInput", ccPanelObj.transform, "Tên người chơi đặt...", new Vector2(0, -130));
            InputField nameField = nameInputObj.GetComponent<InputField>();

            // Navigation Buttons: Next & Prev
            GameObject btnPrevObj = CreateUIButton("BtnPrev", ccPanelObj.transform, "<", new Color(0.3f, 0.3f, 0.3f), new Vector2(-100, 50), new Vector2(40, 40));
            Button prevBtn = btnPrevObj.GetComponent<Button>();

            GameObject btnNextObj = CreateUIButton("BtnNext", ccPanelObj.transform, ">", new Color(0.3f, 0.3f, 0.3f), new Vector2(100, 50), new Vector2(40, 40));
            Button nextBtn = btnNextObj.GetComponent<Button>();

            // Create Button
            GameObject btnCreateObj = CreateUIButton("BtnCreateChar", ccPanelObj.transform, "Vào Thế Giới", Color.cyan, new Vector2(0, -175), new Vector2(150, 35));
            Button createCharBtn = btnCreateObj.GetComponent<Button>();

            // CC Status Text
            GameObject ccStatusObj = new GameObject("CCStatusText");
            ccStatusObj.transform.SetParent(ccPanelObj.transform, false);
            var ccStatusTxt = ccStatusObj.AddComponent<Text>();
            ccStatusTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ccStatusTxt.fontSize = 14;
            ccStatusTxt.color = Color.yellow;
            ccStatusTxt.alignment = TextAnchor.MiddleCenter;
            var ccStatusRect = ccStatusObj.GetComponent<RectTransform>();
            ccStatusRect.anchoredPosition = new Vector2(0, -215);
            ccStatusRect.sizeDelta = new Vector2(280, 40);

            // Bind references in CharacterCreationManager
            SerializedObject serCC = new SerializedObject(ccMgr);
            serCC.FindProperty("characterNameInput").objectReferenceValue = nameField;
            serCC.FindProperty("statusText").objectReferenceValue = ccStatusTxt;
            
            serCC.FindProperty("characterAvatarImage").objectReferenceValue = avatarImg;
            serCC.FindProperty("characterNameLabel").objectReferenceValue = charNameLbl;
            serCC.FindProperty("factionLabel").objectReferenceValue = factionLbl;
            serCC.FindProperty("rarityLabel").objectReferenceValue = rarityLbl;
            serCC.FindProperty("statsLabel").objectReferenceValue = statsLbl;
            serCC.FindProperty("descriptionLabel").objectReferenceValue = descLbl;

            serCC.FindProperty("prevButton").objectReferenceValue = prevBtn;
            serCC.FindProperty("nextButton").objectReferenceValue = nextBtn;
            serCC.FindProperty("createButton").objectReferenceValue = createCharBtn;

            serCC.FindProperty("thanTocSprite").objectReferenceValue = thanPortrait;
            serCC.FindProperty("maTocSprite").objectReferenceValue = maPortrait;
            serCC.FindProperty("yeuTocSprite").objectReferenceValue = yeuPortrait;

            serCC.ApplyModifiedProperties();

            // ==========================================
            // C. CREATE WORLD PANEL (GAME SCREEN)
            // ==========================================
            GameObject worldPanelObj = new GameObject("WorldPanel");
            worldPanelObj.transform.SetParent(canvasObj.transform, false);
            var worldPanelRect = worldPanelObj.AddComponent<RectTransform>();
            worldPanelRect.anchorMin = Vector2.zero;
            worldPanelRect.anchorMax = Vector2.one;
            worldPanelRect.sizeDelta = Vector2.zero;

            // 5. Create UI Panel for HP/MP (Top-Left) (child of WorldPanel)
            GameObject statsPanel = new GameObject("StatsPanel");
            statsPanel.transform.SetParent(worldPanelObj.transform, false);
            var statsRect = statsPanel.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0, 1);
            statsRect.anchorMax = new Vector2(0, 1);
            statsRect.pivot = new Vector2(0, 1);
            statsRect.anchoredPosition = new Vector2(10, -10);
            statsRect.sizeDelta = new Vector2(200, 80);

            // Level Text
            GameObject lvlTextObj = new GameObject("LevelText");
            lvlTextObj.transform.SetParent(statsPanel.transform, false);
            Text lvlText = lvlTextObj.AddComponent<Text>();
            lvlText.text = "Lvl 1";
            lvlText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            lvlText.fontSize = 14;
            lvlText.color = Color.yellow;
            var lvlRect = lvlTextObj.GetComponent<RectTransform>();
            lvlRect.anchoredPosition = new Vector2(0, 20);

            // Name Text
            GameObject nameTextObj = new GameObject("NameText");
            nameTextObj.transform.SetParent(statsPanel.transform, false);
            Text nameText = nameTextObj.AddComponent<Text>();
            nameText.text = "Player";
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 14;
            nameText.color = Color.white;
            var nameRect = nameTextObj.GetComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(60, 20);

            // HP Slider
            GameObject hpSliderObj = new GameObject("HPSlider");
            hpSliderObj.transform.SetParent(statsPanel.transform, false);
            Slider hpSlider = hpSliderObj.AddComponent<Slider>();
            hpSlider.transition = Selectable.Transition.None;
            var hpRect = hpSliderObj.GetComponent<RectTransform>();
            hpRect.sizeDelta = new Vector2(150, 15);
            hpRect.anchoredPosition = new Vector2(0, 0);

            // Create Background for HP
            GameObject hpBg = new GameObject("Background");
            hpBg.transform.SetParent(hpSliderObj.transform, false);
            Image hpBgImg = hpBg.AddComponent<Image>();
            hpBgImg.color = Color.grey;
            var hpBgRect = hpBg.GetComponent<RectTransform>();
            hpBgRect.anchorMin = Vector2.zero;
            hpBgRect.anchorMax = Vector2.one;
            hpBgRect.sizeDelta = Vector2.zero;

            // Create Fill for HP
            GameObject hpFillArea = new GameObject("Fill Area");
            hpFillArea.transform.SetParent(hpSliderObj.transform, false);
            var hpFillAreaRect = hpFillArea.AddComponent<RectTransform>();
            hpFillAreaRect.anchorMin = Vector2.zero;
            hpFillAreaRect.anchorMax = Vector2.one;
            hpFillAreaRect.sizeDelta = Vector2.zero;

            GameObject hpFill = new GameObject("Fill");
            hpFill.transform.SetParent(hpFillArea.transform, false);
            Image hpFillImg = hpFill.AddComponent<Image>();
            hpFillImg.color = Color.red;
            var hpFillRect = hpFill.GetComponent<RectTransform>();
            hpFillRect.anchorMin = Vector2.zero;
            hpFillRect.anchorMax = new Vector2(1, 1);
            hpFillRect.sizeDelta = Vector2.zero;
            hpSlider.fillRect = hpFillRect;

            // MP Slider
            GameObject mpSliderObj = new GameObject("MPSlider");
            mpSliderObj.transform.SetParent(statsPanel.transform, false);
            Slider mpSlider = mpSliderObj.AddComponent<Slider>();
            mpSlider.transition = Selectable.Transition.None;
            var mpRect = mpSliderObj.GetComponent<RectTransform>();
            mpRect.sizeDelta = new Vector2(150, 15);
            mpRect.anchoredPosition = new Vector2(0, -20);

            // Create Background for MP
            GameObject mpBg = new GameObject("Background");
            mpBg.transform.SetParent(mpSliderObj.transform, false);
            Image mpBgImg = mpBg.AddComponent<Image>();
            mpBgImg.color = Color.grey;
            var mpBgRect = mpBg.GetComponent<RectTransform>();
            mpBgRect.anchorMin = Vector2.zero;
            mpBgRect.anchorMax = Vector2.one;
            mpBgRect.sizeDelta = Vector2.zero;

            // Create Fill for MP
            GameObject mpFillArea = new GameObject("Fill Area");
            mpFillArea.transform.SetParent(mpSliderObj.transform, false);
            var mpFillAreaRect = mpFillArea.AddComponent<RectTransform>();
            mpFillAreaRect.anchorMin = Vector2.zero;
            mpFillAreaRect.anchorMax = Vector2.one;
            mpFillAreaRect.sizeDelta = Vector2.zero;

            GameObject mpFill = new GameObject("Fill");
            mpFill.transform.SetParent(mpFillArea.transform, false);
            Image mpFillImg = mpFill.AddComponent<Image>();
            mpFillImg.color = Color.blue;
            var mpFillRect = mpFill.GetComponent<RectTransform>();
            mpFillRect.anchorMin = Vector2.zero;
            mpFillRect.anchorMax = new Vector2(1, 1);
            mpFillRect.sizeDelta = Vector2.zero;
            mpSlider.fillRect = mpFillRect;
            mpSlider.targetGraphic = mpFillImg;

            Debug.Log("Editor: Generated Player HP & MP UI Bars.");

            // 6. Create UI Panel for Chat (Bottom) (child of WorldPanel)
            GameObject chatPanelObj = new GameObject("ChatPanel");
            chatPanelObj.transform.SetParent(worldPanelObj.transform, false);
            var chatRect = chatPanelObj.AddComponent<RectTransform>();
            chatRect.anchorMin = new Vector2(0, 0);
            chatRect.anchorMax = new Vector2(1, 0);
            chatRect.pivot = new Vector2(0.5f, 0);
            chatRect.anchoredPosition = new Vector2(0, 0);
            chatRect.sizeDelta = new Vector2(0, 150);

            // Chat History Text
            GameObject chatHistObj = new GameObject("ChatHistoryText");
            chatHistObj.transform.SetParent(chatPanelObj.transform, false);
            Text chatHist = chatHistObj.AddComponent<Text>();
            chatHist.text = "[Hệ thống]: Chào mừng các đại hiệp tham gia thử nghiệm!";
            chatHist.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            chatHist.fontSize = 14;
            chatHist.color = Color.green;
            chatHist.alignment = TextAnchor.LowerLeft;
            var histRect = chatHistObj.GetComponent<RectTransform>();
            histRect.anchorMin = Vector2.zero;
            histRect.anchorMax = Vector2.one;
            histRect.sizeDelta = new Vector2(-20, -40);
            histRect.anchoredPosition = new Vector2(0, 20);

            // Chat Input Field
            GameObject chatInputObj = CreateInputField("ChatInputField", chatPanelObj.transform, "Nhập tin nhắn chat thế giới...", new Vector2(0, 5));
            chatInputObj.GetComponent<RectTransform>().sizeDelta = new Vector2(-20, 30);
            InputField chatInputField = chatInputObj.GetComponent<InputField>();

            Debug.Log("Editor: Generated Chat box and InputField UI components.");

            // ==========================================
            // D. CREATE COMBAT PANEL
            // ==========================================
            GameObject combatPanelObj = new GameObject("CombatPanel");
            combatPanelObj.transform.SetParent(canvasObj.transform, false);
            var combatRect = combatPanelObj.AddComponent<RectTransform>();
            combatRect.anchorMin = Vector2.zero;
            combatRect.anchorMax = Vector2.one;
            combatRect.sizeDelta = Vector2.zero;
            var combatBg = combatPanelObj.AddComponent<Image>();
            combatBg.color = new Color(0.2f, 0.15f, 0.1f, 1f); // Dark gold-brown theme
            combatPanelObj.SetActive(false); // Default hidden

            // Player Name Text
            GameObject cPlayerNameObj = new GameObject("CombatPlayerName");
            cPlayerNameObj.transform.SetParent(combatPanelObj.transform, false);
            Text cPlayerNameTxt = cPlayerNameObj.AddComponent<Text>();
            cPlayerNameTxt.text = "Người chơi";
            cPlayerNameTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            cPlayerNameTxt.fontSize = 16;
            cPlayerNameTxt.color = Color.white;
            cPlayerNameTxt.alignment = TextAnchor.MiddleCenter;
            var cpNameRect = cPlayerNameObj.GetComponent<RectTransform>();
            cpNameRect.anchoredPosition = new Vector2(-80, 100);
            cpNameRect.sizeDelta = new Vector2(120, 25);

            // Player HP Slider in combat
            GameObject cPlayerHpObj = new GameObject("CombatPlayerHpSlider");
            cPlayerHpObj.transform.SetParent(combatPanelObj.transform, false);
            Slider cPlayerHpSlider = cPlayerHpObj.AddComponent<Slider>();
            cPlayerHpSlider.transition = Selectable.Transition.None;
            var cpHpRect = cPlayerHpObj.GetComponent<RectTransform>();
            cpHpRect.sizeDelta = new Vector2(100, 12);
            cpHpRect.anchoredPosition = new Vector2(-80, 80);

            GameObject cpHpBg = new GameObject("Background");
            cpHpBg.transform.SetParent(cPlayerHpObj.transform, false);
            Image cpHpBgImg = cpHpBg.AddComponent<Image>();
            cpHpBgImg.color = Color.grey;
            var cpHpBgRect = cpHpBg.GetComponent<RectTransform>();
            cpHpBgRect.anchorMin = Vector2.zero;
            cpHpBgRect.anchorMax = Vector2.one;
            cpHpBgRect.sizeDelta = Vector2.zero;

            GameObject cpHpFillArea = new GameObject("Fill Area");
            cpHpFillArea.transform.SetParent(cPlayerHpObj.transform, false);
            var cpHpFillAreaRect = cpHpFillArea.AddComponent<RectTransform>();
            cpHpFillAreaRect.anchorMin = Vector2.zero;
            cpHpFillAreaRect.anchorMax = Vector2.one;
            cpHpFillAreaRect.sizeDelta = Vector2.zero;

            GameObject cpHpFill = new GameObject("Fill");
            cpHpFill.transform.SetParent(cpHpFillArea.transform, false);
            Image cpHpFillImg = cpHpFill.AddComponent<Image>();
            cpHpFillImg.color = Color.red;
            var cpHpFillRect = cpHpFill.GetComponent<RectTransform>();
            cpHpFillRect.anchorMin = Vector2.zero;
            cpHpFillRect.anchorMax = new Vector2(1, 1);
            cpHpFillRect.sizeDelta = Vector2.zero;
            cPlayerHpSlider.fillRect = cpHpFillRect;

            // Enemy Name Text
            GameObject cEnemyNameObj = new GameObject("CombatEnemyName");
            cEnemyNameObj.transform.SetParent(combatPanelObj.transform, false);
            Text cEnemyNameTxt = cEnemyNameObj.AddComponent<Text>();
            cEnemyNameTxt.text = "Quái vật";
            cEnemyNameTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            cEnemyNameTxt.fontSize = 16;
            cEnemyNameTxt.color = Color.yellow;
            cEnemyNameTxt.alignment = TextAnchor.MiddleCenter;
            var ceNameRect = cEnemyNameObj.GetComponent<RectTransform>();
            ceNameRect.anchoredPosition = new Vector2(80, 100);
            ceNameRect.sizeDelta = new Vector2(120, 25);

            // Enemy HP Slider in combat
            GameObject cEnemyHpObj = new GameObject("CombatEnemyHpSlider");
            cEnemyHpObj.transform.SetParent(combatPanelObj.transform, false);
            Slider cEnemyHpSlider = cEnemyHpObj.AddComponent<Slider>();
            cEnemyHpSlider.transition = Selectable.Transition.None;
            var ceHpRect = cEnemyHpObj.GetComponent<RectTransform>();
            ceHpRect.sizeDelta = new Vector2(100, 12);
            ceHpRect.anchoredPosition = new Vector2(80, 80);

            GameObject ceHpBg = new GameObject("Background");
            ceHpBg.transform.SetParent(cEnemyHpObj.transform, false);
            Image ceHpBgImg = ceHpBg.AddComponent<Image>();
            ceHpBgImg.color = Color.grey;
            var ceHpBgRect = ceHpBg.GetComponent<RectTransform>();
            ceHpBgRect.anchorMin = Vector2.zero;
            ceHpBgRect.anchorMax = Vector2.one;
            ceHpBgRect.sizeDelta = Vector2.zero;

            GameObject ceHpFillArea = new GameObject("Fill Area");
            ceHpFillArea.transform.SetParent(cEnemyHpObj.transform, false);
            var ceHpFillAreaRect = ceHpFillArea.AddComponent<RectTransform>();
            ceHpFillAreaRect.anchorMin = Vector2.zero;
            ceHpFillAreaRect.anchorMax = Vector2.one;
            ceHpFillAreaRect.sizeDelta = Vector2.zero;

            GameObject ceHpFill = new GameObject("Fill");
            ceHpFill.transform.SetParent(ceHpFillArea.transform, false);
            Image ceHpFillImg = ceHpFill.AddComponent<Image>();
            ceHpFillImg.color = Color.red;
            var ceHpFillRect = ceHpFill.GetComponent<RectTransform>();
            ceHpFillRect.anchorMin = Vector2.zero;
            ceHpFillRect.anchorMax = new Vector2(1, 1);
            ceHpFillRect.sizeDelta = Vector2.zero;
            cEnemyHpSlider.fillRect = ceHpFillRect;

            // Combat Log Text
            GameObject cLogObj = new GameObject("CombatLogText");
            cLogObj.transform.SetParent(combatPanelObj.transform, false);
            Text cLogTxt = cLogObj.AddComponent<Text>();
            cLogTxt.text = "Bắt đầu trận đấu!";
            cLogTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            cLogTxt.fontSize = 13;
            cLogTxt.color = Color.green;
            cLogTxt.alignment = TextAnchor.UpperLeft;
            var clRect = cLogObj.GetComponent<RectTransform>();
            clRect.anchoredPosition = new Vector2(0, -10);
            clRect.sizeDelta = new Vector2(260, 100);

            // Action Buttons
            GameObject btnAttackObj = CreateUIButton("BtnAttack", combatPanelObj.transform, "TẤN CÔNG", Color.red, new Vector2(-60, -120), new Vector2(100, 35));
            GameObject btnFleeObj = CreateUIButton("BtnFlee", combatPanelObj.transform, "CHẠY TRỐN", Color.gray, new Vector2(60, -120), new Vector2(100, 35));

            Debug.Log("Editor: Generated CombatPanel UI components.");

            // 7. Bind references in UIManager
            SerializedObject serializedUIManager = new SerializedObject(uiManager);
            serializedUIManager.FindProperty("hpSlider").objectReferenceValue = hpSlider;
            serializedUIManager.FindProperty("mpSlider").objectReferenceValue = mpSlider;
            serializedUIManager.FindProperty("levelText").objectReferenceValue = lvlText;
            serializedUIManager.FindProperty("nameText").objectReferenceValue = nameText;
            serializedUIManager.FindProperty("chatHistoryText").objectReferenceValue = chatHist;
            serializedUIManager.FindProperty("chatInputField").objectReferenceValue = chatInputField;
            serializedUIManager.FindProperty("chatPanel").objectReferenceValue = chatPanelObj;

            // Bind new Scene Panels
            serializedUIManager.FindProperty("loginPanel").objectReferenceValue = loginPanelObj;
            serializedUIManager.FindProperty("characterCreationPanel").objectReferenceValue = ccPanelObj;
            serializedUIManager.FindProperty("worldPanel").objectReferenceValue = worldPanelObj;

            // Bind Combat UI properties
            serializedUIManager.FindProperty("combatPanel").objectReferenceValue = combatPanelObj;
            serializedUIManager.FindProperty("combatPlayerNameText").objectReferenceValue = cPlayerNameTxt;
            serializedUIManager.FindProperty("combatEnemyNameText").objectReferenceValue = cEnemyNameTxt;
            serializedUIManager.FindProperty("combatPlayerHpSlider").objectReferenceValue = cPlayerHpSlider;
            serializedUIManager.FindProperty("combatEnemyHpSlider").objectReferenceValue = cEnemyHpSlider;
            serializedUIManager.FindProperty("combatLogText").objectReferenceValue = cLogTxt;

            serializedUIManager.ApplyModifiedProperties();

            // 8. Save the Scene
            string scenePath = "Assets/Scenes/WorldScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            EditorSceneManager.OpenScene(scenePath);
            Debug.Log($"Editor: Saved and opened fully assembled Test Scene successfully at {scenePath}");

            // Show Dialog
            EditorUtility.DisplayDialog("Thành công!", "Hệ thống đã tự động lắp ráp hoàn chỉnh Scene thử nghiệm tại: Assets/Scenes/WorldScene.unity\n\nHãy mở Scene này và nhấn nút Play để kiểm thử luồng đăng nhập và di chuyển!", "OK");
        }

        private static void ConfigureTextureAsSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool modified = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    modified = true;
                }
                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    modified = true;
                }
                
                if (modified)
                {
                    importer.SaveAndReimport();
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                    Debug.Log($"Editor: Converted {path} to Sprite (2D and UI) single mode synchronously.");
                }
            }
            else
            {
                Debug.LogWarning($"Editor: Could not find texture at {path} to configure.");
            }
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
                return sprite;

            // Try loading from sub-assets in case importer hasn't fully propagated main asset lookup
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in subAssets)
            {
                if (asset is Sprite s)
                {
                    return s;
                }
            }
            return null;
        }

        private static void ConfigureAndSliceSpriteSheet(string path, int columns, int rows)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool modified = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    modified = true;
                }
                if (importer.spriteImportMode != SpriteImportMode.Multiple)
                {
                    importer.spriteImportMode = SpriteImportMode.Multiple;
                    modified = true;
                }
                // Ensure Point filtering for pixel art rendering
                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    modified = true;
                }
                // Ensure no compression for pixel art rendering
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    modified = true;
                }

                if (modified)
                {
                    importer.SaveAndReimport();
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                }

                // Slice programmatically
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null)
                {
                    int spriteWidth = texture.width / columns;
                    int spriteHeight = texture.height / rows;
                    
                    var metas = new System.Collections.Generic.List<SpriteMetaData>();
                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < columns; c++)
                        {
                            string direction = "";
                            if (r == 0) direction = "down";
                            else if (r == 1) direction = "left";
                            else if (r == 2) direction = "right";
                            else if (r == 3) direction = "up";

                            SpriteMetaData meta = new SpriteMetaData();
                            meta.name = $"{texture.name}_{direction}_{c}";
                            meta.rect = new Rect(c * spriteWidth, (rows - 1 - r) * spriteHeight, spriteWidth, spriteHeight);
                            meta.alignment = (int)SpriteAlignment.Center;
                            meta.pivot = new Vector2(0.5f, 0.5f);
                            metas.Add(meta);
                        }
                    }
                    
                    importer.spritesheet = metas.ToArray();
                    importer.SaveAndReimport();
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                    Debug.Log($"Editor: Configured and sliced spritesheet at {path} ({columns}x{rows}) synchronously.");
                }
            }
            else
            {
                Debug.LogWarning($"Editor: Could not find spritesheet texture at {path} to configure.");
            }
        }

        // --- Helper Methods to generate UI ---

        private static GameObject CreateInputField(string name, Transform parent, string placeholderText, Vector2 pos)
        {
            GameObject inputObj = new GameObject(name);
            inputObj.transform.SetParent(parent, false);
            InputField inputField = inputObj.AddComponent<InputField>();
            Image inputImg = inputObj.AddComponent<Image>();
            inputImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            var inputRect = inputObj.GetComponent<RectTransform>();
            inputRect.sizeDelta = new Vector2(200, 35);
            inputRect.anchoredPosition = pos;

            GameObject inputPlaceHolderObj = new GameObject("Placeholder");
            inputPlaceHolderObj.transform.SetParent(inputObj.transform, false);
            Text phText = inputPlaceHolderObj.AddComponent<Text>();
            phText.text = placeholderText;
            phText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            phText.fontStyle = FontStyle.Italic;
            phText.fontSize = 12;
            phText.color = Color.gray;
            phText.alignment = TextAnchor.MiddleLeft;
            var phRect = inputPlaceHolderObj.GetComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.sizeDelta = new Vector2(-10, -6);

            GameObject inputTxtObj = new GameObject("Text");
            inputTxtObj.transform.SetParent(inputObj.transform, false);
            Text txt = inputTxtObj.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 14;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleLeft;
            var txtRect = inputTxtObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = new Vector2(-10, -6);

            inputField.placeholder = phText;
            inputField.textComponent = txt;
            return inputObj;
        }

        private static GameObject CreateUIButton(string name, Transform parent, string label, Color color, Vector2 pos, Vector2 size)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            Button btn = btnObj.AddComponent<Button>();
            Image img = btnObj.AddComponent<Image>();
            img.color = color;
            var rect = btnObj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(btnObj.transform, false);
            Text t = textObj.AddComponent<Text>();
            t.text = label;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 14;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            var tRect = textObj.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;

            return btnObj;
        }

        private static GameObject CreateUIToggle(string name, Transform parent, string label, Vector2 pos, ToggleGroup group)
        {
            GameObject toggleObj = new GameObject(name);
            toggleObj.transform.SetParent(parent, false);
            Toggle toggle = toggleObj.AddComponent<Toggle>();
            toggle.group = group;
            var rect = toggleObj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(70, 30);

            // Simple Toggle graphics (Background & Checkmark)
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(toggleObj.transform, false);
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = Color.gray;
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchoredPosition = new Vector2(-25, 0);
            bgRect.sizeDelta = new Vector2(16, 16);

            GameObject checkmarkObj = new GameObject("Checkmark");
            checkmarkObj.transform.SetParent(bgObj.transform, false);
            Image checkImg = checkmarkObj.AddComponent<Image>();
            checkImg.color = Color.green;
            var checkRect = checkmarkObj.GetComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.sizeDelta = Vector2.zero;

            toggle.targetGraphic = bgImg;
            toggle.graphic = checkImg;

            // Toggle Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(toggleObj.transform, false);
            Text t = labelObj.AddComponent<Text>();
            t.text = label;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 12;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleLeft;
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchoredPosition = new Vector2(15, 0);
            labelRect.sizeDelta = new Vector2(60, 20);

            return toggleObj;
        }

        private static Sprite[] LoadAllSheetSprites(string path)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var spritesList = new System.Collections.Generic.List<Sprite>();
            foreach (var a in assets)
            {
                if (a is Sprite s)
                {
                    spritesList.Add(s);
                }
            }

            Sprite[] sorted = new Sprite[16];
            foreach (var s in spritesList)
            {
                string name = s.name;
                if (name.Contains("_up_0")) sorted[0] = s;
                else if (name.Contains("_up_1")) sorted[1] = s;
                else if (name.Contains("_up_2")) sorted[2] = s;
                else if (name.Contains("_up_3")) sorted[3] = s;
                else if (name.Contains("_right_0")) sorted[4] = s;
                else if (name.Contains("_right_1")) sorted[5] = s;
                else if (name.Contains("_right_2")) sorted[6] = s;
                else if (name.Contains("_right_3")) sorted[7] = s;
                else if (name.Contains("_left_0")) sorted[8] = s;
                else if (name.Contains("_left_1")) sorted[9] = s;
                else if (name.Contains("_left_2")) sorted[10] = s;
                else if (name.Contains("_left_3")) sorted[11] = s;
                else if (name.Contains("_down_0")) sorted[12] = s;
                else if (name.Contains("_down_1")) sorted[13] = s;
                else if (name.Contains("_down_2")) sorted[14] = s;
                else if (name.Contains("_down_3")) sorted[15] = s;
            }
            return sorted;
        }

        private static void AssignSpriteArray(SerializedProperty prop, Sprite[] sprites)
        {
            prop.ClearArray();
            if (sprites == null) return;
            prop.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
        }

        [MenuItem("Tools/Tây Du Ký/Kiểm Thử Cổng Dịch Chuyển (Portal Proximity Test)")]
        public static void RunPortalTest()
        {
            // Open the scene
            EditorSceneManager.OpenScene("Assets/Scenes/WorldScene.unity");

            var mapMgr = FindFirstObjectByType<MapManager>();
            var player = FindFirstObjectByType<PlayerController>();

            if (mapMgr == null || player == null)
            {
                Debug.LogError("Portal Test: Không tìm thấy MapManager hoặc PlayerController trong Scene! Hãy lắp ráp scene trước.");
                return;
            }

            Debug.Log("=== BẮT ĐẦU KIỂM THỬ KHOẢNG CÁCH CỔNG DỊCH CHUYỂN ===");

            // Map 101 portals
            // Portal is at (12, 23) with trigger_distance = 1.0
            Vector3 portalPos = new Vector3(12f, 23f, 0f);
            float triggerDist = 1.0f;

            // Case 1: Player stands directly on the portal (12, 23)
            Vector3 testPos1 = new Vector3(12f, 23f, 0f);
            float dist1 = Mathf.Max(Mathf.Abs(testPos1.x - portalPos.x), Mathf.Abs(testPos1.y - portalPos.y));
            bool check1 = dist1 <= triggerDist;
            Debug.Log($"Test Case 1 (Đứng đè lên portal (12, 23)): Khoảng cách Chebyshev = {dist1}, Kích hoạt = {check1} (Kỳ vọng: TRUE)");

            // Case 2: Player stands adjacent to the portal at (12, 22)
            Vector3 testPos2 = new Vector3(12f, 22f, 0f);
            float dist2 = Mathf.Max(Mathf.Abs(testPos2.x - portalPos.x), Mathf.Abs(testPos2.y - portalPos.y));
            bool check2 = dist2 <= triggerDist;
            Debug.Log($"Test Case 2 (Đứng kế dưới portal (12, 22)): Khoảng cách Chebyshev = {dist2}, Kích hoạt = {check2} (Kỳ vọng: TRUE)");

            // Case 3: Player stands diagonally adjacent to the portal at (13, 22)
            Vector3 testPos3 = new Vector3(13f, 22f, 0f);
            float dist3 = Mathf.Max(Mathf.Abs(testPos3.x - portalPos.x), Mathf.Abs(testPos3.y - portalPos.y));
            bool check3 = dist3 <= triggerDist;
            Debug.Log($"Test Case 3 (Đứng chéo portal (13, 22)): Khoảng cách Chebyshev = {dist3}, Kích hoạt = {check3} (Kỳ vọng: TRUE)");

            // Case 4: Player stands away from the portal at (12, 21)
            Vector3 testPos4 = new Vector3(12f, 21f, 0f);
            float dist4 = Mathf.Max(Mathf.Abs(testPos4.x - portalPos.x), Mathf.Abs(testPos4.y - portalPos.y));
            bool check4 = dist4 <= triggerDist;
            Debug.Log($"Test Case 4 (Đứng cách xa portal (12, 21)): Khoảng cách Chebyshev = {dist4}, Kích hoạt = {check4} (Kỳ vọng: FALSE)");

            if (check1 && check2 && check3 && !check4)
            {
                EditorUtility.DisplayDialog("Thành công!", "Hệ thống kiểm thử khoảng cách Chebyshev hoạt động hoàn hảo! Đạt 4/4 test case.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Lỗi!", "Có lỗi trong logic tính toán khoảng cách Chebyshev!", "OK");
            }
        }
    }
}
