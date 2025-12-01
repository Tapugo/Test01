using UnityEngine;
using DG.Tweening;
using Incredicer.Core;
using MoreMountains.Feedbacks;

namespace Incredicer.Helpers
{
    // Type alias to avoid namespace conflict
    using DiceObject = Incredicer.Dice.Dice;
    using DiceManagerClass = Incredicer.Dice.DiceManager;

    /// <summary>
    /// Represents a single helper hand that auto-rolls dice.
    /// Uses DOTween for smooth animations.
    /// </summary>
    public class HelperHand : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float baseCooldown = 2f;
        [SerializeField] private float moveSpeed = 5f;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer handSprite;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new Color(1, 1, 1, 0.5f);

        [Header("Animation")]
        [SerializeField] private float hoverHeight = 0.5f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobAmount = 0.1f;
        [SerializeField] private float rollPunchScale = 0.2f;

        [Header("Effects")]
        [SerializeField] private GameObject rollEffectPrefab;
        [SerializeField] private MMF_Player rollFeedback;

        // State
        private float timer = 0f;
        private DiceObject currentTarget;
        private bool isMovingToTarget = false;
        private Vector3 startPosition;
        private Sequence movementSequence;
        private Tweener bobTween;

        // Properties
        public float BaseCooldown => baseCooldown;
        public bool IsActive { get; private set; } = true;

        private void Start()
        {
            startPosition = transform.position;
            
            // Start bobbing animation
            StartBobAnimation();
            
            // Slightly randomize initial timer to desync multiple hands
            timer = Random.Range(0f, baseCooldown * 0.5f);
        }

        private void OnDestroy()
        {
            movementSequence?.Kill();
            bobTween?.Kill();
        }

        private void Update()
        {
            if (!IsActive) return;
            if (DiceManagerClass.Instance == null) return;
            if (GameStats.Instance == null) return;

            // Calculate effective cooldown
            float effectiveCooldown = baseCooldown / (float)GameStats.Instance.HelperHandSpeedMultiplier;

            timer += Time.deltaTime;

            if (timer >= effectiveCooldown && !isMovingToTarget)
            {
                timer = 0f;
                PerformRollCycle();
            }
        }

        /// <summary>
        /// Performs a complete roll cycle - move to dice, roll, return.
        /// </summary>
        private void PerformRollCycle()
        {
            var allDice = DiceManagerClass.Instance.GetAllDice();
            if (allDice.Count == 0) return;

            // Determine how many dice to roll
            int diceToRoll = 1 + GameStats.Instance.HelperHandExtraRolls;
            diceToRoll = Mathf.Min(diceToRoll, allDice.Count);

            // Select random dice to roll
            var selectedDice = new System.Collections.Generic.List<DiceObject>();
            var availableDice = new System.Collections.Generic.List<DiceObject>(allDice);

            for (int i = 0; i < diceToRoll && availableDice.Count > 0; i++)
            {
                int index = Random.Range(0, availableDice.Count);
                selectedDice.Add(availableDice[index]);
                availableDice.RemoveAt(index);
            }

            if (selectedDice.Count == 0) return;

            // Animate to each dice and roll
            AnimateRollSequence(selectedDice);
        }

        /// <summary>
        /// Animates the hand moving to each dice and rolling them.
        /// </summary>
        private void AnimateRollSequence(System.Collections.Generic.List<DiceObject> diceToRoll)
        {
            isMovingToTarget = true;
            movementSequence?.Kill();
            bobTween?.Pause();

            movementSequence = DOTween.Sequence();

            foreach (DiceObject dice in diceToRoll)
            {
                Vector3 targetPos = dice.transform.position + Vector3.up * hoverHeight;

                // Move to dice
                movementSequence.Append(
                    transform.DOMove(targetPos, 0.3f / (float)GameStats.Instance.HelperHandSpeedMultiplier)
                        .SetEase(Ease.OutQuad)
                );

                // Roll animation (punch down)
                movementSequence.Append(
                    transform.DOMoveY(dice.transform.position.y + 0.1f, 0.1f)
                        .SetEase(Ease.InQuad)
                );

                // Trigger roll
                movementSequence.AppendCallback(() => RollDice(dice));

                // Punch back up
                movementSequence.Append(
                    transform.DOMoveY(targetPos.y, 0.15f)
                        .SetEase(Ease.OutBounce)
                );

                // Small pause between dice
                movementSequence.AppendInterval(0.05f);
            }

            // Return to start position
            movementSequence.Append(
                transform.DOMove(startPosition, 0.4f / (float)GameStats.Instance.HelperHandSpeedMultiplier)
                    .SetEase(Ease.InOutQuad)
            );

            movementSequence.OnComplete(() =>
            {
                isMovingToTarget = false;
                bobTween?.Play();
            });
        }

        /// <summary>
        /// Rolls a specific dice with visual feedback.
        /// </summary>
        private void RollDice(DiceObject dice)
        {
            if (dice == null) return;

            // Roll the dice (idle roll)
            dice.Roll(isManual: false, isIdle: true);

            // Hand punch effect
            transform.DOPunchScale(Vector3.one * rollPunchScale, 0.2f, 5, 0.5f);

            // Play feedback
            if (rollFeedback != null)
            {
                rollFeedback.PlayFeedbacks();
            }

            // Spawn effect
            if (rollEffectPrefab != null)
            {
                GameObject effect = Instantiate(rollEffectPrefab, dice.transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }

        /// <summary>
        /// Starts the idle bobbing animation.
        /// </summary>
        private void StartBobAnimation()
        {
            bobTween?.Kill();
            bobTween = transform.DOMoveY(startPosition.y + bobAmount, 1f / bobSpeed)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        /// <summary>
        /// Sets the hand active or inactive.
        /// </summary>
        public void SetActive(bool active)
        {
            IsActive = active;

            if (handSprite != null)
            {
                handSprite.DOColor(active ? activeColor : inactiveColor, 0.3f);
            }

            if (!active)
            {
                movementSequence?.Kill();
                bobTween?.Pause();
            }
            else
            {
                bobTween?.Play();
            }
        }

        /// <summary>
        /// Sets the start/home position for this hand.
        /// </summary>
        public void SetHomePosition(Vector3 position)
        {
            startPosition = position;
            transform.position = position;
            StartBobAnimation();
        }
    }
}
