using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Incredicer.Core;

namespace Incredicer.UI
{
    /// <summary>
    /// Centralized manager for applying consistent polish, animations, and Feel feedbacks
    /// across all UI elements in the game.
    /// </summary>
    public class UIPolishManager : MonoBehaviour
    {
        public static UIPolishManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GUISpriteAssets guiAssets;

        [Header("Button Press Settings")]
        [SerializeField] private float buttonPressScale = 0.88f;      // More squish on press
        [SerializeField] private float buttonReleaseScale = 1.12f;    // Bigger bounce on release
        [SerializeField] private float buttonPressDuration = 0.06f;   // Snappier press
        [SerializeField] private float buttonReleaseDuration = 0.15f; // Juicier release

        [Header("Audio")]
        [SerializeField] private bool playButtonSounds = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (guiAssets == null)
                guiAssets = GUISpriteAssets.Instance;
        }

        private void Start()
        {
            // Auto-polish all existing buttons
            PolishAllButtons();
        }

        #region Button Polish

        /// <summary>
        /// Finds and polishes all buttons in the scene with consistent feedback.
        /// </summary>
        public void PolishAllButtons()
        {
            Button[] allButtons = FindObjectsOfType<Button>(true);
            foreach (Button btn in allButtons)
            {
                AddButtonPolish(btn);
            }
            Debug.Log($"[UIPolishManager] Polished {allButtons.Length} buttons");
        }

        /// <summary>
        /// Adds press/release animations and sound to a button.
        /// </summary>
        public void AddButtonPolish(Button button)
        {
            if (button == null) return;

            // Skip if already has our polish component
            if (button.GetComponent<PolishedButton>() != null) return;

            // Add the polish component
            PolishedButton polish = button.gameObject.AddComponent<PolishedButton>();
            polish.Initialize(this);
        }

        /// <summary>
        /// Polishes all buttons within a specific panel/GameObject.
        /// Call this after building dynamic UI panels.
        /// </summary>
        public void PolishButtonsInPanel(GameObject panel)
        {
            if (panel == null) return;

            Button[] buttons = panel.GetComponentsInChildren<Button>(true);
            int polishedCount = 0;
            foreach (Button btn in buttons)
            {
                if (btn.GetComponent<PolishedButton>() == null)
                {
                    AddButtonPolish(btn);
                    polishedCount++;
                }
            }

            if (polishedCount > 0)
            {
                Debug.Log($"[UIPolishManager] Polished {polishedCount} buttons in {panel.name}");
            }
        }

        /// <summary>
        /// Creates a polished button with GUI sprite background.
        /// </summary>
        public Button CreatePolishedButton(Transform parent, string name, string label, ButtonStyle style)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200f, UIDesignSystem.ButtonHeightMedium);

            // Background with GUI sprite
            Image bg = btnObj.AddComponent<Image>();
            bg.sprite = GetButtonSprite(style);
            bg.type = Image.Type.Sliced;
            bg.color = Color.white; // Use sprite's own color

            // Button component
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Adjusted colors for sprite buttons
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
            colors.fadeDuration = 0.1f;
            btn.colors = colors;

            // Label
            if (!string.IsNullOrEmpty(label))
            {
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(btnObj.transform, false);

                RectTransform labelRt = labelObj.AddComponent<RectTransform>();
                labelRt.anchorMin = Vector2.zero;
                labelRt.anchorMax = Vector2.one;
                labelRt.offsetMin = new Vector2(UIDesignSystem.PaddingButton, UIDesignSystem.SpacingXS);
                labelRt.offsetMax = new Vector2(-UIDesignSystem.PaddingButton, -UIDesignSystem.SpacingXS);

                TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
                UIDesignSystem.StyleText(labelText, UIDesignSystem.FontSizeButton, UIDesignSystem.TextPrimary, FontStyles.Bold);
                labelText.alignment = TextAlignmentOptions.Center;
                labelText.text = label.ToUpper();
            }

            // Add polish
            AddButtonPolish(btn);

            return btn;
        }

        private Sprite GetButtonSprite(ButtonStyle style)
        {
            if (guiAssets == null) return null;

            switch (style)
            {
                case ButtonStyle.Green:
                    return guiAssets.buttonGreen;
                case ButtonStyle.Blue:
                    return guiAssets.buttonBlue;
                case ButtonStyle.Yellow:
                    return guiAssets.buttonYellow;
                case ButtonStyle.Red:
                    return guiAssets.buttonRed;
                case ButtonStyle.Purple:
                    return guiAssets.buttonPurple;
                case ButtonStyle.Gray:
                default:
                    return guiAssets.buttonGray;
            }
        }

        #endregion

        #region Panel Polish

        /// <summary>
        /// Creates a polished panel with GUI sprite background.
        /// </summary>
        public GameObject CreatePolishedPanel(Transform parent, string name, bool usePopupBg = true)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(UIDesignSystem.MarginScreen, UIDesignSystem.MarginScreen);
            rt.anchorMax = new Vector2(1f - UIDesignSystem.MarginScreen, 1f - UIDesignSystem.MarginScreen);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Background with GUI sprite
            Image bg = panel.AddComponent<Image>();
            if (guiAssets != null && usePopupBg && guiAssets.popupBackground != null)
            {
                bg.sprite = guiAssets.popupBackground;
                bg.type = Image.Type.Sliced;
                bg.color = Color.white;
            }
            else
            {
                bg.color = UIDesignSystem.PanelDark;
            }

            // Add canvas group for animations
            panel.AddComponent<CanvasGroup>();

            return panel;
        }

        /// <summary>
        /// Shows a panel with polished animation.
        /// </summary>
        public void ShowPanel(GameObject panel, bool withBounce = true)
        {
            if (panel == null) return;

            panel.SetActive(true);

            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
            }

            panel.transform.localScale = Vector3.one * 0.7f; // Start smaller for more pop

            Sequence seq = DOTween.Sequence();

            if (withBounce)
            {
                // JUICY bounce sequence!
                seq.Append(panel.transform.DOScale(1.08f, UIDesignSystem.AnimFadeIn * 0.6f).SetEase(Ease.OutBack, 2f));
                seq.Append(panel.transform.DOScale(0.97f, UIDesignSystem.AnimFadeIn * 0.2f).SetEase(Ease.InQuad));
                seq.Append(panel.transform.DOScale(1f, UIDesignSystem.AnimFadeIn * 0.2f).SetEase(Ease.OutQuad));
            }
            else
            {
                seq.Append(panel.transform.DOScale(1f, UIDesignSystem.AnimFadeIn).SetEase(Ease.OutCubic));
            }

            if (cg != null)
            {
                seq.Join(cg.DOFade(1f, UIDesignSystem.AnimFadeIn * 0.7f).SetEase(Ease.OutQuad));
            }

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClickSound();
            }
        }

        /// <summary>
        /// Hides a panel with polished animation.
        /// </summary>
        public void HidePanel(GameObject panel, System.Action onComplete = null)
        {
            if (panel == null) return;

            CanvasGroup cg = panel.GetComponent<CanvasGroup>();

            Sequence seq = DOTween.Sequence();
            // Quick shrink with slight bounce
            seq.Append(panel.transform.DOScale(1.03f, UIDesignSystem.AnimFadeOut * 0.2f).SetEase(Ease.OutQuad));
            seq.Append(panel.transform.DOScale(0.85f, UIDesignSystem.AnimFadeOut * 0.8f).SetEase(Ease.InBack, 1.5f));

            if (cg != null)
            {
                seq.Join(cg.DOFade(0f, UIDesignSystem.AnimFadeOut).SetEase(Ease.InQuad));
            }

            seq.OnComplete(() =>
            {
                panel.SetActive(false);
                panel.transform.localScale = Vector3.one;
                if (cg != null) cg.alpha = 1f;
                onComplete?.Invoke();
            });
        }

        #endregion

        #region Effects

        /// <summary>
        /// Plays a success celebration effect.
        /// </summary>
        public void PlaySuccessEffect(Transform target)
        {
            if (target == null) return;

            // JUICY scale pop with multiple bounces
            target.DOKill();
            Sequence seq = DOTween.Sequence();
            seq.Append(target.DOScale(1.4f, 0.1f).SetEase(Ease.OutBack, 2f));
            seq.Append(target.DOScale(0.95f, 0.08f).SetEase(Ease.InQuad));
            seq.Append(target.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad));
            seq.Append(target.DOScale(1f, 0.08f).SetEase(Ease.InOutQuad));

            // Screen flash - brighter
            if (VisualEffectsManager.Instance != null)
            {
                VisualEffectsManager.Instance.FlashScreen(new Color(0.4f, 1f, 0.5f, 0.4f), 0.25f);
            }

            // Sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPurchaseSound();
            }
        }

        /// <summary>
        /// Plays an error/rejection effect.
        /// </summary>
        public void PlayErrorEffect(Transform target)
        {
            if (target == null) return;

            // More intense shake
            target.DOKill();
            target.DOShakePosition(0.35f, 15f, 35, 0f, false, true);

            // Flash red - more intense
            Image img = target.GetComponent<Image>();
            if (img != null)
            {
                Color originalColor = img.color;
                Sequence colorSeq = DOTween.Sequence();
                colorSeq.Append(img.DOColor(UIDesignSystem.ErrorRed, 0.08f));
                colorSeq.Append(img.DOColor(originalColor, 0.1f));
                colorSeq.Append(img.DOColor(UIDesignSystem.ErrorRed * 0.7f, 0.06f));
                colorSeq.Append(img.DOColor(originalColor, 0.15f));
            }

            // Screen flash red
            if (VisualEffectsManager.Instance != null)
            {
                VisualEffectsManager.Instance.FlashScreen(new Color(1f, 0.3f, 0.3f, 0.25f), 0.15f);
            }
        }

        /// <summary>
        /// Adds a pulsing glow effect to highlight an element.
        /// </summary>
        public Tween AddPulseGlow(Transform target, Color glowColor)
        {
            if (target == null) return null;

            // Create glow object
            GameObject glowObj = new GameObject("PulseGlow");
            glowObj.transform.SetParent(target, false);
            glowObj.transform.SetAsFirstSibling();

            RectTransform glowRt = glowObj.AddComponent<RectTransform>();
            glowRt.anchorMin = Vector2.zero;
            glowRt.anchorMax = Vector2.one;
            glowRt.offsetMin = new Vector2(-10, -10);
            glowRt.offsetMax = new Vector2(10, 10);

            Image glowImg = glowObj.AddComponent<Image>();
            glowImg.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.4f);
            glowImg.raycastTarget = false;

            // Pulse animation
            return glowObj.transform.DOScale(1.1f, UIDesignSystem.AnimPulse)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        /// <summary>
        /// Creates a ribbon/label with GUI sprite.
        /// </summary>
        public GameObject CreateRibbon(Transform parent, string text, RibbonStyle style)
        {
            GameObject ribbonObj = new GameObject("Ribbon");
            ribbonObj.transform.SetParent(parent, false);

            RectTransform rt = ribbonObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 50);

            Image img = ribbonObj.AddComponent<Image>();
            img.sprite = GetRibbonSprite(style);
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(ribbonObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10, 5);
            textRt.offsetMax = new Vector2(-10, -5);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            UIDesignSystem.StyleText(tmp, UIDesignSystem.FontSizeLabel, UIDesignSystem.TextPrimary, FontStyles.Bold);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.text = text;

            return ribbonObj;
        }

        private Sprite GetRibbonSprite(RibbonStyle style)
        {
            if (guiAssets == null) return null;

            switch (style)
            {
                case RibbonStyle.Yellow:
                    return guiAssets.ribbonYellow;
                case RibbonStyle.Green:
                    return guiAssets.ribbonGreen;
                case RibbonStyle.Blue:
                    return guiAssets.ribbonBlue;
                case RibbonStyle.Purple:
                    return guiAssets.ribbonPurple;
                default:
                    return guiAssets.ribbonYellow;
            }
        }

        #endregion

        #region Feel Feedbacks

        /// <summary>
        /// Creates an MMF_Player with scale feedback for button presses.
        /// </summary>
        public MMF_Player CreateButtonFeedback(GameObject target)
        {
            GameObject feedbackObj = new GameObject("ButtonFeedback");
            feedbackObj.transform.SetParent(target.transform, false);

            MMF_Player player = feedbackObj.AddComponent<MMF_Player>();
            player.InitializationMode = MMF_Player.InitializationModes.Script;

            // Add scale feedback
            MMF_Scale scaleFeedback = new MMF_Scale();
            scaleFeedback.Label = "Press Scale";
            scaleFeedback.AnimateScaleTarget = target.transform;
            scaleFeedback.RemapCurveOne = 0.92f;
            scaleFeedback.AnimateScaleDuration = 0.1f;
            player.AddFeedback(scaleFeedback);

            player.Initialization();
            return player;
        }

        /// <summary>
        /// Creates an MMF_Player with celebration effects.
        /// </summary>
        public MMF_Player CreateCelebrationFeedback(GameObject target)
        {
            GameObject feedbackObj = new GameObject("CelebrationFeedback");
            feedbackObj.transform.SetParent(target.transform, false);

            MMF_Player player = feedbackObj.AddComponent<MMF_Player>();
            player.InitializationMode = MMF_Player.InitializationModes.Script;

            // Scale punch
            MMF_Scale scaleFeedback = new MMF_Scale();
            scaleFeedback.Label = "Celebration Scale";
            scaleFeedback.AnimateScaleTarget = target.transform;
            scaleFeedback.RemapCurveOne = 1.3f;
            scaleFeedback.AnimateScaleDuration = 0.2f;
            player.AddFeedback(scaleFeedback);

            player.Initialization();
            return player;
        }

        #endregion

        #region Animation Callbacks

        public void OnButtonPressed(Transform button)
        {
            if (button == null) return;

            button.DOKill();
            // Snappy squish with slight overshoot
            button.DOScale(buttonPressScale, buttonPressDuration).SetEase(Ease.OutQuad);
        }

        public void OnButtonReleased(Transform button)
        {
            if (button == null) return;

            // Juicy bounce sequence with overshoot and settle
            Sequence seq = DOTween.Sequence();
            seq.Append(button.DOScale(buttonReleaseScale, buttonReleaseDuration * 0.4f).SetEase(Ease.OutBack, 2f)); // Strong overshoot
            seq.Append(button.DOScale(0.97f, buttonReleaseDuration * 0.25f).SetEase(Ease.InQuad)); // Subtle undershoot
            seq.Append(button.DOScale(1f, buttonReleaseDuration * 0.35f).SetEase(Ease.OutQuad)); // Settle to normal
        }

        public void OnButtonClicked(Transform button)
        {
            if (playButtonSounds && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClickSound();
            }
        }

        /// <summary>
        /// Play a special feedback for important actions (purchase, claim reward, etc.)
        /// Uses Feel's MMF_Player for enhanced feedback.
        /// </summary>
        public void PlayImportantClickFeedback(Transform target)
        {
            if (target == null) return;

            // JUICY scale punch effect with multiple bounces
            target.DOKill();
            Sequence seq = DOTween.Sequence();
            seq.Append(target.DOScale(0.8f, 0.04f).SetEase(Ease.InQuad));   // Quick squish
            seq.Append(target.DOScale(1.25f, 0.1f).SetEase(Ease.OutBack, 2.5f)); // Big bounce
            seq.Append(target.DOScale(0.95f, 0.06f).SetEase(Ease.InQuad));  // Undershoot
            seq.Append(target.DOScale(1.05f, 0.08f).SetEase(Ease.OutQuad)); // Small bounce
            seq.Append(target.DOScale(1f, 0.06f).SetEase(Ease.InOutSine));  // Settle

            // Satisfying camera shake
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.DOKill();
                cam.transform.DOShakePosition(0.15f, 0.025f, 15, 90f, false, true);
            }

            // Play purchase sound for important actions
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPurchaseSound();
            }

            // Screen flash for extra juice - brighter
            if (VisualEffectsManager.Instance != null)
            {
                VisualEffectsManager.Instance.FlashScreen(new Color(0.4f, 1f, 0.5f, 0.35f), 0.2f);
            }
        }

        /// <summary>
        /// Play a rejection/error feedback for failed actions.
        /// </summary>
        public void PlayErrorFeedback(Transform target)
        {
            if (target == null) return;

            // Shake the element
            target.DOKill();
            target.DOShakePosition(0.25f, 8f, 25, 0f, false, true);

            // Flash red
            Image img = target.GetComponent<Image>();
            if (img != null)
            {
                Color originalColor = img.color;
                img.DOColor(UIDesignSystem.ErrorRed, 0.08f).OnComplete(() =>
                {
                    img.DOColor(originalColor, 0.15f);
                });
            }
        }

        #endregion
    }

    #region Enums

    public enum ButtonStyle
    {
        Green,
        Blue,
        Yellow,
        Red,
        Purple,
        Gray
    }

    public enum RibbonStyle
    {
        Yellow,
        Green,
        Blue,
        Purple
    }

    #endregion

    #region Helper Components

    /// <summary>
    /// Component that adds polish animations to a button.
    /// </summary>
    public class PolishedButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        private UIPolishManager manager;
        private Button button;
        private bool isPressed = false;

        public void Initialize(UIPolishManager polishManager)
        {
            manager = polishManager;
            button = GetComponent<Button>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;

            isPressed = true;
            manager?.OnButtonPressed(transform);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed) return;

            isPressed = false;
            manager?.OnButtonReleased(transform);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;

            manager?.OnButtonClicked(transform);
        }
    }

    #endregion
}
