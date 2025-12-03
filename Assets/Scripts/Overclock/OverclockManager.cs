using System;
using System.Collections.Generic;
using UnityEngine;
using Incredicer.Core;
using Incredicer.Dice;
using Incredicer.Missions;

namespace Incredicer.Overclock
{
    /// <summary>
    /// Configuration for overclock mechanics.
    /// </summary>
    [Serializable]
    public class OverclockConfig
    {
        [Header("Base Settings")]
        [Tooltip("Multiplier applied to dice payout when overclocked")]
        public float payoutMultiplier = 2.5f;

        [Tooltip("Heat gained per roll (0-1 scale, 1 = destroyed)")]
        public float heatPerRoll = 0.1f;

        [Tooltip("Dark Matter bonus when dice is destroyed")]
        public double dmBonusOnDestroy = 10;

        [Header("Visual")]
        [Tooltip("Color tint when dice is overclocked")]
        public Color overclockTint = new Color(1f, 0.5f, 0.2f, 1f);

        [Tooltip("Pulse speed when overclocked")]
        public float pulseSpeed = 2f;

        [Tooltip("Pulse intensity (scale variance)")]
        public float pulseIntensity = 0.1f;
    }

    /// <summary>
    /// Runtime state for an overclocked dice.
    /// </summary>
    [Serializable]
    public class OverclockedDiceState
    {
        public string diceInstanceId;
        public float currentHeat; // 0 to 1
        public int rollsSinceOverclock;
        public double totalBonusEarned;

        public bool IsAboutToExplode => currentHeat >= 0.9f;
        public bool ShouldExplode => currentHeat >= 1f;
    }

    /// <summary>
    /// Manages the overclock/destruction system for dice.
    /// </summary>
    public class OverclockManager : MonoBehaviour
    {
        public static OverclockManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private OverclockConfig config = new OverclockConfig();

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        // Track overclocked dice
        private Dictionary<Dice.Dice, OverclockedDiceState> overclockedDice = new Dictionary<Dice.Dice, OverclockedDiceState>();

        // Stats
        private int totalDiceDestroyed = 0;
        private double totalDMFromDestruction = 0;

        // Events
        public event Action<Dice.Dice> OnDiceOverclocked;
        public event Action<Dice.Dice, float> OnHeatChanged; // dice, newHeat
        public event Action<Dice.Dice, double> OnDiceDestroyed; // dice, dmEarned

        // Properties
        public OverclockConfig Config => config;
        public int TotalDiceDestroyed => totalDiceDestroyed;
        public double TotalDMFromDestruction => totalDMFromDestruction;

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
            // Subscribe to dice roll events
            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.OnDiceSpawned += OnDiceSpawned;

                // Subscribe to existing dice
                foreach (var dice in DiceManager.Instance.GetAllDice())
                {
                    SubscribeToDice(dice);
                }
            }
        }

        private void OnDestroy()
        {
            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.OnDiceSpawned -= OnDiceSpawned;
            }

            // Unsubscribe from all dice
            foreach (var dice in new List<Dice.Dice>(overclockedDice.Keys))
            {
                UnsubscribeFromDice(dice);
            }
        }

        private void Update()
        {
            // Update visual effects for overclocked dice
            foreach (var kvp in overclockedDice)
            {
                if (kvp.Key != null)
                {
                    UpdateOverclockVisuals(kvp.Key, kvp.Value);
                }
            }
        }

        #region Public API

        /// <summary>
        /// Checks if a dice can be overclocked.
        /// </summary>
        public bool CanOverclock(Dice.Dice dice)
        {
            if (dice == null) return false;
            if (IsOverclocked(dice)) return false;

            // Could add additional requirements here (e.g., minimum dice tier)
            return true;
        }

        /// <summary>
        /// Checks if a dice is currently overclocked.
        /// </summary>
        public bool IsOverclocked(Dice.Dice dice)
        {
            return dice != null && overclockedDice.ContainsKey(dice);
        }

        /// <summary>
        /// Gets the overclock state for a dice, or null if not overclocked.
        /// </summary>
        public OverclockedDiceState GetOverclockState(Dice.Dice dice)
        {
            if (dice != null && overclockedDice.TryGetValue(dice, out var state))
            {
                return state;
            }
            return null;
        }

        /// <summary>
        /// Starts overclocking a dice.
        /// </summary>
        public bool StartOverclock(Dice.Dice dice)
        {
            if (!CanOverclock(dice))
            {
                if (debugMode) Debug.Log($"[OverclockManager] Cannot overclock dice: {dice?.name}");
                return false;
            }

            var state = new OverclockedDiceState
            {
                diceInstanceId = dice.GetInstanceID().ToString(),
                currentHeat = 0f,
                rollsSinceOverclock = 0,
                totalBonusEarned = 0
            };

            overclockedDice[dice] = state;

            // Subscribe to this dice's roll events
            SubscribeToDice(dice);

            // Apply visual effect
            ApplyOverclockVisuals(dice);

            if (debugMode) Debug.Log($"[OverclockManager] Dice overclocked: {dice.name}");

            OnDiceOverclocked?.Invoke(dice);

            // Play overclock start effect
            if (VisualEffectsManager.Instance != null)
            {
                VisualEffectsManager.Instance.SpawnSparkleEffect(dice.transform.position, config.overclockTint);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClickSound();
            }

            return true;
        }

        /// <summary>
        /// Gets the payout multiplier for a dice (1.0 if not overclocked).
        /// </summary>
        public float GetPayoutMultiplier(Dice.Dice dice)
        {
            if (IsOverclocked(dice))
            {
                return config.payoutMultiplier;
            }
            return 1f;
        }

        /// <summary>
        /// Gets all currently overclocked dice.
        /// </summary>
        public IEnumerable<Dice.Dice> GetOverclockedDice()
        {
            return overclockedDice.Keys;
        }

        /// <summary>
        /// Gets the count of overclocked dice.
        /// </summary>
        public int GetOverclockedCount()
        {
            return overclockedDice.Count;
        }

        #endregion

        #region Event Handlers

        private void OnDiceSpawned(Dice.Dice dice)
        {
            // New dice are not overclocked by default
            // Just set up the visual reference if needed
        }

        private void SubscribeToDice(Dice.Dice dice)
        {
            if (dice != null)
            {
                dice.OnRolled += OnDiceRolled;
            }
        }

        private void UnsubscribeFromDice(Dice.Dice dice)
        {
            if (dice != null)
            {
                dice.OnRolled -= OnDiceRolled;
            }
        }

        private void OnDiceRolled(Dice.Dice dice, double moneyEarned, bool isJackpot)
        {
            if (!IsOverclocked(dice)) return;

            var state = overclockedDice[dice];

            // Add heat
            state.currentHeat += config.heatPerRoll;
            state.rollsSinceOverclock++;

            // Track bonus earnings (the multiplier portion)
            double bonus = moneyEarned * (config.payoutMultiplier - 1f);
            state.totalBonusEarned += bonus;

            if (debugMode) Debug.Log($"[OverclockManager] Dice rolled. Heat: {state.currentHeat:F2}, Rolls: {state.rollsSinceOverclock}");

            // Notify mission system
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnOverclockedRoll();
            }

            OnHeatChanged?.Invoke(dice, state.currentHeat);

            // Check for destruction
            if (state.ShouldExplode)
            {
                DestroyOverclockedDice(dice, state);
            }
        }

        #endregion

        #region Destruction

        private void DestroyOverclockedDice(Dice.Dice dice, OverclockedDiceState state)
        {
            if (dice == null) return;

            // Calculate DM reward
            double dmReward = config.dmBonusOnDestroy;

            // Bonus based on dice tier
            if (dice.Data != null)
            {
                dmReward *= (1 + (int)dice.Data.type * 0.5); // Higher tier = more DM
            }

            // Grant DM reward
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddDarkMatter(dmReward);
            }

            // Update stats
            totalDiceDestroyed++;
            totalDMFromDestruction += dmReward;

            if (debugMode) Debug.Log($"[OverclockManager] Dice destroyed! DM earned: {dmReward}, Total destroyed: {totalDiceDestroyed}");

            // Notify mission system
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnDiceDestroyedByOverclock();
            }

            // Play destruction effects
            PlayDestructionEffects(dice, dmReward);

            // Fire event before removal
            OnDiceDestroyed?.Invoke(dice, dmReward);

            // Clean up
            UnsubscribeFromDice(dice);
            overclockedDice.Remove(dice);

            // Remove from DiceManager
            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.RemoveDice(dice);
            }
        }

        private void PlayDestructionEffects(Dice.Dice dice, double dmReward)
        {
            Vector3 pos = dice.transform.position;

            // Explosion effect
            if (VisualEffectsManager.Instance != null)
            {
                // Fire burst / prestige-style effect
                VisualEffectsManager.Instance.SpawnPrestigeEffect(pos);

                // Dark matter particles
                VisualEffectsManager.Instance.SpawnDarkMatterEffect(pos);

                // Screen shake
                VisualEffectsManager.Instance.ShakeCamera(0.3f, 0.15f);
            }

            // Sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayJackpotSound(); // Reuse jackpot for destruction
            }
        }

        #endregion

        #region Visuals

        private void ApplyOverclockVisuals(Dice.Dice dice)
        {
            // Initial tint is handled in Update loop
        }

        private void UpdateOverclockVisuals(Dice.Dice dice, OverclockedDiceState state)
        {
            if (dice == null) return;

            var spriteRenderer = dice.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;

            // Pulse effect based on heat
            float pulse = 1f + Mathf.Sin(Time.time * config.pulseSpeed * (1f + state.currentHeat)) * config.pulseIntensity * (1f + state.currentHeat);

            // Don't override dice scale, just apply color
            // Color intensifies with heat
            float heatIntensity = Mathf.Lerp(0.3f, 1f, state.currentHeat);
            Color tint = Color.Lerp(Color.white, config.overclockTint, heatIntensity);

            // Flash red when about to explode
            if (state.IsAboutToExplode)
            {
                float flash = Mathf.PingPong(Time.time * 8f, 1f);
                tint = Color.Lerp(tint, Color.red, flash * 0.5f);
            }

            spriteRenderer.color = tint;
        }

        private void ResetDiceVisuals(Dice.Dice dice)
        {
            if (dice == null) return;

            var spriteRenderer = dice.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// Gets save data for persistence.
        /// </summary>
        public OverclockSaveData GetSaveData()
        {
            var data = new OverclockSaveData
            {
                totalDiceDestroyed = totalDiceDestroyed,
                totalDMFromDestruction = totalDMFromDestruction,
                overclockedStates = new List<OverclockedDiceState>()
            };

            // Note: We don't save individual overclocked dice states because
            // dice instances change between sessions. The overclock is temporary.

            return data;
        }

        /// <summary>
        /// Sets state from save data.
        /// </summary>
        public void SetSaveData(OverclockSaveData data)
        {
            if (data == null) return;

            totalDiceDestroyed = data.totalDiceDestroyed;
            totalDMFromDestruction = data.totalDMFromDestruction;

            if (debugMode) Debug.Log($"[OverclockManager] Loaded stats: {totalDiceDestroyed} destroyed, {totalDMFromDestruction} DM earned");
        }

        #endregion
    }

    /// <summary>
    /// Save data for overclock system.
    /// </summary>
    [Serializable]
    public class OverclockSaveData
    {
        public int totalDiceDestroyed;
        public double totalDMFromDestruction;
        public List<OverclockedDiceState> overclockedStates;
    }
}
