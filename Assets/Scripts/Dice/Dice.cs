using UnityEngine;
using Incredicer.Core;
using Incredicer.UI;
using DG.Tweening;
using MoreMountains.Feedbacks;
using TMPro;

namespace Incredicer.Dice
{
    /// <summary>
    /// MonoBehaviour representing a single dice in the game.
    /// Handles rolling logic and visual feedback using DOTween and Feel.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class Dice : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private DiceData data;

        [Header("State")]
        [SerializeField] private float lastRollTime = -999f;
        [SerializeField] private int currentFaceValue = 1;

        [Header("Feel Feedbacks")]
        [SerializeField] private MMF_Player rollFeedback;
        [SerializeField] private MMF_Player jackpotFeedback;

        [Header("Visual Effects")]
        [SerializeField] private GameObject rollEffectPrefab;
        [SerializeField] private GameObject jackpotEffectPrefab;

        [Header("Animation Settings")]
        [SerializeField] private float bounceHeight = 1.5f;
        [SerializeField] private float bounceDuration = 0.25f;
        [SerializeField] private float moveDistance = 2.5f;
        [SerializeField] private float spinSpeed = 720f;
        [SerializeField] private int bounceCount = 2;

        // Screen bounds (calculated from camera)
        private float minX, maxX, minY, maxY;
        private const float SCREEN_PADDING = 0.8f;

        // Dice face sprites (generated at runtime)
        private Sprite[] faceSprites;
        private bool isAnimating = false;

        // Components
        private SpriteRenderer spriteRenderer;
        private CircleCollider2D circleCollider;
        private Sequence currentSequence;
        private Camera mainCamera;

        // Per-dice multiplier (can be modified by skill nodes)
        private double moneyMultiplier = 1.0;
        private double dmMultiplier = 1.0;

        // Properties
        public DiceData Data => data;
        public int CurrentFaceValue => currentFaceValue;
        public double MoneyMultiplier { get => moneyMultiplier; set => moneyMultiplier = value; }
        public double DmMultiplier { get => dmMultiplier; set => dmMultiplier = value; }
        public bool IsAnimating => isAnimating;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            circleCollider = GetComponent<CircleCollider2D>();
            CreateFaceSprites();
            UpdateFaceSprite(1);
        }

        private void Start()
        {
            mainCamera = Camera.main;
            CalculateScreenBounds();
        }

        private void OnDestroy()
        {
            currentSequence?.Kill();
        }

        /// <summary>
        /// Calculates screen bounds based on camera view.
        /// </summary>
        private void CalculateScreenBounds()
        {
            if (mainCamera == null) return;

            float cameraHeight = mainCamera.orthographicSize;
            float cameraWidth = cameraHeight * mainCamera.aspect;

            minX = mainCamera.transform.position.x - cameraWidth + SCREEN_PADDING;
            maxX = mainCamera.transform.position.x + cameraWidth - SCREEN_PADDING;
            minY = mainCamera.transform.position.y - cameraHeight + SCREEN_PADDING;
            maxY = mainCamera.transform.position.y + cameraHeight - SCREEN_PADDING - 1f; // Leave room for UI at top
        }

        /// <summary>
        /// Creates sprites for all 6 dice faces with dots.
        /// </summary>
        private void CreateFaceSprites()
        {
            faceSprites = new Sprite[6];
            int size = 128; // Higher resolution for better quality

            for (int face = 1; face <= 6; face++)
            {
                Texture2D texture = new Texture2D(size, size);
                Color[] pixels = new Color[size * size];

                // Fill with white background and rounded corners
                Color bgColor = new Color(0.98f, 0.98f, 0.95f);
                Color borderColor = new Color(0.3f, 0.3f, 0.35f);
                int cornerRadius = 12;
                int borderWidth = 4;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        // Check if inside rounded rectangle
                        bool inside = IsInsideRoundedRect(x, y, size, size, cornerRadius);
                        bool border = inside && !IsInsideRoundedRect(x, y, size, size, cornerRadius, borderWidth);

                        if (border)
                            pixels[y * size + x] = borderColor;
                        else if (inside)
                            pixels[y * size + x] = bgColor;
                        else
                            pixels[y * size + x] = Color.clear;
                    }
                }

                // Draw dots based on face value
                DrawDots(pixels, size, face);

                texture.SetPixels(pixels);
                texture.Apply();
                texture.filterMode = FilterMode.Bilinear;

                faceSprites[face - 1] = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128f);
            }
        }

        private bool IsInsideRoundedRect(int x, int y, int width, int height, int radius, int inset = 0)
        {
            int left = inset;
            int right = width - 1 - inset;
            int bottom = inset;
            int top = height - 1 - inset;
            int r = radius - inset;

            if (r <= 0) r = 1;

            // Check corners
            if (x < left + r && y < bottom + r)
                return (x - (left + r)) * (x - (left + r)) + (y - (bottom + r)) * (y - (bottom + r)) <= r * r;
            if (x > right - r && y < bottom + r)
                return (x - (right - r)) * (x - (right - r)) + (y - (bottom + r)) * (y - (bottom + r)) <= r * r;
            if (x < left + r && y > top - r)
                return (x - (left + r)) * (x - (left + r)) + (y - (top - r)) * (y - (top - r)) <= r * r;
            if (x > right - r && y > top - r)
                return (x - (right - r)) * (x - (right - r)) + (y - (top - r)) * (y - (top - r)) <= r * r;

            // Check main rect
            return x >= left && x <= right && y >= bottom && y <= top;
        }

        /// <summary>
        /// Draws dots on the dice face texture.
        /// </summary>
        private void DrawDots(Color[] pixels, int size, int faceValue)
        {
            int dotRadius = 10;
            Color dotColor = new Color(0.15f, 0.15f, 0.2f);

            Vector2[] positions = GetDotPositions(faceValue);

            foreach (Vector2 pos in positions)
            {
                int cx = Mathf.RoundToInt(pos.x * (size - 32) + 16);
                int cy = Mathf.RoundToInt(pos.y * (size - 32) + 16);

                // Draw filled circle with anti-aliasing
                for (int y = -dotRadius - 1; y <= dotRadius + 1; y++)
                {
                    for (int x = -dotRadius - 1; x <= dotRadius + 1; x++)
                    {
                        float dist = Mathf.Sqrt(x * x + y * y);
                        if (dist <= dotRadius + 0.5f)
                        {
                            int px = cx + x;
                            int py = cy + y;
                            if (px >= 0 && px < size && py >= 0 && py < size)
                            {
                                float alpha = Mathf.Clamp01(dotRadius + 0.5f - dist);
                                Color existing = pixels[py * size + px];
                                pixels[py * size + px] = Color.Lerp(existing, dotColor, alpha);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets dot positions for each face value.
        /// </summary>
        private Vector2[] GetDotPositions(int face)
        {
            switch (face)
            {
                case 1:
                    return new Vector2[] { new Vector2(0.5f, 0.5f) };
                case 2:
                    return new Vector2[] { new Vector2(0.25f, 0.75f), new Vector2(0.75f, 0.25f) };
                case 3:
                    return new Vector2[] { new Vector2(0.25f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(0.75f, 0.25f) };
                case 4:
                    return new Vector2[] { new Vector2(0.25f, 0.25f), new Vector2(0.25f, 0.75f),
                                          new Vector2(0.75f, 0.25f), new Vector2(0.75f, 0.75f) };
                case 5:
                    return new Vector2[] { new Vector2(0.25f, 0.25f), new Vector2(0.25f, 0.75f),
                                          new Vector2(0.5f, 0.5f),
                                          new Vector2(0.75f, 0.25f), new Vector2(0.75f, 0.75f) };
                case 6:
                    return new Vector2[] { new Vector2(0.25f, 0.2f), new Vector2(0.25f, 0.5f), new Vector2(0.25f, 0.8f),
                                          new Vector2(0.75f, 0.2f), new Vector2(0.75f, 0.5f), new Vector2(0.75f, 0.8f) };
                default:
                    return new Vector2[] { new Vector2(0.5f, 0.5f) };
            }
        }

        /// <summary>
        /// Updates the dice sprite to show a specific face.
        /// </summary>
        private void UpdateFaceSprite(int faceValue)
        {
            if (faceSprites != null && faceValue >= 1 && faceValue <= 6)
            {
                spriteRenderer.sprite = faceSprites[faceValue - 1];
            }
        }

        /// <summary>
        /// Initializes the dice with the given data.
        /// </summary>
        public void Initialize(DiceData diceData)
        {
            data = diceData;
            UpdateVisuals();
            SetupFeedbacks();
        }

        /// <summary>
        /// Sets up Feel feedbacks if not already assigned.
        /// </summary>
        private void SetupFeedbacks()
        {
            if (rollFeedback == null)
            {
                GameObject feedbackObj = new GameObject("RollFeedback");
                feedbackObj.transform.SetParent(transform);
                feedbackObj.transform.localPosition = Vector3.zero;
                rollFeedback = feedbackObj.AddComponent<MMF_Player>();
                rollFeedback.InitializationMode = MMF_Player.InitializationModes.Awake;
            }
        }

        /// <summary>
        /// Updates the dice visuals based on its data.
        /// </summary>
        public void UpdateVisuals()
        {
            if (data == null) return;
            spriteRenderer.color = data.tintColor;
            UpdateFaceSprite(currentFaceValue);
        }

        /// <summary>
        /// Checks if the dice can be rolled (cooldown has passed and not animating).
        /// </summary>
        public bool CanRoll()
        {
            if (data == null) return false;
            if (isAnimating) return false;
            return Time.time >= lastRollTime + data.rollCooldown;
        }

        /// <summary>
        /// Rolls the dice, generating money based on face value.
        /// Reward = face value + upgrade bonus
        /// </summary>
        public bool Roll(bool isManual, bool isIdle)
        {
            if (!CanRoll()) return false;
            if (data == null) return false;

            lastRollTime = Time.time;

            // Roll a random dice face (1-6)
            currentFaceValue = Random.Range(1, 7);

            // Calculate money based on face value + upgrade bonus
            // Base reward = face value (1-6)
            // With upgrades: face value + upgradeLevel
            int upgradeBonus = GameStats.Instance != null ? GameStats.Instance.DiceValueUpgradeLevel : 0;
            double baseMoney = currentFaceValue + upgradeBonus;

            // Apply any global multipliers
            double finalMoney = baseMoney * moneyMultiplier;

            // Check for jackpot (rolling a 6)
            bool isJackpot = currentFaceValue == 6;
            if (isJackpot && GameStats.Instance != null)
            {
                finalMoney *= 2; // Double reward for rolling 6
            }

            // Add money
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddMoney(finalMoney, true);
            }

            // Calculate target position for bounce (within screen bounds)
            Vector3 targetPos = GetRandomTargetPosition();

            // Play roll animation (money popup shown at end of animation)
            PlayRollAnimation(isJackpot, finalMoney, targetPos);

            // Play Feel feedback
            if (rollFeedback != null)
            {
                rollFeedback.PlayFeedbacks();
            }

            if (isJackpot && jackpotFeedback != null)
            {
                jackpotFeedback.PlayFeedbacks();
            }

            // Spawn visual effect
            SpawnRollEffect(isJackpot);

            // Notify listeners
            OnRolled?.Invoke(this, finalMoney, isJackpot);

            return true;
        }

        /// <summary>
        /// Gets a random target position within screen bounds.
        /// </summary>
        private Vector3 GetRandomTargetPosition()
        {
            // Recalculate bounds in case camera changed
            CalculateScreenBounds();

            Vector3 currentPos = transform.position;
            Vector3 newPos;

            // Try to find a valid position that's different from current
            for (int i = 0; i < 15; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(moveDistance * 0.5f, moveDistance);

                newPos = new Vector3(
                    Mathf.Clamp(currentPos.x + Mathf.Cos(angle) * distance, minX, maxX),
                    Mathf.Clamp(currentPos.y + Mathf.Sin(angle) * distance, minY, maxY),
                    0
                );

                // Check if position is far enough from other dice
                bool valid = true;
                if (DiceManager.Instance != null)
                {
                    foreach (var dice in DiceManager.Instance.ActiveDice)
                    {
                        if (dice != this && Vector3.Distance(newPos, dice.transform.position) < 1.2f)
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                if (valid && Vector3.Distance(newPos, currentPos) > 0.5f)
                {
                    return newPos;
                }
            }

            // Fallback: pick a random spot within bounds
            return new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                0
            );
        }

        /// <summary>
        /// Plays the roll animation with bounce to new position.
        /// </summary>
        private void PlayRollAnimation(bool isJackpot, double moneyEarned, Vector3 targetPos)
        {
            currentSequence?.Kill();
            isAnimating = true;

            Vector3 startPos = transform.position;
            Sequence rollSequence = DOTween.Sequence();

            float totalDuration = 0f;
            float currentHeight = bounceHeight;
            float currentDuration = bounceDuration;

            for (int i = 0; i < bounceCount; i++)
            {
                float progress = (float)(i + 1) / bounceCount;
                Vector3 intermediatePos = Vector3.Lerp(startPos, targetPos, progress);

                // Clamp to screen bounds
                intermediatePos.x = Mathf.Clamp(intermediatePos.x, minX, maxX);
                intermediatePos.y = Mathf.Clamp(intermediatePos.y, minY, maxY);

                // Arc up
                float peakY = Mathf.Min(intermediatePos.y + currentHeight, maxY + 0.5f);

                rollSequence.Append(
                    transform.DOMove(new Vector3(intermediatePos.x, peakY, 0), currentDuration * 0.5f)
                        .SetEase(Ease.OutQuad)
                );

                // Spin while in air
                rollSequence.Join(
                    transform.DORotate(new Vector3(0, 0, spinSpeed * currentDuration * (Random.value > 0.5f ? 1 : -1)),
                        currentDuration, RotateMode.FastBeyond360)
                        .SetEase(Ease.Linear)
                );

                // Random face changes during spin
                int changes = 3 - i;
                for (int c = 0; c < changes; c++)
                {
                    float t = totalDuration + (currentDuration * c / changes);
                    rollSequence.InsertCallback(t, () =>
                    {
                        UpdateFaceSprite(Random.Range(1, 7));
                    });
                }

                // Arc down
                rollSequence.Append(
                    transform.DOMove(new Vector3(intermediatePos.x, intermediatePos.y, 0), currentDuration * 0.5f)
                        .SetEase(Ease.InQuad)
                );

                // Squash on land
                rollSequence.Append(
                    transform.DOScale(new Vector3(1.2f, 0.8f, 1f), 0.04f)
                        .SetEase(Ease.OutQuad)
                );
                rollSequence.Append(
                    transform.DOScale(Vector3.one, 0.06f)
                        .SetEase(Ease.OutBounce)
                );

                totalDuration += currentDuration + 0.1f;
                currentHeight *= 0.5f;
                currentDuration *= 0.7f;
            }

            // Final settle - show the result
            rollSequence.AppendCallback(() =>
            {
                transform.rotation = Quaternion.identity;
                UpdateFaceSprite(currentFaceValue);

                // Show floating text popup above dice
                Color textColor = isJackpot ? new Color(1f, 0.85f, 0f) : new Color(0.3f, 1f, 0.4f);
                string prefix = isJackpot ? "JACKPOT! " : "";
                string displayText = $"{prefix}+${GameUI.FormatNumber(moneyEarned)}";

                if (GameUI.Instance != null)
                {
                    GameUI.Instance.ShowFloatingText(transform.position, displayText, textColor);
                }
                else
                {
                    Debug.LogWarning($"[Dice] GameUI.Instance is null, cannot show floating text: {displayText}");
                }
            });

            // Final punch effect
            float scaleAmount = isJackpot ? 0.3f : 0.15f;
            rollSequence.Append(
                transform.DOPunchScale(Vector3.one * scaleAmount, 0.15f, 6, 0.5f)
            );

            // Jackpot color flash for rolling 6
            if (isJackpot)
            {
                rollSequence.Join(
                    spriteRenderer.DOColor(new Color(1f, 0.9f, 0.5f), 0.08f)
                        .SetLoops(4, LoopType.Yoyo)
                        .OnComplete(() => spriteRenderer.color = data != null ? data.tintColor : Color.white)
                );
            }

            rollSequence.OnComplete(() =>
            {
                isAnimating = false;
                transform.rotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            });

            currentSequence = rollSequence;
        }

        /// <summary>
        /// Spawns a visual effect at the dice position.
        /// </summary>
        private void SpawnRollEffect(bool isJackpot)
        {
            GameObject prefab = isJackpot ? jackpotEffectPrefab : rollEffectPrefab;

            if (prefab != null)
            {
                GameObject effect = Instantiate(prefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }

        /// <summary>
        /// Sets the roll effect prefabs.
        /// </summary>
        public void SetEffectPrefabs(GameObject rollEffect, GameObject jackpotEffect)
        {
            rollEffectPrefab = rollEffect;
            jackpotEffectPrefab = jackpotEffect;
        }

        // Events
        public delegate void RolledHandler(Dice dice, double moneyEarned, bool isJackpot);
        public event RolledHandler OnRolled;

        public delegate void DarkMatterHandler(double amount);
        public event DarkMatterHandler OnDarkMatterGenerated;
    }
}
