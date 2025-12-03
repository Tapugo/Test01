using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.UI;
using System.Linq;

namespace Incredicer.TimeFracture
{
    /// <summary>
    /// UI for the Time Fracture (prestige) system.
    /// Shows requirements, rewards, and allows triggering a fracture.
    /// </summary>
    public class TimeFractureUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas canvas;

        [Header("Settings")]
        [SerializeField] private float animationDuration = 0.3f;

        // UI Elements (created at runtime)
        private GameObject panelRoot;
        private GameObject mainPanel;

        // Info displays
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI levelText;
        private TextMeshProUGUI requirementsText;
        private TextMeshProUGUI rewardsText;
        private TextMeshProUGUI currentBonusesText;
        private TextMeshProUGUI warningText;

        // Buttons
        private Button fractureButton;
        private TextMeshProUGUI fractureButtonText;
        private Button closeButton;

        // Time Shards display in HUD
        private GameObject hudDisplay;
        private TextMeshProUGUI hudShardsText;

        private void Start()
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }

            CreateUI();
            SubscribeToEvents();

            // Start hidden
            panelRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            // Update HUD display
            UpdateHudDisplay();
        }

        #region UI Creation

        private void CreateUI()
        {
            // Panel root (covers screen)
            panelRoot = new GameObject("TimeFracturePanel");
            panelRoot.transform.SetParent(canvas.transform, false);

            var panelRect = panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Semi-transparent background
            var bgImage = panelRoot.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.85f);

            // Click background to close
            var bgButton = panelRoot.AddComponent<Button>();
            bgButton.onClick.AddListener(HidePanel);

            // Main panel
            mainPanel = CreateMainPanel(panelRoot.transform);

            // HUD display
            CreateHudDisplay();
        }

        private GameObject CreateMainPanel(Transform parent)
        {
            var panel = new GameObject("MainCard");
            panel.transform.SetParent(parent, false);

            // Fullscreen with padding like other popups
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.02f);
            rect.anchorMax = new Vector2(0.98f, 0.98f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Card background with gradient feel
            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.04f, 0.12f, 0.98f);

            // Stop clicks from going through
            var button = panel.AddComponent<Button>();
            button.transition = Selectable.Transition.None;

            // Add outline for polish
            var outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.6f, 0.4f, 1f, 0.5f);
            outline.effectDistance = new Vector2(3, -3);

            // Layout
            var layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 40, 40);
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            // Title
            titleText = CreateText(panel.transform, "TIME FRACTURE", 64, new Color(0.7f, 0.5f, 1f));
            titleText.fontStyle = FontStyles.Bold;
            titleText.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 80);

            // Level display
            levelText = CreateText(panel.transform, "Level 0", 48, Color.white);
            levelText.fontStyle = FontStyles.Bold;
            levelText.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 60);

            // Divider
            CreateDivider(panel.transform);

            // Requirements section
            var reqLabel = CreateText(panel.transform, "REQUIREMENTS", 32, new Color(1f, 0.8f, 0.4f));
            reqLabel.fontStyle = FontStyles.Bold;
            reqLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 45);

            requirementsText = CreateText(panel.transform, "", 28, Color.white);
            requirementsText.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 80);

            // Rewards section
            var rewLabel = CreateText(panel.transform, "REWARDS", 32, new Color(0.4f, 1f, 0.6f));
            rewLabel.fontStyle = FontStyles.Bold;
            rewLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 45);

            rewardsText = CreateText(panel.transform, "", 28, Color.white);
            rewardsText.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 80);

            // Current bonuses section
            var bonusLabel = CreateText(panel.transform, "CURRENT BONUSES", 32, new Color(0.6f, 0.8f, 1f));
            bonusLabel.fontStyle = FontStyles.Bold;
            bonusLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 45);

            currentBonusesText = CreateText(panel.transform, "", 26, Color.white);
            currentBonusesText.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 120);

            // Warning
            warningText = CreateText(panel.transform, "", 26, new Color(1f, 0.5f, 0.5f));
            warningText.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 70);

            // Buttons container
            var buttonsObj = new GameObject("Buttons");
            buttonsObj.transform.SetParent(panel.transform, false);
            var buttonsRect = buttonsObj.AddComponent<RectTransform>();
            buttonsRect.sizeDelta = new Vector2(720, 100);

            var buttonsLayout = buttonsObj.AddComponent<HorizontalLayoutGroup>();
            buttonsLayout.spacing = 30;
            buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
            buttonsLayout.childControlWidth = true;
            buttonsLayout.childControlHeight = true;
            buttonsLayout.childForceExpandWidth = true;

            // Fracture button (purple, matching theme)
            fractureButton = CreateStyledButton(buttonsObj.transform, "FRACTURE!", new Color(0.6f, 0.3f, 0.9f));
            fractureButton.onClick.AddListener(OnFractureClicked);
            fractureButtonText = fractureButton.GetComponentInChildren<TextMeshProUGUI>();

            // Close button
            closeButton = CreateStyledButton(buttonsObj.transform, "CLOSE", new Color(0.4f, 0.4f, 0.45f));
            closeButton.onClick.AddListener(HidePanel);

            return panel;
        }

        private TextMeshProUGUI CreateText(Transform parent, string content, int fontSize, Color color)
        {
            var obj = new GameObject("Text");
            obj.transform.SetParent(parent, false);

            var text = obj.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;

            return text;
        }

        private void CreateDivider(Transform parent)
        {
            var divider = new GameObject("Divider");
            divider.transform.SetParent(parent, false);

            var rect = divider.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 2);

            var img = divider.AddComponent<Image>();
            img.color = new Color(0.5f, 0.3f, 0.7f, 0.5f);
        }

        private Button CreateStyledButton(Transform parent, string text, Color bgColor)
        {
            var btnObj = new GameObject(text + "Button");
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, 70);

            var bg = btnObj.AddComponent<Image>();
            bg.color = bgColor;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            var colors = btn.colors;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.8f;
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            btn.colors = colors;

            // Add outline for depth
            var outline = btnObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(2, -2);

            // Button text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = 32;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;
            ApplySharedFont(tmpText);

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return btn;
        }

        private void ApplySharedFont(TextMeshProUGUI text)
        {
            if (text == null) return;

            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                text.font = GameUI.Instance.SharedFont;
            }

            // Add outline effect
            text.fontMaterial.EnableKeyword("OUTLINE_ON");
            text.outlineWidth = 0.2f;
            text.outlineColor = Color.black;
        }

        private void CreateHudDisplay()
        {
            // HUD display is disabled - we use the main menu instead
            hudDisplay = null;
            hudShardsText = null;
        }

        #endregion

        #region Event Handlers

        private void SubscribeToEvents()
        {
            if (TimeFractureManager.Instance != null)
            {
                TimeFractureManager.Instance.OnTimeFractureCompleted += OnFractureCompleted;
            }

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnTimeShardsChanged += OnTimeShardsChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (TimeFractureManager.Instance != null)
            {
                TimeFractureManager.Instance.OnTimeFractureCompleted -= OnFractureCompleted;
            }

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnTimeShardsChanged -= OnTimeShardsChanged;
            }
        }

        private void OnFractureCompleted(int newLevel)
        {
            // Refresh display
            UpdateDisplay();

            // Play celebration effect
            if (mainPanel != null)
            {
                mainPanel.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5);
            }
        }

        private void OnTimeShardsChanged(double newAmount)
        {
            UpdateHudDisplay();
        }

        private void OnFractureClicked()
        {
            if (TimeFractureManager.Instance != null && TimeFractureManager.Instance.CanFracture())
            {
                // Animate button
                fractureButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 3);

                // Perform fracture
                TimeFractureManager.Instance.DoTimeFracture();

                // Panel will update via event
            }
            else
            {
                // Shake button to indicate can't fracture
                fractureButton.transform.DOShakePosition(0.3f, 10f, 20);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Shows the Time Fracture panel.
        /// </summary>
        public void ShowPanel()
        {
            UpdateDisplay();

            panelRoot.SetActive(true);

            // Animate in
            panelRoot.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            panelRoot.GetComponent<Image>().DOFade(0.85f, animationDuration);

            mainPanel.transform.localScale = Vector3.one * 0.8f;
            mainPanel.transform.DOScale(1f, animationDuration).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Hides the Time Fracture panel.
        /// </summary>
        public void HidePanel()
        {
            if (!panelRoot.activeSelf) return;

            panelRoot.GetComponent<Image>().DOFade(0f, animationDuration * 0.5f);
            mainPanel.transform.DOScale(0.8f, animationDuration * 0.5f)
                .SetEase(Ease.InBack)
                .OnComplete(() => panelRoot.SetActive(false));
        }

        /// <summary>
        /// Toggles the Time Fracture panel visibility.
        /// </summary>
        public void Toggle()
        {
            if (panelRoot != null && panelRoot.activeSelf)
                HidePanel();
            else
                ShowPanel();
        }

        #endregion

        #region Updates

        private void UpdateDisplay()
        {
            if (TimeFractureManager.Instance == null) return;

            var manager = TimeFractureManager.Instance;

            // Level
            levelText.text = $"Fracture Level: {manager.FractureLevel}";

            // Requirements
            double moneyReq = manager.GetMoneyRequired();
            double dmReq = manager.GetDarkMatterRequired();
            double currentMoney = CurrencyManager.Instance?.Money ?? 0;
            double currentDM = CurrencyManager.Instance?.DarkMatter ?? 0;

            string moneyColor = currentMoney >= moneyReq ? "#88FF88" : "#FF8888";
            string dmColor = currentDM >= dmReq ? "#88FF88" : "#FF8888";

            requirementsText.text = $"<color={moneyColor}>${GameUI.FormatNumber(moneyReq)}</color> Money\n" +
                                    $"<color={dmColor}>{GameUI.FormatNumber(dmReq)}</color> Dark Matter";

            // Rewards
            double potentialShards = manager.CalculatePotentialTimeShards();
            rewardsText.text = $"+{potentialShards:N0} Time Shards\n" +
                               manager.GetNextBonusPreview();

            // Current bonuses
            currentBonusesText.text = manager.GetBonusDescription();

            // Warning
            warningText.text = "WARNING: All Money, Dark Matter,\nDice, and Skills will be RESET!";

            // Button state
            bool canFracture = manager.CanFracture();
            fractureButton.interactable = canFracture;
            fractureButtonText.text = canFracture ? "FRACTURE!" : "Not Ready";
        }

        private void UpdateHudDisplay()
        {
            // HUD display is disabled - we use the main menu instead
        }

        #endregion
    }
}
