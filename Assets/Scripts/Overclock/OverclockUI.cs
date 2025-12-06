using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.Dice;
using Incredicer.UI;
using Incredicer.Skills;

namespace Incredicer.Overclock
{
    /// <summary>
    /// Simplified UI for activating overclock on random dice.
    /// Shows a panel with an "ACTIVATE" button that overclocks up to 10 random dice.
    /// </summary>
    public class OverclockUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private GUISpriteAssets guiAssets;
        [SerializeField] private GameObject panelPrefab;

        [Header("Settings")]
        [SerializeField] private int maxDiceToOverclock = 10;
        [SerializeField] private float animationDuration = 0.3f;

        // UI Elements (created at runtime or from prefab)
        private GameObject panelRoot;
        private GameObject mainPanel;
        private CanvasGroup panelCanvasGroup;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI infoText;
        private TextMeshProUGUI statsText;
        private Button activateButton;
        private TextMeshProUGUI activateButtonText;
        private Button closeButton;

        private void Start()
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }

            // Get GUI assets from singleton if not assigned
            if (guiAssets == null)
            {
                guiAssets = GUISpriteAssets.Instance;
            }

            // Try to load from prefab first, fallback to runtime creation
            if (panelPrefab != null)
            {
                LoadFromPrefab();
            }
            else
            {
                // Try to load prefab from Resources
                GameObject loadedPrefab = Resources.Load<GameObject>("Prefabs/UI/OverclockPanel");
                if (loadedPrefab != null)
                {
                    panelPrefab = loadedPrefab;
                    LoadFromPrefab();
                }
                else
                {
                    CreateUI();
                }
            }

            SubscribeToEvents();

            // Start hidden
            panelRoot.SetActive(false);
        }

        private void LoadFromPrefab()
        {
            panelRoot = Instantiate(panelPrefab, canvas.transform);
            panelRoot.name = "OverclockPanel";

            // Cache references from prefab
            CachePrefabReferences();

            // Setup button listeners
            SetupButtonListeners();

            // Apply GUI sprites if available
            ApplyGuiSprites();

            // Apply shared font
            ApplySharedFontToPanel();
        }

        private void CachePrefabReferences()
        {
            panelCanvasGroup = panelRoot.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
                panelCanvasGroup = panelRoot.AddComponent<CanvasGroup>();

            mainPanel = panelRoot.transform.Find("MainCard")?.gameObject;
            if (mainPanel != null)
            {
                var mainCanvasGroup = mainPanel.GetComponent<CanvasGroup>();
                if (mainCanvasGroup != null)
                    panelCanvasGroup = mainCanvasGroup;
            }

            // Find text components
            var titleTransform = panelRoot.transform.Find("MainCard/Title");
            if (titleTransform != null)
                titleText = titleTransform.GetComponent<TextMeshProUGUI>();

            var infoTransform = panelRoot.transform.Find("MainCard/InfoText");
            if (infoTransform != null)
                infoText = infoTransform.GetComponent<TextMeshProUGUI>();

            var statsTransform = panelRoot.transform.Find("MainCard/StatsText");
            if (statsTransform != null)
                statsText = statsTransform.GetComponent<TextMeshProUGUI>();

            // Find buttons
            var activateTransform = panelRoot.transform.Find("MainCard/Buttons/ActivateButton");
            if (activateTransform != null)
            {
                activateButton = activateTransform.GetComponent<Button>();
                activateButtonText = activateTransform.GetComponentInChildren<TextMeshProUGUI>();
            }

            var closeTransform = panelRoot.transform.Find("MainCard/Buttons/CloseButton");
            if (closeTransform != null)
                closeButton = closeTransform.GetComponent<Button>();
        }

        private void SetupButtonListeners()
        {
            // Background click to close
            var bgButton = panelRoot.GetComponent<Button>();
            if (bgButton != null)
                bgButton.onClick.AddListener(HidePanel);

            // Main panel click blocker
            if (mainPanel != null)
            {
                var mainButton = mainPanel.GetComponent<Button>();
                if (mainButton != null)
                    mainButton.onClick.RemoveAllListeners(); // Just block clicks
            }

            if (activateButton != null)
                activateButton.onClick.AddListener(OnActivateClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(HidePanel);
        }

        private void ApplyGuiSprites()
        {
            if (guiAssets == null) return;

            // Apply popup background to main card
            if (mainPanel != null && guiAssets.popupBackground != null)
            {
                var bg = mainPanel.GetComponent<Image>();
                if (bg != null)
                {
                    bg.sprite = guiAssets.popupBackground;
                    bg.type = Image.Type.Sliced;
                    bg.color = Color.white;
                }
            }

            // Apply button sprites
            if (activateButton != null && guiAssets.buttonYellow != null)
            {
                var btnBg = activateButton.GetComponent<Image>();
                if (btnBg != null)
                {
                    btnBg.sprite = guiAssets.buttonYellow;
                    btnBg.type = Image.Type.Sliced;
                    btnBg.color = Color.white;
                }
            }

            if (closeButton != null && guiAssets.buttonGray != null)
            {
                var btnBg = closeButton.GetComponent<Image>();
                if (btnBg != null)
                {
                    btnBg.sprite = guiAssets.buttonGray;
                    btnBg.type = Image.Type.Sliced;
                    btnBg.color = Color.white;
                }
            }
        }

        private void ApplySharedFontToPanel()
        {
            if (panelRoot == null || GameUI.Instance == null) return;

            TMP_FontAsset sharedFont = GameUI.Instance.SharedFont;
            if (sharedFont == null) return;

            TextMeshProUGUI[] allTexts = panelRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTexts)
            {
                tmp.font = sharedFont;
                GameUI.ApplyTextOutline(tmp);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
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

            // Semi-transparent background
            var bgImage = panelRoot.AddComponent<Image>();
            bgImage.color = UIDesignSystem.OverlayMedium;

            // Click background to close
            var bgButton = panelRoot.AddComponent<Button>();
            bgButton.onClick.AddListener(HidePanel);

            // Main panel (center card)
            CreateMainPanel(panelRoot.transform);
        }

        private void CreateMainPanel(Transform parent)
        {
            mainPanel = new GameObject("MainCard");
            mainPanel.transform.SetParent(parent, false);

            var rect = mainPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.2f);
            rect.anchorMax = new Vector2(0.9f, 0.8f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Card background
            var bg = mainPanel.AddComponent<Image>();
            if (guiAssets != null && guiAssets.popupBackground != null)
            {
                bg.sprite = guiAssets.popupBackground;
                bg.type = Image.Type.Sliced;
                bg.color = Color.white;
            }
            else
            {
                bg.color = UIDesignSystem.PanelBgDark;
            }

            // Stop clicks from going through
            var button = mainPanel.AddComponent<Button>();
            button.transition = Selectable.Transition.None;

            // Canvas group for fading
            panelCanvasGroup = mainPanel.AddComponent<CanvasGroup>();

            // Layout
            var layout = mainPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 40, 40);
            layout.spacing = 30;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            // Title
            CreateTitle(mainPanel.transform);

            // Fire icon / dice preview
            CreateOverclockIcon(mainPanel.transform);

            // Info text
            CreateInfoText(mainPanel.transform);

            // Stats text
            CreateStatsText(mainPanel.transform);

            // Buttons
            CreateButtons(mainPanel.transform);
        }

        private void CreateTitle(Transform parent)
        {
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent, false);

            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "OVERCLOCK";
            titleText.fontSize = UIDesignSystem.FontSizeHero;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = UIDesignSystem.OverclockOrange;
            ApplySharedFont(titleText);

            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0, 80);

            // Add LayoutElement
            var layoutElement = titleObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 80;
        }

        private void CreateOverclockIcon(Transform parent)
        {
            var iconContainer = new GameObject("IconContainer");
            iconContainer.transform.SetParent(parent, false);

            var containerRect = iconContainer.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(200, 200);

            var layoutElement = iconContainer.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 200;
            layoutElement.preferredWidth = 200;

            // Glow effect
            var glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(iconContainer.transform, false);
            var glowRect = glowObj.AddComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.5f, 0.5f);
            glowRect.anchorMax = new Vector2(0.5f, 0.5f);
            glowRect.sizeDelta = new Vector2(250, 250);
            glowRect.anchoredPosition = Vector2.zero;

            var glowImg = glowObj.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.5f, 0.2f, 0.4f);
            glowObj.transform.DOScale(1.2f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            // Main dice icon with fire effect
            var diceObj = new GameObject("DiceIcon");
            diceObj.transform.SetParent(iconContainer.transform, false);
            var diceRect = diceObj.AddComponent<RectTransform>();
            diceRect.anchorMin = new Vector2(0.5f, 0.5f);
            diceRect.anchorMax = new Vector2(0.5f, 0.5f);
            diceRect.sizeDelta = new Vector2(150, 150);
            diceRect.anchoredPosition = Vector2.zero;

            var diceImg = diceObj.AddComponent<Image>();
            diceImg.color = UIDesignSystem.OverclockOrange;

            // Dice dots
            CreateDiceDots(diceObj.transform);

            // Pulse animation
            diceObj.transform.DOScale(1.05f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        private void CreateDiceDots(Transform parent)
        {
            float dotSize = 20f;
            Vector2[] dotPositions = new Vector2[]
            {
                new Vector2(-35, 35), new Vector2(35, 35),
                new Vector2(-35, 0), new Vector2(35, 0),
                new Vector2(-35, -35), new Vector2(35, -35)
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
                dotImg.color = new Color(0.2f, 0.1f, 0.05f);
            }
        }

        private void CreateInfoText(Transform parent)
        {
            var infoObj = new GameObject("InfoText");
            infoObj.transform.SetParent(parent, false);

            infoText = infoObj.AddComponent<TextMeshProUGUI>();
            infoText.text = $"Activate Overclock to boost up to\n<color=#FFA500>{maxDiceToOverclock}</color> random dice!\n\n" +
                           $"<color=#FFA500>2.5x PAYOUT</color> while overclocked\n" +
                           $"<color=#FF6666>Dice will be destroyed after ~10 rolls</color>";
            infoText.fontSize = UIDesignSystem.FontSizeBody;
            infoText.alignment = TextAlignmentOptions.Center;
            infoText.color = Color.white;
            ApplySharedFont(infoText);

            var infoRect = infoObj.GetComponent<RectTransform>();
            infoRect.sizeDelta = new Vector2(0, 160);

            var layoutElement = infoObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 160;
        }

        private void CreateStatsText(Transform parent)
        {
            var statsObj = new GameObject("StatsText");
            statsObj.transform.SetParent(parent, false);

            statsText = statsObj.AddComponent<TextMeshProUGUI>();
            statsText.text = "Available dice: 0";
            statsText.fontSize = UIDesignSystem.FontSizeHeader;
            statsText.fontStyle = FontStyles.Bold;
            statsText.alignment = TextAlignmentOptions.Center;
            statsText.color = UIDesignSystem.AccentGold;
            ApplySharedFont(statsText);

            var statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.sizeDelta = new Vector2(0, 50);

            var layoutElement = statsObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 50;
        }

        private void CreateButtons(Transform parent)
        {
            var buttonsObj = new GameObject("Buttons");
            buttonsObj.transform.SetParent(parent, false);

            var buttonsRect = buttonsObj.AddComponent<RectTransform>();
            buttonsRect.sizeDelta = new Vector2(0, 80);

            var layoutElement = buttonsObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 80;

            var buttonsLayout = buttonsObj.AddComponent<HorizontalLayoutGroup>();
            buttonsLayout.spacing = 30;
            buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
            buttonsLayout.childControlWidth = false;
            buttonsLayout.childControlHeight = true;
            buttonsLayout.childForceExpandWidth = false;

            // Activate button
            activateButton = CreateStyledButton(buttonsObj.transform, "ACTIVATE!", UIDesignSystem.OverclockOrange, 280);
            activateButton.onClick.AddListener(OnActivateClicked);
            activateButtonText = activateButton.GetComponentInChildren<TextMeshProUGUI>();

            // Close button
            closeButton = CreateStyledButton(buttonsObj.transform, "CLOSE", UIDesignSystem.ButtonSecondary, 200);
            closeButton.onClick.AddListener(HidePanel);
        }

        private Button CreateStyledButton(Transform parent, string text, Color bgColor, float width = 200)
        {
            var btnObj = new GameObject(text + "Button");
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, UIDesignSystem.ButtonHeightLarge);

            var bg = btnObj.AddComponent<Image>();

            // Use appropriate GUI sprite based on color/purpose
            Sprite buttonSprite = null;
            if (guiAssets != null)
            {
                if (bgColor == UIDesignSystem.OverclockOrange || bgColor == UIDesignSystem.AccentOrange)
                    buttonSprite = guiAssets.buttonYellow;
                else if (bgColor == UIDesignSystem.ButtonSecondary)
                    buttonSprite = guiAssets.buttonGray;
                else
                    buttonSprite = guiAssets.buttonBlue;
            }

            if (buttonSprite != null)
            {
                bg.sprite = buttonSprite;
                bg.type = Image.Type.Sliced;
                bg.color = Color.white;
            }
            else
            {
                bg.color = bgColor;
            }

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
            btn.colors = colors;

            // Button text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = UIDesignSystem.FontSizeLarge;
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
            // Add outline effect using TMP's built-in properties (safer than modifying material directly)
            try
            {
                if (text.fontMaterial != null)
                {
                    text.outlineWidth = 0.15f;
                    text.outlineColor = new Color32(0, 0, 0, 180);
                }
            }
            catch (System.NullReferenceException) { }
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
            // Update stats display
            UpdateStatsDisplay();
        }

        private void OnDiceDestroyed(Dice.Dice dice, double dmEarned)
        {
            // Update stats display
            UpdateStatsDisplay();
        }

        private void OnActivateClicked()
        {
            if (OverclockManager.Instance == null) return;

            int available = OverclockManager.Instance.GetAvailableToOverclockCount();
            if (available == 0)
            {
                // No dice available - show message
                if (statsText != null)
                {
                    statsText.text = "<color=#FF6666>No dice available to overclock!</color>";
                }
                return;
            }

            // Overclock random dice
            int overclocked = OverclockManager.Instance.OverclockRandomDice(maxDiceToOverclock);

            if (overclocked > 0)
            {
                // Show success message
                if (statsText != null)
                {
                    statsText.text = $"<color=#00FF00>Overclocked {overclocked} dice!</color>";
                }

                // Show floating text
                if (GameUI.Instance != null)
                {
                    GameUI.Instance.ShowFloatingText(Vector3.zero, $"+{overclocked} OVERCLOCKED!", UIDesignSystem.OverclockOrange);
                }

                // Play sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayJackpotSound();
                }

                // Close panel after a short delay
                DOVirtual.DelayedCall(0.8f, () =>
                {
                    HidePanel();
                });
            }

            UpdateStatsDisplay();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Shows the overclock panel.
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
        /// Shows the overclock panel.
        /// </summary>
        public void ShowPanel()
        {
            UpdateStatsDisplay();

            // Show panel with animation
            panelRoot.SetActive(true);

            // Ensure popup is rendered on top of other UI elements (like menu button)
            panelRoot.transform.SetAsLastSibling();

            panelRoot.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            panelRoot.GetComponent<Image>().DOFade(0.7f, animationDuration);

            mainPanel.transform.localScale = Vector3.one * 0.8f;
            mainPanel.transform.DOScale(1f, animationDuration).SetEase(Ease.OutBack);

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.DOFade(1f, animationDuration);
            }

            // Apply button polish for press/release animations
            if (UI.UIPolishManager.Instance != null)
            {
                UI.UIPolishManager.Instance.PolishButtonsInPanel(panelRoot);
            }

            // Register with PopupManager
            if (Core.PopupManager.Instance != null)
                Core.PopupManager.Instance.RegisterPopupOpen("OverclockUI");
        }

        /// <summary>
        /// Hides the overclock panel.
        /// </summary>
        public void HidePanel()
        {
            if (!panelRoot.activeSelf) return;

            // Unregister from PopupManager
            if (Core.PopupManager.Instance != null)
                Core.PopupManager.Instance.RegisterPopupClosed("OverclockUI");

            panelRoot.GetComponent<Image>().DOFade(0f, animationDuration * 0.5f);

            mainPanel.transform.DOScale(0.8f, animationDuration * 0.5f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    panelRoot.SetActive(false);
                });
        }

        /// <summary>
        /// Shows the overclock panel for a specific dice (legacy support).
        /// Now just shows the main panel.
        /// </summary>
        public void ShowForDice(Dice.Dice dice)
        {
            ShowPanel();
        }

        #endregion

        #region Updates

        private void UpdateStatsDisplay()
        {
            if (statsText == null || OverclockManager.Instance == null) return;

            // Check if Overclock is unlocked
            if (!OverclockManager.Instance.IsUnlocked)
            {
                statsText.text = "<color=#FF6666>LOCKED</color>\n\nUnlock in the <color=#FFA500>Skill Tree</color> first!";
                activateButton.interactable = false;
                if (activateButtonText != null)
                    activateButtonText.text = "LOCKED";
                return;
            }

            int available = OverclockManager.Instance.GetAvailableToOverclockCount();
            int overclocked = OverclockManager.Instance.GetOverclockedCount();

            if (available > 0)
            {
                int toOverclock = Mathf.Min(maxDiceToOverclock, available);
                statsText.text = $"Will overclock: <color=#FFA500>{toOverclock}</color> dice\n" +
                                $"Currently active: <color=#FF6666>{overclocked}</color>";
                activateButton.interactable = true;
                if (activateButtonText != null)
                    activateButtonText.text = "ACTIVATE!";
            }
            else if (overclocked > 0)
            {
                statsText.text = $"All dice are overclocked!\n" +
                                $"Active: <color=#FF6666>{overclocked}</color>";
                activateButton.interactable = false;
                if (activateButtonText != null)
                    activateButtonText.text = "ALL ACTIVE";
            }
            else
            {
                statsText.text = "<color=#888888>No dice available to overclock.\nRoll some dice first!</color>";
                activateButton.interactable = false;
                if (activateButtonText != null)
                    activateButtonText.text = "NO DICE";
            }
        }

        #endregion
    }
}
