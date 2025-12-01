using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using Incredicer.UI;

namespace Incredicer.Editor
{
    public static class CreateGameUI
    {
        // GUI Package paths
        private const string GUI_FONT_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Fonts/TMP_TiltWarp.asset";
        private const string GUI_FONT_OUTLINE_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Fonts/TMP_TiltWarp_Outline.asset";
        private const string GUI_BUTTON_GREEN_PATH = "Assets/Layer Lab/GUI-CasualFantasy/Prefabs/Prefabs_Component_Buttons/Button01_Demo_Green.prefab";
        private const string GUI_BUTTON_ORANGE_PATH = "Assets/Layer Lab/GUI-CasualFantasy/Prefabs/Prefabs_Component_Buttons/Button02_Demo_Orange.prefab";
        private const string GUI_FRAME_PATH = "Assets/Layer Lab/GUI-CasualFantasy/Prefabs/Prefabs_Component_Frames/BasicFrame_RoundedSolid01_Demo.prefab";

        [MenuItem("Incredicer/Create Game UI")]
        public static string Execute()
        {
            // Check if Canvas already exists with GameUI
            Canvas existingCanvas = Object.FindObjectOfType<Canvas>();
            if (existingCanvas != null && existingCanvas.GetComponentInChildren<GameUI>() != null)
            {
                // Delete existing and recreate
                Object.DestroyImmediate(existingCanvas.gameObject);
            }

            // Load fonts from GUI package
            TMP_FontAsset titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(GUI_FONT_OUTLINE_PATH);
            TMP_FontAsset textFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(GUI_FONT_PATH);

            // Fallback to default if not found
            if (titleFont == null) titleFont = TMP_Settings.defaultFontAsset;
            if (textFont == null) textFont = TMP_Settings.defaultFontAsset;

            // Create Canvas
            GameObject canvasObj = new GameObject("GameCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Add GameUI component
            GameUI gameUI = canvasObj.AddComponent<GameUI>();

            // ===== CURRENCY PANEL (top right) =====
            GameObject currencyPanel = CreatePanel(canvasObj.transform, "CurrencyPanel",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-20, -20), new Vector2(280, 100));

            // Stylish background
            Image currencyBg = currencyPanel.GetComponent<Image>();
            currencyBg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

            // Add rounded corners effect (via separate image if needed)
            // For now using solid color

            // Add vertical layout
            VerticalLayoutGroup vlg = currencyPanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 15, 15);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperRight;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Money text - big and bold
            GameObject moneyTextObj = CreateText(currencyPanel.transform, "MoneyText", "$0", 48,
                TextAlignmentOptions.Right, titleFont, new Color(0.4f, 1f, 0.5f));
            RectTransform moneyRT = moneyTextObj.GetComponent<RectTransform>();
            moneyRT.sizeDelta = new Vector2(240, 55);
            TextMeshProUGUI moneyText = moneyTextObj.GetComponent<TextMeshProUGUI>();

            // Dark matter panel (hidden by default)
            GameObject dmPanel = new GameObject("DarkMatterPanel");
            dmPanel.transform.SetParent(currencyPanel.transform);
            RectTransform dmPanelRT = dmPanel.AddComponent<RectTransform>();
            dmPanelRT.sizeDelta = new Vector2(0, 35);
            dmPanel.SetActive(false);

            GameObject dmTextObj = CreateText(dmPanel.transform, "DarkMatterText", "DM: 0", 28,
                TextAlignmentOptions.Right, textFont, new Color(0.8f, 0.5f, 1f));
            TextMeshProUGUI dmText = dmTextObj.GetComponent<TextMeshProUGUI>();
            RectTransform dmTextRT = dmTextObj.GetComponent<RectTransform>();
            dmTextRT.anchorMin = Vector2.zero;
            dmTextRT.anchorMax = Vector2.one;
            dmTextRT.offsetMin = Vector2.zero;
            dmTextRT.offsetMax = Vector2.zero;

            // ===== SKILL TREE BUTTON (top left) =====
            GameObject skillTreeBtn = CreateStylishButton(canvasObj.transform, "SkillTreeButton",
                "Skills",
                new Vector2(120, 50),
                new Color(0.6f, 0.4f, 0.8f),
                new Color(0.45f, 0.3f, 0.6f),
                textFont);

            RectTransform skillTreeRT = skillTreeBtn.GetComponent<RectTransform>();
            skillTreeRT.anchorMin = new Vector2(0, 1);
            skillTreeRT.anchorMax = new Vector2(0, 1);
            skillTreeRT.pivot = new Vector2(0, 1);
            skillTreeRT.anchoredPosition = new Vector2(20, -20);

            Button skillTreeBtnComp = skillTreeBtn.GetComponent<Button>();

            // ===== SHOP PANEL (bottom center) =====
            GameObject shopPanel = CreatePanel(canvasObj.transform, "ShopPanel",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 25), new Vector2(460, 90));

            Image shopBg = shopPanel.GetComponent<Image>();
            shopBg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

            // Add horizontal layout
            HorizontalLayoutGroup hlg = shopPanel.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(15, 15, 12, 12);
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Buy Dice button - Green
            GameObject buyDiceBtn = CreateStylishButton(shopPanel.transform, "BuyDiceButton",
                "Buy Extra Dice\n<size=65%>$10</size>",
                new Vector2(200, 66),
                new Color(0.2f, 0.65f, 0.3f),
                new Color(0.15f, 0.5f, 0.25f),
                textFont);
            Button buyDiceBtnComp = buyDiceBtn.GetComponent<Button>();
            TextMeshProUGUI buyDiceBtnText = buyDiceBtn.GetComponentInChildren<TextMeshProUGUI>();

            // Upgrade Dice button - Orange/Gold
            GameObject upgradeDiceBtn = CreateStylishButton(shopPanel.transform, "UpgradeDiceButton",
                "Upgrade Dice\n<size=65%>$25</size>",
                new Vector2(200, 66),
                new Color(0.85f, 0.55f, 0.15f),
                new Color(0.7f, 0.4f, 0.1f),
                textFont);
            Button upgradeDiceBtnComp = upgradeDiceBtn.GetComponent<Button>();
            TextMeshProUGUI upgradeDiceBtnText = upgradeDiceBtn.GetComponentInChildren<TextMeshProUGUI>();

            // Assign references to GameUI using SerializedObject
            SerializedObject so = new SerializedObject(gameUI);
            so.FindProperty("moneyText").objectReferenceValue = moneyText;
            so.FindProperty("darkMatterText").objectReferenceValue = dmText;
            so.FindProperty("darkMatterPanel").objectReferenceValue = dmPanel;
            so.FindProperty("buyDiceButton").objectReferenceValue = buyDiceBtnComp;
            so.FindProperty("buyDiceButtonText").objectReferenceValue = buyDiceBtnText;
            so.FindProperty("upgradeDiceButton").objectReferenceValue = upgradeDiceBtnComp;
            so.FindProperty("upgradeDiceButtonText").objectReferenceValue = upgradeDiceBtnText;
            so.FindProperty("skillTreeButton").objectReferenceValue = skillTreeBtnComp;

            // Assign font for floating text
            SerializedProperty fontProp = so.FindProperty("floatingTextFont");
            if (fontProp != null)
            {
                fontProp.objectReferenceValue = titleFont;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(gameUI);

            // Create EventSystem if needed
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            return "Created polished GameUI with GUI-CasualFantasy styling.";
        }

        private static GameObject CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = panel.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.7f);
            img.raycastTarget = false;

            return panel;
        }

        private static GameObject CreateText(Transform parent, string name, string text, int fontSize,
            TextAlignmentOptions alignment, TMP_FontAsset font, Color color)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent);

            RectTransform rt = textObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(260, 40);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();

            if (font != null)
            {
                tmp.font = font;
            }
            else if (TMP_Settings.defaultFontAsset != null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
            }

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.raycastTarget = false;

            return textObj;
        }

        private static GameObject CreateStylishButton(Transform parent, string name, string text,
            Vector2 size, Color normalColor, Color darkColor, TMP_FontAsset font)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = size;

            // Add LayoutElement to prevent layout group from overriding size
            var layoutElement = btnObj.AddComponent<UnityEngine.UI.LayoutElement>();
            layoutElement.minWidth = size.x;
            layoutElement.minHeight = size.y;
            layoutElement.preferredWidth = size.x;
            layoutElement.preferredHeight = size.y;

            // Main button image
            Image img = btnObj.AddComponent<Image>();
            img.color = normalColor;
            img.raycastTarget = true;

            // Create button with nice color transitions
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img; // Set target graphic BEFORE setting colors
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.1f;
            btn.colors = colors;

            // Add navigation (none for cleaner feel)
            Navigation nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(8, 6);
            textRT.offsetMax = new Vector2(-8, -6);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();

            if (font != null)
            {
                tmp.font = font;
            }
            else if (TMP_Settings.defaultFontAsset != null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
            }

            tmp.text = text;
            tmp.fontSize = 22;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            return btnObj;
        }
    }
}
