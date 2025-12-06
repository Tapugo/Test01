using System;
using System.Collections;
using UnityEngine;
using Incredicer.Core;
using Incredicer.Dice;

namespace Incredicer.DailyLogin
{
    /// <summary>
    /// Represents the reward generated for today's daily login.
    /// </summary>
    [Serializable]
    public class DailyReward
    {
        public DailyRewardType type;
        public double amount;
        public float boostDuration;
        public string title;
        public string description;
        public int streakDay;
        public float streakMultiplier;
    }

    /// <summary>
    /// Manages the daily login system including streak tracking and reward generation.
    /// </summary>
    public class DailyLoginManager : MonoBehaviour
    {
        public static DailyLoginManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private DailyRewardConfig rewardConfig;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        // State - saved/loaded via SaveSystem
        private DateTime lastLoginDate;
        private int currentStreakDay = 1;
        private bool hasRolledToday = false;
        private int totalLoginDays = 0;
        private double yesterdayDMEarned = 0;

        // Events
        public event Action OnDailyRewardAvailable;
        public event Action<DailyReward> OnDailyRewardClaimed;
        public event Action<int> OnStreakUpdated;

        // Properties
        public int CurrentStreakDay => currentStreakDay;
        public bool HasRolledToday => hasRolledToday;
        public int TotalLoginDays => totalLoginDays;
        public bool CanRollToday => !hasRolledToday;
        public int StreakLength => rewardConfig != null ? rewardConfig.streakLength : 7;
        public DateTime LastLoginDate => lastLoginDate;
        public double YesterdayDMEarned => yesterdayDMEarned;

        /// <summary>
        /// Returns true if the player can claim their daily reward (hasn't rolled today).
        /// </summary>
        public bool CanClaimReward() => CanRollToday;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load default config if not assigned
            if (rewardConfig == null)
            {
                rewardConfig = Resources.Load<DailyRewardConfig>("DailyRewardConfig");
            }
        }

        private void Start()
        {
            // Initialize will be called after SaveSystem loads data
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.OnLoadCompleted += InitializeOnGameStart;
            }
            else
            {
                // Fallback if no save system
                InitializeOnGameStart();
            }
        }

        private void OnDestroy()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.OnLoadCompleted -= InitializeOnGameStart;
            }
        }

        /// <summary>
        /// Called when the game starts or save data is loaded.
        /// Checks if a new day has started and updates state accordingly.
        /// </summary>
        public void InitializeOnGameStart()
        {
            DateTime today = DateTime.Today;
            DateTime lastLogin = lastLoginDate.Date;

            if (debugMode)
            {
                Debug.Log($"[DailyLogin] Initializing - Today: {today}, Last Login: {lastLogin}, Streak: {currentStreakDay}");
            }

            // Check if this is a new calendar day
            if (today > lastLogin)
            {
                int daysSinceLastLogin = (today - lastLogin).Days;

                if (debugMode)
                {
                    Debug.Log($"[DailyLogin] Days since last login: {daysSinceLastLogin}");
                }

                // Store yesterday's DM for potential bonus calculation
                if (CurrencyManager.Instance != null && daysSinceLastLogin == 1)
                {
                    yesterdayDMEarned = CurrencyManager.Instance.LifetimeDarkMatter;
                }

                // Update streak based on days missed
                if (daysSinceLastLogin == 1)
                {
                    // Consecutive day - increment streak
                    currentStreakDay = Mathf.Min(currentStreakDay + 1, StreakLength);
                }
                else if (rewardConfig != null && rewardConfig.resetStreakOnMiss && daysSinceLastLogin > rewardConfig.gracePeriodDays)
                {
                    // Too many days missed - reset streak
                    currentStreakDay = 1;
                }
                // Otherwise keep the same streak day (no punishment for missing days)

                // Increment total login days
                totalLoginDays++;

                // Reset rolled flag for new day
                hasRolledToday = false;

                // Update last login date
                lastLoginDate = today;

                if (debugMode)
                {
                    Debug.Log($"[DailyLogin] New day! Streak: {currentStreakDay}, Total logins: {totalLoginDays}");
                }

                // Notify listeners that a reward is available
                OnDailyRewardAvailable?.Invoke();
                OnStreakUpdated?.Invoke(currentStreakDay);
            }
            else if (lastLogin == DateTime.MinValue.Date)
            {
                // First time playing
                totalLoginDays = 1;
                currentStreakDay = 1;
                hasRolledToday = false;
                lastLoginDate = today;

                if (debugMode)
                {
                    Debug.Log("[DailyLogin] First time player!");
                }

                OnDailyRewardAvailable?.Invoke();
                OnStreakUpdated?.Invoke(currentStreakDay);
            }
        }

        /// <summary>
        /// Generates the reward for the current streak day.
        /// </summary>
        public DailyReward GenerateReward()
        {
            if (rewardConfig == null)
            {
                Debug.LogError("[DailyLogin] No reward config assigned!");
                return null;
            }

            DailyRewardDay dayConfig = rewardConfig.GetRewardForDay(currentStreakDay);
            if (dayConfig == null)
            {
                Debug.LogError($"[DailyLogin] No reward config for day {currentStreakDay}!");
                return null;
            }

            double rewardAmount = dayConfig.baseAmount;

            // Scale money rewards based on player's income
            if (dayConfig.rewardType == DailyRewardType.Money && GameStats.Instance != null)
            {
                // Use base payout scaled by upgrade level as a rough income estimate
                double estimatedIncomePerMinute = GameStats.Instance.BaseMoneyPerRoll * 10; // Rough estimate
                rewardAmount = Mathf.Max((float)rewardAmount, (float)(estimatedIncomePerMinute * rewardConfig.moneyRewardMinutesWorth));
            }

            // Scale DM rewards
            if (dayConfig.rewardType == DailyRewardType.DarkMatter)
            {
                rewardAmount = rewardConfig.baseDarkMatterReward + (yesterdayDMEarned * rewardConfig.dmYesterdayBonusPercent);
            }

            // Apply streak multiplier
            rewardAmount *= dayConfig.streakMultiplier;

            DailyReward reward = new DailyReward
            {
                type = dayConfig.rewardType,
                amount = rewardAmount,
                boostDuration = dayConfig.boostDurationSeconds,
                title = dayConfig.rewardTitle,
                description = dayConfig.rewardDescription,
                streakDay = currentStreakDay,
                streakMultiplier = dayConfig.streakMultiplier
            };

            if (debugMode)
            {
                Debug.Log($"[DailyLogin] Generated reward: {reward.type} x{reward.amount} (Streak Day {reward.streakDay})");
            }

            return reward;
        }

        /// <summary>
        /// Applies the reward to the player and marks today as rolled.
        /// </summary>
        public void ClaimReward(DailyReward reward)
        {
            if (reward == null)
            {
                Debug.LogError("[DailyLogin] Cannot claim null reward!");
                return;
            }

            if (hasRolledToday)
            {
                Debug.LogWarning("[DailyLogin] Already claimed today's reward!");
                return;
            }

            // Apply the reward based on type
            switch (reward.type)
            {
                case DailyRewardType.Money:
                    if (CurrencyManager.Instance != null)
                    {
                        CurrencyManager.Instance.AddMoney(reward.amount, false);
                    }
                    break;

                case DailyRewardType.DarkMatter:
                    if (CurrencyManager.Instance != null)
                    {
                        CurrencyManager.Instance.AddDarkMatter(reward.amount);
                    }
                    break;

                case DailyRewardType.MoneyBoost:
                    if (GameStats.Instance != null)
                    {
                        GameStats.Instance.ApplyTemporaryMoneyBoost((float)(reward.amount / 100f), reward.boostDuration);
                    }
                    break;

                case DailyRewardType.DMBoost:
                    if (GameStats.Instance != null)
                    {
                        GameStats.Instance.ApplyTemporaryDMBoost((float)(reward.amount / 100f), reward.boostDuration);
                    }
                    break;

                case DailyRewardType.JackpotToken:
                    // TODO: Implement jackpot token system
                    Debug.Log($"[DailyLogin] Jackpot token claimed! (Not yet implemented)");
                    break;

                case DailyRewardType.ExtraDice:
                    // Spawn extra dice on the board
                    if (DiceManager.Instance != null)
                    {
                        int diceCount = (int)reward.amount;
                        DiceData basicDice = DiceManager.Instance.GetBasicDiceData();
                        if (basicDice != null)
                        {
                            for (int i = 0; i < diceCount; i++)
                            {
                                // Spawn with slight delay for visual effect
                                float delay = i * 0.15f;
                                DiceManager.Instance.StartCoroutine(SpawnDiceDelayed(basicDice, delay));
                            }
                            Debug.Log($"[DailyLogin] Spawning {diceCount} extra dice!");
                        }
                    }
                    break;
            }

            // Mark as rolled today
            hasRolledToday = true;

            if (debugMode)
            {
                Debug.Log($"[DailyLogin] Reward claimed: {reward.type} x{reward.amount}");
            }

            // Notify listeners
            OnDailyRewardClaimed?.Invoke(reward);

            // Save game state
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SaveGame();
            }
        }

        /// <summary>
        /// Sets state from save data.
        /// </summary>
        public void SetSaveData(DateTime savedLastLogin, int savedStreakDay, bool savedHasRolled, int savedTotalLogins, double savedYesterdayDM)
        {
            lastLoginDate = savedLastLogin;
            currentStreakDay = savedStreakDay;
            hasRolledToday = savedHasRolled;
            totalLoginDays = savedTotalLogins;
            yesterdayDMEarned = savedYesterdayDM;

            if (debugMode)
            {
                Debug.Log($"[DailyLogin] Loaded save data - Last: {lastLoginDate}, Streak: {currentStreakDay}, Rolled: {hasRolledToday}");
            }
        }

        /// <summary>
        /// Gets save data for persistence.
        /// </summary>
        public void GetSaveData(out DateTime outLastLogin, out int outStreakDay, out bool outHasRolled, out int outTotalLogins, out double outYesterdayDM)
        {
            outLastLogin = lastLoginDate;
            outStreakDay = currentStreakDay;
            outHasRolled = hasRolledToday;
            outTotalLogins = totalLoginDays;
            outYesterdayDM = yesterdayDMEarned;
        }

        /// <summary>
        /// Debug method to simulate a new day.
        /// </summary>
        [ContextMenu("Debug: Simulate New Day")]
        public void DebugSimulateNewDay()
        {
            lastLoginDate = DateTime.Today.AddDays(-1);
            hasRolledToday = false;
            InitializeOnGameStart();
        }

        /// <summary>
        /// Debug method to reset streak.
        /// </summary>
        [ContextMenu("Debug: Reset Streak")]
        public void DebugResetStreak()
        {
            currentStreakDay = 1;
            hasRolledToday = false;
            OnStreakUpdated?.Invoke(currentStreakDay);
        }

        /// <summary>
        /// Coroutine to spawn a dice with a delay for visual effect.
        /// </summary>
        private IEnumerator SpawnDiceDelayed(DiceData diceData, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (DiceManager.Instance != null)
            {
                // Get a random spawn position within the play area
                Vector2 spawnPos = GetRandomSpawnPosition();
                DiceManager.Instance.SpawnDice(diceData, spawnPos);

                // Play spawn sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayPurchaseSound();
                }
            }
        }

        /// <summary>
        /// Gets a random spawn position within the play area.
        /// </summary>
        private Vector2 GetRandomSpawnPosition()
        {
            // Get spawn area from camera bounds
            Camera cam = Camera.main;
            if (cam != null)
            {
                float height = cam.orthographicSize * 0.6f;
                float width = height * cam.aspect * 0.6f;

                return new Vector2(
                    UnityEngine.Random.Range(-width, width),
                    UnityEngine.Random.Range(-height, height)
                );
            }

            // Fallback
            return new Vector2(
                UnityEngine.Random.Range(-3f, 3f),
                UnityEngine.Random.Range(-2f, 2f)
            );
        }
    }
}
