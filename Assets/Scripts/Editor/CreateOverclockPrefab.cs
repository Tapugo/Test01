using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using Incredicer.Core;
using Incredicer.UI;

namespace Incredicer.Editor
{
    public static class CreateOverclockPrefab
    {
        [MenuItem("Incredicer/Create Overclock Panel Prefab")]
        public static void Execute()
        {
            // Create the panel hierarchy
            GameObject panelRoot = CreatePanelRoot();
            GameObject mainPanel = CreateMainPanel(panelRoot.transform);

            // Create content
            CreateTitle(mainPanel.transform);
            CreateOverclockIcon(mainPanel.transform);
            CreateInfoText(mainPanel.transform);
            CreateStatsText(mainPanel.transform);
            CreateButtons(mainPanel.transform);

            // Ensure directory exists in Resources folder for runtime loading
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Prefabs");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Prefabs/UI"))
            {
                AssetDatabase.CreateFolder("Assets/Resources/Prefabs", "UI");
            }

            // Save as prefab in Resources folder
            string prefabPath = "Assets/Resources/Prefabs/UI/OverclockPanel.prefab";

            // Delete existing if present
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            PrefabUtility.SaveAsPrefabAsset(panelRoot, prefabPath);
            Object.DestroyImmediate(panelRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CreateOverclockPrefab] Created prefab at {prefabPath}");
        }

        private static GameObject CreatePanelRoot()
        {
            GameObject panelRoot = new GameObject("OverclockPanel");

            RectTransform rt = panelRoot.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Semi-transparent background
            Image bgImage = panelRoot.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.7f);

            // Click background to close
            Button bgButton = panelRoot.AddComponent<Button>();
            bgButton.transition = Selectable.Transition.None;

            // Canvas group
            panelRoot.AddComponent<CanvasGroup>();

            return panelRoot;
        }

        private static GameObject CreateMainPanel(Transform parent)
        {
            GameObject mainPanel = new GameObject("MainCard");
            mainPanel.transform.SetParent(parent, false);

            RectTransform rect = mainPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.08f);
            rect.anchorMax = new Vector2(0.95f, 0.92f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Card background
            Image bg = mainPanel.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);

            // Stop clicks from going through
            Button button = mainPanel.AddComponent<Button>();
            button.transition = Selectable.Transition.None;

            // Canvas group for fading
            mainPanel.AddComponent<CanvasGroup>();

            // Layout
            VerticalLayoutGroup layout = mainPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(50, 50, 50, 50);
            layout.spacing = 40;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            return mainPanel;
        }

        private static void CreateTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent, false);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "OVERCLOCK";
            titleText.fontSize = 96;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.5f, 0.15f); // Overclock orange

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0, 110);

            LayoutElement layoutElement = titleObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 110;
        }

        private static void CreateOverclockIcon(Transform parent)
        {
            GameObject iconContainer = new GameObject("IconContainer");
            iconContainer.transform.SetParent(parent, false);

            RectTransform containerRect = iconContainer.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(280, 280);

            LayoutElement layoutElement = iconContainer.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 280;
            layoutElement.preferredWidth = 280;

            // Glow effect
            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(iconContainer.transform, false);
            RectTransform glowRect = glowObj.AddComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.5f, 0.5f);
            glowRect.anchorMax = new Vector2(0.5f, 0.5f);
            glowRect.sizeDelta = new Vector2(350, 350);
            glowRect.anchoredPosition = Vector2.zero;

            Image glowImg = glowObj.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.5f, 0.2f, 0.4f);

            // Main dice icon
            GameObject diceObj = new GameObject("DiceIcon");
            diceObj.transform.SetParent(iconContainer.transform, false);
            RectTransform diceRect = diceObj.AddComponent<RectTransform>();
            diceRect.anchorMin = new Vector2(0.5f, 0.5f);
            diceRect.anchorMax = new Vector2(0.5f, 0.5f);
            diceRect.sizeDelta = new Vector2(220, 220);
            diceRect.anchoredPosition = Vector2.zero;

            Image diceImg = diceObj.AddComponent<Image>();
            diceImg.color = new Color(1f, 0.5f, 0.15f);

            // Dice dots
            CreateDiceDots(diceObj.transform);
        }

        private static void CreateDiceDots(Transform parent)
        {
            float dotSize = 30f;
            Vector2[] dotPositions = new Vector2[]
            {
                new Vector2(-50, 50), new Vector2(50, 50),
                new Vector2(-50, 0), new Vector2(50, 0),
                new Vector2(-50, -50), new Vector2(50, -50)
            };

            foreach (var pos in dotPositions)
            {
                GameObject dot = new GameObject("Dot");
                dot.transform.SetParent(parent, false);

                RectTransform dotRect = dot.AddComponent<RectTransform>();
                dotRect.anchorMin = new Vector2(0.5f, 0.5f);
                dotRect.anchorMax = new Vector2(0.5f, 0.5f);
                dotRect.sizeDelta = new Vector2(dotSize, dotSize);
                dotRect.anchoredPosition = pos;

                Image dotImg = dot.AddComponent<Image>();
                dotImg.color = new Color(0.2f, 0.1f, 0.05f);
            }
        }

        private static void CreateInfoText(Transform parent)
        {
            GameObject infoObj = new GameObject("InfoText");
            infoObj.transform.SetParent(parent, false);

            TextMeshProUGUI infoText = infoObj.AddComponent<TextMeshProUGUI>();
            infoText.text = "Activate Overclock to boost up to\n<color=#FFA500>10</color> random dice!\n\n" +
                           "<color=#FFA500>2.5x PAYOUT</color> while overclocked\n" +
                           "<color=#FF6666>Dice will be destroyed after ~10 rolls</color>";
            infoText.fontSize = 42;
            infoText.alignment = TextAlignmentOptions.Center;
            infoText.color = Color.white;

            RectTransform infoRect = infoObj.GetComponent<RectTransform>();
            infoRect.sizeDelta = new Vector2(0, 240);

            LayoutElement layoutElement = infoObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 240;
        }

        private static void CreateStatsText(Transform parent)
        {
            GameObject statsObj = new GameObject("StatsText");
            statsObj.transform.SetParent(parent, false);

            TextMeshProUGUI statsText = statsObj.AddComponent<TextMeshProUGUI>();
            statsText.text = "Available dice: 0";
            statsText.fontSize = 48;
            statsText.fontStyle = FontStyles.Bold;
            statsText.alignment = TextAlignmentOptions.Center;
            statsText.color = new Color(1f, 0.85f, 0.2f); // Gold

            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.sizeDelta = new Vector2(0, 70);

            LayoutElement layoutElement = statsObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 70;
        }

        private static void CreateButtons(Transform parent)
        {
            GameObject buttonsObj = new GameObject("Buttons");
            buttonsObj.transform.SetParent(parent, false);

            RectTransform buttonsRect = buttonsObj.AddComponent<RectTransform>();
            buttonsRect.sizeDelta = new Vector2(0, 100);

            LayoutElement layoutElement = buttonsObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 100;

            HorizontalLayoutGroup buttonsLayout = buttonsObj.AddComponent<HorizontalLayoutGroup>();
            buttonsLayout.spacing = 50;
            buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
            buttonsLayout.childControlWidth = false;
            buttonsLayout.childControlHeight = true;
            buttonsLayout.childForceExpandWidth = false;

            // Activate button
            CreateStyledButton(buttonsObj.transform, "ActivateButton", "ACTIVATE!", new Color(1f, 0.5f, 0.15f), 340);

            // Close button
            CreateStyledButton(buttonsObj.transform, "CloseButton", "CLOSE", new Color(0.35f, 0.35f, 0.4f), 240);
        }

        private static void CreateStyledButton(Transform parent, string name, string text, Color bgColor, float width)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, 90);

            Image bg = btnObj.AddComponent<Image>();
            bg.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
            btn.colors = colors;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = 42;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }
}
