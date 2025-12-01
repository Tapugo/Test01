using UnityEngine;
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
        [SerializeField] private float clickRadius = 0.6f; // Radius around click point to detect dice
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
            if (mainCamera == null) return;

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

            // Convert screen position to world position
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(inputPosition);
            worldPos.z = 0;

            // Find dice at click position
            Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, clickRadius, diceLayer);

            // Sort by distance to click the closest one
            Dice closestDice = null;
            float closestDist = float.MaxValue;

            foreach (Collider2D hit in hits)
            {
                Dice dice = hit.GetComponent<Dice>();
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
                if (rolled && showDebugInfo)
                {
                    Debug.Log($"[DiceRollerController] Rolled dice: {closestDice.name}");
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
