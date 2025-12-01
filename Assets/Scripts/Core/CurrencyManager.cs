using System;
using UnityEngine;
using Incredicer.UI;

namespace Incredicer.Core
{
    /// <summary>
    /// Manages all currency (Money and Dark Matter) for the game.
    /// Accessed as a singleton via CurrencyManager.Instance.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        [Header("Current Currencies")]
        [SerializeField] private double money = 0;
        [SerializeField] private double darkMatter = 0;

        [Header("Lifetime Stats")]
        [SerializeField] private double lifetimeMoney = 0;
        [SerializeField] private double lifetimeDarkMatter = 0;

        // Events for UI updates
        public event Action<double> OnMoneyChanged;
        public event Action<double> OnDarkMatterChanged;
        public event Action<double> OnLifetimeMoneyChanged;
        public event Action<double> OnLifetimeDarkMatterChanged;

        // Properties
        public double Money => money;
        public double DarkMatter => darkMatter;
        public double LifetimeMoney => lifetimeMoney;
        public double LifetimeDarkMatter => lifetimeDarkMatter;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Always start fresh with 0 currency - SaveSystem will load saved values if they exist
            money = 0;
            darkMatter = 0;
            lifetimeMoney = 0;
            lifetimeDarkMatter = 0;
        }

        /// <summary>
        /// Adds money with floating effect from a world position.
        /// The money is not added immediately - it's added when the effect reaches the counter.
        /// </summary>
        /// <param name="amount">Amount to add.</param>
        /// <param name="worldPosition">World position to spawn the effect from.</param>
        /// <param name="isJackpot">Whether this was a jackpot roll (affects effect color).</param>
        public void AddMoneyWithEffect(double amount, Vector3 worldPosition, bool isJackpot = false)
        {
            if (amount <= 0) return;

            // Track lifetime immediately for progression checks
            lifetimeMoney += amount;
            OnLifetimeMoneyChanged?.Invoke(lifetimeMoney);

            // Spawn the floating effect - money will be added when it reaches the counter
            if (FloatingCurrencyEffect.Instance != null)
            {
                FloatingCurrencyEffect.Instance.SpawnMoneyEffect(worldPosition, amount, isJackpot);
            }
            else
            {
                // Fallback: add directly if no effect system
                AddMoneyDirect(amount);
            }
        }

        /// <summary>
        /// Adds money directly to the player's balance (used when effect reaches counter or for non-visual adds).
        /// </summary>
        /// <param name="amount">Amount to add.</param>
        public void AddMoneyDirect(double amount)
        {
            if (amount <= 0) return;

            money += amount;
            OnMoneyChanged?.Invoke(money);
        }

        /// <summary>
        /// Adds money to the player's balance (legacy method for compatibility).
        /// </summary>
        /// <param name="amount">Amount to add.</param>
        /// <param name="fromRoll">True if this money came from a dice roll (counts toward lifetime).</param>
        public void AddMoney(double amount, bool fromRoll = true)
        {
            if (amount <= 0) return;

            money += amount;

            if (fromRoll)
            {
                lifetimeMoney += amount;
                OnLifetimeMoneyChanged?.Invoke(lifetimeMoney);
            }

            OnMoneyChanged?.Invoke(money);
        }

        /// <summary>
        /// Attempts to spend money.
        /// </summary>
        /// <param name="amount">Amount to spend.</param>
        /// <returns>True if successful, false if insufficient funds.</returns>
        public bool SpendMoney(double amount)
        {
            if (amount <= 0) return true;
            if (money < amount) return false;

            money -= amount;
            OnMoneyChanged?.Invoke(money);
            return true;
        }

        /// <summary>
        /// Checks if the player can afford a purchase.
        /// </summary>
        /// <param name="amount">Amount to check.</param>
        /// <returns>True if player has enough money.</returns>
        public bool CanAffordMoney(double amount)
        {
            return money >= amount;
        }

        /// <summary>
        /// Adds dark matter with floating effect from a world position.
        /// The dark matter is not added immediately - it's added when the effect reaches the counter.
        /// </summary>
        /// <param name="amount">Amount to add.</param>
        /// <param name="worldPosition">World position to spawn the effect from.</param>
        public void AddDarkMatterWithEffect(double amount, Vector3 worldPosition)
        {
            if (amount <= 0) return;

            // Track lifetime immediately
            lifetimeDarkMatter += amount;
            OnLifetimeDarkMatterChanged?.Invoke(lifetimeDarkMatter);

            // Spawn the floating effect - DM will be added when it reaches the counter
            if (FloatingCurrencyEffect.Instance != null)
            {
                FloatingCurrencyEffect.Instance.SpawnDarkMatterEffect(worldPosition, amount);
            }
            else
            {
                // Fallback: add directly if no effect system
                AddDarkMatterDirect(amount);
            }
        }

        /// <summary>
        /// Adds dark matter directly to the player's balance (used when effect reaches counter).
        /// </summary>
        /// <param name="amount">Amount to add.</param>
        public void AddDarkMatterDirect(double amount)
        {
            if (amount <= 0) return;

            darkMatter += amount;
            OnDarkMatterChanged?.Invoke(darkMatter);
        }

        /// <summary>
        /// Adds dark matter to the player's balance (legacy method for compatibility).
        /// </summary>
        /// <param name="amount">Amount to add.</param>
        public void AddDarkMatter(double amount)
        {
            if (amount <= 0) return;

            darkMatter += amount;
            lifetimeDarkMatter += amount;

            OnDarkMatterChanged?.Invoke(darkMatter);
            OnLifetimeDarkMatterChanged?.Invoke(lifetimeDarkMatter);
        }

        /// <summary>
        /// Attempts to spend dark matter.
        /// </summary>
        /// <param name="amount">Amount to spend.</param>
        /// <returns>True if successful, false if insufficient dark matter.</returns>
        public bool SpendDarkMatter(double amount)
        {
            if (amount <= 0) return true;
            if (darkMatter < amount) return false;

            darkMatter -= amount;
            OnDarkMatterChanged?.Invoke(darkMatter);
            return true;
        }

        /// <summary>
        /// Checks if the player can afford a dark matter purchase.
        /// </summary>
        /// <param name="amount">Amount to check.</param>
        /// <returns>True if player has enough dark matter.</returns>
        public bool CanAffordDarkMatter(double amount)
        {
            return darkMatter >= amount;
        }

        /// <summary>
        /// Sets currency values directly (used for save/load).
        /// </summary>
        public void SetCurrencies(double newMoney, double newDarkMatter, double newLifetimeMoney, double newLifetimeDarkMatter)
        {
            money = newMoney;
            darkMatter = newDarkMatter;
            lifetimeMoney = newLifetimeMoney;
            lifetimeDarkMatter = newLifetimeDarkMatter;

            OnMoneyChanged?.Invoke(money);
            OnDarkMatterChanged?.Invoke(darkMatter);
            OnLifetimeMoneyChanged?.Invoke(lifetimeMoney);
            OnLifetimeDarkMatterChanged?.Invoke(lifetimeDarkMatter);
        }

        /// <summary>
        /// Resets all currencies to zero.
        /// </summary>
        public void ResetAll()
        {
            money = 0;
            darkMatter = 0;
            lifetimeMoney = 0;
            lifetimeDarkMatter = 0;

            OnMoneyChanged?.Invoke(money);
            OnDarkMatterChanged?.Invoke(darkMatter);
            OnLifetimeMoneyChanged?.Invoke(lifetimeMoney);
            OnLifetimeDarkMatterChanged?.Invoke(lifetimeDarkMatter);
        }
    }
}
