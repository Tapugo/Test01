using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.UI;

namespace Incredicer.Missions
{
    /// <summary>
    /// UI controller for the Missions panel with Daily and Weekly tabs.
    /// </summary>
    public class MissionsUI : MonoBehaviour
    {
        public static MissionsUI Instance { get; private set; }

        [Header("Main Panel")]
        [SerializeField] private GameObject panelPrefab;  // Assign prefab in inspector
        private GameObject mainPanel;
        private CanvasGroup panelCanvasGroup;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button closeButton;

        [Header("Tabs")]
        [SerializeField] private Button dailyTabButton;
        [SerializeField] private Button weeklyTabButton;
        [SerializeField] private Image dailyTabBg;
        [SerializeField] private Image weeklyTabBg;
        [SerializeField] private TextMeshProUGUI dailyTabText;
        [SerializeField] private TextMeshProUGUI weeklyTabText;

        [Header("Content")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentContainer;

        [Header("HUD Button")]
        [SerializeField] private Button hudButton;
        [SerializeField] private GameObject hudBadge;
        [SerializeField] private TextMeshProUGUI hudBadgeText;

        [Header("GUI Assets")]
        [SerializeField] private GUISpriteAssets guiAssets;

        [Header("Colors")]
        [SerializeField] private Color tabActiveColor = new Color(1f, 0.6f, 0.2f);  // AccentMissions orange
        [SerializeField] private Color tabInactiveColor = new Color(0.35f, 0.35f, 0.4f);
        [SerializeField] private Color progressBarFillColor = new Color(1f, 0.6f, 0.2f);  // AccentMissions orange
        [SerializeField] private Color progressBarBgColor = new Color(0.15f, 0.15f, 0.2f);

        private bool isShowingDaily = true;
        private bool isOpen = false;
        private bool isBuilt = false;
        private List<GameObject> missionCards = new List<GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (guiAssets == null)
                guiAssets = GUISpriteAssets.Instance;
        }

        private void Start()
        {
            // Subscribe to mission events
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionProgress += OnMissionProgress;
                MissionManager.Instance.OnMissionCompleted += OnMissionCompleted;
                MissionManager.Instance.OnMissionClaimed += OnMissionClaimed;
                MissionManager.Instance.OnMissionsRefreshed += OnMissionsRefreshed;
            }

            // Setup buttons
            if (hudButton != null)
                hudButton.onClick.AddListener(Toggle);
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
            if (dailyTabButton != null)
                dailyTabButton.onClick.AddListener(() => SwitchTab(true));
            if (weeklyTabButton != null)
                weeklyTabButton.onClick.AddListener(() => SwitchTab(false));

            // Hide panel initially
            if (mainPanel != null)
                mainPanel.SetActive(false);

            // Update HUD badge
            UpdateHUDBadge();
        }

        private void OnDestroy()
        {
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionProgress -= OnMissionProgress;
                MissionManager.Instance.OnMissionCompleted -= OnMissionCompleted;
                MissionManager.Instance.OnMissionClaimed -= OnMissionClaimed;
                MissionManager.Instance.OnMissionsRefreshed -= OnMissionsRefreshed;
            }
        }

        #region Panel Control

        public void Show()
        {
            if (isOpen) return;
            isOpen = true;

            if (!isBuilt)
                BuildUI();

            if (mainPanel != null)
            {
                mainPanel.SetActive(true);

                // Ensure popup is rendered on top of other UI elements (like menu button)
                mainPanel.transform.SetAsLastSibling();

                // Ensure it renders above other UI
                Canvas panelCanvas = mainPanel.GetComponent<Canvas>();
                if (panelCanvas == null)
                {
                    panelCanvas = mainPanel.AddComponent<Canvas>();
                    panelCanvas.overrideSorting = true;
                    mainPanel.AddComponent<GraphicRaycaster>();
                }
                panelCanvas.sortingOrder = 250;

                if (panelCanvasGroup != null)
                {
                    panelCanvasGroup.alpha = 0f;
                    panelCanvasGroup.DOFade(1f, 0.25f);
                }
            }

            RefreshMissionList();
            UpdateTabVisuals();

            // Apply button polish for press/release animations
            if (UI.UIPolishManager.Instance != null)
            {
                UI.UIPolishManager.Instance.PolishButtonsInPanel(mainPanel);
            }

            // Register with PopupManager
            if (Core.PopupManager.Instance != null)
                Core.PopupManager.Instance.RegisterPopupOpen("MissionsUI");
        }

        public void Hide()
        {
            if (!isOpen) return;
            isOpen = false;

            // Unregister from PopupManager
            if (Core.PopupManager.Instance != null)
                Core.PopupManager.Instance.RegisterPopupClosed("MissionsUI");

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.DOFade(0f, 0.15f).OnComplete(() =>
                {
                    if (mainPanel != null)
                        mainPanel.SetActive(false);
                });
            }
            else if (mainPanel != null)
            {
                mainPanel.SetActive(false);
            }
        }

        public void Toggle()
        {
            if (isOpen) Hide();
            else Show();
        }

        #endregion

        #region Tab Control

        private void SwitchTab(bool toDaily)
        {
            if (isShowingDaily == toDaily) return;

            isShowingDaily = toDaily;
            UpdateTabVisuals();
            RefreshMissionList();

            // Tab switch animation
            if (contentContainer != null)
            {
                contentContainer.DOKill();
                float startX = toDaily ? 50f : -50f;
                contentContainer.anchoredPosition = new Vector2(startX, contentContainer.anchoredPosition.y);
                contentContainer.DOAnchorPosX(0, 0.2f).SetEase(Ease.OutQuad);
            }
        }

        private void UpdateTabVisuals()
        {
            if (dailyTabBg != null)
                dailyTabBg.color = isShowingDaily ? tabActiveColor : tabInactiveColor;
            if (weeklyTabBg != null)
                weeklyTabBg.color = !isShowingDaily ? tabActiveColor : tabInactiveColor;

            if (dailyTabText != null)
                dailyTabText.color = isShowingDaily ? Color.white : new Color(0.7f, 0.7f, 0.7f);
            if (weeklyTabText != null)
                weeklyTabText.color = !isShowingDaily ? Color.white : new Color(0.7f, 0.7f, 0.7f);
        }

        #endregion

        #region Mission List

        private void RefreshMissionList()
        {
            if (MissionManager.Instance == null) return;

            // Clear existing cards
            foreach (var card in missionCards)
            {
                if (card != null) Destroy(card);
            }
            missionCards.Clear();

            // Get missions for current tab
            var missions = isShowingDaily ?
                MissionManager.Instance.DailyMissions :
                MissionManager.Instance.WeeklyMissions;

            // Create cards
            foreach (var mission in missions)
            {
                CreateMissionCard(mission);
            }

            // Reset scroll position
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 1f;
        }

        private void CreateMissionCard(MissionInstance mission)
        {
            if (contentContainer == null) return;

            GameObject cardObj = new GameObject($"Mission_{mission.missionId}");
            cardObj.transform.SetParent(contentContainer, false);
            missionCards.Add(cardObj);

            RectTransform cardRect = cardObj.AddComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(0, 200);

            LayoutElement le = cardObj.AddComponent<LayoutElement>();
            le.minHeight = 200;
            le.preferredHeight = 200;
            le.flexibleWidth = 1;

            // Background
            Image cardBg = cardObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.listFrame != null)
            {
                cardBg.sprite = guiAssets.listFrame;
                cardBg.type = Image.Type.Sliced;
            }
            cardBg.color = mission.isClaimed ? new Color(0.15f, 0.2f, 0.15f, 0.9f) :
                           mission.isCompleted ? new Color(0.2f, 0.25f, 0.15f, 0.95f) :
                           new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(cardObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.7f);
            titleRect.anchorMax = new Vector2(0.65f, 0.95f);
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = new Vector2(-10, -10);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = mission.displayName;
            titleText.fontSize = UIDesignSystem.FontSizeLarge;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = mission.isClaimed ? new Color(0.5f, 0.7f, 0.5f) : UIDesignSystem.TextPrimary;

            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(cardObj.transform, false);
            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0.45f);
            descRect.anchorMax = new Vector2(0.65f, 0.7f);
            descRect.offsetMin = new Vector2(20, 0);
            descRect.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = mission.description;
            descText.fontSize = UIDesignSystem.FontSizeSmall;
            descText.alignment = TextAlignmentOptions.Left;
            descText.color = UIDesignSystem.TextSecondary;

            // Progress bar background
            GameObject progressBgObj = new GameObject("ProgressBg");
            progressBgObj.transform.SetParent(cardObj.transform, false);
            RectTransform progressBgRect = progressBgObj.AddComponent<RectTransform>();
            progressBgRect.anchorMin = new Vector2(0, 0.2f);
            progressBgRect.anchorMax = new Vector2(0.65f, 0.4f);
            progressBgRect.offsetMin = new Vector2(20, 5);
            progressBgRect.offsetMax = new Vector2(-10, -5);

            Image progressBgImg = progressBgObj.AddComponent<Image>();
            progressBgImg.color = progressBarBgColor;

            // Progress bar fill
            GameObject progressFillObj = new GameObject("ProgressFill");
            progressFillObj.transform.SetParent(progressBgObj.transform, false);
            RectTransform progressFillRect = progressFillObj.AddComponent<RectTransform>();
            progressFillRect.anchorMin = Vector2.zero;
            progressFillRect.anchorMax = new Vector2(mission.ProgressPercent, 1f);
            progressFillRect.offsetMin = Vector2.zero;
            progressFillRect.offsetMax = Vector2.zero;

            Image progressFillImg = progressFillObj.AddComponent<Image>();
            progressFillImg.color = mission.isClaimed ? new Color(0.5f, 0.7f, 0.5f) : progressBarFillColor;

            // Progress text
            GameObject progressTextObj = new GameObject("ProgressText");
            progressTextObj.transform.SetParent(cardObj.transform, false);
            RectTransform progressTextRect = progressTextObj.AddComponent<RectTransform>();
            progressTextRect.anchorMin = new Vector2(0, 0.02f);
            progressTextRect.anchorMax = new Vector2(0.65f, 0.2f);
            progressTextRect.offsetMin = new Vector2(20, 0);
            progressTextRect.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI progressText = progressTextObj.AddComponent<TextMeshProUGUI>();
            progressText.text = $"{GameUI.FormatNumber(mission.currentProgress)} / {GameUI.FormatNumber(mission.targetAmount)}";
            progressText.fontSize = UIDesignSystem.FontSizeSmall;
            progressText.alignment = TextAlignmentOptions.Left;
            progressText.color = UIDesignSystem.TextSecondary;

            // Rewards section
            CreateRewardsDisplay(cardObj, mission);

            // Claim button
            CreateClaimButton(cardObj, mission);

            // Apply shared font
            ApplyFontToCard(cardObj);

            // Completed checkmark overlay - use icon like milestones
            if (mission.isClaimed)
            {
                GameObject checkObj = new GameObject("Checkmark");
                checkObj.transform.SetParent(cardObj.transform, false);
                RectTransform checkRect = checkObj.AddComponent<RectTransform>();
                checkRect.anchorMin = new Vector2(0.85f, 0.5f);
                checkRect.anchorMax = new Vector2(0.85f, 0.5f);
                checkRect.sizeDelta = new Vector2(60, 60);

                // Use iconCheck sprite if available, fallback to text
                if (guiAssets != null && guiAssets.iconCheck != null)
                {
                    Image checkImage = checkObj.AddComponent<Image>();
                    checkImage.sprite = guiAssets.iconCheck;
                    checkImage.preserveAspect = true;
                    checkImage.color = UIDesignSystem.SuccessGreen;
                }
                else
                {
                    TextMeshProUGUI checkText = checkObj.AddComponent<TextMeshProUGUI>();
                    checkText.text = "✓";
                    checkText.fontSize = UIDesignSystem.FontSizeSubtitle;
                    checkText.alignment = TextAlignmentOptions.Center;
                    checkText.color = UIDesignSystem.SuccessGreen;
                }
            }
        }

        private void CreateRewardsDisplay(GameObject cardObj, MissionInstance mission)
        {
            GameObject rewardsObj = new GameObject("Rewards");
            rewardsObj.transform.SetParent(cardObj.transform, false);
            RectTransform rewardsRect = rewardsObj.AddComponent<RectTransform>();
            rewardsRect.anchorMin = new Vector2(0.65f, 0.5f);
            rewardsRect.anchorMax = new Vector2(0.95f, 0.95f);
            rewardsRect.offsetMin = new Vector2(5, 5);
            rewardsRect.offsetMax = new Vector2(-10, -10);

            VerticalLayoutGroup vlg = rewardsObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Rewards label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(rewardsObj.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(0, 25);

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = "Rewards:";
            labelText.fontSize = UIDesignSystem.FontSizeLabel;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = UIDesignSystem.TextMuted;

            LayoutElement labelLe = labelObj.AddComponent<LayoutElement>();
            labelLe.preferredHeight = 25;

            // Display each reward
            if (mission.rewards != null)
            {
                foreach (var reward in mission.rewards)
                {
                    GameObject rewardItemObj = new GameObject("RewardItem");
                    rewardItemObj.transform.SetParent(rewardsObj.transform, false);
                    RectTransform rewardItemRect = rewardItemObj.AddComponent<RectTransform>();
                    rewardItemRect.sizeDelta = new Vector2(0, 30);

                    TextMeshProUGUI rewardText = rewardItemObj.AddComponent<TextMeshProUGUI>();
                    string rewardStr = "";
                    Color rewardColor = Color.white;

                    switch (reward.type)
                    {
                        case MissionRewardType.Money:
                            rewardStr = $"${GameUI.FormatNumber(reward.amount)}";
                            rewardColor = new Color(0.4f, 1f, 0.5f);
                            break;
                        case MissionRewardType.DarkMatter:
                            rewardStr = $"{GameUI.FormatNumber(reward.amount)} DM";
                            rewardColor = new Color(0.6f, 0.4f, 1f);
                            break;
                        case MissionRewardType.TimeShards:
                            rewardStr = $"{reward.amount} Shards";
                            rewardColor = new Color(0.3f, 0.8f, 1f);
                            break;
                        case MissionRewardType.MoneyBoost:
                            rewardStr = $"+{reward.amount}% $";
                            rewardColor = new Color(1f, 0.85f, 0.2f);
                            break;
                        case MissionRewardType.DMBoost:
                            rewardStr = $"+{reward.amount}% DM";
                            rewardColor = new Color(0.8f, 0.4f, 1f);
                            break;
                    }

                    rewardText.text = rewardStr;
                    rewardText.fontSize = UIDesignSystem.FontSizeBody;
                    rewardText.fontStyle = FontStyles.Bold;
                    rewardText.alignment = TextAlignmentOptions.Center;
                    rewardText.color = rewardColor;

                    LayoutElement rewardLe = rewardItemObj.AddComponent<LayoutElement>();
                    rewardLe.preferredHeight = 30;
                }
            }
        }

        private void CreateClaimButton(GameObject cardObj, MissionInstance mission)
        {
            GameObject btnObj = new GameObject("ClaimButton");
            btnObj.transform.SetParent(cardObj.transform, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.65f, 0.05f);
            btnRect.anchorMax = new Vector2(0.95f, 0.45f);
            btnRect.offsetMin = new Vector2(5, 5);
            btnRect.offsetMax = new Vector2(-10, -5);

            Image btnBg = btnObj.AddComponent<Image>();

            string buttonText;
            Color bgColor;
            bool interactable;

            if (mission.isClaimed)
            {
                buttonText = "CLAIMED";
                bgColor = new Color(0.4f, 0.4f, 0.4f);
                interactable = false;
                if (guiAssets != null && guiAssets.buttonGray != null)
                {
                    btnBg.sprite = guiAssets.buttonGray;
                    btnBg.type = Image.Type.Sliced;
                }
            }
            else if (mission.isCompleted)
            {
                buttonText = "CLAIM";
                bgColor = new Color(0.3f, 0.9f, 0.4f);
                interactable = true;
                if (guiAssets != null && guiAssets.buttonGreen != null)
                {
                    btnBg.sprite = guiAssets.buttonGreen;
                    btnBg.type = Image.Type.Sliced;
                }

                // Glow effect for claimable
                btnObj.transform.DOScale(1.05f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                buttonText = "IN PROGRESS";
                bgColor = new Color(0.3f, 0.3f, 0.4f);
                interactable = false;
                if (guiAssets != null && guiAssets.buttonGray != null)
                {
                    btnBg.sprite = guiAssets.buttonGray;
                    btnBg.type = Image.Type.Sliced;
                }
            }

            btnBg.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            btn.interactable = interactable;

            // Capture mission for closure
            MissionInstance capturedMission = mission;
            btn.onClick.AddListener(() => OnClaimClicked(capturedMission, btnObj));

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = buttonText;
            btnText.fontSize = UIDesignSystem.FontSizeButton;
            btnText.fontStyle = FontStyles.Bold;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = UIDesignSystem.TextPrimary;
        }

        private void OnClaimClicked(MissionInstance mission, GameObject buttonObj)
        {
            if (MissionManager.Instance == null) return;
            if (!mission.CanClaim) return;

            // Claim animation
            buttonObj.transform.DOKill();
            buttonObj.transform.DOScale(0.9f, 0.05f)
                .OnComplete(() => buttonObj.transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutBack));

            // Claim the mission
            bool success = MissionManager.Instance.ClaimMission(mission);

            if (success)
            {
                // Play sound
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayPurchaseSound();

                // Show floating text for rewards
                if (GameUI.Instance != null && mission.rewards != null)
                {
                    foreach (var reward in mission.rewards)
                    {
                        string text = "";
                        Color color = Color.white;
                        switch (reward.type)
                        {
                            case MissionRewardType.Money:
                                text = $"+${GameUI.FormatNumber(reward.amount)}";
                                color = new Color(0.4f, 1f, 0.5f);
                                break;
                            case MissionRewardType.DarkMatter:
                                text = $"+{GameUI.FormatNumber(reward.amount)} DM";
                                color = new Color(0.6f, 0.4f, 1f);
                                break;
                        }
                        if (!string.IsNullOrEmpty(text))
                            GameUI.Instance.ShowFloatingText(Vector3.zero, text, color);
                    }
                }

                // Camera shake
                Camera cam = Camera.main;
                if (cam != null)
                    cam.transform.DOShakePosition(0.15f, 0.03f, 12, 90f, false, true);

                // Refresh list after short delay
                Invoke(nameof(RefreshMissionList), 0.3f);
                UpdateHUDBadge();
            }
        }

        private void ApplyFontToCard(GameObject cardObj)
        {
            if (GameUI.Instance == null) return;

            TMP_FontAsset sharedFont = GameUI.Instance.SharedFont;
            if (sharedFont == null) return;

            TextMeshProUGUI[] texts = cardObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in texts)
            {
                tmp.font = sharedFont;
                GameUI.ApplyTextOutline(tmp);
            }
        }

        #endregion

        #region Event Handlers

        private void OnMissionProgress(MissionInstance mission)
        {
            if (isOpen)
                RefreshMissionList();
        }

        private void OnMissionCompleted(MissionInstance mission)
        {
            UpdateHUDBadge();

            // Show toast notification
            if (GameUI.Instance != null)
            {
                string prefix = mission.isDaily ? "Daily" : "Weekly";
                GameUI.Instance.ShowFloatingText(Vector3.zero,
                    $"{prefix} mission complete!\n{mission.displayName}",
                    new Color(1f, 0.85f, 0.2f));
            }

            if (isOpen)
                RefreshMissionList();
        }

        private void OnMissionClaimed(MissionInstance mission)
        {
            UpdateHUDBadge();
        }

        private void OnMissionsRefreshed()
        {
            if (isOpen)
                RefreshMissionList();
            UpdateHUDBadge();
        }

        #endregion

        #region HUD Badge

        private void UpdateHUDBadge()
        {
            if (hudBadge == null) return;

            int claimable = MissionManager.Instance?.ClaimableCount ?? 0;
            hudBadge.SetActive(claimable > 0);

            if (hudBadgeText != null && claimable > 0)
            {
                hudBadgeText.text = claimable.ToString();
            }
        }

        #endregion

        #region Build UI

        private void BuildUI()
        {
            if (isBuilt) return;
            isBuilt = true;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                canvas = FindObjectOfType<Canvas>();

            if (mainPanel == null && canvas != null)
            {
                // Try to instantiate from prefab first
                if (panelPrefab != null)
                {
                    mainPanel = Instantiate(panelPrefab, canvas.transform);
                    mainPanel.name = "MissionsPanel";
                    CacheUIReferences();
                    SetupButtonListeners();
                }
                else
                {
                    // Fallback to programmatic creation
                    CreateMainPanel(canvas.transform);
                }
            }

            ApplySharedFont();
            Debug.Log("[MissionsUI] UI built");
        }

        private void CacheUIReferences()
        {
            if (mainPanel == null) return;

            panelCanvasGroup = mainPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
                panelCanvasGroup = mainPanel.AddComponent<CanvasGroup>();

            // Find header elements
            Transform header = mainPanel.transform.Find("Header");
            if (header != null)
            {
                titleText = header.GetComponentInChildren<TextMeshProUGUI>();
                closeButton = header.GetComponentInChildren<Button>();
            }

            // Find tabs - check both "TabsArea" (runtime) and "Tabs" (prefab)
            Transform tabsArea = mainPanel.transform.Find("TabsArea");
            if (tabsArea == null)
                tabsArea = mainPanel.transform.Find("Tabs");

            if (tabsArea != null)
            {
                Transform dailyTab = tabsArea.Find("DailyTab");
                Transform weeklyTab = tabsArea.Find("WeeklyTab");

                if (dailyTab != null)
                {
                    dailyTabButton = dailyTab.GetComponent<Button>();
                    dailyTabBg = dailyTab.GetComponent<Image>();
                    dailyTabText = dailyTab.GetComponentInChildren<TextMeshProUGUI>();
                }
                if (weeklyTab != null)
                {
                    weeklyTabButton = weeklyTab.GetComponent<Button>();
                    weeklyTabBg = weeklyTab.GetComponent<Image>();
                    weeklyTabText = weeklyTab.GetComponentInChildren<TextMeshProUGUI>();
                }

                Debug.Log($"[MissionsUI] Found tabs - Daily: {dailyTabButton != null}, Weekly: {weeklyTabButton != null}");
            }
            else
            {
                Debug.LogWarning("[MissionsUI] Could not find TabsArea or Tabs in panel");
            }

            // Find scroll area
            scrollRect = mainPanel.GetComponentInChildren<ScrollRect>();
            if (scrollRect != null)
                contentContainer = scrollRect.content;
        }

        private void SetupButtonListeners()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (dailyTabButton != null)
                dailyTabButton.onClick.AddListener(() => SwitchTab(true));

            if (weeklyTabButton != null)
                weeklyTabButton.onClick.AddListener(() => SwitchTab(false));
        }

        private void CreateMainPanel(Transform parent)
        {
            // Main panel
            mainPanel = new GameObject("MissionsPanel");
            mainPanel.transform.SetParent(parent, false);

            RectTransform panelRect = mainPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelBg = mainPanel.AddComponent<Image>();
            panelBg.color = new Color(0.05f, 0.05f, 0.1f, 0.98f);

            panelCanvasGroup = mainPanel.AddComponent<CanvasGroup>();

            // Header
            CreateHeader();

            // Tabs
            CreateTabs();

            // Scroll area
            CreateScrollArea();
        }

        private void CreateHeader()
        {
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(mainPanel.transform, false);
            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.9f);
            headerRect.anchorMax = Vector2.one;
            headerRect.offsetMin = new Vector2(10, 5);
            headerRect.offsetMax = new Vector2(-10, -5);

            Image headerBg = headerObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.horizontalFrame != null)
            {
                headerBg.sprite = guiAssets.horizontalFrame;
                headerBg.type = Image.Type.Sliced;
            }
            headerBg.color = new Color(0.12f, 0.12f, 0.18f, 0.98f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(0.7f, 1);
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = Vector2.zero;

            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "MISSIONS";
            titleText.fontSize = UIDesignSystem.FontSizeTitle;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = UIDesignSystem.AccentMissions;

            // Close button
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(headerObj.transform, false);
            RectTransform closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.85f, 0.1f);
            closeRect.anchorMax = new Vector2(0.98f, 0.9f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;

            Image closeBg = closeObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonRed != null)
            {
                closeBg.sprite = guiAssets.buttonRed;
                closeBg.type = Image.Type.Sliced;
            }
            closeBg.color = new Color(0.9f, 0.3f, 0.3f);

            closeButton = closeObj.AddComponent<Button>();
            closeButton.onClick.AddListener(Hide);

            GameObject closeTextObj = new GameObject("X");
            closeTextObj.transform.SetParent(closeObj.transform, false);
            RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.fontSize = UIDesignSystem.FontSizeSubtitle;
            closeText.fontStyle = FontStyles.Bold;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.color = UIDesignSystem.TextPrimary;
        }

        private void CreateTabs()
        {
            // Use "TabsArea" name to match CacheUIReferences
            GameObject tabsObj = new GameObject("TabsArea");
            tabsObj.transform.SetParent(mainPanel.transform, false);
            RectTransform tabsRect = tabsObj.AddComponent<RectTransform>();
            tabsRect.anchorMin = new Vector2(0, 0.82f);
            tabsRect.anchorMax = new Vector2(1, 0.9f);
            tabsRect.offsetMin = new Vector2(20, 0);
            tabsRect.offsetMax = new Vector2(-20, 0);

            HorizontalLayoutGroup hlg = tabsObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Daily tab
            GameObject dailyObj = new GameObject("DailyTab");
            dailyObj.transform.SetParent(tabsObj.transform, false);

            // Add LayoutElement for proper sizing
            LayoutElement dailyLE = dailyObj.AddComponent<LayoutElement>();
            dailyLE.flexibleWidth = 1;

            dailyTabBg = dailyObj.AddComponent<Image>();
            dailyTabBg.raycastTarget = true;  // Ensure raycast works
            if (guiAssets != null && guiAssets.buttonGreen != null)
            {
                dailyTabBg.sprite = guiAssets.buttonGreen;
                dailyTabBg.type = Image.Type.Sliced;
            }
            dailyTabBg.color = tabActiveColor;

            dailyTabButton = dailyObj.AddComponent<Button>();
            dailyTabButton.targetGraphic = dailyTabBg;
            dailyTabButton.onClick.AddListener(() => SwitchTab(true));

            GameObject dailyTextObj = new GameObject("Text");
            dailyTextObj.transform.SetParent(dailyObj.transform, false);
            RectTransform dailyTextRect = dailyTextObj.AddComponent<RectTransform>();
            dailyTextRect.anchorMin = Vector2.zero;
            dailyTextRect.anchorMax = Vector2.one;
            dailyTextRect.offsetMin = Vector2.zero;
            dailyTextRect.offsetMax = Vector2.zero;

            dailyTabText = dailyTextObj.AddComponent<TextMeshProUGUI>();
            dailyTabText.text = "DAILY";
            dailyTabText.fontSize = UIDesignSystem.FontSizeLarge;
            dailyTabText.fontStyle = FontStyles.Bold;
            dailyTabText.alignment = TextAlignmentOptions.Center;
            dailyTabText.color = UIDesignSystem.TextPrimary;

            // Weekly tab
            GameObject weeklyObj = new GameObject("WeeklyTab");
            weeklyObj.transform.SetParent(tabsObj.transform, false);

            // Add LayoutElement for proper sizing
            LayoutElement weeklyLE = weeklyObj.AddComponent<LayoutElement>();
            weeklyLE.flexibleWidth = 1;

            weeklyTabBg = weeklyObj.AddComponent<Image>();
            weeklyTabBg.raycastTarget = true;  // Ensure raycast works
            if (guiAssets != null && guiAssets.buttonBlue != null)
            {
                weeklyTabBg.sprite = guiAssets.buttonBlue;
                weeklyTabBg.type = Image.Type.Sliced;
            }
            weeklyTabBg.color = tabInactiveColor;

            weeklyTabButton = weeklyObj.AddComponent<Button>();
            weeklyTabButton.targetGraphic = weeklyTabBg;
            weeklyTabButton.onClick.AddListener(() => SwitchTab(false));

            GameObject weeklyTextObj = new GameObject("Text");
            weeklyTextObj.transform.SetParent(weeklyObj.transform, false);
            RectTransform weeklyTextRect = weeklyTextObj.AddComponent<RectTransform>();
            weeklyTextRect.anchorMin = Vector2.zero;
            weeklyTextRect.anchorMax = Vector2.one;
            weeklyTextRect.offsetMin = Vector2.zero;
            weeklyTextRect.offsetMax = Vector2.zero;

            weeklyTabText = weeklyTextObj.AddComponent<TextMeshProUGUI>();
            weeklyTabText.text = "WEEKLY";
            weeklyTabText.fontSize = UIDesignSystem.FontSizeLarge;
            weeklyTabText.fontStyle = FontStyles.Bold;
            weeklyTabText.alignment = TextAlignmentOptions.Center;
            weeklyTabText.color = UIDesignSystem.TextMuted;
        }

        private void CreateScrollArea()
        {
            GameObject scrollObj = new GameObject("ScrollArea");
            scrollObj.transform.SetParent(mainPanel.transform, false);
            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 0.82f);
            scrollRect.offsetMin = new Vector2(10, 10);
            scrollRect.offsetMax = new Vector2(-10, -5);

            this.scrollRect = scrollObj.AddComponent<ScrollRect>();
            this.scrollRect.horizontal = false;
            this.scrollRect.vertical = true;
            this.scrollRect.movementType = ScrollRect.MovementType.Elastic;
            this.scrollRect.elasticity = 0.1f;
            this.scrollRect.scrollSensitivity = 20f;

            scrollObj.AddComponent<RectMask2D>();

            Image scrollBg = scrollObj.AddComponent<Image>();
            scrollBg.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);

            this.scrollRect.viewport = scrollRect;

            // Content container
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollObj.transform, false);
            contentContainer = contentObj.AddComponent<RectTransform>();
            contentContainer.anchorMin = new Vector2(0, 1);
            contentContainer.anchorMax = new Vector2(1, 1);
            contentContainer.pivot = new Vector2(0.5f, 1);
            contentContainer.anchoredPosition = Vector2.zero;
            contentContainer.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            this.scrollRect.content = contentContainer;
        }

        private void ApplySharedFont()
        {
            if (mainPanel == null) return;
            if (GameUI.Instance == null) return;

            TMP_FontAsset sharedFont = GameUI.Instance.SharedFont;
            if (sharedFont == null) return;

            TextMeshProUGUI[] allTexts = mainPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTexts)
            {
                tmp.font = sharedFont;
                GameUI.ApplyTextOutline(tmp);
            }
        }

        #endregion
    }
}
