using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

namespace Incredicer.UI
{
    /// <summary>
    /// Manages floating currency effects that travel from world position to UI counter.
    /// Spawns coin/gem particle sprites that fly in an arc to the currency counters.
    /// </summary>
    public class FloatingCurrencyEffect : MonoBehaviour
    {
        public static FloatingCurrencyEffect Instance { get; private set; }

        [Header("References")]
        [SerializeField] private RectTransform moneyTargetPosition;
        [SerializeField] private RectTransform darkMatterTargetPosition;
        [SerializeField] private Canvas canvas;

        [Header("Settings")]
        [SerializeField] private float travelDuration = 0.5f;        // Slightly faster for snappier feel
        [SerializeField] private float startScale = 2.8f;            // Bigger initial pop
        [SerializeField] private float endScale = 0.8f;              // Smaller at end
        [SerializeField] private float arcHeight = 140f;             // Higher arc for more drama

        [Header("Particle Settings")]
        [SerializeField] private int particleCount = 8;              // More coins!
        [SerializeField] private float particleSpread = 50f;         // More spread
        [SerializeField] private float particleSize = 100f;          // Bigger coins
        [SerializeField] private float particleStagger = 0.04f;      // Tighter stagger for wave effect

        [Header("Prefab")]
        [SerializeField] private GameObject currencyEffectPrefab;

        private Camera mainCamera;
        private RectTransform canvasRect;

        // Generated sprites for coins and gems
        private Sprite coinSprite;
        private Sprite gemSprite;

        // Events for when currency should be added
        public event Action<double> OnMoneyReachedCounter;
        public event Action<double> OnDarkMatterReachedCounter;

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
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }
            if (canvas != null)
            {
                canvasRect = canvas.GetComponent<RectTransform>();
            }

            // Generate coin and gem sprites
            CreateCoinSprite();
            CreateGemSprite();
        }

        /// <summary>
        /// Creates a simple coin sprite procedurally.
        /// </summary>
        private void CreateCoinSprite()
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];

            Color goldOuter = new Color(0.85f, 0.65f, 0.1f);
            Color goldInner = new Color(1f, 0.85f, 0.3f);
            Color goldHighlight = new Color(1f, 0.95f, 0.6f);

            int center = size / 2;
            int radius = size / 2 - 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));

                    if (dist <= radius)
                    {
                        // Gradient from center to edge
                        float t = dist / radius;
                        Color baseColor = Color.Lerp(goldInner, goldOuter, t * 0.7f);

                        // Add highlight in upper left
                        float highlightDist = Mathf.Sqrt((x - center + 8) * (x - center + 8) + (y - center - 8) * (y - center - 8));
                        if (highlightDist < radius * 0.4f)
                        {
                            float ht = 1f - (highlightDist / (radius * 0.4f));
                            baseColor = Color.Lerp(baseColor, goldHighlight, ht * 0.5f);
                        }

                        // Anti-aliased edge
                        float alpha = Mathf.Clamp01((radius - dist + 1f));
                        baseColor.a = alpha;
                        pixels[y * size + x] = baseColor;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            coinSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
        }

        /// <summary>
        /// Creates a simple gem/crystal sprite procedurally for dark matter.
        /// </summary>
        private void CreateGemSprite()
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];

            Color purpleOuter = new Color(0.5f, 0.2f, 0.7f);
            Color purpleInner = new Color(0.8f, 0.5f, 1f);
            Color purpleHighlight = new Color(0.95f, 0.8f, 1f);

            int center = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Diamond shape
                    int dx = Mathf.Abs(x - center);
                    int dy = Mathf.Abs(y - center);
                    float diamondDist = dx + dy;
                    float maxDist = size / 2 - 2;

                    if (diamondDist <= maxDist)
                    {
                        float t = diamondDist / maxDist;
                        Color baseColor = Color.Lerp(purpleInner, purpleOuter, t * 0.8f);

                        // Add sparkle highlight
                        float highlightDist = Mathf.Sqrt((x - center + 6) * (x - center + 6) + (y - center - 6) * (y - center - 6));
                        if (highlightDist < maxDist * 0.3f)
                        {
                            float ht = 1f - (highlightDist / (maxDist * 0.3f));
                            baseColor = Color.Lerp(baseColor, purpleHighlight, ht * 0.6f);
                        }

                        // Anti-aliased edge
                        float alpha = Mathf.Clamp01((maxDist - diamondDist + 1.5f));
                        baseColor.a = alpha;
                        pixels[y * size + x] = baseColor;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            gemSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
        }

        /// <summary>
        /// Spawns a floating money effect from world position to the money counter.
        /// </summary>
        public void SpawnMoneyEffect(Vector3 worldPosition, double amount, bool isJackpot = false)
        {
            if (moneyTargetPosition == null || canvas == null) return;

            // Don't spawn effects when a popup is open
            if (Core.PopupManager.Instance != null && Core.PopupManager.Instance.IsAnyPopupOpen)
            {
                // Still add the money directly since we're skipping the visual effect
                Core.CurrencyManager.Instance?.AddMoneyDirect(amount);
                return;
            }

            Color effectColor = isJackpot ? new Color(1f, 0.85f, 0.2f) : new Color(0.4f, 1f, 0.4f);
            string text = $"+${GameUI.FormatNumber(amount)}";

            // Spawn text effect
            SpawnEffect(worldPosition, moneyTargetPosition, text, effectColor, amount, true);

            // Calculate coin count based on amount - 1 coin per unit, clamped to reasonable range
            // For small amounts (1-10), spawn that many coins
            // For larger amounts, use logarithmic scaling to keep it manageable
            int coins = CalculateParticleCount(amount);
            if (isJackpot) coins = Mathf.Min(coins + 3, 20); // Bonus coins for jackpot, max 20
            SpawnParticleEffects(worldPosition, moneyTargetPosition, coinSprite, coins, true, amount);
        }

        /// <summary>
        /// Spawns a floating dark matter effect from world position to the DM counter.
        /// </summary>
        public void SpawnDarkMatterEffect(Vector3 worldPosition, double amount)
        {
            if (darkMatterTargetPosition == null || canvas == null) return;

            // Don't spawn effects when a popup is open
            if (Core.PopupManager.Instance != null && Core.PopupManager.Instance.IsAnyPopupOpen)
            {
                // Still add the dark matter directly since we're skipping the visual effect
                Core.CurrencyManager.Instance?.AddDarkMatterDirect(amount);
                return;
            }

            Color effectColor = new Color(0.8f, 0.5f, 1f);
            string text = $"+{GameUI.FormatNumber(amount)} DM";

            // Spawn text effect
            SpawnEffect(worldPosition, darkMatterTargetPosition, text, effectColor, amount, false);

            // Calculate gem count based on amount - dark matter is rarer so use direct count for small amounts
            int gems = CalculateDarkMatterParticleCount(amount);
            SpawnParticleEffects(worldPosition, darkMatterTargetPosition, gemSprite, gems, false, amount);
        }

        /// <summary>
        /// Calculates the number of coin particles to spawn based on money amount.
        /// Uses the dice face value (1-6) as the base, scaling with multipliers.
        /// </summary>
        private int CalculateParticleCount(double amount)
        {
            // For very small amounts (face value 1-6 with 1x multiplier = 1-6 coins)
            if (amount <= 6) return Mathf.Max(1, (int)amount);

            // For medium amounts, use square root scaling
            // This gives a nice visual representation without too many particles
            // amount 10 -> ~3 coins, 100 -> ~10 coins, 1000 -> ~15 coins
            int count = Mathf.RoundToInt(Mathf.Sqrt((float)amount) * 0.5f);

            // Clamp between 1 and 15 to keep performance reasonable
            return Mathf.Clamp(count, 1, 15);
        }

        /// <summary>
        /// Calculates the number of gem particles to spawn based on dark matter amount.
        /// Dark matter amounts are typically smaller (0.05 to 25 per roll).
        /// </summary>
        private int CalculateDarkMatterParticleCount(double amount)
        {
            // For fractional amounts (< 1), always show at least 1 gem
            if (amount < 1) return 1;

            // For small whole amounts (1-10), spawn that many gems
            if (amount <= 10) return Mathf.Max(1, (int)amount);

            // For larger amounts, scale more gradually
            // amount 25 -> ~8 gems, 100 -> ~12 gems
            int count = Mathf.RoundToInt(5f + Mathf.Log10((float)amount) * 4f);

            // Clamp between 1 and 15
            return Mathf.Clamp(count, 1, 15);
        }

        /// <summary>
        /// Spawns multiple coin/gem particles that fly to the target.
        /// </summary>
        private void SpawnParticleEffects(Vector3 worldPosition, RectTransform target, Sprite sprite, int count, bool isMoney, double amount)
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null || canvasRect == null || sprite == null) return;

            // Convert world position to canvas position
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
            Vector2 startCanvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera, out startCanvasPos);

            // Convert target position to canvas local coordinates
            // We need to use the target's world position, not anchoredPosition (which is relative to parent)
            Vector3 targetWorldPos = target.position;
            Vector3 targetScreenPos = RectTransformUtility.WorldToScreenPoint(
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                targetWorldPos);
            Vector2 endCanvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, targetScreenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out endCanvasPos);

            for (int i = 0; i < count; i++)
            {
                float delay = i * particleStagger;
                SpawnSingleParticle(startCanvasPos, endCanvasPos, sprite, delay, i == count - 1, isMoney, amount);
            }
        }

        /// <summary>
        /// Spawns a single particle that flies to the target.
        /// </summary>
        private void SpawnSingleParticle(Vector2 startPos, Vector2 endPos, Sprite sprite, float delay, bool isLast, bool isMoney, double amount)
        {
            // Create particle object
            GameObject particleObj = new GameObject("CurrencyParticle");
            particleObj.transform.SetParent(canvas.transform, false);

            RectTransform rt = particleObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(particleSize, particleSize);

            // Randomize start position slightly
            Vector2 randomOffset = new Vector2(
                UnityEngine.Random.Range(-particleSpread, particleSpread),
                UnityEngine.Random.Range(-particleSpread, particleSpread)
            );
            Vector2 actualStart = startPos + randomOffset;
            rt.anchoredPosition = actualStart;
            rt.localScale = Vector3.zero;

            // Add image
            Image img = particleObj.AddComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;

            // Calculate arc with randomized height
            float randomArc = arcHeight * UnityEngine.Random.Range(0.7f, 1.3f);
            Vector2 midPoint = (actualStart + endPos) / 2f;
            midPoint.y += randomArc;

            // Randomize duration slightly
            float duration = travelDuration * UnityEngine.Random.Range(0.85f, 1.15f);

            // Create animation sequence
            Sequence seq = DOTween.Sequence();

            // Delay before starting
            seq.AppendInterval(delay);

            // Pop in with bounce - juicier!
            seq.Append(rt.DOScale(1.3f, 0.08f).SetEase(Ease.OutBack, 2f));
            seq.Append(rt.DOScale(1f, 0.05f).SetEase(Ease.InQuad));

            // Add faster rotation for visual interest
            float randomRotation = UnityEngine.Random.Range(-360f, 360f);
            seq.Join(rt.DORotate(new Vector3(0, 0, randomRotation), duration * 0.8f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad));

            // Move along bezier curve with easing for more satisfying arc
            seq.Append(DOTween.To(() => 0f, t =>
            {
                float oneMinusT = 1f - t;
                Vector2 pos = oneMinusT * oneMinusT * actualStart +
                              2f * oneMinusT * t * midPoint +
                              t * t * endPos;
                rt.anchoredPosition = pos;
            }, 1f, duration).SetEase(Ease.InOutQuad)); // Smoother acceleration

            // Scale up then down - like it's coming toward camera then away
            // Peak scale at 50% of travel, then shrink to end scale
            float peakScale = 1.8f;
            seq.Join(DOTween.To(() => 0f, t =>
            {
                // Use sine curve: scale up to peak at t=0.5, then back down
                float scaleT = Mathf.Sin(t * Mathf.PI); // 0 -> 1 -> 0
                float currentScale = Mathf.Lerp(1f, peakScale, scaleT * 0.7f); // Scale up to ~1.56x at midpoint
                // Also blend toward end scale in the second half
                if (t > 0.5f)
                {
                    float shrinkT = (t - 0.5f) * 2f; // 0 to 1 in second half
                    currentScale = Mathf.Lerp(currentScale, endScale * 0.6f, shrinkT * shrinkT);
                }
                rt.localScale = Vector3.one * currentScale;
            }, 1f, duration).SetEase(Ease.Linear));

            // Fade out at end - quicker fade
            seq.Join(img.DOFade(0f, duration * 0.15f).SetDelay(duration * 0.85f));

            // On complete
            seq.OnComplete(() =>
            {
                // Only trigger currency add on the last particle
                if (isLast)
                {
                    if (isMoney)
                    {
                        OnMoneyReachedCounter?.Invoke(amount);
                    }
                    else
                    {
                        OnDarkMatterReachedCounter?.Invoke(amount);
                    }
                }

                Destroy(particleObj);
            });
        }

        private void SpawnEffect(Vector3 worldPosition, RectTransform target, string text, Color color, double amount, bool isMoney)
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null || canvasRect == null) return;

            // Create effect object
            GameObject effectObj = new GameObject("CurrencyEffect");
            effectObj.transform.SetParent(canvas.transform, false);

            RectTransform rt = effectObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 120);

            // Convert world position to screen position
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);

            // Convert screen position to canvas local position
            Vector2 startCanvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera, out startCanvasPos);

            rt.anchoredPosition = startCanvasPos;
            rt.localScale = Vector3.one * startScale;

            // Add text
            TextMeshProUGUI tmp = effectObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 72;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.raycastTarget = false;

            // Try to use the same font as GameUI
            if (GameUI.Instance != null)
            {
                var existingText = GameUI.Instance.GetComponentInChildren<TextMeshProUGUI>();
                if (existingText != null && existingText.font != null)
                {
                    tmp.font = existingText.font;
                }
            }

            // Apply black outline for readability
            GameUI.ApplyTextOutline(tmp);

            // Get target position - convert world position to canvas local coordinates
            Vector3 targetWorldPos = target.position;
            Vector3 targetScreenPos = RectTransformUtility.WorldToScreenPoint(
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                targetWorldPos);
            Vector2 endCanvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, targetScreenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out endCanvasPos);

            // Calculate arc control point
            Vector2 midPoint = (startCanvasPos + endCanvasPos) / 2f;
            midPoint.y += arcHeight;

            // Create the animation sequence
            Sequence seq = DOTween.Sequence();

            // Animate position along a bezier-like curve using custom path
            float duration = travelDuration;

            // Initial pop - JUICIER with multiple bounces!
            seq.Append(rt.DOScale(startScale * 1.5f, 0.08f).SetEase(Ease.OutBack, 2.5f));
            seq.Append(rt.DOScale(startScale * 0.9f, 0.04f).SetEase(Ease.InQuad));
            seq.Append(rt.DOScale(startScale * 1.1f, 0.05f).SetEase(Ease.OutQuad));
            seq.Append(rt.DOScale(startScale, 0.03f).SetEase(Ease.InOutQuad));

            // Move to target with arc - smoother motion
            seq.Append(DOTween.To(() => 0f, t =>
            {
                // Quadratic bezier curve
                float oneMinusT = 1f - t;
                Vector2 pos = oneMinusT * oneMinusT * startCanvasPos +
                              2f * oneMinusT * t * midPoint +
                              t * t * endCanvasPos;
                rt.anchoredPosition = pos;
            }, 1f, duration).SetEase(Ease.InOutQuad));

            // Shrink as it travels - more dramatic
            seq.Join(rt.DOScale(endScale * 0.7f, duration).SetEase(Ease.InQuad));

            // Fade out near the end - snappier
            seq.Join(tmp.DOFade(0f, duration * 0.2f).SetDelay(duration * 0.8f));

            // On complete - just destroy (particles now handle currency add)
            seq.OnComplete(() =>
            {
                // Note: Currency is now added by the particle effects, not the text
                // This prevents double-adding
                Destroy(effectObj);
            });
        }

        /// <summary>
        /// Sets the target position for money effects.
        /// </summary>
        public void SetMoneyTarget(RectTransform target)
        {
            moneyTargetPosition = target;
        }

        /// <summary>
        /// Sets the target position for dark matter effects.
        /// </summary>
        public void SetDarkMatterTarget(RectTransform target)
        {
            darkMatterTargetPosition = target;
        }
    }
}
