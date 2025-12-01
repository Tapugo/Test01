using UnityEngine;

namespace Incredicer.Core
{
    /// <summary>
    /// Central manager for all global game multipliers and stat modifiers.
    /// Accessed as a singleton via GameStats.Instance.
    /// </summary>
    public class GameStats : MonoBehaviour
    {
        public static GameStats Instance { get; private set; }

        [Header("Money Multipliers")]
        [SerializeField] private double globalMoneyMultiplier = 1.0;
        [SerializeField] private double manualMoneyMultiplier = 1.0;
        [SerializeField] private double idleMoneyMultiplier = 1.0;

        [Header("Jackpot")]
        [SerializeField] private float jackpotChance = 0f; // 0-1
        [SerializeField] private double jackpotMultiplier = 2.0;

        [Header("Dark Matter")]
        [SerializeField] private double darkMatterGainMultiplier = 1.0;

        [Header("Helper Hands")]
        [SerializeField] private double helperHandSpeedMultiplier = 1.0;
        [SerializeField] private int helperHandExtraRolls = 0;

        [Header("Skills")]
        [SerializeField] private double skillCooldownMultiplier = 1.0;
        [SerializeField] private double activeSkillDurationMultiplier = 1.0;

        [Header("Special Flags")]
        [SerializeField] private bool idleKingActive = false;
        [SerializeField] private bool timeDilationActive = false;

        [Header("Cursor/Rolling")]
        [SerializeField] private float cursorRollRadius = 1.0f;

        [Header("Dice Value Upgrades")]
        [SerializeField] private int diceValueUpgradeLevel = 0;

        // Properties for external access
        public int DiceValueUpgradeLevel { get => diceValueUpgradeLevel; set => diceValueUpgradeLevel = value; }
        public double GlobalMoneyMultiplier { get => globalMoneyMultiplier; set => globalMoneyMultiplier = value; }
        public double ManualMoneyMultiplier { get => manualMoneyMultiplier; set => manualMoneyMultiplier = value; }
        public double IdleMoneyMultiplier { get => idleMoneyMultiplier; set => idleMoneyMultiplier = value; }
        public float JackpotChance { get => jackpotChance; set => jackpotChance = Mathf.Clamp01(value); }
        public double JackpotMultiplier { get => jackpotMultiplier; set => jackpotMultiplier = value; }
        public double DarkMatterGainMultiplier { get => darkMatterGainMultiplier; set => darkMatterGainMultiplier = value; }
        public double HelperHandSpeedMultiplier { get => helperHandSpeedMultiplier; set => helperHandSpeedMultiplier = value; }
        public int HelperHandExtraRolls { get => helperHandExtraRolls; set => helperHandExtraRolls = value; }
        public double SkillCooldownMultiplier { get => skillCooldownMultiplier; set => skillCooldownMultiplier = value; }
        public double ActiveSkillDurationMultiplier { get => activeSkillDurationMultiplier; set => activeSkillDurationMultiplier = value; }
        public bool IdleKingActive { get => idleKingActive; set => idleKingActive = value; }
        public bool TimeDilationActive { get => timeDilationActive; set => timeDilationActive = value; }
        public float CursorRollRadius { get => cursorRollRadius; set => cursorRollRadius = value; }

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

        /// <summary>
        /// Applies all money modifiers to a base amount.
        /// </summary>
        /// <param name="baseAmount">The base money amount before modifiers.</param>
        /// <param name="isManual">True if this is from manual player rolling.</param>
        /// <param name="isIdle">True if this is from helper hands/automation.</param>
        /// <returns>The final modified money amount.</returns>
        public double ApplyMoneyModifiers(double baseAmount, bool isManual, bool isIdle)
        {
            double amount = baseAmount * globalMoneyMultiplier;

            if (isManual)
            {
                amount *= manualMoneyMultiplier;
            }

            if (isIdle)
            {
                amount *= idleMoneyMultiplier;
                
                // Idle King: helper rolls don't get extra money (but do get extra DM)
                // This is handled by not applying extra money bonus here
            }

            // Apply jackpot chance
            if (Random.value < jackpotChance)
            {
                amount *= jackpotMultiplier;
            }

            return amount;
        }

        /// <summary>
        /// Applies all dark matter modifiers to a base amount.
        /// </summary>
        /// <param name="baseAmount">The base dark matter amount before modifiers.</param>
        /// <returns>The final modified dark matter amount.</returns>
        public double ApplyDarkMatterModifiers(double baseAmount)
        {
            double amount = baseAmount * darkMatterGainMultiplier;

            // Time Dilation: double DM while active skills are running
            if (timeDilationActive)
            {
                amount *= 2.0;
            }

            return amount;
        }

        /// <summary>
        /// Resets all stats to default values.
        /// </summary>
        public void ResetToDefaults()
        {
            globalMoneyMultiplier = 1.0;
            manualMoneyMultiplier = 1.0;
            idleMoneyMultiplier = 1.0;
            jackpotChance = 0f;
            jackpotMultiplier = 2.0;
            darkMatterGainMultiplier = 1.0;
            helperHandSpeedMultiplier = 1.0;
            helperHandExtraRolls = 0;
            skillCooldownMultiplier = 1.0;
            activeSkillDurationMultiplier = 1.0;
            idleKingActive = false;
            timeDilationActive = false;
            cursorRollRadius = 1.0f;
        }
    }
}
