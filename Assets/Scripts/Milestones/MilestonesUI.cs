using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.UI;

namespace Incredicer.Milestones
{
    /// <summary>
    /// UI for displaying and claiming milestones.
    /// </summary>
    public class MilestonesUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas canvas;

        [Header("Settings")]
        [SerializeField] private float animationDuration = 0.3f;

        // UI Elements (created at runtime)
        private GameObject panelRoot;
        private GameObject mainPanel;
        private GameObject contentContainer;
        private ScrollRect scrollRect;

        // Header
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI progressText;

        // Milestone cards
        private List<GameObject> milestoneCards = new List<GameObject>();

        // HUD badge
        private GameObject hudBadge;
        private TextMeshProUGUI hudBadgeText;

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

            // Initial badge update
            UpdateHudBadge();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #region UI Creation

        private void CreateUI()
        {
            // Panel root - fullscreen
            panelRoot = new GameObject("MilestonesPanel");
            panelRoot.transform.SetParent(canvas.transform, false);

            var panelRect = panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Background - using UIDesignSystem
            var bgImage = panelRoot.AddComponent<Image>();
            bgImage.color = UIDesignSystem.OverlayDark;

            // Main panel - fullscreen with padding - using UIDesignSystem
            mainPanel = new GameObject("MainPanel");
            mainPanel.transform.SetParent(panelRoot.transform, false);

            var mainRect = mainPanel.AddComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.03f, 0.03f);
            mainRect.anchorMax = new Vector2(0.97f, 0.97f);
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;

            var mainBg = mainPanel.AddComponent<Image>();
            mainBg.color = UIDesignSystem.PanelBgDark;

            // Add outline for polish - using UIDesignSystem gold accent
            var outline = mainPanel.AddComponent<Outline>();
            outline.effectColor = UIDesignSystem.AccentGold * 0.6f;
            outline.effectDistance = new Vector2(3, -3);

            // Stop clicks from going through
            var mainButton = mainPanel.AddComponent<Button>();
            mainButton.transition = Selectable.Transition.None;

            // Close button in top-right (created first so it's on top)
            CreateCloseButton(mainPanel.transform);

            // Content container with padding for close button
            var contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(mainPanel.transform, false);

            var contentAreaRect = contentArea.AddComponent<RectTransform>();
            contentAreaRect.anchorMin = Vector2.zero;
            contentAreaRect.anchorMax = Vector2.one;
            contentAreaRect.offsetMin = new Vector2(30, 20);
            contentAreaRect.offsetMax = new Vector2(-30, -20);

            // Layout - use child control height to properly expand scroll view
            var mainLayout = contentArea.AddComponent<VerticalLayoutGroup>();
            mainLayout.padding = new RectOffset(10, 10, 10, 10);
            mainLayout.spacing = 15;
            mainLayout.childAlignment = TextAnchor.UpperCenter;
            mainLayout.childControlHeight = true;
            mainLayout.childControlWidth = true;
            mainLayout.childForceExpandHeight = true;

            // Header
            CreateHeader(contentArea.transform);

            // Scroll view - takes remaining space
            CreateScrollView(contentArea.transform);

            // HUD badge
            CreateHudBadge();
        }

        private void CreateHeader(Transform parent)
        {
            var header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            var headerRect = header.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, 90);

            // Fixed height header - larger for better readability
            var headerLayoutElem = header.AddComponent<LayoutElement>();
            headerLayoutElem.preferredHeight = 90;
            headerLayoutElem.flexibleHeight = 0;

            var headerLayout = header.AddComponent<HorizontalLayoutGroup>();
            headerLayout.childAlignment = TextAnchor.MiddleCenter;
            headerLayout.childControlWidth = true;
            headerLayout.childForceExpandWidth = true;
            headerLayout.padding = new RectOffset((int)UIDesignSystem.SpacingL, (int)UIDesignSystem.SpacingL, 0, 0);

            // Title - using UIDesignSystem
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(header.transform, false);
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "MILESTONES";
            titleText.fontSize = UIDesignSystem.FontSizeHero;  // 72px
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = UIDesignSystem.AccentGold;
            ApplySharedFont(titleText);

            // Progress - using UIDesignSystem
            var progressObj = new GameObject("Progress");
            progressObj.transform.SetParent(header.transform, false);
            progressText = progressObj.AddComponent<TextMeshProUGUI>();
            progressText.text = "0 / 0 Complete";
            progressText.fontSize = UIDesignSystem.FontSizeHeader;  // 36px
            progressText.alignment = TextAlignmentOptions.Right;
            progressText.color = Color.white;
            ApplySharedFont(progressText);
        }

        private void CreateScrollView(Transform parent)
        {
            // Scroll view container - expands to fill available space
            var scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(parent, false);

            var scrollRectTransform = scrollObj.AddComponent<RectTransform>();
            scrollRectTransform.sizeDelta = new Vector2(0, 0);

            // Use layout element to expand - this makes it take all remaining space
            var layoutElement = scrollObj.AddComponent<LayoutElement>();
            layoutElement.flexibleHeight = 100;  // High value ensures it takes remaining space
            layoutElement.minHeight = 100;

            scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 50f;

            var scrollImage = scrollObj.AddComponent<Image>();
            scrollImage.color = new Color(0, 0, 0, 0.4f);

            var mask = scrollObj.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            // Viewport
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);

            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            // Content
            contentContainer = new GameObject("Content");
            contentContainer.transform.SetParent(viewport.transform, false);

            var contentRect = contentContainer.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;

            var contentLayout = contentContainer.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            contentLayout.spacing = 10;
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;

            var contentFitter = contentContainer.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
        }

        private void CreateCloseButton(Transform parent)
        {
            var closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(parent, false);

            var closeRect = closeObj.AddComponent<RectTransform>();
            // Position in top-right corner - using UIDesignSystem
            closeRect.anchorMin = new Vector2(1, 1);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.anchoredPosition = new Vector2(-UIDesignSystem.SafeAreaPadding, -UIDesignSystem.SafeAreaPadding);
            closeRect.sizeDelta = new Vector2(UIDesignSystem.ButtonHeightLarge, UIDesignSystem.ButtonHeightLarge);

            var closeBg = closeObj.AddComponent<Image>();
            closeBg.color = UIDesignSystem.ButtonDanger;

            var closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeBg;
            closeBtn.onClick.AddListener(HidePanel);

            // Button colors - using UIDesignSystem
            var colors = closeBtn.colors;
            colors.highlightedColor = UIDesignSystem.ButtonDanger * 1.2f;
            colors.pressedColor = UIDesignSystem.ButtonDanger * 0.7f;
            closeBtn.colors = colors;

            // Add outline - using UIDesignSystem
            var outline = closeObj.AddComponent<Outline>();
            outline.effectColor = UIDesignSystem.ShadowColor;
            outline.effectDistance = new Vector2(2, -2);

            var closeTextObj = new GameObject("Text");
            closeTextObj.transform.SetParent(closeObj.transform, false);
            var closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.fontSize = UIDesignSystem.FontSizeTitle;  // 48px
            closeText.fontStyle = FontStyles.Bold;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.color = Color.white;
            ApplySharedFont(closeText);

            var closeTextRect = closeTextObj.GetComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;
        }

        private void CreateHudBadge()
        {
            // HUD badge is disabled - we use the main menu instead
            // Keep the object reference but don't create it to avoid UI clutter
            hudBadge = null;
            hudBadgeText = null;
        }

        private GameObject CreateMilestoneCard(MilestoneDefinition milestone, MilestoneProgress progress)
        {
            var card = new GameObject($"Milestone_{milestone.milestoneId}");
            card.transform.SetParent(contentContainer.transform, false);

            var cardRect = card.AddComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(0, 200);  // Very tall cards for visibility

            // Background color based on state
            var cardBg = card.AddComponent<Image>();
            if (progress.isClaimed)
            {
                cardBg.color = new Color(0.15f, 0.25f, 0.15f, 0.9f); // Claimed - green tint
            }
            else if (progress.isCompleted)
            {
                cardBg.color = new Color(0.25f, 0.2f, 0.1f, 0.9f); // Ready to claim - gold tint
            }
            else
            {
                cardBg.color = new Color(0.15f, 0.12f, 0.2f, 0.9f); // In progress
            }

            // Add outline for polish
            var cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = milestone.accentColor * 0.6f;
            cardOutline.effectDistance = new Vector2(2, -2);

            // Layout
            var cardLayout = card.AddComponent<HorizontalLayoutGroup>();
            cardLayout.padding = new RectOffset(20, 20, 15, 15);
            cardLayout.spacing = 20;
            cardLayout.childAlignment = TextAnchor.MiddleLeft;
            cardLayout.childControlWidth = false;
            cardLayout.childControlHeight = true;
            cardLayout.childForceExpandWidth = false;

            // Icon placeholder - much larger
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);
            var iconImage = iconObj.AddComponent<Image>();
            iconImage.color = milestone.accentColor;
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(140, 140);
            var iconLayout = iconObj.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = 140;
            iconLayout.preferredHeight = 140;

            // Info section
            var infoObj = new GameObject("Info");
            infoObj.transform.SetParent(card.transform, false);
            var infoRect = infoObj.AddComponent<RectTransform>();
            var infoLayout = infoObj.AddComponent<LayoutElement>();
            infoLayout.flexibleWidth = 1;

            var infoVLayout = infoObj.AddComponent<VerticalLayoutGroup>();
            infoVLayout.spacing = 8;
            infoVLayout.childAlignment = TextAnchor.UpperLeft;
            infoVLayout.childControlHeight = false;
            infoVLayout.childControlWidth = true;

            // Title - much larger text
            var nameText = CreateTextChild(infoObj.transform, milestone.displayName, 40, Color.white);
            nameText.fontStyle = FontStyles.Bold;
            nameText.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50);

            // Description - larger text
            var descText = CreateTextChild(infoObj.transform, milestone.description, 28, new Color(0.8f, 0.8f, 0.8f));
            descText.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40);

            // Progress bar
            if (!progress.isClaimed)
            {
                CreateProgressBar(infoObj.transform, progress, milestone);
            }
            else
            {
                var claimedText = CreateTextChild(infoObj.transform, "CLAIMED", 32, new Color(0.5f, 1f, 0.5f));
                claimedText.fontStyle = FontStyles.Bold;
                claimedText.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40);
            }

            // Claim button (if ready) - much larger
            if (progress.isCompleted && !progress.isClaimed)
            {
                var claimObj = new GameObject("ClaimButton");
                claimObj.transform.SetParent(card.transform, false);

                var claimRect = claimObj.AddComponent<RectTransform>();
                var claimLayout = claimObj.AddComponent<LayoutElement>();
                claimLayout.preferredWidth = 180;
                claimLayout.preferredHeight = 90;

                var claimBg = claimObj.AddComponent<Image>();
                claimBg.color = new Color(0.2f, 0.8f, 0.3f);

                var claimBtn = claimObj.AddComponent<Button>();
                claimBtn.targetGraphic = claimBg;

                var claimOutline = claimObj.AddComponent<Outline>();
                claimOutline.effectColor = new Color(0, 0, 0, 0.5f);
                claimOutline.effectDistance = new Vector2(2, -2);

                string milestoneId = milestone.milestoneId;
                claimBtn.onClick.AddListener(() => OnClaimClicked(milestoneId));

                var claimText = CreateTextChild(claimObj.transform, "CLAIM", 36, Color.white);
                claimText.fontStyle = FontStyles.Bold;
            }

            return card;
        }

        private void CreateProgressBar(Transform parent, MilestoneProgress progress, MilestoneDefinition milestone)
        {
            var barContainer = new GameObject("ProgressBar");
            barContainer.transform.SetParent(parent, false);

            var containerRect = barContainer.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(0, 45);  // Tall progress bar

            // Background
            var barBg = barContainer.AddComponent<Image>();
            barBg.color = new Color(0.2f, 0.2f, 0.2f);

            // Fill
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(barContainer.transform, false);
            var fillImage = fillObj.AddComponent<Image>();
            fillImage.color = milestone.accentColor;

            var fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(Mathf.Clamp01((float)(progress.currentProgress / milestone.targetAmount)), 1);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            // Progress text - larger and more visible
            var progressTextObj = new GameObject("ProgressText");
            progressTextObj.transform.SetParent(barContainer.transform, false);
            var pText = progressTextObj.AddComponent<TextMeshProUGUI>();
            pText.text = $"{GameUI.FormatNumber(progress.currentProgress)} / {GameUI.FormatNumber(milestone.targetAmount)}";
            pText.fontSize = 20;  // Much larger progress text
            pText.alignment = TextAlignmentOptions.Center;
            pText.color = Color.white;

            var pTextRect = progressTextObj.GetComponent<RectTransform>();
            pTextRect.anchorMin = Vector2.zero;
            pTextRect.anchorMax = Vector2.one;
            pTextRect.offsetMin = Vector2.zero;
            pTextRect.offsetMax = Vector2.zero;
        }

        private TextMeshProUGUI CreateTextChild(Transform parent, string text, int fontSize, Color color)
        {
            var obj = new GameObject("Text");
            obj.transform.SetParent(parent, false);

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = color;

            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return tmp;
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

        #endregion

        #region Event Handlers

        private void SubscribeToEvents()
        {
            if (MilestoneManager.Instance != null)
            {
                MilestoneManager.Instance.OnMilestoneCompleted += OnMilestoneCompleted;
                MilestoneManager.Instance.OnMilestoneClaimed += OnMilestoneClaimed;
                MilestoneManager.Instance.OnClaimableCountChanged += OnClaimableCountChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (MilestoneManager.Instance != null)
            {
                MilestoneManager.Instance.OnMilestoneCompleted -= OnMilestoneCompleted;
                MilestoneManager.Instance.OnMilestoneClaimed -= OnMilestoneClaimed;
                MilestoneManager.Instance.OnClaimableCountChanged -= OnClaimableCountChanged;
            }
        }

        private void OnMilestoneCompleted(MilestoneDefinition milestone)
        {
            UpdateHudBadge();

            // If panel is open, refresh it
            if (panelRoot.activeSelf)
            {
                RefreshMilestoneList();
            }
        }

        private void OnMilestoneClaimed(MilestoneDefinition milestone, MilestoneReward[] rewards)
        {
            UpdateHudBadge();

            if (panelRoot.activeSelf)
            {
                RefreshMilestoneList();
            }
        }

        private void OnClaimableCountChanged(int count)
        {
            UpdateHudBadge();
        }

        private void OnClaimClicked(string milestoneId)
        {
            if (MilestoneManager.Instance != null)
            {
                MilestoneManager.Instance.ClaimMilestone(milestoneId);
            }
        }

        #endregion

        #region Public API

        public void ShowPanel()
        {
            RefreshMilestoneList();

            panelRoot.SetActive(true);

            panelRoot.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            panelRoot.GetComponent<Image>().DOFade(0.85f, animationDuration);

            mainPanel.transform.localScale = Vector3.one * 0.8f;
            mainPanel.transform.DOScale(1f, animationDuration).SetEase(Ease.OutBack);
        }

        public void HidePanel()
        {
            if (!panelRoot.activeSelf) return;

            panelRoot.GetComponent<Image>().DOFade(0f, animationDuration * 0.5f);
            mainPanel.transform.DOScale(0.8f, animationDuration * 0.5f)
                .SetEase(Ease.InBack)
                .OnComplete(() => panelRoot.SetActive(false));
        }

        /// <summary>
        /// Toggles the Milestones panel visibility.
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

        private void RefreshMilestoneList()
        {
            if (MilestoneManager.Instance == null) return;

            // Clear existing cards
            foreach (var card in milestoneCards)
            {
                if (card != null) Destroy(card);
            }
            milestoneCards.Clear();

            // Get all milestones
            var milestones = MilestoneManager.Instance.GetAllMilestones();
            int completed = 0;

            foreach (var milestone in milestones)
            {
                var progress = MilestoneManager.Instance.GetProgress(milestone.milestoneId);
                if (progress == null) continue;

                // Update progress value
                progress.currentProgress = MilestoneManager.Instance.GetCurrentValue(milestone.milestoneType);

                var card = CreateMilestoneCard(milestone, progress);
                milestoneCards.Add(card);

                if (progress.isCompleted) completed++;
            }

            // Update header
            progressText.text = $"{completed} / {milestones.Count} Complete";
        }

        private void UpdateHudBadge()
        {
            // HUD badge is disabled - we use the main menu notification badges instead
        }

        #endregion
    }
}
