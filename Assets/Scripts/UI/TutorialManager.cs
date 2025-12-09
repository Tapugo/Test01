using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.Dice;

namespace Incredicer.UI
{
    /// <summary>
    /// Manages the first-time player tutorial.
    /// Shows an animated hand tapping on the dice with instructions.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float handTapInterval = 1.2f;
        [SerializeField] private float handMoveDistance = 40f;
        [SerializeField] private float handSize = 280f;

        // Tutorial state
        private const string TUTORIAL_COMPLETED_KEY = "TutorialCompleted";
        private bool tutorialActive = false;
        private GameObject tutorialPanel;
        private RectTransform handTransform;
        private TextMeshProUGUI instructionText;
        private Sequence tapAnimation;
        private Canvas canvas;

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
            // Find the main canvas
            canvas = FindObjectOfType<Canvas>();

            // Check if tutorial should be shown
            if (!HasCompletedTutorial())
            {
                // Delay slightly to ensure everything is initialized
                DOVirtual.DelayedCall(1f, ShowTutorial);
            }
        }

        /// <summary>
        /// Ensures a TutorialManager exists in the scene.
        /// Call this from other managers during initialization.
        /// </summary>
        public static void EnsureExists()
        {
            if (Instance != null) return;

            var existing = FindObjectOfType<TutorialManager>();
            if (existing != null) return;

            GameObject tutorialObj = new GameObject("TutorialManager");
            tutorialObj.AddComponent<TutorialManager>();
            Debug.Log("[TutorialManager] Auto-created TutorialManager");
        }

        /// <summary>
        /// Checks if the player has already completed the tutorial.
        /// </summary>
        public bool HasCompletedTutorial()
        {
            return PlayerPrefs.GetInt(TUTORIAL_COMPLETED_KEY, 0) == 1;
        }

        /// <summary>
        /// Marks the tutorial as completed.
        /// </summary>
        public void CompleteTutorial()
        {
            PlayerPrefs.SetInt(TUTORIAL_COMPLETED_KEY, 1);
            PlayerPrefs.Save();
            HideTutorial();
        }

        /// <summary>
        /// Shows the tutorial overlay.
        /// </summary>
        public void ShowTutorial()
        {
            if (tutorialActive) return;
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    Debug.LogWarning("[TutorialManager] No canvas found for tutorial");
                    return;
                }
            }

            tutorialActive = true;
            CreateTutorialUI();
            StartTapAnimation();

            // Subscribe to dice roll events to detect when player taps
            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.OnDiceRolled += OnPlayerRolledDice;
            }

            Debug.Log("[TutorialManager] Tutorial started");
        }

        /// <summary>
        /// Called when the player rolls a dice.
        /// </summary>
        private void OnPlayerRolledDice(Dice.Dice dice, int value)
        {
            // Tutorial complete - player figured it out!
            CompleteTutorial();
        }

        /// <summary>
        /// Hides and destroys the tutorial overlay.
        /// </summary>
        public void HideTutorial()
        {
            if (!tutorialActive) return;

            tutorialActive = false;

            // Unsubscribe from events
            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.OnDiceRolled -= OnPlayerRolledDice;
            }

            // Stop animation
            tapAnimation?.Kill();

            // Animate out
            if (tutorialPanel != null)
            {
                CanvasGroup cg = tutorialPanel.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.DOFade(0f, 0.3f).OnComplete(() =>
                    {
                        if (tutorialPanel != null)
                            Destroy(tutorialPanel);
                    });
                }
                else
                {
                    Destroy(tutorialPanel);
                }
            }

            Debug.Log("[TutorialManager] Tutorial completed");
        }

        /// <summary>
        /// Creates the tutorial UI elements.
        /// </summary>
        private void CreateTutorialUI()
        {
            // Create main panel
            tutorialPanel = new GameObject("TutorialPanel");
            tutorialPanel.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = tutorialPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Add canvas group for fading
            CanvasGroup canvasGroup = tutorialPanel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false; // Allow clicks through to dice

            // Create text background panel for better readability
            GameObject textBgObj = new GameObject("TextBackground");
            textBgObj.transform.SetParent(tutorialPanel.transform, false);

            RectTransform textBgRect = textBgObj.AddComponent<RectTransform>();
            textBgRect.anchorMin = new Vector2(0.5f, 0.75f);
            textBgRect.anchorMax = new Vector2(0.5f, 0.75f);
            textBgRect.pivot = new Vector2(0.5f, 0.5f);
            textBgRect.sizeDelta = new Vector2(750f, 120f);
            textBgRect.anchoredPosition = Vector2.zero;

            Image bgImage = textBgObj.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.8f); // Dark semi-transparent background
            bgImage.raycastTarget = false;

            // Create instruction text
            GameObject textObj = new GameObject("InstructionText");
            textObj.transform.SetParent(textBgObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            instructionText = textObj.AddComponent<TextMeshProUGUI>();
            instructionText.text = "Tap on the dice to roll!";
            instructionText.fontSize = 48;
            instructionText.fontStyle = FontStyles.Bold;
            instructionText.alignment = TextAlignmentOptions.Center;
            instructionText.verticalAlignment = VerticalAlignmentOptions.Middle;
            instructionText.color = Color.white;
            instructionText.enableWordWrapping = false;
            instructionText.overflowMode = TextOverflowModes.Overflow;

            // Get font from existing UI for consistent look
            if (GameUI.Instance != null)
            {
                var existingText = GameUI.Instance.GetComponentInChildren<TextMeshProUGUI>();
                if (existingText != null && existingText.font != null)
                {
                    instructionText.font = existingText.font;
                }
            }

            // Create hand image - position so finger points at dice (center of screen)
            GameObject handObj = new GameObject("TutorialHand");
            handObj.transform.SetParent(tutorialPanel.transform, false);

            handTransform = handObj.AddComponent<RectTransform>();
            // Position hand above dice area
            handTransform.anchorMin = new Vector2(0.5f, 0.5f);
            handTransform.anchorMax = new Vector2(0.5f, 0.5f);
            // Pivot near fingertip so tapping animation keeps finger on target
            handTransform.pivot = new Vector2(0.7f, 0.85f);
            handTransform.sizeDelta = new Vector2(handSize, handSize);
            // Position hand below dice so finger points UP at center of dice
            handTransform.anchoredPosition = new Vector2(60f, -100f);

            Image handImage = handObj.AddComponent<Image>();

            // Load hand sprite from Resources
            Sprite handSprite = Resources.Load<Sprite>("Tutorial/tutorial_hand");
            if (handSprite != null)
            {
                handImage.sprite = handSprite;
                Debug.Log("[TutorialManager] Loaded tutorial_hand sprite from Resources");
            }
            else
            {
                Debug.LogWarning("[TutorialManager] Could not load tutorial_hand sprite from Resources/Tutorial/tutorial_hand");
                // Fallback to procedural if sprite not found
                handImage.sprite = CreateHandSprite();
            }
            handImage.raycastTarget = false;
            handImage.preserveAspect = true;

            // Keep hand upright (finger pointing UP)
            handTransform.localScale = new Vector3(1f, 1f, 1f);

            // Fade in
            canvasGroup.DOFade(1f, 0.5f);

            // Subtle pulse on text background
            textBgObj.transform.DOScale(1.03f, 1f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        /// <summary>
        /// Creates a simple hand/pointer sprite procedurally.
        /// </summary>
        private Sprite CreateHandSprite()
        {
            int size = 128;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];

            // Clear to transparent
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;

            // Hand colors
            Color skinColor = new Color(0.95f, 0.8f, 0.7f);
            Color skinDark = new Color(0.85f, 0.7f, 0.6f);
            Color outline = new Color(0.3f, 0.25f, 0.2f);

            int centerX = size / 2;
            int centerY = size / 2;

            // Draw a pointing finger shape
            // Main finger (index finger pointing)
            DrawEllipse(pixels, size, centerX, centerY + 15, 18, 45, skinColor, skinDark);

            // Finger tip (rounded)
            DrawCircle(pixels, size, centerX, centerY + 55, 16, skinColor);

            // Palm/hand base
            DrawEllipse(pixels, size, centerX, centerY - 25, 35, 25, skinColor, skinDark);

            // Thumb (to the side)
            DrawEllipse(pixels, size, centerX + 25, centerY - 10, 12, 20, skinColor, skinDark);

            // Other fingers (curled, just hints)
            DrawEllipse(pixels, size, centerX - 18, centerY - 15, 10, 18, skinDark, skinDark);
            DrawEllipse(pixels, size, centerX + 5, centerY - 20, 10, 15, skinDark, skinDark);

            // Add outline effect
            AddOutline(pixels, size, outline);

            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;

            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private void DrawCircle(Color[] pixels, int texSize, int cx, int cy, int radius, Color color)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                for (int x = cx - radius; x <= cx + radius; x++)
                {
                    if (x < 0 || x >= texSize || y < 0 || y >= texSize) continue;

                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (dist <= radius)
                    {
                        float alpha = Mathf.Clamp01(radius - dist + 1f);
                        int idx = y * texSize + x;
                        Color existing = pixels[idx];
                        color.a = alpha;
                        pixels[idx] = Color.Lerp(existing, color, alpha);
                    }
                }
            }
        }

        private void DrawEllipse(Color[] pixels, int texSize, int cx, int cy, int rx, int ry, Color color, Color colorDark)
        {
            for (int y = cy - ry; y <= cy + ry; y++)
            {
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    if (x < 0 || x >= texSize || y < 0 || y >= texSize) continue;

                    float dx = (float)(x - cx) / rx;
                    float dy = (float)(y - cy) / ry;
                    float dist = dx * dx + dy * dy;

                    if (dist <= 1f)
                    {
                        // Gradient from center to edge
                        float t = Mathf.Sqrt(dist);
                        Color finalColor = Color.Lerp(color, colorDark, t * 0.5f);

                        // Anti-alias edge
                        float alpha = Mathf.Clamp01((1f - dist) * 3f);
                        finalColor.a = alpha;

                        int idx = y * texSize + x;
                        Color existing = pixels[idx];
                        pixels[idx] = Color.Lerp(existing, finalColor, alpha);
                    }
                }
            }
        }

        private void AddOutline(Color[] pixels, int texSize, Color outlineColor)
        {
            Color[] result = new Color[pixels.Length];
            System.Array.Copy(pixels, result, pixels.Length);

            for (int y = 1; y < texSize - 1; y++)
            {
                for (int x = 1; x < texSize - 1; x++)
                {
                    int idx = y * texSize + x;
                    if (pixels[idx].a < 0.1f)
                    {
                        // Check neighbors
                        bool hasNeighbor = false;
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nidx = (y + dy) * texSize + (x + dx);
                                if (pixels[nidx].a > 0.5f)
                                {
                                    hasNeighbor = true;
                                    break;
                                }
                            }
                            if (hasNeighbor) break;
                        }

                        if (hasNeighbor)
                        {
                            outlineColor.a = 0.8f;
                            result[idx] = outlineColor;
                        }
                    }
                }
            }

            System.Array.Copy(result, pixels, pixels.Length);
        }

        /// <summary>
        /// Starts the tapping animation for the hand.
        /// </summary>
        private void StartTapAnimation()
        {
            if (handTransform == null) return;

            tapAnimation?.Kill();

            Vector2 startPos = handTransform.anchoredPosition;
            // Move up toward the dice (negative Y - finger points up)
            Vector2 tapPos = startPos + new Vector2(0, -handMoveDistance);

            // Normal scale values
            Vector3 baseScale = new Vector3(1f, 1f, 1f);
            Vector3 pressedScale = new Vector3(0.95f, 0.95f, 1f);

            tapAnimation = DOTween.Sequence();

            // Move up (tap toward dice)
            tapAnimation.Append(handTransform.DOAnchorPos(tapPos, 0.15f).SetEase(Ease.InQuad));

            // Slight scale on tap (keeping negative values to stay flipped)
            tapAnimation.Join(handTransform.DOScale(pressedScale, 0.1f).SetEase(Ease.InQuad));

            // Move back up
            tapAnimation.Append(handTransform.DOAnchorPos(startPos, 0.25f).SetEase(Ease.OutQuad));
            tapAnimation.Join(handTransform.DOScale(baseScale, 0.2f).SetEase(Ease.OutBack));

            // Pause
            tapAnimation.AppendInterval(handTapInterval - 0.4f);

            // Loop
            tapAnimation.SetLoops(-1);
        }

        /// <summary>
        /// Resets the tutorial (for testing purposes).
        /// </summary>
        public void ResetTutorial()
        {
            PlayerPrefs.DeleteKey(TUTORIAL_COMPLETED_KEY);
            PlayerPrefs.Save();
            Debug.Log("[TutorialManager] Tutorial reset");
        }

        private void OnDestroy()
        {
            tapAnimation?.Kill();

            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.OnDiceRolled -= OnPlayerRolledDice;
            }
        }
    }
}
