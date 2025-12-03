using System.Collections;
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

        [Header("Table Tax / Tip Jar")]
        [SerializeField] private float tableTaxChance = 0f; // 0-1, chance per roll for bonus coin
        [SerializeField] private float tipJarScaling = 0f; // 0-1, if > 0, bonus coin is this % of current money instead of flat +50

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
        [SerializeField] private bool focusedGravityActive = false;
        [SerializeField] private bool precisionAimActive = false;
        [SerializeField] private bool hyperburstActive = false;

        [Header("Cursor/Rolling")]
        [SerializeField] private float cursorRollRadius = 1.0f;

        [Header("Dice Value Upgrades")]
        [SerializeField] private int diceValueUpgradeLevel = 0;

        [Header("Temporary Boosts")]
        [SerializeField] private float temporaryMoneyBoost = 0f;
        [SerializeField] private float temporaryDMBoost = 0f;

        [Header("Time Fracture Bonuses")]
        [SerializeField] private float fractureMoneyMultiplier = 1f;
        [SerializeField] private float fractureDMMultiplier = 1f;
        [SerializeField] private int fractureDiceValueBonus = 0;

        [Header("Base Stats")]
        [SerializeField] private double baseMoneyPerRoll = 10;

        // Properties for external access
        public double BaseMoneyPerRoll { get => baseMoneyPerRoll; set => baseMoneyPerRoll = value; }
        public float TemporaryMoneyBoost => temporaryMoneyBoost;
        public float TemporaryDMBoost => temporaryDMBoost;
        public int DiceValueUpgradeLevel { get => diceValueUpgradeLevel; set => diceValueUpgradeLevel = value; }
        public double GlobalMoneyMultiplier { get => globalMoneyMultiplier; set => globalMoneyMultiplier = value; }
        public double ManualMoneyMultiplier { get => manualMoneyMultiplier; set => manualMoneyMultiplier = value; }
        public double IdleMoneyMultiplier { get => idleMoneyMultiplier; set => idleMoneyMultiplier = value; }
        public float JackpotChance { get => jackpotChance; set => jackpotChance = Mathf.Clamp01(value); }
        public double JackpotMultiplier { get => jackpotMultiplier; set => jackpotMultiplier = value; }
        public float TableTaxChance { get => tableTaxChance; set => tableTaxChance = Mathf.Clamp01(value); }
        public float TipJarScaling { get => tipJarScaling; set => tipJarScaling = Mathf.Clamp01(value); }
        public double DarkMatterGainMultiplier { get => darkMatterGainMultiplier; set => darkMatterGainMultiplier = value; }
        public double HelperHandSpeedMultiplier { get => helperHandSpeedMultiplier; set => helperHandSpeedMultiplier = value; }
        public int HelperHandExtraRolls { get => helperHandExtraRolls; set => helperHandExtraRolls = value; }
        public double SkillCooldownMultiplier { get => skillCooldownMultiplier; set => skillCooldownMultiplier = value; }
        public double ActiveSkillDurationMultiplier { get => activeSkillDurationMultiplier; set => activeSkillDurationMultiplier = value; }
        public bool IdleKingActive { get => idleKingActive; set => idleKingActive = value; }
        public bool TimeDilationActive { get => timeDilationActive; set => timeDilationActive = value; }
        public bool FocusedGravityActive { get => focusedGravityActive; set => focusedGravityActive = value; }
        public bool PrecisionAimActive { get => precisionAimActive; set => precisionAimActive = value; }
        public bool HyperburstActive { get => hyperburstActive; set => hyperburstActive = value; }
        public float CursorRollRadius { get => cursorRollRadius; set => cursorRollRadius = value; }
        public float FractureMoneyMultiplier => fractureMoneyMultiplier;
        public float FractureDMMultiplier => fractureDMMultiplier;
        public int FractureDiceValueBonus => fractureDiceValueBonus;

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

            // Hyperburst: double all money during the effect
            if (hyperburstActive)
            {
                amount *= 2.0;
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

            // Hyperburst: double all DM during the effect
            if (hyperburstActive)
            {
                amount *= 2.0;
            }

            return amount;
        }

        /// <summary>
        /// Checks if Table Tax procs and returns the bonus coin amount.
        /// Returns 0 if no proc, otherwise returns the bonus amount.
        /// </summary>
        /// <param name="currentMoney">Current money (used for Tip Jar scaling).</param>
        /// <returns>Bonus coin amount, or 0 if no proc.</returns>
        public double CheckTableTaxProc(double currentMoney)
        {
            if (tableTaxChance <= 0) return 0;

            if (Random.value < tableTaxChance)
            {
                // Table Tax proc'd!
                if (tipJarScaling > 0)
                {
                    // Tip Jar: bonus is percentage of current money
                    return currentMoney * tipJarScaling;
                }
                else
                {
                    // Base Table Tax: flat +50 bonus
                    return 50.0;
                }
            }

            return 0;
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
            tableTaxChance = 0f;
            tipJarScaling = 0f;
            darkMatterGainMultiplier = 1.0;
            helperHandSpeedMultiplier = 1.0;
            helperHandExtraRolls = 0;
            skillCooldownMultiplier = 1.0;
            activeSkillDurationMultiplier = 1.0;
            idleKingActive = false;
            timeDilationActive = false;
            focusedGravityActive = false;
            precisionAimActive = false;
            hyperburstActive = false;
            cursorRollRadius = 1.0f;
            temporaryMoneyBoost = 0f;
            temporaryDMBoost = 0f;
        }

        /// <summary>
        /// Applies a temporary money boost for a duration.
        /// </summary>
        /// <param name="boostPercent">Boost amount as decimal (0.5 = 50% boost)</param>
        /// <param name="durationSeconds">Duration in seconds</param>
        public void ApplyTemporaryMoneyBoost(float boostPercent, float durationSeconds)
        {
            StartCoroutine(TemporaryMoneyBoostCoroutine(boostPercent, durationSeconds));
        }

        private IEnumerator TemporaryMoneyBoostCoroutine(float boostPercent, float durationSeconds)
        {
            temporaryMoneyBoost += boostPercent;
            globalMoneyMultiplier *= (1f + boostPercent);
            Debug.Log($"[GameStats] Money boost started: +{boostPercent * 100}% for {durationSeconds}s");

            yield return new WaitForSeconds(durationSeconds);

            temporaryMoneyBoost -= boostPercent;
            globalMoneyMultiplier /= (1f + boostPercent);
            Debug.Log($"[GameStats] Money boost ended");
        }

        /// <summary>
        /// Applies a temporary dark matter boost for a duration.
        /// </summary>
        /// <param name="boostPercent">Boost amount as decimal (1.0 = 100% boost)</param>
        /// <param name="durationSeconds">Duration in seconds</param>
        public void ApplyTemporaryDMBoost(float boostPercent, float durationSeconds)
        {
            StartCoroutine(TemporaryDMBoostCoroutine(boostPercent, durationSeconds));
        }

        private IEnumerator TemporaryDMBoostCoroutine(float boostPercent, float durationSeconds)
        {
            temporaryDMBoost += boostPercent;
            darkMatterGainMultiplier *= (1f + boostPercent);
            Debug.Log($"[GameStats] DM boost started: +{boostPercent * 100}% for {durationSeconds}s");

            yield return new WaitForSeconds(durationSeconds);

            temporaryDMBoost -= boostPercent;
            darkMatterGainMultiplier /= (1f + boostPercent);
            Debug.Log($"[GameStats] DM boost ended");
        }

        #region Time Fracture

        /// <summary>
        /// Sets the fracture money multiplier bonus.
        /// </summary>
        public void SetFractureMoneyMultiplier(float multiplier)
        {
            fractureMoneyMultiplier = multiplier;
            // Apply to global multiplier
            globalMoneyMultiplier = multiplier;
        }

        /// <summary>
        /// Sets the fracture dark matter multiplier bonus.
        /// </summary>
        public void SetFractureDMMultiplier(float multiplier)
        {
            fractureDMMultiplier = multiplier;
            darkMatterGainMultiplier = multiplier;
        }

        /// <summary>
        /// Sets the fracture dice value bonus.
        /// </summary>
        public void SetFractureDiceValueBonus(int bonus)
        {
            fractureDiceValueBonus = bonus;
        }

        /// <summary>
        /// Resets upgrade levels (for Time Fracture reset).
        /// </summary>
        public void ResetUpgradeLevels()
        {
            diceValueUpgradeLevel = 0;
        }

        #endregion
    }
}
