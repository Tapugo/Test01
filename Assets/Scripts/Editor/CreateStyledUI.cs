using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using Incredicer.UI;

namespace Incredicer.Editor
{
    public static class CreateStyledUI
    {
        private const string GUI_FONT_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Fonts/TMP_TiltWarp.asset";
        private const string GUI_FONT_OUTLINE_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Fonts/TMP_TiltWarp_Outline.asset";

        // Button sprite paths from GUI package
        private const string BUTTON_GREEN_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Button/Button01_Demo_Green.png";
        private const string BUTTON_ORANGE_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Button/Button01_Demo_Orange.png";
        private const string BUTTON_PURPLE_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Button/Button01_Demo_Purple.png";
        private const string BUTTON_RED_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Button/Button01_Demo_Red.png";
        private const string BUTTON_GRAY_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Button/Button01_Demo_DarkGray.png";
        private const string PANEL_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Frame/BasicFrame_SquareSolid01_Demo01.png";

        [MenuItem("Incredicer/Restyle All Buttons")]
        public static void Execute()
        {
            // Load fonts
            TMP_FontAsset titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(GUI_FONT_OUTLINE_PATH);
            TMP_FontAsset textFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(GUI_FONT_PATH);
            if (titleFont == null) titleFont = TMP_Settings.defaultFontAsset;
            if (textFont == null) textFont = TMP_Settings.defaultFontAsset;

            // Load button sprites
            Sprite greenSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_GREEN_PATH);
            Sprite orangeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_ORANGE_PATH);
            Sprite purpleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_PURPLE_PATH);
            Sprite graySprite = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_GRAY_PATH);
            Sprite panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PANEL_PATH);

            // Find canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No canvas found!");
                return;
            }

            GameUI gameUI = canvas.GetComponent<GameUI>();

            // Style Skills button - moved down, almost double size
            GameObject skillsBtn = GameObject.Find("SkillTreeButton");
            if (skillsBtn != null)
            {
                StyleButton(skillsBtn, purpleSprite, new Vector2(260, 130), textFont, "Skills", 48);
                RectTransform rt = skillsBtn.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(20, -100);
            }

            // Style Ascend button - positioned underneath Skills button (orange to differentiate from Skills)
            GameObject ascendBtn = GameObject.Find("AscendButton");
            if (ascendBtn != null)
            {
                StyleButton(ascendBtn, orangeSprite, new Vector2(260, 130), textFont, "Ascend\n<size=60%>$1K</size>", 40);
                RectTransform rt = ascendBtn.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(20, -240);
            }

            // Style Buy Dice button - almost double size
            GameObject buyBtn = GameObject.Find("BuyDiceButton");
            if (buyBtn != null)
            {
                StyleButton(buyBtn, greenSprite, new Vector2(400, 150), textFont, "Buy Dice\n<size=60%>$10</size>", 44);
            }

            // Style Upgrade button - almost double size
            GameObject upgradeBtn = GameObject.Find("UpgradeDiceButton");
            if (upgradeBtn != null)
            {
                StyleButton(upgradeBtn, orangeSprite, new Vector2(400, 150), textFont, "Upgrade\n<size=60%>$25</size>", 44);
            }

            // Style currency panel
            GameObject currencyPanel = GameObject.Find("CurrencyPanel");
            if (currencyPanel != null && panelSprite != null)
            {
                Image img = currencyPanel.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = panelSprite;
                    img.type = Image.Type.Sliced;
                    img.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
                }
                RectTransform rt = currencyPanel.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(300, 110);
            }

            // Style shop panel - sized to fit the larger buttons (400 + 400 + spacing + padding)
            GameObject shopPanel = GameObject.Find("ShopPanel");
            if (shopPanel != null && panelSprite != null)
            {
                Image img = shopPanel.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = panelSprite;
                    img.type = Image.Type.Sliced;
                    img.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
                }
                RectTransform rt = shopPanel.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(880, 180); // Wider to fit two 400px buttons + spacing
                rt.anchoredPosition = new Vector2(0, 20); // Keep centered at bottom
            }

            // Update references
            if (gameUI != null)
            {
                SerializedObject so = new SerializedObject(gameUI);

                if (skillsBtn != null)
                    so.FindProperty("skillTreeButton").objectReferenceValue = skillsBtn.GetComponent<Button>();
                if (ascendBtn != null)
                {
                    so.FindProperty("ascendButton").objectReferenceValue = ascendBtn.GetComponent<Button>();
                    var ascendText = ascendBtn.GetComponentInChildren<TextMeshProUGUI>();
                    if (ascendText != null)
                        so.FindProperty("ascendButtonText").objectReferenceValue = ascendText;
                }
                if (buyBtn != null)
                {
                    so.FindProperty("buyDiceButton").objectReferenceValue = buyBtn.GetComponent<Button>();
                    var buyText = buyBtn.GetComponentInChildren<TextMeshProUGUI>();
                    if (buyText != null)
                        so.FindProperty("buyDiceButtonText").objectReferenceValue = buyText;
                }
                if (upgradeBtn != null)
                {
                    so.FindProperty("upgradeDiceButton").objectReferenceValue = upgradeBtn.GetComponent<Button>();
                    var upgradeText = upgradeBtn.GetComponentInChildren<TextMeshProUGUI>();
                    if (upgradeText != null)
                        so.FindProperty("upgradeDiceButtonText").objectReferenceValue = upgradeText;
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(gameUI);
            }

            Debug.Log("[CreateStyledUI] Buttons restyled!");
        }

        private static void StyleButton(GameObject btnObj, Sprite sprite, Vector2 size, TMP_FontAsset font, string text, int fontSize)
        {
            // Set size
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            // Update LayoutElement if present
            LayoutElement le = btnObj.GetComponent<LayoutElement>();
            if (le != null)
            {
                le.minWidth = size.x;
                le.minHeight = size.y;
                le.preferredWidth = size.x;
                le.preferredHeight = size.y;
            }
            else
            {
                le = btnObj.AddComponent<LayoutElement>();
                le.minWidth = size.x;
                le.minHeight = size.y;
                le.preferredWidth = size.x;
                le.preferredHeight = size.y;
            }

            // Set sprite
            Image img = btnObj.GetComponent<Image>();
            if (img != null && sprite != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }

            // Setup button colors
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.targetGraphic = img;
                ColorBlock colors = btn.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
                colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                colors.fadeDuration = 0.1f;
                btn.colors = colors;
            }

            // Find or create text
            TextMeshProUGUI tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp == null)
            {
                // Check for legacy text and remove it
                Text legacyText = btnObj.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    Object.DestroyImmediate(legacyText.gameObject);
                }

                // Create new TMP text
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform);
                RectTransform textRT = textObj.AddComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.offsetMin = new Vector2(10, 8);
                textRT.offsetMax = new Vector2(-10, -8);
                textRT.localScale = Vector3.one;

                tmp = textObj.AddComponent<TextMeshProUGUI>();
            }

            // Style text
            if (tmp != null)
            {
                tmp.text = text;
                tmp.fontSize = fontSize;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                tmp.enableAutoSizing = false;
                tmp.raycastTarget = false;

                if (font != null)
                {
                    tmp.font = font;
                }

                // Ensure proper rect
                RectTransform textRT = tmp.GetComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.offsetMin = new Vector2(10, 8);
                textRT.offsetMax = new Vector2(-10, -8);
                textRT.localScale = Vector3.one;
            }

            EditorUtility.SetDirty(btnObj);
        }
    }
}
