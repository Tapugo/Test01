using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using Incredicer.Core;
using Incredicer.Skills;
using Incredicer.UI;

namespace Incredicer.Editor
{
    public static class SetupSkillTreeUI
    {
        private const string BUTTON_BLUE_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Button/Button01_Demo_Blue.png";
        private const string BUTTON_GREEN_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Button/Button01_Demo_Green.png";
        private const string BUTTON_RED_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Button/Button01_Demo_Red.png";
        private const string PANEL_BG_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Frame/Frame05.png";
        private const string NODE_BG_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Box/Box01_Demo_Brown.png";
        private const string GUI_FONT_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Fonts/TMP_TiltWarp.asset";

        // Branch colors
        private static readonly Color CORE_COLOR = new Color(0.9f, 0.85f, 0.4f);       // Gold
        private static readonly Color MONEY_ENGINE_COLOR = new Color(0.4f, 0.8f, 0.4f); // Green
        private static readonly Color AUTOMATION_COLOR = new Color(0.5f, 0.7f, 0.9f);   // Blue
        private static readonly Color DICE_EVOLUTION_COLOR = new Color(0.85f, 0.5f, 0.85f); // Purple
        private static readonly Color SKILLS_UTILITY_COLOR = new Color(0.9f, 0.5f, 0.4f); // Red-Orange

        [MenuItem("Incredicer/Setup Skill Tree UI")]
        public static void Execute()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[SetupSkillTreeUI] No canvas found!");
                return;
            }

            // Check if panel already exists
            GameObject existingPanel = GameObject.Find("SkillTreePanel");
            if (existingPanel != null)
            {
                Debug.Log("[SetupSkillTreeUI] SkillTreePanel already exists, removing old one...");
                Object.DestroyImmediate(existingPanel);
            }

            // Load assets
            Sprite panelBg = AssetDatabase.LoadAssetAtPath<Sprite>(PANEL_BG_PATH);
            Sprite nodeBg = AssetDatabase.LoadAssetAtPath<Sprite>(NODE_BG_PATH);
            Sprite buttonBlue = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_BLUE_PATH);
            Sprite buttonGreen = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_GREEN_PATH);
            Sprite buttonRed = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_RED_PATH);
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(GUI_FONT_PATH);

            // Load all skill nodes
            string[] guids = AssetDatabase.FindAssets("t:SkillNodeData", new[] { "Assets/Data/SkillNodes" });
            List<SkillNodeData> allNodes = new List<SkillNodeData>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SkillNodeData node = AssetDatabase.LoadAssetAtPath<SkillNodeData>(path);
                if (node != null)
                {
                    allNodes.Add(node);
                }
            }
            Debug.Log($"[SetupSkillTreeUI] Found {allNodes.Count} skill nodes");

            // Create main panel
            GameObject panelObj = new GameObject("SkillTreePanel");
            panelObj.transform.SetParent(canvas.transform);

            RectTransform panelRT = panelObj.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            panelRT.localScale = Vector3.one;

            // Add canvas group for fade animation
            CanvasGroup panelCanvasGroup = panelObj.AddComponent<CanvasGroup>();

            // Background overlay
            Image bgOverlay = panelObj.AddComponent<Image>();
            bgOverlay.color = new Color(0, 0, 0, 0.8f);
            bgOverlay.raycastTarget = true;

            // Create main content panel
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(panelObj.transform);

            RectTransform contentRT = contentObj.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.05f, 0.05f);
            contentRT.anchorMax = new Vector2(0.95f, 0.95f);
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;
            contentRT.localScale = Vector3.one;

            Image contentBg = contentObj.AddComponent<Image>();
            if (panelBg != null)
            {
                contentBg.sprite = panelBg;
                contentBg.type = Image.Type.Sliced;
            }
            contentBg.color = new Color(0.15f, 0.12f, 0.2f, 0.98f);

            // Create header
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(contentObj.transform);

            RectTransform headerRT = headerObj.AddComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = new Vector2(0, -10);
            headerRT.sizeDelta = new Vector2(-40, 80);
            headerRT.localScale = Vector3.one;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerObj.transform);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0);
            titleRT.anchorMax = new Vector2(0.4f, 1);
            titleRT.offsetMin = new Vector2(20, 0);
            titleRT.offsetMax = new Vector2(0, 0);
            titleRT.localScale = Vector3.one;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Skill Tree";
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = Color.white;
            if (font != null) titleText.font = font;

            // Dark Matter display
            GameObject dmObj = new GameObject("DarkMatterText");
            dmObj.transform.SetParent(headerObj.transform);

            RectTransform dmRT = dmObj.AddComponent<RectTransform>();
            dmRT.anchorMin = new Vector2(0.4f, 0);
            dmRT.anchorMax = new Vector2(0.7f, 1);
            dmRT.offsetMin = Vector2.zero;
            dmRT.offsetMax = Vector2.zero;
            dmRT.localScale = Vector3.one;

            TextMeshProUGUI dmText = dmObj.AddComponent<TextMeshProUGUI>();
            dmText.text = "DM: 0";
            dmText.fontSize = 32;
            dmText.alignment = TextAlignmentOptions.Center;
            dmText.color = new Color(0.8f, 0.5f, 1f);
            if (font != null) dmText.font = font;

            // Skill points display
            GameObject spObj = new GameObject("SkillPointsText");
            spObj.transform.SetParent(headerObj.transform);

            RectTransform spRT = spObj.AddComponent<RectTransform>();
            spRT.anchorMin = new Vector2(0.7f, 0);
            spRT.anchorMax = new Vector2(0.85f, 1);
            spRT.offsetMin = Vector2.zero;
            spRT.offsetMax = Vector2.zero;
            spRT.localScale = Vector3.one;

            TextMeshProUGUI spText = spObj.AddComponent<TextMeshProUGUI>();
            spText.text = "Skills: 0";
            spText.fontSize = 28;
            spText.alignment = TextAlignmentOptions.Center;
            spText.color = Color.white;
            if (font != null) spText.font = font;

            // Close button
            GameObject closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(headerObj.transform);

            RectTransform closeBtnRT = closeBtnObj.AddComponent<RectTransform>();
            closeBtnRT.anchorMin = new Vector2(1, 0.5f);
            closeBtnRT.anchorMax = new Vector2(1, 0.5f);
            closeBtnRT.pivot = new Vector2(1, 0.5f);
            closeBtnRT.anchoredPosition = new Vector2(-10, 0);
            closeBtnRT.sizeDelta = new Vector2(60, 60);
            closeBtnRT.localScale = Vector3.one;

            Image closeBtnImg = closeBtnObj.AddComponent<Image>();
            if (buttonRed != null)
            {
                closeBtnImg.sprite = buttonRed;
                closeBtnImg.type = Image.Type.Sliced;
            }
            closeBtnImg.color = Color.white;

            Button closeBtn = closeBtnObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeBtnImg;

            // Close button text
            GameObject closeBtnTextObj = new GameObject("Text");
            closeBtnTextObj.transform.SetParent(closeBtnObj.transform);

            RectTransform closeBtnTextRT = closeBtnTextObj.AddComponent<RectTransform>();
            closeBtnTextRT.anchorMin = Vector2.zero;
            closeBtnTextRT.anchorMax = Vector2.one;
            closeBtnTextRT.offsetMin = Vector2.zero;
            closeBtnTextRT.offsetMax = Vector2.zero;
            closeBtnTextRT.localScale = Vector3.one;

            TextMeshProUGUI closeBtnText = closeBtnTextObj.AddComponent<TextMeshProUGUI>();
            closeBtnText.text = "X";
            closeBtnText.fontSize = 32;
            closeBtnText.fontStyle = FontStyles.Bold;
            closeBtnText.alignment = TextAlignmentOptions.Center;
            closeBtnText.color = Color.white;
            if (font != null) closeBtnText.font = font;

            // Create scroll view for skill nodes
            GameObject scrollViewObj = new GameObject("NodeScrollView");
            scrollViewObj.transform.SetParent(contentObj.transform);

            RectTransform scrollViewRT = scrollViewObj.AddComponent<RectTransform>();
            scrollViewRT.anchorMin = new Vector2(0, 0.15f);
            scrollViewRT.anchorMax = new Vector2(0.7f, 0.88f);
            scrollViewRT.offsetMin = new Vector2(20, 0);
            scrollViewRT.offsetMax = new Vector2(0, 0);
            scrollViewRT.localScale = Vector3.one;

            ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 30f;

            Image scrollBg = scrollViewObj.AddComponent<Image>();
            scrollBg.color = new Color(0.1f, 0.08f, 0.15f, 0.5f);

            // Viewport
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollViewObj.transform);

            RectTransform viewportRT = viewportObj.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = Vector2.zero;
            viewportRT.offsetMax = Vector2.zero;
            viewportRT.localScale = Vector3.one;

            Image viewportMask = viewportObj.AddComponent<Image>();
            viewportMask.color = Color.white;
            Mask mask = viewportObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            scrollRect.viewport = viewportRT;

            // Node container
            GameObject nodeContainerObj = new GameObject("NodeContainer");
            nodeContainerObj.transform.SetParent(viewportObj.transform);

            RectTransform nodeContainerRT = nodeContainerObj.AddComponent<RectTransform>();
            nodeContainerRT.anchorMin = new Vector2(0.5f, 0.5f);
            nodeContainerRT.anchorMax = new Vector2(0.5f, 0.5f);
            nodeContainerRT.pivot = new Vector2(0.5f, 0.5f);
            nodeContainerRT.sizeDelta = new Vector2(1400, 1000);
            nodeContainerRT.anchoredPosition = Vector2.zero;
            nodeContainerRT.localScale = Vector3.one;

            scrollRect.content = nodeContainerRT;

            // Create skill node buttons
            foreach (var node in allNodes)
            {
                CreateSkillNodeButton(node, nodeContainerRT, nodeBg, font);
            }

            // Create info panel on the right
            GameObject infoPanelObj = new GameObject("NodeInfoPanel");
            infoPanelObj.transform.SetParent(contentObj.transform);

            RectTransform infoPanelRT = infoPanelObj.AddComponent<RectTransform>();
            infoPanelRT.anchorMin = new Vector2(0.72f, 0.15f);
            infoPanelRT.anchorMax = new Vector2(0.98f, 0.88f);
            infoPanelRT.offsetMin = Vector2.zero;
            infoPanelRT.offsetMax = Vector2.zero;
            infoPanelRT.localScale = Vector3.one;

            Image infoPanelBg = infoPanelObj.AddComponent<Image>();
            if (panelBg != null)
            {
                infoPanelBg.sprite = panelBg;
                infoPanelBg.type = Image.Type.Sliced;
            }
            infoPanelBg.color = new Color(0.2f, 0.18f, 0.25f, 0.95f);

            // Node name text
            GameObject nodeNameObj = new GameObject("NodeNameText");
            nodeNameObj.transform.SetParent(infoPanelObj.transform);

            RectTransform nodeNameRT = nodeNameObj.AddComponent<RectTransform>();
            nodeNameRT.anchorMin = new Vector2(0, 0.85f);
            nodeNameRT.anchorMax = new Vector2(1, 0.98f);
            nodeNameRT.offsetMin = new Vector2(15, 0);
            nodeNameRT.offsetMax = new Vector2(-15, 0);
            nodeNameRT.localScale = Vector3.one;

            TextMeshProUGUI nodeNameText = nodeNameObj.AddComponent<TextMeshProUGUI>();
            nodeNameText.text = "Select a skill";
            nodeNameText.fontSize = 28;
            nodeNameText.fontStyle = FontStyles.Bold;
            nodeNameText.alignment = TextAlignmentOptions.Center;
            nodeNameText.color = Color.white;
            if (font != null) nodeNameText.font = font;

            // Node description text
            GameObject nodeDescObj = new GameObject("NodeDescriptionText");
            nodeDescObj.transform.SetParent(infoPanelObj.transform);

            RectTransform nodeDescRT = nodeDescObj.AddComponent<RectTransform>();
            nodeDescRT.anchorMin = new Vector2(0, 0.4f);
            nodeDescRT.anchorMax = new Vector2(1, 0.82f);
            nodeDescRT.offsetMin = new Vector2(15, 0);
            nodeDescRT.offsetMax = new Vector2(-15, 0);
            nodeDescRT.localScale = Vector3.one;

            TextMeshProUGUI nodeDescText = nodeDescObj.AddComponent<TextMeshProUGUI>();
            nodeDescText.text = "";
            nodeDescText.fontSize = 22;
            nodeDescText.alignment = TextAlignmentOptions.TopLeft;
            nodeDescText.color = new Color(0.85f, 0.85f, 0.85f);
            nodeDescText.enableWordWrapping = true;
            if (font != null) nodeDescText.font = font;

            // Node cost text
            GameObject nodeCostObj = new GameObject("NodeCostText");
            nodeCostObj.transform.SetParent(infoPanelObj.transform);

            RectTransform nodeCostRT = nodeCostObj.AddComponent<RectTransform>();
            nodeCostRT.anchorMin = new Vector2(0, 0.28f);
            nodeCostRT.anchorMax = new Vector2(1, 0.38f);
            nodeCostRT.offsetMin = new Vector2(15, 0);
            nodeCostRT.offsetMax = new Vector2(-15, 0);
            nodeCostRT.localScale = Vector3.one;

            TextMeshProUGUI nodeCostText = nodeCostObj.AddComponent<TextMeshProUGUI>();
            nodeCostText.text = "";
            nodeCostText.fontSize = 24;
            nodeCostText.alignment = TextAlignmentOptions.Center;
            nodeCostText.color = new Color(0.8f, 0.5f, 1f);
            if (font != null) nodeCostText.font = font;

            // Purchase button
            GameObject purchaseBtnObj = new GameObject("PurchaseButton");
            purchaseBtnObj.transform.SetParent(infoPanelObj.transform);

            RectTransform purchaseBtnRT = purchaseBtnObj.AddComponent<RectTransform>();
            purchaseBtnRT.anchorMin = new Vector2(0.1f, 0.05f);
            purchaseBtnRT.anchorMax = new Vector2(0.9f, 0.22f);
            purchaseBtnRT.offsetMin = Vector2.zero;
            purchaseBtnRT.offsetMax = Vector2.zero;
            purchaseBtnRT.localScale = Vector3.one;

            Image purchaseBtnImg = purchaseBtnObj.AddComponent<Image>();
            if (buttonGreen != null)
            {
                purchaseBtnImg.sprite = buttonGreen;
                purchaseBtnImg.type = Image.Type.Sliced;
            }
            purchaseBtnImg.color = Color.white;

            Button purchaseBtn = purchaseBtnObj.AddComponent<Button>();
            purchaseBtn.targetGraphic = purchaseBtnImg;
            ColorBlock purchaseColors = purchaseBtn.colors;
            purchaseColors.normalColor = Color.white;
            purchaseColors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            purchaseColors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            purchaseColors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
            purchaseBtn.colors = purchaseColors;

            // Purchase button text
            GameObject purchaseBtnTextObj = new GameObject("Text");
            purchaseBtnTextObj.transform.SetParent(purchaseBtnObj.transform);

            RectTransform purchaseBtnTextRT = purchaseBtnTextObj.AddComponent<RectTransform>();
            purchaseBtnTextRT.anchorMin = Vector2.zero;
            purchaseBtnTextRT.anchorMax = Vector2.one;
            purchaseBtnTextRT.offsetMin = Vector2.zero;
            purchaseBtnTextRT.offsetMax = Vector2.zero;
            purchaseBtnTextRT.localScale = Vector3.one;

            TextMeshProUGUI purchaseBtnText = purchaseBtnTextObj.AddComponent<TextMeshProUGUI>();
            purchaseBtnText.text = "Purchase";
            purchaseBtnText.fontSize = 28;
            purchaseBtnText.fontStyle = FontStyles.Bold;
            purchaseBtnText.alignment = TextAlignmentOptions.Center;
            purchaseBtnText.color = Color.white;
            if (font != null) purchaseBtnText.font = font;

            // Add SkillTreeUI component
            SkillTreeUI skillTreeUI = panelObj.AddComponent<SkillTreeUI>();

            // Wire up references via SerializedObject
            SerializedObject so = new SerializedObject(skillTreeUI);
            so.FindProperty("skillTreePanel").objectReferenceValue = panelObj;
            so.FindProperty("panelCanvasGroup").objectReferenceValue = panelCanvasGroup;
            so.FindProperty("darkMatterText").objectReferenceValue = dmText;
            so.FindProperty("skillPointsText").objectReferenceValue = spText;
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("nodeContainer").objectReferenceValue = nodeContainerRT.transform;
            so.FindProperty("nodeInfoPanel").objectReferenceValue = infoPanelObj;
            so.FindProperty("nodeNameText").objectReferenceValue = nodeNameText;
            so.FindProperty("nodeDescriptionText").objectReferenceValue = nodeDescText;
            so.FindProperty("nodeCostText").objectReferenceValue = nodeCostText;
            so.FindProperty("purchaseButton").objectReferenceValue = purchaseBtn;
            so.FindProperty("purchaseButtonText").objectReferenceValue = purchaseBtnText;
            so.ApplyModifiedProperties();

            // Wire GameUI Skills button to open skill tree
            GameUI gameUI = canvas.GetComponent<GameUI>();
            if (gameUI != null)
            {
                SerializedObject gameUISO = new SerializedObject(gameUI);
                var skillTreeUIField = gameUISO.FindProperty("skillTreeUI");
                if (skillTreeUIField != null)
                {
                    skillTreeUIField.objectReferenceValue = skillTreeUI;
                    gameUISO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(gameUI);
                    Debug.Log("[SetupSkillTreeUI] Wired SkillTreeUI to GameUI");
                }
            }

            EditorUtility.SetDirty(panelObj);
            Debug.Log($"[SetupSkillTreeUI] Created SkillTreePanel with {allNodes.Count} nodes");
        }

        private static void CreateSkillNodeButton(SkillNodeData nodeData, RectTransform container, Sprite nodeBg, TMP_FontAsset font)
        {
            if (nodeData == null) return;

            GameObject nodeObj = new GameObject($"Node_{nodeData.nodeId}");
            nodeObj.transform.SetParent(container);

            RectTransform nodeRT = nodeObj.AddComponent<RectTransform>();
            // Position from node's treePosition
            nodeRT.anchoredPosition = new Vector2(
                nodeData.treePosition.x * 140f,
                nodeData.treePosition.y * -120f
            );
            nodeRT.sizeDelta = new Vector2(100, 100);
            nodeRT.localScale = Vector3.one;

            // Background image
            Image bgImg = nodeObj.AddComponent<Image>();
            if (nodeBg != null)
            {
                bgImg.sprite = nodeBg;
                bgImg.type = Image.Type.Sliced;
            }
            bgImg.color = GetBranchColor(nodeData.branch);

            // Button
            Button btn = nodeObj.AddComponent<Button>();
            btn.targetGraphic = bgImg;

            // Icon (if available)
            if (nodeData.icon != null)
            {
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(nodeObj.transform);

                RectTransform iconRT = iconObj.AddComponent<RectTransform>();
                iconRT.anchorMin = new Vector2(0.15f, 0.25f);
                iconRT.anchorMax = new Vector2(0.85f, 0.9f);
                iconRT.offsetMin = Vector2.zero;
                iconRT.offsetMax = Vector2.zero;
                iconRT.localScale = Vector3.one;

                Image iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = nodeData.icon;
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
            }

            // Short name text
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(nodeObj.transform);

            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0);
            nameRT.anchorMax = new Vector2(1, 0.3f);
            nameRT.offsetMin = new Vector2(2, 2);
            nameRT.offsetMax = new Vector2(-2, 0);
            nameRT.localScale = Vector3.one;

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            // Get short name (first word or abbreviation)
            string shortName = GetShortName(nodeData.displayName);
            nameText.text = shortName;
            nameText.fontSize = 14;
            nameText.alignment = TextAlignmentOptions.Bottom;
            nameText.color = Color.white;
            nameText.raycastTarget = false;
            nameText.enableWordWrapping = false;
            nameText.overflowMode = TextOverflowModes.Truncate;
            if (font != null) nameText.font = font;

            // Locked overlay
            GameObject lockedObj = new GameObject("LockedOverlay");
            lockedObj.transform.SetParent(nodeObj.transform);

            RectTransform lockedRT = lockedObj.AddComponent<RectTransform>();
            lockedRT.anchorMin = Vector2.zero;
            lockedRT.anchorMax = Vector2.one;
            lockedRT.offsetMin = Vector2.zero;
            lockedRT.offsetMax = Vector2.zero;
            lockedRT.localScale = Vector3.one;

            Image lockedImg = lockedObj.AddComponent<Image>();
            lockedImg.color = new Color(0, 0, 0, 0.6f);
            lockedImg.raycastTarget = false;

            // Lock icon text
            GameObject lockIconObj = new GameObject("LockIcon");
            lockIconObj.transform.SetParent(lockedObj.transform);

            RectTransform lockIconRT = lockIconObj.AddComponent<RectTransform>();
            lockIconRT.anchorMin = new Vector2(0.25f, 0.25f);
            lockIconRT.anchorMax = new Vector2(0.75f, 0.75f);
            lockIconRT.offsetMin = Vector2.zero;
            lockIconRT.offsetMax = Vector2.zero;
            lockIconRT.localScale = Vector3.one;

            TextMeshProUGUI lockText = lockIconObj.AddComponent<TextMeshProUGUI>();
            lockText.text = "X";
            lockText.fontSize = 40;
            lockText.fontStyle = FontStyles.Bold;
            lockText.alignment = TextAlignmentOptions.Center;
            lockText.color = new Color(1f, 0.3f, 0.3f, 0.8f);
            lockText.raycastTarget = false;
            if (font != null) lockText.font = font;

            // Add SkillNodeButton component
            SkillNodeButton nodeButton = nodeObj.AddComponent<SkillNodeButton>();

            // Wire up references
            SerializedObject so = new SerializedObject(nodeButton);
            so.FindProperty("nodeData").objectReferenceValue = nodeData;
            so.FindProperty("button").objectReferenceValue = btn;
            so.FindProperty("backgroundImage").objectReferenceValue = bgImg;
            so.FindProperty("lockedOverlay").objectReferenceValue = lockedObj;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(nodeObj);
        }

        private static Color GetBranchColor(SkillBranch branch)
        {
            switch (branch)
            {
                case SkillBranch.Core: return CORE_COLOR;
                case SkillBranch.MoneyEngine: return MONEY_ENGINE_COLOR;
                case SkillBranch.Automation: return AUTOMATION_COLOR;
                case SkillBranch.DiceEvolution: return DICE_EVOLUTION_COLOR;
                case SkillBranch.SkillsUtility: return SKILLS_UTILITY_COLOR;
                default: return Color.gray;
            }
        }

        private static string GetShortName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "";

            // Just take the first word or up to 10 chars
            string[] words = fullName.Split(' ');
            string firstWord = words[0];

            if (firstWord.Length > 10)
                return firstWord.Substring(0, 8) + "..";

            return firstWord;
        }
    }
}
