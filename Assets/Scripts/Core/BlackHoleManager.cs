using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Incredicer.UI;
using Incredicer.Dice;

namespace Incredicer.Core
{
    // Alias to avoid namespace conflict with Dice class
    using DiceObject = Incredicer.Dice.Dice;

    /// <summary>
    /// Manages randomly spawning black holes that move across the screen and swallow dice.
    /// Players can roll dice in the hole's path to save them from being destroyed.
    /// </summary>
    public class BlackHoleManager : MonoBehaviour
    {
        public static BlackHoleManager Instance { get; private set; }

        [Header("Timing")]
        [SerializeField] private float minSpawnInterval = 30f;
        [SerializeField] private float maxSpawnInterval = 90f;
        [SerializeField] private float traverseDuration = 8f; // Time to cross the screen

        [Header("Visual")]
        [SerializeField] private float holeSize = 1.0f; // World units - smaller hole
        [SerializeField] private float pulseSpeed = 1.5f;
        [SerializeField] private float pulseAmount = 1.1f;
        [SerializeField] private float rotationSpeed = 45f;
        [SerializeField] private Color holeColor = new Color(0.1f, 0f, 0.2f, 0.95f);
        [SerializeField] private Color ringColor = new Color(0.5f, 0.2f, 0.8f, 0.8f);

        [Header("Dice Swallowing")]
        [SerializeField] private float swallowRadius = 0.5f; // Distance at which dice start getting pulled (reduced)
        [SerializeField] private float destroyRadius = 0.2f; // Distance at which dice are destroyed
        [SerializeField] private float shrinkDuration = 0.5f; // How long dice take to shrink and fall in
        [SerializeField] private float pullStrength = 1.0f; // How strongly dice are pulled toward hole (reduced)

        [Header("Warning")]
        [SerializeField] private float warningDuration = 2f; // Warning before hole appears
        [SerializeField] private Color warningColor = new Color(1f, 0.3f, 0.3f, 0.6f);

        // Runtime state
        private GameObject currentHole;
        private bool isHoleActive = false;
        private float nextSpawnTime;
        private Coroutine spawnCoroutine;
        private Coroutine holeUpdateCoroutine;
        private HashSet<DiceObject> diceBeingSwallowed = new HashSet<DiceObject>();
        private Camera mainCamera;

        // Screen bounds
        private float screenMinX, screenMaxX, screenMinY, screenMaxY;

        // Events
        public event Action<int> OnDiceSwallowed; // Count of dice swallowed

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
            CalculateScreenBounds();
            ScheduleNextSpawn();
            spawnCoroutine = StartCoroutine(SpawnLoop());
        }

        private void OnDestroy()
        {
            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);
            if (holeUpdateCoroutine != null)
                StopCoroutine(holeUpdateCoroutine);
        }

        private void CalculateScreenBounds()
        {
            if (mainCamera == null) return;

            float halfHeight = mainCamera.orthographicSize;
            float halfWidth = halfHeight * mainCamera.aspect;

            screenMinX = mainCamera.transform.position.x - halfWidth;
            screenMaxX = mainCamera.transform.position.x + halfWidth;
            screenMinY = mainCamera.transform.position.y - halfHeight;
            screenMaxY = mainCamera.transform.position.y + halfHeight;
        }

        private void ScheduleNextSpawn()
        {
            nextSpawnTime = Time.time + UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval);
        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (!isHoleActive && Time.time >= nextSpawnTime)
                {
                    // Check if any major UI is open
                    if (!IsAnyMajorUIOpen() && HasEnoughDice())
                    {
                        yield return StartCoroutine(SpawnBlackHole());
                    }
                    ScheduleNextSpawn();
                }
            }
        }

        private bool IsAnyMajorUIOpen()
        {
            if (DiceShopUI.Instance != null && DiceShopUI.Instance.IsOpen) return true;
            if (MainMenuUI.Instance != null && MainMenuUI.Instance.IsMenuOpen) return true;
            return false;
        }

        private bool HasEnoughDice()
        {
            // Only spawn if player has more than 1 dice (don't take their last one)
            return DiceManager.Instance != null && DiceManager.Instance.ActiveDice.Count > 1;
        }

        /// <summary>
        /// Spawns a black hole that traverses the screen.
        /// </summary>
        private IEnumerator SpawnBlackHole()
        {
            isHoleActive = true;

            // Calculate start and end positions (random edges)
            Vector2 startPos, endPos;
            CalculateTraversePath(out startPos, out endPos);

            // Show warning indicator first
            yield return StartCoroutine(ShowWarning(startPos, endPos));

            // Create the black hole
            currentHole = CreateHoleVisual();
            currentHole.transform.position = new Vector3(startPos.x, startPos.y, 0);

            // Play warning sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClickSound();
            }

            // Start the hole update coroutine to check for dice
            holeUpdateCoroutine = StartCoroutine(UpdateHole());

            // Animate hole moving across screen
            currentHole.transform.DOMove(new Vector3(endPos.x, endPos.y, 0), traverseDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    DestroyHole();
                });

            Debug.Log("[BlackHole] Spawned black hole!");
        }

        private void CalculateTraversePath(out Vector2 startPos, out Vector2 endPos)
        {
            float padding = holeSize;

            // Randomly choose entry edge (0=left, 1=right, 2=top, 3=bottom)
            int entryEdge = UnityEngine.Random.Range(0, 4);
            int exitEdge;

            // Exit should be opposite edge to ensure path goes through middle
            exitEdge = (entryEdge + 2) % 4;

            startPos = GetEdgePosition(entryEdge, padding, true);
            endPos = GetEdgePosition(exitEdge, padding, true);
        }

        private Vector2 GetEdgePosition(int edge, float padding, bool biasTowardCenter = false)
        {
            // When biasTowardCenter is true, positions are closer to the middle of each edge
            float centerBias = biasTowardCenter ? 0.3f : 0f; // 30% toward center

            float screenCenterX = (screenMinX + screenMaxX) / 2f;
            float screenCenterY = (screenMinY + screenMaxY) / 2f;
            float screenWidth = screenMaxX - screenMinX;
            float screenHeight = screenMaxY - screenMinY;

            // Reduced range for edge positions (middle 40% of edge instead of full edge)
            float rangeMultiplier = biasTowardCenter ? 0.4f : 1f;

            switch (edge)
            {
                case 0: // Left
                    float leftY = screenCenterY + UnityEngine.Random.Range(-screenHeight * rangeMultiplier / 2f, screenHeight * rangeMultiplier / 2f);
                    return new Vector2(screenMinX - padding, leftY);
                case 1: // Right
                    float rightY = screenCenterY + UnityEngine.Random.Range(-screenHeight * rangeMultiplier / 2f, screenHeight * rangeMultiplier / 2f);
                    return new Vector2(screenMaxX + padding, rightY);
                case 2: // Top
                    float topX = screenCenterX + UnityEngine.Random.Range(-screenWidth * rangeMultiplier / 2f, screenWidth * rangeMultiplier / 2f);
                    return new Vector2(topX, screenMaxY + padding);
                case 3: // Bottom
                default:
                    float bottomX = screenCenterX + UnityEngine.Random.Range(-screenWidth * rangeMultiplier / 2f, screenWidth * rangeMultiplier / 2f);
                    return new Vector2(bottomX, screenMinY - padding);
            }
        }

        private IEnumerator ShowWarning(Vector2 startPos, Vector2 endPos)
        {
            // Create warning line/indicator
            GameObject warning = new GameObject("BlackHoleWarning");

            // Create a line renderer to show the path
            LineRenderer line = warning.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, new Vector3(startPos.x, startPos.y, 0));
            line.SetPosition(1, new Vector3(endPos.x, endPos.y, 0));
            line.startWidth = 0.3f;
            line.endWidth = 0.3f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = warningColor;
            line.endColor = warningColor;
            line.sortingOrder = 90;

            // Create warning banner UI
            GameObject bannerObj = CreateWarningBanner();

            // Pulse the warning
            float elapsed = 0;
            while (elapsed < warningDuration)
            {
                float alpha = Mathf.PingPong(elapsed * 4f, 1f) * 0.6f;
                Color c = warningColor;
                c.a = alpha;
                line.startColor = c;
                line.endColor = c;
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Fade out banner
            if (bannerObj != null)
            {
                CanvasGroup bannerCG = bannerObj.GetComponent<CanvasGroup>();
                if (bannerCG != null)
                {
                    bannerCG.DOFade(0f, 0.3f).OnComplete(() => Destroy(bannerObj));
                }
                else
                {
                    Destroy(bannerObj);
                }
            }

            Destroy(warning);
        }

        private GameObject CreateWarningBanner()
        {
            // Find or create canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return null;

            // Create banner container
            GameObject banner = new GameObject("BlackHoleWarningBanner");
            banner.transform.SetParent(canvas.transform, false);

            RectTransform bannerRect = banner.AddComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0, 0.75f);
            bannerRect.anchorMax = new Vector2(1, 0.85f);
            bannerRect.offsetMin = Vector2.zero;
            bannerRect.offsetMax = Vector2.zero;

            // Add canvas group for fading
            CanvasGroup canvasGroup = banner.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            // Add background
            Image bgImage = banner.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0f, 0.15f, 0.9f);

            // Add warning text
            GameObject textObj = new GameObject("WarningText");
            textObj.transform.SetParent(banner.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 5);
            textRect.offsetMax = new Vector2(-20, -5);

            TMPro.TextMeshProUGUI text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = "⚠ ATTENTION! BLACK HOLE APPROACHING! ⚠";
            text.fontSize = 32;
            text.fontStyle = TMPro.FontStyles.Bold;
            text.color = new Color(1f, 0.3f, 0.3f);
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = 18;
            text.fontSizeMax = 36;

            // Apply shared font if available
            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                text.font = GameUI.Instance.SharedFont;
            }

            // Ensure banner is on top
            Canvas bannerCanvas = banner.AddComponent<Canvas>();
            bannerCanvas.overrideSorting = true;
            bannerCanvas.sortingOrder = 300;
            banner.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Fade in
            canvasGroup.DOFade(1f, 0.3f);

            // Pulse animation on text
            textObj.transform.DOScale(1.05f, 0.3f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            return banner;
        }

        private GameObject CreateHoleVisual()
        {
            GameObject hole = new GameObject("BlackHole");

            // Create outer ring (swirl effect)
            GameObject outerRing = new GameObject("OuterRing");
            outerRing.transform.SetParent(hole.transform);
            outerRing.transform.localPosition = Vector3.zero;

            SpriteRenderer outerRenderer = outerRing.AddComponent<SpriteRenderer>();
            outerRenderer.sprite = CreateCircleSprite(64);
            outerRenderer.color = ringColor;
            outerRenderer.sortingOrder = 95;
            outerRing.transform.localScale = Vector3.one * holeSize * 1.3f;

            // Create inner hole (dark center)
            GameObject innerHole = new GameObject("InnerHole");
            innerHole.transform.SetParent(hole.transform);
            innerHole.transform.localPosition = Vector3.zero;

            SpriteRenderer innerRenderer = innerHole.AddComponent<SpriteRenderer>();
            innerRenderer.sprite = CreateCircleSprite(64);
            innerRenderer.color = holeColor;
            innerRenderer.sortingOrder = 96;
            innerHole.transform.localScale = Vector3.one * holeSize;

            // Create swirl lines
            for (int i = 0; i < 4; i++)
            {
                GameObject swirl = new GameObject($"Swirl{i}");
                swirl.transform.SetParent(outerRing.transform);
                swirl.transform.localPosition = Vector3.zero;
                swirl.transform.localRotation = Quaternion.Euler(0, 0, i * 90f);

                SpriteRenderer swirlRenderer = swirl.AddComponent<SpriteRenderer>();
                swirlRenderer.sprite = CreateSwirlSprite();
                swirlRenderer.color = new Color(ringColor.r, ringColor.g, ringColor.b, 0.5f);
                swirlRenderer.sortingOrder = 94;
                swirl.transform.localScale = Vector3.one;
            }

            // Animate rotation
            outerRing.transform.DORotate(new Vector3(0, 0, -360f), 360f / rotationSpeed, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);

            // Animate pulsing
            hole.transform.DOScale(Vector3.one * pulseAmount, 1f / pulseSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            return hole;
        }

        private Sprite CreateCircleSprite(int resolution)
        {
            Texture2D texture = new Texture2D(resolution, resolution);
            Color[] pixels = new Color[resolution * resolution];
            float center = resolution / 2f;
            float radius = resolution / 2f - 1;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= radius)
                    {
                        float edge = 1f - Mathf.Clamp01((radius - dist) / 3f);
                        pixels[y * resolution + x] = new Color(1, 1, 1, 1 - edge * 0.5f);
                    }
                    else
                    {
                        pixels[y * resolution + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
        }

        private Sprite CreateSwirlSprite()
        {
            int resolution = 64;
            Texture2D texture = new Texture2D(resolution, resolution);
            Color[] pixels = new Color[resolution * resolution];
            float center = resolution / 2f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);

                    // Create spiral pattern
                    float spiral = Mathf.Sin(angle * 2f + dist * 0.2f);
                    float alpha = Mathf.Clamp01(spiral) * Mathf.Clamp01(1 - dist / center);

                    pixels[y * resolution + x] = new Color(1, 1, 1, alpha * 0.5f);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
        }

        private IEnumerator UpdateHole()
        {
            int diceSwallowedCount = 0;

            while (currentHole != null && isHoleActive)
            {
                Vector2 holePos = currentHole.transform.position;

                // Check all active dice
                if (DiceManager.Instance != null)
                {
                    var activeDice = DiceManager.Instance.ActiveDice;

                    foreach (var dice in activeDice)
                    {
                        if (dice == null || diceBeingSwallowed.Contains(dice)) continue;

                        Vector2 dicePos = dice.transform.position;
                        float distance = Vector2.Distance(holePos, dicePos);

                        // Check if dice is within destroy radius
                        if (distance < destroyRadius * holeSize)
                        {
                            // Start swallowing this dice
                            diceBeingSwallowed.Add(dice);
                            StartCoroutine(SwallowDice(dice));
                            diceSwallowedCount++;
                        }
                        // Check if dice is within pull radius (pull toward hole)
                        else if (distance < swallowRadius * holeSize)
                        {
                            // Pull dice toward hole
                            Vector2 direction = (holePos - dicePos).normalized;
                            float pullForce = (1f - distance / (swallowRadius * holeSize)) * pullStrength;
                            dice.transform.position += (Vector3)(direction * pullForce * Time.deltaTime);
                        }
                    }
                }

                yield return null;
            }

            // Fire event with count
            if (diceSwallowedCount > 0)
            {
                OnDiceSwallowed?.Invoke(diceSwallowedCount);
            }
        }

        private IEnumerator SwallowDice(DiceObject dice)
        {
            if (dice == null) yield break;

            Vector3 originalScale = dice.transform.localScale;
            Vector3 holePos = currentHole != null ? currentHole.transform.position : dice.transform.position;

            // Play swallow effect
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClickSound();
            }

            // Haptic feedback
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.MediumHaptic();
            }

            // Animate dice shrinking and moving toward hole center
            float elapsed = 0;
            while (elapsed < shrinkDuration && dice != null)
            {
                float t = elapsed / shrinkDuration;

                // Shrink scale (ease in for acceleration effect)
                float scale = Mathf.Lerp(1f, 0f, Mathf.Pow(t, 2f));
                dice.transform.localScale = originalScale * scale;

                // Move toward hole center
                if (currentHole != null)
                {
                    holePos = currentHole.transform.position;
                }
                dice.transform.position = Vector3.Lerp(dice.transform.position, holePos, t * 0.5f);

                // Spin faster as it falls in
                dice.transform.Rotate(0, 0, 360f * Time.deltaTime * (1f + t * 3f));

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Remove dice from game
            if (dice != null && DiceManager.Instance != null)
            {
                diceBeingSwallowed.Remove(dice);
                DiceManager.Instance.RemoveDice(dice);
                Debug.Log("[BlackHole] Swallowed a dice!");
            }
        }

        private void DestroyHole()
        {
            if (currentHole != null)
            {
                // Fade out animation
                currentHole.transform.DOScale(Vector3.zero, 0.5f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        if (currentHole != null)
                        {
                            Destroy(currentHole);
                            currentHole = null;
                        }
                    });
            }

            if (holeUpdateCoroutine != null)
            {
                StopCoroutine(holeUpdateCoroutine);
                holeUpdateCoroutine = null;
            }

            isHoleActive = false;
            diceBeingSwallowed.Clear();

            Debug.Log("[BlackHole] Black hole closed!");
        }

        /// <summary>
        /// Force spawn a black hole (for testing).
        /// </summary>
        public void ForceSpawn()
        {
            if (!isHoleActive)
            {
                StartCoroutine(SpawnBlackHole());
            }
        }
    }
}
