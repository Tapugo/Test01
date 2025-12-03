using System;
using System.Collections.Generic;
using UnityEngine;
using Incredicer.Core;
using Incredicer.Milestones;
using Incredicer.TimeFracture;
using Incredicer.DailyLogin;

namespace Incredicer.Leaderboards
{
    /// <summary>
    /// Manages asynchronous leaderboards with simulated competition.
    /// Since this is a single-player game, other players are simulated.
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private int entriesPerLeaderboard = 100;
        [SerializeField] private float refreshCooldownSeconds = 60f;
        [SerializeField] private float scoreUpdateInterval = 30f;

        // Player data
        private PlayerProfile localPlayer;

        // Cached leaderboards
        private Dictionary<LeaderboardType, Leaderboard> leaderboards = new Dictionary<LeaderboardType, Leaderboard>();

        // State tracking
        private float lastScoreUpdateTime = 0f;
        private Dictionary<LeaderboardType, float> lastRefreshTimes = new Dictionary<LeaderboardType, float>();

        // Events
        public event Action<LeaderboardType, Leaderboard> OnLeaderboardUpdated;
        public event Action<LeaderboardType, int> OnPlayerRankChanged;  // type, new rank

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Initialize leaderboards
            foreach (LeaderboardType type in Enum.GetValues(typeof(LeaderboardType)))
            {
                leaderboards[type] = new Leaderboard(type);
                lastRefreshTimes[type] = 0f;
            }
        }

        private void Start()
        {
            // Create or load player profile
            if (localPlayer == null)
            {
                localPlayer = new PlayerProfile();
            }
            // Initialize the profile (generates name using Random - safe in Start)
            localPlayer.Initialize();

            // Generate initial fake leaderboard data
            GenerateSimulatedLeaderboards();

            Debug.Log($"[LeaderboardManager] Initialized for player: {localPlayer.playerName}");
        }

        private void Update()
        {
            // Periodically update player scores
            if (Time.time - lastScoreUpdateTime >= scoreUpdateInterval)
            {
                lastScoreUpdateTime = Time.time;
                UpdatePlayerScores();
            }
        }

        #region Simulated Data Generation

        /// <summary>
        /// Generates simulated leaderboard data for all types.
        /// </summary>
        private void GenerateSimulatedLeaderboards()
        {
            GenerateSimulatedLeaderboard(LeaderboardType.LifetimeMoney, 1000, 100000000);
            GenerateSimulatedLeaderboard(LeaderboardType.LifetimeDarkMatter, 100, 500000);
            GenerateSimulatedLeaderboard(LeaderboardType.LifetimeTimeShards, 50, 50000);
            GenerateSimulatedLeaderboard(LeaderboardType.TotalDiceRolls, 1000, 10000000);
            GenerateSimulatedLeaderboard(LeaderboardType.HighestFractureLevel, 1, 50);
            GenerateSimulatedLeaderboard(LeaderboardType.TotalJackpots, 10, 100000);
            GenerateSimulatedLeaderboard(LeaderboardType.LongestLoginStreak, 1, 365);
            GenerateSimulatedLeaderboard(LeaderboardType.MissionsCompleted, 10, 10000);

            Debug.Log("[LeaderboardManager] Generated simulated leaderboard data");
        }

        private void GenerateSimulatedLeaderboard(LeaderboardType type, double minScore, double maxScore)
        {
            var leaderboard = leaderboards[type];
            leaderboard.entries.Clear();

            // Generate fake players with realistic distribution (top players have much higher scores)
            for (int i = 0; i < entriesPerLeaderboard; i++)
            {
                // Use exponential distribution for more realistic scores
                float t = (float)i / entriesPerLeaderboard;
                double score = maxScore * Math.Pow(1 - t, 2.5);
                score = Math.Max(minScore, score);

                // Add some randomness
                score *= UnityEngine.Random.Range(0.8f, 1.2f);

                var entry = new LeaderboardEntry(
                    Guid.NewGuid().ToString().Substring(0, 8),
                    GenerateFakePlayerName(),
                    score,
                    i + 1,
                    false
                );

                leaderboard.entries.Add(entry);
            }

            // Sort by score descending
            leaderboard.entries.Sort((a, b) => b.score.CompareTo(a.score));

            // Reassign ranks
            for (int i = 0; i < leaderboard.entries.Count; i++)
            {
                leaderboard.entries[i].rank = i + 1;
            }

            leaderboard.totalPlayers = UnityEngine.Random.Range(10000, 100000);
            leaderboard.RefreshTimestamp();
        }

        private string GenerateFakePlayerName()
        {
            string[] prefixes = { "Dice", "Lucky", "Roll", "Cosmic", "Dark", "Time", "Mega", "Ultra", "Super", "Grand",
                                  "Epic", "Pro", "Elite", "Master", "Noob", "Quick", "Swift", "Brave", "Wild", "Cool" };
            string[] suffixes = { "Master", "Lord", "King", "Wizard", "Hunter", "Roller", "Pro", "Star", "Boss", "Champion",
                                  "Slayer", "Ninja", "Shark", "Tiger", "Dragon", "Phoenix", "Legend", "Hero", "Ace", "Ghost" };
            int num = UnityEngine.Random.Range(1, 99999);

            string prefix = prefixes[UnityEngine.Random.Range(0, prefixes.Length)];
            string suffix = suffixes[UnityEngine.Random.Range(0, suffixes.Length)];

            // Various name formats
            int format = UnityEngine.Random.Range(0, 4);
            switch (format)
            {
                case 0: return $"{prefix}{suffix}{num}";
                case 1: return $"{prefix}_{suffix}";
                case 2: return $"x{prefix}{num}x";
                default: return $"{prefix}{num}";
            }
        }

        #endregion

        #region Score Updates

        /// <summary>
        /// Updates the local player's scores from game state.
        /// </summary>
        private void UpdatePlayerScores()
        {
            if (localPlayer == null) return;

            // Get current stats from various managers
            if (CurrencyManager.Instance != null)
            {
                localPlayer.UpdateScore(LeaderboardType.LifetimeMoney, CurrencyManager.Instance.LifetimeMoney);
                localPlayer.UpdateScore(LeaderboardType.LifetimeDarkMatter, CurrencyManager.Instance.LifetimeDarkMatter);
                localPlayer.UpdateScore(LeaderboardType.LifetimeTimeShards, CurrencyManager.Instance.LifetimeTimeShards);
            }

            if (MilestoneManager.Instance != null)
            {
                var data = MilestoneManager.Instance.GetSaveData();
                localPlayer.UpdateScore(LeaderboardType.TotalDiceRolls, data.totalDiceRolls);
                localPlayer.UpdateScore(LeaderboardType.TotalJackpots, data.totalJackpots);
                localPlayer.UpdateScore(LeaderboardType.LongestLoginStreak, data.highestLoginStreak);
                localPlayer.UpdateScore(LeaderboardType.MissionsCompleted, data.totalMissionsCompleted);
            }

            if (TimeFractureManager.Instance != null)
            {
                localPlayer.UpdateScore(LeaderboardType.HighestFractureLevel, TimeFractureManager.Instance.FractureLevel);
            }

            // Update player position in each leaderboard
            foreach (LeaderboardType type in Enum.GetValues(typeof(LeaderboardType)))
            {
                UpdatePlayerInLeaderboard(type);
            }
        }

        private void UpdatePlayerInLeaderboard(LeaderboardType type)
        {
            var leaderboard = leaderboards[type];
            double playerScore = localPlayer.GetScore(type);

            // Find or create local player entry
            int existingIndex = leaderboard.entries.FindIndex(e => e.isLocalPlayer);
            LeaderboardEntry playerEntry;

            if (existingIndex >= 0)
            {
                playerEntry = leaderboard.entries[existingIndex];
                int oldRank = playerEntry.rank;
                playerEntry.score = playerScore;
                playerEntry.lastUpdatedIso = DateTime.UtcNow.ToString("o");

                // Resort and recalculate ranks
                leaderboard.entries.Sort((a, b) => b.score.CompareTo(a.score));
                for (int i = 0; i < leaderboard.entries.Count; i++)
                {
                    leaderboard.entries[i].rank = i + 1;
                }

                if (playerEntry.rank != oldRank)
                {
                    OnPlayerRankChanged?.Invoke(type, playerEntry.rank);
                }
            }
            else if (playerScore > 0)
            {
                // Add player to leaderboard
                playerEntry = new LeaderboardEntry(
                    localPlayer.playerId,
                    localPlayer.playerName,
                    playerScore,
                    0,
                    true
                );

                leaderboard.entries.Add(playerEntry);

                // Resort and recalculate ranks
                leaderboard.entries.Sort((a, b) => b.score.CompareTo(a.score));
                for (int i = 0; i < leaderboard.entries.Count; i++)
                {
                    leaderboard.entries[i].rank = i + 1;
                }

                // Trim to max entries, but always keep local player
                while (leaderboard.entries.Count > entriesPerLeaderboard)
                {
                    var lastEntry = leaderboard.entries[leaderboard.entries.Count - 1];
                    if (!lastEntry.isLocalPlayer)
                    {
                        leaderboard.entries.RemoveAt(leaderboard.entries.Count - 1);
                    }
                    else
                    {
                        // Remove second-to-last instead
                        if (leaderboard.entries.Count > 1)
                        {
                            leaderboard.entries.RemoveAt(leaderboard.entries.Count - 2);
                        }
                        else break;
                    }
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Gets the leaderboard for a specific type.
        /// </summary>
        public Leaderboard GetLeaderboard(LeaderboardType type)
        {
            return leaderboards.TryGetValue(type, out var lb) ? lb : null;
        }

        /// <summary>
        /// Forces a refresh of a specific leaderboard.
        /// </summary>
        public bool RefreshLeaderboard(LeaderboardType type)
        {
            if (Time.time - lastRefreshTimes[type] < refreshCooldownSeconds)
            {
                Debug.Log($"[LeaderboardManager] Refresh on cooldown for {type}");
                return false;
            }

            // Simulate network delay and update
            SimulateLeaderboardChanges(type);
            UpdatePlayerInLeaderboard(type);

            lastRefreshTimes[type] = Time.time;
            leaderboards[type].RefreshTimestamp();

            OnLeaderboardUpdated?.Invoke(type, leaderboards[type]);
            Debug.Log($"[LeaderboardManager] Refreshed {type} leaderboard");

            return true;
        }

        /// <summary>
        /// Gets time until next refresh is allowed.
        /// </summary>
        public float GetRefreshCooldown(LeaderboardType type)
        {
            float elapsed = Time.time - lastRefreshTimes[type];
            return Mathf.Max(0, refreshCooldownSeconds - elapsed);
        }

        /// <summary>
        /// Gets the local player's entry for a leaderboard.
        /// </summary>
        public LeaderboardEntry GetPlayerEntry(LeaderboardType type)
        {
            var lb = GetLeaderboard(type);
            if (lb == null) return null;
            return lb.entries.Find(e => e.isLocalPlayer);
        }

        /// <summary>
        /// Gets the local player's rank for a leaderboard.
        /// </summary>
        public int GetPlayerRank(LeaderboardType type)
        {
            var entry = GetPlayerEntry(type);
            return entry?.rank ?? -1;
        }

        /// <summary>
        /// Gets entries around the player's position.
        /// </summary>
        public List<LeaderboardEntry> GetEntriesAroundPlayer(LeaderboardType type, int countAbove = 5, int countBelow = 5)
        {
            var lb = GetLeaderboard(type);
            if (lb == null) return new List<LeaderboardEntry>();

            int playerIndex = lb.entries.FindIndex(e => e.isLocalPlayer);
            if (playerIndex < 0)
            {
                // Return top entries if player not found
                return lb.entries.GetRange(0, Math.Min(countAbove + countBelow + 1, lb.entries.Count));
            }

            int startIndex = Math.Max(0, playerIndex - countAbove);
            int endIndex = Math.Min(lb.entries.Count - 1, playerIndex + countBelow);
            int count = endIndex - startIndex + 1;

            return lb.entries.GetRange(startIndex, count);
        }

        /// <summary>
        /// Gets the top N entries for a leaderboard.
        /// </summary>
        public List<LeaderboardEntry> GetTopEntries(LeaderboardType type, int count = 10)
        {
            var lb = GetLeaderboard(type);
            if (lb == null) return new List<LeaderboardEntry>();
            return lb.entries.GetRange(0, Math.Min(count, lb.entries.Count));
        }

        /// <summary>
        /// Gets the local player profile.
        /// </summary>
        public PlayerProfile GetLocalPlayer()
        {
            return localPlayer;
        }

        /// <summary>
        /// Sets the local player's display name.
        /// </summary>
        public void SetPlayerName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            // Sanitize name
            name = name.Trim();
            if (name.Length > 20) name = name.Substring(0, 20);

            localPlayer.playerName = name;

            // Update in all leaderboards
            foreach (var lb in leaderboards.Values)
            {
                var entry = lb.entries.Find(e => e.isLocalPlayer);
                if (entry != null)
                {
                    entry.playerName = name;
                }
            }

            Debug.Log($"[LeaderboardManager] Player name changed to: {name}");
        }

        /// <summary>
        /// Gets display name for a leaderboard type.
        /// </summary>
        public static string GetLeaderboardDisplayName(LeaderboardType type)
        {
            switch (type)
            {
                case LeaderboardType.LifetimeMoney: return "Lifetime Money";
                case LeaderboardType.LifetimeDarkMatter: return "Lifetime Dark Matter";
                case LeaderboardType.LifetimeTimeShards: return "Lifetime Time Shards";
                case LeaderboardType.TotalDiceRolls: return "Total Dice Rolls";
                case LeaderboardType.HighestFractureLevel: return "Fracture Level";
                case LeaderboardType.TotalJackpots: return "Total Jackpots";
                case LeaderboardType.LongestLoginStreak: return "Login Streak";
                case LeaderboardType.MissionsCompleted: return "Missions Completed";
                default: return type.ToString();
            }
        }

        #endregion

        #region Simulation

        /// <summary>
        /// Simulates other players' scores changing over time.
        /// </summary>
        private void SimulateLeaderboardChanges(LeaderboardType type)
        {
            var lb = leaderboards[type];

            foreach (var entry in lb.entries)
            {
                if (entry.isLocalPlayer) continue;

                // Random chance to update score
                if (UnityEngine.Random.value < 0.3f)
                {
                    // Small random increase
                    double increase = entry.score * UnityEngine.Random.Range(0.001f, 0.05f);
                    entry.score += increase;
                    entry.lastUpdatedIso = DateTime.UtcNow.ToString("o");
                }
            }

            // Resort
            lb.entries.Sort((a, b) => b.score.CompareTo(a.score));
            for (int i = 0; i < lb.entries.Count; i++)
            {
                lb.entries[i].rank = i + 1;
            }
        }

        #endregion

        #region Save/Load

        public LeaderboardSaveData GetSaveData()
        {
            var data = new LeaderboardSaveData
            {
                playerIdStr = localPlayer.playerId,
                playerNameStr = localPlayer.playerName,
                playerScores = new List<LeaderboardScoreEntry>()
            };

            foreach (var kvp in localPlayer.scores)
            {
                data.playerScores.Add(new LeaderboardScoreEntry { type = kvp.Key, score = kvp.Value });
            }

            return data;
        }

        public void LoadSaveData(LeaderboardSaveData data)
        {
            if (data == null) return;

            localPlayer = new PlayerProfile(
                string.IsNullOrEmpty(data.playerIdStr) ? Guid.NewGuid().ToString().Substring(0, 12) : data.playerIdStr,
                string.IsNullOrEmpty(data.playerNameStr) ? "Player" : data.playerNameStr
            );

            if (data.playerScores != null)
            {
                foreach (var scoreEntry in data.playerScores)
                {
                    localPlayer.scores[scoreEntry.type] = scoreEntry.score;
                }
            }

            // Update player in all leaderboards
            foreach (LeaderboardType type in Enum.GetValues(typeof(LeaderboardType)))
            {
                UpdatePlayerInLeaderboard(type);
            }

            Debug.Log($"[LeaderboardManager] Loaded player: {localPlayer.playerName}");
        }

        #endregion
    }
}
