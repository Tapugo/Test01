using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.DailyLogin;
using Incredicer.Missions;
using Incredicer.Overclock;
using Incredicer.TimeFracture;
using Incredicer.Milestones;
using Incredicer.GlobalEvents;
using Incredicer.Leaderboards;

namespace Incredicer.UI
{
    /// <summary>
    /// Main navigation menu providing access to all game features.
    /// Creates a side menu with buttons for all Phase 11-17 systems.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        public static MainMenuUI Instance { get; private set; }

        // Button sizes - using UIDesignSystem constants
        [System.NonSerialized] private float buttonWidth;
        [System.NonSerialized] private float buttonHeight;
        [System.NonSerialized] private float buttonSpacing;

        // UI References
        private GameObject menuPanel;
        private GameObject menuToggleButton;
        private GameObject clickOutsideBlocker;
        private CanvasGroup menuCanvasGroup;
        private bool isMenuOpen = false;

        // Feature buttons
        private Button dailyLoginBtn;
        private Button missionsBtn;
        private Button overclockBtn;
        private Button timeFractureBtn;
        private Button milestonesBtn;
        private Button globalEventBtn;
        private Button leaderboardBtn;

        // Notification badges
        private GameObject dailyLoginBadge;
        private GameObject missionsBadge;
        private GameObject milestoneBadge;
        private GameObject eventBadge;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Use UIDesignSystem constants for consistent sizing
            buttonWidth = 240f;
            buttonHeight = UIDesignSystem.ButtonHeightMedium;  // 56px - ideal touch target
            buttonSpacing = UIDesignSystem.SpacingS;            // 8px spacing
        }

        private void Start()
        {
            CreateUI();
            UpdateNotificationBadges();

            // Subscribe to events for badge updates
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void CreateUI()
        {
            // Create click-outside blocker (fullscreen invisible button)
            CreateClickOutsideBlocker();

            // Create menu toggle button (hamburger icon on left side)
            CreateMenuToggleButton();

            // Create slide-out menu panel
            CreateMenuPanel();

            // Create all feature buttons
            CreateFeatureButtons();

            // Initially hide the menu and blocker
            menuPanel.SetActive(false);
            clickOutsideBlocker.SetActive(false);
        }

        private void CreateClickOutsideBlocker()
        {
            clickOutsideBlocker = new GameObject("ClickOutsideBlocker");
            clickOutsideBlocker.transform.SetParent(transform, false);

            RectTransform rt = clickOutsideBlocker.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = clickOutsideBlocker.AddComponent<Image>();
            bg.color = UIDesignSystem.OverlayMedium;  // 70% dark overlay

            Button btn = clickOutsideBlocker.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(CloseMenu);

            // Make sure colors stay consistent
            var colors = btn.colors;
            colors.highlightedColor = UIDesignSystem.OverlayMedium;
            colors.pressedColor = UIDesignSystem.OverlayDark;
            colors.normalColor = UIDesignSystem.OverlayMedium;
            btn.colors = colors;
        }

        private void CreateMenuToggleButton()
        {
            menuToggleButton = new GameObject("MenuToggleButton");
            menuToggleButton.transform.SetParent(transform, false);

            RectTransform rt = menuToggleButton.AddComponent<RectTransform>();
            // Position in top-right, below currency display - proper touch target size
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-UIDesignSystem.SafeAreaPadding, -220);
            rt.sizeDelta = new Vector2(90, 90);  // Good touch target size

            Image bg = menuToggleButton.AddComponent<Image>();
            bg.color = UIDesignSystem.AccentPurple;

            Button btn = menuToggleButton.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(ToggleMenu);

            // Set button colors
            var colors = btn.colors;
            colors.highlightedColor = UIDesignSystem.AccentPurple * 1.2f;
            colors.pressedColor = UIDesignSystem.AccentPurple * 0.7f;
            btn.colors = colors;

            // Menu text label
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(menuToggleButton.transform, false);

            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            iconRt.anchorMin = Vector2.zero;
            iconRt.anchorMax = Vector2.one;
            iconRt.offsetMin = new Vector2(5, 5);
            iconRt.offsetMax = new Vector2(-5, -5);

            TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
            iconText.text = "☰\nMENU";
            iconText.fontSize = UIDesignSystem.FontSizeSmall;  // 20px
            iconText.fontStyle = FontStyles.Bold;
            iconText.alignment = TextAlignmentOptions.Center;
            iconText.color = Color.white;

            // Apply shared font if available
            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                iconText.font = GameUI.Instance.SharedFont;
            }

            // Add outline for visibility
            Outline outline = menuToggleButton.AddComponent<Outline>();
            outline.effectColor = UIDesignSystem.AccentPurple * 0.6f;
            outline.effectDistance = new Vector2(2, 2);
        }

        private void CreateMenuPanel()
        {
            menuPanel = new GameObject("MenuPanel");
            menuPanel.transform.SetParent(transform, false);

            RectTransform rt = menuPanel.AddComponent<RectTransform>();
            // Anchor to right side of screen
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 0.5f);
            rt.anchoredPosition = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(buttonWidth + 40, 0);

            Image bg = menuPanel.AddComponent<Image>();
            bg.color = UIDesignSystem.PanelBgDark;

            menuCanvasGroup = menuPanel.AddComponent<CanvasGroup>();

            // Add outline on left edge
            Outline outline = menuPanel.AddComponent<Outline>();
            outline.effectColor = UIDesignSystem.AccentPurple * 0.5f;
            outline.effectDistance = new Vector2(-3, 0);
        }

        private void CreateFeatureButtons()
        {
            float startY = -80; // Start below the toggle button
            int index = 0;

            // Daily Login - using UIDesignSystem colors
            dailyLoginBtn = CreateMenuButton("Daily Login", "📅", startY - (buttonHeight + buttonSpacing) * index++,
                UIDesignSystem.ButtonSuccess, OnDailyLoginClicked);
            dailyLoginBadge = CreateNotificationBadge(dailyLoginBtn.gameObject);

            // Missions
            missionsBtn = CreateMenuButton("Missions", "📋", startY - (buttonHeight + buttonSpacing) * index++,
                UIDesignSystem.AccentOrange, OnMissionsClicked);
            missionsBadge = CreateNotificationBadge(missionsBtn.gameObject);

            // Overclock
            overclockBtn = CreateMenuButton("Overclock", "🔥", startY - (buttonHeight + buttonSpacing) * index++,
                UIDesignSystem.OverclockOrange, OnOverclockClicked);

            // Time Fracture
            timeFractureBtn = CreateMenuButton("Time Fracture", "⏱", startY - (buttonHeight + buttonSpacing) * index++,
                UIDesignSystem.TimeShardsBlue, OnTimeFractureClicked);

            // Milestones
            milestonesBtn = CreateMenuButton("Milestones", "🏆", startY - (buttonHeight + buttonSpacing) * index++,
                UIDesignSystem.AccentGold, OnMilestonesClicked);
            milestoneBadge = CreateNotificationBadge(milestonesBtn.gameObject);

            // Global Event
            globalEventBtn = CreateMenuButton("Events", "🌍", startY - (buttonHeight + buttonSpacing) * index++,
                UIDesignSystem.EventBlue, OnGlobalEventClicked);
            eventBadge = CreateNotificationBadge(globalEventBtn.gameObject);

            // Leaderboard
            leaderboardBtn = CreateMenuButton("Leaderboard", "🏅", startY - (buttonHeight + buttonSpacing) * index++,
                UIDesignSystem.LeaderboardGold, OnLeaderboardClicked);
        }

        private Button CreateMenuButton(string label, string icon, float yPos, Color accentColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject($"Btn_{label.Replace(" ", "")}");
            btnObj.transform.SetParent(menuPanel.transform, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, yPos);
            rt.sizeDelta = new Vector2(-UIDesignSystem.SpacingM, buttonHeight);

            // Use accent color as button background (matching game style)
            Image bg = btnObj.AddComponent<Image>();
            bg.color = accentColor;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(onClick);
            btn.onClick.AddListener(() => AudioManager.Instance?.PlayButtonClickSound());

            // Button hover/press colors
            var colors = btn.colors;
            colors.highlightedColor = accentColor * 1.2f;
            colors.pressedColor = accentColor * 0.7f;
            btn.colors = colors;

            // Add outline for depth
            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = UIDesignSystem.ShadowColor;
            outline.effectDistance = new Vector2(2, -2);

            // Label (centered, using design system font size)
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btnObj.transform, false);

            RectTransform labelRt = labelObj.AddComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(UIDesignSystem.SpacingS, 5);
            labelRt.offsetMax = new Vector2(-UIDesignSystem.SpacingS, -5);

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label.ToUpper();
            labelText.fontSize = UIDesignSystem.FontSizeSmall;  // 20px - readable
            labelText.fontStyle = FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;

            // Apply font if available
            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                labelText.font = GameUI.Instance.SharedFont;
            }

            return btn;
        }

        private GameObject CreateNotificationBadge(GameObject parent)
        {
            GameObject badge = new GameObject("Badge");
            badge.transform.SetParent(parent.transform, false);

            RectTransform rt = badge.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-3, -3);
            rt.sizeDelta = new Vector2(20, 20);  // Visible badge size

            Image bg = badge.AddComponent<Image>();
            bg.color = UIDesignSystem.StateError;

            // Make it round
            badge.AddComponent<Mask>().showMaskGraphic = true;

            // Exclamation mark
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(badge.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "!";
            text.fontSize = UIDesignSystem.FontSizeLabel;  // 14px
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            badge.SetActive(false);
            return badge;
        }

        #region Button Click Handlers

        private void OnDailyLoginClicked()
        {
            var ui = FindObjectOfType<DailyLoginUI>();
            if (ui != null) ui.Toggle();
            else Debug.LogWarning("[MainMenuUI] DailyLoginUI not found");
            CloseMenu();
        }

        private void OnMissionsClicked()
        {
            var ui = FindObjectOfType<MissionsUI>();
            if (ui != null) ui.Toggle();
            else Debug.LogWarning("[MainMenuUI] MissionsUI not found");
            CloseMenu();
        }

        private void OnOverclockClicked()
        {
            var ui = FindObjectOfType<OverclockUI>();
            if (ui != null) ui.Toggle();
            else Debug.LogWarning("[MainMenuUI] OverclockUI not found");
            CloseMenu();
        }

        private void OnTimeFractureClicked()
        {
            var ui = FindObjectOfType<TimeFractureUI>();
            if (ui != null) ui.Toggle();
            else Debug.LogWarning("[MainMenuUI] TimeFractureUI not found");
            CloseMenu();
        }

        private void OnMilestonesClicked()
        {
            var ui = FindObjectOfType<MilestonesUI>();
            if (ui != null) ui.Toggle();
            else Debug.LogWarning("[MainMenuUI] MilestonesUI not found");
            CloseMenu();
        }

        private void OnGlobalEventClicked()
        {
            var ui = FindObjectOfType<GlobalEventUI>();
            if (ui != null) ui.Toggle();
            else Debug.LogWarning("[MainMenuUI] GlobalEventUI not found");
            CloseMenu();
        }

        private void OnLeaderboardClicked()
        {
            var ui = FindObjectOfType<LeaderboardUI>();
            if (ui != null) ui.Toggle();
            else Debug.LogWarning("[MainMenuUI] LeaderboardUI not found");
            CloseMenu();
        }

        #endregion

        #region Menu Toggle

        public void ToggleMenu()
        {
            if (isMenuOpen) CloseMenu();
            else OpenMenu();
        }

        public void OpenMenu()
        {
            if (isMenuOpen) return;
            isMenuOpen = true;

            // Show blocker behind menu
            clickOutsideBlocker.SetActive(true);
            menuPanel.SetActive(true);
            menuCanvasGroup.alpha = 0;

            // Make sure menu is on top of blocker
            menuPanel.transform.SetAsLastSibling();

            RectTransform rt = menuPanel.GetComponent<RectTransform>();
            // Start off-screen to the right
            rt.anchoredPosition = new Vector2(buttonWidth + 50, 0);

            // Animate in from right using UIDesignSystem timing
            Sequence seq = DOTween.Sequence();
            seq.Append(rt.DOAnchorPosX(0, UIDesignSystem.AnimSlideIn).SetEase(Ease.OutBack));
            seq.Join(menuCanvasGroup.DOFade(1, UIDesignSystem.AnimFadeIn));

            // Scale up the menu button
            menuToggleButton.transform.DOScale(1.1f, UIDesignSystem.AnimFadeIn);

            UpdateNotificationBadges();
        }

        public void CloseMenu()
        {
            if (!isMenuOpen) return;
            isMenuOpen = false;

            RectTransform rt = menuPanel.GetComponent<RectTransform>();

            // Animate out to the right using UIDesignSystem timing
            Sequence seq = DOTween.Sequence();
            seq.Append(rt.DOAnchorPosX(buttonWidth + 50, UIDesignSystem.AnimSlideOut).SetEase(Ease.InBack));
            seq.Join(menuCanvasGroup.DOFade(0, UIDesignSystem.AnimFadeOut));
            seq.OnComplete(() =>
            {
                menuPanel.SetActive(false);
                clickOutsideBlocker.SetActive(false);
            });

            // Scale button back
            menuToggleButton.transform.DOScale(1f, UIDesignSystem.AnimFadeIn);
        }

        #endregion

        #region Notification Badges

        private void SubscribeToEvents()
        {
            if (DailyLoginManager.Instance != null)
            {
                DailyLoginManager.Instance.OnDailyRewardClaimed += (_) => UpdateNotificationBadges();
                DailyLoginManager.Instance.OnDailyRewardAvailable += () => UpdateNotificationBadges();
            }

            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionCompleted += (_) => UpdateNotificationBadges();
                MissionManager.Instance.OnMissionClaimed += (_) => UpdateNotificationBadges();
            }

            if (MilestoneManager.Instance != null)
            {
                MilestoneManager.Instance.OnMilestoneCompleted += (_) => UpdateNotificationBadges();
                MilestoneManager.Instance.OnMilestoneClaimed += (_, _) => UpdateNotificationBadges();
            }

            if (GlobalEventManager.Instance != null)
            {
                GlobalEventManager.Instance.OnTierReached += (_, _) => UpdateNotificationBadges();
                GlobalEventManager.Instance.OnTierClaimed += (_, _) => UpdateNotificationBadges();
            }
        }

        private void UnsubscribeFromEvents()
        {
            // Events will be cleaned up when managers are destroyed
        }

        public void UpdateNotificationBadges()
        {
            // Daily Login - show if can claim reward
            if (dailyLoginBadge != null && DailyLoginManager.Instance != null)
            {
                bool canClaim = DailyLoginManager.Instance.CanClaimReward();
                dailyLoginBadge.SetActive(canClaim);
            }

            // Missions - show if any mission is complete but not claimed
            if (missionsBadge != null && MissionManager.Instance != null)
            {
                bool hasClaimable = MissionManager.Instance.HasClaimableMissions();
                missionsBadge.SetActive(hasClaimable);
            }

            // Milestones - show if any milestone is complete but not claimed
            if (milestoneBadge != null && MilestoneManager.Instance != null)
            {
                bool hasClaimable = MilestoneManager.Instance.HasClaimableMilestones();
                milestoneBadge.SetActive(hasClaimable);
            }

            // Global Event - show if any tier is claimable
            if (eventBadge != null && GlobalEventManager.Instance != null)
            {
                int claimableTier = GlobalEventManager.Instance.GetNextClaimableTier();
                eventBadge.SetActive(claimableTier >= 0);
            }
        }

        #endregion
    }
}
