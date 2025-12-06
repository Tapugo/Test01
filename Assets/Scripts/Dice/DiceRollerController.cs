using UnityEngine;
using UnityEngine.EventSystems;
using Incredicer.Core;

namespace Incredicer.Dice
{
    /// <summary>
    /// Handles mouse/touch click interaction for rolling dice.
    /// Only rolls dice when the player clicks/taps on them.
    /// </summary>
    public class DiceRollerController : MonoBehaviour
    {
        public static DiceRollerController Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float clickRadius = 0.15f; // Radius around click point to detect dice (very tight for precise tapping)
        [SerializeField] private LayerMask diceLayer = ~0;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        private Camera mainCamera;

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
            mainCamera = Camera.main;
            if (showDebugInfo)
            {
                Debug.Log($"[DiceRollerController] Started. Camera: {(mainCamera != null ? mainCamera.name : "NULL")}");
            }
        }

        private void Update()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            // Check for mouse click or touch
            bool clicked = false;
            Vector3 inputPosition = Vector3.zero;

            // Handle mouse input
            if (Input.GetMouseButtonDown(0))
            {
                clicked = true;
                inputPosition = Input.mousePosition;
            }
            // Handle touch input
            else if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    clicked = true;
                    inputPosition = touch.position;
                }
            }

            if (!clicked) return;

            if (showDebugInfo)
            {
                Debug.Log($"[DiceRollerController] Click detected at screen pos: {inputPosition}");
            }

            // Don't process if clicking on UI elements (this handles popups too since they have UI)
            bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            // For touch input, need to check the touch finger ID
            if (Input.touchCount > 0)
            {
                isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }

            if (isOverUI)
            {
                if (showDebugInfo)
                {
                    Debug.Log("[DiceRollerController] Tap ignored - over UI element");
                }
                return;
            }

            // Convert screen position to world position
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(inputPosition);
            worldPos.z = 0;

            if (showDebugInfo)
            {
                Debug.Log($"[DiceRollerController] World pos: {worldPos}, checking radius: {clickRadius}");
            }

            // Find dice at click position
            Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, clickRadius, diceLayer);

            if (showDebugInfo)
            {
                Debug.Log($"[DiceRollerController] Found {hits.Length} colliders at click position");
            }

            // Sort by distance to click the closest one
            Dice closestDice = null;
            float closestDist = float.MaxValue;

            foreach (Collider2D hit in hits)
            {
                Dice dice = hit.GetComponent<Dice>();
                if (showDebugInfo)
                {
                    Debug.Log($"[DiceRollerController] Hit: {hit.name}, has Dice: {dice != null}, CanRoll: {(dice != null ? dice.CanRoll().ToString() : "N/A")}");
                }
                if (dice != null && dice.CanRoll())
                {
                    float dist = Vector2.Distance(worldPos, dice.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestDice = dice;
                    }
                }
            }

            // Roll the closest dice
            if (closestDice != null)
            {
                bool rolled = closestDice.Roll(isManual: true, isIdle: false);
                if (showDebugInfo)
                {
                    Debug.Log($"[DiceRollerController] Rolling dice: {closestDice.name}, success: {rolled}");
                }
            }
            else if (showDebugInfo && hits.Length == 0)
            {
                // Check what dice exist and where they are
                if (DiceManager.Instance != null)
                {
                    var allDice = DiceManager.Instance.GetAllDice();
                    Debug.Log($"[DiceRollerController] No dice found at click. Total dice in game: {allDice.Count}");
                    foreach (var d in allDice)
                    {
                        if (d != null)
                        {
                            float dist = Vector2.Distance(worldPos, d.transform.position);
                            Debug.Log($"[DiceRollerController]   - {d.name} at {d.transform.position}, distance: {dist:F2}, has collider: {d.GetComponent<Collider2D>() != null}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Forces a roll on all dice within range of a specific position.
        /// Used by active skills like Roll Burst.
        /// </summary>
        public int ForceRollAtPosition(Vector2 position, float radius, bool isManual, bool isIdle)
        {
            int rollCount = 0;
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius, diceLayer);

            foreach (Collider2D hit in hits)
            {
                Dice dice = hit.GetComponent<Dice>();
                if (dice != null)
                {
                    dice.Roll(isManual, isIdle);
                    rollCount++;
                }
            }

            return rollCount;
        }

        /// <summary>
        /// Rolls all dice in the game (used by Roll Burst skill).
        /// </summary>
        public int RollAllDice(bool isManual, bool isIdle, int times = 1)
        {
            if (DiceManager.Instance == null) return 0;

            int totalRolls = 0;
            var allDice = DiceManager.Instance.GetAllDice();

            for (int t = 0; t < times; t++)
            {
                foreach (Dice dice in allDice)
                {
                    if (dice != null)
                    {
                        dice.Roll(isManual, isIdle);
                        totalRolls++;
                    }
                }
            }

            return totalRolls;
        }
    }
}
