using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace Incredicer.Editor
{
    /// <summary>
    /// Editor tool to create a complete dice shop UI in the scene.
    /// </summary>
    public class SetupDiceShopUIEditor : EditorWindow
    {
        [MenuItem("Incredicer/Setup Dice Shop UI")]
        public static void ShowWindow()
        {
            GetWindow<SetupDiceShopUIEditor>("Dice Shop UI Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Dice Shop UI Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Create Dice Shop UI", GUILayout.Height(40)))
            {
                CreateDiceShopUI();
            }

            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "This will create:\n" +
                "- Open Shop button on the main UI\n" +
                "- Shop panel with all dice types\n" +
                "- Close button and money display",
                MessageType.Info);
        }

        private void CreateDiceShopUI()
        {
            // Find or create canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found in scene!");
                return;
            }

            // Check if DiceShopUI already exists
            var existingUI = Object.FindObjectOfType<UI.DiceShopUI>();
            if (existingUI != null)
            {
                if (!EditorUtility.DisplayDialog("Dice Shop UI Exists",
                    "A DiceShopUI already exists. Replace it?", "Replace", "Cancel"))
                {
                    return;
                }
                DestroyImmediate(existingUI.gameObject);
            }

            // Create main object
            GameObject diceShopObj = new GameObject("DiceShopUI");
            diceShopObj.transform.SetParent(canvas.transform, false);
            RectTransform mainRt = diceShopObj.AddComponent<RectTransform>();
            mainRt.anchorMin = Vector2.zero;
            mainRt.anchorMax = Vector2.one;
            mainRt.offsetMin = Vector2.zero;
            mainRt.offsetMax = Vector2.zero;

            UI.DiceShopUI diceShopUI = diceShopObj.AddComponent<UI.DiceShopUI>();

            // === OPEN BUTTON (bottom right) ===
            GameObject openBtnObj = new GameObject("OpenButton");
            openBtnObj.transform.SetParent(diceShopObj.transform, false);
            RectTransform openBtnRt = openBtnObj.AddComponent<RectTransform>();
            openBtnRt.anchorMin = new Vector2(1, 0);
            openBtnRt.anchorMax = new Vector2(1, 0);
            openBtnRt.pivot = new Vector2(1, 0);
            openBtnRt.anchoredPosition = new Vector2(-20, 20);
            openBtnRt.sizeDelta = new Vector2(140, 60);

            Image openBtnBg = openBtnObj.AddComponent<Image>();
            openBtnBg.color = new Color(0.2f, 0.5f, 0.8f, 0.95f);

            Button openBtn = openBtnObj.AddComponent<Button>();
            ColorBlock openCB = openBtn.colors;
            openCB.normalColor = new Color(0.2f, 0.5f, 0.8f);
            openCB.highlightedColor = new Color(0.3f, 0.6f, 0.9f);
            openCB.pressedColor = new Color(0.15f, 0.4f, 0.7f);
            openBtn.colors = openCB;

            GameObject openTextObj = new GameObject("Text");
            openTextObj.transform.SetParent(openBtnObj.transform, false);
            RectTransform openTextRt = openTextObj.AddComponent<RectTransform>();
            openTextRt.anchorMin = Vector2.zero;
            openTextRt.anchorMax = Vector2.one;
            openTextRt.offsetMin = Vector2.zero;
            openTextRt.offsetMax = Vector2.zero;

            TextMeshProUGUI openText = openTextObj.AddComponent<TextMeshProUGUI>();
            openText.text = "DICE SHOP";
            openText.fontSize = 20;
            openText.fontStyle = FontStyles.Bold;
            openText.alignment = TextAlignmentOptions.Center;
            openText.color = Color.white;

            // === SHOP PANEL ===
            GameObject panelObj = new GameObject("ShopPanel");
            panelObj.transform.SetParent(diceShopObj.transform, false);
            RectTransform panelRt = panelObj.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.25f, 0.1f);
            panelRt.anchorMax = new Vector2(0.75f, 0.9f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            Image panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);

            CanvasGroup panelCG = panelObj.AddComponent<CanvasGroup>();

            // === HEADER ===
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(panelObj.transform, false);
            RectTransform headerRt = headerObj.AddComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 0.88f);
            headerRt.anchorMax = new Vector2(1, 1);
            headerRt.offsetMin = new Vector2(10, 0);
            headerRt.offsetMax = new Vector2(-10, -5);

            Image headerBg = headerObj.AddComponent<Image>();
            headerBg.color = new Color(0.12f, 0.12f, 0.18f, 0.95f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerObj.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 0);
            titleRt.anchorMax = new Vector2(0.4f, 1);
            titleRt.offsetMin = new Vector2(15, 5);
            titleRt.offsetMax = new Vector2(0, -5);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "DICE SHOP";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = new Color(0.9f, 0.9f, 1f);

            // Money display
            GameObject moneyObj = new GameObject("MoneyDisplay");
            moneyObj.transform.SetParent(headerObj.transform, false);
            RectTransform moneyRt = moneyObj.AddComponent<RectTransform>();
            moneyRt.anchorMin = new Vector2(0.4f, 0);
            moneyRt.anchorMax = new Vector2(0.8f, 1);
            moneyRt.offsetMin = new Vector2(5, 5);
            moneyRt.offsetMax = new Vector2(-5, -5);

            TextMeshProUGUI moneyText = moneyObj.AddComponent<TextMeshProUGUI>();
            moneyText.text = "$0";
            moneyText.fontSize = 22;
            moneyText.alignment = TextAlignmentOptions.Center;
            moneyText.color = new Color(0.3f, 1f, 0.4f);

            // Close button
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(headerObj.transform, false);
            RectTransform closeRt = closeObj.AddComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(0.9f, 0.15f);
            closeRt.anchorMax = new Vector2(0.98f, 0.85f);
            closeRt.offsetMin = Vector2.zero;
            closeRt.offsetMax = Vector2.zero;

            Image closeBg = closeObj.AddComponent<Image>();
            closeBg.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);

            Button closeBtn = closeObj.AddComponent<Button>();
            ColorBlock closeCB = closeBtn.colors;
            closeCB.normalColor = new Color(0.8f, 0.2f, 0.2f);
            closeCB.highlightedColor = new Color(1f, 0.3f, 0.3f);
            closeCB.pressedColor = new Color(0.6f, 0.15f, 0.15f);
            closeBtn.colors = closeCB;

            GameObject closeTextObj = new GameObject("Text");
            closeTextObj.transform.SetParent(closeObj.transform, false);
            RectTransform closeTextRt = closeTextObj.AddComponent<RectTransform>();
            closeTextRt.anchorMin = Vector2.zero;
            closeTextRt.anchorMax = Vector2.one;
            closeTextRt.offsetMin = Vector2.zero;
            closeTextRt.offsetMax = Vector2.zero;

            TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.fontSize = 24;
            closeText.fontStyle = FontStyles.Bold;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.color = Color.white;

            // === CONTENT AREA (with scroll) ===
            GameObject contentAreaObj = new GameObject("ContentArea");
            contentAreaObj.transform.SetParent(panelObj.transform, false);
            RectTransform contentAreaRt = contentAreaObj.AddComponent<RectTransform>();
            contentAreaRt.anchorMin = new Vector2(0, 0);
            contentAreaRt.anchorMax = new Vector2(1, 0.88f);
            contentAreaRt.offsetMin = new Vector2(10, 10);
            contentAreaRt.offsetMax = new Vector2(-10, -5);

            Image contentAreaBg = contentAreaObj.AddComponent<Image>();
            contentAreaBg.color = new Color(0.06f, 0.06f, 0.09f, 0.95f);

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

            // Content container
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRt = contentObj.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRt;
            scrollRect.content = contentRt;

            // Wire up the DiceShopUI component
            SerializedObject so = new SerializedObject(diceShopUI);
            so.FindProperty("shopPanel").objectReferenceValue = panelObj;
            so.FindProperty("panelCanvasGroup").objectReferenceValue = panelCG;
            so.FindProperty("openButton").objectReferenceValue = openBtn;
            so.FindProperty("openButtonText").objectReferenceValue = openText;
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("contentContainer").objectReferenceValue = contentRt;
            so.FindProperty("moneyDisplayText").objectReferenceValue = moneyText;
            so.ApplyModifiedProperties();

            // Start hidden
            panelObj.SetActive(false);

            Selection.activeGameObject = diceShopObj;
            EditorUtility.SetDirty(diceShopObj);

            Debug.Log("[SetupDiceShopUIEditor] Dice Shop UI created successfully!");
        }
    }
}
