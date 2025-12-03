using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Incredicer.Core;
using Incredicer.Dice;
using Incredicer.Skills;
using Incredicer.Missions;
using Incredicer.TimeFracture;
using Incredicer.Overclock;
using Incredicer.DailyLogin;

namespace Incredicer.Milestones
{
    /// <summary>
    /// Manages long-term milestone achievements.
    /// </summary>
    public class MilestoneManager : MonoBehaviour
    {
        public static MilestoneManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private string milestonesResourcePath = "Milestones";

        [Header("Stats Tracking")]
        [SerializeField] private double totalDiceRolls = 0;
        [SerializeField] private double totalJackpots = 0;
        [SerializeField] private int maxDiceOwned = 0;
        [SerializeField] private int totalSkillNodesUnlocked = 0;
        [SerializeField] private int totalDiceTypesUnlocked = 0;
        [SerializeField] private double totalPlayTimeMinutes = 0;
        [SerializeField] private int highestLoginStreak = 0;
        [SerializeField] private int totalMissionsCompleted = 0;

        [Header("Permanent Boosts")]
        [SerializeField] private float permanentMoneyBoost = 0f;
        [SerializeField] private float permanentDMBoost = 0f;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        // Milestone definitions and progress
        private List<MilestoneDefinition> allMilestones = new List<MilestoneDefinition>();
        private Dictionary<string, MilestoneProgress> milestoneProgress = new Dictionary<string, MilestoneProgress>();

        // Events
        public event Action<MilestoneDefinition> OnMilestoneCompleted;
        public event Action<MilestoneDefinition, MilestoneReward[]> OnMilestoneClaimed;
        public event Action<int> OnClaimableCountChanged;

        // Properties
        public float PermanentMoneyBoost => permanentMoneyBoost;
        public float PermanentDMBoost => permanentDMBoost;
        public double TotalDiceRolls => totalDiceRolls;
        public double TotalJackpots => totalJackpots;

        /// <summary>
        /// Returns true if any milestone is complete but not yet claimed.
        /// </summary>
        public bool HasClaimableMilestones()
        {
            foreach (var kvp in milestoneProgress)
            {
                if (kvp.Value.isCompleted && !kvp.Value.isClaimed)
                    return true;
            }
            return false;
        }

        private float sessionStartTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            sessionStartTime = Time.time;
            LoadMilestoneDefinitions();
        }

        private void Start()
        {
            InitializeMilestoneProgress();
            SubscribeToEvents();
            ApplyPermanentBoosts();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            // Track play time
            totalPlayTimeMinutes += Time.deltaTime / 60f;
        }

        #region Initialization

        private void LoadMilestoneDefinitions()
        {
            var loaded = Resources.LoadAll<MilestoneDefinition>(milestonesResourcePath);
            allMilestones = loaded.OrderBy(m => m.tier).ThenBy(m => m.targetAmount).ToList();

            if (debugMode) Debug.Log($"[MilestoneManager] Loaded {allMilestones.Count} milestone definitions");
        }

        private void InitializeMilestoneProgress()
        {
            foreach (var milestone in allMilestones)
            {
                if (!milestoneProgress.ContainsKey(milestone.milestoneId))
                {
                    milestoneProgress[milestone.milestoneId] = new MilestoneProgress(milestone.milestoneId);
                }
            }

            // Initial check
            CheckAllMilestones();
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeToEvents()
        {
            // Currency events
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnLifetimeMoneyChanged += OnLifetimeMoneyChanged;
                CurrencyManager.Instance.OnLifetimeDarkMatterChanged += OnLifetimeDMChanged;
            }

            // Dice events
            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.OnDiceSpawned += OnDiceSpawned;
                DiceManager.Instance.OnDiceTypeUnlocked += OnDiceTypeUnlocked;

                // Subscribe to existing dice for roll tracking
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

            // Time Fracture events
            if (TimeFractureManager.Instance != null)
            {
                TimeFractureManager.Instance.OnTimeFractureCompleted += OnTimeFractureCompleted;
            }

            // Mission events
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionClaimed += OnMissionClaimed;
            }

            // Overclock events
            if (OverclockManager.Instance != null)
            {
                OverclockManager.Instance.OnDiceDestroyed += OnDiceDestroyed;
            }

            // Daily login events
            if (DailyLoginManager.Instance != null)
            {
                DailyLoginManager.Instance.OnStreakUpdated += OnStreakUpdated;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnLifetimeMoneyChanged -= OnLifetimeMoneyChanged;
                CurrencyManager.Instance.OnLifetimeDarkMatterChanged -= OnLifetimeDMChanged;
            }

            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.OnDiceSpawned -= OnDiceSpawned;
                DiceManager.Instance.OnDiceTypeUnlocked -= OnDiceTypeUnlocked;
            }

            if (SkillTreeManager.Instance != null)
            {
                SkillTreeManager.Instance.OnSkillUnlocked -= OnSkillUnlocked;
            }

            if (TimeFractureManager.Instance != null)
            {
                TimeFractureManager.Instance.OnTimeFractureCompleted -= OnTimeFractureCompleted;
            }

            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionClaimed -= OnMissionClaimed;
            }

            if (OverclockManager.Instance != null)
            {
                OverclockManager.Instance.OnDiceDestroyed -= OnDiceDestroyed;
            }

            if (DailyLoginManager.Instance != null)
            {
                DailyLoginManager.Instance.OnStreakUpdated -= OnStreakUpdated;
            }
        }

        #endregion

        #region Event Handlers

        private void OnLifetimeMoneyChanged(double amount)
        {
            CheckMilestonesOfType(MilestoneType.LifetimeMoney);
        }

        private void OnLifetimeDMChanged(double amount)
        {
            CheckMilestonesOfType(MilestoneType.LifetimeDarkMatter);
        }

        private void OnDiceRolled(Dice.Dice dice, double money, bool isJackpot)
        {
            totalDiceRolls++;
            CheckMilestonesOfType(MilestoneType.TotalDiceRolls);

            if (isJackpot)
            {
                totalJackpots++;
                CheckMilestonesOfType(MilestoneType.TotalJackpots);
            }
        }

        private void OnDiceSpawned(Dice.Dice dice)
        {
            // Subscribe to new dice
            dice.OnRolled += OnDiceRolled;

            // Track max dice owned
            if (DiceManager.Instance != null)
            {
                int currentCount = DiceManager.Instance.GetAllDice().Count;
                if (currentCount > maxDiceOwned)
                {
                    maxDiceOwned = currentCount;
                    CheckMilestonesOfType(MilestoneType.TotalDiceOwned);
                }
            }
        }

        private void OnDiceTypeUnlocked(DiceType type)
        {
            totalDiceTypesUnlocked++;
            CheckMilestonesOfType(MilestoneType.DiceTypesUnlocked);
        }

        private void OnSkillUnlocked(SkillNodeId nodeId)
        {
            totalSkillNodesUnlocked++;
            CheckMilestonesOfType(MilestoneType.SkillNodesUnlocked);
        }

        private void OnTimeFractureCompleted(int level)
        {
            CheckMilestonesOfType(MilestoneType.TimeFractureLevel);
            CheckMilestonesOfType(MilestoneType.TotalTimeFractures);
            CheckMilestonesOfType(MilestoneType.LifetimeTimeShards);
        }

        private void OnMissionClaimed(MissionInstance mission)
        {
            totalMissionsCompleted++;
            CheckMilestonesOfType(MilestoneType.MissionsCompleted);
        }

        private void OnDiceDestroyed(Dice.Dice dice, double dm)
        {
            CheckMilestonesOfType(MilestoneType.TotalDiceDestroyed);
        }

        private void OnStreakUpdated(int streak)
        {
            if (streak > highestLoginStreak)
            {
                highestLoginStreak = streak;
                CheckMilestonesOfType(MilestoneType.DailyLoginStreak);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Gets all milestone definitions.
        /// </summary>
        public IReadOnlyList<MilestoneDefinition> GetAllMilestones()
        {
            return allMilestones;
        }

        /// <summary>
        /// Gets progress for a specific milestone.
        /// </summary>
        public MilestoneProgress GetProgress(string milestoneId)
        {
            if (milestoneProgress.TryGetValue(milestoneId, out var progress))
            {
                return progress;
            }
            return null;
        }

        /// <summary>
        /// Gets current value for a milestone type.
        /// </summary>
        public double GetCurrentValue(MilestoneType type)
        {
            switch (type)
            {
                case MilestoneType.LifetimeMoney:
                    return CurrencyManager.Instance?.LifetimeMoney ?? 0;
                case MilestoneType.LifetimeDarkMatter:
                    return CurrencyManager.Instance?.LifetimeDarkMatter ?? 0;
                case MilestoneType.LifetimeTimeShards:
                    return CurrencyManager.Instance?.LifetimeTimeShards ?? 0;
                case MilestoneType.TotalDiceRolls:
                    return totalDiceRolls;
                case MilestoneType.TotalJackpots:
                    return totalJackpots;
                case MilestoneType.TotalDiceOwned:
                    return maxDiceOwned;
                case MilestoneType.TotalDiceDestroyed:
                    return OverclockManager.Instance?.TotalDiceDestroyed ?? 0;
                case MilestoneType.TimeFractureLevel:
                    return TimeFractureManager.Instance?.FractureLevel ?? 0;
                case MilestoneType.TotalTimeFractures:
                    return TimeFractureManager.Instance?.TotalFractures ?? 0;
                case MilestoneType.SkillNodesUnlocked:
                    return totalSkillNodesUnlocked;
                case MilestoneType.DiceTypesUnlocked:
                    return totalDiceTypesUnlocked;
                case MilestoneType.TotalPlayTime:
                    return totalPlayTimeMinutes;
                case MilestoneType.DailyLoginStreak:
                    return highestLoginStreak;
                case MilestoneType.MissionsCompleted:
                    return totalMissionsCompleted;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Claims a completed milestone.
        /// </summary>
        public bool ClaimMilestone(string milestoneId)
        {
            if (!milestoneProgress.TryGetValue(milestoneId, out var progress))
                return false;

            if (!progress.isCompleted || progress.isClaimed)
                return false;

            var definition = allMilestones.FirstOrDefault(m => m.milestoneId == milestoneId);
            if (definition == null)
                return false;

            // Grant rewards
            foreach (var reward in definition.rewards)
            {
                GrantReward(reward);
            }

            progress.isClaimed = true;

            if (debugMode) Debug.Log($"[MilestoneManager] Claimed milestone: {definition.displayName}");

            OnMilestoneClaimed?.Invoke(definition, definition.rewards);
            OnClaimableCountChanged?.Invoke(GetClaimableCount());

            // Play effects
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySkillUnlockSound();
            }

            return true;
        }

        /// <summary>
        /// Gets count of milestones ready to claim.
        /// </summary>
        public int GetClaimableCount()
        {
            return milestoneProgress.Values.Count(p => p.isCompleted && !p.isClaimed);
        }

        /// <summary>
        /// Gets count of completed milestones.
        /// </summary>
        public int GetCompletedCount()
        {
            return milestoneProgress.Values.Count(p => p.isCompleted);
        }

        #endregion

        #region Private Methods

        private void CheckAllMilestones()
        {
            foreach (MilestoneType type in Enum.GetValues(typeof(MilestoneType)))
            {
                CheckMilestonesOfType(type);
            }
        }

        private void CheckMilestonesOfType(MilestoneType type)
        {
            double currentValue = GetCurrentValue(type);

            foreach (var milestone in allMilestones.Where(m => m.milestoneType == type))
            {
                if (!milestoneProgress.TryGetValue(milestone.milestoneId, out var progress))
                    continue;

                if (progress.isCompleted)
                    continue;

                progress.currentProgress = currentValue;

                if (currentValue >= milestone.targetAmount)
                {
                    progress.isCompleted = true;

                    if (debugMode) Debug.Log($"[MilestoneManager] Milestone completed: {milestone.displayName}");

                    OnMilestoneCompleted?.Invoke(milestone);
                    OnClaimableCountChanged?.Invoke(GetClaimableCount());
                }
            }
        }

        private void GrantReward(MilestoneReward reward)
        {
            switch (reward.type)
            {
                case MilestoneRewardType.TimeShards:
                    CurrencyManager.Instance?.AddTimeShards(reward.amount);
                    break;

                case MilestoneRewardType.DarkMatter:
                    CurrencyManager.Instance?.AddDarkMatter(reward.amount);
                    break;

                case MilestoneRewardType.Money:
                    CurrencyManager.Instance?.AddMoney(reward.amount, false);
                    break;

                case MilestoneRewardType.PermanentMoneyBoost:
                    permanentMoneyBoost += (float)reward.amount;
                    ApplyPermanentBoosts();
                    break;

                case MilestoneRewardType.PermanentDMBoost:
                    permanentDMBoost += (float)reward.amount;
                    ApplyPermanentBoosts();
                    break;

                case MilestoneRewardType.UnlockFeature:
                    // Handle feature unlocks
                    if (debugMode) Debug.Log($"[MilestoneManager] Feature unlocked: {reward.featureId}");
                    break;
            }
        }

        private void ApplyPermanentBoosts()
        {
            if (GameStats.Instance != null)
            {
                // These boosts stack with fracture bonuses
                // The milestone boost is additive on top of existing multipliers
                GameStats.Instance.GlobalMoneyMultiplier *= (1f + permanentMoneyBoost);
                GameStats.Instance.DarkMatterGainMultiplier *= (1f + permanentDMBoost);
            }
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// Gets save data for persistence.
        /// </summary>
        public MilestoneSaveData GetSaveData()
        {
            return new MilestoneSaveData
            {
                progressList = milestoneProgress.Values.ToList(),
                totalDiceRolls = totalDiceRolls,
                totalJackpots = totalJackpots,
                maxDiceOwned = maxDiceOwned,
                totalSkillNodesUnlocked = totalSkillNodesUnlocked,
                totalDiceTypesUnlocked = totalDiceTypesUnlocked,
                totalPlayTimeMinutes = totalPlayTimeMinutes,
                highestLoginStreak = highestLoginStreak,
                totalMissionsCompleted = totalMissionsCompleted,
                permanentMoneyBoost = permanentMoneyBoost,
                permanentDMBoost = permanentDMBoost
            };
        }

        /// <summary>
        /// Sets state from save data.
        /// </summary>
        public void SetSaveData(MilestoneSaveData data)
        {
            if (data == null) return;

            // Restore stats
            totalDiceRolls = data.totalDiceRolls;
            totalJackpots = data.totalJackpots;
            maxDiceOwned = data.maxDiceOwned;
            totalSkillNodesUnlocked = data.totalSkillNodesUnlocked;
            totalDiceTypesUnlocked = data.totalDiceTypesUnlocked;
            totalPlayTimeMinutes = data.totalPlayTimeMinutes;
            highestLoginStreak = data.highestLoginStreak;
            totalMissionsCompleted = data.totalMissionsCompleted;
            permanentMoneyBoost = data.permanentMoneyBoost;
            permanentDMBoost = data.permanentDMBoost;

            // Restore progress
            if (data.progressList != null)
            {
                foreach (var progress in data.progressList)
                {
                    milestoneProgress[progress.milestoneId] = progress;
                }
            }

            // Re-check all milestones
            CheckAllMilestones();
            ApplyPermanentBoosts();

            if (debugMode) Debug.Log($"[MilestoneManager] Loaded {milestoneProgress.Count} milestone progress entries");
        }

        #endregion
    }

    /// <summary>
    /// Save data for milestone system.
    /// </summary>
    [Serializable]
    public class MilestoneSaveData
    {
        public List<MilestoneProgress> progressList;
        public double totalDiceRolls;
        public double totalJackpots;
        public int maxDiceOwned;
        public int totalSkillNodesUnlocked;
        public int totalDiceTypesUnlocked;
        public double totalPlayTimeMinutes;
        public int highestLoginStreak;
        public int totalMissionsCompleted;
        public float permanentMoneyBoost;
        public float permanentDMBoost;
    }
}
