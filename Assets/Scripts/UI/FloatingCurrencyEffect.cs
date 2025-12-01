using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

namespace Incredicer.UI
{
    /// <summary>
    /// Manages floating currency effects that travel from world position to UI counter.
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

        [Header("Prefab")]
        [SerializeField] private GameObject currencyEffectPrefab;

        private Camera mainCamera;
        private RectTransform canvasRect;

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
        }

        /// <summary>
        /// Spawns a floating money effect from world position to the money counter.
        /// </summary>
        public void SpawnMoneyEffect(Vector3 worldPosition, double amount, bool isJackpot = false)
        {
            if (moneyTargetPosition == null || canvas == null) return;

            Color effectColor = isJackpot ? new Color(1f, 0.85f, 0.2f) : new Color(0.4f, 1f, 0.4f);
            string text = $"+${GameUI.FormatNumber(amount)}";

            SpawnEffect(worldPosition, moneyTargetPosition, text, effectColor, amount, true);
        }

        /// <summary>
        /// Spawns a floating dark matter effect from world position to the DM counter.
        /// </summary>
        public void SpawnDarkMatterEffect(Vector3 worldPosition, double amount)
        {
            if (darkMatterTargetPosition == null || canvas == null) return;

            Color effectColor = new Color(0.8f, 0.5f, 1f);
            string text = $"+{GameUI.FormatNumber(amount)} DM";

            SpawnEffect(worldPosition, darkMatterTargetPosition, text, effectColor, amount, false);
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

            // Get target position
            Vector2 endCanvasPos = target.anchoredPosition;

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

            // On complete - trigger currency add and destroy
            seq.OnComplete(() =>
            {
                if (isMoney)
                {
                    OnMoneyReachedCounter?.Invoke(amount);
                }
                else
                {
                    OnDarkMatterReachedCounter?.Invoke(amount);
                }

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
