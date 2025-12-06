using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.UI;
using MoreMountains.Feedbacks;

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

        // GUI Assets reference
        private GUISpriteAssets guiAssets;

        // UI Elements (created at runtime)
        private GameObject panelRoot;
        private GameObject mainPanel;
        private CanvasGroup mainPanelCanvasGroup;
        private GameObject contentContainer;
        private ScrollRect scrollRect;

        // Header
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI progressText;
        private Image progressFill;

        // Milestone cards
        private List<GameObject> milestoneCards = new List<GameObject>();

        // HUD badge
        private GameObject hudBadge;
        private TextMeshProUGUI hudBadgeText;

        // Feel feedbacks
        private MMF_Player claimFeedback;

        private void Start()
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }

            // Load GUI assets
            guiAssets = GUISpriteAssets.Instance;

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
            // Panel root - fullscreen overlay
            panelRoot = new GameObject("MilestonesPanel");
            panelRoot.transform.SetParent(canvas.transform, false);

            var panelRect = panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Background overlay with gradient feel
            var bgImage = panelRoot.AddComponent<Image>();
            bgImage.color = new Color(0.02f, 0.01f, 0.05f, 0.92f);

            // Click outside to close
            var bgButton = panelRoot.AddComponent<Button>();
            bgButton.transition = Selectable.Transition.None;
            bgButton.onClick.AddListener(HidePanel);

            // Main panel with GUI popup background - slightly more padding from edges
            mainPanel = new GameObject("MainPanel");
            mainPanel.transform.SetParent(panelRoot.transform, false);

            var mainRect = mainPanel.AddComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.04f, 0.05f);
            mainRect.anchorMax = new Vector2(0.96f, 0.95f);
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;

            var mainBg = mainPanel.AddComponent<Image>();
            if (guiAssets != null && guiAssets.popupBackground != null)
            {
                mainBg.sprite = guiAssets.popupBackground;
                mainBg.type = Image.Type.Sliced;
                mainBg.color = Color.white;
            }
            else
            {
                mainBg.color = UIDesignSystem.PanelBgDark;
            }

            // Add canvas group for fade animations
            mainPanelCanvasGroup = mainPanel.AddComponent<CanvasGroup>();

            // Stop clicks from going through to background
            var mainButton = mainPanel.AddComponent<Button>();
            mainButton.transition = Selectable.Transition.None;

            // Content container with generous padding
            var contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(mainPanel.transform, false);

            var contentAreaRect = contentArea.AddComponent<RectTransform>();
            contentAreaRect.anchorMin = Vector2.zero;
            contentAreaRect.anchorMax = Vector2.one;
            contentAreaRect.offsetMin = new Vector2(28, 28);
            contentAreaRect.offsetMax = new Vector2(-28, -28);

            // Layout with better spacing
            var mainLayout = contentArea.AddComponent<VerticalLayoutGroup>();
            mainLayout.padding = new RectOffset(8, 8, 8, 8);
            mainLayout.spacing = 20f;
            mainLayout.childAlignment = TextAnchor.UpperCenter;
            mainLayout.childControlHeight = true;
            mainLayout.childControlWidth = true;
            mainLayout.childForceExpandHeight = true;

            // Header with ribbon
            CreateHeader(contentArea.transform);

            // Overall progress bar
            CreateOverallProgressBar(contentArea.transform);

            // Scroll view - takes remaining space
            CreateScrollView(contentArea.transform);

            // Close button (on top of everything)
            CreateCloseButton(mainPanel.transform);

            // HUD badge
            CreateHudBadge();

            // Setup Feel feedback
            SetupFeedbacks();
        }

        private void SetupFeedbacks()
        {
            // Create feedback holder
            var feedbackHolder = new GameObject("Feedbacks");
            feedbackHolder.transform.SetParent(mainPanel.transform, false);

            claimFeedback = feedbackHolder.AddComponent<MMF_Player>();
            claimFeedback.InitializationMode = MMF_Player.InitializationModes.Awake;
        }

        private void PlayClaimFeedback()
        {
            if (claimFeedback != null)
            {
                claimFeedback.PlayFeedbacks();
            }

            // Haptic feedback on mobile
            #if UNITY_IOS || UNITY_ANDROID
            if (MoreMountains.NiceVibrations.MMVibrationManager.HapticsSupported())
            {
                MoreMountains.NiceVibrations.MMVibrationManager.Haptic(MoreMountains.NiceVibrations.HapticTypes.Success);
            }
            #endif
        }

        private void CreateHeader(Transform parent)
        {
            var header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            var headerRect = header.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, 110);

            // Fixed height header
            var headerLayoutElem = header.AddComponent<LayoutElement>();
            headerLayoutElem.preferredHeight = 110;
            headerLayoutElem.flexibleHeight = 0;

            // Ribbon background for title - wider and more prominent
            if (guiAssets != null && guiAssets.ribbonYellow != null)
            {
                var ribbonBg = new GameObject("RibbonBg");
                ribbonBg.transform.SetParent(header.transform, false);
                var ribbonRect = ribbonBg.AddComponent<RectTransform>();
                ribbonRect.anchorMin = new Vector2(0.02f, 0.05f);
                ribbonRect.anchorMax = new Vector2(0.98f, 0.95f);
                ribbonRect.offsetMin = Vector2.zero;
                ribbonRect.offsetMax = Vector2.zero;

                var ribbonImg = ribbonBg.AddComponent<Image>();
                ribbonImg.sprite = guiAssets.ribbonYellow;
                ribbonImg.type = Image.Type.Sliced;
                ribbonImg.color = new Color(1f, 1f, 1f, 0.95f);
            }

            // Trophy/Star icon on left - bigger
            var iconContainer = new GameObject("TrophyIcon");
            iconContainer.transform.SetParent(header.transform, false);
            var iconRect = iconContainer.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.anchoredPosition = new Vector2(24, 0);
            iconRect.sizeDelta = new Vector2(80, 80);

            // Create star icon
            CreateStarIcon(iconContainer.transform);

            // Title text centered with larger font
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(header.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "MILESTONES";
            titleText.fontSize = 62f; // Larger, bolder title
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.95f, 0.75f, 0.2f); // Rich gold
            ApplySharedFont(titleText);

            // Add shadow/outline for depth
            titleText.outlineWidth = 0.25f;
            titleText.outlineColor = new Color(0.4f, 0.25f, 0.05f);

            // Add glow effect behind title
            CreateGlowEffect(header.transform, new Color(1f, 0.85f, 0.3f), 0.85f);
        }

        private void CreateOverallProgressBar(Transform parent)
        {
            var progressContainer = new GameObject("OverallProgress");
            progressContainer.transform.SetParent(parent, false);

            var containerRect = progressContainer.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(0, 70);

            var containerLayout = progressContainer.AddComponent<LayoutElement>();
            containerLayout.preferredHeight = 70;
            containerLayout.flexibleHeight = 0;

            // Use horizontal frame for progress bar background
            var progressBg = progressContainer.AddComponent<Image>();
            if (guiAssets != null && guiAssets.horizontalFrame != null)
            {
                progressBg.sprite = guiAssets.horizontalFrame;
                progressBg.type = Image.Type.Sliced;
                progressBg.color = Color.white;
            }
            else
            {
                progressBg.color = new Color(0.15f, 0.12f, 0.2f, 0.9f);
            }

            // Progress fill bar - more rounded look
            var fillContainer = new GameObject("FillContainer");
            fillContainer.transform.SetParent(progressContainer.transform, false);
            var fillContainerRect = fillContainer.AddComponent<RectTransform>();
            fillContainerRect.anchorMin = new Vector2(0.025f, 0.18f);
            fillContainerRect.anchorMax = new Vector2(0.975f, 0.82f);
            fillContainerRect.offsetMin = Vector2.zero;
            fillContainerRect.offsetMax = Vector2.zero;

            var fillBgImg = fillContainer.AddComponent<Image>();
            fillBgImg.color = new Color(0.08f, 0.06f, 0.12f, 0.9f);

            // Actual fill with gradient-like appearance
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillContainer.transform, false);
            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1); // Will be animated
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            progressFill = fillObj.AddComponent<Image>();
            progressFill.color = new Color(1f, 0.75f, 0.2f); // Golden progress

            // Add shine overlay on fill for extra polish
            var shineObj = new GameObject("Shine");
            shineObj.transform.SetParent(fillObj.transform, false);
            var shineRect = shineObj.AddComponent<RectTransform>();
            shineRect.anchorMin = new Vector2(0, 0.5f);
            shineRect.anchorMax = new Vector2(1, 1);
            shineRect.offsetMin = Vector2.zero;
            shineRect.offsetMax = Vector2.zero;
            var shineImg = shineObj.AddComponent<Image>();
            shineImg.color = new Color(1f, 1f, 1f, 0.25f);

            // Progress text overlay - larger font
            var textObj = new GameObject("ProgressText");
            textObj.transform.SetParent(progressContainer.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            progressText = textObj.AddComponent<TextMeshProUGUI>();
            progressText.text = "0 / 0 Complete";
            progressText.fontSize = 34f; // Larger, more readable
            progressText.fontStyle = FontStyles.Bold;
            progressText.alignment = TextAlignmentOptions.Center;
            progressText.color = Color.white;
            ApplySharedFont(progressText);
        }

        private void CreateStarIcon(Transform parent)
        {
            // Outer glow
            var glow = new GameObject("Glow");
            glow.transform.SetParent(parent, false);
            var glowRect = glow.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.offsetMin = new Vector2(-15, -15);
            glowRect.offsetMax = new Vector2(15, 15);

            var glowImg = glow.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.85f, 0.2f, 0.4f);
            glowImg.DOFade(0.2f, UIDesignSystem.AnimGlow).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            // Star body - use iconStar if available
            var star = new GameObject("Star");
            star.transform.SetParent(parent, false);
            var starRect = star.AddComponent<RectTransform>();
            starRect.anchorMin = Vector2.zero;
            starRect.anchorMax = Vector2.one;
            starRect.offsetMin = Vector2.zero;
            starRect.offsetMax = Vector2.zero;

            var starImg = star.AddComponent<Image>();
            if (guiAssets != null && guiAssets.iconStar != null)
            {
                starImg.sprite = guiAssets.iconStar;
                starImg.color = Color.white;
            }
            else
            {
                starImg.color = UIDesignSystem.AccentMilestones;
            }

            // Subtle rotation animation
            parent.DORotate(new Vector3(0, 0, 5f), UIDesignSystem.AnimFloat)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void CreateGlowEffect(Transform parent, Color glowColor, float scale)
        {
            var glow = new GameObject("GlowEffect");
            glow.transform.SetParent(parent, false);
            glow.transform.SetAsFirstSibling();

            var glowRect = glow.AddComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.5f - scale * 0.4f, 0f);
            glowRect.anchorMax = new Vector2(0.5f + scale * 0.4f, 1f);
            glowRect.offsetMin = Vector2.zero;
            glowRect.offsetMax = Vector2.zero;

            var glowImg = glow.AddComponent<Image>();
            glowImg.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.2f);
            glowImg.DOFade(0.1f, UIDesignSystem.AnimPulse).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        private void CreateScrollView(Transform parent)
        {
            // Scroll view container - expands to fill available space
            var scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(parent, false);

            var scrollRectTransform = scrollObj.AddComponent<RectTransform>();
            scrollRectTransform.sizeDelta = new Vector2(0, 0);

            // Use layout element to expand
            var layoutElement = scrollObj.AddComponent<LayoutElement>();
            layoutElement.flexibleHeight = 100;
            layoutElement.minHeight = 100;

            scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 60f; // Smoother scrolling
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.08f;
            scrollRect.decelerationRate = 0.12f; // Smoother deceleration

            // Use listFrame for scroll background
            var scrollImage = scrollObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.listFrame != null)
            {
                scrollImage.sprite = guiAssets.listFrame;
                scrollImage.type = Image.Type.Sliced;
                scrollImage.color = new Color(1f, 1f, 1f, 0.98f);
            }
            else
            {
                scrollImage.color = new Color(0.06f, 0.04f, 0.1f, 0.95f);
            }

            var mask = scrollObj.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            // Viewport with better padding
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);

            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = new Vector2(0.015f, 0.02f);
            viewportRect.anchorMax = new Vector2(0.985f, 0.98f);
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
            contentLayout.padding = new RectOffset(12, 12, 16, 16); // Better padding
            contentLayout.spacing = 18f; // Good spacing between cards
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

            // Bigger close button - 70x70 for better touch target
            var closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 1);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.anchoredPosition = new Vector2(-12, -12);
            closeRect.sizeDelta = new Vector2(70, 70);

            var closeBg = closeObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonRed != null)
            {
                closeBg.sprite = guiAssets.buttonRed;
                closeBg.type = Image.Type.Sliced;
                closeBg.color = Color.white;
            }
            else
            {
                closeBg.color = new Color(0.85f, 0.25f, 0.25f); // Red background
            }

            var closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeBg;
            closeBtn.onClick.AddListener(HidePanel);

            var colors = closeBtn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            closeBtn.colors = colors;

            // Always use text X for consistent look across popups
            var closeTextObj = new GameObject("Text");
            closeTextObj.transform.SetParent(closeObj.transform, false);
            var closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.fontSize = 42f; // Large, visible X
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
            cardRect.sizeDelta = new Vector2(0, 220); // Card height to fit larger text

            // Card background using cardFrame
            var cardBg = card.AddComponent<Image>();
            if (guiAssets != null && guiAssets.cardFrame != null)
            {
                cardBg.sprite = guiAssets.cardFrame;
                cardBg.type = Image.Type.Sliced;
                // Tint based on state
                if (progress.isClaimed)
                {
                    cardBg.color = new Color(0.8f, 0.95f, 0.8f, 1f); // Claimed - subtle green tint
                }
                else if (progress.isCompleted)
                {
                    cardBg.color = new Color(1f, 0.97f, 0.88f, 1f); // Ready to claim - warm gold tint
                }
                else
                {
                    cardBg.color = Color.white; // In progress - normal
                }
            }
            else
            {
                if (progress.isClaimed)
                {
                    cardBg.color = new Color(0.12f, 0.22f, 0.12f, 0.95f);
                }
                else if (progress.isCompleted)
                {
                    cardBg.color = new Color(0.22f, 0.18f, 0.08f, 0.95f);
                }
                else
                {
                    cardBg.color = UIDesignSystem.PanelMedium;
                }
            }

            // Add subtle border glow for claimable
            if (progress.isCompleted && !progress.isClaimed)
            {
                var borderGlow = new GameObject("BorderGlow");
                borderGlow.transform.SetParent(card.transform, false);
                borderGlow.transform.SetAsFirstSibling();
                var borderRect = borderGlow.AddComponent<RectTransform>();
                borderRect.anchorMin = Vector2.zero;
                borderRect.anchorMax = Vector2.one;
                borderRect.offsetMin = new Vector2(-4, -4);
                borderRect.offsetMax = new Vector2(4, 4);
                var borderImg = borderGlow.AddComponent<Image>();
                borderImg.color = new Color(1f, 0.85f, 0.3f, 0.7f);
                borderImg.DOFade(0.3f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }

            // === LEFT: Icon container (fixed width) ===
            var iconContainer = new GameObject("IconContainer");
            iconContainer.transform.SetParent(card.transform, false);
            var iconContainerRect = iconContainer.AddComponent<RectTransform>();
            iconContainerRect.anchorMin = new Vector2(0, 0.1f);
            iconContainerRect.anchorMax = new Vector2(0, 0.9f);
            iconContainerRect.pivot = new Vector2(0, 0.5f);
            iconContainerRect.anchoredPosition = new Vector2(18, 0);
            iconContainerRect.sizeDelta = new Vector2(90, 0);

            // Glow for claimable milestones
            if (progress.isCompleted && !progress.isClaimed)
            {
                var glow = new GameObject("Glow");
                glow.transform.SetParent(iconContainer.transform, false);
                var glowRect = glow.AddComponent<RectTransform>();
                glowRect.anchorMin = Vector2.zero;
                glowRect.anchorMax = Vector2.one;
                glowRect.offsetMin = new Vector2(-14, -14);
                glowRect.offsetMax = new Vector2(14, 14);

                var glowImg = glow.AddComponent<Image>();
                glowImg.color = new Color(milestone.accentColor.r, milestone.accentColor.g, milestone.accentColor.b, 0.5f);
                glowImg.DOFade(0.2f, UIDesignSystem.AnimPulse).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

                // Scale pulse for extra juice
                iconContainer.transform.DOScale(1.06f, UIDesignSystem.AnimPulse).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }

            // Icon
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(iconContainer.transform, false);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            var iconImage = iconObj.AddComponent<Image>();
            if (milestone.icon != null)
            {
                iconImage.sprite = milestone.icon;
                iconImage.preserveAspect = true;
                iconImage.color = progress.isClaimed ? new Color(0.65f, 0.65f, 0.65f) : Color.white;
            }
            else
            {
                iconImage.color = milestone.accentColor;
                if (progress.isClaimed)
                {
                    iconImage.color = new Color(milestone.accentColor.r * 0.5f, milestone.accentColor.g * 0.5f, milestone.accentColor.b * 0.5f);
                }
            }

            // Checkmark badge for claimed - larger and more visible
            if (progress.isClaimed)
            {
                var checkBadge = new GameObject("CheckBadge");
                checkBadge.transform.SetParent(iconContainer.transform, false);
                var checkBadgeRect = checkBadge.AddComponent<RectTransform>();
                checkBadgeRect.anchorMin = new Vector2(0.5f, -0.1f);
                checkBadgeRect.anchorMax = new Vector2(1.15f, 0.5f);
                checkBadgeRect.offsetMin = Vector2.zero;
                checkBadgeRect.offsetMax = Vector2.zero;

                // Badge background - circular green
                var checkBadgeBg = checkBadge.AddComponent<Image>();
                checkBadgeBg.color = new Color(0.2f, 0.8f, 0.3f); // Bright green

                // Try to use checkmark icon from GUI assets
                if (guiAssets != null && guiAssets.iconCheck != null)
                {
                    var checkIcon = new GameObject("CheckIcon");
                    checkIcon.transform.SetParent(checkBadge.transform, false);
                    var checkIconRect = checkIcon.AddComponent<RectTransform>();
                    checkIconRect.anchorMin = new Vector2(0.15f, 0.15f);
                    checkIconRect.anchorMax = new Vector2(0.85f, 0.85f);
                    checkIconRect.offsetMin = Vector2.zero;
                    checkIconRect.offsetMax = Vector2.zero;

                    var checkIconImg = checkIcon.AddComponent<Image>();
                    checkIconImg.sprite = guiAssets.iconCheck;
                    checkIconImg.color = Color.white;
                    checkIconImg.preserveAspect = true;
                }
                else
                {
                    // Fallback to text checkmark
                    var checkText = new GameObject("CheckText");
                    checkText.transform.SetParent(checkBadge.transform, false);
                    var checkTextRect = checkText.AddComponent<RectTransform>();
                    checkTextRect.anchorMin = Vector2.zero;
                    checkTextRect.anchorMax = Vector2.one;
                    checkTextRect.offsetMin = Vector2.zero;
                    checkTextRect.offsetMax = Vector2.zero;

                    var checkTmp = checkText.AddComponent<TextMeshProUGUI>();
                    checkTmp.text = "✓";
                    checkTmp.fontSize = 32;
                    checkTmp.fontStyle = FontStyles.Bold;
                    checkTmp.alignment = TextAlignmentOptions.Center;
                    checkTmp.color = Color.white;
                    ApplySharedFont(checkTmp);
                }
            }

            // === RIGHT: Claim button (fixed width, positioned properly - moved more to left) ===
            float rightPadding = 35f;
            float buttonWidth = 100f;
            if (progress.isCompleted && !progress.isClaimed)
            {
                CreateClaimButton(card.transform, milestone.milestoneId, buttonWidth, rightPadding);
            }

            // Calculate right offset for info section
            float infoRightOffset = rightPadding + (progress.isCompleted && !progress.isClaimed ? buttonWidth + 16f : 20f);

            // === CENTER: Info section using DIRECT ANCHOR POSITIONING (no layout group) ===
            var infoObj = new GameObject("Info");
            infoObj.transform.SetParent(card.transform, false);
            var infoRect = infoObj.AddComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0, 0);
            infoRect.anchorMax = new Vector2(1, 1);
            infoRect.offsetMin = new Vector2(120, 10); // Left offset (icon width + padding)
            infoRect.offsetMax = new Vector2(-infoRightOffset, -10); // Right offset

            // Title - positioned at top of info section
            var nameText = CreateAnchoredText(infoObj.transform, milestone.displayName,
                32f, progress.isClaimed ? new Color(0.4f, 0.4f, 0.4f) : new Color(0.15f, 0.15f, 0.2f),
                0f, 1f, 1f, 1f, 0, -4, 0, -40); // Top section
            nameText.fontStyle = FontStyles.Bold;

            // Description - positioned below title, larger area for bigger text
            var descText = CreateAnchoredText(infoObj.transform, milestone.description,
                48f, Color.white,
                0f, 0.30f, 1f, 0.75f, 0, 0, 0, 0); // Middle section - expanded
            descText.enableWordWrapping = true;
            descText.overflowMode = TextOverflowModes.Overflow;

            // Reward preview - positioned below description
            string rewardText = GetRewardPreviewText(milestone);
            var rewardLabel = CreateAnchoredText(infoObj.transform, rewardText,
                26f, new Color(0.85f, 0.55f, 0.1f),
                0f, 0.22f, 1f, 0.45f, 0, 0, 0, 0); // Lower-middle section
            rewardLabel.fontStyle = FontStyles.Bold;

            // Progress bar or claimed status - at bottom
            if (!progress.isClaimed)
            {
                CreateAnchoredProgressBar(infoObj.transform, progress, milestone);
            }
            else
            {
                var claimedLabel = CreateAnchoredText(infoObj.transform, "CLAIMED",
                    24f, new Color(0.2f, 0.65f, 0.2f),
                    0f, 0f, 1f, 0.22f, 0, 4, 0, 0);
                claimedLabel.fontStyle = FontStyles.Bold;
            }

            return card;
        }

        /// <summary>
        /// Creates a text element with explicit anchor-based positioning (no layout group dependency).
        /// </summary>
        private TextMeshProUGUI CreateAnchoredText(Transform parent, string text, float fontSize, Color color,
            float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY,
            float offsetMinX, float offsetMinY, float offsetMaxX, float offsetMaxY)
        {
            string safeName = (text != null && text.Length > 10) ? text.Substring(0, 10) : (text ?? "Text");
            var obj = new GameObject("Text_" + safeName.Replace(" ", "_"));
            obj.transform.SetParent(parent, false);

            // RectTransform with explicit anchors - this is the key to reliable text positioning
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(anchorMinX, anchorMinY);
            rect.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
            rect.offsetMin = new Vector2(offsetMinX, offsetMinY);
            rect.offsetMax = new Vector2(offsetMaxX, offsetMaxY);

            // TextMeshPro component
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text ?? "";
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = color;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;

            // Apply font
            ApplySharedFont(tmp);

            return tmp;
        }

        /// <summary>
        /// Creates a progress bar with explicit anchor-based positioning.
        /// </summary>
        private void CreateAnchoredProgressBar(Transform parent, MilestoneProgress progress, MilestoneDefinition milestone)
        {
            var barContainer = new GameObject("ProgressBar");
            barContainer.transform.SetParent(parent, false);

            var containerRect = barContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 0);
            containerRect.anchorMax = new Vector2(1, 0.22f);
            containerRect.offsetMin = new Vector2(0, 4);
            containerRect.offsetMax = new Vector2(0, 0);

            // Background
            var barBg = barContainer.AddComponent<Image>();
            barBg.color = new Color(0.12f, 0.1f, 0.18f, 0.95f);

            // Fill
            float progressPercent = Mathf.Clamp01((float)(progress.currentProgress / milestone.targetAmount));

            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(barContainer.transform, false);
            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0.01f, 0.1f);
            fillRect.anchorMax = new Vector2(0.01f + progressPercent * 0.98f, 0.9f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            var fillImage = fillObj.AddComponent<Image>();
            fillImage.color = progress.isCompleted
                ? new Color(0.35f, 0.9f, 0.35f)
                : new Color(milestone.accentColor.r, milestone.accentColor.g, milestone.accentColor.b, 1f);

            // Progress text
            var progressTextObj = new GameObject("ProgressText");
            progressTextObj.transform.SetParent(barContainer.transform, false);
            var pTextRect = progressTextObj.AddComponent<RectTransform>();
            pTextRect.anchorMin = Vector2.zero;
            pTextRect.anchorMax = Vector2.one;
            pTextRect.offsetMin = Vector2.zero;
            pTextRect.offsetMax = Vector2.zero;

            var pText = progressTextObj.AddComponent<TextMeshProUGUI>();
            pText.text = progress.isCompleted
                ? "COMPLETE!"
                : $"{GameUI.FormatNumber(progress.currentProgress)} / {GameUI.FormatNumber(milestone.targetAmount)}";
            pText.fontSize = 22f;
            pText.fontStyle = FontStyles.Bold;
            pText.alignment = TextAlignmentOptions.Center;
            pText.color = Color.white;
            ApplySharedFont(pText);
        }

        private string GetRewardPreviewText(MilestoneDefinition milestone)
        {
            if (milestone.rewards == null || milestone.rewards.Length == 0)
                return "";

            var reward = milestone.rewards[0];
            string rewardName = reward.type switch
            {
                MilestoneRewardType.TimeShards => "Time Shards",
                MilestoneRewardType.DarkMatter => "Dark Matter",
                MilestoneRewardType.Money => "Money",
                MilestoneRewardType.PermanentMoneyBoost => "Money Boost",
                MilestoneRewardType.PermanentDMBoost => "DM Boost",
                MilestoneRewardType.UnlockFeature => "Unlock",
                _ => "Reward"
            };

            if (reward.type == MilestoneRewardType.PermanentMoneyBoost || reward.type == MilestoneRewardType.PermanentDMBoost)
            {
                return $"Reward: +{reward.amount}% {rewardName}";
            }
            return $"Reward: {GameUI.FormatNumber(reward.amount)} {rewardName}";
        }

        private void CreateClaimButton(Transform parent, string milestoneId, float buttonWidth = 100f, float rightPadding = 35f)
        {
            var claimObj = new GameObject("ClaimButton");
            claimObj.transform.SetParent(parent, false);

            // Position on the RIGHT side of the card using anchors - moved more to the left
            var claimRect = claimObj.AddComponent<RectTransform>();
            claimRect.anchorMin = new Vector2(1, 0.18f);
            claimRect.anchorMax = new Vector2(1, 0.82f);
            claimRect.pivot = new Vector2(1, 0.5f);
            claimRect.anchoredPosition = new Vector2(-rightPadding, 0);
            claimRect.sizeDelta = new Vector2(buttonWidth, 0);

            // Add Image for button background
            var claimBg = claimObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonGreen != null)
            {
                claimBg.sprite = guiAssets.buttonGreen;
                claimBg.type = Image.Type.Sliced;
                claimBg.color = Color.white;
            }
            else
            {
                claimBg.color = UIDesignSystem.ButtonPrimary;
            }

            var claimBtn = claimObj.AddComponent<Button>();
            claimBtn.targetGraphic = claimBg;

            var colors = claimBtn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor = Color.white;
            claimBtn.colors = colors;

            claimBtn.onClick.AddListener(() => OnClaimClicked(milestoneId));

            // Button text - larger, more readable
            var claimTextObj = new GameObject("Text");
            claimTextObj.transform.SetParent(claimObj.transform, false);
            var claimText = claimTextObj.AddComponent<TextMeshProUGUI>();
            claimText.text = "CLAIM";
            claimText.fontSize = 32f; // Larger font
            claimText.fontStyle = FontStyles.Bold;
            claimText.alignment = TextAlignmentOptions.Center;
            claimText.color = Color.white;
            ApplySharedFont(claimText);

            var claimTextRect = claimTextObj.GetComponent<RectTransform>();
            claimTextRect.anchorMin = Vector2.zero;
            claimTextRect.anchorMax = Vector2.one;
            claimTextRect.offsetMin = Vector2.zero;
            claimTextRect.offsetMax = Vector2.zero;

            // Gentle pulse animation - not too aggressive
            Sequence pulseSeq = DOTween.Sequence();
            pulseSeq.Append(claimObj.transform.DOScale(1.05f, 0.5f).SetEase(Ease.InOutSine));
            pulseSeq.Append(claimObj.transform.DOScale(1f, 0.5f).SetEase(Ease.InOutSine));
            pulseSeq.SetLoops(-1);

            // Add subtle glow behind button
            var glowObj = new GameObject("ButtonGlow");
            glowObj.transform.SetParent(claimObj.transform, false);
            glowObj.transform.SetAsFirstSibling();
            var glowRect = glowObj.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.offsetMin = new Vector2(-6, -6);
            glowRect.offsetMax = new Vector2(6, 6);

            var glowImg = glowObj.AddComponent<Image>();
            glowImg.color = new Color(0.3f, 0.9f, 0.3f, 0.45f);
            glowImg.DOFade(0.15f, 0.7f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        private void ApplySharedFont(TextMeshProUGUI text)
        {
            if (text == null) return;

            // Try to get shared font, fall back to TMP default if not available
            TMP_FontAsset fontToUse = null;

            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                fontToUse = GameUI.Instance.SharedFont;
            }
            else if (TMP_Settings.defaultFontAsset != null)
            {
                fontToUse = TMP_Settings.defaultFontAsset;
            }

            if (fontToUse != null)
            {
                text.font = fontToUse;
            }

            // Ensure text is visible - set alpha to 1
            Color c = text.color;
            c.a = 1f;
            text.color = c;
        }

        /// <summary>
        /// Applies the shared font to ALL text elements in the panel.
        /// Called when showing panel to ensure fonts are applied even if GameUI loaded later.
        /// </summary>
        private void ApplySharedFontToAllText()
        {
            if (panelRoot == null) return;

            // Try to get shared font, fall back to TMP default if not available
            TMP_FontAsset sharedFont = null;

            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                sharedFont = GameUI.Instance.SharedFont;
            }
            else if (TMP_Settings.defaultFontAsset != null)
            {
                sharedFont = TMP_Settings.defaultFontAsset;
            }

            if (sharedFont == null) return;

            // Get all TextMeshProUGUI components in the panel (including inactive)
            TextMeshProUGUI[] allTexts = panelRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTexts)
            {
                if (tmp != null)
                {
                    tmp.font = sharedFont;
                    GameUI.ApplyTextOutline(tmp);
                }
            }
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
                // Play claim feedback (haptics + effects)
                PlayClaimFeedback();

                // Find the card for this milestone to animate it
                foreach (var card in milestoneCards)
                {
                    if (card != null && card.name == $"Milestone_{milestoneId}")
                    {
                        // Enhanced success animation sequence
                        Sequence claimSeq = DOTween.Sequence();

                        // Quick scale punch
                        claimSeq.Append(card.transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad));
                        claimSeq.Append(card.transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack));

                        // Flash effect
                        var cardImage = card.GetComponent<Image>();
                        if (cardImage != null)
                        {
                            Color originalColor = cardImage.color;
                            claimSeq.Join(cardImage.DOColor(Color.white, 0.1f).SetLoops(2, LoopType.Yoyo));
                        }

                        // Play audio
                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.PlayJackpotSound();
                        }

                        break;
                    }
                }

                MilestoneManager.Instance.ClaimMilestone(milestoneId);
            }
        }

        #endregion

        #region Public API

        public void ShowPanel()
        {
            RefreshMilestoneList();

            // Apply shared font to all text elements (in case GameUI wasn't ready at Start)
            ApplySharedFontToAllText();

            panelRoot.SetActive(true);

            // Ensure popup is rendered on top of other UI elements (like menu button)
            panelRoot.transform.SetAsLastSibling();

            // Kill any existing animations
            DOTween.Kill(panelRoot.transform);
            DOTween.Kill(mainPanel.transform);

            // Animate background overlay
            var bgImage = panelRoot.GetComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0);
            bgImage.DOFade(0.85f, UIDesignSystem.AnimFadeIn).SetEase(UIDesignSystem.EaseFade);

            // Animate main panel with scale and fade
            mainPanel.transform.localScale = Vector3.one * 0.85f;
            if (mainPanelCanvasGroup != null)
            {
                mainPanelCanvasGroup.alpha = 0f;
                mainPanelCanvasGroup.DOFade(1f, UIDesignSystem.AnimFadeIn).SetEase(UIDesignSystem.EaseFade);
            }
            mainPanel.transform.DOScale(1f, UIDesignSystem.AnimSlideIn).SetEase(UIDesignSystem.EasePopIn);

            // Apply button polish for press/release animations
            if (UI.UIPolishManager.Instance != null)
            {
                UI.UIPolishManager.Instance.PolishButtonsInPanel(panelRoot);
            }

            // Register with PopupManager
            if (PopupManager.Instance != null)
                PopupManager.Instance.RegisterPopupOpen("MilestonesUI");
        }

        public void HidePanel()
        {
            if (!panelRoot.activeSelf) return;

            // Unregister from PopupManager
            if (PopupManager.Instance != null)
                PopupManager.Instance.RegisterPopupClosed("MilestonesUI");

            // Kill any existing animations
            DOTween.Kill(panelRoot.transform);
            DOTween.Kill(mainPanel.transform);

            // Animate out
            panelRoot.GetComponent<Image>().DOFade(0f, UIDesignSystem.AnimFadeOut).SetEase(UIDesignSystem.EaseFade);

            Sequence hideSeq = DOTween.Sequence();
            if (mainPanelCanvasGroup != null)
            {
                hideSeq.Join(mainPanelCanvasGroup.DOFade(0f, UIDesignSystem.AnimFadeOut).SetEase(UIDesignSystem.EaseFade));
            }
            hideSeq.Join(mainPanel.transform.DOScale(0.85f, UIDesignSystem.AnimSlideOut).SetEase(UIDesignSystem.EasePopOut));
            hideSeq.OnComplete(() => panelRoot.SetActive(false));
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
            int claimed = 0;

            foreach (var milestone in milestones)
            {
                var progress = MilestoneManager.Instance.GetProgress(milestone.milestoneId);
                if (progress == null) continue;

                // Update progress value
                progress.currentProgress = MilestoneManager.Instance.GetCurrentValue(milestone.milestoneType);

                var card = CreateMilestoneCard(milestone, progress);
                milestoneCards.Add(card);

                if (progress.isCompleted) completed++;
                if (progress.isClaimed) claimed++;
            }

            // Update progress text
            progressText.text = $"{claimed} / {milestones.Count} Complete";

            // Update overall progress bar
            if (progressFill != null && milestones.Count > 0)
            {
                float progressPercent = (float)claimed / milestones.Count;
                var fillRect = progressFill.GetComponent<RectTransform>();
                fillRect.DOKill();
                fillRect.DOAnchorMax(new Vector2(progressPercent, 1f), UIDesignSystem.AnimSlideIn).SetEase(Ease.OutCubic);
            }
        }

        private void UpdateHudBadge()
        {
            // HUD badge is disabled - we use the main menu notification badges instead
        }

        #endregion
    }
}
