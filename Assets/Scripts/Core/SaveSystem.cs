using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Incredicer.Dice;
using Incredicer.Skills;
using Incredicer.DailyLogin;
using Incredicer.Missions;
using Incredicer.Overclock;
using Incredicer.TimeFracture;
using Incredicer.Milestones;
using Incredicer.GlobalEvents;
using Incredicer.Leaderboards;

namespace Incredicer.Core
{
    /// <summary>
    /// Data container for all saveable game state.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // Version for migration support
        public int saveVersion = 1;
        public DateTime saveTimestamp;

        // Currency
        public double money;
        public double darkMatter;
        public double lifetimeMoney;
        public double lifetimeDarkMatter;

        // Dice
        public List<SavedDice> ownedDice = new List<SavedDice>();
        public List<DiceType> unlockedDiceTypes = new List<DiceType>();
        public bool darkMatterUnlocked;

        // Skill Tree
        public List<SkillNodeId> unlockedSkillNodes = new List<SkillNodeId>();
        public List<ActiveSkillType> unlockedActiveSkills = new List<ActiveSkillType>();

        // GameStats (upgrade levels)
        public int diceValueUpgradeLevel;

        // Prestige
        public int prestigeLevel;
        public double totalPrestigeDarkMatterEarned;
        public bool hasAscended;

        // Daily Login
        public string lastLoginDateStr; // DateTime stored as string for JSON serialization
        public int currentStreakDay;
        public bool hasRolledToday;
        public int totalLoginDays;
        public double yesterdayDMEarned;

        // Missions
        public List<MissionInstance> dailyMissions = new List<MissionInstance>();
        public List<MissionInstance> weeklyMissions = new List<MissionInstance>();
        public string lastDailyResetDateStr;
        public string lastWeeklyResetDateStr;

        // Overclock
        public int totalDiceDestroyed;
        public double totalDMFromDestruction;

        // Time Fracture
        public double timeShards;
        public double lifetimeTimeShards;
        public int fractureLevel;
        public int totalFractures;
        public double totalTimeShardsEarned;

        // Milestones
        public List<MilestoneProgress> milestoneProgress = new List<MilestoneProgress>();
        public double milestoneTotalDiceRolls;
        public double milestoneTotalJackpots;
        public int milestoneMaxDiceOwned;
        public int milestoneTotalSkillNodes;
        public int milestoneTotalDiceTypes;
        public double milestoneTotalPlayTime;
        public int milestoneHighestStreak;
        public int milestoneTotalMissions;
        public float milestonePermanentMoneyBoost;
        public float milestonePermanentDMBoost;

        // Global Events
        public GlobalEventSaveData globalEventData;

        // Leaderboards
        public LeaderboardSaveData leaderboardData;
    }

    /// <summary>
    /// Saved data for a single dice.
    /// </summary>
    [Serializable]
    public class SavedDice
    {
        public DiceType type;
        public Vector2 position;
    }

    /// <summary>
    /// Handles saving and loading game state.
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float autoSaveInterval = 60f; // seconds
        [SerializeField] private bool enableAutoSave = true;

        private string SavePath => Path.Combine(Application.persistentDataPath, "incredicer_save.json");
        private float lastSaveTime;

        public event Action OnSaveCompleted;
        public event Action OnLoadCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Load game on start
            LoadGame();
            lastSaveTime = Time.time;
        }

        private void Update()
        {
            // Auto-save periodically
            if (enableAutoSave && Time.time - lastSaveTime >= autoSaveInterval)
            {
                SaveGame();
                lastSaveTime = Time.time;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // Save when app is paused (mobile)
            if (pauseStatus)
            {
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            // Save when quitting
            SaveGame();
        }

        /// <summary>
        /// Saves the current game state to disk.
        /// </summary>
        public void SaveGame()
        {
            try
            {
                SaveData data = GatherSaveData();
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
                Debug.Log($"[SaveSystem] Game saved to: {SavePath}");
                OnSaveCompleted?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save game: {e.Message}");
            }
        }

        /// <summary>
        /// Loads the game state from disk.
        /// </summary>
        public void LoadGame()
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    Debug.Log("[SaveSystem] No save file found, starting fresh");
                    return;
                }

                string json = File.ReadAllText(SavePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                if (data != null)
                {
                    ApplySaveData(data);
                    Debug.Log("[SaveSystem] Game loaded successfully");
                    OnLoadCompleted?.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load game: {e.Message}");
            }
        }

        /// <summary>
        /// Deletes the save file and resets the game.
        /// </summary>
        public void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                    Debug.Log("[SaveSystem] Save file deleted");
                }

                // Reset all systems
                ResetGame();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to delete save: {e.Message}");
            }
        }

        /// <summary>
        /// Resets the game to initial state.
        /// </summary>
        public void ResetGame()
        {
            // Reset currency
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.ResetAll();
            }

            // Reset stats
            if (GameStats.Instance != null)
            {
                GameStats.Instance.ResetToDefaults();
                GameStats.Instance.DiceValueUpgradeLevel = 0;
            }

            // Reset skill tree
            if (SkillTreeManager.Instance != null)
            {
                SkillTreeManager.Instance.ResetSkillTree(false);
            }

            Debug.Log("[SaveSystem] Game reset to initial state");
        }

        /// <summary>
        /// Gathers all saveable data from game systems.
        /// </summary>
        private SaveData GatherSaveData()
        {
            SaveData data = new SaveData
            {
                saveTimestamp = DateTime.Now
            };

            // Currency
            if (CurrencyManager.Instance != null)
            {
                data.money = CurrencyManager.Instance.Money;
                data.darkMatter = CurrencyManager.Instance.DarkMatter;
                data.lifetimeMoney = CurrencyManager.Instance.LifetimeMoney;
                data.lifetimeDarkMatter = CurrencyManager.Instance.LifetimeDarkMatter;
            }

            // Dice
            if (DiceManager.Instance != null)
            {
                data.darkMatterUnlocked = DiceManager.Instance.DarkMatterUnlocked;

                // Save dice positions and types
                var allDice = DiceManager.Instance.GetAllDice();
                foreach (var dice in allDice)
                {
                    if (dice != null && dice.Data != null)
                    {
                        data.ownedDice.Add(new SavedDice
                        {
                            type = dice.Data.type,
                            position = dice.transform.position
                        });
                    }
                }

                // Save unlocked dice types
                foreach (DiceType type in Enum.GetValues(typeof(DiceType)))
                {
                    if (DiceManager.Instance.IsDiceTypeUnlocked(type))
                    {
                        data.unlockedDiceTypes.Add(type);
                    }
                }
            }

            // Skill Tree
            if (SkillTreeManager.Instance != null)
            {
                data.unlockedSkillNodes = SkillTreeManager.Instance.GetUnlockedNodesList();
                data.unlockedActiveSkills = SkillTreeManager.Instance.GetUnlockedActiveSkillsList();
            }

            // GameStats
            if (GameStats.Instance != null)
            {
                data.diceValueUpgradeLevel = GameStats.Instance.DiceValueUpgradeLevel;
            }

            // Prestige state
            if (PrestigeManager.Instance != null)
            {
                data.prestigeLevel = PrestigeManager.Instance.AscensionLevel;
                data.totalPrestigeDarkMatterEarned = PrestigeManager.Instance.TotalDarkMatterEarned;
                data.hasAscended = PrestigeManager.Instance.HasAscended;
            }

            // Daily Login state
            if (DailyLoginManager.Instance != null)
            {
                DateTime lastLogin;
                int streakDay, totalLogins;
                bool hasRolled;
                double yesterdayDM;
                DailyLoginManager.Instance.GetSaveData(out lastLogin, out streakDay, out hasRolled, out totalLogins, out yesterdayDM);

                data.lastLoginDateStr = lastLogin.ToString("o"); // ISO 8601 format
                data.currentStreakDay = streakDay;
                data.hasRolledToday = hasRolled;
                data.totalLoginDays = totalLogins;
                data.yesterdayDMEarned = yesterdayDM;
            }

            // Missions
            if (MissionManager.Instance != null)
            {
                var missionData = MissionManager.Instance.GetSaveData();
                data.dailyMissions = missionData.dailyMissions;
                data.weeklyMissions = missionData.weeklyMissions;
                data.lastDailyResetDateStr = missionData.lastDailyResetStr;
                data.lastWeeklyResetDateStr = missionData.lastWeeklyResetStr;
            }

            // Overclock
            if (OverclockManager.Instance != null)
            {
                var overclockData = OverclockManager.Instance.GetSaveData();
                data.totalDiceDestroyed = overclockData.totalDiceDestroyed;
                data.totalDMFromDestruction = overclockData.totalDMFromDestruction;
            }

            // Time Fracture
            if (CurrencyManager.Instance != null)
            {
                data.timeShards = CurrencyManager.Instance.TimeShards;
                data.lifetimeTimeShards = CurrencyManager.Instance.LifetimeTimeShards;
            }

            if (TimeFractureManager.Instance != null)
            {
                var fractureData = TimeFractureManager.Instance.GetSaveData();
                data.fractureLevel = fractureData.fractureLevel;
                data.totalFractures = fractureData.totalFractures;
                data.totalTimeShardsEarned = fractureData.totalTimeShardsEarned;
            }

            // Milestones
            if (MilestoneManager.Instance != null)
            {
                var milestoneData = MilestoneManager.Instance.GetSaveData();
                data.milestoneProgress = milestoneData.progressList;
                data.milestoneTotalDiceRolls = milestoneData.totalDiceRolls;
                data.milestoneTotalJackpots = milestoneData.totalJackpots;
                data.milestoneMaxDiceOwned = milestoneData.maxDiceOwned;
                data.milestoneTotalSkillNodes = milestoneData.totalSkillNodesUnlocked;
                data.milestoneTotalDiceTypes = milestoneData.totalDiceTypesUnlocked;
                data.milestoneTotalPlayTime = milestoneData.totalPlayTimeMinutes;
                data.milestoneHighestStreak = milestoneData.highestLoginStreak;
                data.milestoneTotalMissions = milestoneData.totalMissionsCompleted;
                data.milestonePermanentMoneyBoost = milestoneData.permanentMoneyBoost;
                data.milestonePermanentDMBoost = milestoneData.permanentDMBoost;
            }

            // Global Events
            if (GlobalEventManager.Instance != null)
            {
                data.globalEventData = GlobalEventManager.Instance.GetSaveData();
            }

            // Leaderboards
            if (LeaderboardManager.Instance != null)
            {
                data.leaderboardData = LeaderboardManager.Instance.GetSaveData();
            }

            return data;
        }

        /// <summary>
        /// Applies loaded save data to game systems.
        /// </summary>
        private void ApplySaveData(SaveData data)
        {
            // Currency
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.SetCurrencies(
                    data.money,
                    data.darkMatter,
                    data.lifetimeMoney,
                    data.lifetimeDarkMatter
                );
            }

            // GameStats (before skill tree so base values are set)
            if (GameStats.Instance != null)
            {
                GameStats.Instance.DiceValueUpgradeLevel = data.diceValueUpgradeLevel;
            }

            // Skill Tree - set state then reapply effects
            if (SkillTreeManager.Instance != null)
            {
                SkillTreeManager.Instance.SetUnlockedNodes(new HashSet<SkillNodeId>(data.unlockedSkillNodes));
                SkillTreeManager.Instance.SetUnlockedActiveSkills(new HashSet<ActiveSkillType>(data.unlockedActiveSkills));
                SkillTreeManager.Instance.ReapplyAllEffects();
            }

            // Prestige state (before dice so DarkMatterUnlocked is handled correctly)
            if (PrestigeManager.Instance != null)
            {
                PrestigeManager.Instance.SetPrestigeState(data.prestigeLevel, data.totalPrestigeDarkMatterEarned);
            }

            // Dice - unlock types and restore saved dice
            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.DarkMatterUnlocked = data.darkMatterUnlocked;

                foreach (var type in data.unlockedDiceTypes)
                {
                    DiceManager.Instance.UnlockDiceType(type);
                }

                // Restore saved dice (spawn them from saved data)
                if (data.ownedDice != null && data.ownedDice.Count > 0)
                {
                    DiceManager.Instance.RestoreSavedDice(data.ownedDice);
                }
            }

            // Daily Login state
            if (DailyLoginManager.Instance != null && !string.IsNullOrEmpty(data.lastLoginDateStr))
            {
                DateTime lastLogin;
                if (DateTime.TryParse(data.lastLoginDateStr, out lastLogin))
                {
                    DailyLoginManager.Instance.SetSaveData(
                        lastLogin,
                        data.currentStreakDay,
                        data.hasRolledToday,
                        data.totalLoginDays,
                        data.yesterdayDMEarned
                    );
                }
            }

            // Missions
            if (MissionManager.Instance != null)
            {
                var missionData = new MissionSaveData
                {
                    dailyMissions = data.dailyMissions,
                    weeklyMissions = data.weeklyMissions,
                    lastDailyResetStr = data.lastDailyResetDateStr,
                    lastWeeklyResetStr = data.lastWeeklyResetDateStr
                };
                MissionManager.Instance.SetSaveData(missionData);
            }

            // Overclock
            if (OverclockManager.Instance != null)
            {
                var overclockData = new OverclockSaveData
                {
                    totalDiceDestroyed = data.totalDiceDestroyed,
                    totalDMFromDestruction = data.totalDMFromDestruction
                };
                OverclockManager.Instance.SetSaveData(overclockData);
            }

            // Time Fracture - load fracture manager first so bonuses are applied
            if (TimeFractureManager.Instance != null)
            {
                var fractureData = new TimeFractureSaveData
                {
                    fractureLevel = data.fractureLevel,
                    totalFractures = data.totalFractures,
                    totalTimeShardsEarned = data.totalTimeShardsEarned
                };
                TimeFractureManager.Instance.SetSaveData(fractureData);
            }

            // Load Time Shards into currency (after fracture manager so bonuses are set)
            if (CurrencyManager.Instance != null && data.timeShards > 0)
            {
                CurrencyManager.Instance.SetAllCurrencies(
                    data.money,
                    data.darkMatter,
                    data.timeShards,
                    data.lifetimeMoney,
                    data.lifetimeDarkMatter,
                    data.lifetimeTimeShards
                );
            }

            // Milestones
            if (MilestoneManager.Instance != null)
            {
                var milestoneData = new MilestoneSaveData
                {
                    progressList = data.milestoneProgress,
                    totalDiceRolls = data.milestoneTotalDiceRolls,
                    totalJackpots = data.milestoneTotalJackpots,
                    maxDiceOwned = data.milestoneMaxDiceOwned,
                    totalSkillNodesUnlocked = data.milestoneTotalSkillNodes,
                    totalDiceTypesUnlocked = data.milestoneTotalDiceTypes,
                    totalPlayTimeMinutes = data.milestoneTotalPlayTime,
                    highestLoginStreak = data.milestoneHighestStreak,
                    totalMissionsCompleted = data.milestoneTotalMissions,
                    permanentMoneyBoost = data.milestonePermanentMoneyBoost,
                    permanentDMBoost = data.milestonePermanentDMBoost
                };
                MilestoneManager.Instance.SetSaveData(milestoneData);
            }

            // Global Events
            if (GlobalEventManager.Instance != null && data.globalEventData != null)
            {
                GlobalEventManager.Instance.LoadSaveData(data.globalEventData);
            }

            // Leaderboards
            if (LeaderboardManager.Instance != null && data.leaderboardData != null)
            {
                LeaderboardManager.Instance.LoadSaveData(data.leaderboardData);
            }
        }

        /// <summary>
        /// Checks if a save file exists.
        /// </summary>
        public bool HasSaveFile()
        {
            return File.Exists(SavePath);
        }
    }
}
