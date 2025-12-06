using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace Incredicer.Editor
{
    /// <summary>
    /// Master editor script to create all UI prefabs for the game and auto-assign them to their UI components.
    /// Run from menu: Tools > Incredicer > Create All UI Prefabs
    /// </summary>
    public class CreateAllUIPrefabs
    {
        private static UI.GUISpriteAssets guiAssets;
        private static string prefabFolder = "Assets/Prefabs/UI";

        [MenuItem("Tools/Incredicer/Create All UI Prefabs")]
        public static void CreateAllPrefabs()
        {
            // Load GUI assets
            guiAssets = Resources.Load<UI.GUISpriteAssets>("GUISpriteAssets");

            // Ensure prefab folder exists
            EnsureFolderExists(prefabFolder);

            // Create all prefabs
            Debug.Log("[CreateAllUIPrefabs] Creating all UI prefabs...");

            CreateTimeFracturePanelPrefab();
            CreateDiceShopPanelPrefab();
            CreateSkillTreePanelPrefab();
            CreateMissionsPanelPrefab();
            CreateDailyLoginPanelPrefab();
            CreateGlobalEventPanelPrefab();
            CreateMilestonesPanelPrefab();
            CreateMainMenuPanelPrefab();

            Debug.Log("[CreateAllUIPrefabs] All prefabs created successfully!");

            // Auto-assign prefabs to scene objects
            AutoAssignPrefabsToScene();
        }

        [MenuItem("Tools/Incredicer/Auto-Assign UI Prefabs")]
        public static void AutoAssignPrefabsToScene()
        {
            Debug.Log("[CreateAllUIPrefabs] Auto-assigning prefabs to scene UI components...");

            // Find and assign to TimeFractureUI
            AssignPrefabToComponent<TimeFracture.TimeFractureUI>("TimeFracturePanel", "panelPrefab");

            // Find and assign to DiceShopUI
            AssignPrefabToComponent<UI.DiceShopUI>("DiceShopPanel", "panelPrefab");

            // Find and assign to SkillTreeUI
            AssignPrefabToComponent<UI.SkillTreeUI>("SkillTreePanel", "panelPrefab");

            // Find and assign to MissionsUI
            AssignPrefabToComponent<Missions.MissionsUI>("MissionsPanel", "panelPrefab");

            // Find and assign to DailyLoginUI
            AssignPrefabToComponent<DailyLogin.DailyLoginUI>("DailyLoginPanel", "mainPanel");

            // Find and assign to GlobalEventUI (creates overlay, not direct panel)
            // GlobalEventUI creates its own panel structure, so skip

            // Find and assign to MilestonesUI (creates its own panel structure)
            // MilestonesUI creates its own panel structure, so skip

            // MainMenuUI creates its own panel structure, so skip

            // Mark scene dirty so changes are saved
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("[CreateAllUIPrefabs] Prefab assignment complete! Save the scene to persist changes.");
        }

        private static void AssignPrefabToComponent<T>(string prefabName, string fieldName) where T : MonoBehaviour
        {
            string prefabPath = $"{prefabFolder}/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogWarning($"[CreateAllUIPrefabs] Prefab not found: {prefabPath}");
                return;
            }

            T component = Object.FindObjectOfType<T>();
            if (component == null)
            {
                Debug.LogWarning($"[CreateAllUIPrefabs] Component not found in scene: {typeof(T).Name}");
                return;
            }

            // Use reflection to set the field
            var field = typeof(T).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(component, prefab);
                EditorUtility.SetDirty(component);
                Debug.Log($"[CreateAllUIPrefabs] Assigned {prefabName} to {typeof(T).Name}.{fieldName}");
            }
            else
            {
                Debug.LogWarning($"[CreateAllUIPrefabs] Field '{fieldName}' not found on {typeof(T).Name}");
            }
        }

        private static void EnsureFolderExists(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];

            for (int i = 1; i < folders.Length; i++)
            {
                string nextFolder = folders[i];
                string fullPath = currentPath + "/" + nextFolder;

                if (!AssetDatabase.IsValidFolder(fullPath))
                {
                    AssetDatabase.CreateFolder(currentPath, nextFolder);
                }
                currentPath = fullPath;
            }
        }

        #region Time Fracture Panel

        private static void CreateTimeFracturePanelPrefab()
        {
            GameObject panelRoot = new GameObject("TimeFracturePanel");
            RectTransform rootRect = panelRoot.AddComponent<RectTransform>();
            SetFullscreen(rootRect);

            // Dark background overlay
            Image bgImage = panelRoot.AddComponent<Image>();
            bgImage.color = new Color(0.02f, 0.01f, 0.08f, 0.95f);

            Button bgButton = panelRoot.AddComponent<Button>();
            bgButton.transition = Selectable.Transition.None;

            // Main panel
            GameObject mainPanel = CreateStyledPanel(panelRoot.transform, "MainPanel", 0.02f, 0.02f, 0.98f, 0.98f);

            // Content area
            GameObject contentArea = CreateContentArea(mainPanel.transform);

            // Create sections
            CreateTimeFractureHeader(contentArea.transform);
            CreateTimeFractureTimeShardsDisplay(contentArea.transform);
            CreateStyledSectionCard(contentArea.transform, "RequirementsSection", "REQUIREMENTS", new Color(1f, 0.75f, 0.3f));
            CreateStyledSectionCard(contentArea.transform, "RewardsSection", "REWARDS", new Color(0.3f, 1f, 0.5f));
            CreateStyledSectionCard(contentArea.transform, "BonusesSection", "CURRENT BONUSES", new Color(0.4f, 0.8f, 1f));
            CreateWarningSection(contentArea.transform);
            CreateFractureButton(contentArea.transform);

            // Close button
            CreateCloseButton(mainPanel.transform);

            SavePrefab(panelRoot, "TimeFracturePanel");
        }

        #endregion

        #region Dice Shop Panel

        private static void CreateDiceShopPanelPrefab()
        {
            GameObject panelRoot = new GameObject("DiceShopPanel");
            RectTransform rootRect = panelRoot.AddComponent<RectTransform>();
            SetFullscreen(rootRect);

            // Background
            Image panelBg = panelRoot.AddComponent<Image>();
            panelBg.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);

            CanvasGroup canvasGroup = panelRoot.AddComponent<CanvasGroup>();

            // Header
            CreateShopHeader(panelRoot.transform, "DICE SHOP", new Color(0.3f, 0.9f, 0.4f));

            // Scroll area
            CreateScrollArea(panelRoot.transform, 0.88f);

            SavePrefab(panelRoot, "DiceShopPanel");
        }

        #endregion

        #region Skill Tree Panel

        private static void CreateSkillTreePanelPrefab()
        {
            GameObject panelRoot = new GameObject("SkillTreePanel");
            RectTransform rootRect = panelRoot.AddComponent<RectTransform>();
            SetFullscreen(rootRect);

            // Background
            Image panelBg = panelRoot.AddComponent<Image>();
            panelBg.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);

            CanvasGroup canvasGroup = panelRoot.AddComponent<CanvasGroup>();

            // Header
            CreateShopHeader(panelRoot.transform, "SKILLS", new Color(0.8f, 0.6f, 1f));

            // Scroll area
            CreateScrollArea(panelRoot.transform, 0.88f);

            SavePrefab(panelRoot, "SkillTreePanel");
        }

        #endregion

        #region Missions Panel

        private static void CreateMissionsPanelPrefab()
        {
            GameObject panelRoot = new GameObject("MissionsPanel");
            RectTransform rootRect = panelRoot.AddComponent<RectTransform>();
            SetFullscreen(rootRect);

            // Background
            Image panelBg = panelRoot.AddComponent<Image>();
            panelBg.color = new Color(0.05f, 0.05f, 0.1f, 0.98f);

            CanvasGroup canvasGroup = panelRoot.AddComponent<CanvasGroup>();

            // Header (10% height)
            CreateMissionsHeader(panelRoot.transform);

            // Tabs (8% height)
            CreateMissionsTabs(panelRoot.transform);

            // Scroll area (remaining space)
            CreateScrollArea(panelRoot.transform, 0.82f);

            SavePrefab(panelRoot, "MissionsPanel");
        }

        private static void CreateMissionsHeader(Transform parent)
        {
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(parent, false);
            RectTransform headerRt = headerObj.AddComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 0.9f);
            headerRt.anchorMax = Vector2.one;
            headerRt.offsetMin = new Vector2(10, 5);
            headerRt.offsetMax = new Vector2(-10, -5);

            Image headerBg = headerObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.horizontalFrame != null)
            {
                headerBg.sprite = guiAssets.horizontalFrame;
                headerBg.type = Image.Type.Sliced;
            }
            headerBg.color = new Color(0.12f, 0.12f, 0.18f, 0.98f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerObj.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 0);
            titleRt.anchorMax = new Vector2(0.7f, 1);
            titleRt.offsetMin = new Vector2(20, 0);
            titleRt.offsetMax = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "MISSIONS";
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = new Color(1f, 0.6f, 0.2f);

            // Close button
            CreateCloseButtonInHeader(headerObj.transform);
        }

        private static void CreateMissionsTabs(Transform parent)
        {
            GameObject tabsObj = new GameObject("Tabs");
            tabsObj.transform.SetParent(parent, false);
            RectTransform tabsRect = tabsObj.AddComponent<RectTransform>();
            tabsRect.anchorMin = new Vector2(0, 0.82f);
            tabsRect.anchorMax = new Vector2(1, 0.9f);
            tabsRect.offsetMin = new Vector2(20, 0);
            tabsRect.offsetMax = new Vector2(-20, 0);

            HorizontalLayoutGroup hlg = tabsObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Daily tab
            CreateTabButton(tabsObj.transform, "DailyTab", "DAILY", true);

            // Weekly tab
            CreateTabButton(tabsObj.transform, "WeeklyTab", "WEEKLY", false);
        }

        private static void CreateTabButton(Transform parent, string name, string label, bool isActive)
        {
            GameObject tabObj = new GameObject(name);
            tabObj.transform.SetParent(parent, false);

            Image tabBg = tabObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonGreen != null)
            {
                tabBg.sprite = guiAssets.buttonGreen;
                tabBg.type = Image.Type.Sliced;
            }
            tabBg.color = isActive ? new Color(1f, 0.6f, 0.2f) : new Color(0.35f, 0.35f, 0.4f);

            Button tabButton = tabObj.AddComponent<Button>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(tabObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            SetFullscreen(textRect);

            TextMeshProUGUI tabText = textObj.AddComponent<TextMeshProUGUI>();
            tabText.text = label;
            tabText.fontSize = 32;
            tabText.fontStyle = FontStyles.Bold;
            tabText.alignment = TextAlignmentOptions.Center;
            tabText.color = isActive ? Color.white : new Color(0.7f, 0.7f, 0.7f);
        }

        #endregion

        #region Daily Login Panel

        private static void CreateDailyLoginPanelPrefab()
        {
            GameObject panelRoot = new GameObject("DailyLoginPanel");
            RectTransform rootRect = panelRoot.AddComponent<RectTransform>();
            SetFullscreen(rootRect);

            // Dark background
            Image bgImage = panelRoot.AddComponent<Image>();
            bgImage.color = new Color(0.02f, 0.01f, 0.08f, 0.95f);

            CanvasGroup canvasGroup = panelRoot.AddComponent<CanvasGroup>();

            // Title ribbon
            CreateDailyLoginTitle(panelRoot.transform);

            // Dice display area
            CreateDiceDisplay(panelRoot.transform);

            // Streak display
            CreateStreakDisplay(panelRoot.transform);

            // Close button
            CreateCloseButtonTopRight(panelRoot.transform);

            SavePrefab(panelRoot, "DailyLoginPanel");
        }

        private static void CreateDailyLoginTitle(Transform parent)
        {
            GameObject ribbonObj = new GameObject("TitleRibbon");
            ribbonObj.transform.SetParent(parent, false);
            RectTransform ribbonRect = ribbonObj.AddComponent<RectTransform>();
            ribbonRect.anchorMin = new Vector2(0.5f, 0.85f);
            ribbonRect.anchorMax = new Vector2(0.5f, 0.85f);
            ribbonRect.sizeDelta = new Vector2(550, 110);
            ribbonRect.anchoredPosition = new Vector2(0, 30);

            Image ribbonBg = ribbonObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.ribbonYellow != null)
            {
                ribbonBg.sprite = guiAssets.ribbonYellow;
                ribbonBg.type = Image.Type.Sliced;
                ribbonBg.color = Color.white;
            }
            else
            {
                ribbonBg.color = new Color(1f, 0.85f, 0.2f);
            }

            // Title text
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(ribbonObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            SetFullscreen(titleRect, 10);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "DAILY ROLL";
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.25f, 0.15f, 0.05f);
        }

        private static void CreateDiceDisplay(Transform parent)
        {
            GameObject diceContainer = new GameObject("DiceContainer");
            diceContainer.transform.SetParent(parent, false);
            RectTransform diceRect = diceContainer.AddComponent<RectTransform>();
            diceRect.anchorMin = new Vector2(0.5f, 0.35f);
            diceRect.anchorMax = new Vector2(0.5f, 0.35f);
            diceRect.sizeDelta = new Vector2(320, 320);

            // Dice image placeholder
            GameObject diceImg = new GameObject("DiceImage");
            diceImg.transform.SetParent(diceContainer.transform, false);
            RectTransform imgRect = diceImg.AddComponent<RectTransform>();
            imgRect.anchorMin = new Vector2(0.5f, 0.5f);
            imgRect.anchorMax = new Vector2(0.5f, 0.5f);
            imgRect.sizeDelta = new Vector2(280, 280);

            Image dice = diceImg.AddComponent<Image>();
            dice.color = new Color(1f, 0.85f, 0.2f);

            Button diceButton = diceContainer.AddComponent<Button>();
            diceButton.targetGraphic = dice;

            // Tap to roll text
            GameObject tapText = new GameObject("TapText");
            tapText.transform.SetParent(diceContainer.transform, false);
            RectTransform tapRect = tapText.AddComponent<RectTransform>();
            tapRect.anchorMin = new Vector2(0.5f, 0);
            tapRect.anchorMax = new Vector2(0.5f, 0);
            tapRect.sizeDelta = new Vector2(400, 60);
            tapRect.anchoredPosition = new Vector2(0, -70);

            TextMeshProUGUI tapTmp = tapText.AddComponent<TextMeshProUGUI>();
            tapTmp.text = "TAP DICE TO ROLL!";
            tapTmp.fontSize = 32;
            tapTmp.fontStyle = FontStyles.Bold;
            tapTmp.alignment = TextAlignmentOptions.Center;
            tapTmp.color = new Color(1f, 0.85f, 0.2f);
        }

        private static void CreateStreakDisplay(Transform parent)
        {
            // Streak frame
            GameObject streakFrame = new GameObject("StreakFrame");
            streakFrame.transform.SetParent(parent, false);
            RectTransform frameRect = streakFrame.AddComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.03f, 0.03f);
            frameRect.anchorMax = new Vector2(0.97f, 0.16f);

            Image frameBg = streakFrame.AddComponent<Image>();
            if (guiAssets != null && guiAssets.horizontalFrame != null)
            {
                frameBg.sprite = guiAssets.horizontalFrame;
                frameBg.type = Image.Type.Sliced;
                frameBg.color = Color.white;
            }
            else
            {
                frameBg.color = new Color(0.1f, 0.08f, 0.15f, 0.9f);
            }

            // Streak container
            GameObject streakContainer = new GameObject("StreakContainer");
            streakContainer.transform.SetParent(streakFrame.transform, false);
            RectTransform streakRect = streakContainer.AddComponent<RectTransform>();
            SetFullscreen(streakRect, 16);

            HorizontalLayoutGroup hlg = streakContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // Create 7 day nodes
            for (int i = 0; i < 7; i++)
            {
                CreateStreakNode(streakContainer.transform, i + 1);
            }
        }

        private static void CreateStreakNode(Transform parent, int day)
        {
            GameObject nodeContainer = new GameObject($"Day{day}");
            nodeContainer.transform.SetParent(parent, false);
            RectTransform containerRect = nodeContainer.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(52, 68);

            // Node
            GameObject nodeObj = new GameObject("Node");
            nodeObj.transform.SetParent(nodeContainer.transform, false);
            RectTransform nodeRect = nodeObj.AddComponent<RectTransform>();
            nodeRect.anchorMin = new Vector2(0.5f, 0.6f);
            nodeRect.anchorMax = new Vector2(0.5f, 0.6f);
            nodeRect.sizeDelta = new Vector2(48, 48);

            Image nodeImg = nodeObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.cardFrame != null)
            {
                nodeImg.sprite = guiAssets.cardFrame;
                nodeImg.type = Image.Type.Sliced;
            }
            nodeImg.color = new Color(0.3f, 0.3f, 0.35f);

            // Day number
            GameObject numObj = new GameObject("Number");
            numObj.transform.SetParent(nodeObj.transform, false);
            RectTransform numRect = numObj.AddComponent<RectTransform>();
            SetFullscreen(numRect);

            TextMeshProUGUI numText = numObj.AddComponent<TextMeshProUGUI>();
            numText.text = day.ToString();
            numText.fontSize = 24;
            numText.fontStyle = FontStyles.Bold;
            numText.alignment = TextAlignmentOptions.Center;
            numText.color = Color.white;
        }

        #endregion

        #region Global Event Panel

        private static void CreateGlobalEventPanelPrefab()
        {
            // Create overlay container
            GameObject overlay = new GameObject("GlobalEventOverlay");
            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            SetFullscreen(overlayRect);

            Image overlayBg = overlay.AddComponent<Image>();
            overlayBg.color = new Color(0.02f, 0.01f, 0.08f, 0.9f);

            // Main panel
            GameObject panelRoot = new GameObject("GlobalEventPanel");
            panelRoot.transform.SetParent(overlay.transform, false);
            RectTransform rootRect = panelRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.02f, 0.02f);
            rootRect.anchorMax = new Vector2(0.98f, 0.98f);
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Image panelBg = panelRoot.AddComponent<Image>();
            if (guiAssets != null && guiAssets.popupBackground != null)
            {
                panelBg.sprite = guiAssets.popupBackground;
                panelBg.type = Image.Type.Sliced;
                panelBg.color = Color.white;
            }
            else
            {
                panelBg.color = new Color(0.08f, 0.06f, 0.12f, 0.98f);
            }

            CanvasGroup canvasGroup = panelRoot.AddComponent<CanvasGroup>();

            // Header with event name
            CreateEventHeader(panelRoot.transform);

            // Progress section
            CreateEventProgressSection(panelRoot.transform);

            // Close button
            CreateCloseButtonTopRight(panelRoot.transform);

            SavePrefab(overlay, "GlobalEventPanel");
        }

        private static void CreateEventHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);
            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.88f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.offsetMin = new Vector2(24, 0);
            headerRect.offsetMax = new Vector2(-100, -16);

            // Title ribbon
            GameObject ribbonObj = new GameObject("TitleRibbon");
            ribbonObj.transform.SetParent(header.transform, false);
            RectTransform ribbonRect = ribbonObj.AddComponent<RectTransform>();
            ribbonRect.anchorMin = new Vector2(0.5f, 0.6f);
            ribbonRect.anchorMax = new Vector2(0.5f, 0.6f);
            ribbonRect.sizeDelta = new Vector2(500, 90);

            Image ribbonBg = ribbonObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.ribbonBlue != null)
            {
                ribbonBg.sprite = guiAssets.ribbonBlue;
                ribbonBg.type = Image.Type.Sliced;
                ribbonBg.color = Color.white;
            }
            else
            {
                ribbonBg.color = new Color(0.3f, 0.6f, 0.9f);
            }

            // Title text
            GameObject titleObj = new GameObject("EventName");
            titleObj.transform.SetParent(ribbonObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            SetFullscreen(titleRect, 10);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "COMMUNITY EVENT";
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
        }

        private static void CreateEventProgressSection(Transform parent)
        {
            GameObject progressSection = new GameObject("ProgressSection");
            progressSection.transform.SetParent(parent, false);
            RectTransform sectionRect = progressSection.AddComponent<RectTransform>();
            sectionRect.anchorMin = new Vector2(0, 0.75f);
            sectionRect.anchorMax = new Vector2(1, 0.88f);
            sectionRect.offsetMin = new Vector2(24, 0);
            sectionRect.offsetMax = new Vector2(-24, 0);

            // Progress bar background
            GameObject progressBg = new GameObject("ProgressBarBg");
            progressBg.transform.SetParent(progressSection.transform, false);
            RectTransform bgRect = progressBg.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.5f);
            bgRect.anchorMax = new Vector2(1, 0.5f);
            bgRect.sizeDelta = new Vector2(-80, 60);
            bgRect.anchoredPosition = new Vector2(0, 15);

            Image bgImage = progressBg.AddComponent<Image>();
            if (guiAssets != null && guiAssets.horizontalFrame != null)
            {
                bgImage.sprite = guiAssets.horizontalFrame;
                bgImage.type = Image.Type.Sliced;
            }
            bgImage.color = new Color(0.2f, 0.2f, 0.3f);

            // Progress fill
            GameObject fillMask = new GameObject("FillMask");
            fillMask.transform.SetParent(progressBg.transform, false);
            RectTransform maskRect = fillMask.AddComponent<RectTransform>();
            maskRect.anchorMin = new Vector2(0.02f, 0.15f);
            maskRect.anchorMax = new Vector2(0.98f, 0.85f);
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;

            Image maskImage = fillMask.AddComponent<Image>();
            maskImage.color = new Color(0.1f, 0.1f, 0.15f);

            GameObject progressFill = new GameObject("ProgressFill");
            progressFill.transform.SetParent(fillMask.transform, false);
            RectTransform fillRect = progressFill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0.5f, 1);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImage = progressFill.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.6f, 0.9f);

            // Progress text
            GameObject textObj = new GameObject("ProgressText");
            textObj.transform.SetParent(progressSection.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(500, 40);

            TextMeshProUGUI progressText = textObj.AddComponent<TextMeshProUGUI>();
            progressText.text = "0 / 1,000,000";
            progressText.fontSize = 36;
            progressText.fontStyle = FontStyles.Bold;
            progressText.alignment = TextAlignmentOptions.Center;
            progressText.color = Color.white;
        }

        #endregion

        #region Milestones Panel

        private static void CreateMilestonesPanelPrefab()
        {
            GameObject panelRoot = new GameObject("MilestonesPanel");
            RectTransform rootRect = panelRoot.AddComponent<RectTransform>();
            SetFullscreen(rootRect);

            // Dark background overlay
            Image bgImage = panelRoot.AddComponent<Image>();
            bgImage.color = new Color(0.02f, 0.01f, 0.05f, 0.92f);

            Button bgButton = panelRoot.AddComponent<Button>();
            bgButton.transition = Selectable.Transition.None;

            // Main panel
            GameObject mainPanel = new GameObject("MainPanel");
            mainPanel.transform.SetParent(panelRoot.transform, false);
            RectTransform mainRect = mainPanel.AddComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.04f, 0.05f);
            mainRect.anchorMax = new Vector2(0.96f, 0.95f);
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;

            Image mainBg = mainPanel.AddComponent<Image>();
            if (guiAssets != null && guiAssets.popupBackground != null)
            {
                mainBg.sprite = guiAssets.popupBackground;
                mainBg.type = Image.Type.Sliced;
                mainBg.color = Color.white;
            }
            else
            {
                mainBg.color = new Color(0.1f, 0.08f, 0.15f, 0.98f);
            }

            CanvasGroup canvasGroup = mainPanel.AddComponent<CanvasGroup>();

            Button mainButton = mainPanel.AddComponent<Button>();
            mainButton.transition = Selectable.Transition.None;

            // Content area
            GameObject contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(mainPanel.transform, false);
            RectTransform contentRect = contentArea.AddComponent<RectTransform>();
            SetFullscreen(contentRect, 28);

            VerticalLayoutGroup layout = contentArea.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;

            // Header
            CreateMilestonesHeader(contentArea.transform);

            // Progress bar
            CreateMilestonesProgressBar(contentArea.transform);

            // Scroll view
            CreateMilestonesScrollView(contentArea.transform);

            // Close button
            CreateCloseButtonOnPanel(mainPanel.transform);

            SavePrefab(panelRoot, "MilestonesPanel");
        }

        private static void CreateMilestonesHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, 110);

            LayoutElement layoutElem = header.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 110;
            layoutElem.flexibleHeight = 0;

            // Ribbon background
            if (guiAssets != null && guiAssets.ribbonYellow != null)
            {
                GameObject ribbonBg = new GameObject("RibbonBg");
                ribbonBg.transform.SetParent(header.transform, false);
                RectTransform ribbonRect = ribbonBg.AddComponent<RectTransform>();
                ribbonRect.anchorMin = new Vector2(0.02f, 0.05f);
                ribbonRect.anchorMax = new Vector2(0.98f, 0.95f);
                ribbonRect.offsetMin = Vector2.zero;
                ribbonRect.offsetMax = Vector2.zero;

                Image ribbonImg = ribbonBg.AddComponent<Image>();
                ribbonImg.sprite = guiAssets.ribbonYellow;
                ribbonImg.type = Image.Type.Sliced;
            }

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(header.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            SetFullscreen(titleRect);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "MILESTONES";
            titleText.fontSize = 62f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.95f, 0.75f, 0.2f);
        }

        private static void CreateMilestonesProgressBar(Transform parent)
        {
            GameObject progressContainer = new GameObject("OverallProgress");
            progressContainer.transform.SetParent(parent, false);

            RectTransform containerRect = progressContainer.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(0, 70);

            LayoutElement layoutElem = progressContainer.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 70;
            layoutElem.flexibleHeight = 0;

            Image progressBg = progressContainer.AddComponent<Image>();
            if (guiAssets != null && guiAssets.horizontalFrame != null)
            {
                progressBg.sprite = guiAssets.horizontalFrame;
                progressBg.type = Image.Type.Sliced;
                progressBg.color = Color.white;
            }
            else
            {
                progressBg.color = new Color(0.15f, 0.12f, 0.2f, 0.9f);
            }

            // Fill container
            GameObject fillContainer = new GameObject("FillContainer");
            fillContainer.transform.SetParent(progressContainer.transform, false);
            RectTransform fillContainerRect = fillContainer.AddComponent<RectTransform>();
            fillContainerRect.anchorMin = new Vector2(0.025f, 0.18f);
            fillContainerRect.anchorMax = new Vector2(0.975f, 0.82f);
            fillContainerRect.offsetMin = Vector2.zero;
            fillContainerRect.offsetMax = Vector2.zero;

            Image fillBgImg = fillContainer.AddComponent<Image>();
            fillBgImg.color = new Color(0.08f, 0.06f, 0.12f, 0.9f);

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillContainer.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = new Color(1f, 0.75f, 0.2f);

            // Progress text
            GameObject textObj = new GameObject("ProgressText");
            textObj.transform.SetParent(progressContainer.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            SetFullscreen(textRect);

            TextMeshProUGUI progressText = textObj.AddComponent<TextMeshProUGUI>();
            progressText.text = "0 / 0 Complete";
            progressText.fontSize = 34f;
            progressText.fontStyle = FontStyles.Bold;
            progressText.alignment = TextAlignmentOptions.Center;
            progressText.color = Color.white;
        }

        private static void CreateMilestonesScrollView(Transform parent)
        {
            GameObject scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(parent, false);

            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();

            LayoutElement layoutElem = scrollObj.AddComponent<LayoutElement>();
            layoutElem.flexibleHeight = 100;
            layoutElem.minHeight = 100;

            ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 60f;

            Image scrollBg = scrollObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.listFrame != null)
            {
                scrollBg.sprite = guiAssets.listFrame;
                scrollBg.type = Image.Type.Sliced;
            }
            scrollBg.color = new Color(0.06f, 0.04f, 0.1f, 0.95f);

            scrollObj.AddComponent<Mask>().showMaskGraphic = true;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = new Vector2(0.015f, 0.02f);
            viewportRect.anchorMax = new Vector2(0.985f, 0.98f);
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 16, 16);
            vlg.spacing = 18f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = contentRect;
        }

        #endregion

        #region Main Menu Panel

        private static void CreateMainMenuPanelPrefab()
        {
            GameObject panelRoot = new GameObject("MainMenuPanel");
            RectTransform rootRect = panelRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(1, 0);
            rootRect.anchorMax = new Vector2(1, 1);
            rootRect.pivot = new Vector2(1, 0.5f);
            rootRect.sizeDelta = new Vector2(520, 0);

            Image panelBg = panelRoot.AddComponent<Image>();
            panelBg.color = new Color(0.08f, 0.06f, 0.12f, 0.98f);

            CanvasGroup canvasGroup = panelRoot.AddComponent<CanvasGroup>();

            // Add outline
            Outline outline = panelRoot.AddComponent<Outline>();
            outline.effectColor = new Color(0.5f, 0.3f, 0.8f, 0.5f);
            outline.effectDistance = new Vector2(-3, 0);

            // Create menu buttons
            CreateMenuButtons(panelRoot.transform);

            SavePrefab(panelRoot, "MainMenuPanel");
        }

        private static void CreateMenuButtons(Transform parent)
        {
            float startY = -80;
            float buttonHeight = 112;
            float spacing = 16;
            int index = 0;

            string[] labels = { "Daily Login", "Missions", "Overclock", "Time Fracture", "Milestones", "Events", "Leaderboard" };
            Color[] colors = {
                new Color(0.3f, 0.9f, 0.4f),   // Daily Login
                new Color(1f, 0.6f, 0.2f),     // Missions
                new Color(1f, 0.4f, 0.1f),     // Overclock
                new Color(0.3f, 0.7f, 1f),     // Time Fracture
                new Color(1f, 0.85f, 0.2f),    // Milestones
                new Color(0.4f, 0.7f, 1f),     // Events
                new Color(1f, 0.9f, 0.3f)      // Leaderboard
            };

            foreach (string label in labels)
            {
                CreateMenuButton(parent, label, startY - (buttonHeight + spacing) * index, colors[index]);
                index++;
            }
        }

        private static void CreateMenuButton(Transform parent, string label, float yPos, Color color)
        {
            GameObject btnObj = new GameObject($"Btn_{label.Replace(" ", "")}");
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, yPos);
            rt.sizeDelta = new Vector2(-16, 112);

            Image bg = btnObj.AddComponent<Image>();
            bg.color = color;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Add outline for depth
            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(2, -2);

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btnObj.transform, false);

            RectTransform labelRt = labelObj.AddComponent<RectTransform>();
            SetFullscreen(labelRt, 8);

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label.ToUpper();
            labelText.fontSize = 40;
            labelText.fontStyle = FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;
        }

        #endregion

        #region Shared Components

        private static GameObject CreateStyledPanel(Transform parent, string name, float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(anchorMinX, anchorMinY);
            rect.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bg = panel.AddComponent<Image>();
            if (guiAssets != null && guiAssets.popupBackground != null)
            {
                bg.sprite = guiAssets.popupBackground;
                bg.type = Image.Type.Sliced;
                bg.color = Color.white;
            }
            else
            {
                bg.color = new Color(0.12f, 0.08f, 0.18f, 0.98f);
            }

            panel.AddComponent<CanvasGroup>();

            Button blocker = panel.AddComponent<Button>();
            blocker.transition = Selectable.Transition.None;

            return panel;
        }

        private static GameObject CreateContentArea(Transform parent)
        {
            GameObject contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(parent, false);

            RectTransform rect = contentArea.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(20, 20);
            rect.offsetMax = new Vector2(-20, -20);

            VerticalLayoutGroup layout = contentArea.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = contentArea.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return contentArea;
        }

        private static void CreateTimeFractureHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform rect = header.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 80);

            LayoutElement layout = header.AddComponent<LayoutElement>();
            layout.preferredHeight = 80;
            layout.flexibleWidth = 1;

            if (guiAssets != null && guiAssets.ribbonPurple != null)
            {
                Image ribbonBg = header.AddComponent<Image>();
                ribbonBg.sprite = guiAssets.ribbonPurple;
                ribbonBg.type = Image.Type.Sliced;
                ribbonBg.color = Color.white;
            }

            // Title text
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(header.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            SetFullscreen(titleRect, 10);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "TIME FRACTURE";
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 36;
            titleText.fontSizeMax = 48;
        }

        private static void CreateTimeFractureTimeShardsDisplay(Transform parent)
        {
            GameObject container = new GameObject("TimeShardsDisplay");
            container.transform.SetParent(parent, false);

            RectTransform rect = container.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 100);

            LayoutElement layoutElem = container.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 100;
            layoutElem.flexibleWidth = 1;

            Image bg = container.AddComponent<Image>();
            if (guiAssets != null && guiAssets.horizontalFrame != null)
            {
                bg.sprite = guiAssets.horizontalFrame;
                bg.type = Image.Type.Sliced;
                bg.color = new Color(0.6f, 0.4f, 1f, 0.3f);
            }
            else
            {
                bg.color = new Color(0.3f, 0.2f, 0.5f, 0.5f);
            }

            HorizontalLayoutGroup hLayout = container.AddComponent<HorizontalLayoutGroup>();
            hLayout.padding = new RectOffset(20, 20, 10, 10);
            hLayout.spacing = 20f;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = false;

            // Level text
            GameObject levelObj = new GameObject("LevelText");
            levelObj.transform.SetParent(container.transform, false);
            RectTransform levelRect = levelObj.AddComponent<RectTransform>();
            levelRect.sizeDelta = new Vector2(300, 80);

            TextMeshProUGUI levelText = levelObj.AddComponent<TextMeshProUGUI>();
            levelText.text = "Fracture Level: 0";
            levelText.fontSize = 36;
            levelText.fontStyle = FontStyles.Bold;
            levelText.color = Color.white;
            levelText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateStyledSectionCard(Transform parent, string name, string title, Color titleColor)
        {
            GameObject card = new GameObject(name);
            card.transform.SetParent(parent, false);

            RectTransform rect = card.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 140);

            LayoutElement layoutElem = card.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 140;
            layoutElem.flexibleWidth = 1;

            Image bg = card.AddComponent<Image>();
            if (guiAssets != null && guiAssets.listFrame != null)
            {
                bg.sprite = guiAssets.listFrame;
                bg.type = Image.Type.Sliced;
                bg.color = Color.white;
            }
            else
            {
                bg.color = new Color(0.95f, 0.95f, 0.95f, 0.95f);
            }

            VerticalLayoutGroup cardLayout = card.AddComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(20, 20, 12, 12);
            cardLayout.spacing = 8f;
            cardLayout.childAlignment = TextAnchor.UpperCenter;
            cardLayout.childControlHeight = false;
            cardLayout.childControlWidth = true;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = title;
            titleTmp.fontSize = 32;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = titleColor;
            titleTmp.alignment = TextAlignmentOptions.Center;

            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 38;

            // Content
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI contentTmp = contentObj.AddComponent<TextMeshProUGUI>();
            contentTmp.text = "Loading...";
            contentTmp.fontSize = 28;
            contentTmp.color = new Color(0.15f, 0.15f, 0.15f);
            contentTmp.alignment = TextAlignmentOptions.Center;
            contentTmp.richText = true;

            LayoutElement contentLayout = contentObj.AddComponent<LayoutElement>();
            contentLayout.preferredHeight = 70;
            contentLayout.flexibleHeight = 1;
        }

        private static void CreateWarningSection(Transform parent)
        {
            GameObject warning = new GameObject("WarningSection");
            warning.transform.SetParent(parent, false);

            RectTransform rect = warning.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 60);

            LayoutElement layoutElem = warning.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 60;
            layoutElem.flexibleWidth = 1;

            Image bg = warning.AddComponent<Image>();
            bg.color = new Color(1f, 0.2f, 0.2f, 0.2f);

            // Warning text
            GameObject textObj = new GameObject("WarningText");
            textObj.transform.SetParent(warning.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            SetFullscreen(textRect, 16);

            TextMeshProUGUI warningText = textObj.AddComponent<TextMeshProUGUI>();
            warningText.text = "! All progress will be RESET!";
            warningText.fontSize = 26;
            warningText.fontStyle = FontStyles.Bold;
            warningText.color = new Color(1f, 0.4f, 0.4f);
            warningText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateFractureButton(Transform parent)
        {
            GameObject container = new GameObject("FractureButtonContainer");
            container.transform.SetParent(parent, false);

            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(0, 90);

            LayoutElement layoutElem = container.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 90;
            layoutElem.flexibleWidth = 1;

            // Main button
            GameObject btnObj = new GameObject("FractureButton");
            btnObj.transform.SetParent(container.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.05f, 0.1f);
            btnRect.anchorMax = new Vector2(0.95f, 0.9f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnBg = btnObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonPurple != null)
            {
                btnBg.sprite = guiAssets.buttonPurple;
                btnBg.type = Image.Type.Sliced;
                btnBg.color = Color.white;
            }
            else
            {
                btnBg.color = new Color(0.6f, 0.3f, 0.9f);
            }

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnBg;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            SetFullscreen(textRect, 10);

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "ACTIVATE TIME FRACTURE";
            btnText.fontSize = 36;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateCloseButton(Transform parent)
        {
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(parent, false);

            RectTransform rect = closeObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-8, -8);
            rect.sizeDelta = new Vector2(60, 60);

            Image bg = closeObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonRed != null)
            {
                bg.sprite = guiAssets.buttonRed;
                bg.type = Image.Type.Sliced;
                bg.color = Color.white;
            }
            else
            {
                bg.color = new Color(0.9f, 0.3f, 0.3f);
            }

            Button btn = closeObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            // X text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(closeObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            SetFullscreen(textRect);

            TextMeshProUGUI closeText = textObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.fontSize = 36;
            closeText.fontStyle = FontStyles.Bold;
            closeText.color = Color.white;
            closeText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateCloseButtonTopRight(Transform parent)
        {
            CreateCloseButton(parent);
        }

        private static void CreateCloseButtonOnPanel(Transform parent)
        {
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(parent, false);

            RectTransform rect = closeObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-12, -12);
            rect.sizeDelta = new Vector2(70, 70);

            Image bg = closeObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonRed != null)
            {
                bg.sprite = guiAssets.buttonRed;
                bg.type = Image.Type.Sliced;
                bg.color = Color.white;
            }
            else
            {
                bg.color = new Color(0.85f, 0.25f, 0.25f);
            }

            Button btn = closeObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            // X text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(closeObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            SetFullscreen(textRect);

            TextMeshProUGUI closeText = textObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.fontSize = 42;
            closeText.fontStyle = FontStyles.Bold;
            closeText.color = Color.white;
            closeText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateShopHeader(Transform parent, string title, Color titleColor)
        {
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(parent, false);
            RectTransform headerRt = headerObj.AddComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 0.88f);
            headerRt.anchorMax = Vector2.one;
            headerRt.offsetMin = new Vector2(10, 5);
            headerRt.offsetMax = new Vector2(-10, -5);

            Image headerBg = headerObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.horizontalFrame != null)
            {
                headerBg.sprite = guiAssets.horizontalFrame;
                headerBg.type = Image.Type.Sliced;
            }
            headerBg.color = new Color(0.12f, 0.12f, 0.18f, 0.98f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerObj.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 0);
            titleRt.anchorMax = new Vector2(0.35f, 1);
            titleRt.offsetMin = new Vector2(15, 0);
            titleRt.offsetMax = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = titleColor;

            // Currency display
            GameObject moneyObj = new GameObject("Currency");
            moneyObj.transform.SetParent(headerObj.transform, false);
            RectTransform moneyRt = moneyObj.AddComponent<RectTransform>();
            moneyRt.anchorMin = new Vector2(0.35f, 0);
            moneyRt.anchorMax = new Vector2(0.8f, 1);
            moneyRt.offsetMin = Vector2.zero;
            moneyRt.offsetMax = Vector2.zero;

            TextMeshProUGUI moneyText = moneyObj.AddComponent<TextMeshProUGUI>();
            moneyText.text = "$0";
            moneyText.fontSize = 40;
            moneyText.fontStyle = FontStyles.Bold;
            moneyText.alignment = TextAlignmentOptions.Center;
            moneyText.color = titleColor;

            // Close button
            CreateCloseButtonInHeader(headerObj.transform);
        }

        private static void CreateCloseButtonInHeader(Transform parent)
        {
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(parent, false);
            RectTransform closeRt = closeObj.AddComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(0.85f, 0.1f);
            closeRt.anchorMax = new Vector2(0.98f, 0.9f);
            closeRt.offsetMin = Vector2.zero;
            closeRt.offsetMax = Vector2.zero;

            Image closeBg = closeObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonRed != null)
            {
                closeBg.sprite = guiAssets.buttonRed;
                closeBg.type = Image.Type.Sliced;
                closeBg.color = Color.white;
            }
            else
            {
                closeBg.color = new Color(0.9f, 0.3f, 0.3f);
            }

            Button closeButton = closeObj.AddComponent<Button>();

            GameObject closeTextObj = new GameObject("X");
            closeTextObj.transform.SetParent(closeObj.transform, false);
            RectTransform closeTextRt = closeTextObj.AddComponent<RectTransform>();
            SetFullscreen(closeTextRt);

            TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.fontSize = 40;
            closeText.fontStyle = FontStyles.Bold;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.color = Color.white;
        }

        private static void CreateScrollArea(Transform parent, float topAnchor)
        {
            GameObject scrollObj = new GameObject("ScrollArea");
            scrollObj.transform.SetParent(parent, false);
            RectTransform scrollRt = scrollObj.AddComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0, 0);
            scrollRt.anchorMax = new Vector2(1, topAnchor);
            scrollRt.offsetMin = new Vector2(10, 10);
            scrollRt.offsetMax = new Vector2(-10, -5);

            ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.1f;
            scroll.scrollSensitivity = 20f;

            scrollObj.AddComponent<RectMask2D>();

            Image scrollBg = scrollObj.AddComponent<Image>();
            scrollBg.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);

            scroll.viewport = scrollRt;

            // Content
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollObj.transform, false);
            RectTransform contentContainer = contentObj.AddComponent<RectTransform>();
            contentContainer.anchorMin = new Vector2(0, 1);
            contentContainer.anchorMax = new Vector2(1, 1);
            contentContainer.pivot = new Vector2(0.5f, 1);
            contentContainer.anchoredPosition = Vector2.zero;
            contentContainer.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentContainer;
        }

        #endregion

        #region Utility Methods

        private static void SetFullscreen(RectTransform rect, float padding = 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }

        private static void SavePrefab(GameObject obj, string name)
        {
            string path = $"{prefabFolder}/{name}.prefab";

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);

            Debug.Log($"[CreateAllUIPrefabs] Created prefab: {path}");
        }

        #endregion
    }
}
