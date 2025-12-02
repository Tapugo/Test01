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
        [SerializeField] private float travelDuration = 0.6f;
        [SerializeField] private float startScale = 1.2f;
        [SerializeField] private float endScale = 0.5f;
        [SerializeField] private float arcHeight = 100f;

        [Header("Particle Settings")]
        [SerializeField] private int particleCount = 5;
        [SerializeField] private float particleSpread = 30f;
        [SerializeField] private float particleSize = 40f;
        [SerializeField] private float particleStagger = 0.05f;

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

            Color effectColor = isJackpot ? new Color(1f, 0.85f, 0.2f) : new Color(0.4f, 1f, 0.4f);
            string text = $"+${GameUI.FormatNumber(amount)}";

            // Spawn text effect
            SpawnEffect(worldPosition, moneyTargetPosition, text, effectColor, amount, true);

            // Spawn coin particle effects
            int coins = isJackpot ? particleCount + 3 : particleCount;
            SpawnParticleEffects(worldPosition, moneyTargetPosition, coinSprite, coins, true, amount);
        }

        /// <summary>
        /// Spawns a floating dark matter effect from world position to the DM counter.
        /// </summary>
        public void SpawnDarkMatterEffect(Vector3 worldPosition, double amount)
        {
            if (darkMatterTargetPosition == null || canvas == null) return;

            Color effectColor = new Color(0.8f, 0.5f, 1f);
            string text = $"+{GameUI.FormatNumber(amount)} DM";

            // Spawn text effect
            SpawnEffect(worldPosition, darkMatterTargetPosition, text, effectColor, amount, false);

            // Spawn gem particle effects
            SpawnParticleEffects(worldPosition, darkMatterTargetPosition, gemSprite, particleCount, false, amount);
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

            // Pop in
            seq.Append(rt.DOScale(1f, 0.1f).SetEase(Ease.OutBack));

            // Add rotation for visual interest
            float randomRotation = UnityEngine.Random.Range(-180f, 180f);
            seq.Join(rt.DORotate(new Vector3(0, 0, randomRotation), duration, RotateMode.FastBeyond360));

            // Move along bezier curve
            seq.Append(DOTween.To(() => 0f, t =>
            {
                float oneMinusT = 1f - t;
                Vector2 pos = oneMinusT * oneMinusT * actualStart +
                              2f * oneMinusT * t * midPoint +
                              t * t * endPos;
                rt.anchoredPosition = pos;
            }, 1f, duration).SetEase(Ease.InQuad));

            // Shrink as it travels
            seq.Join(rt.DOScale(endScale * 0.8f, duration).SetEase(Ease.InQuad));

            // Fade out at end
            seq.Join(img.DOFade(0f, duration * 0.2f).SetDelay(duration * 0.8f));

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
            rt.sizeDelta = new Vector2(200, 60);

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
            tmp.fontSize = 36;
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

            // Initial pop
            seq.Append(rt.DOScale(startScale * 1.3f, 0.1f).SetEase(Ease.OutBack));
            seq.Append(rt.DOScale(startScale, 0.05f));

            // Move to target with arc
            seq.Append(DOTween.To(() => 0f, t =>
            {
                // Quadratic bezier curve
                float oneMinusT = 1f - t;
                Vector2 pos = oneMinusT * oneMinusT * startCanvasPos +
                              2f * oneMinusT * t * midPoint +
                              t * t * endCanvasPos;
                rt.anchoredPosition = pos;
            }, 1f, duration).SetEase(Ease.InOutQuad));

            // Shrink as it travels
            seq.Join(rt.DOScale(endScale, duration).SetEase(Ease.InQuad));

            // Fade out near the end
            seq.Join(tmp.DOFade(0f, duration * 0.3f).SetDelay(duration * 0.7f));

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
