using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using Incredicer.UI;

namespace Incredicer.Editor
{
    public static class SetupCurrencyUI
    {
        private const string GUI_FONT_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Fonts/TMP_TiltWarp.asset";
        private const string PANEL_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Frame/BasicFrame_SquareSolid01_Demo01.png";

        [MenuItem("Incredicer/Setup Currency UI")]
        public static void Execute()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[SetupCurrencyUI] No canvas found!");
                return;
            }

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(GUI_FONT_PATH);
            Sprite panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PANEL_PATH);

            // Find or create CurrencyPanel
            GameObject currencyPanel = GameObject.Find("CurrencyPanel");
            if (currencyPanel == null)
            {
                currencyPanel = new GameObject("CurrencyPanel");
                currencyPanel.transform.SetParent(canvas.transform);
                currencyPanel.AddComponent<RectTransform>();
            }

            // Position panel in top-right, aligned with SkillTreeButton height (Y: -100)
            RectTransform currencyRT = currencyPanel.GetComponent<RectTransform>();
            currencyRT.anchorMin = new Vector2(1, 1);
            currencyRT.anchorMax = new Vector2(1, 1);
            currencyRT.pivot = new Vector2(1, 1);
            currencyRT.anchoredPosition = new Vector2(-20, -100);
            currencyRT.localScale = Vector3.one;
            currencyRT.sizeDelta = new Vector2(280, 130);

            // Set CurrencyPanel to render behind popups (early in sibling order)
            currencyPanel.transform.SetAsFirstSibling();

            // Find or create background as a child (so layout group doesn't affect it)
            Transform existingBg = currencyPanel.transform.Find("Background");
            GameObject backgroundObj;
            if (existingBg != null)
            {
                backgroundObj = existingBg.gameObject;
            }
            else
            {
                backgroundObj = new GameObject("Background");
                backgroundObj.transform.SetParent(currencyPanel.transform);
            }
            // Always ensure background is first child so it renders behind text
            backgroundObj.transform.SetAsFirstSibling();

            RectTransform bgRT = backgroundObj.GetComponent<RectTransform>();
            if (bgRT == null) bgRT = backgroundObj.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            bgRT.localScale = Vector3.one;

            Image bgImg = backgroundObj.GetComponent<Image>();
            if (bgImg == null) bgImg = backgroundObj.AddComponent<Image>();
            // Use Unity's built-in Background sprite for reliable rendering
            bgImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            bgImg.type = Image.Type.Sliced;
            bgImg.color = new Color(0.12f, 0.12f, 0.18f, 0.95f);
            bgImg.raycastTarget = false;

            // Make background ignore layout
            LayoutElement bgLayout = backgroundObj.GetComponent<LayoutElement>();
            if (bgLayout == null) bgLayout = backgroundObj.AddComponent<LayoutElement>();
            bgLayout.ignoreLayout = true;

            // Remove image from panel itself if exists (we use the background child now)
            Image panelImg = currencyPanel.GetComponent<Image>();
            if (panelImg != null)
            {
                Object.DestroyImmediate(panelImg);
            }

            // Add vertical layout group for nice centering
            VerticalLayoutGroup layoutGroup = currencyPanel.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = currencyPanel.AddComponent<VerticalLayoutGroup>();
            }
            layoutGroup.padding = new RectOffset(20, 20, 15, 15);
            layoutGroup.spacing = 5;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            // Remove ContentSizeFitter if exists - we'll use fixed size
            ContentSizeFitter sizeFitter = currencyPanel.GetComponent<ContentSizeFitter>();
            if (sizeFitter != null)
            {
                Object.DestroyImmediate(sizeFitter);
            }

            // Set fixed panel size
            currencyRT.sizeDelta = new Vector2(220, 130);

            // Find or create Money Text
            TextMeshProUGUI moneyText = null;
            Transform existingMoneyText = currencyPanel.transform.Find("MoneyText");
            if (existingMoneyText != null)
            {
                moneyText = existingMoneyText.GetComponent<TextMeshProUGUI>();
            }

            if (moneyText == null)
            {
                GameObject moneyObj = new GameObject("MoneyText");
                moneyObj.transform.SetParent(currencyPanel.transform);
                moneyObj.transform.SetAsFirstSibling();

                RectTransform moneyRT = moneyObj.AddComponent<RectTransform>();
                moneyRT.localScale = Vector3.one;

                LayoutElement moneyLayout = moneyObj.AddComponent<LayoutElement>();
                moneyLayout.preferredHeight = 45;

                moneyText = moneyObj.AddComponent<TextMeshProUGUI>();
            }
            else
            {
                moneyText.transform.SetAsFirstSibling();
                RectTransform moneyRT = moneyText.GetComponent<RectTransform>();
                moneyRT.localScale = Vector3.one;

                LayoutElement moneyLayout = moneyText.GetComponent<LayoutElement>();
                if (moneyLayout == null)
                {
                    moneyLayout = moneyText.gameObject.AddComponent<LayoutElement>();
                }
                moneyLayout.preferredHeight = 45;
            }

            moneyText.text = "$0";
            moneyText.fontSize = 36;
            moneyText.fontStyle = FontStyles.Bold;
            moneyText.alignment = TextAlignmentOptions.Center;
            moneyText.color = new Color(0.4f, 1f, 0.4f);
            moneyText.enableAutoSizing = false;
            if (font != null) moneyText.font = font;

            // Find or create Dark Matter Panel (container that can be shown/hidden)
            GameObject darkMatterPanelObj = null;
            Transform existingDMPanel = currencyPanel.transform.Find("DarkMatterPanel");
            if (existingDMPanel != null)
            {
                darkMatterPanelObj = existingDMPanel.gameObject;
            }
            else
            {
                darkMatterPanelObj = new GameObject("DarkMatterPanel");
                darkMatterPanelObj.transform.SetParent(currencyPanel.transform);

                RectTransform dmPanelRT = darkMatterPanelObj.AddComponent<RectTransform>();
                dmPanelRT.localScale = Vector3.one;

                LayoutElement dmPanelLayout = darkMatterPanelObj.AddComponent<LayoutElement>();
                dmPanelLayout.preferredHeight = 40;
            }

            // Start visible - GameUI will hide it if dark matter isn't unlocked yet
            darkMatterPanelObj.SetActive(true);

            // Find or create Dark Matter Text inside the panel
            TextMeshProUGUI darkMatterText = null;
            Transform existingDMText = darkMatterPanelObj.transform.Find("DarkMatterText");
            if (existingDMText != null)
            {
                darkMatterText = existingDMText.GetComponent<TextMeshProUGUI>();
            }

            if (darkMatterText == null)
            {
                GameObject dmTextObj = new GameObject("DarkMatterText");
                dmTextObj.transform.SetParent(darkMatterPanelObj.transform);

                RectTransform dmRT = dmTextObj.AddComponent<RectTransform>();
                dmRT.anchorMin = Vector2.zero;
                dmRT.anchorMax = Vector2.one;
                dmRT.offsetMin = Vector2.zero;
                dmRT.offsetMax = Vector2.zero;
                dmRT.localScale = Vector3.one;

                darkMatterText = dmTextObj.AddComponent<TextMeshProUGUI>();
            }

            darkMatterText.text = "DM: 0";
            darkMatterText.fontSize = 28;
            darkMatterText.fontStyle = FontStyles.Bold;
            darkMatterText.alignment = TextAlignmentOptions.Center;
            darkMatterText.color = new Color(0.8f, 0.5f, 1f);
            darkMatterText.enableAutoSizing = false;
            if (font != null) darkMatterText.font = font;

            // Also remove old DarkMatterText that was directly under CurrencyPanel
            Transform oldDMText = currencyPanel.transform.Find("DarkMatterText");
            if (oldDMText != null && oldDMText.parent == currencyPanel.transform)
            {
                Object.DestroyImmediate(oldDMText.gameObject);
            }

            // Add FloatingCurrencyEffect component to canvas if not exists
            FloatingCurrencyEffect floatingEffect = canvas.GetComponent<FloatingCurrencyEffect>();
            if (floatingEffect == null)
            {
                floatingEffect = canvas.gameObject.AddComponent<FloatingCurrencyEffect>();
            }

            // Wire up references for FloatingCurrencyEffect
            SerializedObject floatingSO = new SerializedObject(floatingEffect);
            floatingSO.FindProperty("moneyTargetPosition").objectReferenceValue = moneyText.GetComponent<RectTransform>();
            floatingSO.FindProperty("darkMatterTargetPosition").objectReferenceValue = darkMatterText.GetComponent<RectTransform>();
            floatingSO.FindProperty("canvas").objectReferenceValue = canvas;
            floatingSO.ApplyModifiedProperties();

            // Wire GameUI
            GameUI gameUI = canvas.GetComponent<GameUI>();
            if (gameUI != null)
            {
                SerializedObject gameUISO = new SerializedObject(gameUI);
                gameUISO.FindProperty("moneyText").objectReferenceValue = moneyText;
                gameUISO.FindProperty("moneyCounterTarget").objectReferenceValue = moneyText.GetComponent<RectTransform>();
                gameUISO.FindProperty("darkMatterText").objectReferenceValue = darkMatterText;
                gameUISO.FindProperty("darkMatterCounterTarget").objectReferenceValue = darkMatterText.GetComponent<RectTransform>();
                gameUISO.FindProperty("darkMatterPanel").objectReferenceValue = darkMatterPanelObj;
                gameUISO.ApplyModifiedProperties();
                EditorUtility.SetDirty(gameUI);
            }

            EditorUtility.SetDirty(currencyPanel);
            EditorUtility.SetDirty(darkMatterPanelObj);
            EditorUtility.SetDirty(floatingEffect);

            Debug.Log("[SetupCurrencyUI] Currency UI setup complete with centered money and dark matter counters!");
        }
    }
}
