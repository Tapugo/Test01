using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace Incredicer.Editor
{
    /// <summary>
    /// Editor script to create a polished Time Fracture UI prefab.
    /// Run from menu: Tools > Incredicer > Create Time Fracture Prefab
    /// </summary>
    public class CreateTimeFracturePrefab
    {
        private static UI.GUISpriteAssets guiAssets;

        [MenuItem("Tools/Incredicer/Create Time Fracture Prefab")]
        public static void CreatePrefab()
        {
            // Load GUI assets
            guiAssets = Resources.Load<UI.GUISpriteAssets>("GUISpriteAssets");

            // Create root panel (fullscreen overlay)
            GameObject panelRoot = new GameObject("TimeFracturePanelPrefab");
            RectTransform rootRect = panelRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // Dark background overlay
            Image bgImage = panelRoot.AddComponent<Image>();
            bgImage.color = new Color(0.02f, 0.01f, 0.08f, 0.95f);

            // Add button for closing on background click
            Button bgButton = panelRoot.AddComponent<Button>();
            bgButton.transition = Selectable.Transition.None;

            // Main panel - centered with good margins
            GameObject mainPanel = CreateMainPanel(panelRoot.transform);

            // Content area with scroll capability
            GameObject contentArea = CreateContentArea(mainPanel.transform);

            // Create all sections with clean spacing
            CreateHeader(contentArea.transform);
            CreateTimeShardsDisplay(contentArea.transform);
            CreateSectionCard(contentArea.transform, "RequirementsSection", "REQUIREMENTS", new Color(1f, 0.75f, 0.3f));
            CreateSectionCard(contentArea.transform, "RewardsSection", "REWARDS", new Color(0.3f, 1f, 0.5f));
            CreateSectionCard(contentArea.transform, "BonusesSection", "CURRENT BONUSES", new Color(0.4f, 0.8f, 1f));
            CreateWarningSection(contentArea.transform);
            CreateFractureButton(contentArea.transform);

            // Close button (top right)
            CreateCloseButton(mainPanel.transform);

            // Save as prefab
            string prefabPath = "Assets/Prefabs/UI/TimeFracturePanel.prefab";

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

            // Save prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(panelRoot, prefabPath);

            // Clean up scene object
            Object.DestroyImmediate(panelRoot);

            // Select the created prefab
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            Debug.Log($"[CreateTimeFracturePrefab] Polished prefab created at: {prefabPath}");
        }

        private static GameObject CreateMainPanel(Transform parent)
        {
            GameObject mainPanel = new GameObject("MainPanel");
            mainPanel.transform.SetParent(parent, false);

            RectTransform rect = mainPanel.AddComponent<RectTransform>();
            // Fullscreen with small padding for portrait mode
            rect.anchorMin = new Vector2(0.02f, 0.02f);
            rect.anchorMax = new Vector2(0.98f, 0.98f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Use GUI popup background
            Image bg = mainPanel.AddComponent<Image>();
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

            mainPanel.AddComponent<CanvasGroup>();

            // Block clicks from going through
            Button blocker = mainPanel.AddComponent<Button>();
            blocker.transition = Selectable.Transition.None;

            return mainPanel;
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

            // Add content size fitter for proper sizing
            ContentSizeFitter fitter = contentArea.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return contentArea;
        }

        private static void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform rect = header.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 80);

            LayoutElement layout = header.AddComponent<LayoutElement>();
            layout.preferredHeight = 80;
            layout.flexibleWidth = 1;

            // Ribbon background
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
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(10, 5);
            titleRect.offsetMax = new Vector2(-10, -5);

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

        private static void CreateTimeShardsDisplay(Transform parent)
        {
            GameObject container = new GameObject("TimeShardsDisplay");
            container.transform.SetParent(parent, false);

            RectTransform rect = container.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 100);

            LayoutElement layoutElem = container.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 100;
            layoutElem.flexibleWidth = 1;

            // Background frame
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

            // Crystal icon
            GameObject iconContainer = new GameObject("CrystalIcon");
            iconContainer.transform.SetParent(container.transform, false);
            RectTransform iconRect = iconContainer.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(70, 70);

            Image iconImg = iconContainer.AddComponent<Image>();
            iconImg.color = new Color(0.4f, 0.7f, 1f);

            // Info container
            GameObject infoContainer = new GameObject("Info");
            infoContainer.transform.SetParent(container.transform, false);
            RectTransform infoRect = infoContainer.AddComponent<RectTransform>();
            infoRect.sizeDelta = new Vector2(300, 80);

            VerticalLayoutGroup infoLayout = infoContainer.AddComponent<VerticalLayoutGroup>();
            infoLayout.spacing = 8f;
            infoLayout.childAlignment = TextAnchor.MiddleLeft;
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = false;

            // Level text
            GameObject levelObj = new GameObject("LevelText");
            levelObj.transform.SetParent(infoContainer.transform, false);
            TextMeshProUGUI levelText = levelObj.AddComponent<TextMeshProUGUI>();
            levelText.text = "Fracture Level: 0";
            levelText.fontSize = 36;
            levelText.fontStyle = FontStyles.Bold;
            levelText.color = Color.white;
            levelText.alignment = TextAlignmentOptions.Left;

            LayoutElement levelElem = levelObj.AddComponent<LayoutElement>();
            levelElem.preferredHeight = 40;

            // Shards text
            GameObject shardsObj = new GameObject("ShardsText");
            shardsObj.transform.SetParent(infoContainer.transform, false);
            TextMeshProUGUI shardsText = shardsObj.AddComponent<TextMeshProUGUI>();
            shardsText.text = "Time Shards: 0";
            shardsText.fontSize = 32;
            shardsText.color = new Color(0.5f, 0.85f, 1f);
            shardsText.alignment = TextAlignmentOptions.Left;

            LayoutElement shardsElem = shardsObj.AddComponent<LayoutElement>();
            shardsElem.preferredHeight = 36;
        }

        private static void CreateSectionCard(Transform parent, string name, string title, Color titleColor)
        {
            GameObject card = new GameObject(name);
            card.transform.SetParent(parent, false);

            RectTransform rect = card.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 260);

            LayoutElement layoutElem = card.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 260;
            layoutElem.flexibleWidth = 1;

            // Card background - use listFrame for clean look
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

            // Title with colored text
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = title;
            titleTmp.fontSize = 64;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = titleColor;
            titleTmp.alignment = TextAlignmentOptions.Center;

            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 76;

            // Content text - dark for readability on light background
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI contentTmp = contentObj.AddComponent<TextMeshProUGUI>();
            contentTmp.text = "Loading...";
            contentTmp.fontSize = 56;
            contentTmp.color = new Color(0.15f, 0.15f, 0.15f);
            contentTmp.alignment = TextAlignmentOptions.Center;
            contentTmp.enableAutoSizing = true;
            contentTmp.fontSizeMin = 44;
            contentTmp.fontSizeMax = 56;
            contentTmp.richText = true;

            LayoutElement contentLayout = contentObj.AddComponent<LayoutElement>();
            contentLayout.preferredHeight = 140;
            contentLayout.flexibleHeight = 1;
        }

        private static void CreateWarningSection(Transform parent)
        {
            GameObject warning = new GameObject("WarningSection");
            warning.transform.SetParent(parent, false);

            RectTransform rect = warning.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 100);

            LayoutElement layoutElem = warning.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 100;
            layoutElem.flexibleWidth = 1;

            // Red tinted background
            Image bg = warning.AddComponent<Image>();
            bg.color = new Color(1f, 0.2f, 0.2f, 0.2f);

            // Use vertical layout for centered content
            VerticalLayoutGroup vLayout = warning.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(16, 16, 12, 12);
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;

            // Warning text - centered with icon included in text
            GameObject textObj = new GameObject("WarningText");
            textObj.transform.SetParent(warning.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(0, 76);

            TextMeshProUGUI warningText = textObj.AddComponent<TextMeshProUGUI>();
            warningText.text = "⚠ All progress will be RESET!";
            warningText.fontSize = 52;
            warningText.fontStyle = FontStyles.Bold;
            warningText.color = new Color(1f, 0.4f, 0.4f);
            warningText.alignment = TextAlignmentOptions.Center;
            warningText.enableAutoSizing = true;
            warningText.fontSizeMin = 40;
            warningText.fontSizeMax = 52;

            LayoutElement textLayout = textObj.AddComponent<LayoutElement>();
            textLayout.preferredHeight = 76;
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

            // Button glow (optional)
            GameObject glowObj = new GameObject("ButtonGlow");
            glowObj.transform.SetParent(container.transform, false);
            RectTransform glowRect = glowObj.AddComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.05f, 0);
            glowRect.anchorMax = new Vector2(0.95f, 1);
            glowRect.offsetMin = new Vector2(-10, -10);
            glowRect.offsetMax = new Vector2(10, 10);

            Image glowImg = glowObj.AddComponent<Image>();
            glowImg.color = new Color(0.6f, 0.3f, 1f, 0.4f);

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

            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            btn.colors = colors;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "ACTIVATE TIME FRACTURE";
            btnText.fontSize = 36;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.enableAutoSizing = true;
            btnText.fontSizeMin = 28;
            btnText.fontSizeMax = 36;
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

            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            btn.colors = colors;

            // X text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(closeObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI closeText = textObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.fontSize = 36;
            closeText.fontStyle = FontStyles.Bold;
            closeText.color = Color.white;
            closeText.alignment = TextAlignmentOptions.Center;
        }
    }
}
