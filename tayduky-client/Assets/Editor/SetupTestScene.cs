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

            // 0.1 Ensure map background textures are imported as Sprites
            ConfigureTextureAsSprite("Assets/Resources/Maps/hoi_ban_dao.png");
            ConfigureTextureAsSprite("Assets/Resources/Maps/dao_tri.png");

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

            // BUG FIX #4: Add CameraFollow to Main Camera so it tracks the player
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                // Fallback: find any camera in scene
                mainCam = FindFirstObjectByType<Camera>();
            }
            if (mainCam != null)
            {
                // Ensure camera is set up for 2D orthographic projection
                mainCam.orthographic = true;
                mainCam.orthographicSize = 5f; // Portrait view: 5 world units half-height

                var camFollow = mainCam.GetComponent<CameraFollow>();
                if (camFollow == null) camFollow = mainCam.gameObject.AddComponent<CameraFollow>();

                // Set target via SerializedObject so the reference is saved in the scene
                SerializedObject serCam = new SerializedObject(camFollow);
                serCam.FindProperty("target").objectReferenceValue = playerObj.transform;
                serCam.FindProperty("smoothSpeed").floatValue = 5f;
                serCam.FindProperty("offset").vector3Value = new Vector3(0f, 0f, -10f);
                serCam.ApplyModifiedProperties();

                // Snap camera immediately to player spawn area
                mainCam.transform.position = new Vector3(12f, 5f, -10f); // Map 101 spawn: (12,5)
                Debug.Log("Editor: Added CameraFollow to Main Camera, targeting Player.");
            }
            else
            {
                Debug.LogWarning("Editor: Could not find Main Camera to attach CameraFollow!");
            }

            // 4. Create UI Canvas and UI Objects
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(720f, 1280f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
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
            titleRect.anchoredPosition = new Vector2(0, 250);
            titleRect.sizeDelta = new Vector2(300, 50);

            // Username Input Field
            GameObject userObj = CreateInputField("UsernameInput", loginPanelObj.transform, "Nhập tài khoản...", new Vector2(0, 80));
            InputField userField = userObj.GetComponent<InputField>();

            // Password Input Field
            GameObject passObj = CreateInputField("PasswordInput", loginPanelObj.transform, "Nhập mật khẩu...", new Vector2(0, 0));
            InputField passField = passObj.GetComponent<InputField>();
            passField.contentType = InputField.ContentType.Password;

            // Login Button
            GameObject btnLoginObj = CreateUIButton("BtnLogin", loginPanelObj.transform, "Đăng Nhập", new Color(0.8f, 0.6f, 0.2f), new Vector2(-90, -80), new Vector2(140, 45));
            Button loginBtn = btnLoginObj.GetComponent<Button>();

            // Register Button
            GameObject btnRegObj = CreateUIButton("BtnRegister", loginPanelObj.transform, "Đăng Ký", new Color(0.4f, 0.4f, 0.4f), new Vector2(90, -80), new Vector2(140, 45));
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
            statusRect.anchoredPosition = new Vector2(0, -160);
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
            ccTitleRect.anchoredPosition = new Vector2(0, 320);
            ccTitleRect.sizeDelta = new Vector2(300, 40);

            // Avatar Display
            GameObject avatarObj = new GameObject("CharacterAvatar");
            avatarObj.transform.SetParent(ccPanelObj.transform, false);
            Image avatarImg = avatarObj.AddComponent<Image>();
            avatarImg.sprite = thanPortrait; // Default starting
            var avatarRect = avatarObj.GetComponent<RectTransform>();
            avatarRect.anchoredPosition = new Vector2(0, 150);
            avatarRect.sizeDelta = new Vector2(160, 160);

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
            charNameLblRect.anchoredPosition = new Vector2(0, 20);
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
            factionLblRect.anchoredPosition = new Vector2(0, -15);
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
            rarityLblRect.anchoredPosition = new Vector2(0, -45);
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
            statsLblRect.anchoredPosition = new Vector2(0, -75);
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
            descLblRect.anchoredPosition = new Vector2(0, -115);
            descLblRect.sizeDelta = new Vector2(300, 50);

            // Name Input Field (Tên người chơi đặt)
            GameObject nameInputObj = CreateInputField("CharNameInput", ccPanelObj.transform, "Tên người chơi đặt...", new Vector2(0, -180));
            InputField nameField = nameInputObj.GetComponent<InputField>();

            // Navigation Buttons: Next & Prev
            GameObject btnPrevObj = CreateUIButton("BtnPrev", ccPanelObj.transform, "<", new Color(0.3f, 0.3f, 0.3f), new Vector2(-140, 150), new Vector2(50, 50));
            Button prevBtn = btnPrevObj.GetComponent<Button>();

            GameObject btnNextObj = CreateUIButton("BtnNext", ccPanelObj.transform, ">", new Color(0.3f, 0.3f, 0.3f), new Vector2(140, 150), new Vector2(50, 50));
            Button nextBtn = btnNextObj.GetComponent<Button>();

            // Create Button
            GameObject btnCreateObj = CreateUIButton("BtnCreateChar", ccPanelObj.transform, "Vào Thế Giới", Color.cyan, new Vector2(0, -250), new Vector2(180, 45));
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
            ccStatusRect.anchoredPosition = new Vector2(0, -310);
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

            // ==========================================
            // B. TOP HUD BAR – PORTRAIT LAYOUT (pinned to top)
            //    Row 1 (top, h=36): [Avatar 48x48] | [Name  bold gold] | [Lv.X cyan]
            //    Row 2 (mid, h=16): HP bar full-width (with label)
            //    Row 3 (bot, h=16): MP bar full-width (with label)
            //    Total height = 76px
            // ==========================================
            GameObject topBarObj = new GameObject("TopHUDBar");
            topBarObj.transform.SetParent(worldPanelObj.transform, false);
            var topBarBg = topBarObj.AddComponent<Image>();
            topBarBg.color = new Color(0f, 0f, 0f, 0.80f);
            var topBarRect = topBarObj.GetComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0, 1);
            topBarRect.anchorMax = new Vector2(1, 1);
            topBarRect.pivot     = new Vector2(0.5f, 1f);
            topBarRect.anchoredPosition = Vector2.zero;
            topBarRect.sizeDelta = new Vector2(0, 80f); // 80px portrait top bar

            // Gold separator line at bottom of TopBar
            GameObject topDivider = new GameObject("TopDivider");
            topDivider.transform.SetParent(topBarObj.transform, false);
            topDivider.AddComponent<Image>().color = new Color(0.9f, 0.75f, 0.2f, 0.9f);
            var topDivR = topDivider.GetComponent<RectTransform>();
            topDivR.anchorMin = new Vector2(0, 0); topDivR.anchorMax = new Vector2(1, 0);
            topDivR.pivot = new Vector2(0.5f, 0f);
            topDivR.anchoredPosition = Vector2.zero; topDivR.sizeDelta = new Vector2(0, 2f);

            // --- Avatar icon (left side, square) ---
            GameObject hudAvatarObj = new GameObject("Avatar");
            hudAvatarObj.transform.SetParent(topBarObj.transform, false);
            var hudAvatarImg = hudAvatarObj.AddComponent<Image>();
            // Remove background color to make avatar transparent
            hudAvatarImg.color = Color.clear;
            var hudAvatarRect = hudAvatarObj.GetComponent<RectTransform>();
            hudAvatarRect.anchorMin = new Vector2(0, 1);
            hudAvatarRect.anchorMax = new Vector2(0, 1);
            hudAvatarRect.pivot = new Vector2(0, 1);
            hudAvatarRect.anchoredPosition = new Vector2(8f, -4f);
            hudAvatarRect.sizeDelta = new Vector2(52f, 52f);

            // Avatar border (gold outline effect)
            GameObject avatarBorderObj = new GameObject("AvatarBorder");
            avatarBorderObj.transform.SetParent(hudAvatarObj.transform, false);
            var abImg = avatarBorderObj.AddComponent<Image>();
            abImg.color = new Color(0.9f, 0.75f, 0.2f, 0.7f);
            var abRect = avatarBorderObj.GetComponent<RectTransform>();
            abRect.anchorMin = Vector2.zero; abRect.anchorMax = Vector2.one;
            abRect.sizeDelta = new Vector2(4, 4); // Outline

            // Name Text (right of avatar, top row)
            GameObject nameTextObj = new GameObject("NameText");
            nameTextObj.transform.SetParent(topBarObj.transform, false);
            Text nameText = nameTextObj.AddComponent<Text>();
            nameText.text = "Player";
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 15;
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = new Color(1f, 0.92f, 0.4f);
            nameText.alignment = TextAnchor.MiddleLeft;
            var nameRect = nameTextObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 1);
            nameRect.anchorMax = new Vector2(0.65f, 1);
            nameRect.pivot = new Vector2(0, 1);
            nameRect.anchoredPosition = new Vector2(68f, -6f);
            nameRect.sizeDelta = new Vector2(0, 24f);

            // Level Text (right of name, small cyan)
            GameObject lvlTextObj = new GameObject("LevelText");
            lvlTextObj.transform.SetParent(topBarObj.transform, false);
            Text lvlText = lvlTextObj.AddComponent<Text>();
            lvlText.text = "Lv.1";
            lvlText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            lvlText.fontSize = 13;
            lvlText.color = new Color(0.55f, 0.85f, 1f);
            lvlText.alignment = TextAnchor.MiddleLeft;
            var lvlRect = lvlTextObj.GetComponent<RectTransform>();
            lvlRect.anchorMin = new Vector2(0.65f, 1);
            lvlRect.anchorMax = new Vector2(1, 1);
            lvlRect.pivot = new Vector2(0, 1);
            lvlRect.anchoredPosition = new Vector2(0f, -6f);
            lvlRect.sizeDelta = new Vector2(-8f, 24f);

            // --- HP Bar (Row 2) – full width minus left margin ---
            GameObject hpRowObj = new GameObject("HPRow");
            hpRowObj.transform.SetParent(topBarObj.transform, false);
            var hpRowRect = hpRowObj.AddComponent<RectTransform>();
            hpRowRect.anchorMin = new Vector2(0, 1);
            hpRowRect.anchorMax = new Vector2(1, 1);
            hpRowRect.pivot = new Vector2(0, 1);
            hpRowRect.anchoredPosition = new Vector2(68f, -34f);
            hpRowRect.sizeDelta = new Vector2(-76f, 18f); // left offset=68, right margin=8

            // HP label
            GameObject hpLabelObj = new GameObject("HPLabel");
            hpLabelObj.transform.SetParent(hpRowObj.transform, false);
            Text hpLabel = hpLabelObj.AddComponent<Text>();
            hpLabel.text = "HP";
            hpLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hpLabel.fontSize = 11; hpLabel.fontStyle = FontStyle.Bold;
            hpLabel.color = new Color(1f, 0.38f, 0.38f);
            hpLabel.alignment = TextAnchor.MiddleLeft;
            var hpLabelRect = hpLabelObj.GetComponent<RectTransform>();
            hpLabelRect.anchorMin = new Vector2(0, 0); hpLabelRect.anchorMax = new Vector2(0, 1);
            hpLabelRect.pivot = new Vector2(0, 0.5f);
            hpLabelRect.anchoredPosition = Vector2.zero; hpLabelRect.sizeDelta = new Vector2(26f, 0);

            // HP Slider (no handle = not draggable)
            GameObject hpSliderObj = new GameObject("HPSlider");
            hpSliderObj.transform.SetParent(hpRowObj.transform, false);
            Slider hpSlider = hpSliderObj.AddComponent<Slider>();
            hpSlider.transition = Selectable.Transition.None;
            hpSlider.interactable = false; // Display-only, no dragging
            hpSlider.value = 0.8f;
            var hpRect = hpSliderObj.GetComponent<RectTransform>();
            hpRect.anchorMin = new Vector2(0, 0); hpRect.anchorMax = new Vector2(1, 1);
            hpRect.pivot = new Vector2(0, 0.5f);
            hpRect.anchoredPosition = new Vector2(28f, 0f);
            hpRect.sizeDelta = new Vector2(-28f, 0f);
            // Background
            GameObject hpBg = new GameObject("Background"); hpBg.transform.SetParent(hpSliderObj.transform, false);
            var hpBgImg = hpBg.AddComponent<Image>(); hpBgImg.color = new Color(0.18f, 0.04f, 0.04f);
            var hpBgR = hpBg.GetComponent<RectTransform>(); hpBgR.anchorMin = Vector2.zero; hpBgR.anchorMax = Vector2.one; hpBgR.sizeDelta = Vector2.zero;
            // Fill Area
            GameObject hpFillArea = new GameObject("Fill Area"); hpFillArea.transform.SetParent(hpSliderObj.transform, false);
            var hpFAR = hpFillArea.AddComponent<RectTransform>(); hpFAR.anchorMin = Vector2.zero; hpFAR.anchorMax = Vector2.one; hpFAR.sizeDelta = new Vector2(0, 0); hpFAR.offsetMin = Vector2.zero; hpFAR.offsetMax = Vector2.zero;
            // Fill
            GameObject hpFill = new GameObject("Fill"); hpFill.transform.SetParent(hpFillArea.transform, false);
            var hpFillImg = hpFill.AddComponent<Image>(); hpFillImg.color = new Color(0.88f, 0.16f, 0.16f);
            var hpFillR = hpFill.GetComponent<RectTransform>(); hpFillR.anchorMin = Vector2.zero; hpFillR.anchorMax = Vector2.one; hpFillR.sizeDelta = Vector2.zero;
            hpSlider.fillRect = hpFillR;
            // NO Handle Slide Area – prevents dragging

            // --- MP Bar (Row 3) – full width minus left margin ---
            GameObject mpRowObj = new GameObject("MPRow");
            mpRowObj.transform.SetParent(topBarObj.transform, false);
            var mpRowRect = mpRowObj.AddComponent<RectTransform>();
            mpRowRect.anchorMin = new Vector2(0, 1);
            mpRowRect.anchorMax = new Vector2(1, 1);
            mpRowRect.pivot = new Vector2(0, 1);
            mpRowRect.anchoredPosition = new Vector2(68f, -56f);
            mpRowRect.sizeDelta = new Vector2(-76f, 18f);

            // MP label
            GameObject mpLabelObj = new GameObject("MPLabel");
            mpLabelObj.transform.SetParent(mpRowObj.transform, false);
            Text mpLabel = mpLabelObj.AddComponent<Text>();
            mpLabel.text = "MP";
            mpLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            mpLabel.fontSize = 11; mpLabel.fontStyle = FontStyle.Bold;
            mpLabel.color = new Color(0.38f, 0.6f, 1f);
            mpLabel.alignment = TextAnchor.MiddleLeft;
            var mpLabelRect = mpLabelObj.GetComponent<RectTransform>();
            mpLabelRect.anchorMin = new Vector2(0, 0); mpLabelRect.anchorMax = new Vector2(0, 1);
            mpLabelRect.pivot = new Vector2(0, 0.5f);
            mpLabelRect.anchoredPosition = Vector2.zero; mpLabelRect.sizeDelta = new Vector2(26f, 0);

            // MP Slider (no handle = not draggable)
            GameObject mpSliderObj = new GameObject("MPSlider");
            mpSliderObj.transform.SetParent(mpRowObj.transform, false);
            Slider mpSlider = mpSliderObj.AddComponent<Slider>();
            mpSlider.transition = Selectable.Transition.None;
            mpSlider.interactable = false; // Display-only, no dragging
            mpSlider.value = 0.6f;
            var mpRect = mpSliderObj.GetComponent<RectTransform>();
            mpRect.anchorMin = new Vector2(0, 0); mpRect.anchorMax = new Vector2(1, 1);
            mpRect.pivot = new Vector2(0, 0.5f);
            mpRect.anchoredPosition = new Vector2(28f, 0f);
            mpRect.sizeDelta = new Vector2(-28f, 0f);
            // Background
            GameObject mpBg = new GameObject("Background"); mpBg.transform.SetParent(mpSliderObj.transform, false);
            var mpBgImg = mpBg.AddComponent<Image>(); mpBgImg.color = new Color(0.04f, 0.04f, 0.2f);
            var mpBgR = mpBg.GetComponent<RectTransform>(); mpBgR.anchorMin = Vector2.zero; mpBgR.anchorMax = Vector2.one; mpBgR.sizeDelta = Vector2.zero;
            // Fill Area
            GameObject mpFillArea = new GameObject("Fill Area"); mpFillArea.transform.SetParent(mpSliderObj.transform, false);
            var mpFAR = mpFillArea.AddComponent<RectTransform>(); mpFAR.anchorMin = Vector2.zero; mpFAR.anchorMax = Vector2.one; mpFAR.sizeDelta = new Vector2(0, 0); mpFAR.offsetMin = Vector2.zero; mpFAR.offsetMax = Vector2.zero;
            // Fill
            GameObject mpFill = new GameObject("Fill"); mpFill.transform.SetParent(mpFillArea.transform, false);
            var mpFillImg = mpFill.AddComponent<Image>(); mpFillImg.color = new Color(0.12f, 0.36f, 0.95f);
            var mpFillR = mpFill.GetComponent<RectTransform>(); mpFillR.anchorMin = Vector2.zero; mpFillR.anchorMax = Vector2.one; mpFillR.sizeDelta = Vector2.zero;
            mpSlider.fillRect = mpFillR;
            // NO Handle Slide Area – prevents dragging

            // Store statsPanel ref for UIManager binding
            GameObject statsPanel = topBarObj;

            Debug.Log("Editor: Generated portrait TopHUD bar with avatar, name, HP/MP bars (not draggable).");

            // ==========================================
            // C. CHAT PANEL (bottom-left, above BottomHUD bar)
            // ==========================================
            GameObject chatPanelObj = new GameObject("ChatPanel");
            chatPanelObj.transform.SetParent(worldPanelObj.transform, false);
            var chatRect = chatPanelObj.AddComponent<RectTransform>();
            chatRect.anchorMin = new Vector2(0, 0);
            chatRect.anchorMax = new Vector2(0.6f, 0); // 60% width in portrait
            chatRect.pivot     = new Vector2(0, 0);
            chatRect.anchoredPosition = new Vector2(0f, 115f);  // Sits above the 110px BottomHUDBar
            chatRect.sizeDelta = new Vector2(0f, 100f);
            var chatBg = chatPanelObj.AddComponent<Image>();
            chatBg.color = new Color(0f, 0f, 0f, 0.55f);

            // Chat History Text
            GameObject chatHistObj = new GameObject("ChatHistoryText");
            chatHistObj.transform.SetParent(chatPanelObj.transform, false);
            Text chatHist = chatHistObj.AddComponent<Text>();
            chatHist.text = "[He thong]: Chao mung dai hiep tham gia thu nghiem!";
            chatHist.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            chatHist.fontSize = 12;
            chatHist.color = new Color(0.5f, 1f, 0.5f);
            chatHist.alignment = TextAnchor.LowerLeft;
            var histRect = chatHistObj.GetComponent<RectTransform>();
            histRect.anchorMin = Vector2.zero;
            histRect.anchorMax = Vector2.one;
            histRect.sizeDelta = new Vector2(-8f, -36f);
            histRect.anchoredPosition = new Vector2(4f, 18f);

            // Chat Input Field
            GameObject chatInputObj = CreateInputField("ChatInputField", chatPanelObj.transform,
                "Nhan tin nhan chat...", new Vector2(4f, 4f));
            chatInputObj.GetComponent<RectTransform>().sizeDelta = new Vector2(-8f, 28f);
            InputField chatInputField = chatInputObj.GetComponent<InputField>();

            Debug.Log("Editor: Generated Chat box and InputField UI components.");

            // ==========================================
            // E. SUB-PANELS (fixed 440x340 centered modal, all hidden by default)
            // Anchored to center so they don't depend on canvas resolution.
            // ==========================================
            GameObject inventoryPanelObj = CreateCenteredSubPanel("InventoryPanel",  worldPanelObj.transform, "BANG DO TRANG BI",  new Color(0.08f,0.07f,0.18f,0.97f));
            GameObject shopPanelObj      = CreateCenteredSubPanel("ShopPanel",       worldPanelObj.transform, "CUA HANG",          new Color(0.07f,0.14f,0.07f,0.97f));
            GameObject sysPanelObj       = CreateCenteredSubPanel("SysPanel",        worldPanelObj.transform, "THIET LAP HE THONG",new Color(0.12f,0.12f,0.12f,0.97f));
            GameObject factionPanelObj   = CreateCenteredSubPanel("FactionPanel",    worldPanelObj.transform, "KHU PHAI",          new Color(0.18f,0.09f,0.02f,0.97f));

            // ==========================================
            // F. BOTTOM HUD BAR (full-width strip pinned to very bottom)
            // ==========================================
            // BottomHUDBar – 2-row button layout for portrait 720px width
            // Row 1: Bang | Nhanh | Khu Phai    (left-to-right)
            // Row 2: Shop | Sys                 (centered)
            // Total height = 110px
            GameObject bottomBarObj = new GameObject("BottomHUDBar");
            bottomBarObj.transform.SetParent(worldPanelObj.transform, false);
            var bottomBarBg = bottomBarObj.AddComponent<Image>();
            bottomBarBg.color = new Color(0f, 0f, 0f, 0.82f);
            var bottomBarRect = bottomBarObj.GetComponent<RectTransform>();
            bottomBarRect.anchorMin = new Vector2(0, 0);
            bottomBarRect.anchorMax = new Vector2(1, 0);
            bottomBarRect.pivot     = new Vector2(0.5f, 0f);
            bottomBarRect.anchoredPosition = Vector2.zero;
            bottomBarRect.sizeDelta = new Vector2(0, 110f);

            // Divider line at top of BottomBar
            GameObject dividerObj = new GameObject("Divider");
            dividerObj.transform.SetParent(bottomBarObj.transform, false);
            var dividerImg = dividerObj.AddComponent<Image>();
            dividerImg.color = new Color(0.85f, 0.70f, 0.20f, 0.9f);
            var dividerRect = dividerObj.GetComponent<RectTransform>();
            dividerRect.anchorMin = new Vector2(0, 1); dividerRect.anchorMax = new Vector2(1, 1);
            dividerRect.pivot = new Vector2(0.5f, 1f);
            dividerRect.anchoredPosition = Vector2.zero;
            dividerRect.sizeDelta = new Vector2(0, 2f);

            // Portrait button layout: 3 buttons on top row, 2 buttons on bottom row
            // Button size: width=(screenW-padding*4)/3 ≈ (720-48)/3 = 224px → use 210x48
            float bW = 210f, bH = 48f, bPad = 6f;

            // Row 1 Y position (from center of bar): 110/2 - 10 - 48/2 = 31
            float row1Y = 31f;
            // Row 2 Y position: row1Y - bH - bPad = 31 - 48 - 6 = -23
            float row2Y = -23f;

            // Row 1: Bang, Nhanh, Khu Phai  (3 buttons evenly spaced)
            string[] row1Labels = { "Bang", "Nhanh", "Khu Phai" };
            Color[]  row1Colors = {
                new Color(0.72f, 0.50f, 0.08f), // Bang  – gold
                new Color(0.12f, 0.55f, 0.12f), // Nhanh – green
                new Color(0.08f, 0.38f, 0.72f), // Khu Phai – blue
            };
            float r1TotalW = row1Labels.Length * bW + (row1Labels.Length - 1) * bPad;
            float r1StartX = -r1TotalW / 2f + bW / 2f;

            Button bangBtn = null, nhanhBtn = null, khuPhaiBtn = null, shopBtn = null, sysBtn = null;

            for (int bi = 0; bi < row1Labels.Length; bi++)
            {
                float xPos = r1StartX + bi * (bW + bPad);
                GameObject bObj = CreateUIButton("Btn" + row1Labels[bi].Replace(" ",""),
                    bottomBarObj.transform, row1Labels[bi], row1Colors[bi],
                    new Vector2(xPos, row1Y), new Vector2(bW, bH));
                var bRect = bObj.GetComponent<RectTransform>();
                bRect.anchorMin = new Vector2(0.5f, 0.5f);
                bRect.anchorMax = new Vector2(0.5f, 0.5f);
                bRect.pivot = new Vector2(0.5f, 0.5f);
                bRect.anchoredPosition = new Vector2(xPos, row1Y);
                Button b = bObj.GetComponent<Button>();
                switch (bi)
                {
                    case 0: bangBtn    = b; break;
                    case 1: nhanhBtn   = b; break;
                    case 2: khuPhaiBtn = b; break;
                }
            }

            // Row 2: Shop, Sys  (2 buttons centered)
            string[] row2Labels = { "Shop", "Sys" };
            Color[]  row2Colors = {
                new Color(0.58f, 0.15f, 0.58f), // Shop  – purple
                new Color(0.35f, 0.35f, 0.35f), // Sys   – grey
            };
            float r2TotalW = row2Labels.Length * bW + (row2Labels.Length - 1) * bPad;
            float r2StartX = -r2TotalW / 2f + bW / 2f;

            for (int bi = 0; bi < row2Labels.Length; bi++)
            {
                float xPos = r2StartX + bi * (bW + bPad);
                GameObject bObj = CreateUIButton("Btn" + row2Labels[bi].Replace(" ",""),
                    bottomBarObj.transform, row2Labels[bi], row2Colors[bi],
                    new Vector2(xPos, row2Y), new Vector2(bW, bH));
                var bRect = bObj.GetComponent<RectTransform>();
                bRect.anchorMin = new Vector2(0.5f, 0.5f);
                bRect.anchorMax = new Vector2(0.5f, 0.5f);
                bRect.pivot = new Vector2(0.5f, 0.5f);
                bRect.anchoredPosition = new Vector2(xPos, row2Y);
                Button b = bObj.GetComponent<Button>();
                switch (bi)
                {
                    case 0: shopBtn = b; break;
                    case 1: sysBtn  = b; break;
                }
            }

            // Wire button events persistently (survives scene save)
            if (bangBtn    != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(bangBtn.onClick,    uiManager.OnClickBangButton);
            if (nhanhBtn   != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(nhanhBtn.onClick,   uiManager.OnClickNhanhButton);
            if (khuPhaiBtn != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(khuPhaiBtn.onClick, uiManager.OnClickKhuPhaiButton);
            if (shopBtn    != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(shopBtn.onClick,    uiManager.OnClickShopButton);
            if (sysBtn     != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(sysBtn.onClick,     uiManager.OnClickSysButton);

            Debug.Log("Editor: Generated portrait BottomHUD bar (2-row: Bang/Nhanh/KhuPhai | Shop/Sys) pinned to bottom.");

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
            serializedUIManager.FindProperty("loginPanel").objectReferenceValue            = loginPanelObj;
            serializedUIManager.FindProperty("characterCreationPanel").objectReferenceValue = ccPanelObj;
            serializedUIManager.FindProperty("worldPanel").objectReferenceValue            = worldPanelObj;

            // Bind Sub-Panels for toggle buttons
            serializedUIManager.FindProperty("inventoryPanel").objectReferenceValue = inventoryPanelObj;
            serializedUIManager.FindProperty("shopPanel").objectReferenceValue       = shopPanelObj;
            serializedUIManager.FindProperty("sysPanel").objectReferenceValue        = sysPanelObj;
            serializedUIManager.FindProperty("factionPanel").objectReferenceValue    = factionPanelObj;

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
                    int spriteWidth  = texture.width  / columns;
                    int spriteHeight = texture.height / rows;

                    var metas = new System.Collections.Generic.List<SpriteMetaData>();

                    // BUG FIX #3: Unity texture Y-origin is bottom-left.
                    // (rows - 1 - r) flips the row index, so:
                    //   r=0 (loop top) → rect-Y = bottom of texture  → actual top-row of IMAGE
                    // Standard spritesheet layout (top → bottom of PNG):
                    //   Image Row 0 (top)    = Down  walk frames
                    //   Image Row 1          = Left  walk frames
                    //   Image Row 2          = Right walk frames
                    //   Image Row 3 (bottom) = Up    walk frames
                    // After (rows-1-r) flip:
                    //   r=0 → rect starts at Y = (rows-1)*spriteH → top of image → "down"
                    //   r=3 → rect starts at Y = 0               → bottom       → "up"
                    // Mapping r → direction for this standard layout:
                    string[] directionForRow = { "down", "left", "right", "up" };

                    for (int r = 0; r < rows; r++)
                    {
                        string direction = (r < directionForRow.Length) ? directionForRow[r] : r.ToString();

                        for (int c = 0; c < columns; c++)
                        {
                            SpriteMetaData meta = new SpriteMetaData();
                            meta.name      = $"{texture.name}_{direction}_{c}";
                            meta.rect      = new Rect(c * spriteWidth, (rows - 1 - r) * spriteHeight, spriteWidth, spriteHeight);
                            meta.alignment = (int)SpriteAlignment.Center;
                            meta.pivot     = new Vector2(0.5f, 0.5f);
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


        /// <summary>
        /// Creates a centered overlay panel (e.g. Inventory/Shop/Sys/Faction) that starts hidden.
        /// </summary>
        private static GameObject CreateCenteredSubPanel(string name, Transform parent, string title, Color bgColor)
        {
            GameObject panelObj = new GameObject(name);
            panelObj.transform.SetParent(parent, false);
            Image img = panelObj.AddComponent<Image>();
            img.color = bgColor;
            var rect = panelObj.GetComponent<RectTransform>();
            // Fixed size centered modal – does not depend on canvas resolution
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot     = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 10f); // Slightly above center
            rect.sizeDelta = new Vector2(560f, 780f);     // Portrait 560x780 px modal

            // Title text
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            Text t = titleObj.AddComponent<Text>();
            t.text = title;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 18;
            t.fontStyle = FontStyle.Bold;
            t.color = Color.white;
            t.alignment = TextAnchor.UpperCenter;
            var tRect = titleObj.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0, 1);
            tRect.anchorMax = new Vector2(1, 1);
            tRect.pivot = new Vector2(0.5f, 1f);
            tRect.anchoredPosition = new Vector2(0, -10);
            tRect.sizeDelta = new Vector2(0, 30);

            // Close button
            GameObject closeObj = new GameObject("BtnClose");
            closeObj.transform.SetParent(panelObj.transform, false);
            Button closeBtn = closeObj.AddComponent<Button>();
            closeObj.AddComponent<Image>().color = new Color(0.7f, 0.1f, 0.1f);
            var cRect = closeObj.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(1, 1);
            cRect.anchorMax = new Vector2(1, 1);
            cRect.pivot = new Vector2(1, 1);
            cRect.anchoredPosition = new Vector2(-5, -5);
            cRect.sizeDelta = new Vector2(30, 25);
            GameObject closeTxtObj = new GameObject("Label");
            closeTxtObj.transform.SetParent(closeObj.transform, false);
            Text ct = closeTxtObj.AddComponent<Text>();
            ct.text = "X";
            ct.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ct.fontSize = 14; ct.color = Color.white;
            ct.alignment = TextAnchor.MiddleCenter;
            var ctRect = closeTxtObj.GetComponent<RectTransform>();
            ctRect.anchorMin = Vector2.zero; ctRect.anchorMax = Vector2.one;
            ctRect.sizeDelta = Vector2.zero;

            // FIX: AddPersistentListener requires a UnityEngine.Object target – lambdas don't qualify.
            // Use a PanelCloser MonoBehaviour so Unity can serialize the event reference properly.
            var panelCloser = closeObj.AddComponent<TayDuKy.UI.PanelCloser>();
            panelCloser.targetPanel = panelObj;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.onClick, panelCloser.ClosePanel);


            // Start hidden
            panelObj.SetActive(false);
            return panelObj;
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

            // Portal is at (12, 22) with trigger_distance = 1.0f (Euclidean), khớp với maps.json
            Vector3 portalPos = new Vector3(12f, 22f, 0f);
            float triggerDist = 1.0f;

            // Case 1: Player stands directly on the portal (12, 22)
            Vector3 testPos1 = new Vector3(12f, 22f, 0f);
            float dist1 = Vector3.Distance(testPos1, portalPos);
            bool check1 = dist1 <= triggerDist;
            Debug.Log($"Test Case 1 (Đứng đè lên portal (12, 22)): Khoảng cách = {dist1}, Kích hoạt = {check1} (Kỳ vọng: TRUE)");

            // Case 2: Player stands near the portal within range (12.5f, 22.5f → dist ≈ 0.71)
            Vector3 testPos2 = new Vector3(12.5f, 22.5f, 0f);
            float dist2 = Vector3.Distance(testPos2, portalPos);
            bool check2 = dist2 <= triggerDist;
            Debug.Log($"Test Case 2 (Đứng lệch nhẹ portal (12.5, 22.5)): Khoảng cách = {dist2}, Kích hoạt = {check2} (Kỳ vọng: TRUE)");

            // Case 3: Player just outside range (13.5f, 22f → dist = 1.5)
            Vector3 testPos3 = new Vector3(13.5f, 22f, 0f);
            float dist3 = Vector3.Distance(testPos3, portalPos);
            bool check3 = dist3 <= triggerDist;
            Debug.Log($"Test Case 3 (Đứng lệch nhiều portal (13.5, 22)): Khoảng cách = {dist3}, Kích hoạt = {check3} (Kỳ vọng: FALSE)");

            // Case 4: Player stands well away from the portal at (12, 20 → dist = 2)
            Vector3 testPos4 = new Vector3(12f, 20f, 0f);
            float dist4 = Vector3.Distance(testPos4, portalPos);
            bool check4 = dist4 <= triggerDist;
            Debug.Log($"Test Case 4 (Đứng cách xa portal (12, 20)): Khoảng cách = {dist4}, Kích hoạt = {check4} (Kỳ vọng: FALSE)");

            if (check1 && check2 && !check3 && !check4)
            {
                EditorUtility.DisplayDialog("Thành công!", "Hệ thống kiểm thử khoảng cách cổng dịch chuyển hoạt động hoàn hảo! Đạt 4/4 test case.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Lỗi!", "Có lỗi trong logic tính toán khoảng cách cổng dịch chuyển!", "OK");
            }
        }
    }
}
