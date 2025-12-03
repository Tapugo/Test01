using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Incredicer.UI
{
    /// <summary>
    /// Centralized UI Design System for consistent styling across all game UI.
    /// This is the single source of truth for colors, sizes, animations, and typography.
    /// </summary>
    public static class UIDesignSystem
    {
        #region Color Palette

        // Primary Brand Colors
        public static readonly Color PrimaryPurple = new Color(0.6f, 0.4f, 1f);
        public static readonly Color PrimaryGold = new Color(1f, 0.85f, 0.2f);
        public static readonly Color PrimaryOrange = new Color(1f, 0.5f, 0.2f);

        // Currency Colors (SINGLE SOURCE)
        public static readonly Color MoneyGreen = new Color(0.4f, 0.95f, 0.5f);
        public static readonly Color DarkMatterPurple = new Color(0.8f, 0.5f, 1f);
        public static readonly Color TimeShardsBlue = new Color(0.4f, 0.8f, 1f);
        public static readonly Color TokensYellow = new Color(1f, 0.9f, 0.3f);

        // UI Background Colors
        public static readonly Color PanelDark = new Color(0.06f, 0.05f, 0.1f, 0.98f);
        public static readonly Color PanelMedium = new Color(0.1f, 0.08f, 0.15f, 0.95f);
        public static readonly Color PanelLight = new Color(0.15f, 0.12f, 0.2f, 0.9f);
        public static readonly Color OverlayDark = new Color(0f, 0f, 0f, 0.85f);
        public static readonly Color OverlayMedium = new Color(0f, 0f, 0f, 0.7f);

        // Button Colors
        public static readonly Color ButtonPrimary = new Color(0.3f, 0.7f, 0.4f);      // Green - primary action
        public static readonly Color ButtonSecondary = new Color(0.4f, 0.3f, 0.6f);    // Purple - secondary
        public static readonly Color ButtonDanger = new Color(0.8f, 0.3f, 0.3f);       // Red - destructive
        public static readonly Color ButtonDisabled = new Color(0.35f, 0.35f, 0.4f);   // Gray
        public static readonly Color ButtonWarning = new Color(1f, 0.6f, 0.2f);        // Orange

        // State Colors
        public static readonly Color SuccessGreen = new Color(0.3f, 0.9f, 0.4f);
        public static readonly Color WarningOrange = new Color(1f, 0.7f, 0.3f);
        public static readonly Color ErrorRed = new Color(0.9f, 0.3f, 0.3f);
        public static readonly Color InfoBlue = new Color(0.4f, 0.7f, 1f);

        // Text Colors
        public static readonly Color TextPrimary = Color.white;
        public static readonly Color TextSecondary = new Color(0.8f, 0.8f, 0.85f);
        public static readonly Color TextMuted = new Color(0.6f, 0.6f, 0.65f);
        public static readonly Color TextDisabled = new Color(0.4f, 0.4f, 0.45f);

        // Accent Colors per Feature
        public static readonly Color AccentDailyLogin = new Color(0.3f, 0.8f, 0.4f);
        public static readonly Color AccentMissions = new Color(1f, 0.6f, 0.2f);
        public static readonly Color AccentOverclock = new Color(1f, 0.4f, 0.15f);
        public static readonly Color AccentTimeFracture = new Color(0.6f, 0.4f, 1f);
        public static readonly Color AccentMilestones = new Color(1f, 0.8f, 0.2f);
        public static readonly Color AccentEvents = new Color(0.3f, 0.7f, 1f);
        public static readonly Color AccentLeaderboard = new Color(0.9f, 0.7f, 0.2f);

        #endregion

        #region Typography - Font Sizes

        // Title sizes (for panel headers)
        public const float FontSizeHero = 72f;        // Main splash screens
        public const float FontSizeTitle = 56f;       // Panel titles
        public const float FontSizeSubtitle = 40f;    // Section headers

        // Body sizes
        public const float FontSizeLarge = 32f;       // Important info
        public const float FontSizeBody = 28f;        // Regular content
        public const float FontSizeSmall = 24f;       // Secondary info

        // UI Element sizes
        public const float FontSizeButton = 28f;      // Button text
        public const float FontSizeLabel = 20f;       // Small labels
        public const float FontSizeCaption = 18f;     // Tiny text (use sparingly)

        #endregion

        #region Spacing & Sizing

        // Touch Targets (Mobile-friendly minimums)
        public const float TouchTargetMin = 48f;      // Absolute minimum
        public const float TouchTargetIdeal = 56f;    // Comfortable touch
        public const float TouchTargetLarge = 72f;    // Primary actions

        // Button Sizes
        public const float ButtonHeightSmall = 48f;
        public const float ButtonHeightMedium = 56f;
        public const float ButtonHeightLarge = 72f;
        public const float ButtonHeightXLarge = 90f;

        // Spacing
        public const float SpacingXS = 4f;
        public const float SpacingS = 8f;
        public const float SpacingM = 16f;
        public const float SpacingL = 24f;
        public const float SpacingXL = 32f;
        public const float SpacingXXL = 48f;

        // Padding
        public const float PaddingPanel = 24f;        // Inside panels
        public const float PaddingCard = 16f;         // Inside cards
        public const float PaddingButton = 12f;       // Inside buttons

        // Panel Margins (from screen edge)
        public const float MarginScreen = 0.03f;      // 3% from edges (anchor-based)

        // Card/Item Sizes
        public const float CardHeightSmall = 120f;
        public const float CardHeightMedium = 160f;
        public const float CardHeightLarge = 200f;

        // Icon Sizes
        public const float IconSizeSmall = 32f;
        public const float IconSizeMedium = 48f;
        public const float IconSizeLarge = 64f;
        public const float IconSizeXLarge = 96f;

        #endregion

        #region Animation Durations

        // Panel Animations
        public const float AnimFadeIn = 0.25f;
        public const float AnimFadeOut = 0.2f;
        public const float AnimSlideIn = 0.3f;
        public const float AnimSlideOut = 0.25f;

        // Button Feedback
        public const float AnimButtonPress = 0.08f;
        public const float AnimButtonRelease = 0.12f;
        public const float AnimButtonBounce = 0.15f;

        // Purchase/Success Animations
        public const float AnimSuccessPop = 0.3f;
        public const float AnimErrorShake = 0.3f;

        // Looping Animations
        public const float AnimPulse = 0.8f;
        public const float AnimFloat = 1.5f;
        public const float AnimGlow = 1.2f;

        #endregion

        #region Animation Easing

        public static readonly Ease EasePopIn = Ease.OutBack;
        public static readonly Ease EasePopOut = Ease.InBack;
        public static readonly Ease EaseFade = Ease.InOutSine;
        public static readonly Ease EaseSlide = Ease.OutCubic;
        public static readonly Ease EaseBounce = Ease.OutBounce;
        public static readonly Ease EaseSmooth = Ease.InOutQuad;

        #endregion

        #region Border & Effects

        public const float OutlineThickness = 0.18f;
        public static readonly Color OutlineColor = new Color(0f, 0f, 0f, 0.8f);

        public const float ShadowDistance = 3f;
        public static readonly Color ShadowColor = new Color(0f, 0f, 0f, 0.5f);

        public const float GlowIntensity = 0.6f;

        #endregion

        #region Z-Order (Sorting Order)

        public const int SortBackground = 0;
        public const int SortMainUI = 50;
        public const int SortFloatingEffects = 100;
        public const int SortPopups = 150;
        public const int SortModals = 200;
        public const int SortNotifications = 250;
        public const int SortTooltips = 300;

        #endregion

        #region Helper Methods

        /// <summary>
        /// Apply consistent text styling with outline
        /// </summary>
        public static void StyleText(TextMeshProUGUI text, float fontSize, Color color,
            FontStyles style = FontStyles.Normal, bool withOutline = true)
        {
            if (text == null) return;

            text.fontSize = fontSize;
            text.color = color;
            text.fontStyle = style;

            if (withOutline)
            {
                ApplyTextOutline(text);
            }

            // Apply shared font if available
            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                text.font = GameUI.Instance.SharedFont;
            }
        }

        /// <summary>
        /// Apply text outline for readability
        /// </summary>
        public static void ApplyTextOutline(TextMeshProUGUI text, float thickness = -1f)
        {
            if (text == null || text.fontSharedMaterial == null) return;

            float outlineWidth = thickness > 0 ? thickness : OutlineThickness;
            text.outlineWidth = outlineWidth;
            text.outlineColor = OutlineColor;
        }

        /// <summary>
        /// Create a styled button with consistent look
        /// </summary>
        public static Button CreateButton(Transform parent, string name, string label,
            Color bgColor, float width = -1, float height = -1)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(
                width > 0 ? width : 200f,
                height > 0 ? height : ButtonHeightMedium
            );

            // Background
            Image bg = btnObj.AddComponent<Image>();
            bg.color = bgColor;

            // Button component
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Button colors
            ColorBlock colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = bgColor * 1.15f;
            colors.pressedColor = bgColor * 0.85f;
            colors.disabledColor = ButtonDisabled;
            colors.fadeDuration = 0.1f;
            btn.colors = colors;

            // Outline for depth
            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = ShadowColor;
            outline.effectDistance = new Vector2(2, -2);

            // Label
            if (!string.IsNullOrEmpty(label))
            {
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(btnObj.transform, false);

                RectTransform labelRt = labelObj.AddComponent<RectTransform>();
                labelRt.anchorMin = Vector2.zero;
                labelRt.anchorMax = Vector2.one;
                labelRt.offsetMin = new Vector2(PaddingButton, SpacingXS);
                labelRt.offsetMax = new Vector2(-PaddingButton, -SpacingXS);

                TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
                StyleText(labelText, FontSizeButton, TextPrimary, FontStyles.Bold);
                labelText.alignment = TextAlignmentOptions.Center;
                labelText.text = label.ToUpper();
            }

            return btn;
        }

        /// <summary>
        /// Create a panel with standard styling
        /// </summary>
        public static GameObject CreatePanel(Transform parent, string name, bool fullscreen = true)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();

            if (fullscreen)
            {
                rt.anchorMin = new Vector2(MarginScreen, MarginScreen);
                rt.anchorMax = new Vector2(1f - MarginScreen, 1f - MarginScreen);
            }
            else
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
            }
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Background
            Image bg = panel.AddComponent<Image>();
            bg.color = PanelDark;

            // Outline
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = PrimaryPurple * 0.5f;
            outline.effectDistance = new Vector2(3, -3);

            return panel;
        }

        /// <summary>
        /// Create an overlay that blocks interaction behind a popup
        /// </summary>
        public static GameObject CreateOverlay(Transform parent, string name, System.Action onClickOutside = null)
        {
            GameObject overlay = new GameObject(name);
            overlay.transform.SetParent(parent, false);

            RectTransform rt = overlay.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = overlay.AddComponent<Image>();
            bg.color = OverlayDark;

            if (onClickOutside != null)
            {
                Button btn = overlay.AddComponent<Button>();
                btn.targetGraphic = bg;
                btn.onClick.AddListener(() => onClickOutside());

                // Keep color consistent on hover
                ColorBlock colors = btn.colors;
                colors.highlightedColor = OverlayDark;
                colors.pressedColor = OverlayDark;
                btn.colors = colors;
            }

            return overlay;
        }

        /// <summary>
        /// Create a header for a panel
        /// </summary>
        public static TextMeshProUGUI CreateHeader(Transform parent, string text, Color accentColor)
        {
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(parent, false);

            RectTransform rt = headerObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 80);
            rt.anchoredPosition = new Vector2(0, -PaddingPanel);

            TextMeshProUGUI headerText = headerObj.AddComponent<TextMeshProUGUI>();
            StyleText(headerText, FontSizeTitle, accentColor, FontStyles.Bold);
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.text = text;

            return headerText;
        }

        /// <summary>
        /// Create a close button (X) in top-right corner
        /// </summary>
        public static Button CreateCloseButton(Transform parent, System.Action onClick)
        {
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(parent, false);

            RectTransform rt = closeObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-SpacingM, -SpacingM);
            rt.sizeDelta = new Vector2(TouchTargetIdeal, TouchTargetIdeal);

            Image bg = closeObj.AddComponent<Image>();
            bg.color = ButtonDanger;

            Button btn = closeObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => onClick());

            ColorBlock colors = btn.colors;
            colors.highlightedColor = ButtonDanger * 1.2f;
            colors.pressedColor = ButtonDanger * 0.8f;
            btn.colors = colors;

            // X text
            GameObject xObj = new GameObject("X");
            xObj.transform.SetParent(closeObj.transform, false);

            RectTransform xRt = xObj.AddComponent<RectTransform>();
            xRt.anchorMin = Vector2.zero;
            xRt.anchorMax = Vector2.one;
            xRt.offsetMin = Vector2.zero;
            xRt.offsetMax = Vector2.zero;

            TextMeshProUGUI xText = xObj.AddComponent<TextMeshProUGUI>();
            StyleText(xText, FontSizeLarge, TextPrimary, FontStyles.Bold);
            xText.alignment = TextAlignmentOptions.Center;
            xText.text = "✕";

            return btn;
        }

        /// <summary>
        /// Standard panel show animation
        /// </summary>
        public static void AnimateShowPanel(GameObject panel, CanvasGroup canvasGroup = null)
        {
            if (panel == null) return;

            panel.SetActive(true);
            panel.transform.localScale = Vector3.one * 0.9f;

            Sequence seq = DOTween.Sequence();
            seq.Append(panel.transform.DOScale(1f, AnimFadeIn).SetEase(EasePopIn));

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                seq.Join(canvasGroup.DOFade(1f, AnimFadeIn).SetEase(EaseFade));
            }
        }

        /// <summary>
        /// Standard panel hide animation
        /// </summary>
        public static void AnimateHidePanel(GameObject panel, CanvasGroup canvasGroup = null,
            System.Action onComplete = null)
        {
            if (panel == null) return;

            Sequence seq = DOTween.Sequence();
            seq.Append(panel.transform.DOScale(0.9f, AnimFadeOut).SetEase(EasePopOut));

            if (canvasGroup != null)
            {
                seq.Join(canvasGroup.DOFade(0f, AnimFadeOut).SetEase(EaseFade));
            }

            seq.OnComplete(() =>
            {
                panel.SetActive(false);
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// Button press feedback animation
        /// </summary>
        public static void AnimateButtonPress(Transform button)
        {
            if (button == null) return;

            button.DOKill();
            Sequence seq = DOTween.Sequence();
            seq.Append(button.DOScale(0.92f, AnimButtonPress).SetEase(Ease.InQuad));
            seq.Append(button.DOScale(1.05f, AnimButtonRelease).SetEase(Ease.OutBack));
            seq.Append(button.DOScale(1f, AnimButtonBounce).SetEase(Ease.InOutSine));
        }

        /// <summary>
        /// Error/rejection shake animation
        /// </summary>
        public static void AnimateErrorShake(Transform target)
        {
            if (target == null) return;

            target.DOKill();
            target.DOShakePosition(AnimErrorShake, 8f, 25, 0f, false, true);
        }

        /// <summary>
        /// Success pop animation
        /// </summary>
        public static void AnimateSuccessPop(Transform target)
        {
            if (target == null) return;

            target.DOKill();
            Sequence seq = DOTween.Sequence();
            seq.Append(target.DOScale(1.2f, AnimSuccessPop * 0.4f).SetEase(Ease.OutBack));
            seq.Append(target.DOScale(1f, AnimSuccessPop * 0.6f).SetEase(Ease.InOutSine));
        }

        /// <summary>
        /// Pulsing attention animation (loops)
        /// </summary>
        public static Tween AnimatePulse(Transform target, float intensity = 1.08f)
        {
            if (target == null) return null;

            return target.DOScale(intensity, AnimPulse)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        /// <summary>
        /// Get button color based on state
        /// </summary>
        public static Color GetButtonColor(bool isEnabled, bool isPrimary = true)
        {
            if (!isEnabled) return ButtonDisabled;
            return isPrimary ? ButtonPrimary : ButtonSecondary;
        }

        /// <summary>
        /// Create a progress bar
        /// </summary>
        public static (GameObject container, Image fill) CreateProgressBar(Transform parent,
            float width, float height, Color fillColor)
        {
            GameObject container = new GameObject("ProgressBar");
            container.transform.SetParent(parent, false);

            RectTransform containerRt = container.AddComponent<RectTransform>();
            containerRt.sizeDelta = new Vector2(width, height);

            // Background
            Image bgImg = container.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.2f);

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(container.transform, false);

            RectTransform fillRt = fillObj.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0, 1); // Will be animated
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = fillColor;

            return (container, fillImg);
        }

        /// <summary>
        /// Set progress bar fill (0-1)
        /// </summary>
        public static void SetProgressBarFill(Image fill, float progress, bool animate = true)
        {
            if (fill == null) return;

            RectTransform rt = fill.GetComponent<RectTransform>();
            float targetX = Mathf.Clamp01(progress);

            if (animate)
            {
                DOTween.To(() => rt.anchorMax.x, x => rt.anchorMax = new Vector2(x, 1), targetX, 0.3f)
                    .SetEase(Ease.OutCubic);
            }
            else
            {
                rt.anchorMax = new Vector2(targetX, 1);
            }
        }

        /// <summary>
        /// Create dice dot pattern (for dice visuals)
        /// </summary>
        public static void CreateDiceDots(Transform parent, int value, float dotSize, Color dotColor)
        {
            // Dice face positions for values 1-6
            Vector2[][] dotPatterns = new Vector2[][]
            {
                // 1
                new Vector2[] { Vector2.zero },
                // 2
                new Vector2[] { new Vector2(-0.3f, 0.3f), new Vector2(0.3f, -0.3f) },
                // 3
                new Vector2[] { new Vector2(-0.3f, 0.3f), Vector2.zero, new Vector2(0.3f, -0.3f) },
                // 4
                new Vector2[] { new Vector2(-0.3f, 0.3f), new Vector2(0.3f, 0.3f),
                               new Vector2(-0.3f, -0.3f), new Vector2(0.3f, -0.3f) },
                // 5
                new Vector2[] { new Vector2(-0.3f, 0.3f), new Vector2(0.3f, 0.3f), Vector2.zero,
                               new Vector2(-0.3f, -0.3f), new Vector2(0.3f, -0.3f) },
                // 6
                new Vector2[] { new Vector2(-0.3f, 0.3f), new Vector2(0.3f, 0.3f),
                               new Vector2(-0.3f, 0f), new Vector2(0.3f, 0f),
                               new Vector2(-0.3f, -0.3f), new Vector2(0.3f, -0.3f) }
            };

            int index = Mathf.Clamp(value - 1, 0, 5);
            Vector2[] positions = dotPatterns[index];

            RectTransform parentRt = parent.GetComponent<RectTransform>();
            float parentSize = Mathf.Min(parentRt.rect.width, parentRt.rect.height);
            if (parentSize <= 0) parentSize = 200f; // Fallback

            foreach (Vector2 pos in positions)
            {
                GameObject dot = new GameObject("Dot");
                dot.transform.SetParent(parent, false);

                RectTransform dotRt = dot.AddComponent<RectTransform>();
                dotRt.anchorMin = new Vector2(0.5f, 0.5f);
                dotRt.anchorMax = new Vector2(0.5f, 0.5f);
                dotRt.sizeDelta = new Vector2(dotSize, dotSize);
                dotRt.anchoredPosition = pos * parentSize;

                Image dotImg = dot.AddComponent<Image>();
                dotImg.color = dotColor;

                // Round appearance
                Outline outline = dot.AddComponent<Outline>();
                outline.effectColor = new Color(0, 0, 0, 0.4f);
                outline.effectDistance = new Vector2(1, -1);
            }
        }

        #endregion
    }
}
