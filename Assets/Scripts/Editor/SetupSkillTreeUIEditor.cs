using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Incredicer.UI;

namespace Incredicer.Editor
{
    public static class SetupSkillTreeUIEditor
    {
        [MenuItem("Incredicer/Setup Skill Tree UI")]
        public static void Execute()
        {
            // Find Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[SetupSkillTreeUI] Canvas not found in scene!");
                return;
            }

            // Find or create SkillTreeUI
            SkillTreeUI existingUI = Object.FindObjectOfType<SkillTreeUI>();
            GameObject skillTreeUIObj;

            if (existingUI != null)
            {
                skillTreeUIObj = existingUI.gameObject;
                Debug.Log("[SetupSkillTreeUI] Found existing SkillTreeUI");
            }
            else
            {
                skillTreeUIObj = new GameObject("SkillTreeUI");
                skillTreeUIObj.transform.SetParent(canvas.transform, false);
                existingUI = skillTreeUIObj.AddComponent<SkillTreeUI>();
                Debug.Log("[SetupSkillTreeUI] Created new SkillTreeUI");
            }

            SerializedObject uiSO = new SerializedObject(existingUI);

            // Create or find SkillTreePanel
            Transform existingPanel = skillTreeUIObj.transform.Find("SkillTreePanel");
            GameObject panelObj;

            if (existingPanel != null)
            {
                panelObj = existingPanel.gameObject;
            }
            else
            {
                panelObj = CreateSkillTreePanel(skillTreeUIObj.transform);
            }

            // Get references
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            CanvasGroup panelCG = panelObj.GetComponent<CanvasGroup>();
            if (panelCG == null) panelCG = panelObj.AddComponent<CanvasGroup>();

            // Find child elements
            Transform headerTransform = panelObj.transform.Find("Header");
            Transform treeAreaTransform = panelObj.transform.Find("TreeArea");
            Transform infoAreaTransform = panelObj.transform.Find("InfoArea");

            TextMeshProUGUI darkMatterText = headerTransform?.Find("DarkMatterText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI skillPointsText = headerTransform?.Find("SkillPointsText")?.GetComponent<TextMeshProUGUI>();
            Button closeButton = headerTransform?.Find("CloseButton")?.GetComponent<Button>();

            ScrollRect scrollRect = treeAreaTransform?.Find("ScrollView")?.GetComponent<ScrollRect>();
            RectTransform nodeContainer = scrollRect?.content?.Find("NodeContainer") as RectTransform;
            RectTransform connectionContainer = scrollRect?.content?.Find("ConnectionContainer") as RectTransform;

            GameObject nodeInfoPanel = infoAreaTransform?.Find("NodeInfoPanel")?.gameObject;
            TextMeshProUGUI nodeNameText = nodeInfoPanel?.transform.Find("NodeName")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nodeDescriptionText = nodeInfoPanel?.transform.Find("NodeDescription")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nodeCostText = nodeInfoPanel?.transform.Find("NodeCost")?.GetComponent<TextMeshProUGUI>();
            Button purchaseButton = nodeInfoPanel?.transform.Find("PurchaseButton")?.GetComponent<Button>();
            TextMeshProUGUI purchaseButtonText = purchaseButton?.GetComponentInChildren<TextMeshProUGUI>();

            // Assign references
            uiSO.FindProperty("skillTreePanel").objectReferenceValue = panelObj;
            uiSO.FindProperty("panelCanvasGroup").objectReferenceValue = panelCG;
            uiSO.FindProperty("panelRect").objectReferenceValue = panelRect;
            uiSO.FindProperty("darkMatterText").objectReferenceValue = darkMatterText;
            uiSO.FindProperty("skillPointsText").objectReferenceValue = skillPointsText;
            uiSO.FindProperty("closeButton").objectReferenceValue = closeButton;
            uiSO.FindProperty("nodeContainer").objectReferenceValue = nodeContainer;
            uiSO.FindProperty("connectionContainer").objectReferenceValue = connectionContainer;
            uiSO.FindProperty("scrollRect").objectReferenceValue = scrollRect;
            uiSO.FindProperty("nodeInfoPanel").objectReferenceValue = nodeInfoPanel;
            uiSO.FindProperty("nodeNameText").objectReferenceValue = nodeNameText;
            uiSO.FindProperty("nodeDescriptionText").objectReferenceValue = nodeDescriptionText;
            uiSO.FindProperty("nodeCostText").objectReferenceValue = nodeCostText;
            uiSO.FindProperty("purchaseButton").objectReferenceValue = purchaseButton;
            uiSO.FindProperty("purchaseButtonText").objectReferenceValue = purchaseButtonText;

            uiSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(existingUI);

            // Start hidden
            panelObj.SetActive(false);

            Debug.Log("[SetupSkillTreeUI] Setup complete!");
            Selection.activeGameObject = skillTreeUIObj;
        }

        private static GameObject CreateSkillTreePanel(Transform parent)
        {
            // Main Panel
            GameObject panelObj = new GameObject("SkillTreePanel");
            panelObj.transform.SetParent(parent, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = new Vector2(20, 20);
            panelRect.offsetMax = new Vector2(-20, -20);

            Image panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);

            CanvasGroup cg = panelObj.AddComponent<CanvasGroup>();

            // ===== HEADER =====
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(panelObj.transform, false);

            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0, 60);

            Image headerBg = headerObj.AddComponent<Image>();
            headerBg.color = new Color(0.12f, 0.12f, 0.18f, 1f);

            // Dark Matter Text
            GameObject dmTextObj = new GameObject("DarkMatterText");
            dmTextObj.transform.SetParent(headerObj.transform, false);
            RectTransform dmRect = dmTextObj.AddComponent<RectTransform>();
            dmRect.anchorMin = new Vector2(0, 0);
            dmRect.anchorMax = new Vector2(0.4f, 1);
            dmRect.offsetMin = new Vector2(20, 10);
            dmRect.offsetMax = new Vector2(0, -10);

            TextMeshProUGUI dmText = dmTextObj.AddComponent<TextMeshProUGUI>();
            dmText.text = "Dark Matter: 0";
            dmText.fontSize = 24;
            dmText.fontStyle = FontStyles.Bold;
            dmText.alignment = TextAlignmentOptions.MidlineLeft;
            dmText.color = new Color(0.8f, 0.5f, 1f);

            // Skill Points Text
            GameObject spTextObj = new GameObject("SkillPointsText");
            spTextObj.transform.SetParent(headerObj.transform, false);
            RectTransform spRect = spTextObj.AddComponent<RectTransform>();
            spRect.anchorMin = new Vector2(0.4f, 0);
            spRect.anchorMax = new Vector2(0.7f, 1);
            spRect.offsetMin = new Vector2(10, 10);
            spRect.offsetMax = new Vector2(-10, -10);

            TextMeshProUGUI spText = spTextObj.AddComponent<TextMeshProUGUI>();
            spText.text = "Skills: 0";
            spText.fontSize = 20;
            spText.alignment = TextAlignmentOptions.Center;
            spText.color = Color.white;

            // Close Button
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(headerObj.transform, false);
            RectTransform closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 0.5f);
            closeRect.anchorMax = new Vector2(1, 0.5f);
            closeRect.pivot = new Vector2(1, 0.5f);
            closeRect.anchoredPosition = new Vector2(-15, 0);
            closeRect.sizeDelta = new Vector2(40, 40);

            Image closeBg = closeObj.AddComponent<Image>();
            closeBg.color = new Color(0.8f, 0.3f, 0.3f, 1f);

            Button closeBtn = closeObj.AddComponent<Button>();

            GameObject closeTextObj = new GameObject("Text");
            closeTextObj.transform.SetParent(closeObj.transform, false);
            RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.fontSize = 24;
            closeText.fontStyle = FontStyles.Bold;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.color = Color.white;

            // ===== TREE AREA (with scroll) =====
            GameObject treeAreaObj = new GameObject("TreeArea");
            treeAreaObj.transform.SetParent(panelObj.transform, false);

            RectTransform treeAreaRect = treeAreaObj.AddComponent<RectTransform>();
            treeAreaRect.anchorMin = new Vector2(0, 0.25f);
            treeAreaRect.anchorMax = new Vector2(1, 1);
            treeAreaRect.offsetMin = new Vector2(10, 10);
            treeAreaRect.offsetMax = new Vector2(-10, -70);

            // Scroll View
            GameObject scrollViewObj = new GameObject("ScrollView");
            scrollViewObj.transform.SetParent(treeAreaObj.transform, false);

            RectTransform scrollViewRect = scrollViewObj.AddComponent<RectTransform>();
            scrollViewRect.anchorMin = Vector2.zero;
            scrollViewRect.anchorMax = Vector2.one;
            scrollViewRect.offsetMin = Vector2.zero;
            scrollViewRect.offsetMax = Vector2.zero;

            ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 20f;

            Image scrollBg = scrollViewObj.AddComponent<Image>();
            scrollBg.color = new Color(0.05f, 0.05f, 0.08f, 1f);

            // Viewport
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollViewObj.transform, false);

            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            Image viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = Color.clear;
            Mask viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            // Content (large area for nodes)
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);

            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(2000, 1500); // Large area for skill tree

            // Connection Container (below nodes)
            GameObject connectionContainerObj = new GameObject("ConnectionContainer");
            connectionContainerObj.transform.SetParent(contentObj.transform, false);

            RectTransform connectionRect = connectionContainerObj.AddComponent<RectTransform>();
            connectionRect.anchorMin = Vector2.zero;
            connectionRect.anchorMax = Vector2.one;
            connectionRect.offsetMin = Vector2.zero;
            connectionRect.offsetMax = Vector2.zero;

            // Node Container (above connections)
            GameObject nodeContainerObj = new GameObject("NodeContainer");
            nodeContainerObj.transform.SetParent(contentObj.transform, false);

            RectTransform nodeRect = nodeContainerObj.AddComponent<RectTransform>();
            nodeRect.anchorMin = Vector2.zero;
            nodeRect.anchorMax = Vector2.one;
            nodeRect.offsetMin = Vector2.zero;
            nodeRect.offsetMax = Vector2.zero;

            // Setup scroll rect references
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;

            // ===== INFO AREA (below tree) =====
            GameObject infoAreaObj = new GameObject("InfoArea");
            infoAreaObj.transform.SetParent(panelObj.transform, false);

            RectTransform infoAreaRect = infoAreaObj.AddComponent<RectTransform>();
            infoAreaRect.anchorMin = new Vector2(0, 0);
            infoAreaRect.anchorMax = new Vector2(1, 0.25f);
            infoAreaRect.offsetMin = new Vector2(10, 10);
            infoAreaRect.offsetMax = new Vector2(-10, -5);

            Image infoAreaBg = infoAreaObj.AddComponent<Image>();
            infoAreaBg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

            // Node Info Panel
            GameObject nodeInfoPanel = new GameObject("NodeInfoPanel");
            nodeInfoPanel.transform.SetParent(infoAreaObj.transform, false);

            RectTransform nodeInfoRect = nodeInfoPanel.AddComponent<RectTransform>();
            nodeInfoRect.anchorMin = Vector2.zero;
            nodeInfoRect.anchorMax = Vector2.one;
            nodeInfoRect.offsetMin = new Vector2(15, 10);
            nodeInfoRect.offsetMax = new Vector2(-15, -10);

            // Node Name
            GameObject nodeNameObj = new GameObject("NodeName");
            nodeNameObj.transform.SetParent(nodeInfoPanel.transform, false);

            RectTransform nodeNameRect = nodeNameObj.AddComponent<RectTransform>();
            nodeNameRect.anchorMin = new Vector2(0, 0.7f);
            nodeNameRect.anchorMax = new Vector2(0.6f, 1);
            nodeNameRect.offsetMin = Vector2.zero;
            nodeNameRect.offsetMax = Vector2.zero;

            TextMeshProUGUI nodeNameText = nodeNameObj.AddComponent<TextMeshProUGUI>();
            nodeNameText.text = "Select a Skill";
            nodeNameText.fontSize = 22;
            nodeNameText.fontStyle = FontStyles.Bold;
            nodeNameText.alignment = TextAlignmentOptions.TopLeft;
            nodeNameText.color = Color.white;

            // Node Description
            GameObject nodeDescObj = new GameObject("NodeDescription");
            nodeDescObj.transform.SetParent(nodeInfoPanel.transform, false);

            RectTransform nodeDescRect = nodeDescObj.AddComponent<RectTransform>();
            nodeDescRect.anchorMin = new Vector2(0, 0.2f);
            nodeDescRect.anchorMax = new Vector2(0.65f, 0.7f);
            nodeDescRect.offsetMin = Vector2.zero;
            nodeDescRect.offsetMax = Vector2.zero;

            TextMeshProUGUI nodeDescText = nodeDescObj.AddComponent<TextMeshProUGUI>();
            nodeDescText.text = "Click on a skill node to see details.";
            nodeDescText.fontSize = 16;
            nodeDescText.alignment = TextAlignmentOptions.TopLeft;
            nodeDescText.color = new Color(0.8f, 0.8f, 0.8f);

            // Node Cost
            GameObject nodeCostObj = new GameObject("NodeCost");
            nodeCostObj.transform.SetParent(nodeInfoPanel.transform, false);

            RectTransform nodeCostRect = nodeCostObj.AddComponent<RectTransform>();
            nodeCostRect.anchorMin = new Vector2(0, 0);
            nodeCostRect.anchorMax = new Vector2(0.4f, 0.25f);
            nodeCostRect.offsetMin = Vector2.zero;
            nodeCostRect.offsetMax = Vector2.zero;

            TextMeshProUGUI nodeCostText = nodeCostObj.AddComponent<TextMeshProUGUI>();
            nodeCostText.text = "";
            nodeCostText.fontSize = 18;
            nodeCostText.fontStyle = FontStyles.Bold;
            nodeCostText.alignment = TextAlignmentOptions.BottomLeft;
            nodeCostText.color = new Color(0.8f, 0.5f, 1f);

            // Purchase Button
            GameObject purchaseBtnObj = new GameObject("PurchaseButton");
            purchaseBtnObj.transform.SetParent(nodeInfoPanel.transform, false);

            RectTransform purchaseBtnRect = purchaseBtnObj.AddComponent<RectTransform>();
            purchaseBtnRect.anchorMin = new Vector2(0.7f, 0.1f);
            purchaseBtnRect.anchorMax = new Vector2(1, 0.9f);
            purchaseBtnRect.offsetMin = Vector2.zero;
            purchaseBtnRect.offsetMax = Vector2.zero;

            Image purchaseBtnBg = purchaseBtnObj.AddComponent<Image>();
            purchaseBtnBg.color = new Color(0.3f, 0.7f, 0.4f, 1f);

            Button purchaseBtn = purchaseBtnObj.AddComponent<Button>();
            ColorBlock colors = purchaseBtn.colors;
            colors.normalColor = new Color(0.3f, 0.7f, 0.4f, 1f);
            colors.highlightedColor = new Color(0.4f, 0.8f, 0.5f, 1f);
            colors.pressedColor = new Color(0.2f, 0.6f, 0.3f, 1f);
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            purchaseBtn.colors = colors;

            GameObject purchaseBtnTextObj = new GameObject("Text");
            purchaseBtnTextObj.transform.SetParent(purchaseBtnObj.transform, false);

            RectTransform purchaseBtnTextRect = purchaseBtnTextObj.AddComponent<RectTransform>();
            purchaseBtnTextRect.anchorMin = Vector2.zero;
            purchaseBtnTextRect.anchorMax = Vector2.one;
            purchaseBtnTextRect.offsetMin = Vector2.zero;
            purchaseBtnTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI purchaseBtnText = purchaseBtnTextObj.AddComponent<TextMeshProUGUI>();
            purchaseBtnText.text = "Purchase";
            purchaseBtnText.fontSize = 20;
            purchaseBtnText.fontStyle = FontStyles.Bold;
            purchaseBtnText.alignment = TextAlignmentOptions.Center;
            purchaseBtnText.color = Color.white;

            return panelObj;
        }
    }
}
