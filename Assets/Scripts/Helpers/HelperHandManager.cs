using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Incredicer.Core;

namespace Incredicer.Helpers
{
    // Type alias to avoid namespace conflict
    using DiceManagerClass = Incredicer.Dice.DiceManager;
    /// <summary>
    /// Manages all helper hands in the game.
    /// Handles spawning, positioning, and offline simulation.
    /// </summary>
    public class HelperHandManager : MonoBehaviour
    {
        public static HelperHandManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject helperHandPrefab;

        [Header("Settings")]
        [SerializeField] private int maxHands = 0;
        [SerializeField] private float handSpacing = 1.5f;
        [SerializeField] private Vector2 handsAreaOffset = new Vector2(0, 3f);

        [Header("Offline Simulation")]
        [SerializeField] private float offlineEfficiency = 0.25f; // 0-1, can be upgraded via skills (default 25%, upgradeable to 100%)
        [SerializeField] private double lastPlayTime;

        // Active hands
        private List<HelperHand> activeHands = new List<HelperHand>();

        // Properties
        public int MaxHands 
        { 
            get => maxHands; 
            set 
            { 
                maxHands = value;
                OnMaxHandsChanged?.Invoke(maxHands);
            }
        }
        public int ActiveHandCount => activeHands.Count;
        public float OfflineEfficiency { get => offlineEfficiency; set => offlineEfficiency = Mathf.Clamp01(value); }
        public IReadOnlyList<HelperHand> ActiveHands => activeHands;

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
            // Check for offline earnings
            ProcessOfflineEarnings();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Save current time when pausing
                lastPlayTime = GetCurrentTimestamp();
                SaveLastPlayTime();
            }
            else
            {
                // Process offline earnings when resuming
                ProcessOfflineEarnings();
            }
        }

        private void OnApplicationQuit()
        {
            lastPlayTime = GetCurrentTimestamp();
            SaveLastPlayTime();
        }

        /// <summary>
        /// Adds a new helper hand if under the limit.
        /// </summary>
        public bool AddHand()
        {
            if (activeHands.Count >= maxHands)
            {
                return false;
            }

            SpawnHand();
            return true;
        }

        /// <summary>
        /// Spawns a new helper hand at the appropriate position.
        /// </summary>
        private HelperHand SpawnHand()
        {
            GameObject handObj;

            if (helperHandPrefab != null)
            {
                handObj = Instantiate(helperHandPrefab);
            }
            else
            {
                // Create hand object manually if no prefab
                handObj = new GameObject($"HelperHand_{activeHands.Count + 1}");
                
                // Add sprite renderer
                var sr = handObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 10;
                sr.color = new Color(0.8f, 0.6f, 0.4f); // Hand color
                
                handObj.AddComponent<HelperHand>();
            }

            HelperHand hand = handObj.GetComponent<HelperHand>();
            
            // Position the hand
            Vector3 handPosition = CalculateHandPosition(activeHands.Count);
            hand.SetHomePosition(handPosition);

            activeHands.Add(hand);
            
            // Animate spawn
            hand.transform.localScale = Vector3.zero;
            hand.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

            OnHandAdded?.Invoke(hand);
            return hand;
        }

        /// <summary>
        /// Calculates the position for a hand based on its index.
        /// </summary>
        private Vector3 CalculateHandPosition(int index)
        {
            float totalWidth = (maxHands - 1) * handSpacing;
            float startX = -totalWidth / 2f;
            float x = startX + index * handSpacing;
            
            return new Vector3(x + handsAreaOffset.x, handsAreaOffset.y, 0);
        }

        /// <summary>
        /// Repositions all hands when max hands changes.
        /// </summary>
        public void RepositionHands()
        {
            for (int i = 0; i < activeHands.Count; i++)
            {
                Vector3 newPos = CalculateHandPosition(i);
                activeHands[i].transform.DOMove(newPos, 0.5f).SetEase(Ease.OutQuad);
                activeHands[i].SetHomePosition(newPos);
            }
        }

        /// <summary>
        /// Removes a specific helper hand.
        /// </summary>
        public void RemoveHand(HelperHand hand)
        {
            if (hand == null || !activeHands.Contains(hand)) return;

            activeHands.Remove(hand);

            // Animate and destroy
            hand.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                .OnComplete(() => Destroy(hand.gameObject));
        }

        /// <summary>
        /// Removes all helper hands.
        /// </summary>
        public void RemoveAllHands()
        {
            foreach (var hand in activeHands)
            {
                if (hand != null)
                {
                    Destroy(hand.gameObject);
                }
            }
            activeHands.Clear();
            Debug.Log("[HelperHandManager] All helper hands removed");
        }

        /// <summary>
        /// Resets helper hands to initial state (for Time Fracture).
        /// </summary>
        public void ResetForTimeFracture()
        {
            RemoveAllHands();
            maxHands = 0;
            offlineEfficiency = 0.25f;
            OnMaxHandsChanged?.Invoke(maxHands);
            Debug.Log("[HelperHandManager] Reset for Time Fracture");
        }

        /// <summary>
        /// Removes excess hands when max hands is reduced.
        /// </summary>
        public void TrimToMaxHands()
        {
            while (activeHands.Count > maxHands)
            {
                var handToRemove = activeHands[activeHands.Count - 1];
                activeHands.RemoveAt(activeHands.Count - 1);
                if (handToRemove != null)
                {
                    handToRemove.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                        .OnComplete(() => Destroy(handToRemove.gameObject));
                }
            }
        }

        /// <summary>
        /// Sets all hands active or inactive.
        /// </summary>
        public void SetAllHandsActive(bool active)
        {
            foreach (var hand in activeHands)
            {
                hand.SetActive(active);
            }
        }

        /// <summary>
        /// Processes earnings from time spent offline.
        /// </summary>
        private void ProcessOfflineEarnings()
        {
            double savedTime = LoadLastPlayTime();
            if (savedTime <= 0) return;

            double currentTime = GetCurrentTimestamp();
            double elapsedSeconds = currentTime - savedTime;

            if (elapsedSeconds <= 0) return;

            // Cap offline time (e.g., max 24 hours)
            elapsedSeconds = System.Math.Min(elapsedSeconds, 24 * 60 * 60);

            // Calculate offline earnings
            CalculateOfflineEarnings(elapsedSeconds);
        }

        /// <summary>
        /// Calculates and applies offline earnings.
        /// </summary>
        private void CalculateOfflineEarnings(double elapsedSeconds)
        {
            if (activeHands.Count == 0) return;
            if (DiceManagerClass.Instance == null) return;
            if (CurrencyManager.Instance == null) return;
            if (GameStats.Instance == null) return;

            var allDice = DiceManagerClass.Instance.GetAllDice();
            if (allDice.Count == 0) return;

            // Calculate rolls per second per hand
            float baseCooldown = 4f; // Default hand cooldown
            float effectiveCooldown = baseCooldown / (float)GameStats.Instance.HelperHandSpeedMultiplier;
            float rollsPerSecondPerHand = 1f / effectiveCooldown;
            
            // Dice rolled per cycle
            int dicePerCycle = 1 + GameStats.Instance.HelperHandExtraRolls;
            dicePerCycle = Mathf.Min(dicePerCycle, allDice.Count);

            // Total rolls during offline time
            double totalRolls = elapsedSeconds * rollsPerSecondPerHand * activeHands.Count * dicePerCycle;
            totalRolls *= offlineEfficiency;

            // Calculate average payout per roll
            double avgMoneyPerRoll = 0;
            double avgDmPerRoll = 0;

            foreach (var dice in allDice)
            {
                if (dice.Data != null)
                {
                    avgMoneyPerRoll += dice.Data.basePayout * dice.MoneyMultiplier;
                    avgDmPerRoll += dice.Data.dmPerRoll * dice.DmMultiplier;
                }
            }
            avgMoneyPerRoll /= allDice.Count;
            avgDmPerRoll /= allDice.Count;

            // Apply modifiers
            double moneyEarned = totalRolls * avgMoneyPerRoll * 
                                 GameStats.Instance.GlobalMoneyMultiplier * 
                                 GameStats.Instance.IdleMoneyMultiplier;

            double dmEarned = 0;
            if (DiceManagerClass.Instance.DarkMatterUnlocked)
            {
                dmEarned = totalRolls * avgDmPerRoll * GameStats.Instance.DarkMatterGainMultiplier;
            }

            // Apply earnings
            if (moneyEarned > 0)
            {
                CurrencyManager.Instance.AddMoney(moneyEarned, true);
            }

            if (dmEarned > 0)
            {
                CurrencyManager.Instance.AddDarkMatter(dmEarned);
            }

            // Notify about offline earnings
            OnOfflineEarningsApplied?.Invoke(moneyEarned, dmEarned, elapsedSeconds);

            Debug.Log($"[HelperHandManager] Offline earnings: {moneyEarned:F0} money, {dmEarned:F2} DM over {elapsedSeconds:F0} seconds");
        }

        /// <summary>
        /// Gets the current Unix timestamp.
        /// </summary>
        private double GetCurrentTimestamp()
        {
            return System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /// <summary>
        /// Saves the last play time to PlayerPrefs.
        /// </summary>
        private void SaveLastPlayTime()
        {
            PlayerPrefs.SetString("LastPlayTime", lastPlayTime.ToString());
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads the last play time from PlayerPrefs.
        /// </summary>
        private double LoadLastPlayTime()
        {
            string saved = PlayerPrefs.GetString("LastPlayTime", "0");
            if (double.TryParse(saved, out double result))
            {
                return result;
            }
            return 0;
        }

        /// <summary>
        /// Gets save data for the helper hand system.
        /// </summary>
        public HelperHandSaveData GetSaveData()
        {
            return new HelperHandSaveData
            {
                handCount = activeHands.Count,
                maxHands = maxHands,
                offlineEfficiency = offlineEfficiency
            };
        }

        /// <summary>
        /// Loads save data for the helper hand system.
        /// </summary>
        public void LoadSaveData(HelperHandSaveData data)
        {
            maxHands = data.maxHands;
            offlineEfficiency = data.offlineEfficiency;

            // Spawn saved hands
            while (activeHands.Count < data.handCount && activeHands.Count < maxHands)
            {
                SpawnHand();
            }
        }

        // Events
        public event System.Action<HelperHand> OnHandAdded;
        public event System.Action<int> OnMaxHandsChanged;
        public event System.Action<double, double, double> OnOfflineEarningsApplied; // money, dm, seconds
    }

    /// <summary>
    /// Save data structure for helper hands.
    /// </summary>
    [System.Serializable]
    public class HelperHandSaveData
    {
        public int handCount;
        public int maxHands;
        public float offlineEfficiency;
    }
}
