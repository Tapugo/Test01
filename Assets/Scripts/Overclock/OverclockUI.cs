using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.Dice;
using Incredicer.UI;

namespace Incredicer.Overclock
{
    /// <summary>
    /// UI for selecting and overclocking dice.
    /// Shows when player taps on a dice to bring up the overclock option.
    /// </summary>
    public class OverclockUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas canvas;

        [Header("Settings")]
        [SerializeField] private float panelSlideDistance = 300f;
        [SerializeField] private float animationDuration = 0.3f;

        // UI Elements (created at runtime)
        private GameObject panelRoot;
        private GameObject selectionPanel;
        private Image dicePreview;
        private TextMeshProUGUI diceNameText;
        private TextMeshProUGUI multiplierText;
        private TextMeshProUGUI warningText;
        private Button overclockButton;
        private Button cancelButton;

        // Heat display for overclocked dice
        private GameObject heatDisplayRoot;
        private Image heatFillBar;
        private TextMeshProUGUI heatPercentText;

        // Currently selected dice
        private Dice.Dice selectedDice;

        // HUD indicator for overclocked dice count
        private GameObject hudBadge;
        private TextMeshProUGUI hudBadgeText;

        // Dice selection list
        private GameObject diceListContainer;
        private List<Button> diceButtons = new List<Button>();

        // Force rebuild flag
        private bool needsRebuild = true;

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
            // Update heat display if we have a selected overclocked dice
            if (selectedDice != null && OverclockManager.Instance != null)
            {
                var state = OverclockManager.Instance.GetOverclockState(selectedDice);
                if (state != null)
                {
                    UpdateHeatDisplay(state);
                }
            }

            // Update HUD badge
            UpdateHudBadge();
        }

        #region UI Creation

        private void CreateUI()
        {
            // Panel root (covers screen for click blocking)
            panelRoot = new GameObject("OverclockPanel");
            panelRoot.transform.SetParent(canvas.transform, false);

            var panelRect = panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Semi-transparent background - using UIDesignSystem
            var bgImage = panelRoot.AddComponent<Image>();
            bgImage.color = UIDesignSystem.OverlayMedium;

            // Click background to close
            var bgButton = panelRoot.AddComponent<Button>();
            bgButton.onClick.AddListener(HidePanel);

            // Selection panel (center card)
            selectionPanel = CreateSelectionPanel(panelRoot.transform);

            // Heat display (shown when dice is overclocked)
            heatDisplayRoot = CreateHeatDisplay(panelRoot.transform);
            heatDisplayRoot.SetActive(false);

            // HUD badge
            CreateHudBadge();
        }

        private GameObject CreateSelectionPanel(Transform parent)
        {
            var panel = new GameObject("SelectionCard");
            panel.transform.SetParent(parent, false);

            var rect = panel.AddComponent<RectTransform>();
            // Make fullscreen with padding like other popups - using UIDesignSystem
            rect.anchorMin = new Vector2(0.03f, 0.03f);
            rect.anchorMax = new Vector2(0.97f, 0.97f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Card background - using UIDesignSystem
            var bg = panel.AddComponent<Image>();
            bg.color = UIDesignSystem.PanelBgDark;

            // Stop clicks from going through
            var button = panel.AddComponent<Button>();
            button.transition = Selectable.Transition.None;

            // Add outline for polish - using overclock orange color
            var outline = panel.AddComponent<Outline>();
            outline.effectColor = UIDesignSystem.OverclockOrange * 0.6f;
            outline.effectDistance = new Vector2(3, -3);

            // Layout
            var layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset((int)UIDesignSystem.SpacingXL, (int)UIDesignSystem.SpacingXL,
                                            (int)UIDesignSystem.SpacingL, (int)UIDesignSystem.SpacingL);
            layout.spacing = UIDesignSystem.SpacingL;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            // Title - using UIDesignSystem
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "OVERCLOCK";
            titleText.fontSize = UIDesignSystem.FontSizeHero;  // 72px
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = UIDesignSystem.OverclockOrange;
            ApplySharedFont(titleText);
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(620, 90);

            // Dice preview - larger
            var previewObj = new GameObject("DicePreview");
            previewObj.transform.SetParent(panel.transform, false);
            dicePreview = previewObj.AddComponent<Image>();
            dicePreview.color = UIDesignSystem.OverclockOrange;
            var previewRect = previewObj.GetComponent<RectTransform>();
            previewRect.sizeDelta = new Vector2(180, 180);

            // Add dice dots to preview
            UIDesignSystem.CreateDiceDots(previewObj.transform, 6, 28f, new Color(0.2f, 0.1f, 0.05f));

            // Dice name
            var nameObj = new GameObject("DiceName");
            nameObj.transform.SetParent(panel.transform, false);
            diceNameText = nameObj.AddComponent<TextMeshProUGUI>();
            diceNameText.text = "Select a Dice";
            diceNameText.fontSize = UIDesignSystem.FontSizeLarge;  // 40px
            diceNameText.fontStyle = FontStyles.Bold;
            diceNameText.alignment = TextAlignmentOptions.Center;
            diceNameText.color = Color.white;
            ApplySharedFont(diceNameText);
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.sizeDelta = new Vector2(620, 50);

            // Multiplier info
            var multObj = new GameObject("MultiplierInfo");
            multObj.transform.SetParent(panel.transform, false);
            multiplierText = multObj.AddComponent<TextMeshProUGUI>();
            multiplierText.text = "<color=#FFA500>2.5x PAYOUT</color>\nwhile overclocked";
            multiplierText.fontSize = UIDesignSystem.FontSizeHeader;  // 36px
            multiplierText.alignment = TextAlignmentOptions.Center;
            multiplierText.color = Color.white;
            ApplySharedFont(multiplierText);
            var multRect = multObj.GetComponent<RectTransform>();
            multRect.sizeDelta = new Vector2(620, 90);

            // Warning text
            var warnObj = new GameObject("Warning");
            warnObj.transform.SetParent(panel.transform, false);
            warningText = warnObj.AddComponent<TextMeshProUGUI>();
            warningText.text = "<color=#FF4444>WARNING:</color> Dice will be\n<color=#FF4444>DESTROYED</color> after ~10 rolls!";
            warningText.fontSize = UIDesignSystem.FontSizeBody;  // 28px
            warningText.alignment = TextAlignmentOptions.Center;
            warningText.color = new Color(1f, 0.7f, 0.7f);
            ApplySharedFont(warningText);
            var warnRect = warnObj.GetComponent<RectTransform>();
            warnRect.sizeDelta = new Vector2(620, 80);

            // Dice list container (scrollable list of available dice)
            CreateDiceListContainer(panel.transform);

            // Buttons container
            var buttonsObj = new GameObject("Buttons");
            buttonsObj.transform.SetParent(panel.transform, false);
            var buttonsRect = buttonsObj.AddComponent<RectTransform>();
            buttonsRect.sizeDelta = new Vector2(620, 80);

            var buttonsLayout = buttonsObj.AddComponent<HorizontalLayoutGroup>();
            buttonsLayout.spacing = UIDesignSystem.SpacingL;
            buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
            buttonsLayout.childControlWidth = true;
            buttonsLayout.childControlHeight = true;
            buttonsLayout.childForceExpandWidth = true;

            // Overclock button (orange, matching game style) - using UIDesignSystem
            overclockButton = CreateStyledButton(buttonsObj.transform, "OVERCLOCK!", UIDesignSystem.OverclockOrange);
            overclockButton.onClick.AddListener(OnOverclockClicked);

            // Cancel button (gray) - using UIDesignSystem
            cancelButton = CreateStyledButton(buttonsObj.transform, "CLOSE", UIDesignSystem.ButtonSecondary);
            cancelButton.onClick.AddListener(HidePanel);

            return panel;
        }

        private void CreateDiceListContainer(Transform parent)
        {
            // Container for dice selection - large with visible border
            diceListContainer = new GameObject("DiceListContainer");
            diceListContainer.transform.SetParent(parent, false);

            var containerRect = diceListContainer.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(0, 240);  // Full width, good height

            // Background with border - using UIDesignSystem
            var bg = diceListContainer.AddComponent<Image>();
            bg.color = UIDesignSystem.PanelBgMedium;

            // Add outline for visibility - using UIDesignSystem
            var containerOutline = diceListContainer.AddComponent<Outline>();
            containerOutline.effectColor = UIDesignSystem.AccentPurple * 0.5f;
            containerOutline.effectDistance = new Vector2(2, -2);

            // Add mask to contain dice icons
            diceListContainer.AddComponent<Mask>().showMaskGraphic = true;

            // Scroll view for many dice
            var scrollRect = diceListContainer.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.scrollSensitivity = 30f;

            // Viewport inside container (for proper masking)
            var viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(diceListContainer.transform, false);

            var viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(5, 5);
            viewportRect.offsetMax = new Vector2(-5, -5);

            viewportObj.AddComponent<Image>().color = Color.clear;
            viewportObj.AddComponent<Mask>().showMaskGraphic = false;

            scrollRect.viewport = viewportRect;

            // Content container
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);

            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(0, 1);
            contentRect.pivot = new Vector2(0, 0.5f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);

            // Horizontal layout for dice buttons
            var layout = contentObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 15;
            layout.padding = new RectOffset(15, 15, 15, 15);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;

            var contentSizeFitter = contentObj.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;
        }

        private void ApplySharedFont(TextMeshProUGUI text)
        {
            if (text == null) return;
            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                text.font = GameUI.Instance.SharedFont;
            }
            // Add black outline
            text.fontMaterial.EnableKeyword("OUTLINE_ON");
            text.outlineWidth = 0.2f;
            text.outlineColor = Color.black;
        }

        /// <summary>
        /// Creates dice dot pattern on an icon (shows 6 dots like a real dice)
        /// </summary>
        private void CreateDiceDotsOnIcon(Transform parent)
        {
            float dotSize = 20f;
            float margin = 30f;

            Vector2[] dotPositions = new Vector2[]
            {
                new Vector2(-margin, margin),
                new Vector2(-margin, 0),
                new Vector2(-margin, -margin),
                new Vector2(margin, margin),
                new Vector2(margin, 0),
                new Vector2(margin, -margin)
            };

            foreach (var pos in dotPositions)
            {
                var dot = new GameObject("Dot");
                dot.transform.SetParent(parent, false);

                var dotRect = dot.AddComponent<RectTransform>();
                dotRect.anchorMin = new Vector2(0.5f, 0.5f);
                dotRect.anchorMax = new Vector2(0.5f, 0.5f);
                dotRect.sizeDelta = new Vector2(dotSize, dotSize);
                dotRect.anchoredPosition = pos;

                var dotImg = dot.AddComponent<Image>();
                dotImg.color = new Color(0.2f, 0.1f, 0.05f);  // Dark brown pips
            }
        }

        private Button CreateStyledButton(Transform parent, string text, Color bgColor)
        {
            var btnObj = new GameObject(text + "Button");
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, UIDesignSystem.ButtonHeightLarge);  // 72px height

            var bg = btnObj.AddComponent<Image>();
            bg.color = bgColor;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            var colors = btn.colors;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.7f;
            btn.colors = colors;

            // Add outline for depth - using UIDesignSystem
            var outline = btnObj.AddComponent<Outline>();
            outline.effectColor = UIDesignSystem.ShadowColor;
            outline.effectDistance = new Vector2(2, -2);

            // Button text - using UIDesignSystem
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = UIDesignSystem.FontSizeHeader;  // 36px
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

        private GameObject CreateHeatDisplay(Transform parent)
        {
            var display = new GameObject("HeatDisplay");
            display.transform.SetParent(parent, false);

            var rect = display.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.3f);
            rect.anchorMax = new Vector2(0.5f, 0.3f);
            rect.sizeDelta = new Vector2(350, 80);

            // Background
            var bg = display.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            // Layout
            var layout = display.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(15, 15, 10, 10);
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;

            // Heat label
            var labelObj = new GameObject("HeatLabel");
            labelObj.transform.SetParent(display.transform, false);
            var labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = "HEAT LEVEL";
            labelText.fontSize = 16;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = new Color(1f, 0.5f, 0.2f);
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(320, 25);

            // Heat bar background
            var barBgObj = new GameObject("HeatBarBg");
            barBgObj.transform.SetParent(display.transform, false);
            var barBg = barBgObj.AddComponent<Image>();
            barBg.color = new Color(0.2f, 0.2f, 0.2f);
            var barBgRect = barBgObj.GetComponent<RectTransform>();
            barBgRect.sizeDelta = new Vector2(320, 25);

            // Heat bar fill
            var barFillObj = new GameObject("HeatBarFill");
            barFillObj.transform.SetParent(barBgObj.transform, false);
            heatFillBar = barFillObj.AddComponent<Image>();
            heatFillBar.color = new Color(1f, 0.4f, 0.1f);
            var fillRect = barFillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0.5f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            // Heat percent text
            var percentObj = new GameObject("HeatPercent");
            percentObj.transform.SetParent(barBgObj.transform, false);
            heatPercentText = percentObj.AddComponent<TextMeshProUGUI>();
            heatPercentText.text = "50%";
            heatPercentText.fontSize = 16;
            heatPercentText.fontStyle = FontStyles.Bold;
            heatPercentText.alignment = TextAlignmentOptions.Center;
            heatPercentText.color = Color.white;
            var percentRect = percentObj.GetComponent<RectTransform>();
            percentRect.anchorMin = Vector2.zero;
            percentRect.anchorMax = Vector2.one;
            percentRect.offsetMin = Vector2.zero;
            percentRect.offsetMax = Vector2.zero;

            return display;
        }

        private Button CreateButton(Transform parent, string text, Color bgColor)
        {
            var btnObj = new GameObject(text + "Button");
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 50);

            var bg = btnObj.AddComponent<Image>();
            bg.color = bgColor;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            var colors = btn.colors;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;

            // Button text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = 18;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return btn;
        }

        private void CreateHudBadge()
        {
            // HUD badge is disabled - we use the main menu instead
            hudBadge = null;
            hudBadgeText = null;
        }

        #endregion

        #region Event Handlers

        private void SubscribeToEvents()
        {
            if (OverclockManager.Instance != null)
            {
                OverclockManager.Instance.OnDiceOverclocked += OnDiceOverclocked;
                OverclockManager.Instance.OnDiceDestroyed += OnDiceDestroyed;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (OverclockManager.Instance != null)
            {
                OverclockManager.Instance.OnDiceOverclocked -= OnDiceOverclocked;
                OverclockManager.Instance.OnDiceDestroyed -= OnDiceDestroyed;
            }
        }

        private void OnDiceOverclocked(Dice.Dice dice)
        {
            // Update UI if this is our selected dice
            if (selectedDice == dice)
            {
                // Switch to showing heat display instead of overclock option
                selectionPanel.SetActive(false);
                heatDisplayRoot.SetActive(true);
            }
        }

        private void OnDiceDestroyed(Dice.Dice dice, double dmEarned)
        {
            // Close panel if the destroyed dice was selected
            if (selectedDice == dice)
            {
                HidePanel();

                // Show destruction reward popup
                ShowDestructionReward(dmEarned);
            }
        }

        private void OnOverclockClicked()
        {
            if (selectedDice != null && OverclockManager.Instance != null)
            {
                if (OverclockManager.Instance.StartOverclock(selectedDice))
                {
                    // Successfully overclocked - switch to heat display
                    selectionPanel.SetActive(false);
                    heatDisplayRoot.SetActive(true);

                    // Animate
                    heatDisplayRoot.transform.localScale = Vector3.zero;
                    heatDisplayRoot.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Shows the overclock panel for a specific dice.
        /// Call this when player long-presses or taps a dice.
        /// </summary>
        public void ShowForDice(Dice.Dice dice)
        {
            if (dice == null) return;

            selectedDice = dice;

            // Update display
            if (dice.Data != null)
            {
                diceNameText.text = dice.Data.displayName;
                if (dice.Data.sprite != null)
                {
                    dicePreview.sprite = dice.Data.sprite;
                }
            }

            // Check if already overclocked
            bool isOverclocked = OverclockManager.Instance != null &&
                                  OverclockManager.Instance.IsOverclocked(dice);

            if (isOverclocked)
            {
                // Show heat display
                selectionPanel.SetActive(false);
                heatDisplayRoot.SetActive(true);
            }
            else
            {
                // Show overclock option
                selectionPanel.SetActive(true);
                heatDisplayRoot.SetActive(false);

                // Update multiplier text
                if (OverclockManager.Instance != null)
                {
                    float mult = OverclockManager.Instance.Config.payoutMultiplier;
                    multiplierText.text = $"<color=#FFA500>{mult}x PAYOUT</color>\nwhile overclocked";
                }
            }

            // Show panel with animation
            panelRoot.SetActive(true);
            panelRoot.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            panelRoot.GetComponent<Image>().DOFade(0.7f, animationDuration);

            var activePanel = isOverclocked ? heatDisplayRoot : selectionPanel;
            activePanel.transform.localScale = Vector3.one * 0.8f;
            activePanel.transform.DOScale(1f, animationDuration).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Hides the overclock panel.
        /// </summary>
        public void HidePanel()
        {
            if (!panelRoot.activeSelf) return;

            panelRoot.GetComponent<Image>().DOFade(0f, animationDuration * 0.5f);

            var activePanel = selectionPanel.activeSelf ? selectionPanel : heatDisplayRoot;
            activePanel.transform.DOScale(0.8f, animationDuration * 0.5f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    panelRoot.SetActive(false);
                    selectedDice = null;
                });
        }

        /// <summary>
        /// Shows the overclock panel. If no dice is selected, shows instruction.
        /// </summary>
        public void Show()
        {
            ShowPanel();
        }

        /// <summary>
        /// Toggles the overclock panel visibility.
        /// </summary>
        public void Toggle()
        {
            if (panelRoot != null && panelRoot.activeSelf)
                HidePanel();
            else
                ShowPanel();
        }

        /// <summary>
        /// Shows the overclock panel with instructions to select a dice.
        /// </summary>
        public void ShowPanel()
        {
            selectedDice = null;

            // Update display to show instruction
            diceNameText.text = "Select a Dice Below";
            dicePreview.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            // Update multiplier text to show instructions
            if (OverclockManager.Instance != null)
            {
                float mult = OverclockManager.Instance.Config.payoutMultiplier;
                multiplierText.text = $"Choose a dice to overclock\nfor {mult}x payout!";
            }
            else
            {
                multiplierText.text = "Choose a dice to overclock!";
            }

            warningText.text = "Overclocked dice will be destroyed\nafter several rolls.";

            // Populate dice list
            PopulateDiceList();

            // Show selection panel, hide heat display
            selectionPanel.SetActive(true);
            heatDisplayRoot.SetActive(false);

            // Disable overclock button when no dice selected
            overclockButton.interactable = false;

            // Show panel with animation
            panelRoot.SetActive(true);
            panelRoot.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            panelRoot.GetComponent<Image>().DOFade(0.7f, animationDuration);

            selectionPanel.transform.localScale = Vector3.one * 0.8f;
            selectionPanel.transform.DOScale(1f, animationDuration).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Populates the dice list with available dice from DiceManager.
        /// </summary>
        private void PopulateDiceList()
        {
            if (diceListContainer == null) return;

            // Find content through the scroll hierarchy (DiceListContainer > Viewport > Content)
            var scrollRect = diceListContainer.GetComponent<ScrollRect>();
            Transform content = scrollRect?.content;
            if (content == null) return;

            foreach (var btn in diceButtons)
            {
                if (btn != null && btn.gameObject != null)
                    Destroy(btn.gameObject);
            }
            diceButtons.Clear();

            // Get all dice from DiceManager
            if (DiceManager.Instance == null)
            {
                Debug.LogWarning("[OverclockUI] DiceManager not found");
                return;
            }

            var allDice = DiceManager.Instance.GetAllDice();
            int diceCount = 0;

            foreach (var dice in allDice)
            {
                if (dice == null) continue;

                // Check if already overclocked
                bool isOverclocked = OverclockManager.Instance != null &&
                                      OverclockManager.Instance.IsOverclocked(dice);

                // Create dice button - larger cards
                var btnObj = new GameObject($"Dice_{diceCount}");
                btnObj.transform.SetParent(content, false);

                var btnRect = btnObj.AddComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(180, 220);  // Larger dice cards

                var btnBg = btnObj.AddComponent<Image>();
                btnBg.color = isOverclocked ? new Color(0.4f, 0.2f, 0.1f) : new Color(0.2f, 0.15f, 0.3f);

                var btn = btnObj.AddComponent<Button>();
                btn.targetGraphic = btnBg;
                btn.interactable = !isOverclocked;

                // Capture dice for closure
                var capturedDice = dice;
                btn.onClick.AddListener(() => OnDiceSelected(capturedDice));

                var colors = btn.colors;
                colors.highlightedColor = new Color(0.5f, 0.3f, 0.8f);
                colors.pressedColor = new Color(0.3f, 0.2f, 0.5f);
                colors.disabledColor = new Color(0.3f, 0.2f, 0.15f);
                btn.colors = colors;

                // Dice icon/preview
                var iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(btnObj.transform, false);
                var iconRect = iconObj.AddComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.1f, 0.35f);
                iconRect.anchorMax = new Vector2(0.9f, 0.95f);
                iconRect.offsetMin = Vector2.zero;
                iconRect.offsetMax = Vector2.zero;

                var iconImg = iconObj.AddComponent<Image>();
                if (dice.Data != null && dice.Data.sprite != null)
                {
                    iconImg.sprite = dice.Data.sprite;
                    iconImg.color = Color.white;
                }
                else
                {
                    // Create a dice-like visual with dots
                    iconImg.color = new Color(1f, 0.85f, 0.2f);  // Golden dice color
                    CreateDiceDotsOnIcon(iconObj.transform);
                }

                // Status text (name or "OVERCLOCKED")
                var textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform, false);
                var textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0, 0);
                textRect.anchorMax = new Vector2(1, 0.35f);
                textRect.offsetMin = new Vector2(5, 5);
                textRect.offsetMax = new Vector2(-5, 0);

                var tmpText = textObj.AddComponent<TextMeshProUGUI>();
                if (isOverclocked)
                {
                    tmpText.text = "ACTIVE";
                    tmpText.color = new Color(1f, 0.5f, 0.2f);
                }
                else
                {
                    tmpText.text = dice.Data != null ? dice.Data.displayName : "Dice";
                    tmpText.color = Color.white;
                }
                tmpText.fontSize = 18;
                tmpText.fontStyle = FontStyles.Bold;
                tmpText.alignment = TextAlignmentOptions.Center;
                ApplySharedFont(tmpText);

                diceButtons.Add(btn);
                diceCount++;
            }

            // Show message if no dice
            if (diceCount == 0)
            {
                var noDataObj = new GameObject("NoData");
                noDataObj.transform.SetParent(content, false);
                var noDataRect = noDataObj.AddComponent<RectTransform>();
                noDataRect.sizeDelta = new Vector2(600, 160);

                var noDataText = noDataObj.AddComponent<TextMeshProUGUI>();
                noDataText.text = "No dice available!\nPurchase dice to overclock them.";
                noDataText.fontSize = 28;
                noDataText.alignment = TextAlignmentOptions.Center;
                noDataText.color = new Color(0.6f, 0.6f, 0.7f);
                ApplySharedFont(noDataText);
            }
        }

        /// <summary>
        /// Called when a dice is selected from the list.
        /// </summary>
        private void OnDiceSelected(Dice.Dice dice)
        {
            if (dice == null) return;

            selectedDice = dice;

            // Update display
            if (dice.Data != null)
            {
                diceNameText.text = dice.Data.displayName;
                if (dice.Data.sprite != null)
                {
                    dicePreview.sprite = dice.Data.sprite;
                    dicePreview.color = Color.white;
                }
            }
            else
            {
                diceNameText.text = "Selected Dice";
                dicePreview.color = new Color(1f, 0.6f, 0.2f);
            }

            // Update multiplier text
            if (OverclockManager.Instance != null)
            {
                float mult = OverclockManager.Instance.Config.payoutMultiplier;
                multiplierText.text = $"<color=#FFA500>{mult}x PAYOUT</color>\nwhile overclocked";
            }

            warningText.text = "<color=#FF4444>WARNING:</color> Dice will be\n<color=#FF4444>DESTROYED</color> after ~10 rolls!";

            // Enable overclock button
            overclockButton.interactable = true;

            // Visual feedback - highlight selected dice button
            foreach (var btn in diceButtons)
            {
                if (btn != null)
                {
                    var img = btn.GetComponent<Image>();
                    if (img != null)
                    {
                        // Check if this is the selected dice
                        // Reset all to default, highlight selected
                        img.color = new Color(0.2f, 0.15f, 0.3f);
                    }
                }
            }

            // Play selection sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClickSound();
            }
        }

        #endregion

        #region Updates

        private void UpdateHeatDisplay(OverclockedDiceState state)
        {
            if (heatFillBar == null || heatPercentText == null) return;

            // Update fill bar
            var fillRect = heatFillBar.GetComponent<RectTransform>();
            fillRect.anchorMax = new Vector2(state.currentHeat, 1f);

            // Update color based on heat
            heatFillBar.color = Color.Lerp(
                new Color(1f, 0.6f, 0.2f),
                new Color(1f, 0.1f, 0.1f),
                state.currentHeat
            );

            // Update text
            heatPercentText.text = $"{Mathf.RoundToInt(state.currentHeat * 100)}%";

            // Flash when about to explode
            if (state.IsAboutToExplode)
            {
                float flash = Mathf.PingPong(Time.time * 5f, 1f);
                heatFillBar.color = Color.Lerp(heatFillBar.color, Color.red, flash);
            }
        }

        private void UpdateHudBadge()
        {
            if (OverclockManager.Instance == null || hudBadge == null) return;

            int count = OverclockManager.Instance.GetOverclockedCount();

            if (count > 0)
            {
                hudBadge.SetActive(true);
                hudBadgeText.text = count.ToString();
            }
            else
            {
                hudBadge.SetActive(false);
            }
        }

        private void ShowDestructionReward(double dmEarned)
        {
            // Could show a popup here
            Debug.Log($"[OverclockUI] Dice destroyed! Earned {dmEarned} Dark Matter");
        }

        #endregion
    }
}
