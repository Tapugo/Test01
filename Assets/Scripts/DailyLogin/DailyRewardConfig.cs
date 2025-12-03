using System;
using UnityEngine;

namespace Incredicer.DailyLogin
{
    /// <summary>
    /// Types of rewards that can be given for daily login.
    /// </summary>
    public enum DailyRewardType
    {
        Money,
        DarkMatter,
        MoneyBoost,
        DMBoost,
        JackpotToken
    }

    /// <summary>
    /// Defines a single day's reward in the streak.
    /// </summary>
    [Serializable]
    public class DailyRewardDay
    {
        [Tooltip("Day number in the streak (1-7)")]
        public int dayNumber = 1;

        [Tooltip("Type of reward for this day")]
        public DailyRewardType rewardType = DailyRewardType.Money;

        [Tooltip("Base amount of the reward (before streak multiplier)")]
        public double baseAmount = 100;

        [Tooltip("Streak multiplier applied to this day's reward")]
        public float streakMultiplier = 1f;

        [Tooltip("Duration in seconds for temporary boosts (0 for permanent rewards)")]
        public float boostDurationSeconds = 0f;

        [Tooltip("Display name for this reward")]
        public string rewardTitle = "Money Burst";

        [Tooltip("Description shown to player")]
        public string rewardDescription = "A burst of cash to start your day!";
    }

    /// <summary>
    /// ScriptableObject configuration for daily login rewards.
    /// </summary>
    [CreateAssetMenu(fileName = "DailyRewardConfig", menuName = "Incredicer/Daily Reward Config")]
    public class DailyRewardConfig : ScriptableObject
    {
        [Header("Streak Settings")]
        [Tooltip("Number of days in the streak cycle (typically 7)")]
        public int streakLength = 7;

        [Tooltip("If true, missing a day resets streak. If false, streak just pauses.")]
        public bool resetStreakOnMiss = false;

        [Tooltip("Number of days offline before streak resets (only if resetStreakOnMiss is true)")]
        public int gracePeriodDays = 2;

        [Header("Rewards Per Day")]
        [Tooltip("Define rewards for each day of the streak")]
        public DailyRewardDay[] dayRewards = new DailyRewardDay[7];

        [Header("Scaling")]
        [Tooltip("Money reward scales with player's average income per minute")]
        public float moneyRewardMinutesWorth = 10f;

        [Tooltip("Base dark matter reward amount")]
        public double baseDarkMatterReward = 5;

        [Tooltip("Percentage of yesterday's DM earned to add as bonus")]
        public float dmYesterdayBonusPercent = 0.1f;

        /// <summary>
        /// Gets the reward configuration for a specific streak day.
        /// </summary>
        public DailyRewardDay GetRewardForDay(int streakDay)
        {
            int index = Mathf.Clamp(streakDay - 1, 0, dayRewards.Length - 1);
            return dayRewards[index];
        }

        /// <summary>
        /// Creates default reward configuration.
        /// </summary>
        private void Reset()
        {
            streakLength = 7;
            dayRewards = new DailyRewardDay[7];

            // Day 1: Small money burst
            dayRewards[0] = new DailyRewardDay
            {
                dayNumber = 1,
                rewardType = DailyRewardType.Money,
                baseAmount = 100,
                streakMultiplier = 1f,
                rewardTitle = "Welcome Back!",
                rewardDescription = "A small reward to start your streak."
            };

            // Day 2: Slightly more money
            dayRewards[1] = new DailyRewardDay
            {
                dayNumber = 2,
                rewardType = DailyRewardType.Money,
                baseAmount = 200,
                streakMultiplier = 1.5f,
                rewardTitle = "Keep It Rolling!",
                rewardDescription = "Your streak grows stronger!"
            };

            // Day 3: Dark Matter
            dayRewards[2] = new DailyRewardDay
            {
                dayNumber = 3,
                rewardType = DailyRewardType.DarkMatter,
                baseAmount = 10,
                streakMultiplier = 1f,
                rewardTitle = "Dark Matter Bonus!",
                rewardDescription = "Precious dark matter for your collection."
            };

            // Day 4: Money boost (temporary)
            dayRewards[3] = new DailyRewardDay
            {
                dayNumber = 4,
                rewardType = DailyRewardType.MoneyBoost,
                baseAmount = 50, // 50% boost
                streakMultiplier = 1f,
                boostDurationSeconds = 600, // 10 minutes
                rewardTitle = "Money Frenzy!",
                rewardDescription = "+50% money for 10 minutes!"
            };

            // Day 5: Big money burst
            dayRewards[4] = new DailyRewardDay
            {
                dayNumber = 5,
                rewardType = DailyRewardType.Money,
                baseAmount = 500,
                streakMultiplier = 2f,
                rewardTitle = "Halfway There!",
                rewardDescription = "A big reward for your dedication!"
            };

            // Day 6: Dark Matter boost
            dayRewards[5] = new DailyRewardDay
            {
                dayNumber = 6,
                rewardType = DailyRewardType.DMBoost,
                baseAmount = 100, // 100% boost
                streakMultiplier = 1f,
                boostDurationSeconds = 600, // 10 minutes
                rewardTitle = "Dark Matter Surge!",
                rewardDescription = "+100% dark matter for 10 minutes!"
            };

            // Day 7: Jackpot - everything!
            dayRewards[6] = new DailyRewardDay
            {
                dayNumber = 7,
                rewardType = DailyRewardType.Money,
                baseAmount = 1000,
                streakMultiplier = 3f,
                rewardTitle = "JACKPOT DAY!",
                rewardDescription = "Maximum streak! Massive rewards!"
            };
        }
    }
}
