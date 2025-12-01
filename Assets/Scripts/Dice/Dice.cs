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

        // UI exclusion zones (in world units from screen edges)
        // Left buttons zone: top-left corner where Skills/Ascend buttons are
        private const float LEFT_UI_ZONE_WIDTH = 2.5f;   // How far from left edge
        private const float LEFT_UI_ZONE_TOP = 3.5f;     // How far down from top the zone extends
        // Bottom shop panel zone: center-bottom where Buy/Upgrade buttons are
        private const float BOTTOM_UI_ZONE_HEIGHT = 1.8f; // How far up from bottom

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

        // Store the initial scale set during Initialize
        private Vector3 initialScale = Vector3.one * 0.7f;

        // FocusedGravity / PrecisionAim movement
        [Header("Gravity Settings")]
        [SerializeField] private float gravityForce = 0.5f;
        [SerializeField] private float precisionAimForce = 3f;
        [SerializeField] private float maxDriftSpeed = 2f;
        private Vector2 currentVelocity = Vector2.zero;

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

        private void Update()
        {
            // Skip movement updates if animating (rolling)
            if (isAnimating) return;

            ApplyGravityAndAimEffects();
        }

        /// <summary>
        /// Applies FocusedGravity and PrecisionAim movement effects.
        /// </summary>
        private void ApplyGravityAndAimEffects()
        {
            if (GameStats.Instance == null) return;

            Vector2 force = Vector2.zero;
            Vector3 currentPos = transform.position;

            // FocusedGravity: Dice drift toward the center of all dice (cluster together)
            if (GameStats.Instance.FocusedGravityActive)
            {
                Vector2 clusterCenter = GetDiceClusterCenter();
                Vector2 toCenter = clusterCenter - (Vector2)currentPos;
                float distance = toCenter.magnitude;

                // Only apply force if not already at center
                if (distance > 0.5f)
                {
                    force += toCenter.normalized * gravityForce;
                }
            }

            // PrecisionAim: When mouse is held, dice are pulled toward cursor
            if (GameStats.Instance.PrecisionAimActive && Input.GetMouseButton(0))
            {
                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0;

                Vector2 toCursor = (Vector2)mouseWorldPos - (Vector2)currentPos;
                float distanceToCursor = toCursor.magnitude;

                // Apply stronger force when closer, capped at max
                if (distanceToCursor > 0.3f)
                {
                    float pullStrength = Mathf.Min(precisionAimForce, precisionAimForce * (3f / distanceToCursor));
                    force += toCursor.normalized * pullStrength;
                }
            }

            // Apply force to velocity with damping
            if (force.sqrMagnitude > 0.001f)
            {
                currentVelocity += force * Time.deltaTime;
                currentVelocity = Vector2.ClampMagnitude(currentVelocity, maxDriftSpeed);
            }
            else
            {
                // Dampen velocity when no force applied
                currentVelocity *= (1f - Time.deltaTime * 3f);
            }

            // Apply velocity to position
            if (currentVelocity.sqrMagnitude > 0.001f)
            {
                Vector3 newPos = currentPos + (Vector3)currentVelocity * Time.deltaTime;

                // Clamp to screen bounds
                newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
                newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

                // Avoid UI zones
                if (!IsInUIZone(newPos))
                {
                    transform.position = newPos;
                }
                else
                {
                    // Bounce off UI zones
                    currentVelocity *= -0.5f;
                }
            }
        }

        /// <summary>
        /// Gets the center position of all dice (for FocusedGravity clustering).
        /// </summary>
        private Vector2 GetDiceClusterCenter()
        {
            if (DiceManager.Instance == null || DiceManager.Instance.ActiveDice.Count == 0)
            {
                return mainCamera != null ? (Vector2)mainCamera.transform.position : Vector2.zero;
            }

            Vector2 sum = Vector2.zero;
            int count = 0;

            foreach (var dice in DiceManager.Instance.ActiveDice)
            {
                if (dice != null)
                {
                    sum += (Vector2)dice.transform.position;
                    count++;
                }
            }

            return count > 0 ? sum / count : Vector2.zero;
        }

        /// <summary>
        /// Calculates screen bounds based on camera view.
        /// </summary>
        private void CalculateScreenBounds()
        {
            if (mainCamera == null) return;

            float cameraHeight = mainCamera.orthographicSize;
            float cameraWidth = cameraHeight * mainCamera.aspect;

            // Basic screen bounds with small padding
            minX = mainCamera.transform.position.x - cameraWidth + SCREEN_PADDING;
            maxX = mainCamera.transform.position.x + cameraWidth - SCREEN_PADDING;
            minY = mainCamera.transform.position.y - cameraHeight + SCREEN_PADDING;
            maxY = mainCamera.transform.position.y + cameraHeight - SCREEN_PADDING - 0.5f; // Small space for top UI
        }

        /// <summary>
        /// Checks if a position is inside a UI exclusion zone.
        /// </summary>
        private bool IsInUIZone(Vector3 pos)
        {
            if (mainCamera == null) return false;

            float cameraHeight = mainCamera.orthographicSize;
            float cameraWidth = cameraHeight * mainCamera.aspect;
            float camX = mainCamera.transform.position.x;
            float camY = mainCamera.transform.position.y;

            float screenLeft = camX - cameraWidth;
            float screenTop = camY + cameraHeight;
            float screenBottom = camY - cameraHeight;

            // Check if in left buttons zone (top-left corner)
            bool inLeftZone = pos.x < screenLeft + LEFT_UI_ZONE_WIDTH &&
                              pos.y > screenTop - LEFT_UI_ZONE_TOP;

            // Check if in bottom shop panel zone (center-bottom)
            bool inBottomZone = pos.y < screenBottom + BOTTOM_UI_ZONE_HEIGHT;

            return inLeftZone || inBottomZone;
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

            // Set dice size (smaller dice)
            initialScale = Vector3.one * 0.7f;
            transform.localScale = initialScale;

            // Update collider to match
            if (circleCollider != null)
            {
                circleCollider.radius = 0.5f;
            }
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

            // Add money with floating effect (money added when effect reaches counter)
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddMoneyWithEffect(finalMoney, transform.position, isJackpot);
            }

            // Check for Table Tax bonus coin proc
            if (GameStats.Instance != null && CurrencyManager.Instance != null)
            {
                double tableTaxBonus = GameStats.Instance.CheckTableTaxProc(CurrencyManager.Instance.Money);
                if (tableTaxBonus > 0)
                {
                    // Spawn a separate bonus coin effect with gold color
                    CurrencyManager.Instance.AddMoneyWithEffect(tableTaxBonus, transform.position + Vector3.up * 0.3f, true);

                    // Show floating text for bonus coin
                    if (GameUI.Instance != null)
                    {
                        Color bonusColor = new Color(1f, 0.8f, 0.2f); // Gold color for bonus
                        GameUI.Instance.ShowFloatingText(transform.position + Vector3.up * 0.5f, $"+${GameUI.FormatNumber(tableTaxBonus)} BONUS!", bonusColor);
                    }

                    // Play bonus sound
                    if (Core.AudioManager.Instance != null)
                    {
                        Core.AudioManager.Instance.PlayJackpotSound();
                    }
                }
            }

            // Generate Dark Matter based on face value (only if DM is unlocked after ascending)
            // Base DM = face value (+1 for rolling 1, +2 for rolling 2, etc.)
            // Higher tier dice add bonus DM from their dmPerRoll
            if (DiceManager.Instance != null && DiceManager.Instance.DarkMatterUnlocked)
            {
                // Base DM from face value
                double baseDM = currentFaceValue;

                // Add bonus DM from dice tier (dmPerRoll)
                double tierBonus = data.dmPerRoll * dmMultiplier;
                double dmEarned = baseDM + tierBonus;

                // Apply DM modifiers from GameStats
                if (GameStats.Instance != null)
                {
                    dmEarned = GameStats.Instance.ApplyDarkMatterModifiers(dmEarned);
                }

                if (dmEarned > 0 && CurrencyManager.Instance != null)
                {
                    CurrencyManager.Instance.AddDarkMatterWithEffect(dmEarned, transform.position);

                    // Track for statistics
                    if (PrestigeManager.Instance != null)
                    {
                        PrestigeManager.Instance.TrackDarkMatterEarned(dmEarned);
                    }
                }

                // Notify listeners about DM generation
                OnDarkMatterGenerated?.Invoke(dmEarned);
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

            // Play sound effects
            if (Core.AudioManager.Instance != null)
            {
                if (isJackpot)
                {
                    Core.AudioManager.Instance.PlayJackpotSound();
                }
                else
                {
                    Core.AudioManager.Instance.PlayRollSound();
                }
            }

            // Spawn visual effect
            SpawnRollEffect(isJackpot);

            // Spawn particle effects
            if (Core.VisualEffectsManager.Instance != null)
            {
                if (isJackpot)
                {
                    Core.VisualEffectsManager.Instance.SpawnJackpotEffect(transform.position);
                }
                else
                {
                    Core.VisualEffectsManager.Instance.SpawnRollEffect(transform.position);
                }
            }

            // Screen shake on rolling 6 (jackpot) - Always get camera fresh to ensure it works
            if (isJackpot)
            {
                Camera cam = mainCamera ?? Camera.main;
                if (cam != null)
                {
                    cam.transform.DOKill();
                    cam.transform.DOShakePosition(0.35f, 0.2f, 25, 90f, false, true);
                }
            }

            // Notify listeners
            OnRolled?.Invoke(this, finalMoney, isJackpot);

            return true;
        }

        /// <summary>
        /// Gets a random target position within screen bounds, avoiding UI zones.
        /// </summary>
        private Vector3 GetRandomTargetPosition()
        {
            // Recalculate bounds in case camera changed
            CalculateScreenBounds();

            Vector3 currentPos = transform.position;
            Vector3 newPos;

            // Try to find a valid position that's different from current
            for (int i = 0; i < 20; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(moveDistance * 0.5f, moveDistance);

                newPos = new Vector3(
                    Mathf.Clamp(currentPos.x + Mathf.Cos(angle) * distance, minX, maxX),
                    Mathf.Clamp(currentPos.y + Mathf.Sin(angle) * distance, minY, maxY),
                    0
                );

                // Skip if position is in a UI zone
                if (IsInUIZone(newPos))
                {
                    continue;
                }

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

            // Fallback: pick a random spot within bounds, avoiding UI zones
            for (int i = 0; i < 10; i++)
            {
                newPos = new Vector3(
                    Random.Range(minX, maxX),
                    Random.Range(minY, maxY),
                    0
                );
                if (!IsInUIZone(newPos))
                {
                    return newPos;
                }
            }

            // Last resort: return center of screen
            return mainCamera != null ? mainCamera.transform.position : Vector3.zero;
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

                // Squash on land (relative to initial scale)
                rollSequence.Append(
                    transform.DOScale(new Vector3(initialScale.x * 1.2f, initialScale.y * 0.8f, initialScale.z), 0.04f)
                        .SetEase(Ease.OutQuad)
                );
                rollSequence.Append(
                    transform.DOScale(initialScale, 0.06f)
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

            // Final punch effect (relative to initial scale)
            float scaleAmount = isJackpot ? 0.3f : 0.15f;
            rollSequence.Append(
                transform.DOPunchScale(initialScale * scaleAmount, 0.15f, 6, 0.5f)
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
                transform.localScale = initialScale;
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
