using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Incredicer.Editor
{
    /// <summary>
    /// One-click fix for all UI issues - Skill Tree and Dice Shop
    /// </summary>
    public class FixAllUIEditor : EditorWindow
    {
        [MenuItem("Incredicer/Fix All UI (Skill Tree + Dice Shop)")]
        public static void FixAllUI()
        {
            Debug.Log("=== Starting Complete UI Fix ===");

            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found!");
                return;
            }

            FixSkillTreeUI(canvas);
            FixDiceShopUI(canvas);

            // Save scene
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("=== Complete UI Fix Done! Save the scene! ===");
        }

        private static void FixSkillTreeUI(Canvas canvas)
        {
            Debug.Log("[FixUI] Fixing Skill Tree UI...");

            // Delete existing SkillTreeUI components and panels
            var existingUIs = Object.FindObjectsOfType<UI.SkillTreeUI>(true);
            foreach (var ui in existingUIs)
            {
                Object.DestroyImmediate(ui.gameObject);
            }

            // Also delete any leftover SkillTreePanel
            foreach (Transform child in canvas.transform)
            {
                if (child.name == "SkillTreePanel" || child.name == "SkillTreeUI")
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            // Create SkillTreeUI holder (stays ACTIVE always)
            GameObject holderObj = new GameObject("SkillTreeUI");
            holderObj.transform.SetParent(canvas.transform, false);

            RectTransform holderRt = holderObj.AddComponent<RectTransform>();
            holderRt.anchorMin = Vector2.zero;
            holderRt.anchorMax = Vector2.one;
            holderRt.offsetMin = Vector2.zero;
            holderRt.offsetMax = Vector2.zero;

            // Add SkillTreeUI component to the HOLDER (which stays active)
            UI.SkillTreeUI skillTreeUI = holderObj.AddComponent<UI.SkillTreeUI>();

            // Create the PANEL as a child (this is what gets shown/hidden)
            GameObject panelObj = new GameObject("SkillTreePanel");
            panelObj.transform.SetParent(holderObj.transform, false);

            RectTransform panelRt = panelObj.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            Image panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.05f, 0.05f, 0.1f, 0.98f);
            panelBg.raycastTarget = true;

            CanvasGroup cg = panelObj.AddComponent<CanvasGroup>();

            // Wire up references - the panel is the child, component is on holder
            SerializedObject so = new SerializedObject(skillTreeUI);
            so.FindProperty("skillTreePanel").objectReferenceValue = panelObj;
            so.FindProperty("panelCanvasGroup").objectReferenceValue = cg;
            so.FindProperty("panelRect").objectReferenceValue = panelRt;

            // Clear dynamic references (will be created at runtime)
            so.FindProperty("darkMatterText").objectReferenceValue = null;
            so.FindProperty("skillPointsText").objectReferenceValue = null;
            so.FindProperty("closeButton").objectReferenceValue = null;
            so.FindProperty("treeContainer").objectReferenceValue = null;
            so.FindProperty("nodeContainer").objectReferenceValue = null;
            so.FindProperty("connectionContainer").objectReferenceValue = null;
            so.FindProperty("scrollRect").objectReferenceValue = null;
            so.FindProperty("infoPanelRect").objectReferenceValue = null;
            so.FindProperty("nodeInfoPanel").objectReferenceValue = null;
            so.FindProperty("nodeNameText").objectReferenceValue = null;
            so.FindProperty("nodeDescriptionText").objectReferenceValue = null;
            so.FindProperty("nodeCostText").objectReferenceValue = null;
            so.FindProperty("purchaseButton").objectReferenceValue = null;
            so.FindProperty("purchaseButtonText").objectReferenceValue = null;

            // Set large node size for mobile
            so.FindProperty("nodeSize").floatValue = 120f;
            so.FindProperty("nodeSpacingX").floatValue = 160f;
            so.FindProperty("nodeSpacingY").floatValue = 140f;

            so.ApplyModifiedProperties();

            // Panel starts HIDDEN, but holder stays active (so Awake runs)
            panelObj.SetActive(false);
            holderObj.SetActive(true);

            EditorUtility.SetDirty(skillTreeUI);
            Debug.Log("[FixUI] Skill Tree UI restructured - holder active, panel hidden");
        }

        private static void FixDiceShopUI(Canvas canvas)
        {
            Debug.Log("[FixUI] Fixing Dice Shop UI...");

            // Find the SkillTreeButton to copy its style
            Transform skillTreeBtn = null;
            foreach (Transform child in canvas.transform)
            {
                if (child.name == "SkillTreeButton")
                {
                    skillTreeBtn = child;
                    break;
                }
            }

            Sprite buttonSprite = null;
            Vector2 buttonSize = new Vector2(260, 130);
            if (skillTreeBtn != null)
            {
                Image btnImg = skillTreeBtn.GetComponent<Image>();
                if (btnImg != null) buttonSprite = btnImg.sprite;
                RectTransform btnRt = skillTreeBtn.GetComponent<RectTransform>();
                if (btnRt != null) buttonSize = btnRt.sizeDelta;
            }

            // Delete existing DiceShopUI and DiceShopButton
            var existingShopUIs = Object.FindObjectsOfType<UI.DiceShopUI>(true);
            foreach (var ui in existingShopUIs)
            {
                Object.DestroyImmediate(ui.gameObject);
            }

            foreach (Transform child in canvas.transform)
            {
                if (child.name == "DiceShopButton" || child.name == "DiceShopUI")
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            // Create DiceShopUI container (stays active)
            GameObject shopUIObj = new GameObject("DiceShopUI");
            shopUIObj.transform.SetParent(canvas.transform, false);

            RectTransform mainRt = shopUIObj.AddComponent<RectTransform>();
            mainRt.anchorMin = Vector2.zero;
            mainRt.anchorMax = Vector2.one;
            mainRt.offsetMin = Vector2.zero;
            mainRt.offsetMax = Vector2.zero;

            UI.DiceShopUI shopUI = shopUIObj.AddComponent<UI.DiceShopUI>();

            // === DICE SHOP BUTTON (same style as Skill Tree, below it) ===
            GameObject diceShopBtnObj = new GameObject("DiceShopButton");
            diceShopBtnObj.transform.SetParent(canvas.transform, false);

            RectTransform diceShopBtnRt = diceShopBtnObj.AddComponent<RectTransform>();
            diceShopBtnRt.anchorMin = new Vector2(0, 1);
            diceShopBtnRt.anchorMax = new Vector2(0, 1);
            diceShopBtnRt.pivot = new Vector2(0, 1);
            // Position next to SkillTreeButton (which is at x=20, y=-100)
            diceShopBtnRt.anchoredPosition = new Vector2(290, -100);
            diceShopBtnRt.sizeDelta = buttonSize;

            Image diceShopBtnImg = diceShopBtnObj.AddComponent<Image>();
            if (buttonSprite != null)
            {
                diceShopBtnImg.sprite = buttonSprite;
                diceShopBtnImg.type = Image.Type.Sliced;
                diceShopBtnImg.color = new Color(0.6f, 0.8f, 1.0f);
            }
            else
            {
                diceShopBtnImg.color = new Color(0.2f, 0.5f, 0.9f, 0.95f);
            }

            Button diceShopBtn = diceShopBtnObj.AddComponent<Button>();
            ColorBlock colors = diceShopBtn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            diceShopBtn.colors = colors;
            diceShopBtn.targetGraphic = diceShopBtnImg;

            LayoutElement diceShopBtnLE = diceShopBtnObj.AddComponent<LayoutElement>();
            diceShopBtnLE.minWidth = buttonSize.x;
            diceShopBtnLE.minHeight = buttonSize.y;
            diceShopBtnLE.preferredWidth = buttonSize.x;
            diceShopBtnLE.preferredHeight = buttonSize.y;

            // Button Text
            GameObject diceShopTextObj = new GameObject("Text");
            diceShopTextObj.transform.SetParent(diceShopBtnObj.transform, false);
            RectTransform diceShopTextRt = diceShopTextObj.AddComponent<RectTransform>();
            diceShopTextRt.anchorMin = Vector2.zero;
            diceShopTextRt.anchorMax = Vector2.one;
            diceShopTextRt.offsetMin = new Vector2(10, 10);
            diceShopTextRt.offsetMax = new Vector2(-10, -10);

            TextMeshProUGUI diceShopText = diceShopTextObj.AddComponent<TextMeshProUGUI>();
            diceShopText.text = "DICE SHOP";
            diceShopText.fontSize = 32;
            diceShopText.fontStyle = FontStyles.Bold;
            diceShopText.alignment = TextAlignmentOptions.Center;
            diceShopText.color = Color.white;
            diceShopText.enableAutoSizing = true;
            diceShopText.fontSizeMin = 18;
            diceShopText.fontSizeMax = 36;

            // === SHOP PANEL ===
            GameObject panelObj = new GameObject("ShopPanel");
            panelObj.transform.SetParent(shopUIObj.transform, false);

            RectTransform panelRt = panelObj.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = new Vector2(20, 20);
            panelRt.offsetMax = new Vector2(-20, -20);

            Image panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.06f, 0.06f, 0.1f, 0.98f);
            panelBg.raycastTarget = true;

            CanvasGroup panelCG = panelObj.AddComponent<CanvasGroup>();

            // === HEADER ===
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(panelObj.transform, false);

            RectTransform headerRt = headerObj.AddComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 0.9f);
            headerRt.anchorMax = new Vector2(1, 1);
            headerRt.offsetMin = new Vector2(10, 5);
            headerRt.offsetMax = new Vector2(-10, -5);

            Image headerBg = headerObj.AddComponent<Image>();
            headerBg.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerObj.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 0);
            titleRt.anchorMax = new Vector2(0.35f, 1);
            titleRt.offsetMin = new Vector2(20, 5);
            titleRt.offsetMax = new Vector2(0, -5);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "DICE SHOP";
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = Color.white;

            // Money Display
            GameObject moneyObj = new GameObject("MoneyDisplay");
            moneyObj.transform.SetParent(headerObj.transform, false);
            RectTransform moneyRt = moneyObj.AddComponent<RectTransform>();
            moneyRt.anchorMin = new Vector2(0.35f, 0);
            moneyRt.anchorMax = new Vector2(0.8f, 1);
            moneyRt.offsetMin = new Vector2(5, 5);
            moneyRt.offsetMax = new Vector2(-5, -5);

            TextMeshProUGUI moneyText = moneyObj.AddComponent<TextMeshProUGUI>();
            moneyText.text = "$0";
            moneyText.fontSize = 28;
            moneyText.fontStyle = FontStyles.Bold;
            moneyText.alignment = TextAlignmentOptions.Center;
            moneyText.color = new Color(0.3f, 1f, 0.4f);

            // Close Button
            GameObject closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(headerObj.transform, false);
            RectTransform closeBtnRt = closeBtnObj.AddComponent<RectTransform>();
            closeBtnRt.anchorMin = new Vector2(0.88f, 0.1f);
            closeBtnRt.anchorMax = new Vector2(0.98f, 0.9f);
            closeBtnRt.offsetMin = Vector2.zero;
            closeBtnRt.offsetMax = Vector2.zero;

            Image closeBtnBg = closeBtnObj.AddComponent<Image>();
            closeBtnBg.color = new Color(0.9f, 0.25f, 0.25f, 0.95f);

            Button closeBtn = closeBtnObj.AddComponent<Button>();

            GameObject closeTextObj = new GameObject("Text");
            closeTextObj.transform.SetParent(closeBtnObj.transform, false);
            RectTransform closeTextRt = closeTextObj.AddComponent<RectTransform>();
            closeTextRt.anchorMin = Vector2.zero;
            closeTextRt.anchorMax = Vector2.one;
            closeTextRt.offsetMin = Vector2.zero;
            closeTextRt.offsetMax = Vector2.zero;

            TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.fontSize = 32;
            closeText.fontStyle = FontStyles.Bold;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.color = Color.white;

            // === CONTENT AREA ===
            GameObject contentAreaObj = new GameObject("ContentArea");
            contentAreaObj.transform.SetParent(panelObj.transform, false);

            RectTransform contentAreaRt = contentAreaObj.AddComponent<RectTransform>();
            contentAreaRt.anchorMin = new Vector2(0, 0);
            contentAreaRt.anchorMax = new Vector2(1, 0.9f);
            contentAreaRt.offsetMin = new Vector2(10, 10);
            contentAreaRt.offsetMax = new Vector2(-10, -5);

            Image contentAreaBg = contentAreaObj.AddComponent<Image>();
            contentAreaBg.color = new Color(0.04f, 0.04f, 0.07f, 0.95f);

            ScrollRect scrollRect = contentAreaObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            // Viewport
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(contentAreaObj.transform, false);
            RectTransform viewportRt = viewportObj.AddComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;

            Image viewportImg = viewportObj.AddComponent<Image>();
            viewportImg.color = Color.clear;
            Mask viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            // Content Container
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRt = contentObj.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.padding = new RectOffset(15, 15, 15, 15);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRt;
            scrollRect.content = contentRt;

            // Wire up DiceShopUI component
            SerializedObject so = new SerializedObject(shopUI);
            so.FindProperty("shopPanel").objectReferenceValue = panelObj;
            so.FindProperty("panelCanvasGroup").objectReferenceValue = panelCG;
            so.FindProperty("openButton").objectReferenceValue = diceShopBtn;
            so.FindProperty("openButtonText").objectReferenceValue = diceShopText;
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("contentContainer").objectReferenceValue = contentRt;
            so.FindProperty("moneyDisplayText").objectReferenceValue = moneyText;
            so.ApplyModifiedProperties();

            // Panel hidden, holder active
            panelObj.SetActive(false);
            shopUIObj.SetActive(true);

            // Move DiceShopButton BEFORE the UI panels in hierarchy so it renders behind them
            // Get current sibling indices
            int skillTreeUIIndex = -1;
            int diceShopUIIndex = -1;
            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                Transform child = canvas.transform.GetChild(i);
                if (child.name == "SkillTreeUI") skillTreeUIIndex = i;
                if (child.name == "DiceShopUI") diceShopUIIndex = i;
            }

            // Move DiceShopButton before SkillTreeUI (which should be before DiceShopUI)
            if (skillTreeUIIndex > 0)
            {
                diceShopBtnObj.transform.SetSiblingIndex(skillTreeUIIndex - 1);
            }

            EditorUtility.SetDirty(shopUI);
            Debug.Log("[FixUI] Dice Shop UI created!");
        }
    }
}
