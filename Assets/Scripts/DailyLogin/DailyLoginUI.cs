using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.UI;
using MoreMountains.Feedbacks;

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
        private Button diceButton;  // The clickable dice itself

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
        [SerializeField] private float diceSpinDuration = 1.2f; // 20% slower for more anticipation
        [SerializeField] private float diceJumpHeight = 100f;
        [SerializeField] private int diceSpinRotations = 2;

        private DailyReward currentReward;
        private bool isAnimating = false;
        [System.NonSerialized] private bool isBuilt = false;
        private int currentDiceValue = 6;  // The rolled dice value (1-6)
        private Transform diceFaceContainer;  // Container for dice dots that can be updated
        private Image diceGlowImage;  // Glow behind dice for animations
        private GameObject titleRibbonObj;  // Title ribbon for polish
        private Image[] streakDayGlows;  // Glow effects for streak days
        private GameObject alreadyRolledPanel;  // Panel shown when player already rolled today
        private TextMeshProUGUI alreadyRolledText;  // Text for already rolled message
        private TextMeshProUGUI countdownText;  // Countdown to next roll
        private MMF_Player rollFeedback;  // Feel feedback for dice roll
        private MMF_Player collectFeedback;  // Feel feedback for collect
        private MMF_Player jackpotFeedback;  // Feel feedback for jackpot

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

            // Clear prefab reference if mainPanel is assigned to a prefab asset (not a scene object)
            // This ensures BuildUI() will create a fresh panel instead of using the invalid reference
            if (mainPanel != null && !mainPanel.scene.IsValid())
            {
                Debug.Log("[DailyLoginUI] Clearing prefab reference from mainPanel");
                mainPanel = null;
            }

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

            // Only show panel if the feature is unlocked via skill tree
            if (!IsFeatureUnlocked())
            {
                UpdateHUDIcon();
                yield break;
            }

            if (DailyLoginManager.Instance != null && DailyLoginManager.Instance.CanRollToday)
            {
                ShowPanel();
            }
            else
            {
                UpdateHUDIcon();
            }
        }

        /// <summary>
        /// Checks if the Daily Login feature is unlocked via skill tree.
        /// </summary>
        private bool IsFeatureUnlocked()
        {
            if (Skills.SkillTreeManager.Instance == null) return false;
            return Skills.SkillTreeManager.Instance.IsNodeUnlocked(Core.SkillNodeId.FU_DailyLogin);
        }

        private void OnDestroy()
        {
            // Always unregister from PopupManager when destroyed
            if (Core.PopupManager.Instance != null)
                Core.PopupManager.Instance.RegisterPopupClosed("DailyLoginUI");

            if (DailyLoginManager.Instance != null)
            {
                DailyLoginManager.Instance.OnDailyRewardAvailable -= OnRewardAvailable;
                DailyLoginManager.Instance.OnStreakUpdated -= OnStreakUpdated;
            }
        }

        private void OnRewardAvailable()
        {
            UpdateHUDIcon();
            // Only show panel if feature is unlocked
            if (IsFeatureUnlocked())
            {
                ShowPanel();
            }
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
            Debug.Log($"[DailyLoginUI] ShowPanel called. isBuilt: {isBuilt}");

            // Build UI if needed (do this first, before checking mainPanel)
            if (!isBuilt)
            {
                Debug.Log("[DailyLoginUI] Building UI...");
                BuildUI();
            }

            if (mainPanel == null)
            {
                Debug.LogError("[DailyLoginUI] mainPanel is null after BuildUI!");
                return;
            }
            Debug.Log("[DailyLoginUI] mainPanel exists, showing panel...");

            // Use fallback values if no manager is available
            int currentStreak = DailyLoginManager.Instance != null ? DailyLoginManager.Instance.CurrentStreakDay : 1;
            bool canRoll = DailyLoginManager.Instance != null ? DailyLoginManager.Instance.CanRollToday : true;

            // Update streak display
            UpdateStreakDisplay(currentStreak);

            // Show/hide the "already rolled" panel based on whether player can roll
            if (alreadyRolledPanel != null)
            {
                alreadyRolledPanel.SetActive(!canRoll);
                if (!canRoll)
                {
                    UpdateCountdownDisplay();
                    StartCoroutine(CountdownUpdateRoutine());
                }
            }

            // Show/hide dice and roll elements based on roll availability
            if (diceContainer != null)
            {
                diceContainer.gameObject.SetActive(canRoll);
            }

            // Reset roll button and dice button
            if (rollButton != null)
            {
                rollButton.interactable = canRoll;
            }
            if (diceButton != null)
            {
                diceButton.interactable = canRoll;
            }
            if (rollButtonText != null)
            {
                rollButtonText.text = canRoll ? "ROLL!" : "ROLLED";
            }

            // Hide reward panel
            if (rewardPanel != null) rewardPanel.SetActive(false);

            // Ensure popup is rendered on top of other UI elements (like menu button)
            mainPanel.transform.SetAsLastSibling();

            // Show main panel with animated entrance
            UIDesignSystem.AnimateShowPanel(mainPanel, panelCanvasGroup);

            // Animate the title ribbon
            if (titleRibbonObj != null)
            {
                titleRibbonObj.transform.localScale = new Vector3(0.8f, 1f, 1f);
                titleRibbonObj.transform.DOScale(1f, UIDesignSystem.AnimFadeIn).SetEase(Ease.OutBack);
            }

            // Register with PopupManager
            if (Core.PopupManager.Instance != null)
                Core.PopupManager.Instance.RegisterPopupOpen("DailyLoginUI");
        }

        /// <summary>
        /// Updates the countdown display showing time until next roll.
        /// </summary>
        private void UpdateCountdownDisplay()
        {
            if (countdownText == null) return;

            // Calculate time until midnight (next day)
            System.DateTime now = System.DateTime.Now;
            System.DateTime tomorrow = now.Date.AddDays(1);
            System.TimeSpan timeUntilTomorrow = tomorrow - now;

            int hours = timeUntilTomorrow.Hours;
            int minutes = timeUntilTomorrow.Minutes;
            int seconds = timeUntilTomorrow.Seconds;

            countdownText.text = $"Next roll in: <color=#FFD700>{hours:D2}:{minutes:D2}:{seconds:D2}</color>";
        }

        /// <summary>
        /// Coroutine to update the countdown every second in real-time.
        /// </summary>
        private IEnumerator CountdownUpdateRoutine()
        {
            while (mainPanel != null && mainPanel.activeSelf)
            {
                UpdateCountdownDisplay();
                yield return new WaitForSecondsRealtime(1f);
            }
        }

        /// <summary>
        /// Hides the daily login panel.
        /// </summary>
        public void HidePanel()
        {
            // Always unregister from PopupManager first, even if panel is null
            if (Core.PopupManager.Instance != null)
                Core.PopupManager.Instance.RegisterPopupClosed("DailyLoginUI");

            if (mainPanel == null) return;

            // Use design system animation
            UIDesignSystem.AnimateHidePanel(mainPanel, panelCanvasGroup, () =>
            {
                UpdateHUDIcon();
            });
        }

        /// <summary>
        /// Toggles the daily login panel visibility.
        /// </summary>
        public void Toggle()
        {
            Debug.Log($"[DailyLoginUI] Toggle called. mainPanel: {mainPanel != null}, activeSelf: {(mainPanel != null ? mainPanel.activeSelf.ToString() : "N/A")}");
            if (mainPanel != null && mainPanel.activeSelf)
                HidePanel();
            else
                ShowPanel();
        }


        private void OnRollClicked()
        {
            if (isAnimating) return;

            // Check if player can roll today
            if (DailyLoginManager.Instance != null && !DailyLoginManager.Instance.CanRollToday)
            {
                Debug.Log("[DailyLoginUI] Already rolled today!");
                return;
            }

            // Roll the dice! Generate random value 1-6
            currentDiceValue = Random.Range(1, 7);

            // Generate the reward
            if (DailyLoginManager.Instance != null)
            {
                currentReward = DailyLoginManager.Instance.GenerateReward();
            }
            else
            {
                // Fallback reward for testing when no manager is available
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

            // Special handling for ExtraDice - dice roll value = number of dice
            if (currentReward.type == DailyRewardType.ExtraDice)
            {
                currentReward.amount = currentDiceValue;  // 1-6 dice based on roll
                currentReward.title = currentDiceValue == 6 ? "JACKPOT!" : "EXTRA DICE!";
                currentReward.description = $"You rolled a {currentDiceValue}!";
            }
            else
            {
                // Apply dice value multiplier to other reward types
                // Dice value acts as a multiplier: 1 = 1x, 2 = 1.5x, 3 = 2x, 4 = 2.5x, 5 = 3x, 6 = 4x (JACKPOT!)
                float diceMultiplier = GetDiceMultiplier(currentDiceValue);
                currentReward.amount *= diceMultiplier;

                // Update reward description to show dice bonus
                if (currentDiceValue == 6)
                {
                    currentReward.title = "JACKPOT!";
                    currentReward.description = $"You rolled a 6! {diceMultiplier}x bonus!";
                }
                else if (currentDiceValue >= 4)
                {
                    currentReward.title = $"Great Roll! ({currentDiceValue})";
                    currentReward.description = $"{diceMultiplier}x bonus!";
                }
                else
                {
                    currentReward.title = $"You Rolled: {currentDiceValue}";
                    currentReward.description = diceMultiplier > 1f ? $"{diceMultiplier}x bonus!" : "Better luck tomorrow!";
                }
            }

            // Start roll animation
            StartCoroutine(PlayRollAnimation());
        }

        /// <summary>
        /// Returns multiplier based on dice value.
        /// </summary>
        private float GetDiceMultiplier(int diceValue)
        {
            switch (diceValue)
            {
                case 1: return 1.0f;
                case 2: return 1.5f;
                case 3: return 2.0f;
                case 4: return 2.5f;
                case 5: return 3.0f;
                case 6: return 4.0f;  // Jackpot!
                default: return 1.0f;
            }
        }

        private IEnumerator PlayRollAnimation()
        {
            isAnimating = true;

            // Play roll feedback (haptics + effects)
            PlayRollFeedback();

            // Disable roll button and dice button
            if (rollButton != null) rollButton.interactable = false;
            if (diceButton != null) diceButton.interactable = false;

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

            // Update dice face to show the rolled value!
            UpdateDiceFace(currentDiceValue);

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

            // Special effect for high rolls
            if (currentDiceValue >= 5)
            {
                // Jackpot/high roll effect - flash the dice gold
                if (diceImage != null)
                {
                    diceImage.DOColor(Color.white, 0.1f).SetLoops(4, LoopType.Yoyo);
                }

                // Play jackpot feedback for 6
                if (currentDiceValue == 6)
                {
                    PlayJackpotFeedback();
                }
            }

            yield return new WaitForSeconds(0.3f);

            // Show reward panel
            ShowRewardPanel();

            isAnimating = false;
        }

        /// <summary>
        /// Updates the dice face to show the specified value (1-6).
        /// </summary>
        private void UpdateDiceFace(int value)
        {
            if (diceFaceContainer == null) return;

            // Clear existing dots
            foreach (Transform child in diceFaceContainer)
            {
                Destroy(child.gameObject);
            }

            // Create new dots for this value
            CreateStylizedDiceDots(diceFaceContainer, value);
        }

        private void ShowRewardPanel()
        {
            if (rewardPanel == null || currentReward == null) return;

            // Update reward display - format dice roll info nicely
            if (rewardTitleText != null)
            {
                // Simple clean title without emojis
                if (currentDiceValue == 6)
                    rewardTitleText.text = "JACKPOT!";
                else if (currentDiceValue >= 4)
                    rewardTitleText.text = $"NICE ROLL!";
                else
                    rewardTitleText.text = "REWARD!";
            }

            if (rewardDescText != null)
            {
                string desc = $"You rolled a {currentDiceValue}! ";
                float mult = GetDiceMultiplier(currentDiceValue);
                if (mult > 1f)
                    desc += $"<color=#FFD700>{mult}x Bonus!</color>";
                if (currentReward.streakMultiplier > 1f)
                    desc += $"\n<color=#FFD700>+Streak x{currentReward.streakMultiplier}!</color>";
                rewardDescText.text = desc;
            }

            if (rewardAmountText != null)
            {
                string amountStr = "";
                switch (currentReward.type)
                {
                    case DailyRewardType.Money:
                        amountStr = $"+${GameUI.FormatNumber(currentReward.amount)}";
                        rewardAmountText.color = UIDesignSystem.MoneyGreen;
                        break;
                    case DailyRewardType.DarkMatter:
                        amountStr = $"+{GameUI.FormatNumber(currentReward.amount)} DM";
                        rewardAmountText.color = UIDesignSystem.DarkMatterPurple;
                        break;
                    case DailyRewardType.MoneyBoost:
                        amountStr = $"+{currentReward.amount}% Money\nfor {currentReward.boostDuration / 60}min";
                        rewardAmountText.color = UIDesignSystem.MoneyGreen;
                        break;
                    case DailyRewardType.DMBoost:
                        amountStr = $"+{currentReward.amount}% DM\nfor {currentReward.boostDuration / 60}min";
                        rewardAmountText.color = UIDesignSystem.DarkMatterPurple;
                        break;
                    case DailyRewardType.JackpotToken:
                        amountStr = $"+{currentReward.amount} Jackpot Token";
                        rewardAmountText.color = UIDesignSystem.AccentGold;
                        break;
                    case DailyRewardType.ExtraDice:
                        int diceCount = (int)currentReward.amount;
                        amountStr = $"+{diceCount} {(diceCount == 1 ? "Dice" : "Dice")}";
                        rewardAmountText.color = UIDesignSystem.AccentGold;
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
                        rewardIcon.sprite = darkMatterIcon ?? guiAssets?.iconStar;
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

            // Show reward panel with pop-in animation
            UIDesignSystem.AnimateShowPanel(rewardPanel, rewardCanvasGroup);

            // Additional celebration for jackpot!
            if (currentDiceValue == 6)
            {
                // Camera shake for jackpot
                Camera cam = Camera.main;
                if (cam != null)
                {
                    cam.transform.DOShakePosition(0.3f, 0.06f, 20, 90f, false, true);
                }

                // Play special sound if available
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayPurchaseSound();
                }
            }

            // Success pop on the amount text
            if (rewardAmountText != null)
            {
                UIDesignSystem.AnimateSuccessPop(rewardAmountText.transform);
            }
        }

        private void OnCollectClicked()
        {
            if (currentReward == null) return;

            // Play collect feedback (haptics + effects)
            PlayCollectFeedback();

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
                    case DailyRewardType.ExtraDice:
                        int diceCount = (int)currentReward.amount;
                        text = $"+{diceCount} Dice!";
                        color = new Color(1f, 0.85f, 0.2f);
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

                // Kill any existing animations
                streakNodes[i].transform.DOKill();
                if (streakDayGlows != null && streakDayGlows[i] != null)
                {
                    streakDayGlows[i].transform.DOKill();
                    streakDayGlows[i].DOKill();
                }

                if (dayNum < currentDay)
                {
                    // Past days - filled with checkmark-like styling
                    streakNodes[i].color = streakFilledColor;
                    streakNodes[i].transform.localScale = Vector3.one;

                    // Hide glow for past days
                    if (streakDayGlows != null && streakDayGlows[i] != null)
                    {
                        streakDayGlows[i].color = new Color(streakFilledColor.r, streakFilledColor.g, streakFilledColor.b, 0f);
                    }
                }
                else if (dayNum == currentDay)
                {
                    // Today - special color with pulse and glow
                    streakNodes[i].color = streakTodayColor;
                    streakNodes[i].transform.DOScale(1.15f, UIDesignSystem.AnimPulse).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

                    // Animate glow for today
                    if (streakDayGlows != null && streakDayGlows[i] != null)
                    {
                        streakDayGlows[i].color = new Color(streakTodayColor.r, streakTodayColor.g, streakTodayColor.b, 0.4f);
                        streakDayGlows[i].transform.DOScale(1.15f, UIDesignSystem.AnimGlow).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                        streakDayGlows[i].DOFade(0.2f, UIDesignSystem.AnimGlow).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                    }
                }
                else
                {
                    // Future days - empty/dim
                    streakNodes[i].color = streakEmptyColor;
                    streakNodes[i].transform.localScale = Vector3.one;

                    // Hide glow for future days
                    if (streakDayGlows != null && streakDayGlows[i] != null)
                    {
                        streakDayGlows[i].color = new Color(0, 0, 0, 0);
                    }
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

            // Add button to background so clicking outside content closes the panel
            Button bgButton = mainPanel.AddComponent<Button>();
            bgButton.targetGraphic = panelBg;
            bgButton.onClick.AddListener(HidePanel);
            // Make background button have no visual transitions
            var bgColors = bgButton.colors;
            bgColors.normalColor = Color.white;
            bgColors.highlightedColor = Color.white;
            bgColors.pressedColor = Color.white;
            bgColors.selectedColor = Color.white;
            bgButton.colors = bgColors;

            panelCanvasGroup = mainPanel.AddComponent<CanvasGroup>();

            // Ensure it renders above other UI
            Canvas panelCanvas = mainPanel.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 300;
            mainPanel.AddComponent<GraphicRaycaster>();

            // Create title section with ribbon
            CreateTitleSection();

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
            diceContainer.anchorMin = new Vector2(0.5f, 0.35f);
            diceContainer.anchorMax = new Vector2(0.5f, 0.35f);
            diceContainer.sizeDelta = new Vector2(320, 320);  // Large square dice
            diceContainer.anchoredPosition = Vector2.zero;

            // Dice shadow - elliptical shadow beneath
            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(diceObj.transform, false);
            RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
            shadowRect.anchorMin = new Vector2(0.5f, 0);
            shadowRect.anchorMax = new Vector2(0.5f, 0);
            shadowRect.sizeDelta = new Vector2(260, 50);
            shadowRect.anchoredPosition = new Vector2(15, -40);

            diceShadow = shadowObj.AddComponent<Image>();
            diceShadow.color = new Color(0f, 0f, 0f, 0.5f);

            // Glow effect behind dice
            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(diceObj.transform, false);
            RectTransform glowRect = glowObj.AddComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.5f, 0.5f);
            glowRect.anchorMax = new Vector2(0.5f, 0.5f);
            glowRect.sizeDelta = new Vector2(400, 400);
            glowRect.anchoredPosition = Vector2.zero;

            Image glowImg = glowObj.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.85f, 0.3f, 0.3f);
            glowObj.transform.DOScale(1.15f, 1.2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            // 3D dice - right side (visible 3D edge)
            GameObject diceRightObj = new GameObject("DiceRight");
            diceRightObj.transform.SetParent(diceObj.transform, false);
            RectTransform diceRightRect = diceRightObj.AddComponent<RectTransform>();
            diceRightRect.anchorMin = new Vector2(0.5f, 0.5f);
            diceRightRect.anchorMax = new Vector2(0.5f, 0.5f);
            diceRightRect.sizeDelta = new Vector2(30, 280);
            diceRightRect.anchoredPosition = new Vector2(155, -15);
            diceRightRect.localRotation = Quaternion.Euler(0, 0, -8);

            Image diceRightImg = diceRightObj.AddComponent<Image>();
            diceRightImg.color = new Color(0.85f, 0.65f, 0.1f);

            // 3D dice - bottom side (visible 3D edge)
            GameObject diceBottomObj = new GameObject("DiceBottom");
            diceBottomObj.transform.SetParent(diceObj.transform, false);
            RectTransform diceBottomRect = diceBottomObj.AddComponent<RectTransform>();
            diceBottomRect.anchorMin = new Vector2(0.5f, 0.5f);
            diceBottomRect.anchorMax = new Vector2(0.5f, 0.5f);
            diceBottomRect.sizeDelta = new Vector2(280, 30);
            diceBottomRect.anchoredPosition = new Vector2(15, -155);
            diceBottomRect.localRotation = Quaternion.Euler(0, 0, -5);

            Image diceBottomImg = diceBottomObj.AddComponent<Image>();
            diceBottomImg.color = new Color(0.7f, 0.5f, 0.08f);

            // Main dice face - bright golden using design system
            GameObject diceImgObj = new GameObject("DiceImage");
            diceImgObj.transform.SetParent(diceObj.transform, false);
            RectTransform diceImgRect = diceImgObj.AddComponent<RectTransform>();
            diceImgRect.anchorMin = new Vector2(0.5f, 0.5f);
            diceImgRect.anchorMax = new Vector2(0.5f, 0.5f);
            diceImgRect.sizeDelta = new Vector2(280, 280);
            diceImgRect.anchoredPosition = Vector2.zero;

            diceImage = diceImgObj.AddComponent<Image>();
            diceImage.color = UIDesignSystem.AccentGold;  // Bright golden dice

            // Add highlight edge (top-left)
            Outline diceHighlight = diceImgObj.AddComponent<Outline>();
            diceHighlight.effectColor = new Color(1f, 0.95f, 0.6f);
            diceHighlight.effectDistance = new Vector2(-4, 4);

            // Inner border for depth
            GameObject innerBorderObj = new GameObject("InnerBorder");
            innerBorderObj.transform.SetParent(diceImgObj.transform, false);
            RectTransform innerBorderRect = innerBorderObj.AddComponent<RectTransform>();
            innerBorderRect.anchorMin = Vector2.zero;
            innerBorderRect.anchorMax = Vector2.one;
            innerBorderRect.offsetMin = new Vector2(12, 12);
            innerBorderRect.offsetMax = new Vector2(-12, -12);

            Image innerBorderImg = innerBorderObj.AddComponent<Image>();
            innerBorderImg.color = new Color(0.95f, 0.8f, 0.25f);

            // Dice face inner area
            GameObject diceFaceObj = new GameObject("DiceFace");
            diceFaceObj.transform.SetParent(innerBorderObj.transform, false);
            RectTransform diceFaceRect = diceFaceObj.AddComponent<RectTransform>();
            diceFaceRect.anchorMin = Vector2.zero;
            diceFaceRect.anchorMax = Vector2.one;
            diceFaceRect.offsetMin = new Vector2(8, 8);
            diceFaceRect.offsetMax = new Vector2(-8, -8);

            Image diceFaceImg = diceFaceObj.AddComponent<Image>();
            diceFaceImg.color = new Color(1f, 0.9f, 0.35f);

            // Store reference to dice face for updating after roll
            diceFaceContainer = diceFaceObj.transform;

            // Dice dots (show a 6 initially) - larger dots with better styling
            CreateStylizedDiceDots(diceFaceObj.transform, 6);

            // Make the entire dice container clickable
            diceButton = diceObj.AddComponent<Button>();
            diceButton.targetGraphic = diceImage;
            diceButton.onClick.AddListener(OnRollClicked);

            // Set up button colors
            var btnColors = diceButton.colors;
            btnColors.highlightedColor = Color.white;
            btnColors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
            diceButton.colors = btnColors;

            // Pulse animation on the dice using design system timing
            diceObj.transform.DOScale(1.03f, UIDesignSystem.AnimPulse).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            // Slight rotation wiggle for attention
            diceObj.transform.DORotate(new Vector3(0, 0, 2f), 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            // Add "TAP TO ROLL!" text below the dice
            GameObject rollMeObj = new GameObject("RollMeText");
            rollMeObj.transform.SetParent(diceObj.transform, false);
            RectTransform rollMeRect = rollMeObj.AddComponent<RectTransform>();
            rollMeRect.anchorMin = new Vector2(0.5f, 0);
            rollMeRect.anchorMax = new Vector2(0.5f, 0);
            rollMeRect.sizeDelta = new Vector2(400, 60);
            rollMeRect.anchoredPosition = new Vector2(0, -70);

            TextMeshProUGUI rollMeText = rollMeObj.AddComponent<TextMeshProUGUI>();
            rollMeText.text = "👆 TAP DICE TO ROLL! 👆";
            rollMeText.fontSize = UIDesignSystem.FontSizeLarge;  // 32px
            rollMeText.fontStyle = FontStyles.Bold;
            rollMeText.alignment = TextAlignmentOptions.Center;
            rollMeText.color = UIDesignSystem.AccentGold;

            // Pulse the tap text
            rollMeObj.transform.DOScale(1.08f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            // Streak container
            CreateStreakDisplay();

            // Close button (X in top right)
            CreateCloseButton();

            // Reward panel
            CreateRewardPanel();

            // Already rolled panel (shown when player has already rolled today)
            CreateAlreadyRolledPanel();

            // Setup MMFeedbacks for juice effects
            SetupFeedbacks();
        }

        private void CreateTitleSection()
        {
            // Title ribbon container
            titleRibbonObj = new GameObject("TitleRibbon");
            titleRibbonObj.transform.SetParent(mainPanel.transform, false);
            RectTransform ribbonRect = titleRibbonObj.AddComponent<RectTransform>();
            ribbonRect.anchorMin = new Vector2(0.5f, 0.85f);
            ribbonRect.anchorMax = new Vector2(0.5f, 0.85f);
            ribbonRect.sizeDelta = new Vector2(550, 110);
            ribbonRect.anchoredPosition = new Vector2(0, 30);

            // Ribbon background
            Image ribbonBg = titleRibbonObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.ribbonYellow != null)
            {
                ribbonBg.sprite = guiAssets.ribbonYellow;
                ribbonBg.type = Image.Type.Sliced;
                ribbonBg.color = Color.white;
            }
            else
            {
                ribbonBg.color = UIDesignSystem.AccentGold;
            }

            // Dice icon on the left
            GameObject diceIconObj = new GameObject("DiceIcon");
            diceIconObj.transform.SetParent(titleRibbonObj.transform, false);
            RectTransform diceIconRect = diceIconObj.AddComponent<RectTransform>();
            diceIconRect.anchorMin = new Vector2(0, 0.5f);
            diceIconRect.anchorMax = new Vector2(0, 0.5f);
            diceIconRect.sizeDelta = new Vector2(60, 60);
            diceIconRect.anchoredPosition = new Vector2(50, 0);

            Image diceIconImg = diceIconObj.AddComponent<Image>();
            diceIconImg.color = new Color(0.2f, 0.15f, 0.05f);

            // Add some dice dots to the icon
            CreateMiniDiceDots(diceIconObj.transform, 6);

            // Animate the dice icon
            diceIconObj.transform.DORotate(new Vector3(0, 0, 10f), 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            // Title text
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(titleRibbonObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(80, 10);
            titleRect.offsetMax = new Vector2(-20, -10);

            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "DAILY ROLL";
            titleText.fontSize = UIDesignSystem.FontSizeTitle;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.25f, 0.15f, 0.05f);  // Dark text on gold ribbon

            // Glow behind ribbon
            GameObject glowObj = new GameObject("RibbonGlow");
            glowObj.transform.SetParent(mainPanel.transform, false);
            glowObj.transform.SetSiblingIndex(titleRibbonObj.transform.GetSiblingIndex());  // Behind ribbon
            RectTransform glowRect = glowObj.AddComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.5f, 0.85f);
            glowRect.anchorMax = new Vector2(0.5f, 0.85f);
            glowRect.sizeDelta = new Vector2(650, 150);
            glowRect.anchoredPosition = new Vector2(0, 30);

            Image glowImg = glowObj.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.9f, 0.3f, 0.3f);
            glowObj.transform.DOScale(1.1f, UIDesignSystem.AnimGlow).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        private void CreateMiniDiceDots(Transform parent, int value)
        {
            // Mini dice dots for the icon
            Vector2[][] dotPatterns = new Vector2[][]
            {
                new Vector2[] { Vector2.zero },
                new Vector2[] { new Vector2(-0.3f, 0.3f), new Vector2(0.3f, -0.3f) },
                new Vector2[] { new Vector2(-0.3f, 0.3f), Vector2.zero, new Vector2(0.3f, -0.3f) },
                new Vector2[] { new Vector2(-0.3f, 0.3f), new Vector2(0.3f, 0.3f), new Vector2(-0.3f, -0.3f), new Vector2(0.3f, -0.3f) },
                new Vector2[] { new Vector2(-0.3f, 0.3f), new Vector2(0.3f, 0.3f), Vector2.zero, new Vector2(-0.3f, -0.3f), new Vector2(0.3f, -0.3f) },
                new Vector2[] { new Vector2(-0.3f, 0.3f), new Vector2(0.3f, 0.3f), new Vector2(-0.3f, 0f), new Vector2(0.3f, 0f), new Vector2(-0.3f, -0.3f), new Vector2(0.3f, -0.3f) }
            };

            int index = Mathf.Clamp(value - 1, 0, 5);
            Vector2[] positions = dotPatterns[index];

            foreach (Vector2 pos in positions)
            {
                GameObject dot = new GameObject("Dot");
                dot.transform.SetParent(parent, false);
                RectTransform dotRt = dot.AddComponent<RectTransform>();
                dotRt.anchorMin = new Vector2(0.5f, 0.5f);
                dotRt.anchorMax = new Vector2(0.5f, 0.5f);
                dotRt.sizeDelta = new Vector2(8, 8);
                dotRt.anchoredPosition = pos * 20f;

                Image dotImg = dot.AddComponent<Image>();
                dotImg.color = new Color(0.95f, 0.85f, 0.4f);
            }
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
            if (guiAssets != null && guiAssets.buttonRed != null)
            {
                closeBg.sprite = guiAssets.buttonRed;
                closeBg.type = Image.Type.Sliced;
                closeBg.color = Color.white;
            }
            else
            {
                closeBg.color = UIDesignSystem.ButtonDanger;
            }

            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeBg;
            closeBtn.onClick.AddListener(HidePanel);

            var colors = closeBtn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            closeBtn.colors = colors;

            // X icon or text
            GameObject xObj = new GameObject("X");
            xObj.transform.SetParent(closeObj.transform, false);
            RectTransform xRect = xObj.AddComponent<RectTransform>();
            xRect.anchorMin = Vector2.zero;
            xRect.anchorMax = Vector2.one;
            xRect.offsetMin = Vector2.zero;
            xRect.offsetMax = Vector2.zero;

            if (guiAssets != null && guiAssets.iconClose != null)
            {
                Image xImg = xObj.AddComponent<Image>();
                xImg.sprite = guiAssets.iconClose;
                xImg.preserveAspect = true;
                xImg.color = Color.white;
            }
            else
            {
                TextMeshProUGUI xTmp = xObj.AddComponent<TextMeshProUGUI>();
                xTmp.text = "X";
                xTmp.fontSize = UIDesignSystem.FontSizeTitle;
                xTmp.fontStyle = FontStyles.Bold;
                xTmp.alignment = TextAlignmentOptions.Center;
                xTmp.color = Color.white;
            }

            // Add polish for press animations
            if (UIPolishManager.Instance != null)
            {
                UIPolishManager.Instance.AddButtonPolish(closeBtn);
            }
        }

        private void CreateStylizedDiceDots(Transform parent, int value)
        {
            // Dice face positions for values 1-6 (normalized -1 to 1)
            Vector2[][] dotPatterns = new Vector2[][]
            {
                // 1 - center
                new Vector2[] { Vector2.zero },
                // 2 - diagonal
                new Vector2[] { new Vector2(-0.35f, 0.35f), new Vector2(0.35f, -0.35f) },
                // 3 - diagonal with center
                new Vector2[] { new Vector2(-0.35f, 0.35f), Vector2.zero, new Vector2(0.35f, -0.35f) },
                // 4 - corners
                new Vector2[] { new Vector2(-0.35f, 0.35f), new Vector2(0.35f, 0.35f),
                               new Vector2(-0.35f, -0.35f), new Vector2(0.35f, -0.35f) },
                // 5 - corners + center
                new Vector2[] { new Vector2(-0.35f, 0.35f), new Vector2(0.35f, 0.35f), Vector2.zero,
                               new Vector2(-0.35f, -0.35f), new Vector2(0.35f, -0.35f) },
                // 6 - two columns
                new Vector2[] { new Vector2(-0.35f, 0.35f), new Vector2(0.35f, 0.35f),
                               new Vector2(-0.35f, 0f), new Vector2(0.35f, 0f),
                               new Vector2(-0.35f, -0.35f), new Vector2(0.35f, -0.35f) }
            };

            int index = Mathf.Clamp(value - 1, 0, 5);
            Vector2[] positions = dotPatterns[index];

            RectTransform parentRt = parent.GetComponent<RectTransform>();
            float parentSize = 200f; // Use fixed size for consistent dots

            foreach (Vector2 pos in positions)
            {
                // Dot container for layered effect
                GameObject dotContainer = new GameObject("DotContainer");
                dotContainer.transform.SetParent(parent, false);

                RectTransform containerRt = dotContainer.AddComponent<RectTransform>();
                containerRt.anchorMin = new Vector2(0.5f, 0.5f);
                containerRt.anchorMax = new Vector2(0.5f, 0.5f);
                containerRt.sizeDelta = new Vector2(42, 42);
                containerRt.anchoredPosition = pos * parentSize;

                // Outer shadow/depth
                GameObject shadowDot = new GameObject("Shadow");
                shadowDot.transform.SetParent(dotContainer.transform, false);
                RectTransform shadowRt = shadowDot.AddComponent<RectTransform>();
                shadowRt.anchorMin = Vector2.zero;
                shadowRt.anchorMax = Vector2.one;
                shadowRt.offsetMin = new Vector2(-2, -4);
                shadowRt.offsetMax = new Vector2(2, 0);

                Image shadowImg = shadowDot.AddComponent<Image>();
                shadowImg.color = new Color(0.3f, 0.2f, 0.05f, 0.8f);

                // Main dot (dark indented look)
                GameObject mainDot = new GameObject("Dot");
                mainDot.transform.SetParent(dotContainer.transform, false);
                RectTransform dotRt = mainDot.AddComponent<RectTransform>();
                dotRt.anchorMin = Vector2.zero;
                dotRt.anchorMax = Vector2.one;
                dotRt.offsetMin = Vector2.zero;
                dotRt.offsetMax = Vector2.zero;

                Image dotImg = mainDot.AddComponent<Image>();
                dotImg.color = new Color(0.15f, 0.1f, 0.03f);

                // Inner highlight (subtle 3D effect)
                GameObject highlightDot = new GameObject("Highlight");
                highlightDot.transform.SetParent(mainDot.transform, false);
                RectTransform highlightRt = highlightDot.AddComponent<RectTransform>();
                highlightRt.anchorMin = new Vector2(0.15f, 0.5f);
                highlightRt.anchorMax = new Vector2(0.45f, 0.85f);
                highlightRt.offsetMin = Vector2.zero;
                highlightRt.offsetMax = Vector2.zero;

                Image highlightImg = highlightDot.AddComponent<Image>();
                highlightImg.color = new Color(0.25f, 0.18f, 0.08f, 0.6f);
            }
        }

        private void CreateStreakDisplay()
        {
            // Streak label
            GameObject streakLabelObj = new GameObject("StreakLabel");
            streakLabelObj.transform.SetParent(mainPanel.transform, false);
            RectTransform streakLabelRect = streakLabelObj.AddComponent<RectTransform>();
            streakLabelRect.anchorMin = new Vector2(0, 0.16f);
            streakLabelRect.anchorMax = new Vector2(1, 0.20f);
            streakLabelRect.offsetMin = Vector2.zero;
            streakLabelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI streakLabelText = streakLabelObj.AddComponent<TextMeshProUGUI>();
            streakLabelText.text = "LOGIN STREAK";
            streakLabelText.fontSize = UIDesignSystem.FontSizeSmall;
            streakLabelText.fontStyle = FontStyles.Bold;
            streakLabelText.alignment = TextAlignmentOptions.Center;
            streakLabelText.color = UIDesignSystem.AccentGold;

            // Streak frame background
            GameObject streakFrameObj = new GameObject("StreakFrame");
            streakFrameObj.transform.SetParent(mainPanel.transform, false);
            RectTransform streakFrameRect = streakFrameObj.AddComponent<RectTransform>();
            streakFrameRect.anchorMin = new Vector2(0.03f, 0.03f);
            streakFrameRect.anchorMax = new Vector2(0.97f, 0.16f);
            streakFrameRect.offsetMin = Vector2.zero;
            streakFrameRect.offsetMax = Vector2.zero;

            Image streakFrameBg = streakFrameObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.horizontalFrame != null)
            {
                streakFrameBg.sprite = guiAssets.horizontalFrame;
                streakFrameBg.type = Image.Type.Sliced;
                streakFrameBg.color = Color.white;
            }
            else
            {
                streakFrameBg.color = new Color(0.1f, 0.08f, 0.15f, 0.9f);
            }

            GameObject streakObj = new GameObject("StreakContainer");
            streakObj.transform.SetParent(streakFrameObj.transform, false);
            streakContainer = streakObj.AddComponent<RectTransform>();
            streakContainer.anchorMin = Vector2.zero;
            streakContainer.anchorMax = Vector2.one;
            streakContainer.offsetMin = new Vector2(UIDesignSystem.SpacingM, UIDesignSystem.SpacingS);
            streakContainer.offsetMax = new Vector2(-UIDesignSystem.SpacingM, -UIDesignSystem.SpacingS);

            HorizontalLayoutGroup hlg = streakObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = UIDesignSystem.SpacingS;  // 8px for tighter fit
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            int streakLength = DailyLoginManager.Instance?.StreakLength ?? 7;
            streakNodes = new Image[streakLength];
            streakDayGlows = new Image[streakLength];

            for (int i = 0; i < streakLength; i++)
            {
                // Node container
                GameObject nodeContainer = new GameObject($"Day{i + 1}Container");
                nodeContainer.transform.SetParent(streakObj.transform, false);
                RectTransform containerRect = nodeContainer.AddComponent<RectTransform>();
                containerRect.sizeDelta = new Vector2(52, 68);

                // Glow behind (for today/claimable)
                GameObject glowObj = new GameObject("Glow");
                glowObj.transform.SetParent(nodeContainer.transform, false);
                RectTransform glowRect = glowObj.AddComponent<RectTransform>();
                glowRect.anchorMin = new Vector2(0.5f, 0.5f);
                glowRect.anchorMax = new Vector2(0.5f, 0.5f);
                glowRect.sizeDelta = new Vector2(70, 86);
                glowRect.anchoredPosition = Vector2.zero;

                Image glowImg = glowObj.AddComponent<Image>();
                glowImg.color = new Color(streakTodayColor.r, streakTodayColor.g, streakTodayColor.b, 0f);  // Start invisible
                streakDayGlows[i] = glowImg;

                // Main node
                GameObject nodeObj = new GameObject("Node");
                nodeObj.transform.SetParent(nodeContainer.transform, false);
                RectTransform nodeRect = nodeObj.AddComponent<RectTransform>();
                nodeRect.anchorMin = new Vector2(0.5f, 0.6f);
                nodeRect.anchorMax = new Vector2(0.5f, 0.6f);
                nodeRect.sizeDelta = new Vector2(48, 48);
                nodeRect.anchoredPosition = Vector2.zero;

                Image nodeImg = nodeObj.AddComponent<Image>();
                if (guiAssets != null && guiAssets.cardFrame != null)
                {
                    nodeImg.sprite = guiAssets.cardFrame;
                    nodeImg.type = Image.Type.Sliced;
                }
                nodeImg.color = streakEmptyColor;
                streakNodes[i] = nodeImg;

                // Day number inside node
                GameObject numObj = new GameObject("Number");
                numObj.transform.SetParent(nodeObj.transform, false);
                RectTransform numRect = numObj.AddComponent<RectTransform>();
                numRect.anchorMin = Vector2.zero;
                numRect.anchorMax = Vector2.one;
                numRect.offsetMin = Vector2.zero;
                numRect.offsetMax = Vector2.zero;

                TextMeshProUGUI numText = numObj.AddComponent<TextMeshProUGUI>();
                numText.text = (i + 1).ToString();
                numText.fontSize = UIDesignSystem.FontSizeSmall;
                numText.fontStyle = FontStyles.Bold;
                numText.alignment = TextAlignmentOptions.Center;
                numText.color = Color.white;

                // Day label below
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(nodeContainer.transform, false);
                RectTransform labelRect = labelObj.AddComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0.5f, 0);
                labelRect.anchorMax = new Vector2(0.5f, 0);
                labelRect.sizeDelta = new Vector2(50, 20);
                labelRect.anchoredPosition = new Vector2(0, 10);

                TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
                labelText.text = $"Day";
                labelText.fontSize = UIDesignSystem.FontSizeCaption;
                labelText.alignment = TextAlignmentOptions.Center;
                labelText.color = UIDesignSystem.TextMuted;
            }
        }

        private void CreateRewardPanel()
        {
            rewardPanel = new GameObject("RewardPanel");
            rewardPanel.transform.SetParent(mainPanel.transform, false);

            RectTransform rewardRect = rewardPanel.AddComponent<RectTransform>();
            rewardRect.anchorMin = new Vector2(0.05f, 0.15f);
            rewardRect.anchorMax = new Vector2(0.95f, 0.85f);
            rewardRect.offsetMin = Vector2.zero;
            rewardRect.offsetMax = Vector2.zero;

            // Use popup background from GUI assets
            Image rewardBg = rewardPanel.AddComponent<Image>();
            if (guiAssets != null && guiAssets.popupBackground != null)
            {
                rewardBg.sprite = guiAssets.popupBackground;
                rewardBg.type = Image.Type.Sliced;
                rewardBg.color = Color.white;
            }
            else
            {
                rewardBg.color = UIDesignSystem.PanelBgDark;
            }

            rewardCanvasGroup = rewardPanel.AddComponent<CanvasGroup>();

            // Title ribbon for reward panel
            GameObject rewardRibbonObj = new GameObject("RewardRibbon");
            rewardRibbonObj.transform.SetParent(rewardPanel.transform, false);
            RectTransform rewardRibbonRect = rewardRibbonObj.AddComponent<RectTransform>();
            rewardRibbonRect.anchorMin = new Vector2(0.5f, 0.88f);
            rewardRibbonRect.anchorMax = new Vector2(0.5f, 0.88f);
            rewardRibbonRect.sizeDelta = new Vector2(400, 80);
            rewardRibbonRect.anchoredPosition = Vector2.zero;

            Image rewardRibbonBg = rewardRibbonObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.ribbonYellow != null)
            {
                rewardRibbonBg.sprite = guiAssets.ribbonYellow;
                rewardRibbonBg.type = Image.Type.Sliced;
                rewardRibbonBg.color = Color.white;
            }
            else
            {
                rewardRibbonBg.color = UIDesignSystem.AccentGold;
            }

            // Reward title inside ribbon
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(rewardRibbonObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(10, 5);
            titleRect.offsetMax = new Vector2(-10, -5);

            rewardTitleText = titleObj.AddComponent<TextMeshProUGUI>();
            rewardTitleText.text = "REWARD!";
            rewardTitleText.fontSize = UIDesignSystem.FontSizeSubtitle;
            rewardTitleText.fontStyle = FontStyles.Bold;
            rewardTitleText.alignment = TextAlignmentOptions.Center;
            rewardTitleText.color = new Color(0.25f, 0.15f, 0.05f);  // Dark on gold
            rewardTitleText.enableAutoSizing = true;
            rewardTitleText.fontSizeMin = 28;
            rewardTitleText.fontSizeMax = 40;

            // Glow effect behind icon - larger and more prominent
            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(rewardPanel.transform, false);
            RectTransform glowRect = glowObj.AddComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.5f, 0.52f);
            glowRect.anchorMax = new Vector2(0.5f, 0.52f);
            glowRect.sizeDelta = new Vector2(280, 280);
            glowRect.anchoredPosition = Vector2.zero;

            Image glowImg = glowObj.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.9f, 0.3f, 0.5f);
            glowObj.transform.DOScale(1.25f, UIDesignSystem.AnimGlow).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            // Reward icon - much larger with frame
            GameObject iconContainerObj = new GameObject("IconContainer");
            iconContainerObj.transform.SetParent(rewardPanel.transform, false);
            RectTransform iconContainerRect = iconContainerObj.AddComponent<RectTransform>();
            iconContainerRect.anchorMin = new Vector2(0.5f, 0.52f);
            iconContainerRect.anchorMax = new Vector2(0.5f, 0.52f);
            iconContainerRect.sizeDelta = new Vector2(160, 160);
            iconContainerRect.anchoredPosition = Vector2.zero;

            // Icon frame
            Image iconFrameImg = iconContainerObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.cardFrame != null)
            {
                iconFrameImg.sprite = guiAssets.cardFrame;
                iconFrameImg.type = Image.Type.Sliced;
                iconFrameImg.color = UIDesignSystem.AccentGold;
            }
            else
            {
                iconFrameImg.color = new Color(0.9f, 0.7f, 0.1f);
            }

            // Bounce the icon container
            iconContainerObj.transform.DOScale(1.08f, UIDesignSystem.AnimPulse).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            // Icon inside container
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(iconContainerObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            rewardIcon = iconObj.AddComponent<Image>();
            rewardIcon.color = Color.white;
            rewardIcon.preserveAspect = true;

            // If no sprite, create a coin-like icon
            CreateDefaultRewardIcon(iconObj.transform);

            // Reward amount - very large and prominent
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(rewardPanel.transform, false);
            RectTransform amountRect = amountObj.AddComponent<RectTransform>();
            amountRect.anchorMin = new Vector2(0, 0.28f);
            amountRect.anchorMax = new Vector2(1, 0.42f);
            amountRect.offsetMin = new Vector2(UIDesignSystem.SpacingL, 0);
            amountRect.offsetMax = new Vector2(-UIDesignSystem.SpacingL, 0);

            rewardAmountText = amountObj.AddComponent<TextMeshProUGUI>();
            rewardAmountText.text = "+$1,000";
            rewardAmountText.fontSize = UIDesignSystem.FontSizeHero;
            rewardAmountText.fontStyle = FontStyles.Bold;
            rewardAmountText.alignment = TextAlignmentOptions.Center;
            rewardAmountText.color = UIDesignSystem.MoneyGreen;
            rewardAmountText.enableAutoSizing = true;
            rewardAmountText.fontSizeMin = 40;
            rewardAmountText.fontSizeMax = 72;

            // Reward description - more readable
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(rewardPanel.transform, false);
            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0.18f);
            descRect.anchorMax = new Vector2(1, 0.28f);
            descRect.offsetMin = new Vector2(UIDesignSystem.SpacingL, 0);
            descRect.offsetMax = new Vector2(-UIDesignSystem.SpacingL, 0);

            rewardDescText = descObj.AddComponent<TextMeshProUGUI>();
            rewardDescText.text = "A burst of cash!";
            rewardDescText.fontSize = UIDesignSystem.FontSizeBody;
            rewardDescText.alignment = TextAlignmentOptions.Center;
            rewardDescText.color = UIDesignSystem.TextSecondary;
            rewardDescText.richText = true;

            // Collect button - larger and more prominent
            GameObject collectObj = new GameObject("CollectButton");
            collectObj.transform.SetParent(rewardPanel.transform, false);

            RectTransform collectRect = collectObj.AddComponent<RectTransform>();
            collectRect.anchorMin = new Vector2(0.1f, 0.03f);
            collectRect.anchorMax = new Vector2(0.9f, 0.16f);
            collectRect.offsetMin = Vector2.zero;
            collectRect.offsetMax = Vector2.zero;

            Image collectBg = collectObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonGreen != null)
            {
                collectBg.sprite = guiAssets.buttonGreen;
                collectBg.type = Image.Type.Sliced;
                collectBg.color = Color.white;
            }
            else
            {
                collectBg.color = UIDesignSystem.ButtonSuccess;
            }

            collectButton = collectObj.AddComponent<Button>();
            collectButton.targetGraphic = collectBg;
            collectButton.onClick.AddListener(OnCollectClicked);

            // Button hover/press colors
            var btnColors = collectButton.colors;
            btnColors.normalColor = Color.white;
            btnColors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            btnColors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            collectButton.colors = btnColors;

            // Collect button text - larger
            GameObject collectTextObj = new GameObject("Text");
            collectTextObj.transform.SetParent(collectObj.transform, false);
            RectTransform collectTextRect = collectTextObj.AddComponent<RectTransform>();
            collectTextRect.anchorMin = Vector2.zero;
            collectTextRect.anchorMax = Vector2.one;
            collectTextRect.offsetMin = Vector2.zero;
            collectTextRect.offsetMax = Vector2.zero;

            collectButtonText = collectTextObj.AddComponent<TextMeshProUGUI>();
            collectButtonText.text = "COLLECT!";
            collectButtonText.fontSize = UIDesignSystem.FontSizeTitle;  // 48px - bigger button text
            collectButtonText.fontStyle = FontStyles.Bold;
            collectButtonText.alignment = TextAlignmentOptions.Center;
            collectButtonText.color = Color.white;

            // Apply text outline
            GameUI.ApplyTextOutline(collectButtonText);

            // Pulse animation on collect button
            collectObj.transform.DOScale(1.05f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            // Add polish for press animations
            if (UIPolishManager.Instance != null)
            {
                UIPolishManager.Instance.AddButtonPolish(collectButton);
            }

            rewardPanel.SetActive(false);
        }

        /// <summary>
        /// Creates a default coin-like icon when no sprite is available.
        /// </summary>
        private void CreateDefaultRewardIcon(Transform parent)
        {
            // Outer ring
            GameObject ringObj = new GameObject("Ring");
            ringObj.transform.SetParent(parent, false);
            RectTransform ringRect = ringObj.AddComponent<RectTransform>();
            ringRect.anchorMin = Vector2.zero;
            ringRect.anchorMax = Vector2.one;
            ringRect.offsetMin = Vector2.zero;
            ringRect.offsetMax = Vector2.zero;

            Image ringImg = ringObj.AddComponent<Image>();
            ringImg.color = new Color(0.9f, 0.7f, 0.1f);  // Gold ring

            // Inner circle
            GameObject innerObj = new GameObject("Inner");
            innerObj.transform.SetParent(ringObj.transform, false);
            RectTransform innerRect = innerObj.AddComponent<RectTransform>();
            innerRect.anchorMin = new Vector2(0.1f, 0.1f);
            innerRect.anchorMax = new Vector2(0.9f, 0.9f);
            innerRect.offsetMin = Vector2.zero;
            innerRect.offsetMax = Vector2.zero;

            Image innerImg = innerObj.AddComponent<Image>();
            innerImg.color = new Color(1f, 0.85f, 0.2f);  // Lighter gold

            // Dollar sign or star
            GameObject symbolObj = new GameObject("Symbol");
            symbolObj.transform.SetParent(innerObj.transform, false);
            RectTransform symbolRect = symbolObj.AddComponent<RectTransform>();
            symbolRect.anchorMin = Vector2.zero;
            symbolRect.anchorMax = Vector2.one;
            symbolRect.offsetMin = Vector2.zero;
            symbolRect.offsetMax = Vector2.zero;

            TextMeshProUGUI symbolText = symbolObj.AddComponent<TextMeshProUGUI>();
            symbolText.text = "$";
            symbolText.fontSize = 80;
            symbolText.fontStyle = FontStyles.Bold;
            symbolText.alignment = TextAlignmentOptions.Center;
            symbolText.color = new Color(0.6f, 0.4f, 0.1f);
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

        /// <summary>
        /// Creates the "Already Rolled Today" panel with countdown to next roll.
        /// </summary>
        private void CreateAlreadyRolledPanel()
        {
            alreadyRolledPanel = new GameObject("AlreadyRolledPanel");
            alreadyRolledPanel.transform.SetParent(mainPanel.transform, false);

            RectTransform panelRect = alreadyRolledPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.08f, 0.25f);
            panelRect.anchorMax = new Vector2(0.92f, 0.75f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Background with GUI sprite
            Image panelBg = alreadyRolledPanel.AddComponent<Image>();
            if (guiAssets != null && guiAssets.popupBackground != null)
            {
                panelBg.sprite = guiAssets.popupBackground;
                panelBg.type = Image.Type.Sliced;
                panelBg.color = Color.white;
            }
            else
            {
                panelBg.color = new Color(0.12f, 0.1f, 0.18f, 0.98f);
            }

            // Title ribbon - "Come Back Tomorrow!"
            GameObject ribbonObj = new GameObject("Ribbon");
            ribbonObj.transform.SetParent(alreadyRolledPanel.transform, false);
            RectTransform ribbonRect = ribbonObj.AddComponent<RectTransform>();
            ribbonRect.anchorMin = new Vector2(0.5f, 0.85f);
            ribbonRect.anchorMax = new Vector2(0.5f, 0.85f);
            ribbonRect.sizeDelta = new Vector2(500, 90);
            ribbonRect.anchoredPosition = Vector2.zero;

            Image ribbonBg = ribbonObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.ribbonBlue != null)
            {
                ribbonBg.sprite = guiAssets.ribbonBlue;
                ribbonBg.type = Image.Type.Sliced;
                ribbonBg.color = Color.white;
            }
            else
            {
                ribbonBg.color = new Color(0.3f, 0.5f, 0.9f);
            }

            // Title text inside ribbon
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(ribbonObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(10, 5);
            titleRect.offsetMax = new Vector2(-10, -5);

            alreadyRolledText = titleObj.AddComponent<TextMeshProUGUI>();
            alreadyRolledText.text = "COME BACK TOMORROW!";
            alreadyRolledText.fontSize = UIDesignSystem.FontSizeSubtitle;
            alreadyRolledText.fontStyle = FontStyles.Bold;
            alreadyRolledText.alignment = TextAlignmentOptions.Center;
            alreadyRolledText.color = Color.white;
            alreadyRolledText.enableAutoSizing = true;
            alreadyRolledText.fontSizeMin = 28;
            alreadyRolledText.fontSizeMax = 40;

            // Sleeping dice/clock icon in center
            GameObject iconContainerObj = new GameObject("IconContainer");
            iconContainerObj.transform.SetParent(alreadyRolledPanel.transform, false);
            RectTransform iconContainerRect = iconContainerObj.AddComponent<RectTransform>();
            iconContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconContainerRect.sizeDelta = new Vector2(180, 180);
            iconContainerRect.anchoredPosition = new Vector2(0, 20);

            // Icon frame
            Image iconFrameImg = iconContainerObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.cardFrame != null)
            {
                iconFrameImg.sprite = guiAssets.cardFrame;
                iconFrameImg.type = Image.Type.Sliced;
                iconFrameImg.color = new Color(0.4f, 0.5f, 0.7f);
            }
            else
            {
                iconFrameImg.color = new Color(0.3f, 0.4f, 0.6f);
            }

            // Clock/moon icon inside
            GameObject clockObj = new GameObject("ClockIcon");
            clockObj.transform.SetParent(iconContainerObj.transform, false);
            RectTransform clockRect = clockObj.AddComponent<RectTransform>();
            clockRect.anchorMin = new Vector2(0.15f, 0.15f);
            clockRect.anchorMax = new Vector2(0.85f, 0.85f);
            clockRect.offsetMin = Vector2.zero;
            clockRect.offsetMax = Vector2.zero;

            TextMeshProUGUI clockText = clockObj.AddComponent<TextMeshProUGUI>();
            clockText.text = "ZZZ";
            clockText.fontSize = 60;
            clockText.fontStyle = FontStyles.Bold;
            clockText.alignment = TextAlignmentOptions.Center;
            clockText.color = new Color(0.7f, 0.8f, 1f);

            // Gentle floating animation
            iconContainerObj.transform.DOLocalMoveY(30, 1.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            // Message text
            GameObject messageObj = new GameObject("Message");
            messageObj.transform.SetParent(alreadyRolledPanel.transform, false);
            RectTransform messageRect = messageObj.AddComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0, 0.22f);
            messageRect.anchorMax = new Vector2(1, 0.35f);
            messageRect.offsetMin = new Vector2(UIDesignSystem.SpacingL, 0);
            messageRect.offsetMax = new Vector2(-UIDesignSystem.SpacingL, 0);

            TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
            messageText.text = "You've already claimed today's reward.\nKeep your streak going!";
            messageText.fontSize = UIDesignSystem.FontSizeBody;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.color = UIDesignSystem.TextSecondary;

            // Countdown timer
            GameObject countdownObj = new GameObject("Countdown");
            countdownObj.transform.SetParent(alreadyRolledPanel.transform, false);
            RectTransform countdownRect = countdownObj.AddComponent<RectTransform>();
            countdownRect.anchorMin = new Vector2(0, 0.08f);
            countdownRect.anchorMax = new Vector2(1, 0.20f);
            countdownRect.offsetMin = new Vector2(UIDesignSystem.SpacingL, 0);
            countdownRect.offsetMax = new Vector2(-UIDesignSystem.SpacingL, 0);

            countdownText = countdownObj.AddComponent<TextMeshProUGUI>();
            countdownText.text = "Next roll in: 00:00:00";
            countdownText.fontSize = UIDesignSystem.FontSizeLarge;
            countdownText.fontStyle = FontStyles.Bold;
            countdownText.alignment = TextAlignmentOptions.Center;
            countdownText.color = Color.white;
            countdownText.richText = true;

            // Pulse the countdown
            countdownObj.transform.DOScale(1.05f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            // Stars/sparkle decoration
            CreateSparkleDecorations(alreadyRolledPanel.transform);

            // Start hidden
            alreadyRolledPanel.SetActive(false);
        }

        /// <summary>
        /// Creates decorative sparkle effects for the panel.
        /// </summary>
        private void CreateSparkleDecorations(Transform parent)
        {
            // Create a few sparkle stars around the panel
            Vector2[] positions = new Vector2[]
            {
                new Vector2(-0.35f, 0.4f),
                new Vector2(0.38f, 0.45f),
                new Vector2(-0.4f, -0.1f),
                new Vector2(0.42f, -0.05f),
            };

            foreach (var pos in positions)
            {
                GameObject sparkleObj = new GameObject("Sparkle");
                sparkleObj.transform.SetParent(parent, false);
                RectTransform sparkleRect = sparkleObj.AddComponent<RectTransform>();
                sparkleRect.anchorMin = new Vector2(0.5f + pos.x, 0.5f + pos.y);
                sparkleRect.anchorMax = new Vector2(0.5f + pos.x, 0.5f + pos.y);
                sparkleRect.sizeDelta = new Vector2(30, 30);
                sparkleRect.anchoredPosition = Vector2.zero;

                TextMeshProUGUI sparkleText = sparkleObj.AddComponent<TextMeshProUGUI>();
                sparkleText.text = "*";
                sparkleText.fontSize = 40;
                sparkleText.fontStyle = FontStyles.Bold;
                sparkleText.alignment = TextAlignmentOptions.Center;
                sparkleText.color = new Color(1f, 0.95f, 0.6f, 0.8f);

                // Random twinkle animation
                float delay = Random.Range(0f, 1f);
                float duration = Random.Range(0.8f, 1.4f);
                sparkleObj.transform.DOScale(0.6f, duration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetDelay(delay);

                CanvasGroup sparkleCg = sparkleObj.AddComponent<CanvasGroup>();
                sparkleCg.DOFade(0.4f, duration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetDelay(delay);
            }
        }

        /// <summary>
        /// Sets up MMFeedbacks for juicy effects.
        /// </summary>
        private void SetupFeedbacks()
        {
            // Create a feedback holder object
            GameObject feedbackHolder = new GameObject("Feedbacks");
            feedbackHolder.transform.SetParent(mainPanel.transform, false);

            // Create separate child objects for each feedback player
            // (MMF_Player works best as single component per GameObject)
            GameObject rollFeedbackObj = new GameObject("RollFeedback");
            rollFeedbackObj.transform.SetParent(feedbackHolder.transform, false);
            rollFeedback = rollFeedbackObj.AddComponent<MMF_Player>();
            if (rollFeedback != null)
            {
                rollFeedback.InitializationMode = MMF_Player.InitializationModes.Awake;
            }

            GameObject collectFeedbackObj = new GameObject("CollectFeedback");
            collectFeedbackObj.transform.SetParent(feedbackHolder.transform, false);
            collectFeedback = collectFeedbackObj.AddComponent<MMF_Player>();
            if (collectFeedback != null)
            {
                collectFeedback.InitializationMode = MMF_Player.InitializationModes.Script;

                // Add scale punch feedback for collect
                MMF_Scale collectScale = new MMF_Scale();
                collectScale.Label = "Collect Scale Punch";
                collectScale.AnimateScaleTarget = collectButton != null ? collectButton.transform : null;
                collectScale.RemapCurveOne = 1.2f;
                collectScale.AnimateScaleDuration = 0.15f;
                collectFeedback.AddFeedback(collectScale);

                collectFeedback.Initialization();
            }

            GameObject jackpotFeedbackObj = new GameObject("JackpotFeedback");
            jackpotFeedbackObj.transform.SetParent(feedbackHolder.transform, false);
            jackpotFeedback = jackpotFeedbackObj.AddComponent<MMF_Player>();
            if (jackpotFeedback != null)
            {
                jackpotFeedback.InitializationMode = MMF_Player.InitializationModes.Script;

                // Add scale punch feedback for jackpot (bigger effect)
                if (rewardPanel != null)
                {
                    MMF_Scale jackpotScale = new MMF_Scale();
                    jackpotScale.Label = "Jackpot Scale Punch";
                    jackpotScale.AnimateScaleTarget = rewardPanel.transform;
                    jackpotScale.RemapCurveOne = 1.3f;
                    jackpotScale.AnimateScaleDuration = 0.2f;
                    jackpotFeedback.AddFeedback(jackpotScale);
                }

                jackpotFeedback.Initialization();
            }
        }

        /// <summary>
        /// Plays the roll feedback effect.
        /// </summary>
        private void PlayRollFeedback()
        {
            if (rollFeedback != null)
            {
                rollFeedback.PlayFeedbacks();
            }

            // Also trigger haptic feedback on mobile
            #if UNITY_IOS || UNITY_ANDROID
            if (MoreMountains.NiceVibrations.MMVibrationManager.HapticsSupported())
            {
                MoreMountains.NiceVibrations.MMVibrationManager.Haptic(MoreMountains.NiceVibrations.HapticTypes.MediumImpact);
            }
            #endif
        }

        /// <summary>
        /// Plays the collect feedback effect.
        /// </summary>
        private void PlayCollectFeedback()
        {
            if (collectFeedback != null)
            {
                collectFeedback.PlayFeedbacks();
            }

            // Also trigger haptic feedback on mobile
            #if UNITY_IOS || UNITY_ANDROID
            if (MoreMountains.NiceVibrations.MMVibrationManager.HapticsSupported())
            {
                MoreMountains.NiceVibrations.MMVibrationManager.Haptic(MoreMountains.NiceVibrations.HapticTypes.Success);
            }
            #endif
        }

        /// <summary>
        /// Plays the jackpot feedback effect.
        /// </summary>
        private void PlayJackpotFeedback()
        {
            if (jackpotFeedback != null)
            {
                jackpotFeedback.PlayFeedbacks();
            }

            // Strong haptic for jackpot
            #if UNITY_IOS || UNITY_ANDROID
            if (MoreMountains.NiceVibrations.MMVibrationManager.HapticsSupported())
            {
                MoreMountains.NiceVibrations.MMVibrationManager.Haptic(MoreMountains.NiceVibrations.HapticTypes.HeavyImpact);
            }
            #endif
        }
    }
}
