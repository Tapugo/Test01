using System;
using UnityEngine;

namespace Incredicer.GlobalEvents
{
    /// <summary>
    /// Types of contributions that can be made to global events.
    /// </summary>
    public enum GlobalEventContributionType
    {
        DiceRolls,          // Number of dice rolls
        MoneyEarned,        // Total money earned during event
        DarkMatterEarned,   // Total DM earned during event
        JackpotsHit,        // Jackpots during event
        DiceDestroyed,      // Dice destroyed via overclock
        MissionsCompleted   // Missions completed during event
    }

    /// <summary>
    /// Tier rewards for reaching milestones in the global event.
    /// </summary>
    [Serializable]
    public class GlobalEventTierReward
    {
        [Tooltip("Progress required to reach this tier (0-1)")]
        public float progressThreshold = 0.25f;

        [Tooltip("Display name for this tier")]
        public string tierName = "Bronze";

        [Tooltip("Time Shards reward")]
        public int timeShardsReward = 10;

        [Tooltip("Dark Matter reward")]
        public double darkMatterReward = 0;

        [Tooltip("Optional special reward description")]
        public string specialReward = "";

        [Tooltip("Color for this tier")]
        public Color tierColor = Color.white;
    }

    /// <summary>
    /// ScriptableObject defining a global event.
    /// </summary>
    [CreateAssetMenu(fileName = "GlobalEvent", menuName = "Incredicer/Global Event Definition")]
    public class GlobalEventDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string eventId;
        public string eventName = "Community Challenge";
        [TextArea(2, 4)]
        public string description = "Work together to reach the goal!";

        [Header("Timing")]
        [Tooltip("Duration of the event in hours")]
        public int durationHours = 72;

        [Header("Goal")]
        public GlobalEventContributionType contributionType = GlobalEventContributionType.DiceRolls;
        [Tooltip("Total community goal to reach")]
        public double communityGoal = 1000000;

        [Header("Tier Rewards")]
        public GlobalEventTierReward[] tierRewards;

        [Header("Visual")]
        public Sprite eventIcon;
        public Color eventColor = new Color(1f, 0.8f, 0.2f);

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(eventId))
            {
                eventId = System.Guid.NewGuid().ToString().Substring(0, 8);
            }
        }
    }

    /// <summary>
    /// Runtime state for a global event.
    /// </summary>
    [Serializable]
    public class GlobalEventProgress
    {
        public string eventId;
        public double playerContribution;
        public double communityProgress;  // Simulated community progress
        public int highestTierClaimed;    // Index of highest tier claimed (-1 = none)
        public string startTimeIso;       // When the event started
        public bool isActive;

        public GlobalEventProgress() { }

        public GlobalEventProgress(string id)
        {
            eventId = id;
            playerContribution = 0;
            communityProgress = 0;
            highestTierClaimed = -1;
            startTimeIso = DateTime.UtcNow.ToString("o");
            isActive = true;
        }

        public DateTime GetStartTime()
        {
            if (DateTime.TryParse(startTimeIso, out DateTime result))
                return result;
            return DateTime.UtcNow;
        }
    }
}
