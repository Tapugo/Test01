using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Incredicer.Core;

namespace Incredicer.Dice
{
    /// <summary>
    /// Manages all dice in the game: spawning, tracking, and unlocking.
    /// </summary>
    public class DiceManager : MonoBehaviour
    {
        public static DiceManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private List<DiceData> allDiceData = new List<DiceData>();
        [SerializeField] private GameObject dicePrefab;

        [Header("Spawn Settings")]
        [SerializeField] private float minDistanceBetweenDice = 1.2f;
        [SerializeField] private float screenPadding = 0.8f;

        // Screen bounds (calculated from camera)
        private float minX, maxX, minY, maxY;
        private Camera mainCamera;

        // UI exclusion zones (in world units from screen edges)
        // These should match the values in Dice.cs
        private const float LEFT_UI_ZONE_WIDTH = 2.5f;   // How far from left edge for Skills/Ascend buttons
        private const float LEFT_UI_ZONE_TOP = 3.5f;     // How far down from top the zone extends
        private const float BOTTOM_UI_ZONE_HEIGHT = 1.8f; // How far up from bottom for shop panel
        private const float RIGHT_UI_ZONE_WIDTH = 2.5f;  // How far from right edge for currency panel and menu
        private const float RIGHT_UI_ZONE_TOP = 3.0f;    // How far down from top the right zone extends

        [Header("State")]
        [SerializeField] private bool darkMatterUnlocked = false;

        // Track all active dice
        private List<Dice> activeDice = new List<Dice>();
        
        // Track which dice types are unlocked
        private HashSet<DiceType> unlockedDiceTypes = new HashSet<DiceType>();
        
        // Track owned count per type
        private Dictionary<DiceType, int> ownedDiceCount = new Dictionary<DiceType, int>();

        // Properties
        public bool DarkMatterUnlocked 
        { 
            get => darkMatterUnlocked; 
            set => darkMatterUnlocked = value; 
        }
        public IReadOnlyList<Dice> ActiveDice => activeDice;
        public IReadOnlyCollection<DiceType> UnlockedDiceTypes => unlockedDiceTypes;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Initialize owned counts
            foreach (DiceType type in System.Enum.GetValues(typeof(DiceType)))
            {
                ownedDiceCount[type] = 0;
            }

            // Basic dice is always unlocked
            unlockedDiceTypes.Add(DiceType.Basic);
        }

        private void Start()
        {
            mainCamera = Camera.main;
            CalculateScreenBounds();

            Debug.Log($"[DiceManager] Start called. AllDiceData count: {allDiceData.Count}, DiceRestored: {diceRestored}");

            // Wait a frame to allow SaveSystem to load data first
            StartCoroutine(SpawnInitialDiceDelayed());
        }

        private System.Collections.IEnumerator SpawnInitialDiceDelayed()
        {
            // Wait for save system to potentially restore dice
            yield return new WaitForSeconds(0.2f);

            // Spawn initial basic die only if no dice exist and none were restored from save
            if (activeDice.Count == 0 && !diceRestored)
            {
                DiceData basicData = GetDiceData(DiceType.Basic);
                Debug.Log($"[DiceManager] Basic dice data: {(basicData != null ? basicData.displayName : "NULL")}");

                if (basicData != null)
                {
                    SpawnDice(basicData, Vector2.zero);
                    Debug.Log("[DiceManager] Spawned initial basic dice");
                }
                else
                {
                    Debug.LogWarning("[DiceManager] Could not find Basic dice data!");
                }
            }
        }

        /// <summary>
        /// Calculates screen bounds based on camera view.
        /// </summary>
        private void CalculateScreenBounds()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            if (mainCamera == null) return;

            float cameraHeight = mainCamera.orthographicSize;
            float cameraWidth = cameraHeight * mainCamera.aspect;

            minX = mainCamera.transform.position.x - cameraWidth + screenPadding;
            maxX = mainCamera.transform.position.x + cameraWidth - screenPadding;
            minY = mainCamera.transform.position.y - cameraHeight + screenPadding;
            maxY = mainCamera.transform.position.y + cameraHeight - screenPadding - 1f; // Leave room for UI at top
        }

        /// <summary>
        /// Gets the DiceData for a specific type.
        /// </summary>
        public DiceData GetDiceData(DiceType type)
        {
            return allDiceData.Find(d => d.type == type);
        }

        /// <summary>
        /// Gets the basic dice data (convenience method).
        /// </summary>
        public DiceData GetBasicDiceData()
        {
            return GetDiceData(DiceType.Basic);
        }

        /// <summary>
        /// Checks if a dice type is unlocked.
        /// </summary>
        public bool IsDiceTypeUnlocked(DiceType type)
        {
            return unlockedDiceTypes.Contains(type);
        }

        /// <summary>
        /// Unlocks a dice type for purchase.
        /// </summary>
        public void UnlockDiceType(DiceType type)
        {
            unlockedDiceTypes.Add(type);
            OnDiceTypeUnlocked?.Invoke(type);
        }

        /// <summary>
        /// Gets the count of owned dice of a specific type.
        /// </summary>
        public int GetOwnedCount(DiceType type)
        {
            return ownedDiceCount.TryGetValue(type, out int count) ? count : 0;
        }

        /// <summary>
        /// Gets the current price for a dice type.
        /// </summary>
        public double GetCurrentPrice(DiceType type)
        {
            DiceData data = GetDiceData(type);
            if (data == null) return double.MaxValue;
            return data.GetCurrentPrice(GetOwnedCount(type));
        }

        /// <summary>
        /// Attempts to buy a dice of the specified type.
        /// </summary>
        public bool TryBuyDice(DiceType type)
        {
            if (!IsDiceTypeUnlocked(type)) return false;

            DiceData data = GetDiceData(type);
            if (data == null) return false;

            double price = GetCurrentPrice(type);
            if (!CurrencyManager.Instance.SpendMoney(price)) return false;

            Vector2 spawnPos = GetRandomSpawnPosition();
            SpawnDice(data, spawnPos);

            // Play purchase sound
            if (Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlayPurchaseSound();
            }

            // Spawn purchase particle effect
            if (Core.VisualEffectsManager.Instance != null)
            {
                Core.VisualEffectsManager.Instance.SpawnPurchaseEffect(spawnPos);
            }

            return true;
        }

        /// <summary>
        /// Spawns a dice at the specified position.
        /// </summary>
        public Dice SpawnDice(DiceData data, Vector2 position)
        {
            GameObject diceObj;
            
            if (dicePrefab != null)
            {
                diceObj = Instantiate(dicePrefab, position, Quaternion.identity);
            }
            else
            {
                // Create dice object manually if no prefab
                diceObj = new GameObject($"Dice_{data.type}");
                diceObj.transform.position = position;
                
                // Add required components
                var sr = diceObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 1;
                
                var collider = diceObj.AddComponent<CircleCollider2D>();
                collider.radius = 0.25f; // Smaller radius for tighter click detection
                
                diceObj.AddComponent<Dice>();
            }

            Dice dice = diceObj.GetComponent<Dice>();
            dice.Initialize(data);

            // Subscribe to dark matter generation
            dice.OnDarkMatterGenerated += HandleDarkMatterGenerated;

            activeDice.Add(dice);
            ownedDiceCount[data.type]++;

            OnDiceSpawned?.Invoke(dice);
            return dice;
        }

        /// <summary>
        /// Gets all dice of a specific type.
        /// </summary>
        public List<Dice> GetDiceOfType(DiceType type)
        {
            return activeDice.Where(d => d.Data != null && d.Data.type == type).ToList();
        }

        /// <summary>
        /// Gets all dice currently in the game.
        /// </summary>
        public List<Dice> GetAllDice()
        {
            // Clean up any null references first
            activeDice.RemoveAll(d => d == null);
            return new List<Dice>(activeDice);
        }

        /// <summary>
        /// Removes a dice from tracking and destroys it.
        /// </summary>
        public void RemoveDice(Dice dice)
        {
            if (dice == null) return;

            // Unsubscribe from events
            dice.OnDarkMatterGenerated -= HandleDarkMatterGenerated;

            // Update count
            if (dice.Data != null && ownedDiceCount.ContainsKey(dice.Data.type))
            {
                ownedDiceCount[dice.Data.type] = Mathf.Max(0, ownedDiceCount[dice.Data.type] - 1);
            }

            // Remove from list
            activeDice.Remove(dice);

            // Destroy the game object
            Destroy(dice.gameObject);

            // Ensure at least one dice always exists (prevent soft-lock)
            EnsureAtLeastOneDice();
        }

        /// <summary>
        /// Ensures the player always has at least one dice to prevent soft-lock.
        /// Spawns a free basic dice if all dice have been destroyed.
        /// </summary>
        private void EnsureAtLeastOneDice()
        {
            // Clean up null references first
            activeDice.RemoveAll(d => d == null);

            // If no dice remain, spawn a free basic dice
            if (activeDice.Count == 0)
            {
                Debug.Log("[DiceManager] No dice remaining! Spawning free basic dice to prevent soft-lock.");

                DiceData basicData = allDiceData.Find(d => d.type == DiceType.Basic);
                if (basicData == null && allDiceData.Count > 0)
                {
                    basicData = allDiceData[0]; // Fallback to first available
                }

                if (basicData != null)
                {
                    SpawnDice(basicData, Vector2.zero);
                }
            }
        }

        /// <summary>
        /// Removes all dice except one basic die (used for prestige reset).
        /// </summary>
        public void ResetToSingleBasicDice()
        {
            // Find one basic dice to keep
            Dice diceToKeep = null;
            foreach (var dice in activeDice)
            {
                if (dice != null && dice.Data != null && dice.Data.type == DiceType.Basic)
                {
                    diceToKeep = dice;
                    break;
                }
            }

            // Create a list of dice to remove (can't modify during iteration)
            List<Dice> diceToRemove = new List<Dice>();
            foreach (var dice in activeDice)
            {
                if (dice != null && dice != diceToKeep)
                {
                    diceToRemove.Add(dice);
                }
            }

            // Remove all dice except the one we're keeping
            foreach (var dice in diceToRemove)
            {
                if (dice != null)
                {
                    // Unsubscribe from events
                    dice.OnDarkMatterGenerated -= HandleDarkMatterGenerated;
                    Destroy(dice.gameObject);
                }
            }

            // Clear and rebuild the active dice list
            activeDice.Clear();
            if (diceToKeep != null)
            {
                activeDice.Add(diceToKeep);
            }

            // Reset owned counts
            foreach (DiceType type in System.Enum.GetValues(typeof(DiceType)))
            {
                ownedDiceCount[type] = 0;
            }
            if (diceToKeep != null && diceToKeep.Data != null)
            {
                ownedDiceCount[diceToKeep.Data.type] = 1;
            }

            // If no dice to keep, spawn a new basic one
            if (diceToKeep == null)
            {
                DiceData basicData = GetDiceData(DiceType.Basic);
                if (basicData != null)
                {
                    SpawnDice(basicData, Vector2.zero);
                }
            }

            Debug.Log($"[DiceManager] Reset to single basic dice. Active dice count: {activeDice.Count}");
        }

        /// <summary>
        /// Gets the count of unique dice types owned.
        /// </summary>
        public int GetUniqueDiceTypesOwned()
        {
            return ownedDiceCount.Count(kvp => kvp.Value > 0);
        }

        /// <summary>
        /// Handles dark matter generated from dice rolls.
        /// </summary>
        private void HandleDarkMatterGenerated(double amount)
        {
            if (darkMatterUnlocked && amount > 0)
            {
                CurrencyManager.Instance.AddDarkMatter(amount);
            }
        }

        /// <summary>
        /// Checks if a position is inside a UI exclusion zone.
        /// </summary>
        private bool IsInUIZone(Vector2 pos)
        {
            if (mainCamera == null) return false;

            float cameraHeight = mainCamera.orthographicSize;
            float cameraWidth = cameraHeight * mainCamera.aspect;
            float camX = mainCamera.transform.position.x;
            float camY = mainCamera.transform.position.y;

            float screenLeft = camX - cameraWidth;
            float screenRight = camX + cameraWidth;
            float screenTop = camY + cameraHeight;
            float screenBottom = camY - cameraHeight;

            // Check if in left buttons zone (top-left corner - Skills/Ascend buttons)
            bool inLeftZone = pos.x < screenLeft + LEFT_UI_ZONE_WIDTH &&
                              pos.y > screenTop - LEFT_UI_ZONE_TOP;

            // Check if in right UI zone (top-right corner - currency panel and menu)
            bool inRightZone = pos.x > screenRight - RIGHT_UI_ZONE_WIDTH &&
                               pos.y > screenTop - RIGHT_UI_ZONE_TOP;

            // Check if in bottom shop panel zone (center-bottom)
            bool inBottomZone = pos.y < screenBottom + BOTTOM_UI_ZONE_HEIGHT;

            return inLeftZone || inRightZone || inBottomZone;
        }

        /// <summary>
        /// Gets a random spawn position within screen bounds, avoiding UI zones.
        /// </summary>
        private Vector2 GetRandomSpawnPosition()
        {
            // Recalculate bounds in case camera changed
            CalculateScreenBounds();

            // Try to find a position that doesn't overlap with existing dice and avoids UI zones
            for (int attempts = 0; attempts < 50; attempts++)
            {
                Vector2 pos = new Vector2(
                    Random.Range(minX, maxX),
                    Random.Range(minY, maxY)
                );

                // Skip if position is in a UI zone
                if (IsInUIZone(pos))
                {
                    continue;
                }

                bool valid = true;
                foreach (Dice dice in activeDice)
                {
                    if (dice != null && Vector2.Distance(pos, dice.transform.position) < minDistanceBetweenDice)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid) return pos;
            }

            // Fallback: try to find any position outside UI zones
            for (int attempts = 0; attempts < 20; attempts++)
            {
                Vector2 pos = new Vector2(
                    Random.Range(minX, maxX),
                    Random.Range(minY, maxY)
                );

                if (!IsInUIZone(pos))
                {
                    return pos;
                }
            }

            // Last resort: return center of screen
            return mainCamera != null ? (Vector2)mainCamera.transform.position : Vector2.zero;
        }

        /// <summary>
        /// Sets owned dice counts (used for save/load).
        /// </summary>
        public void SetOwnedCounts(Dictionary<DiceType, int> counts)
        {
            ownedDiceCount = new Dictionary<DiceType, int>(counts);
        }

        /// <summary>
        /// Gets owned dice counts (used for save/load).
        /// </summary>
        public Dictionary<DiceType, int> GetOwnedCounts()
        {
            return new Dictionary<DiceType, int>(ownedDiceCount);
        }

        /// <summary>
        /// Restores saved dice from save data.
        /// </summary>
        public void RestoreSavedDice(List<Core.SavedDice> savedDice)
        {
            if (savedDice == null || savedDice.Count == 0) return;

            // Clear existing dice first
            foreach (var dice in new List<Dice>(activeDice))
            {
                if (dice != null)
                {
                    dice.OnDarkMatterGenerated -= HandleDarkMatterGenerated;
                    Destroy(dice.gameObject);
                }
            }
            activeDice.Clear();

            // Reset owned counts
            foreach (DiceType type in System.Enum.GetValues(typeof(DiceType)))
            {
                ownedDiceCount[type] = 0;
            }

            // Spawn saved dice
            foreach (var saved in savedDice)
            {
                DiceData data = GetDiceData(saved.type);
                if (data != null)
                {
                    SpawnDice(data, saved.position);
                }
            }

            diceRestored = true;
            Debug.Log($"[DiceManager] Restored {savedDice.Count} dice from save");
        }

        // Flag to prevent double-spawning on load
        private bool diceRestored = false;
        public bool DiceRestored => diceRestored;

        // Events
        public event System.Action<Dice> OnDiceSpawned;
        public event System.Action<DiceType> OnDiceTypeUnlocked;
    }
}
