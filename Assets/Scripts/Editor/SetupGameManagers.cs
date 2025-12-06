using UnityEngine;
using UnityEditor;
using Incredicer.Core;
using Incredicer.Dice;
using Incredicer.Skills;
using Incredicer.UI;

namespace Incredicer.Editor
{
    public static class SetupGameManagers
    {
        [MenuItem("Incredicer/Setup Game Managers")]
        public static void Execute()
        {
            // Create or find GameManagers root
            GameObject managersRoot = GameObject.Find("GameManagers");
            if (managersRoot == null)
            {
                managersRoot = new GameObject("GameManagers");
            }

            // Add CurrencyManager
            if (Object.FindObjectOfType<CurrencyManager>() == null)
            {
                GameObject currencyMgr = new GameObject("CurrencyManager");
                currencyMgr.transform.SetParent(managersRoot.transform);
                currencyMgr.AddComponent<CurrencyManager>();
                Debug.Log("[Setup] Created CurrencyManager");
            }

            // Add GameStats
            if (Object.FindObjectOfType<GameStats>() == null)
            {
                GameObject statsMgr = new GameObject("GameStats");
                statsMgr.transform.SetParent(managersRoot.transform);
                statsMgr.AddComponent<GameStats>();
                Debug.Log("[Setup] Created GameStats");
            }

            // Add DiceManager
            if (Object.FindObjectOfType<DiceManager>() == null)
            {
                GameObject diceMgr = new GameObject("DiceManager");
                diceMgr.transform.SetParent(managersRoot.transform);
                diceMgr.AddComponent<DiceManager>();
                Debug.Log("[Setup] Created DiceManager");
            }

            // Add DiceRollerController
            if (Object.FindObjectOfType<DiceRollerController>() == null)
            {
                GameObject rollerCtrl = new GameObject("DiceRollerController");
                rollerCtrl.transform.SetParent(managersRoot.transform);
                rollerCtrl.AddComponent<DiceRollerController>();
                Debug.Log("[Setup] Created DiceRollerController");
            }

            // Add SkillTreeManager
            if (Object.FindObjectOfType<SkillTreeManager>() == null)
            {
                GameObject skillMgr = new GameObject("SkillTreeManager");
                skillMgr.transform.SetParent(managersRoot.transform);
                skillMgr.AddComponent<SkillTreeManager>();
                Debug.Log("[Setup] Created SkillTreeManager");
            }

            // Add PrestigeManager
            if (Object.FindObjectOfType<PrestigeManager>() == null)
            {
                GameObject prestigeMgr = new GameObject("PrestigeManager");
                prestigeMgr.transform.SetParent(managersRoot.transform);
                prestigeMgr.AddComponent<PrestigeManager>();
                Debug.Log("[Setup] Created PrestigeManager");
            }

            // Add SaveSystem
            if (Object.FindObjectOfType<SaveSystem>() == null)
            {
                GameObject saveSys = new GameObject("SaveSystem");
                saveSys.transform.SetParent(managersRoot.transform);
                saveSys.AddComponent<SaveSystem>();
                Debug.Log("[Setup] Created SaveSystem");
            }

            // Add FlyingJackpotManager
            if (Object.FindObjectOfType<FlyingJackpotManager>() == null)
            {
                GameObject jackpotMgr = new GameObject("FlyingJackpotManager");
                jackpotMgr.transform.SetParent(managersRoot.transform);
                jackpotMgr.AddComponent<FlyingJackpotManager>();
                Debug.Log("[Setup] Created FlyingJackpotManager");
            }

            // Add HapticManager
            if (Object.FindObjectOfType<HapticManager>() == null)
            {
                GameObject hapticMgr = new GameObject("HapticManager");
                hapticMgr.transform.SetParent(managersRoot.transform);
                hapticMgr.AddComponent<HapticManager>();
                Debug.Log("[Setup] Created HapticManager");
            }

            // Add BlackHoleManager
            if (Object.FindObjectOfType<BlackHoleManager>() == null)
            {
                GameObject blackHoleMgr = new GameObject("BlackHoleManager");
                blackHoleMgr.transform.SetParent(managersRoot.transform);
                blackHoleMgr.AddComponent<BlackHoleManager>();
                Debug.Log("[Setup] Created BlackHoleManager");
            }

            // Add PopupManager
            if (Object.FindObjectOfType<PopupManager>() == null)
            {
                GameObject popupMgr = new GameObject("PopupManager");
                popupMgr.transform.SetParent(managersRoot.transform);
                popupMgr.AddComponent<PopupManager>();
                Debug.Log("[Setup] Created PopupManager");
            }

            Debug.Log("[Setup] All game managers configured!");
        }

        [MenuItem("Incredicer/Create Skill Tree UI")]
        public static void CreateSkillTreeUI()
        {
            // Find or create canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[Setup] No Canvas found! Run 'Create Game UI' first.");
                return;
            }

            // Check if SkillTreeUI already exists
            if (Object.FindObjectOfType<SkillTreeUI>() != null)
            {
                Debug.Log("[Setup] SkillTreeUI already exists");
                return;
            }

            // Create SkillTree panel
            GameObject skillTreePanel = new GameObject("SkillTreePanel");
            skillTreePanel.transform.SetParent(canvas.transform);

            RectTransform panelRT = skillTreePanel.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            // Add background
            UnityEngine.UI.Image bgImage = skillTreePanel.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

            // Add CanvasGroup for fading
            CanvasGroup cg = skillTreePanel.AddComponent<CanvasGroup>();

            // Add SkillTreeUI component
            SkillTreeUI skillTreeUI = skillTreePanel.AddComponent<SkillTreeUI>();

            // Create header
            GameObject header = new GameObject("Header");
            header.transform.SetParent(skillTreePanel.transform);
            RectTransform headerRT = header.AddComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0, 80);

            UnityEngine.UI.Image headerBg = header.AddComponent<UnityEngine.UI.Image>();
            headerBg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

            // Create Dark Matter text
            GameObject dmTextObj = new GameObject("DarkMatterText");
            dmTextObj.transform.SetParent(header.transform);
            RectTransform dmRT = dmTextObj.AddComponent<RectTransform>();
            dmRT.anchorMin = new Vector2(0, 0.5f);
            dmRT.anchorMax = new Vector2(0, 0.5f);
            dmRT.pivot = new Vector2(0, 0.5f);
            dmRT.anchoredPosition = new Vector2(30, 0);
            dmRT.sizeDelta = new Vector2(300, 50);

            TMPro.TextMeshProUGUI dmText = dmTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            dmText.text = "DM: 0";
            dmText.fontSize = 32;
            dmText.color = new Color(0.8f, 0.5f, 1f);
            dmText.alignment = TMPro.TextAlignmentOptions.Left;

            // Create Skill Points text
            GameObject spTextObj = new GameObject("SkillPointsText");
            spTextObj.transform.SetParent(header.transform);
            RectTransform spRT = spTextObj.AddComponent<RectTransform>();
            spRT.anchorMin = new Vector2(0.5f, 0.5f);
            spRT.anchorMax = new Vector2(0.5f, 0.5f);
            spRT.pivot = new Vector2(0.5f, 0.5f);
            spRT.anchoredPosition = Vector2.zero;
            spRT.sizeDelta = new Vector2(200, 50);

            TMPro.TextMeshProUGUI spText = spTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            spText.text = "Skills: 0";
            spText.fontSize = 28;
            spText.color = Color.white;
            spText.alignment = TMPro.TextAlignmentOptions.Center;

            // Create Close button
            GameObject closeBtn = new GameObject("CloseButton");
            closeBtn.transform.SetParent(header.transform);
            RectTransform closeBtnRT = closeBtn.AddComponent<RectTransform>();
            closeBtnRT.anchorMin = new Vector2(1, 0.5f);
            closeBtnRT.anchorMax = new Vector2(1, 0.5f);
            closeBtnRT.pivot = new Vector2(1, 0.5f);
            closeBtnRT.anchoredPosition = new Vector2(-30, 0);
            closeBtnRT.sizeDelta = new Vector2(50, 50);

            UnityEngine.UI.Image closeBtnImg = closeBtn.AddComponent<UnityEngine.UI.Image>();
            closeBtnImg.color = new Color(0.8f, 0.3f, 0.3f);

            UnityEngine.UI.Button closeBtnComp = closeBtn.AddComponent<UnityEngine.UI.Button>();
            closeBtnComp.targetGraphic = closeBtnImg;

            GameObject closeBtnText = new GameObject("Text");
            closeBtnText.transform.SetParent(closeBtn.transform);
            RectTransform closeBtnTextRT = closeBtnText.AddComponent<RectTransform>();
            closeBtnTextRT.anchorMin = Vector2.zero;
            closeBtnTextRT.anchorMax = Vector2.one;
            closeBtnTextRT.offsetMin = Vector2.zero;
            closeBtnTextRT.offsetMax = Vector2.zero;

            TMPro.TextMeshProUGUI closeTMP = closeBtnText.AddComponent<TMPro.TextMeshProUGUI>();
            closeTMP.text = "X";
            closeTMP.fontSize = 28;
            closeTMP.color = Color.white;
            closeTMP.alignment = TMPro.TextAlignmentOptions.Center;

            // Create node container (scrollable area for skill nodes)
            GameObject nodeContainer = new GameObject("NodeContainer");
            nodeContainer.transform.SetParent(skillTreePanel.transform);
            RectTransform nodeContRT = nodeContainer.AddComponent<RectTransform>();
            nodeContRT.anchorMin = new Vector2(0, 0);
            nodeContRT.anchorMax = new Vector2(0.7f, 1);
            nodeContRT.offsetMin = new Vector2(20, 20);
            nodeContRT.offsetMax = new Vector2(0, -100);

            // Create node info panel (right side)
            GameObject nodeInfoPanel = new GameObject("NodeInfoPanel");
            nodeInfoPanel.transform.SetParent(skillTreePanel.transform);
            RectTransform infoRT = nodeInfoPanel.AddComponent<RectTransform>();
            infoRT.anchorMin = new Vector2(0.7f, 0);
            infoRT.anchorMax = new Vector2(1, 1);
            infoRT.offsetMin = new Vector2(10, 20);
            infoRT.offsetMax = new Vector2(-20, -100);

            UnityEngine.UI.Image infoBg = nodeInfoPanel.AddComponent<UnityEngine.UI.Image>();
            infoBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Node name
            GameObject nodeNameObj = new GameObject("NodeNameText");
            nodeNameObj.transform.SetParent(nodeInfoPanel.transform);
            RectTransform nodeNameRT = nodeNameObj.AddComponent<RectTransform>();
            nodeNameRT.anchorMin = new Vector2(0, 1);
            nodeNameRT.anchorMax = new Vector2(1, 1);
            nodeNameRT.pivot = new Vector2(0.5f, 1);
            nodeNameRT.anchoredPosition = new Vector2(0, -20);
            nodeNameRT.sizeDelta = new Vector2(-40, 50);

            TMPro.TextMeshProUGUI nodeName = nodeNameObj.AddComponent<TMPro.TextMeshProUGUI>();
            nodeName.text = "Select a Skill";
            nodeName.fontSize = 28;
            nodeName.fontStyle = TMPro.FontStyles.Bold;
            nodeName.color = Color.white;
            nodeName.alignment = TMPro.TextAlignmentOptions.Center;

            // Node description
            GameObject nodeDescObj = new GameObject("NodeDescriptionText");
            nodeDescObj.transform.SetParent(nodeInfoPanel.transform);
            RectTransform nodeDescRT = nodeDescObj.AddComponent<RectTransform>();
            nodeDescRT.anchorMin = new Vector2(0, 0.4f);
            nodeDescRT.anchorMax = new Vector2(1, 0.85f);
            nodeDescRT.offsetMin = new Vector2(20, 0);
            nodeDescRT.offsetMax = new Vector2(-20, 0);

            TMPro.TextMeshProUGUI nodeDesc = nodeDescObj.AddComponent<TMPro.TextMeshProUGUI>();
            nodeDesc.text = "Click on a skill node to see its details.";
            nodeDesc.fontSize = 20;
            nodeDesc.color = new Color(0.8f, 0.8f, 0.8f);
            nodeDesc.alignment = TMPro.TextAlignmentOptions.TopLeft;

            // Node cost
            GameObject nodeCostObj = new GameObject("NodeCostText");
            nodeCostObj.transform.SetParent(nodeInfoPanel.transform);
            RectTransform nodeCostRT = nodeCostObj.AddComponent<RectTransform>();
            nodeCostRT.anchorMin = new Vector2(0, 0.25f);
            nodeCostRT.anchorMax = new Vector2(1, 0.35f);
            nodeCostRT.offsetMin = new Vector2(20, 0);
            nodeCostRT.offsetMax = new Vector2(-20, 0);

            TMPro.TextMeshProUGUI nodeCost = nodeCostObj.AddComponent<TMPro.TextMeshProUGUI>();
            nodeCost.text = "Cost: 0 DM";
            nodeCost.fontSize = 24;
            nodeCost.color = new Color(0.8f, 0.5f, 1f);
            nodeCost.alignment = TMPro.TextAlignmentOptions.Center;

            // Purchase button
            GameObject purchaseBtn = new GameObject("PurchaseButton");
            purchaseBtn.transform.SetParent(nodeInfoPanel.transform);
            RectTransform purchaseBtnRT = purchaseBtn.AddComponent<RectTransform>();
            purchaseBtnRT.anchorMin = new Vector2(0.5f, 0);
            purchaseBtnRT.anchorMax = new Vector2(0.5f, 0);
            purchaseBtnRT.pivot = new Vector2(0.5f, 0);
            purchaseBtnRT.anchoredPosition = new Vector2(0, 30);
            purchaseBtnRT.sizeDelta = new Vector2(200, 60);

            UnityEngine.UI.Image purchaseBtnImg = purchaseBtn.AddComponent<UnityEngine.UI.Image>();
            purchaseBtnImg.color = new Color(0.3f, 0.7f, 0.4f);

            UnityEngine.UI.Button purchaseBtnComp = purchaseBtn.AddComponent<UnityEngine.UI.Button>();
            purchaseBtnComp.targetGraphic = purchaseBtnImg;

            GameObject purchaseBtnTextObj = new GameObject("Text");
            purchaseBtnTextObj.transform.SetParent(purchaseBtn.transform);
            RectTransform purchaseBtnTextRT = purchaseBtnTextObj.AddComponent<RectTransform>();
            purchaseBtnTextRT.anchorMin = Vector2.zero;
            purchaseBtnTextRT.anchorMax = Vector2.one;
            purchaseBtnTextRT.offsetMin = Vector2.zero;
            purchaseBtnTextRT.offsetMax = Vector2.zero;

            TMPro.TextMeshProUGUI purchaseTMP = purchaseBtnTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            purchaseTMP.text = "Purchase";
            purchaseTMP.fontSize = 24;
            purchaseTMP.fontStyle = TMPro.FontStyles.Bold;
            purchaseTMP.color = Color.white;
            purchaseTMP.alignment = TMPro.TextAlignmentOptions.Center;

            // Assign references via SerializedObject
            SerializedObject so = new SerializedObject(skillTreeUI);
            so.FindProperty("skillTreePanel").objectReferenceValue = skillTreePanel;
            so.FindProperty("panelCanvasGroup").objectReferenceValue = cg;
            so.FindProperty("darkMatterText").objectReferenceValue = dmText;
            so.FindProperty("skillPointsText").objectReferenceValue = spText;
            so.FindProperty("closeButton").objectReferenceValue = closeBtnComp;
            so.FindProperty("nodeContainer").objectReferenceValue = nodeContainer.transform;
            so.FindProperty("nodeInfoPanel").objectReferenceValue = nodeInfoPanel;
            so.FindProperty("nodeNameText").objectReferenceValue = nodeName;
            so.FindProperty("nodeDescriptionText").objectReferenceValue = nodeDesc;
            so.FindProperty("nodeCostText").objectReferenceValue = nodeCost;
            so.FindProperty("purchaseButton").objectReferenceValue = purchaseBtnComp;
            so.FindProperty("purchaseButtonText").objectReferenceValue = purchaseTMP;
            so.ApplyModifiedProperties();

            // Start panel hidden
            skillTreePanel.SetActive(false);

            EditorUtility.SetDirty(skillTreeUI);
            Debug.Log("[Setup] Created SkillTreeUI!");
        }
    }
}
