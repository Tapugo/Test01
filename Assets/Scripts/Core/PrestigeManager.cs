using System;
using UnityEngine;
using Incredicer.Dice;
using Incredicer.Skills;

namespace Incredicer.Core
{
    /// <summary>
    /// Manages the prestige/ascension system.
    /// Players can reset progress to earn Dark Matter based on lifetime money earned.
    /// </summary>
    public class PrestigeManager : MonoBehaviour
    {
        public static PrestigeManager Instance { get; private set; }

        [Header("Prestige Settings")]
        [SerializeField] private double moneyRequiredForPrestige = 1000;
        [SerializeField] private double darkMatterPerPrestige = 1;
        [SerializeField] private double prestigeMoneyScaling = 10; // Each prestige requires 10x more money
        [SerializeField] private double darkMatterScalingFactor = 1.5; // Each prestige gives more DM

        [Header("State")]
        [SerializeField] private int prestigeLevel = 0;
        [SerializeField] private double totalDarkMatterEarned = 0;

        // Events
        public event Action<int> OnPrestigeCompleted;
        public event Action<double> OnPotentialDMChanged;

        // Properties
        public int PrestigeLevel => prestigeLevel;
        public double TotalDarkMatterEarned => totalDarkMatterEarned;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Subscribe to lifetime money changes to update potential DM
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnLifetimeMoneyChanged += OnLifetimeMoneyUpdated;
            }
        }

        private void OnDestroy()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnLifetimeMoneyChanged -= OnLifetimeMoneyUpdated;
            }
        }

        /// <summary>
        /// Gets the amount of lifetime money required to prestige.
        /// </summary>
        public double GetMoneyRequiredForPrestige()
        {
            return moneyRequiredForPrestige * Math.Pow(prestigeMoneyScaling, prestigeLevel);
        }

        /// <summary>
        /// Checks if the player can currently prestige.
        /// </summary>
        public bool CanPrestige()
        {
            if (CurrencyManager.Instance == null) return false;
            return CurrencyManager.Instance.LifetimeMoney >= GetMoneyRequiredForPrestige();
        }

        /// <summary>
        /// Calculates how much Dark Matter would be earned from prestiging now.
        /// </summary>
        public double CalculatePotentialDarkMatter()
        {
            if (CurrencyManager.Instance == null) return 0;

            double lifetimeMoney = CurrencyManager.Instance.LifetimeMoney;
            double required = GetMoneyRequiredForPrestige();

            if (lifetimeMoney < required) return 0;

            // Base DM scales with how much over the requirement you are
            double ratio = lifetimeMoney / required;
            double baseDM = darkMatterPerPrestige * Math.Log10(ratio + 1) * 10;

            // Apply prestige level bonus
            double levelBonus = Math.Pow(darkMatterScalingFactor, prestigeLevel);
            double totalDM = baseDM * levelBonus;

            // Apply dark matter gain multiplier from skills
            if (GameStats.Instance != null)
            {
                totalDM = GameStats.Instance.ApplyDarkMatterModifiers(totalDM);
            }

            return Math.Floor(totalDM);
        }

        /// <summary>
        /// Performs a prestige, resetting progress and awarding Dark Matter.
        /// </summary>
        public bool DoPrestige()
        {
            if (!CanPrestige())
            {
                Debug.Log("[PrestigeManager] Cannot prestige - requirements not met");
                return false;
            }

            double earnedDM = CalculatePotentialDarkMatter();
            if (earnedDM <= 0)
            {
                Debug.Log("[PrestigeManager] Cannot prestige - would earn 0 DM");
                return false;
            }

            Debug.Log($"[PrestigeManager] Prestiging! Earning {earnedDM} Dark Matter");

            // Award Dark Matter
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddDarkMatter(earnedDM);
            }

            // Track totals
            totalDarkMatterEarned += earnedDM;
            prestigeLevel++;

            // Reset progress
            ResetForPrestige();

            // Notify listeners
            OnPrestigeCompleted?.Invoke(prestigeLevel);

            // Unlock dark matter display if first prestige
            if (prestigeLevel == 1 && DiceManager.Instance != null)
            {
                DiceManager.Instance.DarkMatterUnlocked = true;
            }

            return true;
        }

        /// <summary>
        /// Resets game progress for prestige (keeps Dark Matter and skill tree).
        /// </summary>
        private void ResetForPrestige()
        {
            // Reset money (not dark matter)
            if (CurrencyManager.Instance != null)
            {
                // Keep dark matter, reset everything else
                double currentDM = CurrencyManager.Instance.DarkMatter;
                double lifetimeDM = CurrencyManager.Instance.LifetimeDarkMatter;
                CurrencyManager.Instance.SetCurrencies(0, currentDM, 0, lifetimeDM);
            }

            // Reset dice value upgrades (the money-bought ones)
            if (GameStats.Instance != null)
            {
                GameStats.Instance.DiceValueUpgradeLevel = 0;
            }

            // Destroy all extra dice (keep one basic)
            if (DiceManager.Instance != null)
            {
                var allDice = DiceManager.Instance.GetAllDice();
                bool keptOne = false;
                foreach (var dice in allDice)
                {
                    if (dice != null)
                    {
                        if (!keptOne && dice.Data != null && dice.Data.type == DiceType.Basic)
                        {
                            keptOne = true;
                            continue;
                        }
                        Destroy(dice.gameObject);
                    }
                }
            }

            // Note: Skill tree is NOT reset - that's the permanent progression

            Debug.Log("[PrestigeManager] Progress reset for prestige");
        }

        /// <summary>
        /// Called when lifetime money changes.
        /// </summary>
        private void OnLifetimeMoneyUpdated(double newLifetimeMoney)
        {
            double potentialDM = CalculatePotentialDarkMatter();
            OnPotentialDMChanged?.Invoke(potentialDM);
        }

        /// <summary>
        /// Sets prestige state (used for save/load).
        /// </summary>
        public void SetPrestigeState(int level, double totalDMEarned)
        {
            prestigeLevel = level;
            totalDarkMatterEarned = totalDMEarned;
        }

        /// <summary>
        /// Gets a description of what will happen on prestige.
        /// </summary>
        public string GetPrestigeDescription()
        {
            double required = GetMoneyRequiredForPrestige();
            double potential = CalculatePotentialDarkMatter();
            bool canPrestige = CanPrestige();

            if (!canPrestige)
            {
                return $"Earn ${UI.GameUI.FormatNumber(required)} lifetime money to unlock Ascension.";
            }
            else
            {
                return $"Ascend now to earn {UI.GameUI.FormatNumber(potential)} Dark Matter!\n" +
                       $"Your money and dice will reset, but skills remain.";
            }
        }
    }
}
