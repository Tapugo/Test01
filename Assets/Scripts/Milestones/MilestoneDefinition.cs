using System;
using UnityEngine;

namespace Incredicer.Milestones
{
    /// <summary>
    /// Types of milestones that can be tracked.
    /// </summary>
    public enum MilestoneType
    {
        // Currency milestones
        LifetimeMoney,          // Total money earned ever
        LifetimeDarkMatter,     // Total DM earned ever
        LifetimeTimeShards,     // Total Time Shards earned ever

        // Dice milestones
        TotalDiceRolls,         // Total dice rolled ever
        TotalJackpots,          // Total jackpots hit
        TotalDiceOwned,         // Max dice owned at once
        TotalDiceDestroyed,     // Dice destroyed via overclock

        // Progression milestones
        TimeFractureLevel,      // Time Fracture level reached
        TotalTimeFractures,     // Total fractures performed
        SkillNodesUnlocked,     // Total skill nodes unlocked
        DiceTypesUnlocked,      // Dice types unlocked

        // Session milestones
        TotalPlayTime,          // Total minutes played
        DailyLoginStreak,       // Highest login streak
        MissionsCompleted       // Total missions completed
    }

    /// <summary>
    /// Types of rewards for completing milestones.
    /// </summary>
    public enum MilestoneRewardType
    {
        TimeShards,
        DarkMatter,
        Money,
        PermanentMoneyBoost,    // Permanent % boost to money
        PermanentDMBoost,       // Permanent % boost to DM
        UnlockFeature           // Unlock a special feature
    }

    /// <summary>
    /// Defines a reward for completing a milestone.
    /// </summary>
    [Serializable]
    public class MilestoneReward
    {
        public MilestoneRewardType type = MilestoneRewardType.TimeShards;
        public double amount = 10;
        public string featureId = ""; // For UnlockFeature type
    }

    /// <summary>
    /// ScriptableObject defining a single milestone.
    /// </summary>
    [CreateAssetMenu(fileName = "Milestone", menuName = "Incredicer/Milestone Definition")]
    public class MilestoneDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string milestoneId;
        public string displayName = "Milestone";
        public string description = "Achieve something great.";

        [Header("Requirement")]
        public MilestoneType milestoneType = MilestoneType.LifetimeMoney;
        public double targetAmount = 1000;

        [Header("Rewards")]
        public MilestoneReward[] rewards;

        [Header("Visual")]
        public Sprite icon;
        public Color accentColor = Color.yellow;

        [Header("Tier")]
        [Tooltip("Higher tier = later in progression, affects sorting")]
        public int tier = 1;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(milestoneId))
            {
                milestoneId = System.Guid.NewGuid().ToString().Substring(0, 8);
            }
        }
    }

    /// <summary>
    /// Runtime state for a milestone.
    /// </summary>
    [Serializable]
    public class MilestoneProgress
    {
        public string milestoneId;
        public double currentProgress;
        public bool isCompleted;
        public bool isClaimed;

        public MilestoneProgress() { }

        public MilestoneProgress(string id)
        {
            milestoneId = id;
            currentProgress = 0;
            isCompleted = false;
            isClaimed = false;
        }
    }
}
