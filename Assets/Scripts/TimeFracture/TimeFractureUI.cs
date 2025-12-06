using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.UI;

namespace Incredicer.TimeFracture
{
    /// <summary>
    /// UI for the Time Fracture (prestige) system.
    /// Shows requirements, rewards, and allows triggering a fracture.
    /// Uses a prefab for easy visual editing in the Unity Editor.
    /// </summary>
    public class TimeFractureUI : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject panelPrefab;

        [Header("References (Auto-populated from prefab)")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private CanvasGroup mainPanelCanvasGroup;

        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI timeShardsText;
        [SerializeField] private TextMeshProUGUI requirementsText;
        [SerializeField] private TextMeshProUGUI rewardsText;
        [SerializeField] private TextMeshProUGUI currentBonusesText;
        [SerializeField] private TextMeshProUGUI warningText;
        [SerializeField] private TextMeshProUGUI fractureButtonText;

        [Header("Buttons")]
        [SerializeField] private Button fractureButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backgroundButton;

        [Header("Visual Elements")]
        [SerializeField] private Image fractureButtonGlow;
        [SerializeField] private Transform timeShardsIcon;

        [Header("Settings")]
        [SerializeField] private float animationDuration = 0.3f;

        private Canvas canvas;
        private Tween fractureGlowTween;
        private bool isInitialized = false;

        private void Start()
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }

            InitializeUI();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #region Initialization

        private void InitializeUI()
        {
            if (isInitialized) return;

            // If prefab is assigned, instantiate it
            if (panelPrefab != null && panelRoot == null)
            {
                panelRoot = Instantiate(panelPrefab, canvas.transform);
                panelRoot.name = "TimeFracturePanel";
                FindReferences();
            }
            // If no prefab but panelRoot exists (manually set up in scene), just find references
            else if (panelRoot != null)
            {
                FindReferences();
            }
            // Fallback: create UI programmatically (legacy support)
            else
            {
                CreateUIFallback();
            }

            // Setup button listeners
            SetupButtonListeners();

            // Apply shared font to all text
            ApplySharedFontToAll();

            // Start hidden
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            isInitialized = true;
        }

        private void FindReferences()
        {
            if (panelRoot == null) return;

            // Find main panel
            Transform mainPanelTransform = panelRoot.transform.Find("MainPanel");
            if (mainPanelTransform != null)
            {
                mainPanel = mainPanelTransform.gameObject;
                mainPanelCanvasGroup = mainPanel.GetComponent<CanvasGroup>();
                if (mainPanelCanvasGroup == null)
                {
                    mainPanelCanvasGroup = mainPanel.AddComponent<CanvasGroup>();
                }
            }

            // Find content area
            Transform contentArea = mainPanel?.transform.Find("ContentArea");
            if (contentArea == null) return;

            // Header
            Transform header = contentArea.Find("Header");
            if (header != null)
            {
                Transform titleTransform = header.Find("TitleText");
                if (titleTransform != null)
                    titleText = titleTransform.GetComponent<TextMeshProUGUI>();
            }

            // Time Shards Display
            Transform shardsDisplay = contentArea.Find("TimeShardsDisplay");
            if (shardsDisplay != null)
            {
                timeShardsIcon = shardsDisplay.Find("CrystalIcon");
                Transform info = shardsDisplay.Find("Info");
                if (info != null)
                {
                    Transform levelTransform = info.Find("LevelText");
                    if (levelTransform != null)
                        levelText = levelTransform.GetComponent<TextMeshProUGUI>();

                    Transform shardsTransform = info.Find("ShardsText");
                    if (shardsTransform != null)
                        timeShardsText = shardsTransform.GetComponent<TextMeshProUGUI>();
                }
            }

            // Requirements Section
            Transform reqSection = contentArea.Find("RequirementsSection");
            if (reqSection != null)
            {
                Transform content = reqSection.Find("Content");
                if (content != null)
                    requirementsText = content.GetComponent<TextMeshProUGUI>();
            }

            // Rewards Section
            Transform rewardsSection = contentArea.Find("RewardsSection");
            if (rewardsSection != null)
            {
                Transform content = rewardsSection.Find("Content");
                if (content != null)
                    rewardsText = content.GetComponent<TextMeshProUGUI>();
            }

            // Bonuses Section
            Transform bonusesSection = contentArea.Find("BonusesSection");
            if (bonusesSection != null)
            {
                Transform content = bonusesSection.Find("Content");
                if (content != null)
                    currentBonusesText = content.GetComponent<TextMeshProUGUI>();
            }

            // Warning Section
            Transform warningSection = contentArea.Find("WarningSection");
            if (warningSection != null)
            {
                Transform warningTextTransform = warningSection.Find("WarningText");
                if (warningTextTransform != null)
                    warningText = warningTextTransform.GetComponent<TextMeshProUGUI>();
            }

            // Fracture Button
            Transform fractureBtnContainer = contentArea.Find("FractureButtonContainer");
            if (fractureBtnContainer != null)
            {
                Transform btnTransform = fractureBtnContainer.Find("FractureButton");
                if (btnTransform != null)
                {
                    fractureButton = btnTransform.GetComponent<Button>();
                    Transform textTransform = btnTransform.Find("Text");
                    if (textTransform != null)
                        fractureButtonText = textTransform.GetComponent<TextMeshProUGUI>();
                }

                // Look for glow
                Transform glowTransform = fractureBtnContainer.Find("ButtonGlow");
                if (glowTransform != null)
                    fractureButtonGlow = glowTransform.GetComponent<Image>();
            }

            // Close Button
            Transform closeBtnTransform = mainPanel?.transform.Find("CloseButton");
            if (closeBtnTransform != null)
            {
                closeButton = closeBtnTransform.GetComponent<Button>();
            }

            // Background Button
            backgroundButton = panelRoot.GetComponent<Button>();
        }

        private void SetupButtonListeners()
        {
            if (fractureButton != null)
            {
                fractureButton.onClick.RemoveAllListeners();
                fractureButton.onClick.AddListener(OnFractureClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(HidePanel);
            }

            if (backgroundButton != null)
            {
                backgroundButton.onClick.RemoveAllListeners();
                backgroundButton.onClick.AddListener(HidePanel);
            }
        }

        private void ApplySharedFontToAll()
        {
            if (GameUI.Instance == null || GameUI.Instance.SharedFont == null) return;

            var allText = panelRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in allText)
            {
                text.font = GameUI.Instance.SharedFont;
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
        }

        #endregion

        #region Legacy Fallback (creates UI programmatically if no prefab)

        private void CreateUIFallback()
        {
            Debug.LogWarning("[TimeFractureUI] No prefab assigned. Creating UI programmatically. " +
                "For easier editing, assign a prefab via Tools > Incredicer > Create Time Fracture Prefab");

            // Panel root - fullscreen overlay
            panelRoot = new GameObject("TimeFracturePanel");
            panelRoot.transform.SetParent(canvas.transform, false);

            var panelRect = panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var bgImage = panelRoot.AddComponent<Image>();
            bgImage.color = new Color(0.02f, 0.01f, 0.05f, 0.95f);

            backgroundButton = panelRoot.AddComponent<Button>();
            backgroundButton.transition = Selectable.Transition.None;

            // Main panel
            mainPanel = new GameObject("MainPanel");
            mainPanel.transform.SetParent(panelRoot.transform, false);

            var mainRect = mainPanel.AddComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.03f, 0.03f);
            mainRect.anchorMax = new Vector2(0.97f, 0.97f);
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;

            var mainBg = mainPanel.AddComponent<Image>();
            mainBg.color = new Color(0.08f, 0.05f, 0.12f, 0.98f);

            mainPanelCanvasGroup = mainPanel.AddComponent<CanvasGroup>();

            var mainButton = mainPanel.AddComponent<Button>();
            mainButton.transition = Selectable.Transition.None;

            // Content area
            var contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(mainPanel.transform, false);

            var contentRect = contentArea.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(24, 24);
            contentRect.offsetMax = new Vector2(-24, -24);

            var mainLayout = contentArea.AddComponent<VerticalLayoutGroup>();
            mainLayout.padding = new RectOffset(12, 12, 12, 12);
            mainLayout.spacing = 12f;
            mainLayout.childAlignment = TextAnchor.UpperCenter;
            mainLayout.childControlHeight = false;
            mainLayout.childControlWidth = true;
            mainLayout.childForceExpandHeight = false;

            // Create simplified sections
            CreateFallbackHeader(contentArea.transform);
            CreateFallbackTimeShardsDisplay(contentArea.transform);
            CreateFallbackSection(contentArea.transform, "RequirementsSection", "REQUIREMENTS", new Color(1f, 0.7f, 0.3f), out requirementsText);
            CreateFallbackSection(contentArea.transform, "RewardsSection", "REWARDS", new Color(0.4f, 1f, 0.5f), out rewardsText);
            CreateFallbackSection(contentArea.transform, "BonusesSection", "CURRENT BONUSES", new Color(0.4f, 0.8f, 1f), out currentBonusesText);
            CreateFallbackWarning(contentArea.transform);
            CreateFallbackFractureButton(contentArea.transform);
            CreateFallbackCloseButton(mainPanel.transform);
        }

        private void CreateFallbackHeader(Transform parent)
        {
            var header = new GameObject("Header");
            header.transform.SetParent(parent, false);
            var rect = header.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 130);
            header.AddComponent<LayoutElement>().preferredHeight = 130;

            var titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(header.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "TIME FRACTURE";
            titleText.fontSize = 112;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.7f, 0.5f, 1f);
        }

        private void CreateFallbackTimeShardsDisplay(Transform parent)
        {
            var container = new GameObject("TimeShardsDisplay");
            container.transform.SetParent(parent, false);
            container.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 160);
            container.AddComponent<LayoutElement>().preferredHeight = 160;

            var hLayout = container.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 16f;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = false;

            // Icon
            var icon = new GameObject("CrystalIcon");
            icon.transform.SetParent(container.transform, false);
            icon.AddComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            icon.AddComponent<Image>().color = new Color(0.3f, 0.7f, 1f);
            timeShardsIcon = icon.transform;

            // Info
            var info = new GameObject("Info");
            info.transform.SetParent(container.transform, false);
            info.AddComponent<RectTransform>().sizeDelta = new Vector2(500, 150);
            var infoLayout = info.AddComponent<VerticalLayoutGroup>();
            infoLayout.spacing = 4f;
            infoLayout.childAlignment = TextAnchor.MiddleLeft;
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = false;

            var levelObj = new GameObject("LevelText");
            levelObj.transform.SetParent(info.transform, false);
            levelText = levelObj.AddComponent<TextMeshProUGUI>();
            levelText.text = "Fracture Level: 0";
            levelText.fontSize = 80;
            levelText.fontStyle = FontStyles.Bold;
            levelText.color = Color.white;
            levelObj.AddComponent<LayoutElement>().preferredHeight = 88;

            var shardsObj = new GameObject("ShardsText");
            shardsObj.transform.SetParent(info.transform, false);
            timeShardsText = shardsObj.AddComponent<TextMeshProUGUI>();
            timeShardsText.text = "Time Shards: 0";
            timeShardsText.fontSize = 72;
            timeShardsText.color = new Color(0.4f, 0.8f, 1f);
            shardsObj.AddComponent<LayoutElement>().preferredHeight = 80;
        }

        private void CreateFallbackSection(Transform parent, string name, string title, Color titleColor, out TextMeshProUGUI contentText)
        {
            var card = new GameObject(name);
            card.transform.SetParent(parent, false);
            card.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 340);
            card.AddComponent<LayoutElement>().preferredHeight = 340;
            card.AddComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f, 0.95f);

            var cardLayout = card.AddComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(16, 16, 12, 12);
            cardLayout.spacing = 8f;
            cardLayout.childAlignment = TextAnchor.UpperCenter;
            cardLayout.childControlHeight = false;
            cardLayout.childControlWidth = true;

            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(card.transform, false);
            var titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = title;
            titleTmp.fontSize = 104;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = titleColor;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleObj.AddComponent<LayoutElement>().preferredHeight = 120;

            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(card.transform, false);
            contentText = contentObj.AddComponent<TextMeshProUGUI>();
            contentText.text = "Loading...";
            contentText.fontSize = 96;
            contentText.color = new Color(0.1f, 0.1f, 0.1f);
            contentText.alignment = TextAlignmentOptions.Center;
            contentText.richText = true;
            contentObj.AddComponent<LayoutElement>().preferredHeight = 200;
        }

        private void CreateFallbackWarning(Transform parent)
        {
            var warning = new GameObject("WarningSection");
            warning.transform.SetParent(parent, false);
            warning.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 140);
            warning.AddComponent<LayoutElement>().preferredHeight = 140;
            warning.AddComponent<Image>().color = new Color(1f, 0.3f, 0.3f, 0.25f);

            // Use vertical layout for centered content
            var vLayout = warning.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(20, 20, 15, 15);
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;

            var textObj = new GameObject("WarningText");
            textObj.transform.SetParent(warning.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(0, 110);
            warningText = textObj.AddComponent<TextMeshProUGUI>();
            warningText.text = "⚠ All progress will be RESET!";
            warningText.fontSize = 72;
            warningText.fontStyle = FontStyles.Bold;
            warningText.color = new Color(1f, 0.5f, 0.5f);
            warningText.alignment = TextAlignmentOptions.Center;
            textObj.AddComponent<LayoutElement>().preferredHeight = 110;
        }

        private void CreateFallbackFractureButton(Transform parent)
        {
            var container = new GameObject("FractureButtonContainer");
            container.transform.SetParent(parent, false);
            container.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 150);
            container.AddComponent<LayoutElement>().preferredHeight = 150;

            var btnObj = new GameObject("FractureButton");
            btnObj.transform.SetParent(container.transform, false);
            var btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.1f, 0.05f);
            btnRect.anchorMax = new Vector2(0.9f, 0.95f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            var btnBg = btnObj.AddComponent<Image>();
            btnBg.color = new Color(0.5f, 0.2f, 0.8f);

            fractureButton = btnObj.AddComponent<Button>();
            fractureButton.targetGraphic = btnBg;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            fractureButtonText = textObj.AddComponent<TextMeshProUGUI>();
            fractureButtonText.text = "ACTIVATE TIME FRACTURE";
            fractureButtonText.fontSize = 72;
            fractureButtonText.fontStyle = FontStyles.Bold;
            fractureButtonText.color = Color.white;
            fractureButtonText.alignment = TextAlignmentOptions.Center;
        }

        private void CreateFallbackCloseButton(Transform parent)
        {
            var closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(parent, false);
            var rect = closeObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-12, -12);
            rect.sizeDelta = new Vector2(80, 80);

            var bg = closeObj.AddComponent<Image>();
            bg.color = new Color(0.85f, 0.25f, 0.25f);

            closeButton = closeObj.AddComponent<Button>();
            closeButton.targetGraphic = bg;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(closeObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var closeText = textObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.fontSize = 48;
            closeText.fontStyle = FontStyles.Bold;
            closeText.color = Color.white;
            closeText.alignment = TextAlignmentOptions.Center;
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
            UpdateDisplay();

            if (mainPanel != null)
            {
                Sequence celebrationSeq = DOTween.Sequence();
                celebrationSeq.Append(mainPanel.transform.DOPunchScale(Vector3.one * 0.15f, UIDesignSystem.AnimSuccessPop, 8));

                if (timeShardsIcon != null)
                {
                    timeShardsIcon.DOScale(1.5f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutBack);
                }

                if (levelText != null)
                {
                    levelText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5);
                    levelText.DOColor(UIDesignSystem.SuccessGreen, 0.2f)
                        .OnComplete(() => levelText.DOColor(Color.white, 0.3f));
                }
            }
        }

        private void OnTimeShardsChanged(double newAmount)
        {
            if (panelRoot != null && panelRoot.activeSelf)
            {
                UpdateDisplay();
            }
        }

        private void OnFractureClicked()
        {
            if (TimeFractureManager.Instance != null && TimeFractureManager.Instance.CanFracture())
            {
                UIDesignSystem.AnimateSuccessPop(fractureButton.transform);

                if (fractureButtonGlow != null)
                {
                    fractureButtonGlow.DOKill();
                    fractureButtonGlow.color = new Color(0.8f, 0.5f, 1f, 0.9f);
                    fractureButtonGlow.DOFade(0f, 0.5f);
                }

                TimeFractureManager.Instance.DoTimeFracture();
            }
            else
            {
                UIDesignSystem.AnimateErrorShake(fractureButton.transform);
            }
        }

        #endregion

        #region Public API

        public void ShowPanel()
        {
            if (!isInitialized)
            {
                InitializeUI();
            }

            UpdateDisplay();

            panelRoot.SetActive(true);
            panelRoot.transform.SetAsLastSibling();

            DOTween.Kill(panelRoot.transform);
            DOTween.Kill(mainPanel.transform);

            var bgImage = panelRoot.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = new Color(0, 0, 0, 0);
                bgImage.DOFade(0.92f, UIDesignSystem.AnimFadeIn).SetEase(UIDesignSystem.EaseFade);
            }

            mainPanel.transform.localScale = Vector3.one * 0.85f;
            if (mainPanelCanvasGroup != null)
            {
                mainPanelCanvasGroup.alpha = 0f;
                mainPanelCanvasGroup.DOFade(1f, UIDesignSystem.AnimFadeIn).SetEase(UIDesignSystem.EaseFade);
            }
            mainPanel.transform.DOScale(1f, UIDesignSystem.AnimSlideIn).SetEase(UIDesignSystem.EasePopIn);

            if (TimeFractureManager.Instance != null && TimeFractureManager.Instance.CanFracture())
            {
                StartFractureButtonGlowAnimation();
            }

            // Apply button polish for press/release animations
            if (UI.UIPolishManager.Instance != null)
            {
                UI.UIPolishManager.Instance.PolishButtonsInPanel(panelRoot);
            }

            if (PopupManager.Instance != null)
                PopupManager.Instance.RegisterPopupOpen("TimeFractureUI");
        }

        public void HidePanel()
        {
            if (panelRoot == null || !panelRoot.activeSelf) return;

            if (PopupManager.Instance != null)
                PopupManager.Instance.RegisterPopupClosed("TimeFractureUI");

            DOTween.Kill(panelRoot.transform);
            DOTween.Kill(mainPanel.transform);
            StopFractureButtonGlowAnimation();

            var bgImage = panelRoot.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.DOFade(0f, UIDesignSystem.AnimFadeOut).SetEase(UIDesignSystem.EaseFade);
            }

            Sequence hideSeq = DOTween.Sequence();
            if (mainPanelCanvasGroup != null)
            {
                hideSeq.Join(mainPanelCanvasGroup.DOFade(0f, UIDesignSystem.AnimFadeOut).SetEase(UIDesignSystem.EaseFade));
            }
            hideSeq.Join(mainPanel.transform.DOScale(0.85f, UIDesignSystem.AnimSlideOut).SetEase(UIDesignSystem.EasePopOut));
            hideSeq.OnComplete(() => panelRoot.SetActive(false));
        }

        public void Toggle()
        {
            if (panelRoot != null && panelRoot.activeSelf)
                HidePanel();
            else
                ShowPanel();
        }

        #endregion

        #region Animations

        private void StartFractureButtonGlowAnimation()
        {
            if (fractureButtonGlow == null) return;

            fractureButtonGlow.gameObject.SetActive(true);
            fractureGlowTween = fractureButtonGlow.DOFade(0.25f, 0.8f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void StopFractureButtonGlowAnimation()
        {
            if (fractureGlowTween != null)
            {
                fractureGlowTween.Kill();
                fractureGlowTween = null;
            }
            if (fractureButtonGlow != null)
            {
                fractureButtonGlow.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Updates

        private void UpdateDisplay()
        {
            if (TimeFractureManager.Instance == null) return;

            var manager = TimeFractureManager.Instance;

            if (levelText != null)
                levelText.text = $"Fracture Level: {manager.FractureLevel}";

            if (timeShardsText != null)
            {
                double currentShards = CurrencyManager.Instance?.TimeShards ?? 0;
                timeShardsText.text = $"Time Shards: {currentShards:N0}";
            }

            // Requirements
            if (requirementsText != null)
            {
                double moneyReq = manager.GetMoneyRequired();
                double dmReq = manager.GetDarkMatterRequired();
                double currentMoney = CurrencyManager.Instance?.Money ?? 0;
                double currentDM = CurrencyManager.Instance?.DarkMatter ?? 0;

                bool moneyMet = currentMoney >= moneyReq;
                bool dmMet = currentDM >= dmReq;

                string moneyColor = moneyMet ? "#88FF88" : "#FF8888";
                string dmColor = dmMet ? "#88FF88" : "#FF8888";
                string moneyIcon = moneyMet ? "<color=#00FF00>OK</color>" : "<color=#FF0000>X</color>";
                string dmIcon = dmMet ? "<color=#00FF00>OK</color>" : "<color=#FF0000>X</color>";

                requirementsText.text = $"<color={moneyColor}>{moneyIcon} ${GameUI.FormatNumber(moneyReq)} Money</color>\n" +
                                        $"<color={dmColor}>{dmIcon} {GameUI.FormatNumber(dmReq)} Dark Matter</color>";
            }

            // Rewards
            if (rewardsText != null)
            {
                double potentialShards = manager.CalculatePotentialTimeShards();
                string bonusPreview = manager.GetNextBonusPreview();
                rewardsText.text = $"<color=#66CCFF>+{potentialShards:N0} Time Shards</color>\n" +
                                   $"<color=#AAFFAA>{bonusPreview}</color>";
            }

            // Current bonuses
            if (currentBonusesText != null)
            {
                string bonuses = manager.GetBonusDescription();
                currentBonusesText.text = string.IsNullOrEmpty(bonuses) ? "<color=#888888>No bonuses yet</color>" : bonuses;
            }

            // Warning
            if (warningText != null)
            {
                warningText.text = "All Money, Dark Matter, Dice & Skills RESET!";
            }

            // Button state
            if (fractureButton != null)
            {
                bool canFracture = manager.CanFracture();
                fractureButton.interactable = canFracture;

                if (fractureButtonText != null)
                {
                    fractureButtonText.text = canFracture ? "ACTIVATE TIME FRACTURE" : "REQUIREMENTS NOT MET";
                }

                if (panelRoot != null && panelRoot.activeSelf)
                {
                    if (canFracture)
                        StartFractureButtonGlowAnimation();
                    else
                        StopFractureButtonGlowAnimation();
                }
            }
        }

        #endregion
    }
}
