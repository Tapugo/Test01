using System;
using System.Collections.Generic;
using UnityEngine;
using Incredicer.Core;
using Incredicer.Dice;
using Incredicer.Skills;
using Incredicer.Missions;
using Incredicer.Helpers;

namespace Incredicer.TimeFracture
{
    /// <summary>
    /// Configuration for Time Fracture bonuses per level.
    /// </summary>
    [Serializable]
    public class TimeFractureBonuses
    {
        [Tooltip("Multiplier to all money earned (1.0 = no bonus)")]
        public float moneyMultiplier = 1f;

        [Tooltip("Multiplier to all dark matter earned")]
        public float darkMatterMultiplier = 1f;

        [Tooltip("Bonus to dice roll value")]
        public int diceValueBonus = 0;

        [Tooltip("Starting money after fracture")]
        public double startingMoney = 0;

        [Tooltip("Reduction to skill costs (0.1 = 10% cheaper)")]
        public float skillCostReduction = 0f;
    }

    /// <summary>
    /// Manages the Time Fracture (prestige) system.
    /// Players sacrifice their progress to earn Time Shards and permanent bonuses.
    /// </summary>
    public class TimeFractureManager : MonoBehaviour
    {
        public static TimeFractureManager Instance { get; private set; }

        [Header("Requirements")]
        [SerializeField] private double baseMoneyRequired = 100000;
        [SerializeField] private float moneyRequiredGrowth = 3.0f;
        [SerializeField] private double baseDarkMatterRequired = 500;

        [Header("Rewards")]
        [SerializeField] private double baseTimeShardsReward = 10;
        [SerializeField] private float timeShardsPerLevel = 5f;
        [SerializeField] private float timeShardsFromDMPercent = 0.1f; // 10% of DM converted to shards

        [Header("Bonuses Per Level")]
        [SerializeField] private float moneyMultiplierPerLevel = 0.1f;      // +10% per level
        [SerializeField] private float dmMultiplierPerLevel = 0.05f;        // +5% per level
        [SerializeField] private int diceValueBonusPerLevel = 1;            // +1 per level
        [SerializeField] private float skillCostReductionPerLevel = 0.02f;  // -2% per level (max 50%)
        [SerializeField] private double startingMoneyPerLevel = 100;        // +100 starting money per level

        [Header("State")]
        [SerializeField] private int fractureLevel = 0;
        [SerializeField] private int totalFractures = 0;
        [SerializeField] private double totalTimeShardsEarned = 0;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        // Events
        public event Action<int> OnTimeFractureCompleted; // fractureLevel
        public event Action<TimeFractureBonuses> OnBonusesChanged;

        // Current bonuses (cached)
        private TimeFractureBonuses currentBonuses;

        // Properties
        public int FractureLevel => fractureLevel;
        public int TotalFractures => totalFractures;
        public double TotalTimeShardsEarned => totalTimeShardsEarned;
        public TimeFractureBonuses CurrentBonuses => currentBonuses;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            currentBonuses = new TimeFractureBonuses();
            RecalculateBonuses();
        }

        private void Start()
        {
            // Apply bonuses to game systems
            ApplyBonusesToGameSystems();
        }

        #region Public API

        /// <summary>
        /// Gets the money required to perform a Time Fracture.
        /// </summary>
        public double GetMoneyRequired()
        {
            return baseMoneyRequired * Math.Pow(moneyRequiredGrowth, fractureLevel);
        }

        /// <summary>
        /// Gets the dark matter required to perform a Time Fracture.
        /// </summary>
        public double GetDarkMatterRequired()
        {
            return baseDarkMatterRequired * (1 + fractureLevel * 0.5);
        }

        /// <summary>
        /// Checks if the player can perform a Time Fracture.
        /// </summary>
        public bool CanFracture()
        {
            if (CurrencyManager.Instance == null) return false;

            double moneyReq = GetMoneyRequired();
            double dmReq = GetDarkMatterRequired();

            return CurrencyManager.Instance.Money >= moneyReq &&
                   CurrencyManager.Instance.DarkMatter >= dmReq;
        }

        /// <summary>
        /// Calculates the Time Shards that would be earned from fracturing now.
        /// </summary>
        public double CalculatePotentialTimeShards()
        {
            if (CurrencyManager.Instance == null) return 0;

            // Base reward increases with level
            double baseReward = baseTimeShardsReward + (fractureLevel * timeShardsPerLevel);

            // Bonus from current Dark Matter
            double dmBonus = CurrencyManager.Instance.DarkMatter * timeShardsFromDMPercent;

            // Bonus from lifetime money (small percentage)
            double moneyBonus = CurrencyManager.Instance.LifetimeMoney * 0.0001;

            return Math.Floor(baseReward + dmBonus + moneyBonus);
        }

        /// <summary>
        /// Performs a Time Fracture, resetting progress and granting rewards.
        /// </summary>
        public bool DoTimeFracture()
        {
            if (!CanFracture())
            {
                if (debugMode) Debug.Log("[TimeFractureManager] Cannot fracture - requirements not met");
                return false;
            }

            // Calculate rewards before reset
            double timeShardsEarned = CalculatePotentialTimeShards();

            if (debugMode) Debug.Log($"[TimeFractureManager] Fracturing! Earning {timeShardsEarned} Time Shards");

            // Grant Time Shards
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddTimeShards(timeShardsEarned);
            }

            // Track stats
            totalTimeShardsEarned += timeShardsEarned;
            totalFractures++;
            fractureLevel++;

            // Notify mission system
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnTimeFracturePerformed();
            }

            // Perform the reset
            PerformFractureReset();

            // Recalculate and apply new bonuses
            RecalculateBonuses();
            ApplyBonusesToGameSystems();

            // Play effects
            PlayFractureEffects();

            // Notify listeners
            OnTimeFractureCompleted?.Invoke(fractureLevel);
            OnBonusesChanged?.Invoke(currentBonuses);

            return true;
        }

        /// <summary>
        /// Gets a description of current bonuses.
        /// </summary>
        public string GetBonusDescription()
        {
            if (fractureLevel == 0)
            {
                return "No bonuses yet.\nPerform your first Time Fracture!";
            }

            var lines = new List<string>();

            if (currentBonuses.moneyMultiplier > 1f)
                lines.Add($"+{(currentBonuses.moneyMultiplier - 1f) * 100:F0}% Money");

            if (currentBonuses.darkMatterMultiplier > 1f)
                lines.Add($"+{(currentBonuses.darkMatterMultiplier - 1f) * 100:F0}% Dark Matter");

            if (currentBonuses.diceValueBonus > 0)
                lines.Add($"+{currentBonuses.diceValueBonus} Dice Value");

            if (currentBonuses.startingMoney > 0)
                lines.Add($"+{currentBonuses.startingMoney:N0} Starting Money");

            if (currentBonuses.skillCostReduction > 0)
                lines.Add($"-{currentBonuses.skillCostReduction * 100:F0}% Skill Costs");

            return string.Join("\n", lines);
        }

        /// <summary>
        /// Gets the next level's bonus preview.
        /// </summary>
        public string GetNextBonusPreview()
        {
            int nextLevel = fractureLevel + 1;

            float nextMoneyMult = 1f + (nextLevel * moneyMultiplierPerLevel);
            float nextDMMult = 1f + (nextLevel * dmMultiplierPerLevel);
            int nextDiceBonus = nextLevel * diceValueBonusPerLevel;

            return $"Next Level ({nextLevel}):\n" +
                   $"+{(nextMoneyMult - 1f) * 100:F0}% Money\n" +
                   $"+{(nextDMMult - 1f) * 100:F0}% Dark Matter\n" +
                   $"+{nextDiceBonus} Dice Value";
        }

        #endregion

        #region Private Methods

        private void RecalculateBonuses()
        {
            currentBonuses = new TimeFractureBonuses
            {
                moneyMultiplier = 1f + (fractureLevel * moneyMultiplierPerLevel),
                darkMatterMultiplier = 1f + (fractureLevel * dmMultiplierPerLevel),
                diceValueBonus = fractureLevel * diceValueBonusPerLevel,
                startingMoney = fractureLevel * startingMoneyPerLevel,
                skillCostReduction = Mathf.Min(fractureLevel * skillCostReductionPerLevel, 0.5f) // Cap at 50%
            };

            if (debugMode) Debug.Log($"[TimeFractureManager] Bonuses recalculated: " +
                $"Money x{currentBonuses.moneyMultiplier:F2}, " +
                $"DM x{currentBonuses.darkMatterMultiplier:F2}, " +
                $"Dice +{currentBonuses.diceValueBonus}");
        }

        private void ApplyBonusesToGameSystems()
        {
            // Apply money multiplier to GameStats
            if (GameStats.Instance != null)
            {
                // The fracture bonus is applied as a global multiplier
                GameStats.Instance.SetFractureMoneyMultiplier(currentBonuses.moneyMultiplier);
                GameStats.Instance.SetFractureDMMultiplier(currentBonuses.darkMatterMultiplier);
                GameStats.Instance.SetFractureDiceValueBonus(currentBonuses.diceValueBonus);
            }

            if (debugMode) Debug.Log("[TimeFractureManager] Bonuses applied to game systems");
        }

        private void PerformFractureReset()
        {
            // Reset currencies (but not Time Shards)
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.ResetForTimeFracture();

                // Grant starting money bonus
                if (currentBonuses.startingMoney > 0)
                {
                    CurrencyManager.Instance.AddMoney(currentBonuses.startingMoney, false);
                }
            }

            // Remove all dice except starter
            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.ResetToSingleBasicDice();
            }

            // Reset skill tree (don't refund DM since we're resetting anyway)
            if (SkillTreeManager.Instance != null)
            {
                SkillTreeManager.Instance.ResetSkillTree(refundDarkMatter: false);
            }

            // Reset helper hands - this ensures they are removed even if skill removal didn't work properly
            if (HelperHandManager.Instance != null)
            {
                HelperHandManager.Instance.ResetForTimeFracture();
            }

            // Reset upgrade levels in GameStats
            if (GameStats.Instance != null)
            {
                GameStats.Instance.ResetUpgradeLevels();
            }

            if (debugMode) Debug.Log("[TimeFractureManager] Fracture reset complete");
        }

        private void PlayFractureEffects()
        {
            // Screen flash
            if (VisualEffectsManager.Instance != null)
            {
                VisualEffectsManager.Instance.FlashScreen(new Color(0.8f, 0.6f, 1f, 0.8f), 0.5f);
                VisualEffectsManager.Instance.ShakeCamera(0.5f, 0.3f);

                Camera cam = Camera.main;
                if (cam != null)
                {
                    VisualEffectsManager.Instance.SpawnPrestigeEffect(cam.transform.position);
                }
            }

            // Sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPrestigeSound();
            }
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// Gets save data for persistence.
        /// </summary>
        public TimeFractureSaveData GetSaveData()
        {
            return new TimeFractureSaveData
            {
                fractureLevel = fractureLevel,
                totalFractures = totalFractures,
                totalTimeShardsEarned = totalTimeShardsEarned
            };
        }

        /// <summary>
        /// Sets state from save data.
        /// </summary>
        public void SetSaveData(TimeFractureSaveData data)
        {
            if (data == null) return;

            fractureLevel = data.fractureLevel;
            totalFractures = data.totalFractures;
            totalTimeShardsEarned = data.totalTimeShardsEarned;

            RecalculateBonuses();
            ApplyBonusesToGameSystems();

            if (debugMode) Debug.Log($"[TimeFractureManager] Loaded: Level {fractureLevel}, Total {totalFractures}");
        }

        #endregion

        #region Debug

        [ContextMenu("Debug: Force Fracture")]
        public void DebugForceFracture()
        {
            // Give enough currency to fracture
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddMoney(GetMoneyRequired() * 2, false);
                CurrencyManager.Instance.AddDarkMatter(GetDarkMatterRequired() * 2);
            }
            DoTimeFracture();
        }

        #endregion
    }

    /// <summary>
    /// Save data for Time Fracture system.
    /// </summary>
    [Serializable]
    public class TimeFractureSaveData
    {
        public int fractureLevel;
        public int totalFractures;
        public double totalTimeShardsEarned;
    }
}
