using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Incredicer.Core;
using Incredicer.Dice;
using Incredicer.Skills;
using Incredicer.DailyLogin;

namespace Incredicer.Missions
{
    /// <summary>
    /// Manages daily and weekly missions, including progress tracking and rewards.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private int dailyMissionCount = 3;
        [SerializeField] private int weeklyMissionCount = 5;
        [SerializeField] private List<MissionDefinition> dailyMissionPool;
        [SerializeField] private List<MissionDefinition> weeklyMissionPool;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        // Active missions
        private List<MissionInstance> activeDailyMissions = new List<MissionInstance>();
        private List<MissionInstance> activeWeeklyMissions = new List<MissionInstance>();

        // Reset times
        private DateTime lastDailyReset;
        private DateTime lastWeeklyReset;

        // Events
        public event Action<MissionInstance> OnMissionProgress;
        public event Action<MissionInstance> OnMissionCompleted;
        public event Action<MissionInstance> OnMissionClaimed;
        public event Action OnMissionsRefreshed;

        // Properties
        public IReadOnlyList<MissionInstance> DailyMissions => activeDailyMissions;
        public IReadOnlyList<MissionInstance> WeeklyMissions => activeWeeklyMissions;
        public int ClaimableCount => activeDailyMissions.Count(m => m.CanClaim) + activeWeeklyMissions.Count(m => m.CanClaim);

        /// <summary>
        /// Returns true if any mission is complete but not yet claimed.
        /// </summary>
        public bool HasClaimableMissions() => ClaimableCount > 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load mission pools from Resources if not assigned
            if (dailyMissionPool == null || dailyMissionPool.Count == 0)
            {
                dailyMissionPool = Resources.LoadAll<MissionDefinition>("Missions/Daily").ToList();
            }
            if (weeklyMissionPool == null || weeklyMissionPool.Count == 0)
            {
                weeklyMissionPool = Resources.LoadAll<MissionDefinition>("Missions/Weekly").ToList();
            }
        }

        private void Start()
        {
            // Subscribe to save system
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.OnLoadCompleted += OnLoadCompleted;
            }

            // Subscribe to game events for progress tracking
            SubscribeToGameEvents();

            // Check for mission reset after a short delay
            Invoke(nameof(CheckMissionResets), 0.5f);
        }

        private void OnDestroy()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.OnLoadCompleted -= OnLoadCompleted;
            }
            UnsubscribeFromGameEvents();
        }

        private void OnLoadCompleted()
        {
            CheckMissionResets();
        }

        /// <summary>
        /// Checks if daily or weekly missions need to be reset.
        /// </summary>
        public void CheckMissionResets()
        {
            DateTime now = DateTime.Now;
            bool refreshed = false;

            // Check daily reset (new calendar day)
            if (now.Date > lastDailyReset.Date)
            {
                if (debugMode) Debug.Log("[MissionManager] Daily reset triggered");
                RefreshDailyMissions();
                lastDailyReset = now.Date;
                refreshed = true;
            }

            // Check weekly reset (Monday)
            DateTime thisWeekMonday = now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
            if (now.DayOfWeek == DayOfWeek.Sunday) thisWeekMonday = thisWeekMonday.AddDays(-7);

            DateTime lastWeekMonday = lastWeeklyReset.Date.AddDays(-(int)lastWeeklyReset.DayOfWeek + (int)DayOfWeek.Monday);
            if (lastWeeklyReset.DayOfWeek == DayOfWeek.Sunday) lastWeekMonday = lastWeekMonday.AddDays(-7);

            if (thisWeekMonday > lastWeekMonday || lastWeeklyReset == DateTime.MinValue)
            {
                if (debugMode) Debug.Log("[MissionManager] Weekly reset triggered");
                RefreshWeeklyMissions();
                lastWeeklyReset = now.Date;
                refreshed = true;
            }

            // Generate initial missions if none exist
            if (activeDailyMissions.Count == 0)
            {
                RefreshDailyMissions();
                lastDailyReset = now.Date;
                refreshed = true;
            }
            if (activeWeeklyMissions.Count == 0)
            {
                RefreshWeeklyMissions();
                lastWeeklyReset = now.Date;
                refreshed = true;
            }

            if (refreshed)
            {
                OnMissionsRefreshed?.Invoke();
                if (SaveSystem.Instance != null)
                {
                    SaveSystem.Instance.SaveGame();
                }
            }
        }

        /// <summary>
        /// Refreshes daily missions with new random selection.
        /// </summary>
        private void RefreshDailyMissions()
        {
            activeDailyMissions.Clear();

            if (dailyMissionPool == null || dailyMissionPool.Count == 0)
            {
                // Create default missions if no pool
                CreateDefaultDailyMissions();
                return;
            }

            // Select random missions from pool
            var shuffled = dailyMissionPool.OrderBy(x => UnityEngine.Random.value).ToList();
            for (int i = 0; i < Mathf.Min(dailyMissionCount, shuffled.Count); i++)
            {
                activeDailyMissions.Add(new MissionInstance(shuffled[i]));
            }

            if (debugMode) Debug.Log($"[MissionManager] Refreshed {activeDailyMissions.Count} daily missions");
        }

        /// <summary>
        /// Refreshes weekly missions with new random selection.
        /// </summary>
        private void RefreshWeeklyMissions()
        {
            activeWeeklyMissions.Clear();

            if (weeklyMissionPool == null || weeklyMissionPool.Count == 0)
            {
                // Create default missions if no pool
                CreateDefaultWeeklyMissions();
                return;
            }

            // Select random missions from pool
            var shuffled = weeklyMissionPool.OrderBy(x => UnityEngine.Random.value).ToList();
            for (int i = 0; i < Mathf.Min(weeklyMissionCount, shuffled.Count); i++)
            {
                activeWeeklyMissions.Add(new MissionInstance(shuffled[i]));
            }

            if (debugMode) Debug.Log($"[MissionManager] Refreshed {activeWeeklyMissions.Count} weekly missions");
        }

        /// <summary>
        /// Creates default daily missions when no pool is configured.
        /// </summary>
        private void CreateDefaultDailyMissions()
        {
            activeDailyMissions.Add(new MissionInstance
            {
                missionId = "daily_rolls",
                displayName = "Roll 100 Dice",
                description = "Roll any dice 100 times.",
                isDaily = true,
                missionType = MissionType.RollDice,
                targetAmount = 100,
                rewards = new[] { new MissionReward { type = MissionRewardType.Money, amount = 500 } }
            });

            activeDailyMissions.Add(new MissionInstance
            {
                missionId = "daily_money",
                displayName = "Earn $1,000",
                description = "Earn money from dice rolls.",
                isDaily = true,
                missionType = MissionType.EarnMoney,
                targetAmount = 1000,
                rewards = new[] { new MissionReward { type = MissionRewardType.DarkMatter, amount = 5 } }
            });

            activeDailyMissions.Add(new MissionInstance
            {
                missionId = "daily_dm",
                displayName = "Earn 10 Dark Matter",
                description = "Collect dark matter from special dice.",
                isDaily = true,
                missionType = MissionType.EarnDarkMatter,
                targetAmount = 10,
                rewards = new[] { new MissionReward { type = MissionRewardType.Money, amount = 1000 } }
            });
        }

        /// <summary>
        /// Creates default weekly missions when no pool is configured.
        /// </summary>
        private void CreateDefaultWeeklyMissions()
        {
            activeWeeklyMissions.Add(new MissionInstance
            {
                missionId = "weekly_rolls",
                displayName = "Roll 1,000 Dice",
                description = "Roll dice throughout the week.",
                isDaily = false,
                missionType = MissionType.RollDice,
                targetAmount = 1000,
                rewards = new[] { new MissionReward { type = MissionRewardType.DarkMatter, amount = 50 } }
            });

            activeWeeklyMissions.Add(new MissionInstance
            {
                missionId = "weekly_money",
                displayName = "Earn $100,000",
                description = "Accumulate wealth this week.",
                isDaily = false,
                missionType = MissionType.EarnMoney,
                targetAmount = 100000,
                rewards = new[] { new MissionReward { type = MissionRewardType.DarkMatter, amount = 25 } }
            });

            activeWeeklyMissions.Add(new MissionInstance
            {
                missionId = "weekly_buy",
                displayName = "Buy 5 Dice",
                description = "Expand your dice collection.",
                isDaily = false,
                missionType = MissionType.BuyDice,
                targetAmount = 5,
                rewards = new[] { new MissionReward { type = MissionRewardType.Money, amount = 5000 } }
            });

            activeWeeklyMissions.Add(new MissionInstance
            {
                missionId = "weekly_skills",
                displayName = "Unlock 3 Skills",
                description = "Progress through the skill tree.",
                isDaily = false,
                missionType = MissionType.UnlockSkillNodes,
                targetAmount = 3,
                rewards = new[] { new MissionReward { type = MissionRewardType.DarkMatter, amount = 30 } }
            });

            activeWeeklyMissions.Add(new MissionInstance
            {
                missionId = "weekly_dm",
                displayName = "Earn 100 Dark Matter",
                description = "Harvest the power of dark matter.",
                isDaily = false,
                missionType = MissionType.EarnDarkMatter,
                targetAmount = 100,
                rewards = new[] {
                    new MissionReward { type = MissionRewardType.Money, amount = 10000 },
                    new MissionReward { type = MissionRewardType.DarkMatter, amount = 20 }
                }
            });
        }

        #region Progress Tracking

        /// <summary>
        /// Updates progress for all missions of a given type.
        /// </summary>
        public void UpdateProgress(MissionType type, double amount)
        {
            UpdateMissionList(activeDailyMissions, type, amount);
            UpdateMissionList(activeWeeklyMissions, type, amount);
        }

        private void UpdateMissionList(List<MissionInstance> missions, MissionType type, double amount)
        {
            foreach (var mission in missions)
            {
                if (mission.missionType == type && !mission.isClaimed)
                {
                    bool wasCompleted = mission.isCompleted;
                    mission.AddProgress(amount);

                    OnMissionProgress?.Invoke(mission);

                    if (!wasCompleted && mission.isCompleted)
                    {
                        if (debugMode) Debug.Log($"[MissionManager] Mission completed: {mission.displayName}");
                        OnMissionCompleted?.Invoke(mission);
                    }
                }
            }
        }

        /// <summary>
        /// Sets absolute progress for missions (used for cumulative stats like lifetime money).
        /// </summary>
        public void SetProgress(MissionType type, double absoluteValue)
        {
            SetMissionProgress(activeDailyMissions, type, absoluteValue);
            SetMissionProgress(activeWeeklyMissions, type, absoluteValue);
        }

        private void SetMissionProgress(List<MissionInstance> missions, MissionType type, double absoluteValue)
        {
            foreach (var mission in missions)
            {
                if (mission.missionType == type && !mission.isClaimed)
                {
                    bool wasCompleted = mission.isCompleted;
                    mission.SetProgress(absoluteValue);

                    if (!wasCompleted && mission.isCompleted)
                    {
                        if (debugMode) Debug.Log($"[MissionManager] Mission completed: {mission.displayName}");
                        OnMissionCompleted?.Invoke(mission);
                    }
                }
            }
        }

        #endregion

        #region Claiming

        /// <summary>
        /// Claims rewards for a completed mission.
        /// </summary>
        public bool ClaimMission(MissionInstance mission)
        {
            if (mission == null || !mission.CanClaim) return false;

            // Grant rewards
            foreach (var reward in mission.rewards)
            {
                ApplyReward(reward);
            }

            mission.isClaimed = true;

            if (debugMode) Debug.Log($"[MissionManager] Claimed mission: {mission.displayName}");
            OnMissionClaimed?.Invoke(mission);

            // Save
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SaveGame();
            }

            return true;
        }

        private void ApplyReward(MissionReward reward)
        {
            switch (reward.type)
            {
                case MissionRewardType.Money:
                    if (CurrencyManager.Instance != null)
                        CurrencyManager.Instance.AddMoney(reward.amount, false);
                    break;

                case MissionRewardType.DarkMatter:
                    if (CurrencyManager.Instance != null)
                        CurrencyManager.Instance.AddDarkMatter(reward.amount);
                    break;

                case MissionRewardType.TimeShards:
                    // TODO: Add when MetaProgressionManager is implemented
                    Debug.Log($"[MissionManager] Time Shards reward: {reward.amount} (not yet implemented)");
                    break;

                case MissionRewardType.MoneyBoost:
                    if (GameStats.Instance != null)
                        GameStats.Instance.ApplyTemporaryMoneyBoost((float)(reward.amount / 100f), reward.boostDuration);
                    break;

                case MissionRewardType.DMBoost:
                    if (GameStats.Instance != null)
                        GameStats.Instance.ApplyTemporaryDMBoost((float)(reward.amount / 100f), reward.boostDuration);
                    break;
            }
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeToGameEvents()
        {
            // Currency events
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnMoneyChanged += OnMoneyChanged;
                CurrencyManager.Instance.OnDarkMatterChanged += OnDarkMatterChanged;
            }

            // Dice events - subscribe to spawned dice for roll tracking
            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.OnDiceSpawned += OnDiceSpawned;

                // Subscribe to existing dice
                foreach (var dice in DiceManager.Instance.GetAllDice())
                {
                    dice.OnRolled += OnDiceRolled;
                }
            }

            // Skill tree events
            if (SkillTreeManager.Instance != null)
            {
                SkillTreeManager.Instance.OnSkillUnlocked += OnSkillUnlocked;
            }

            // Daily login events
            if (DailyLoginManager.Instance != null)
            {
                DailyLoginManager.Instance.OnDailyRewardClaimed += OnDailyLoginClaimed;
            }
        }

        private void UnsubscribeFromGameEvents()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnMoneyChanged -= OnMoneyChanged;
                CurrencyManager.Instance.OnDarkMatterChanged -= OnDarkMatterChanged;
            }

            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.OnDiceSpawned -= OnDiceSpawned;

                // Unsubscribe from all dice
                foreach (var dice in DiceManager.Instance.GetAllDice())
                {
                    dice.OnRolled -= OnDiceRolled;
                }
            }

            if (SkillTreeManager.Instance != null)
            {
                SkillTreeManager.Instance.OnSkillUnlocked -= OnSkillUnlocked;
            }

            if (DailyLoginManager.Instance != null)
            {
                DailyLoginManager.Instance.OnDailyRewardClaimed -= OnDailyLoginClaimed;
            }
        }

        // Event handlers
        private double lastMoney = 0;
        private double lastDM = 0;

        private void OnMoneyChanged(double newMoney)
        {
            double gained = newMoney - lastMoney;
            if (gained > 0)
            {
                UpdateProgress(MissionType.EarnMoney, gained);
            }
            else if (gained < 0)
            {
                UpdateProgress(MissionType.SpendMoney, -gained);
            }
            lastMoney = newMoney;
        }

        private void OnDarkMatterChanged(double newDM)
        {
            double gained = newDM - lastDM;
            if (gained > 0)
            {
                UpdateProgress(MissionType.EarnDarkMatter, gained);
            }
            else if (gained < 0)
            {
                UpdateProgress(MissionType.SpendDarkMatter, -gained);
            }
            lastDM = newDM;
        }

        private void OnDiceRolled(Dice.Dice dice, double moneyEarned, bool isJackpot)
        {
            UpdateProgress(MissionType.RollDice, 1);

            if (isJackpot)
            {
                UpdateProgress(MissionType.EarnJackpots, 1);
            }
        }

        private void OnDiceSpawned(Dice.Dice dice)
        {
            // Subscribe to the new dice's roll events
            dice.OnRolled += OnDiceRolled;

            // Track as a dice purchase
            UpdateProgress(MissionType.BuyDice, 1);
        }

        private void OnSkillUnlocked(SkillNodeId nodeId)
        {
            UpdateProgress(MissionType.UnlockSkillNodes, 1);
        }

        private void OnDailyLoginClaimed(DailyReward reward)
        {
            UpdateProgress(MissionType.DailyLogins, 1);
        }

        /// <summary>
        /// Called when a dice is destroyed by overclocking.
        /// </summary>
        public void OnDiceDestroyedByOverclock()
        {
            UpdateProgress(MissionType.DestroyDice, 1);
        }

        /// <summary>
        /// Called when a dice makes an overclocked roll.
        /// </summary>
        public void OnOverclockedRoll()
        {
            UpdateProgress(MissionType.OverclockRolls, 1);
        }

        /// <summary>
        /// Called when a time fracture is performed.
        /// </summary>
        public void OnTimeFracturePerformed()
        {
            UpdateProgress(MissionType.TimeFractures, 1);
        }

        /// <summary>
        /// Called when an active skill is used.
        /// </summary>
        public void OnActiveSkillUsed()
        {
            UpdateProgress(MissionType.UseActiveSkills, 1);
        }

        /// <summary>
        /// Called when a jackpot roll occurs.
        /// </summary>
        public void OnJackpotRoll()
        {
            UpdateProgress(MissionType.EarnJackpots, 1);
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// Gets save data for persistence.
        /// </summary>
        public MissionSaveData GetSaveData()
        {
            return new MissionSaveData
            {
                dailyMissions = activeDailyMissions,
                weeklyMissions = activeWeeklyMissions,
                lastDailyResetStr = lastDailyReset.ToString("o"),
                lastWeeklyResetStr = lastWeeklyReset.ToString("o")
            };
        }

        /// <summary>
        /// Sets state from save data.
        /// </summary>
        public void SetSaveData(MissionSaveData data)
        {
            if (data == null) return;

            if (data.dailyMissions != null)
                activeDailyMissions = data.dailyMissions;
            if (data.weeklyMissions != null)
                activeWeeklyMissions = data.weeklyMissions;

            if (!string.IsNullOrEmpty(data.lastDailyResetStr))
                DateTime.TryParse(data.lastDailyResetStr, out lastDailyReset);
            if (!string.IsNullOrEmpty(data.lastWeeklyResetStr))
                DateTime.TryParse(data.lastWeeklyResetStr, out lastWeeklyReset);

            // Initialize currency tracking
            if (CurrencyManager.Instance != null)
            {
                lastMoney = CurrencyManager.Instance.Money;
                lastDM = CurrencyManager.Instance.DarkMatter;
            }

            if (debugMode) Debug.Log($"[MissionManager] Loaded {activeDailyMissions.Count} daily, {activeWeeklyMissions.Count} weekly missions");
        }

        #endregion

        #region Debug

        [ContextMenu("Debug: Complete All Daily")]
        public void DebugCompleteAllDaily()
        {
            foreach (var mission in activeDailyMissions)
            {
                mission.currentProgress = mission.targetAmount;
                mission.isCompleted = true;
                OnMissionCompleted?.Invoke(mission);
            }
        }

        [ContextMenu("Debug: Reset Daily Missions")]
        public void DebugResetDaily()
        {
            lastDailyReset = DateTime.MinValue;
            CheckMissionResets();
        }

        [ContextMenu("Debug: Reset Weekly Missions")]
        public void DebugResetWeekly()
        {
            lastWeeklyReset = DateTime.MinValue;
            CheckMissionResets();
        }

        #endregion
    }

    /// <summary>
    /// Container for mission save data.
    /// </summary>
    [Serializable]
    public class MissionSaveData
    {
        public List<MissionInstance> dailyMissions;
        public List<MissionInstance> weeklyMissions;
        public string lastDailyResetStr;
        public string lastWeeklyResetStr;
    }
}
