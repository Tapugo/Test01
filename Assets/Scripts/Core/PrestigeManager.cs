using System;
using UnityEngine;
using Incredicer.Dice;
using Incredicer.Skills;

namespace Incredicer.Core
{
    /// <summary>
    /// Manages the ascension system.
    /// Players spend $1000 to ascend, which unlocks Dark Matter generation.
    /// Dark Matter is earned per dice roll: +1 DM for rolling 1, +2 for rolling 2, etc.
    /// </summary>
    public class PrestigeManager : MonoBehaviour
    {
        public static PrestigeManager Instance { get; private set; }

        [Header("Ascension Settings")]
        [SerializeField] private double ascensionCost = 1000;

        [Header("State")]
        [SerializeField] private int ascensionLevel = 0;
        [SerializeField] private double totalDarkMatterEarned = 0;
        [SerializeField] private bool hasAscended = false;

        // Events
        public event Action<int> OnAscensionCompleted;
        public event Action OnDarkMatterUnlocked;

        // Properties
        public int AscensionLevel => ascensionLevel;
        public double TotalDarkMatterEarned => totalDarkMatterEarned;
        public bool HasAscended => hasAscended;
        public double AscensionCost => ascensionCost;

        // Legacy compatibility
        public int PrestigeLevel => ascensionLevel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Gets the cost to ascend (always $1000).
        /// </summary>
        public double GetMoneyRequiredForPrestige()
        {
            return ascensionCost;
        }

        /// <summary>
        /// Checks if the player can currently ascend.
        /// </summary>
        public bool CanPrestige()
        {
            if (hasAscended) return false; // Can only ascend once
            if (CurrencyManager.Instance == null) return false;
            return CurrencyManager.Instance.Money >= ascensionCost;
        }

        /// <summary>
        /// Returns 0 since we don't give DM directly from ascending.
        /// DM is earned through dice rolls after ascending.
        /// </summary>
        public double CalculatePotentialDarkMatter()
        {
            return 0; // DM is earned through rolls, not from ascending
        }

        /// <summary>
        /// Performs an ascension, spending money to unlock Dark Matter generation.
        /// </summary>
        public bool DoPrestige()
        {
            if (!CanPrestige())
            {
                Debug.Log("[PrestigeManager] Cannot ascend - requirements not met or already ascended");
                return false;
            }

            // Spend the money
            if (!CurrencyManager.Instance.SpendMoney(ascensionCost))
            {
                Debug.Log("[PrestigeManager] Cannot ascend - couldn't spend money");
                return false;
            }

            Debug.Log($"[PrestigeManager] Ascending! Unlocking Dark Matter generation.");

            hasAscended = true;
            ascensionLevel = 1;

            // Unlock dark matter display and generation
            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.DarkMatterUnlocked = true;
            }

            // Play prestige sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPrestigeSound();
            }

            // Spawn prestige particle effect at screen center
            if (VisualEffectsManager.Instance != null)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    VisualEffectsManager.Instance.SpawnPrestigeEffect(cam.transform.position);
                }
            }

            // Notify listeners
            OnAscensionCompleted?.Invoke(ascensionLevel);
            OnDarkMatterUnlocked?.Invoke();

            return true;
        }

        /// <summary>
        /// Tracks dark matter earned (for statistics).
        /// </summary>
        public void TrackDarkMatterEarned(double amount)
        {
            totalDarkMatterEarned += amount;
        }

        /// <summary>
        /// Sets ascension state (used for save/load).
        /// </summary>
        public void SetPrestigeState(int level, double totalDMEarned)
        {
            ascensionLevel = level;
            totalDarkMatterEarned = totalDMEarned;
            hasAscended = level > 0;
        }

        /// <summary>
        /// Gets a description of what will happen on ascension.
        /// </summary>
        public string GetPrestigeDescription()
        {
            if (hasAscended)
            {
                return "You have ascended!\nDark Matter is now earned from dice rolls.";
            }
            else if (CanPrestige())
            {
                return $"Ascend for ${UI.GameUI.FormatNumber(ascensionCost)} to unlock Dark Matter!\n" +
                       $"Earn DM from dice rolls (+1 for 1, +2 for 2, etc.)";
            }
            else
            {
                return $"Save ${UI.GameUI.FormatNumber(ascensionCost)} to unlock Ascension.";
            }
        }
    }
}
