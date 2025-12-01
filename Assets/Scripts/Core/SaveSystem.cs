using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Incredicer.Dice;
using Incredicer.Skills;

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

            // Dice - unlock types
            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.DarkMatterUnlocked = data.darkMatterUnlocked;

                foreach (var type in data.unlockedDiceTypes)
                {
                    DiceManager.Instance.UnlockDiceType(type);
                }

                // Note: Dice spawning is handled by DiceManager.Start()
                // We could restore exact positions in a more advanced implementation
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
