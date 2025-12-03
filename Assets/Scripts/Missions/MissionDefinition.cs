using System;
using UnityEngine;

namespace Incredicer.Missions
{
    /// <summary>
    /// Types of missions that can be tracked.
    /// </summary>
    public enum MissionType
    {
        RollDice,           // Roll X dice
        EarnMoney,          // Earn X money
        EarnDarkMatter,     // Earn X dark matter
        BuyDice,            // Buy X dice
        UnlockSkillNodes,   // Unlock X skill nodes
        OverclockRolls,     // Roll overclocked dice X times
        DestroyDice,        // Destroy X dice by overclocking
        TimeFractures,      // Perform X time fractures
        DailyLogins,        // Log in X days
        UseActiveSkills,    // Use active skills X times
        EarnJackpots,       // Get X jackpot rolls
        SpendMoney,         // Spend X money
        SpendDarkMatter     // Spend X dark matter
    }

    /// <summary>
    /// Types of rewards that can be given for completing missions.
    /// </summary>
    public enum MissionRewardType
    {
        Money,
        DarkMatter,
        TimeShards,
        MoneyBoost,
        DMBoost
    }

    /// <summary>
    /// Defines a reward for completing a mission.
    /// </summary>
    [Serializable]
    public class MissionReward
    {
        public MissionRewardType type = MissionRewardType.Money;
        public double amount = 100;
        public float boostDuration = 0f; // For boost rewards, duration in seconds
    }

    /// <summary>
    /// ScriptableObject defining a single mission.
    /// </summary>
    [CreateAssetMenu(fileName = "Mission", menuName = "Incredicer/Mission Definition")]
    public class MissionDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string missionId;
        public string displayName = "Mission";
        public string description = "Complete this task.";

        [Header("Type")]
        public bool isDaily = true;
        public MissionType missionType = MissionType.RollDice;

        [Header("Target")]
        public double targetAmount = 100;

        [Header("Rewards")]
        public MissionReward[] rewards;

        [Header("Visual")]
        public Sprite icon;
        public Color accentColor = Color.white;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(missionId))
            {
                missionId = System.Guid.NewGuid().ToString().Substring(0, 8);
            }
        }
    }

    /// <summary>
    /// Runtime instance of a mission with progress tracking.
    /// </summary>
    [Serializable]
    public class MissionInstance
    {
        public string missionId;
        public string displayName;
        public string description;
        public bool isDaily;
        public MissionType missionType;
        public double targetAmount;
        public double currentProgress;
        public MissionReward[] rewards;
        public bool isCompleted;
        public bool isClaimed;

        public float ProgressPercent => targetAmount > 0 ? (float)(currentProgress / targetAmount) : 0f;
        public bool CanClaim => isCompleted && !isClaimed;

        public MissionInstance() { }

        public MissionInstance(MissionDefinition definition)
        {
            missionId = definition.missionId;
            displayName = definition.displayName;
            description = definition.description;
            isDaily = definition.isDaily;
            missionType = definition.missionType;
            targetAmount = definition.targetAmount;
            currentProgress = 0;
            rewards = definition.rewards;
            isCompleted = false;
            isClaimed = false;
        }

        public void AddProgress(double amount)
        {
            if (isClaimed) return;

            currentProgress += amount;
            if (currentProgress >= targetAmount)
            {
                currentProgress = targetAmount;
                isCompleted = true;
            }
        }

        public void SetProgress(double amount)
        {
            if (isClaimed) return;

            currentProgress = amount;
            if (currentProgress >= targetAmount)
            {
                currentProgress = targetAmount;
                isCompleted = true;
            }
        }
    }
}
