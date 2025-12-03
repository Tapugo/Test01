using System;
using System.Collections.Generic;
using UnityEngine;

namespace Incredicer.Leaderboards
{
    /// <summary>
    /// Types of leaderboards available.
    /// </summary>
    public enum LeaderboardType
    {
        LifetimeMoney,          // Total money ever earned
        LifetimeDarkMatter,     // Total DM ever earned
        LifetimeTimeShards,     // Total Time Shards ever earned
        TotalDiceRolls,         // All-time dice rolls
        HighestFractureLevel,   // Highest Time Fracture level reached
        TotalJackpots,          // Total jackpots hit
        LongestLoginStreak,     // Longest daily streak
        MissionsCompleted       // Total missions completed
    }

    /// <summary>
    /// A single entry in a leaderboard.
    /// </summary>
    [Serializable]
    public class LeaderboardEntry
    {
        public string playerId;
        public string playerName;
        public double score;
        public int rank;
        public string lastUpdatedIso;
        public bool isLocalPlayer;

        public LeaderboardEntry() { }

        public LeaderboardEntry(string id, string name, double score, int rank = 0, bool isLocal = false)
        {
            this.playerId = id;
            this.playerName = name;
            this.score = score;
            this.rank = rank;
            this.isLocalPlayer = isLocal;
            this.lastUpdatedIso = DateTime.UtcNow.ToString("o");
        }

        public DateTime GetLastUpdated()
        {
            if (DateTime.TryParse(lastUpdatedIso, out DateTime result))
                return result;
            return DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Complete leaderboard for a specific type.
    /// </summary>
    [Serializable]
    public class Leaderboard
    {
        public LeaderboardType type;
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
        public string lastRefreshIso;
        public int totalPlayers;

        public Leaderboard() { }

        public Leaderboard(LeaderboardType type)
        {
            this.type = type;
            this.entries = new List<LeaderboardEntry>();
            this.lastRefreshIso = DateTime.UtcNow.ToString("o");
            this.totalPlayers = 0;
        }

        public DateTime GetLastRefresh()
        {
            if (DateTime.TryParse(lastRefreshIso, out DateTime result))
                return result;
            return DateTime.MinValue;
        }

        public void RefreshTimestamp()
        {
            lastRefreshIso = DateTime.UtcNow.ToString("o");
        }
    }

    /// <summary>
    /// Player profile for leaderboards.
    /// </summary>
    [Serializable]
    public class PlayerProfile
    {
        public string playerId;
        public string playerName;
        public Dictionary<LeaderboardType, double> scores = new Dictionary<LeaderboardType, double>();

        // Default constructor - does NOT call Random (safe for serialization)
        public PlayerProfile()
        {
            // Leave empty - will be initialized later via Initialize()
        }

        public PlayerProfile(string id, string name)
        {
            playerId = id;
            playerName = name;
        }

        /// <summary>
        /// Initialize the player profile with generated values.
        /// Call this from Awake/Start, not during serialization.
        /// </summary>
        public void Initialize()
        {
            if (string.IsNullOrEmpty(playerId))
            {
                playerId = GeneratePlayerId();
            }
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = GeneratePlayerName();
            }
        }

        private static string GeneratePlayerId()
        {
            return Guid.NewGuid().ToString().Substring(0, 12);
        }

        public static string GeneratePlayerName()
        {
            string[] prefixes = { "Dice", "Lucky", "Roll", "Cosmic", "Dark", "Time", "Mega", "Ultra", "Super", "Grand" };
            string[] suffixes = { "Master", "Lord", "King", "Wizard", "Hunter", "Roller", "Pro", "Star", "Boss", "Champion" };
            int num = UnityEngine.Random.Range(10, 9999);

            string prefix = prefixes[UnityEngine.Random.Range(0, prefixes.Length)];
            string suffix = suffixes[UnityEngine.Random.Range(0, suffixes.Length)];

            return $"{prefix}{suffix}{num}";
        }

        public void UpdateScore(LeaderboardType type, double value)
        {
            scores[type] = value;
        }

        public double GetScore(LeaderboardType type)
        {
            return scores.TryGetValue(type, out double value) ? value : 0;
        }
    }

    /// <summary>
    /// Save data for leaderboards.
    /// </summary>
    [Serializable]
    public class LeaderboardSaveData
    {
        public string playerIdStr;
        public string playerNameStr;
        public List<LeaderboardScoreEntry> playerScores = new List<LeaderboardScoreEntry>();
    }

    /// <summary>
    /// Helper class for serializing score dictionary.
    /// </summary>
    [Serializable]
    public class LeaderboardScoreEntry
    {
        public LeaderboardType type;
        public double score;
    }
}
