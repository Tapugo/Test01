using UnityEngine;
using Incredicer.Core;
using Incredicer.UI;
using Incredicer.Overclock;
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
        private Vector3 initialScale = Vector3.one * 0.504f; // 30% smaller than original 0.7f (10% smaller than 0.56)

        // FocusedGravity / PrecisionAim movement
        [Header("Gravity Settings")]
        [SerializeField] private float gravityForce = 0.15f; // Reduced for gentler drift
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

            // FocusedGravity: Dice slowly drift toward the center of the screen
            if (GameStats.Instance.FocusedGravityActive)
            {
                // Get screen center in world coordinates
                Vector2 screenCenter = mainCamera != null ? (Vector2)mainCamera.transform.position : Vector2.zero;
                Vector2 toCenter = screenCenter - (Vector2)currentPos;
                float distance = toCenter.magnitude;

                // Only apply force if outside the "comfort zone" radius
                // Dice will drift inward but stop before clustering at center
                float comfortRadius = 1.5f; // Distance from center where dice stop drifting
                if (distance > comfortRadius)
                {
                    // Very slow drift - use a fraction of the normal gravity force
                    float slowGravityForce = gravityForce * 0.15f;

                    // Gradually reduce force as dice get closer to comfort zone
                    float distanceFactor = Mathf.Clamp01((distance - comfortRadius) / 2f);
                    force += toCenter.normalized * slowGravityForce * distanceFactor;
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
            initialScale = Vector3.one * 0.504f; // 30% smaller than original 0.7f (10% smaller than 0.56)
            transform.localScale = initialScale;

            // Update collider to match
            if (circleCollider != null)
            {
                circleCollider.radius = 0.5f;
            }
        }

        /// <summary>
        /// Sets up Feel feedbacks if not already assigned.
        /// Creates MMF_Player objects for satisfying game feel.
        /// Note: Feedbacks are added via the Feel API at runtime.
        /// </summary>
        private void SetupFeedbacks()
        {
            // Setup Roll Feedback (plays when dice is tapped)
            if (rollFeedback == null)
            {
                GameObject feedbackObj = new GameObject("RollFeedback");
                feedbackObj.transform.SetParent(transform);
                feedbackObj.transform.localPosition = Vector3.zero;
                rollFeedback = feedbackObj.AddComponent<MMF_Player>();
                rollFeedback.InitializationMode = MMF_Player.InitializationModes.Script;

                // Add feedbacks through the MMF_Player's list
                // Create a scale feedback for tap punch effect
                var scaleFeedback = new MoreMountains.Feedbacks.MMF_Scale();
                scaleFeedback.Label = "TapPunch";
                scaleFeedback.AnimateScaleTarget = transform;
                scaleFeedback.RemapCurveZero = 1f;
                scaleFeedback.RemapCurveOne = 1.15f;
                scaleFeedback.AnimateScaleDuration = 0.1f;
                rollFeedback.AddFeedback(scaleFeedback);

                rollFeedback.Initialization();
            }

            // Setup Jackpot Feedback (plays when rolling a 6)
            if (jackpotFeedback == null)
            {
                GameObject jackpotFeedbackObj = new GameObject("JackpotFeedback");
                jackpotFeedbackObj.transform.SetParent(transform);
                jackpotFeedbackObj.transform.localPosition = Vector3.zero;
                jackpotFeedback = jackpotFeedbackObj.AddComponent<MMF_Player>();
                jackpotFeedback.InitializationMode = MMF_Player.InitializationModes.Script;

                // Add intense scale feedback for jackpot
                var jackpotScale = new MoreMountains.Feedbacks.MMF_Scale();
                jackpotScale.Label = "JackpotPunch";
                jackpotScale.AnimateScaleTarget = transform;
                jackpotScale.RemapCurveZero = 1f;
                jackpotScale.RemapCurveOne = 1.4f;
                jackpotScale.AnimateScaleDuration = 0.2f;
                jackpotFeedback.AddFeedback(jackpotScale);

                jackpotFeedback.Initialization();
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

            // Calculate money based on face value * dice tier multiplier + upgrade bonus
            // Higher tier dice (Silver, Gold, etc.) earn SIGNIFICANTLY more money
            // Base reward = face value * basePayout (tier multiplier)
            // With upgrades: (face value * basePayout) + upgradeLevel
            int upgradeBonus = GameStats.Instance != null ? GameStats.Instance.DiceValueUpgradeLevel : 0;
            double tierMultiplier = data.basePayout; // This is the key difference between dice types!
            double baseMoney = (currentFaceValue * tierMultiplier) + upgradeBonus;

            // Apply any global multipliers from dice data
            double finalMoney = baseMoney * moneyMultiplier;

            // Apply overclock multiplier if dice is overclocked
            if (OverclockManager.Instance != null)
            {
                finalMoney *= OverclockManager.Instance.GetPayoutMultiplier(this);
            }

            // Check for jackpot (rolling a 6)
            bool isJackpot = currentFaceValue == 6;
            if (isJackpot && GameStats.Instance != null)
            {
                finalMoney *= 2; // Double reward for rolling 6
            }

            // Apply all money modifiers from GameStats (including Hyperburst!)
            if (GameStats.Instance != null)
            {
                finalMoney = GameStats.Instance.ApplyMoneyModifiers(finalMoney, isManual, isIdle);
            }

            // NOTE: Currency effects are now spawned when dice LANDS (in PlayRollAnimation callback)
            // This ensures coins fly from the dice's final position when result is shown

            // Calculate dark matter earned (will be spawned when dice lands)
            double dmEarned = 0;
            if (DiceManager.Instance != null && DiceManager.Instance.DarkMatterUnlocked)
            {
                // Base DM from face value
                double baseDM = currentFaceValue;

                // Add bonus DM from dice tier (dmPerRoll)
                double tierBonus = data.dmPerRoll * dmMultiplier;
                dmEarned = baseDM + tierBonus;

                // Apply DM modifiers from GameStats
                if (GameStats.Instance != null)
                {
                    dmEarned = GameStats.Instance.ApplyDarkMatterModifiers(dmEarned);
                }
            }

            // Calculate target position for bounce (within screen bounds)
            Vector3 targetPos = GetRandomTargetPosition();

            // Play roll animation (currency effects spawned when dice lands)
            PlayRollAnimation(isJackpot, finalMoney, dmEarned, targetPos);

            // Play roll feedback (start of roll - NOT jackpot effects yet)
            if (rollFeedback != null)
            {
                rollFeedback.PlayFeedbacks();
            }

            // Play roll sound immediately (jackpot sound plays when dice lands)
            if (Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlayRollSound();
            }

            // Spawn roll visual effect at start
            SpawnRollEffect(false); // false = not jackpot effect at start

            // Spawn initial roll particle effect
            if (Core.VisualEffectsManager.Instance != null)
            {
                Core.VisualEffectsManager.Instance.SpawnRollEffect(transform.position);
            }

            // NOTE: Jackpot effects (screen shake, jackpot sound, jackpot particles)
            // are now triggered in PlayRollAnimation when the dice LANDS and shows the 6

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
        /// Currency effects are spawned when the dice lands and shows the result.
        /// </summary>
        private void PlayRollAnimation(bool isJackpot, double moneyEarned, double dmEarned, Vector3 targetPos)
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

            // Final settle - show the result AND trigger jackpot effects when dice lands!
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

                // === SPAWN CURRENCY EFFECTS - coins/gems fly from dice to counters ===
                if (CurrencyManager.Instance != null)
                {
                    // Spawn money coins flying to the counter
                    CurrencyManager.Instance.AddMoneyWithEffect(moneyEarned, transform.position, isJackpot);

                    // Check for Table Tax bonus coin proc
                    if (GameStats.Instance != null)
                    {
                        double tableTaxBonus = GameStats.Instance.CheckTableTaxProc(CurrencyManager.Instance.Money);
                        if (tableTaxBonus > 0)
                        {
                            // Spawn bonus coins with slight delay
                            CurrencyManager.Instance.AddMoneyWithEffect(tableTaxBonus, transform.position + Vector3.up * 0.3f, true);

                            if (GameUI.Instance != null)
                            {
                                Color bonusColor = new Color(1f, 0.8f, 0.2f);
                                GameUI.Instance.ShowFloatingText(transform.position + Vector3.up * 0.5f, $"+${GameUI.FormatNumber(tableTaxBonus)} BONUS!", bonusColor);
                            }

                            if (Core.AudioManager.Instance != null)
                            {
                                Core.AudioManager.Instance.PlayJackpotSound();
                            }
                        }
                    }

                    // Spawn dark matter gems flying to the counter
                    if (dmEarned > 0)
                    {
                        CurrencyManager.Instance.AddDarkMatterWithEffect(dmEarned, transform.position);

                        // Track for statistics
                        if (PrestigeManager.Instance != null)
                        {
                            PrestigeManager.Instance.TrackDarkMatterEarned(dmEarned);
                        }

                        // Notify listeners about DM generation
                        OnDarkMatterGenerated?.Invoke(dmEarned);
                    }
                }

                // === VALUE-BASED EFFECTS - higher numbers = better effects! ===
                SpawnValueBasedEffects(currentFaceValue, isJackpot);
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
        /// Spawns visual and audio effects based on the dice value.
        /// Higher values produce more impressive effects!
        /// </summary>
        private void SpawnValueBasedEffects(int value, bool isJackpot)
        {
            Camera cam = mainCamera ?? Camera.main;

            // Base intensity scales with value (1-6)
            float intensity = value / 6f;

            // Colors that become more vibrant with higher values
            Color[] valueColors = new Color[]
            {
                new Color(0.6f, 0.6f, 0.6f),     // 1 - Gray (muted)
                new Color(0.5f, 0.7f, 1f),       // 2 - Light blue
                new Color(0.4f, 1f, 0.5f),       // 3 - Green
                new Color(1f, 0.85f, 0.3f),      // 4 - Gold
                new Color(1f, 0.5f, 0.8f),       // 5 - Pink/Magenta
                new Color(1f, 0.9f, 0.2f),       // 6 - Bright Gold (Jackpot!)
            };
            Color effectColor = valueColors[Mathf.Clamp(value - 1, 0, 5)];

            // Value 1-2: Subtle effect (small particles)
            if (value >= 1)
            {
                if (Core.VisualEffectsManager.Instance != null)
                {
                    Core.VisualEffectsManager.Instance.SpawnRollEffect(transform.position);
                }
            }

            // Value 3-4: Medium effect (sparkles + scale punch)
            if (value >= 3)
            {
                if (Core.VisualEffectsManager.Instance != null)
                {
                    Core.VisualEffectsManager.Instance.SpawnSparkleEffect(transform.position, effectColor);
                }

                // Small punch scale
                transform.DOPunchScale(initialScale * 0.1f, 0.1f, 4, 0.5f);
            }

            // Value 4: Nice sparkle burst
            if (value >= 4)
            {
                if (Core.VisualEffectsManager.Instance != null)
                {
                    Core.VisualEffectsManager.Instance.SpawnMoneyCollectEffect(transform.position);
                }

                // Color flash
                spriteRenderer.DOColor(effectColor, 0.06f)
                    .SetLoops(2, LoopType.Yoyo)
                    .OnComplete(() => spriteRenderer.color = data != null ? data.tintColor : Color.white);
            }

            // Value 5: Great effect (combo burst + mild shake)
            if (value >= 5)
            {
                if (Core.VisualEffectsManager.Instance != null)
                {
                    Core.VisualEffectsManager.Instance.SpawnComboEffect(transform.position);
                }

                // Mild screen shake
                if (cam != null)
                {
                    cam.transform.DOKill();
                    cam.transform.DOShakePosition(0.15f, 0.08f, 15, 90f, false, true);
                }

                // Play a sound
                if (Core.AudioManager.Instance != null)
                {
                    Core.AudioManager.Instance.PlayRollSound();
                }
            }

            // Value 6: JACKPOT! Maximum effects!
            if (isJackpot)
            {
                // Play jackpot feedback
                if (jackpotFeedback != null)
                {
                    jackpotFeedback.PlayFeedbacks();
                }

                // Play jackpot sound when dice shows 6
                if (Core.AudioManager.Instance != null)
                {
                    Core.AudioManager.Instance.PlayJackpotSound();
                }

                // Spawn jackpot visual effect (radial burst) at the landed position
                SpawnRollEffect(true); // true = jackpot effect

                // Spawn jackpot particle effects at the landed position
                if (Core.VisualEffectsManager.Instance != null)
                {
                    Core.VisualEffectsManager.Instance.SpawnJackpotEffect(transform.position);

                    // Extra celebration sparkles around the dice
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = i * 90f * Mathf.Deg2Rad;
                        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 0.5f;
                        DOVirtual.DelayedCall(i * 0.05f, () =>
                        {
                            Core.VisualEffectsManager.Instance.SpawnSparkleEffect(transform.position + offset, effectColor);
                        });
                    }
                }

                // Screen shake when dice lands on 6!
                if (cam != null)
                {
                    cam.transform.DOKill();
                    cam.transform.DOShakePosition(0.4f, 0.25f, 30, 90f, false, true);
                }

                // Flash screen briefly
                if (Core.VisualEffectsManager.Instance != null)
                {
                    Core.VisualEffectsManager.Instance.FlashScreen(new Color(1f, 0.9f, 0.4f, 0.3f), 0.15f);
                }
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
