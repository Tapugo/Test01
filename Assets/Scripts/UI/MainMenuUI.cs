using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.DailyLogin;
using Incredicer.Missions;
using Incredicer.Overclock;
using Incredicer.Skills;
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

        // Public property for external checks
        public bool IsMenuOpen => isMenuOpen;

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
        private GameObject dailyLoginLockBadge;
        private TextMeshProUGUI dailyLoginLabelText;
        private GameObject missionsBadge;
        private GameObject missionsLockBadge;
        private TextMeshProUGUI missionsLabelText;
        private GameObject milestoneBadge;
        private GameObject milestonesLockBadge;
        private TextMeshProUGUI milestonesLabelText;
        private GameObject eventBadge;
        private GameObject eventsLockBadge;
        private TextMeshProUGUI eventsLabelText;
        private GameObject overclockLockBadge;
        private TextMeshProUGUI overclockLabelText;
        private GameObject timeFractureLockBadge;
        private TextMeshProUGUI timeFractureLabelText;
        private GameObject leaderboardLockBadge;
        private TextMeshProUGUI leaderboardLabelText;

        // GUI Assets
        private GUISpriteAssets guiAssets;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Load GUI assets
            guiAssets = GUISpriteAssets.Instance;

            // Use UIDesignSystem constants for consistent sizing (2x size for better visibility)
            buttonWidth = 480f;
            buttonHeight = UIDesignSystem.ButtonHeightMedium * 2;  // 112px - 2x size
            buttonSpacing = UIDesignSystem.SpacingM;               // 16px spacing
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
            // Position below currency display, aligned to right edge with padding
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-20, -250);  // Aligned with currency panel
            rt.sizeDelta = new Vector2(220, 70);  // Same width as currency panel

            // Use GUI button sprite if available
            Image bg = menuToggleButton.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonPurple != null)
            {
                bg.sprite = guiAssets.buttonPurple;
                bg.type = Image.Type.Sliced;
                bg.color = Color.white;
            }
            else
            {
                bg.color = UIDesignSystem.AccentPurple;
            }

            Button btn = menuToggleButton.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(ToggleMenu);

            // Set button colors
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            btn.colors = colors;

            // Menu text label - simple and clean
            GameObject iconObj = new GameObject("Label");
            iconObj.transform.SetParent(menuToggleButton.transform, false);

            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            iconRt.anchorMin = Vector2.zero;
            iconRt.anchorMax = Vector2.one;
            iconRt.offsetMin = new Vector2(8, 8);
            iconRt.offsetMax = new Vector2(-8, -8);

            TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
            iconText.text = "MENU";
            iconText.fontSize = UIDesignSystem.FontSizeButton;
            iconText.fontStyle = FontStyles.Bold;
            iconText.alignment = TextAlignmentOptions.Center;
            iconText.color = UIDesignSystem.TextPrimary;

            // Apply shared font if available
            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                iconText.font = GameUI.Instance.SharedFont;
                GameUI.ApplyTextOutline(iconText);
            }
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
            dailyLoginLockBadge = CreateLockBadge(dailyLoginBtn.gameObject);
            dailyLoginLabelText = dailyLoginBtn.GetComponentInChildren<TextMeshProUGUI>();

            // Missions
            missionsBtn = CreateMenuButton("Missions", "📋", startY - (buttonHeight + buttonSpacing) * index++,
                UIDesignSystem.AccentOrange, OnMissionsClicked);
            missionsBadge = CreateNotificationBadge(missionsBtn.gameObject);
            missionsLockBadge = CreateLockBadge(missionsBtn.gameObject);
            missionsLabelText = missionsBtn.GetComponentInChildren<TextMeshProUGUI>();

            // Overclock
            overclockBtn = CreateMenuButton("Overclock", "🔥", startY - (buttonHeight + buttonSpacing) * index++,
                UIDesignSystem.OverclockOrange, OnOverclockClicked);
            overclockLockBadge = CreateLockBadge(overclockBtn.gameObject);
            overclockLabelText = overclockBtn.GetComponentInChildren<TextMeshProUGUI>();

            // Time Fracture
            timeFractureBtn = CreateMenuButton("Time Fracture", "⏱", startY - (buttonHeight + buttonSpacing) * index++,
                UIDesignSystem.TimeShardsBlue, OnTimeFractureClicked);
            timeFractureLockBadge = CreateLockBadge(timeFractureBtn.gameObject);
            timeFractureLabelText = timeFractureBtn.GetComponentInChildren<TextMeshProUGUI>();

            // Milestones
            milestonesBtn = CreateMenuButton("Milestones", "🏆", startY - (buttonHeight + buttonSpacing) * index++,
                UIDesignSystem.AccentGold, OnMilestonesClicked);
            milestoneBadge = CreateNotificationBadge(milestonesBtn.gameObject);
            milestonesLockBadge = CreateLockBadge(milestonesBtn.gameObject);
            milestonesLabelText = milestonesBtn.GetComponentInChildren<TextMeshProUGUI>();

            // Global Event
            globalEventBtn = CreateMenuButton("Events", "🌍", startY - (buttonHeight + buttonSpacing) * index++,
                UIDesignSystem.EventBlue, OnGlobalEventClicked);
            eventBadge = CreateNotificationBadge(globalEventBtn.gameObject);
            eventsLockBadge = CreateLockBadge(globalEventBtn.gameObject);
            eventsLabelText = globalEventBtn.GetComponentInChildren<TextMeshProUGUI>();

            // Leaderboard
            leaderboardBtn = CreateMenuButton("Leaderboard", "🏅", startY - (buttonHeight + buttonSpacing) * index++,
                UIDesignSystem.LeaderboardGold, OnLeaderboardClicked);
            leaderboardLockBadge = CreateLockBadge(leaderboardBtn.gameObject);
            leaderboardLabelText = leaderboardBtn.GetComponentInChildren<TextMeshProUGUI>();
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

            // Use GUI sprite if available, otherwise fallback to color
            Image bg = btnObj.AddComponent<Image>();
            GUISpriteAssets guiAssets = GUISpriteAssets.Instance;
            Sprite buttonSprite = GetButtonSpriteForColor(guiAssets, accentColor);

            if (buttonSprite != null)
            {
                bg.sprite = buttonSprite;
                bg.type = Image.Type.Sliced;
                bg.color = Color.white;
            }
            else
            {
                bg.color = accentColor;
            }

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(onClick);
            btn.onClick.AddListener(() => AudioManager.Instance?.PlayButtonClickSound());

            // Button hover/press colors
            var colors = btn.colors;
            if (buttonSprite != null)
            {
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
                colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            }
            else
            {
                colors.highlightedColor = accentColor * 1.2f;
                colors.pressedColor = accentColor * 0.7f;
            }
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
            labelText.fontSize = UIDesignSystem.FontSizeSubtitle;  // 40px - 2x size for larger buttons
            labelText.fontStyle = FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;

            // Apply font and outline
            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                labelText.font = GameUI.Instance.SharedFont;
            }
            GameUI.ApplyTextOutline(labelText);

            // Add polish for press animations
            if (UIPolishManager.Instance != null)
            {
                UIPolishManager.Instance.AddButtonPolish(btn);
            }

            return btn;
        }

        /// <summary>
        /// Gets the appropriate GUI sprite for a given accent color.
        /// </summary>
        private Sprite GetButtonSpriteForColor(GUISpriteAssets guiAssets, Color color)
        {
            if (guiAssets == null) return null;

            // Match color to button sprite
            if (color == UIDesignSystem.ButtonSuccess || color == UIDesignSystem.AccentDailyLogin)
                return guiAssets.buttonGreen;
            if (color == UIDesignSystem.AccentOrange || color == UIDesignSystem.AccentMissions || color == UIDesignSystem.OverclockOrange)
                return guiAssets.buttonYellow; // Orange-ish
            if (color == UIDesignSystem.TimeShardsBlue || color == UIDesignSystem.EventBlue)
                return guiAssets.buttonBlue;
            if (color == UIDesignSystem.AccentGold || color == UIDesignSystem.LeaderboardGold)
                return guiAssets.buttonYellow;
            if (color == UIDesignSystem.AccentPurple || color == UIDesignSystem.AccentTimeFracture)
                return guiAssets.buttonPurple;

            return null; // Fallback to color
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
            Debug.Log("[MainMenuUI] Daily Login button clicked");

            // Check if Daily Login is unlocked
            bool isUnlocked = IsDailyLoginUnlocked();
            Debug.Log($"[MainMenuUI] Daily Login unlocked: {isUnlocked}");

            if (!isUnlocked)
            {
                // Show "Unlock in Skills" message
                if (GameUI.Instance != null)
                {
                    GameUI.Instance.ShowFloatingText(Vector3.zero, "Unlock in Skills first!", Color.red);
                }
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayButtonClickSound();
                }
                // Don't close menu - let user see the message
                return;
            }

            var ui = FindObjectOfType<DailyLoginUI>();
            Debug.Log($"[MainMenuUI] DailyLoginUI found: {ui != null}");
            if (ui != null)
            {
                Debug.Log("[MainMenuUI] Calling DailyLoginUI.Toggle()");
                ui.Toggle();
            }
            else Debug.LogWarning("[MainMenuUI] DailyLoginUI not found");
            CloseMenu();
        }

        private bool IsDailyLoginUnlocked()
        {
            return IsFeatureUnlocked(SkillNodeId.FU_DailyLogin);
        }

        private void OnMissionsClicked()
        {
            // Check if Missions is unlocked
            bool isUnlocked = IsFeatureUnlocked(SkillNodeId.FU_Missions);

            if (!isUnlocked)
            {
                ShowLockedMessage();
                return;
            }

            var ui = FindObjectOfType<MissionsUI>();
            if (ui != null) ui.Toggle();
            else Debug.LogWarning("[MainMenuUI] MissionsUI not found");
            CloseMenu();
        }

        private void OnOverclockClicked()
        {
            // Check if Overclock is unlocked
            bool isUnlocked = IsFeatureUnlocked(SkillNodeId.FU_Overclock);

            if (!isUnlocked)
            {
                ShowLockedMessage();
                return;
            }

            var ui = FindObjectOfType<OverclockUI>();
            if (ui != null) ui.Toggle();
            else Debug.LogWarning("[MainMenuUI] OverclockUI not found");
            CloseMenu();
        }

        private void OnTimeFractureClicked()
        {
            // Check if Time Fracture is unlocked
            bool isUnlocked = IsFeatureUnlocked(SkillNodeId.FU_TimeFracture);

            if (!isUnlocked)
            {
                ShowLockedMessage();
                return;
            }

            var ui = FindObjectOfType<TimeFractureUI>();
            if (ui != null) ui.Toggle();
            else Debug.LogWarning("[MainMenuUI] TimeFractureUI not found");
            CloseMenu();
        }

        private void OnMilestonesClicked()
        {
            // Check if Milestones is unlocked
            bool isUnlocked = IsFeatureUnlocked(SkillNodeId.FU_Milestones);

            if (!isUnlocked)
            {
                ShowLockedMessage();
                return;
            }

            var ui = FindObjectOfType<MilestonesUI>();
            if (ui != null) ui.Toggle();
            else Debug.LogWarning("[MainMenuUI] MilestonesUI not found");
            CloseMenu();
        }

        private void OnGlobalEventClicked()
        {
            // Check if Global Events is unlocked
            bool isUnlocked = IsFeatureUnlocked(SkillNodeId.FU_GlobalEvents);

            if (!isUnlocked)
            {
                ShowLockedMessage();
                return;
            }

            var ui = FindObjectOfType<GlobalEventUI>();
            if (ui != null) ui.Toggle();
            else Debug.LogWarning("[MainMenuUI] GlobalEventUI not found");
            CloseMenu();
        }

        private void OnLeaderboardClicked()
        {
            // Check if Leaderboard is unlocked
            bool isUnlocked = IsFeatureUnlocked(SkillNodeId.FU_Leaderboard);

            if (!isUnlocked)
            {
                ShowLockedMessage();
                return;
            }

            var ui = FindObjectOfType<LeaderboardUI>();
            if (ui != null) ui.Toggle();
            else Debug.LogWarning("[MainMenuUI] LeaderboardUI not found");
            CloseMenu();
        }

        /// <summary>
        /// Check if a feature is unlocked via skill tree node.
        /// </summary>
        private bool IsFeatureUnlocked(SkillNodeId nodeId)
        {
            if (SkillTreeManager.Instance == null) return false;
            return SkillTreeManager.Instance.IsNodeUnlocked(nodeId);
        }

        /// <summary>
        /// Shows a "locked" message to the player.
        /// </summary>
        private void ShowLockedMessage()
        {
            if (GameUI.Instance != null)
            {
                GameUI.Instance.ShowFloatingText(Vector3.zero, "Unlock in Skills first!", Color.red);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClickSound();
            }
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

            // Register with PopupManager
            if (PopupManager.Instance != null)
                PopupManager.Instance.RegisterPopupOpen("MainMenuUI");

            UpdateNotificationBadges();
        }

        public void CloseMenu()
        {
            if (!isMenuOpen) return;
            isMenuOpen = false;

            // Unregister from PopupManager
            if (PopupManager.Instance != null)
                PopupManager.Instance.RegisterPopupClosed("MainMenuUI");

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

            // Daily Login - show lock if not unlocked in skill tree
            UpdateDailyLoginLockState();

            // Overclock - show lock if not unlocked in skill tree
            UpdateOverclockLockState();

            // Feature unlock locks - show lock if not unlocked in skill tree
            UpdateMissionsLockState();
            UpdateTimeFractureLockState();
            UpdateMilestonesLockState();
            UpdateEventsLockState();
            UpdateLeaderboardLockState();
        }

        private void UpdateDailyLoginLockState()
        {
            bool isUnlocked = IsDailyLoginUnlocked();

            if (dailyLoginLockBadge != null)
            {
                dailyLoginLockBadge.SetActive(!isUnlocked);
            }

            // Hide notification badge if locked
            if (dailyLoginBadge != null && !isUnlocked)
            {
                dailyLoginBadge.SetActive(false);
            }

            if (dailyLoginLabelText != null)
            {
                if (isUnlocked)
                {
                    dailyLoginLabelText.text = "DAILY LOGIN";
                }
                else
                {
                    dailyLoginLabelText.text = "DAILY LOGIN\n<size=60%><color=#888888>(LOCKED)</color></size>";
                }
            }

            // Dim the button if locked
            if (dailyLoginBtn != null)
            {
                var bg = dailyLoginBtn.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = isUnlocked ? UIDesignSystem.ButtonSuccess : UIDesignSystem.ButtonSuccess * 0.5f;
                }
            }
        }

        private void UpdateOverclockLockState()
        {
            bool isUnlocked = IsFeatureUnlocked(SkillNodeId.FU_Overclock);

            if (overclockLockBadge != null)
            {
                overclockLockBadge.SetActive(!isUnlocked);
            }

            if (overclockLabelText != null)
            {
                if (isUnlocked)
                {
                    overclockLabelText.text = "OVERCLOCK";
                }
                else
                {
                    overclockLabelText.text = "OVERCLOCK\n<size=60%><color=#888888>(LOCKED)</color></size>";
                }
            }

            // Dim the button if locked
            if (overclockBtn != null)
            {
                var bg = overclockBtn.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = isUnlocked ? UIDesignSystem.OverclockOrange : UIDesignSystem.OverclockOrange * 0.5f;
                }
            }
        }

        private void UpdateMissionsLockState()
        {
            bool isUnlocked = IsFeatureUnlocked(SkillNodeId.FU_Missions);

            if (missionsLockBadge != null)
            {
                missionsLockBadge.SetActive(!isUnlocked);
            }

            // Hide notification badge if locked
            if (missionsBadge != null && !isUnlocked)
            {
                missionsBadge.SetActive(false);
            }

            if (missionsLabelText != null)
            {
                missionsLabelText.text = isUnlocked ? "MISSIONS" : "MISSIONS\n<size=60%><color=#888888>(LOCKED)</color></size>";
            }

            // Dim the button if locked
            if (missionsBtn != null)
            {
                var bg = missionsBtn.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = isUnlocked ? UIDesignSystem.AccentOrange : UIDesignSystem.AccentOrange * 0.5f;
                }
            }
        }

        private void UpdateTimeFractureLockState()
        {
            bool isUnlocked = IsFeatureUnlocked(SkillNodeId.FU_TimeFracture);

            if (timeFractureLockBadge != null)
            {
                timeFractureLockBadge.SetActive(!isUnlocked);
            }

            if (timeFractureLabelText != null)
            {
                timeFractureLabelText.text = isUnlocked ? "TIME FRACTURE" : "TIME FRACTURE\n<size=60%><color=#888888>(LOCKED)</color></size>";
            }

            // Dim the button if locked
            if (timeFractureBtn != null)
            {
                var bg = timeFractureBtn.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = isUnlocked ? UIDesignSystem.TimeShardsBlue : UIDesignSystem.TimeShardsBlue * 0.5f;
                }
            }
        }

        private void UpdateMilestonesLockState()
        {
            bool isUnlocked = IsFeatureUnlocked(SkillNodeId.FU_Milestones);

            if (milestonesLockBadge != null)
            {
                milestonesLockBadge.SetActive(!isUnlocked);
            }

            // Hide notification badge if locked
            if (milestoneBadge != null && !isUnlocked)
            {
                milestoneBadge.SetActive(false);
            }

            if (milestonesLabelText != null)
            {
                milestonesLabelText.text = isUnlocked ? "MILESTONES" : "MILESTONES\n<size=60%><color=#888888>(LOCKED)</color></size>";
            }

            // Dim the button if locked
            if (milestonesBtn != null)
            {
                var bg = milestonesBtn.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = isUnlocked ? UIDesignSystem.AccentGold : UIDesignSystem.AccentGold * 0.5f;
                }
            }
        }

        private void UpdateEventsLockState()
        {
            bool isUnlocked = IsFeatureUnlocked(SkillNodeId.FU_GlobalEvents);

            if (eventsLockBadge != null)
            {
                eventsLockBadge.SetActive(!isUnlocked);
            }

            // Hide notification badge if locked
            if (eventBadge != null && !isUnlocked)
            {
                eventBadge.SetActive(false);
            }

            if (eventsLabelText != null)
            {
                eventsLabelText.text = isUnlocked ? "EVENTS" : "EVENTS\n<size=60%><color=#888888>(LOCKED)</color></size>";
            }

            // Dim the button if locked
            if (globalEventBtn != null)
            {
                var bg = globalEventBtn.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = isUnlocked ? UIDesignSystem.EventBlue : UIDesignSystem.EventBlue * 0.5f;
                }
            }
        }

        private void UpdateLeaderboardLockState()
        {
            bool isUnlocked = IsFeatureUnlocked(SkillNodeId.FU_Leaderboard);

            if (leaderboardLockBadge != null)
            {
                leaderboardLockBadge.SetActive(!isUnlocked);
            }

            if (leaderboardLabelText != null)
            {
                leaderboardLabelText.text = isUnlocked ? "LEADERBOARD" : "LEADERBOARD\n<size=60%><color=#888888>(LOCKED)</color></size>";
            }

            // Dim the button if locked
            if (leaderboardBtn != null)
            {
                var bg = leaderboardBtn.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = isUnlocked ? UIDesignSystem.LeaderboardGold : UIDesignSystem.LeaderboardGold * 0.5f;
                }
            }
        }

        private GameObject CreateLockBadge(GameObject parent)
        {
            GameObject badge = new GameObject("LockBadge");
            badge.transform.SetParent(parent.transform, false);

            RectTransform rt = badge.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.anchoredPosition = new Vector2(-10, 0);
            rt.sizeDelta = new Vector2(50, 50);

            // Use lock icon sprite from GUI assets
            Image lockIcon = badge.AddComponent<Image>();
            if (guiAssets != null && guiAssets.iconLock != null)
            {
                lockIcon.sprite = guiAssets.iconLock;
                lockIcon.color = new Color(0.7f, 0.7f, 0.7f, 1f); // Slightly dimmed
                lockIcon.preserveAspect = true;
            }
            else
            {
                // Fallback: gray circle with "X" if no sprite available
                lockIcon.color = new Color(0.4f, 0.4f, 0.4f, 0.9f);

                GameObject iconObj = new GameObject("FallbackIcon");
                iconObj.transform.SetParent(badge.transform, false);

                RectTransform iconRt = iconObj.AddComponent<RectTransform>();
                iconRt.anchorMin = Vector2.zero;
                iconRt.anchorMax = Vector2.one;
                iconRt.offsetMin = Vector2.zero;
                iconRt.offsetMax = Vector2.zero;

                TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
                iconText.text = "X";
                iconText.fontSize = 32;
                iconText.fontStyle = FontStyles.Bold;
                iconText.alignment = TextAlignmentOptions.Center;
                iconText.color = Color.white;
            }

            return badge;
        }

        #endregion
    }
}
