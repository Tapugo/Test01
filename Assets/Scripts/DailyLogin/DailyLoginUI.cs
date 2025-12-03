using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.UI;

namespace Incredicer.DailyLogin
{
    /// <summary>
    /// UI controller for the Daily Login system.
    /// Shows the daily roll panel with streak display and animated dice roll.
    /// </summary>
    public class DailyLoginUI : MonoBehaviour
    {
        public static DailyLoginUI Instance { get; private set; }

        [Header("Main Panel")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;

        [Header("Dice Display")]
        [SerializeField] private RectTransform diceContainer;
        [SerializeField] private Image diceImage;
        [SerializeField] private Image diceShadow;

        [Header("Streak Display")]
        [SerializeField] private RectTransform streakContainer;
        [SerializeField] private Image[] streakNodes;
        [SerializeField] private Color streakFilledColor = new Color(1f, 0.85f, 0.2f);
        [SerializeField] private Color streakEmptyColor = new Color(0.3f, 0.3f, 0.35f);
        [SerializeField] private Color streakTodayColor = new Color(0.4f, 1f, 0.5f);

        [Header("Roll Button")]
        [SerializeField] private Button rollButton;
        [SerializeField] private TextMeshProUGUI rollButtonText;
        [SerializeField] private Image rollButtonBg;

        [Header("Reward Panel")]
        [SerializeField] private GameObject rewardPanel;
        [SerializeField] private CanvasGroup rewardCanvasGroup;
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TextMeshProUGUI rewardTitleText;
        [SerializeField] private TextMeshProUGUI rewardDescText;
        [SerializeField] private TextMeshProUGUI rewardAmountText;
        [SerializeField] private Button collectButton;
        [SerializeField] private TextMeshProUGUI collectButtonText;

        [Header("HUD Icon")]
        [SerializeField] private GameObject hudIcon;
        [SerializeField] private Image hudIconImage;
        [SerializeField] private GameObject hudBadge;

        [Header("GUI Assets")]
        [SerializeField] private GUISpriteAssets guiAssets;

        [Header("Reward Icons")]
        [SerializeField] private Sprite moneyIcon;
        [SerializeField] private Sprite darkMatterIcon;
        [SerializeField] private Sprite boostIcon;
        [SerializeField] private Sprite tokenIcon;

        [Header("Animation Settings")]
        [SerializeField] private float diceSpinDuration = 1f;
        [SerializeField] private float diceJumpHeight = 100f;
        [SerializeField] private int diceSpinRotations = 2;

        private DailyReward currentReward;
        private bool isAnimating = false;
        [System.NonSerialized] private bool isBuilt = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Force rebuild on next ShowPanel
            isBuilt = false;

            if (guiAssets == null)
                guiAssets = GUISpriteAssets.Instance;
        }

        private void Start()
        {
            // Subscribe to daily login events
            if (DailyLoginManager.Instance != null)
            {
                DailyLoginManager.Instance.OnDailyRewardAvailable += OnRewardAvailable;
                DailyLoginManager.Instance.OnStreakUpdated += OnStreakUpdated;
            }

            // Check if reward is available on start (after a short delay for systems to initialize)
            StartCoroutine(CheckInitialReward());
        }

        private IEnumerator CheckInitialReward()
        {
            yield return new WaitForSeconds(0.5f);

            if (DailyLoginManager.Instance != null && DailyLoginManager.Instance.CanRollToday)
            {
                ShowPanel();
            }
            else
            {
                UpdateHUDIcon();
            }
        }

        private void OnDestroy()
        {
            if (DailyLoginManager.Instance != null)
            {
                DailyLoginManager.Instance.OnDailyRewardAvailable -= OnRewardAvailable;
                DailyLoginManager.Instance.OnStreakUpdated -= OnStreakUpdated;
            }
        }

        private void OnRewardAvailable()
        {
            UpdateHUDIcon();
            ShowPanel();
        }

        private void OnStreakUpdated(int streakDay)
        {
            UpdateStreakDisplay(streakDay);
        }

        /// <summary>
        /// Shows the daily login panel.
        /// </summary>
        public void ShowPanel()
        {
            // Build UI if needed (do this first, before checking mainPanel)
            if (!isBuilt)
            {
                BuildUI();
            }

            if (mainPanel == null)
            {
                Debug.LogError("[DailyLoginUI] mainPanel is null after BuildUI!");
                return;
            }

            // Use fallback values if no manager is available
            int currentStreak = DailyLoginManager.Instance != null ? DailyLoginManager.Instance.CurrentStreakDay : 1;
            bool canRoll = DailyLoginManager.Instance != null ? DailyLoginManager.Instance.CanRollToday : true;

            // Update streak display
            UpdateStreakDisplay(currentStreak);

            // Reset roll button
            if (rollButton != null)
            {
                rollButton.interactable = canRoll;
            }
            if (rollButtonText != null)
            {
                rollButtonText.text = canRoll ? "ROLL!" : "ROLLED";
            }

            // Hide reward panel
            if (rewardPanel != null) rewardPanel.SetActive(false);

            // Show main panel with fade
            mainPanel.SetActive(true);
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.DOFade(1f, 0.3f);
            }

            // Block dice input
            SetDiceInputBlocked(true);
        }

        /// <summary>
        /// Hides the daily login panel.
        /// </summary>
        public void HidePanel()
        {
            if (mainPanel == null) return;

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
                {
                    mainPanel.SetActive(false);
                });
            }
            else
            {
                mainPanel.SetActive(false);
            }

            // Unblock dice input
            SetDiceInputBlocked(false);
            UpdateHUDIcon();
        }

        /// <summary>
        /// Toggles the daily login panel visibility.
        /// </summary>
        public void Toggle()
        {
            if (mainPanel != null && mainPanel.activeSelf)
                HidePanel();
            else
                ShowPanel();
        }

        private void SetDiceInputBlocked(bool blocked)
        {
            if (Dice.DiceRollerController.Instance != null)
            {
                Dice.DiceRollerController.Instance.enabled = !blocked;
            }
        }

        private void OnRollClicked()
        {
            if (isAnimating) return;

            // Generate the reward
            if (DailyLoginManager.Instance != null && DailyLoginManager.Instance.CanRollToday)
            {
                currentReward = DailyLoginManager.Instance.GenerateReward();
            }
            else
            {
                // Fallback reward for testing or if manager is unavailable
                currentReward = new DailyReward
                {
                    type = DailyRewardType.Money,
                    amount = 1000,
                    boostDuration = 0,
                    title = "Daily Bonus!",
                    description = "Thanks for logging in today!",
                    streakDay = 1,
                    streakMultiplier = 1f
                };
            }

            if (currentReward == null) return;

            // Start roll animation
            StartCoroutine(PlayRollAnimation());
        }

        private IEnumerator PlayRollAnimation()
        {
            isAnimating = true;

            // Disable roll button
            if (rollButton != null) rollButton.interactable = false;

            // Button press animation
            if (rollButton != null)
            {
                rollButton.transform.DOScale(0.9f, 0.05f).SetEase(Ease.InQuad)
                    .OnComplete(() => rollButton.transform.DOScale(1.05f, 0.1f).SetEase(Ease.OutBack)
                    .OnComplete(() => rollButton.transform.DOScale(1f, 0.05f)));
            }

            yield return new WaitForSeconds(0.15f);

            // Dice jump and spin animation
            if (diceContainer != null)
            {
                Vector3 startPos = diceContainer.anchoredPosition;

                // Jump up
                Sequence diceSeq = DOTween.Sequence();
                diceSeq.Append(diceContainer.DOAnchorPosY(startPos.y + diceJumpHeight, diceSpinDuration * 0.4f).SetEase(Ease.OutQuad));
                diceSeq.Append(diceContainer.DOAnchorPosY(startPos.y, diceSpinDuration * 0.6f).SetEase(Ease.InQuad));

                // Spin
                if (diceImage != null)
                {
                    diceImage.transform.DORotate(new Vector3(0, 0, 360f * diceSpinRotations), diceSpinDuration, RotateMode.FastBeyond360)
                        .SetEase(Ease.OutQuad);
                }

                // Shadow squash during jump
                if (diceShadow != null)
                {
                    diceShadow.transform.DOScale(new Vector3(0.5f, 0.5f, 1f), diceSpinDuration * 0.4f)
                        .OnComplete(() => diceShadow.transform.DOScale(Vector3.one, diceSpinDuration * 0.6f));
                }
            }

            yield return new WaitForSeconds(diceSpinDuration);

            // Landing thud - camera shake
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.DOShakePosition(0.15f, 0.03f, 15, 90f, false, true);
            }

            // Play landing sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRollSound();
            }

            yield return new WaitForSeconds(0.2f);

            // Show reward panel
            ShowRewardPanel();

            isAnimating = false;
        }

        private void ShowRewardPanel()
        {
            if (rewardPanel == null || currentReward == null) return;

            // Update reward display
            if (rewardTitleText != null)
                rewardTitleText.text = currentReward.title;

            if (rewardDescText != null)
            {
                string desc = currentReward.description;
                if (currentReward.streakMultiplier > 1f)
                {
                    desc += $"\n<color=#FFD700>Streak x{currentReward.streakMultiplier} bonus!</color>";
                }
                rewardDescText.text = desc;
            }

            if (rewardAmountText != null)
            {
                string amountStr = "";
                switch (currentReward.type)
                {
                    case DailyRewardType.Money:
                        amountStr = $"+${GameUI.FormatNumber(currentReward.amount)}";
                        break;
                    case DailyRewardType.DarkMatter:
                        amountStr = $"+{GameUI.FormatNumber(currentReward.amount)} DM";
                        break;
                    case DailyRewardType.MoneyBoost:
                        amountStr = $"+{currentReward.amount}% Money\nfor {currentReward.boostDuration / 60}min";
                        break;
                    case DailyRewardType.DMBoost:
                        amountStr = $"+{currentReward.amount}% DM\nfor {currentReward.boostDuration / 60}min";
                        break;
                    case DailyRewardType.JackpotToken:
                        amountStr = $"+{currentReward.amount} Jackpot Token";
                        break;
                }
                rewardAmountText.text = amountStr;
            }

            // Set reward icon
            if (rewardIcon != null)
            {
                switch (currentReward.type)
                {
                    case DailyRewardType.Money:
                        rewardIcon.sprite = moneyIcon ?? guiAssets?.iconCoin;
                        break;
                    case DailyRewardType.DarkMatter:
                        rewardIcon.sprite = darkMatterIcon ?? guiAssets?.iconStar; // Use star as fallback
                        break;
                    case DailyRewardType.MoneyBoost:
                    case DailyRewardType.DMBoost:
                        rewardIcon.sprite = boostIcon ?? guiAssets?.iconStar;
                        break;
                    case DailyRewardType.JackpotToken:
                        rewardIcon.sprite = tokenIcon ?? guiAssets?.iconStar;
                        break;
                }
            }

            // Show reward panel with slide-in animation
            rewardPanel.SetActive(true);
            RectTransform rewardRect = rewardPanel.GetComponent<RectTransform>();
            if (rewardRect != null)
            {
                Vector2 startPos = rewardRect.anchoredPosition;
                rewardRect.anchoredPosition = new Vector2(startPos.x, startPos.y - 300f);
                rewardRect.DOAnchorPos(startPos, 0.4f).SetEase(Ease.OutBack);
            }

            if (rewardCanvasGroup != null)
            {
                rewardCanvasGroup.alpha = 0f;
                rewardCanvasGroup.DOFade(1f, 0.3f);
            }
        }

        private void OnCollectClicked()
        {
            if (currentReward == null) return;

            // Claim the reward if manager is available
            if (DailyLoginManager.Instance != null)
            {
                DailyLoginManager.Instance.ClaimReward(currentReward);
            }
            else
            {
                // Fallback: directly add money to player if manager unavailable
                if (CurrencyManager.Instance != null && currentReward.type == DailyRewardType.Money)
                {
                    CurrencyManager.Instance.AddMoney(currentReward.amount, false);
                }
            }

            // Collect button animation
            if (collectButton != null)
            {
                collectButton.transform.DOScale(0.9f, 0.05f)
                    .OnComplete(() => collectButton.transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutBack)
                    .OnComplete(() => collectButton.transform.DOScale(1f, 0.05f)));
            }

            // Spawn floating reward effect
            SpawnRewardEffect();

            // Play collect sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPurchaseSound();
            }

            // Fade out reward panel
            if (rewardCanvasGroup != null)
            {
                rewardCanvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
                {
                    rewardPanel.SetActive(false);
                    HidePanel();
                });
            }
            else
            {
                rewardPanel.SetActive(false);
                HidePanel();
            }

            // Show floating text
            if (GameUI.Instance != null)
            {
                string text = "";
                Color color = Color.white;
                switch (currentReward.type)
                {
                    case DailyRewardType.Money:
                        text = $"+${GameUI.FormatNumber(currentReward.amount)}";
                        color = new Color(0.4f, 1f, 0.5f);
                        break;
                    case DailyRewardType.DarkMatter:
                        text = $"+{GameUI.FormatNumber(currentReward.amount)} DM";
                        color = new Color(0.6f, 0.4f, 1f);
                        break;
                    case DailyRewardType.MoneyBoost:
                        text = $"Money Boost Active!";
                        color = new Color(1f, 0.85f, 0.2f);
                        break;
                    case DailyRewardType.DMBoost:
                        text = $"DM Boost Active!";
                        color = new Color(0.8f, 0.4f, 1f);
                        break;
                }
                GameUI.Instance.ShowFloatingText(Vector3.zero, text, color);
            }

            currentReward = null;
        }

        private void SpawnRewardEffect()
        {
            // Spawn particles at center of screen
            Vector3 worldPos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            worldPos.z = 0;

            if (VisualEffectsManager.Instance != null)
            {
                VisualEffectsManager.Instance.SpawnPurchaseEffect(worldPos);
            }

            // Camera shake
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.DOShakePosition(0.2f, 0.05f, 15, 90f, false, true);
            }
        }

        private void UpdateStreakDisplay(int currentDay)
        {
            if (streakNodes == null) return;

            int streakLength = DailyLoginManager.Instance?.StreakLength ?? 7;

            for (int i = 0; i < streakNodes.Length; i++)
            {
                if (streakNodes[i] == null) continue;

                int dayNum = i + 1;

                if (dayNum < currentDay)
                {
                    // Past days - filled
                    streakNodes[i].color = streakFilledColor;
                }
                else if (dayNum == currentDay)
                {
                    // Today - special color with pulse
                    streakNodes[i].color = streakTodayColor;
                    streakNodes[i].transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                }
                else
                {
                    // Future days - empty
                    streakNodes[i].color = streakEmptyColor;
                    streakNodes[i].transform.DOKill();
                    streakNodes[i].transform.localScale = Vector3.one;
                }
            }
        }

        private void UpdateHUDIcon()
        {
            if (hudIcon == null) return;

            bool canRoll = DailyLoginManager.Instance != null && DailyLoginManager.Instance.CanRollToday;

            // Show/hide badge
            if (hudBadge != null)
            {
                hudBadge.SetActive(canRoll);
            }

            // Glow effect if available
            if (hudIconImage != null)
            {
                hudIconImage.color = canRoll ? Color.white : new Color(0.6f, 0.6f, 0.6f);
            }
        }

        /// <summary>
        /// Called when HUD icon is clicked.
        /// </summary>
        public void OnHUDIconClicked()
        {
            if (DailyLoginManager.Instance != null && DailyLoginManager.Instance.CanRollToday)
            {
                ShowPanel();
            }
            else
            {
                // Show summary popup
                ShowSummaryPopup();
            }
        }

        private void ShowSummaryPopup()
        {
            if (DailyLoginManager.Instance == null) return;
            if (GameUI.Instance == null) return;

            int streak = DailyLoginManager.Instance.CurrentStreakDay;
            int total = DailyLoginManager.Instance.TotalLoginDays;

            GameUI.Instance.ShowFloatingText(Vector3.zero,
                $"Streak: Day {streak}\nTotal Logins: {total}",
                new Color(1f, 0.85f, 0.2f));
        }

        /// <summary>
        /// Builds the UI dynamically if not set up in scene.
        /// </summary>
        private void BuildUI()
        {
            // Always rebuild to ensure latest changes are applied
            if (isBuilt && mainPanel != null)
            {
                Destroy(mainPanel);
                mainPanel = null;
            }
            isBuilt = true;

            // Find or create canvas - use the canvas this component is attached to
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }

            if (mainPanel == null && canvas != null)
            {
                CreateMainPanel(canvas.transform);
                Debug.Log("[DailyLoginUI] UI built successfully");
            }
            else if (canvas == null)
            {
                Debug.LogError("[DailyLoginUI] No Canvas found - cannot build UI!");
                return;
            }

            // Apply shared font
            ApplySharedFont();
        }

        private void CreateMainPanel(Transform parent)
        {
            // Create main panel - fullscreen overlay
            mainPanel = new GameObject("DailyLoginPanel");
            mainPanel.transform.SetParent(parent, false);

            RectTransform panelRect = mainPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Background - using UIDesignSystem
            Image panelBg = mainPanel.AddComponent<Image>();
            panelBg.color = UIDesignSystem.OverlayDark;

            panelCanvasGroup = mainPanel.AddComponent<CanvasGroup>();

            // Ensure it renders above other UI
            Canvas panelCanvas = mainPanel.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 300;
            mainPanel.AddComponent<GraphicRaycaster>();

            // Title - using UIDesignSystem colors and sizes
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(mainPanel.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.82f);
            titleRect.anchorMax = new Vector2(1, 0.95f);
            titleRect.offsetMin = new Vector2(UIDesignSystem.SpacingL, 0);
            titleRect.offsetMax = new Vector2(-UIDesignSystem.SpacingL, 0);

            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "DAILY ROLL";
            titleText.fontSize = UIDesignSystem.FontSizeHero;  // 72px
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = UIDesignSystem.AccentGold;

            // Subtitle
            GameObject subObj = new GameObject("Subtitle");
            subObj.transform.SetParent(mainPanel.transform, false);
            RectTransform subRect = subObj.AddComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0, 0.76f);
            subRect.anchorMax = new Vector2(1, 0.82f);
            subRect.offsetMin = new Vector2(UIDesignSystem.SpacingXL, 0);
            subRect.offsetMax = new Vector2(-UIDesignSystem.SpacingXL, 0);

            subtitleText = subObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "Roll once per day. The higher your streak, the bigger your reward!";
            subtitleText.fontSize = UIDesignSystem.FontSizeBody;  // 28px
            subtitleText.alignment = TextAlignmentOptions.Center;
            subtitleText.color = UIDesignSystem.TextSecondary;

            // Dice container - VERY LARGE prominent 3D dice in center
            GameObject diceObj = new GameObject("DiceContainer");
            diceObj.transform.SetParent(mainPanel.transform, false);
            diceContainer = diceObj.AddComponent<RectTransform>();
            diceContainer.anchorMin = new Vector2(0.5f, 0.38f);
            diceContainer.anchorMax = new Vector2(0.5f, 0.75f);
            diceContainer.sizeDelta = new Vector2(280, 280);  // Large square dice
            diceContainer.anchoredPosition = Vector2.zero;

            // Dice shadow - elliptical shadow beneath
            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(diceObj.transform, false);
            RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
            shadowRect.anchorMin = new Vector2(0.5f, 0);
            shadowRect.anchorMax = new Vector2(0.5f, 0);
            shadowRect.sizeDelta = new Vector2(200, 40);
            shadowRect.anchoredPosition = new Vector2(12, -25);

            diceShadow = shadowObj.AddComponent<Image>();
            diceShadow.color = UIDesignSystem.ShadowColor;

            // 3D dice effect - back face (darker, offset)
            GameObject diceBackObj = new GameObject("DiceBack");
            diceBackObj.transform.SetParent(diceObj.transform, false);
            RectTransform diceBackRect = diceBackObj.AddComponent<RectTransform>();
            diceBackRect.anchorMin = Vector2.zero;
            diceBackRect.anchorMax = Vector2.one;
            diceBackRect.offsetMin = new Vector2(12, -12);
            diceBackRect.offsetMax = new Vector2(12, -12);

            Image diceBackImg = diceBackObj.AddComponent<Image>();
            diceBackImg.color = new Color(0.75f, 0.55f, 0.15f);  // Darker gold for 3D depth

            // Main dice face - bright golden using design system
            GameObject diceImgObj = new GameObject("DiceImage");
            diceImgObj.transform.SetParent(diceObj.transform, false);
            RectTransform diceImgRect = diceImgObj.AddComponent<RectTransform>();
            diceImgRect.anchorMin = Vector2.zero;
            diceImgRect.anchorMax = Vector2.one;
            diceImgRect.offsetMin = Vector2.zero;
            diceImgRect.offsetMax = Vector2.zero;

            diceImage = diceImgObj.AddComponent<Image>();
            diceImage.color = UIDesignSystem.AccentGold;  // Bright golden dice

            // Add multiple outlines for 3D beveled effect
            Outline diceOutline = diceImgObj.AddComponent<Outline>();
            diceOutline.effectColor = new Color(1f, 0.95f, 0.7f);  // Light highlight
            diceOutline.effectDistance = new Vector2(-3, 3);

            // Dice dots (show a 6) - using design system helper
            UIDesignSystem.CreateDiceDots(diceImgObj.transform, 6, 50f, new Color(0.12f, 0.08f, 0.04f));

            // Add "TAP TO ROLL!" text below the dice
            GameObject rollMeObj = new GameObject("RollMeText");
            rollMeObj.transform.SetParent(diceObj.transform, false);
            RectTransform rollMeRect = rollMeObj.AddComponent<RectTransform>();
            rollMeRect.anchorMin = new Vector2(0.5f, 0);
            rollMeRect.anchorMax = new Vector2(0.5f, 0);
            rollMeRect.sizeDelta = new Vector2(300, 50);
            rollMeRect.anchoredPosition = new Vector2(0, -50);

            TextMeshProUGUI rollMeText = rollMeObj.AddComponent<TextMeshProUGUI>();
            rollMeText.text = "TAP TO ROLL!";
            rollMeText.fontSize = UIDesignSystem.FontSizeButton;  // 28px
            rollMeText.fontStyle = FontStyles.Bold;
            rollMeText.alignment = TextAlignmentOptions.Center;
            rollMeText.color = UIDesignSystem.AccentGold;

            // Make the dice itself clickable
            Button diceBtn = diceImgObj.AddComponent<Button>();
            diceBtn.targetGraphic = diceImage;
            diceBtn.onClick.AddListener(OnRollClicked);

            // Pulse animation on the dice using design system timing
            diceImgObj.transform.DOScale(1.05f, UIDesignSystem.AnimPulse).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            // Streak container
            CreateStreakDisplay();

            // Roll button
            CreateRollButton();

            // Close button (X in top right)
            CreateCloseButton();

            // Reward panel
            CreateRewardPanel();
        }

        private void CreateCloseButton()
        {
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(mainPanel.transform, false);

            RectTransform closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 1);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.anchoredPosition = new Vector2(-UIDesignSystem.SafeAreaPadding, -UIDesignSystem.SafeAreaPadding);
            closeRect.sizeDelta = new Vector2(UIDesignSystem.ButtonHeightLarge, UIDesignSystem.ButtonHeightLarge);

            Image closeBg = closeObj.AddComponent<Image>();
            closeBg.color = UIDesignSystem.ButtonDanger;

            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeBg;
            closeBtn.onClick.AddListener(HidePanel);

            var colors = closeBtn.colors;
            colors.highlightedColor = UIDesignSystem.ButtonDanger * 1.2f;
            colors.pressedColor = UIDesignSystem.ButtonDanger * 0.7f;
            closeBtn.colors = colors;

            // X text
            GameObject xText = new GameObject("X");
            xText.transform.SetParent(closeObj.transform, false);
            RectTransform xRect = xText.AddComponent<RectTransform>();
            xRect.anchorMin = Vector2.zero;
            xRect.anchorMax = Vector2.one;
            xRect.offsetMin = Vector2.zero;
            xRect.offsetMax = Vector2.zero;

            TextMeshProUGUI xTmp = xText.AddComponent<TextMeshProUGUI>();
            xTmp.text = "X";
            xTmp.fontSize = UIDesignSystem.FontSizeTitle;  // 48px
            xTmp.fontStyle = FontStyles.Bold;
            xTmp.alignment = TextAlignmentOptions.Center;
            xTmp.color = Color.white;
        }

        private void CreateStreakDisplay()
        {
            GameObject streakObj = new GameObject("StreakContainer");
            streakObj.transform.SetParent(mainPanel.transform, false);
            streakContainer = streakObj.AddComponent<RectTransform>();
            streakContainer.anchorMin = new Vector2(0.05f, 0.28f);
            streakContainer.anchorMax = new Vector2(0.95f, 0.38f);
            streakContainer.offsetMin = Vector2.zero;
            streakContainer.offsetMax = Vector2.zero;

            HorizontalLayoutGroup hlg = streakObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = UIDesignSystem.SpacingM;  // 16px
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            int streakLength = DailyLoginManager.Instance?.StreakLength ?? 7;
            streakNodes = new Image[streakLength];

            for (int i = 0; i < streakLength; i++)
            {
                GameObject nodeObj = new GameObject($"Day{i + 1}");
                nodeObj.transform.SetParent(streakObj.transform, false);

                RectTransform nodeRect = nodeObj.AddComponent<RectTransform>();
                nodeRect.sizeDelta = new Vector2(UIDesignSystem.ButtonHeightMedium, UIDesignSystem.ButtonHeightMedium);  // 56x56

                Image nodeImg = nodeObj.AddComponent<Image>();
                nodeImg.color = streakEmptyColor;
                streakNodes[i] = nodeImg;

                // Day number label
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(nodeObj.transform, false);
                RectTransform labelRect = labelObj.AddComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;

                TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
                labelText.text = (i + 1).ToString();
                labelText.fontSize = UIDesignSystem.FontSizeBody;  // 28px
                labelText.fontStyle = FontStyles.Bold;
                labelText.alignment = TextAlignmentOptions.Center;
                labelText.color = Color.white;
            }
        }

        private void CreateRollButton()
        {
            GameObject btnObj = new GameObject("RollButton");
            btnObj.transform.SetParent(mainPanel.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.2f, 0.08f);
            btnRect.anchorMax = new Vector2(0.8f, 0.2f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            rollButtonBg = btnObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonGreen != null)
            {
                rollButtonBg.sprite = guiAssets.buttonGreen;
                rollButtonBg.type = Image.Type.Sliced;
            }
            rollButtonBg.color = UIDesignSystem.ButtonSuccess;

            rollButton = btnObj.AddComponent<Button>();
            rollButton.onClick.AddListener(OnRollClicked);

            ColorBlock colors = rollButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            rollButton.colors = colors;

            // Add outline for polish
            Outline btnOutline = btnObj.AddComponent<Outline>();
            btnOutline.effectColor = UIDesignSystem.ShadowColor;
            btnOutline.effectDistance = new Vector2(3, -3);

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            rollButtonText = textObj.AddComponent<TextMeshProUGUI>();
            rollButtonText.text = "ROLL!";
            rollButtonText.fontSize = UIDesignSystem.FontSizeTitle;  // 48px
            rollButtonText.fontStyle = FontStyles.Bold;
            rollButtonText.alignment = TextAlignmentOptions.Center;
            rollButtonText.color = Color.white;
        }

        private void CreateRewardPanel()
        {
            rewardPanel = new GameObject("RewardPanel");
            rewardPanel.transform.SetParent(mainPanel.transform, false);

            RectTransform rewardRect = rewardPanel.AddComponent<RectTransform>();
            rewardRect.anchorMin = new Vector2(0.08f, 0.28f);
            rewardRect.anchorMax = new Vector2(0.92f, 0.78f);
            rewardRect.offsetMin = Vector2.zero;
            rewardRect.offsetMax = Vector2.zero;

            Image rewardBg = rewardPanel.AddComponent<Image>();
            if (guiAssets != null && guiAssets.listFrame != null)
            {
                rewardBg.sprite = guiAssets.listFrame;
                rewardBg.type = Image.Type.Sliced;
            }
            rewardBg.color = UIDesignSystem.PanelBgMedium;

            // Add outline for polish
            Outline rewardOutline = rewardPanel.AddComponent<Outline>();
            rewardOutline.effectColor = UIDesignSystem.AccentGold * 0.6f;
            rewardOutline.effectDistance = new Vector2(3, -3);

            rewardCanvasGroup = rewardPanel.AddComponent<CanvasGroup>();

            // Reward icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(rewardPanel.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.55f);
            iconRect.anchorMax = new Vector2(0.5f, 0.55f);
            iconRect.sizeDelta = new Vector2(100, 100);
            iconRect.anchoredPosition = Vector2.zero;

            rewardIcon = iconObj.AddComponent<Image>();
            rewardIcon.color = Color.white;

            // Reward title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(rewardPanel.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.8f);
            titleRect.anchorMax = new Vector2(1, 0.95f);
            titleRect.offsetMin = new Vector2(UIDesignSystem.SpacingL, 0);
            titleRect.offsetMax = new Vector2(-UIDesignSystem.SpacingL, 0);

            rewardTitleText = titleObj.AddComponent<TextMeshProUGUI>();
            rewardTitleText.text = "Reward!";
            rewardTitleText.fontSize = UIDesignSystem.FontSizeTitle;  // 48px
            rewardTitleText.fontStyle = FontStyles.Bold;
            rewardTitleText.alignment = TextAlignmentOptions.Center;
            rewardTitleText.color = UIDesignSystem.AccentGold;

            // Reward amount
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(rewardPanel.transform, false);
            RectTransform amountRect = amountObj.AddComponent<RectTransform>();
            amountRect.anchorMin = new Vector2(0, 0.3f);
            amountRect.anchorMax = new Vector2(1, 0.5f);
            amountRect.offsetMin = new Vector2(UIDesignSystem.SpacingL, 0);
            amountRect.offsetMax = new Vector2(-UIDesignSystem.SpacingL, 0);

            rewardAmountText = amountObj.AddComponent<TextMeshProUGUI>();
            rewardAmountText.text = "+$1,000";
            rewardAmountText.fontSize = UIDesignSystem.FontSizeLarge;  // 40px
            rewardAmountText.fontStyle = FontStyles.Bold;
            rewardAmountText.alignment = TextAlignmentOptions.Center;
            rewardAmountText.color = UIDesignSystem.MoneyGreen;

            // Reward description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(rewardPanel.transform, false);
            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0.15f);
            descRect.anchorMax = new Vector2(1, 0.3f);
            descRect.offsetMin = new Vector2(UIDesignSystem.SpacingL, 0);
            descRect.offsetMax = new Vector2(-UIDesignSystem.SpacingL, 0);

            rewardDescText = descObj.AddComponent<TextMeshProUGUI>();
            rewardDescText.text = "A burst of cash!";
            rewardDescText.fontSize = UIDesignSystem.FontSizeBody;  // 28px
            rewardDescText.alignment = TextAlignmentOptions.Center;
            rewardDescText.color = UIDesignSystem.TextSecondary;
            rewardDescText.richText = true;

            // Collect button
            GameObject collectObj = new GameObject("CollectButton");
            collectObj.transform.SetParent(rewardPanel.transform, false);

            RectTransform collectRect = collectObj.AddComponent<RectTransform>();
            collectRect.anchorMin = new Vector2(0.15f, 0.02f);
            collectRect.anchorMax = new Vector2(0.85f, 0.14f);
            collectRect.offsetMin = Vector2.zero;
            collectRect.offsetMax = Vector2.zero;

            Image collectBg = collectObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonGreen != null)
            {
                collectBg.sprite = guiAssets.buttonGreen;
                collectBg.type = Image.Type.Sliced;
            }
            collectBg.color = UIDesignSystem.ButtonSuccess;

            collectButton = collectObj.AddComponent<Button>();
            collectButton.onClick.AddListener(OnCollectClicked);

            // Collect button text
            GameObject collectTextObj = new GameObject("Text");
            collectTextObj.transform.SetParent(collectObj.transform, false);
            RectTransform collectTextRect = collectTextObj.AddComponent<RectTransform>();
            collectTextRect.anchorMin = Vector2.zero;
            collectTextRect.anchorMax = Vector2.one;
            collectTextRect.offsetMin = Vector2.zero;
            collectTextRect.offsetMax = Vector2.zero;

            collectButtonText = collectTextObj.AddComponent<TextMeshProUGUI>();
            collectButtonText.text = "COLLECT";
            collectButtonText.fontSize = UIDesignSystem.FontSizeHeader;  // 36px
            collectButtonText.fontStyle = FontStyles.Bold;
            collectButtonText.alignment = TextAlignmentOptions.Center;
            collectButtonText.color = Color.white;

            rewardPanel.SetActive(false);
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
                if (tmp != null)
                {
                    tmp.font = sharedFont;
                    GameUI.ApplyTextOutline(tmp);
                }
            }
        }
    }
}
