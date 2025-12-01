using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Incredicer.UI;

namespace Incredicer.Editor
{
    public static class SetupDiceShopUI
    {
        [MenuItem("Incredicer/Setup Dice Shop UI")]
        public static void Execute()
        {
            // Find GameUI
            GameUI gameUI = Object.FindObjectOfType<GameUI>();
            if (gameUI == null)
            {
                Debug.LogError("[SetupDiceShopUI] GameUI not found in scene!");
                return;
            }

            // Find or create Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[SetupDiceShopUI] Canvas not found in scene!");
                return;
            }

            SerializedObject gameUISO = new SerializedObject(gameUI);

            // Create Dice Shop Button if not exists
            Button existingDiceShopButton = FindChildByName<Button>(canvas.transform, "DiceShopButton");
            if (existingDiceShopButton == null)
            {
                GameObject diceShopButtonObj = CreateButton(canvas.transform, "DiceShopButton", "Dice Shop");
                RectTransform buttonRect = diceShopButtonObj.GetComponent<RectTransform>();

                // Position below the Dark Matter Generator button (bottom center area)
                buttonRect.anchorMin = new Vector2(0.5f, 0f);
                buttonRect.anchorMax = new Vector2(0.5f, 0f);
                buttonRect.pivot = new Vector2(0.5f, 0f);
                buttonRect.anchoredPosition = new Vector2(150f, 80f);
                buttonRect.sizeDelta = new Vector2(140f, 40f);

                Button diceShopButton = diceShopButtonObj.GetComponent<Button>();
                TextMeshProUGUI diceShopButtonText = diceShopButtonObj.GetComponentInChildren<TextMeshProUGUI>();

                // Assign to GameUI
                gameUISO.FindProperty("diceShopButton").objectReferenceValue = diceShopButton;
                gameUISO.FindProperty("diceShopButtonText").objectReferenceValue = diceShopButtonText;

                Debug.Log("[SetupDiceShopUI] Created DiceShopButton");
            }
            else
            {
                gameUISO.FindProperty("diceShopButton").objectReferenceValue = existingDiceShopButton;
                gameUISO.FindProperty("diceShopButtonText").objectReferenceValue = existingDiceShopButton.GetComponentInChildren<TextMeshProUGUI>();
                Debug.Log("[SetupDiceShopUI] Found existing DiceShopButton");
            }

            // Create Dice Shop Panel if not exists
            GameObject existingPanel = FindChildByName<Transform>(canvas.transform, "DiceShopPanel")?.gameObject;
            if (existingPanel == null)
            {
                // Create panel
                GameObject panelObj = new GameObject("DiceShopPanel");
                panelObj.transform.SetParent(canvas.transform, false);

                RectTransform panelRect = panelObj.AddComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
                panelRect.anchoredPosition = Vector2.zero;
                panelRect.sizeDelta = new Vector2(400f, 300f);

                // Add background image
                Image panelImage = panelObj.AddComponent<Image>();
                panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

                // Create header text
                GameObject headerObj = new GameObject("Header");
                headerObj.transform.SetParent(panelObj.transform, false);
                RectTransform headerRect = headerObj.AddComponent<RectTransform>();
                headerRect.anchorMin = new Vector2(0f, 1f);
                headerRect.anchorMax = new Vector2(1f, 1f);
                headerRect.pivot = new Vector2(0.5f, 1f);
                headerRect.anchoredPosition = new Vector2(0f, -10f);
                headerRect.sizeDelta = new Vector2(0f, 40f);

                TextMeshProUGUI headerText = headerObj.AddComponent<TextMeshProUGUI>();
                headerText.text = "Dice Shop";
                headerText.fontSize = 24;
                headerText.alignment = TextAlignmentOptions.Center;
                headerText.color = Color.white;

                // Create scroll view for dice list
                GameObject scrollViewObj = new GameObject("ScrollView");
                scrollViewObj.transform.SetParent(panelObj.transform, false);
                RectTransform scrollRect = scrollViewObj.AddComponent<RectTransform>();
                scrollRect.anchorMin = new Vector2(0f, 0f);
                scrollRect.anchorMax = new Vector2(1f, 1f);
                scrollRect.offsetMin = new Vector2(10f, 50f);
                scrollRect.offsetMax = new Vector2(-10f, -50f);

                ScrollRect scroll = scrollViewObj.AddComponent<ScrollRect>();
                scroll.horizontal = false;
                scroll.vertical = true;

                // Create viewport
                GameObject viewportObj = new GameObject("Viewport");
                viewportObj.transform.SetParent(scrollViewObj.transform, false);
                RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
                viewportRect.anchorMin = Vector2.zero;
                viewportRect.anchorMax = Vector2.one;
                viewportRect.offsetMin = Vector2.zero;
                viewportRect.offsetMax = Vector2.zero;

                Image viewportImage = viewportObj.AddComponent<Image>();
                viewportImage.color = new Color(0.05f, 0.05f, 0.1f, 1f);
                Mask viewportMask = viewportObj.AddComponent<Mask>();
                viewportMask.showMaskGraphic = true;

                // Create content container
                GameObject contentObj = new GameObject("Content");
                contentObj.transform.SetParent(viewportObj.transform, false);
                RectTransform contentRect = contentObj.AddComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0f, 1f);
                contentRect.anchorMax = new Vector2(1f, 1f);
                contentRect.pivot = new Vector2(0.5f, 1f);
                contentRect.anchoredPosition = Vector2.zero;
                contentRect.sizeDelta = new Vector2(0f, 400f); // Will grow as items are added

                // Add vertical layout group
                VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.spacing = 10f;
                layout.padding = new RectOffset(10, 10, 10, 10);
                layout.childControlWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;

                // Add content size fitter
                ContentSizeFitter sizeFitter = contentObj.AddComponent<ContentSizeFitter>();
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                // Setup scroll rect references
                scroll.viewport = viewportRect;
                scroll.content = contentRect;

                // Create close button
                GameObject closeButtonObj = CreateButton(panelObj.transform, "CloseButton", "X");
                RectTransform closeRect = closeButtonObj.GetComponent<RectTransform>();
                closeRect.anchorMin = new Vector2(1f, 1f);
                closeRect.anchorMax = new Vector2(1f, 1f);
                closeRect.pivot = new Vector2(1f, 1f);
                closeRect.anchoredPosition = new Vector2(-5f, -5f);
                closeRect.sizeDelta = new Vector2(30f, 30f);

                // Make close button toggle the panel
                Button closeButton = closeButtonObj.GetComponent<Button>();

                // Assign to GameUI
                gameUISO.FindProperty("diceShopPanel").objectReferenceValue = panelObj;
                gameUISO.FindProperty("diceShopContent").objectReferenceValue = contentObj.transform;

                // Start hidden
                panelObj.SetActive(false);

                Debug.Log("[SetupDiceShopUI] Created DiceShopPanel");
            }
            else
            {
                gameUISO.FindProperty("diceShopPanel").objectReferenceValue = existingPanel;
                Transform content = FindChildByName<Transform>(existingPanel.transform, "Content");
                if (content != null)
                {
                    gameUISO.FindProperty("diceShopContent").objectReferenceValue = content;
                }
                Debug.Log("[SetupDiceShopUI] Found existing DiceShopPanel");
            }

            gameUISO.ApplyModifiedProperties();
            EditorUtility.SetDirty(gameUI);

            Debug.Log("[SetupDiceShopUI] Setup complete!");
        }

        private static T FindChildByName<T>(Transform parent, string name) where T : Component
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    T component = child.GetComponent<T>();
                    if (component != null) return component;
                }

                T found = FindChildByName<T>(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static GameObject CreateButton(Transform parent, string name, string text)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160f, 40f);

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.5f, 0.3f, 1f);

            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.5f, 0.3f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.6f, 0.4f, 1f);
            colors.pressedColor = new Color(0.15f, 0.4f, 0.25f, 1f);
            colors.selectedColor = new Color(0.25f, 0.55f, 0.35f, 1f);
            button.colors = colors;

            // Create text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = 16;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;

            return buttonObj;
        }
    }
}
