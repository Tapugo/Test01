using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;

namespace Incredicer.UI
{
    /// <summary>
    /// Settings UI for managing sound effects, music, and haptics.
    /// Creates a settings button and popup using the GUI-CasualFantasy style.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        public static SettingsUI Instance { get; private set; }

        [Header("Settings Button")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private Image settingsButtonIcon;

        [Header("Popup")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private CanvasGroup popupCanvasGroup;
        [SerializeField] private RectTransform popupContent;

        [Header("Toggles")]
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle hapticsToggle;

        [Header("Toggle Images")]
        [SerializeField] private Image sfxToggleImage;
        [SerializeField] private Image musicToggleImage;
        [SerializeField] private Image hapticsToggleImage;

        // Custom switch references for animation
        private RectTransform sfxKnob;
        private RectTransform musicKnob;
        private RectTransform hapticsKnob;
        private TextMeshProUGUI sfxStatusText;
        private TextMeshProUGUI musicStatusText;
        private TextMeshProUGUI hapticsStatusText;

        [Header("Close Button")]
        [SerializeField] private Button closeButton;

        [Header("Sprites")]
        [SerializeField] private Sprite settingsIconSprite;
        [SerializeField] private Sprite toggleOnSprite;
        [SerializeField] private Sprite toggleOffSprite;
        [SerializeField] private Sprite popupBgSprite;
        [SerializeField] private Sprite closeButtonSprite;

        // State
        private bool isOpen = false;
        private bool isInitialized = false;

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
            // Load sprites
            LoadSprites();

            // Try to build UI if not already set up
            if (settingsButton == null)
            {
                BuildUI();
            }

            SetupListeners();
            LoadSettings();
            isInitialized = true;

            // Start with popup hidden
            if (popupPanel != null)
            {
                popupPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Loads all required sprites from the GUI package via GUISpriteAssets.
        /// </summary>
        private void LoadSprites()
        {
            // Load sprites from GUISpriteAssets (works in builds, not just editor)
            GUISpriteAssets guiAssets = GUISpriteAssets.Instance;

            if (guiAssets != null)
            {
                if (settingsIconSprite == null)
                    settingsIconSprite = guiAssets.iconSettings;

                if (toggleOnSprite == null)
                    toggleOnSprite = guiAssets.toggleOn;

                if (toggleOffSprite == null)
                    toggleOffSprite = guiAssets.toggleOff;

                if (popupBgSprite == null)
                    popupBgSprite = guiAssets.popupBackground;

                if (closeButtonSprite == null)
                    closeButtonSprite = guiAssets.buttonGreen;
            }

            // Fallback: try loading from AssetDatabase in editor if GUISpriteAssets not set up
#if UNITY_EDITOR
            if (settingsIconSprite == null)
            {
                settingsIconSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/IconMisc/Icon_Setting01.Png");
            }

            if (toggleOnSprite == null)
            {
                toggleOnSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/UI_Etc/Toggle01_White_On.png");
            }

            if (toggleOffSprite == null)
            {
                toggleOffSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/UI_Etc/Toggle01_White_Off.png");
            }

            if (popupBgSprite == null)
            {
                popupBgSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Popup/Popup01_White1.Png");
            }

            if (closeButtonSprite == null)
            {
                closeButtonSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Button/Button01_Demo_Green.png");
            }
#endif
        }

        /// <summary>
        /// Builds the settings UI elements programmatically.
        /// </summary>
        private void BuildUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[SettingsUI] No canvas found!");
                return;
            }

            // Create settings button in top right (no background, just icon)
            CreateSettingsButton();

            // Create popup panel
            CreatePopupPanel();

            Debug.Log("[SettingsUI] UI built successfully");
        }

        /// <summary>
        /// Creates the settings button in the top right corner (icon only, no background).
        /// </summary>
        private void CreateSettingsButton()
        {
            try
            {
                // Create button container
                GameObject buttonObj = new GameObject("SettingsButton");
                buttonObj.transform.SetParent(transform, false);

                RectTransform rt = buttonObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(1, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0.5f, 1);
                rt.anchoredPosition = new Vector2(-130, -20); // Centered above currency counter (which is 220 wide at x=-20)
                rt.sizeDelta = new Vector2(60, 60);

                // Add Image component for the icon (acts as button graphic)
                settingsButtonIcon = buttonObj.AddComponent<Image>();
                if (settingsIconSprite != null)
                {
                    settingsButtonIcon.sprite = settingsIconSprite;
                }
                settingsButtonIcon.color = Color.white;
                settingsButtonIcon.raycastTarget = true;

                // Add button component
                settingsButton = buttonObj.AddComponent<Button>();
                settingsButton.targetGraphic = settingsButtonIcon;

                // Set button colors for visual feedback
                ColorBlock colors = settingsButton.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
                colors.selectedColor = Color.white;
                settingsButton.colors = colors;

                Debug.Log("[SettingsUI] Settings button created (icon only)");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SettingsUI] Error creating settings button: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Creates the settings popup panel with GUI package styling.
        /// </summary>
        private void CreatePopupPanel()
        {
            // Create dark overlay
            GameObject overlayObj = new GameObject("SettingsPopup");
            overlayObj.transform.SetParent(transform, false);

            RectTransform overlayRt = overlayObj.AddComponent<RectTransform>();
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.offsetMin = Vector2.zero;
            overlayRt.offsetMax = Vector2.zero;

            Image overlayBg = overlayObj.AddComponent<Image>();
            overlayBg.color = new Color(0, 0, 0, 0.7f);

            // Make overlay clickable to close
            Button overlayButton = overlayObj.AddComponent<Button>();
            overlayButton.onClick.AddListener(Hide);
            overlayButton.transition = Selectable.Transition.None;

            popupPanel = overlayObj;
            popupCanvasGroup = overlayObj.AddComponent<CanvasGroup>();

            // Create popup content panel with GUI package background
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(overlayObj.transform, false);

            popupContent = contentObj.AddComponent<RectTransform>();
            popupContent.anchorMin = new Vector2(0.5f, 0.5f);
            popupContent.anchorMax = new Vector2(0.5f, 0.5f);
            popupContent.pivot = new Vector2(0.5f, 0.5f);
            popupContent.sizeDelta = new Vector2(580, 750); // Taller popup for more spacing

            // Panel background using GUI package sprite
            Image contentBg = contentObj.AddComponent<Image>();
            if (popupBgSprite != null)
            {
                contentBg.sprite = popupBgSprite;
                contentBg.type = Image.Type.Sliced;
                contentBg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            }
            else
            {
                contentBg.color = new Color(0.15f, 0.15f, 0.2f, 0.98f);
            }

            // Prevent clicks from closing when clicking content
            Button contentBlocker = contentObj.AddComponent<Button>();
            contentBlocker.transition = Selectable.Transition.None;

            // Create title
            CreateTitle(contentObj.transform);

            // Create toggles with proper styling - more spacing for taller popup
            float yPos = 150f;
            CreateStyledToggleRow(contentObj.transform, "Sound Effects", yPos, out sfxToggle, out sfxToggleImage, out sfxKnob, out sfxStatusText);
            yPos -= 140f;
            CreateStyledToggleRow(contentObj.transform, "Music", yPos, out musicToggle, out musicToggleImage, out musicKnob, out musicStatusText);
            yPos -= 140f;
            CreateStyledToggleRow(contentObj.transform, "Haptics", yPos, out hapticsToggle, out hapticsToggleImage, out hapticsKnob, out hapticsStatusText);

            // Create close button with extra spacing below haptics
            CreateCloseButton(contentObj.transform);
        }

        /// <summary>
        /// Creates the popup title.
        /// </summary>
        private void CreateTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent, false);

            RectTransform rt = titleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -20);
            rt.sizeDelta = new Vector2(0, 60);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Settings";
            titleText.fontSize = 44;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                titleText.font = GameUI.Instance.SharedFont;
            }

            GameUI.ApplyTextOutline(titleText, 0.25f);
        }

        /// <summary>
        /// Creates a styled toggle row with a custom horizontal slider switch.
        /// </summary>
        private void CreateStyledToggleRow(Transform parent, string labelText, float yPosition,
            out Toggle toggle, out Image toggleImage, out RectTransform knob, out TextMeshProUGUI statusText)
        {
            // Row container
            GameObject rowObj = new GameObject($"Row_{labelText.Replace(" ", "")}");
            rowObj.transform.SetParent(parent, false);

            RectTransform rowRt = rowObj.AddComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0.5f, 0.5f);
            rowRt.anchorMax = new Vector2(0.5f, 0.5f);
            rowRt.pivot = new Vector2(0.5f, 0.5f);
            rowRt.anchoredPosition = new Vector2(0, yPosition);
            rowRt.sizeDelta = new Vector2(520, 110);

            // Row background for visual separation
            Image rowBg = rowObj.AddComponent<Image>();
            rowBg.color = new Color(0.25f, 0.25f, 0.3f, 0.5f);

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(rowObj.transform, false);

            RectTransform labelRt = labelObj.AddComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0, 0);
            labelRt.anchorMax = new Vector2(0.5f, 1);
            labelRt.offsetMin = new Vector2(25, 0);
            labelRt.offsetMax = Vector2.zero;

            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.text = labelText;
            label.fontSize = 34;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.color = Color.white;

            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                label.font = GameUI.Instance.SharedFont;
            }

            GameUI.ApplyTextOutline(label, 0.15f);

            // Create custom horizontal switch
            GameObject switchContainer = new GameObject("SwitchContainer");
            switchContainer.transform.SetParent(rowObj.transform, false);

            RectTransform switchContainerRt = switchContainer.AddComponent<RectTransform>();
            switchContainerRt.anchorMin = new Vector2(0.5f, 0.5f);
            switchContainerRt.anchorMax = new Vector2(0.5f, 0.5f);
            switchContainerRt.pivot = new Vector2(0f, 0.5f);
            switchContainerRt.anchoredPosition = new Vector2(30, 0);
            switchContainerRt.sizeDelta = new Vector2(180, 70); // Wide horizontal track

            // Switch track background (the pill-shaped background)
            Image trackBg = switchContainer.AddComponent<Image>();
            trackBg.color = new Color(0.2f, 0.6f, 0.3f, 1f); // Green when ON
            trackBg.raycastTarget = true;

            // Create the sliding knob
            GameObject knobObj = new GameObject("Knob");
            knobObj.transform.SetParent(switchContainer.transform, false);

            RectTransform knobRt = knobObj.AddComponent<RectTransform>();
            knobRt.anchorMin = new Vector2(0.5f, 0.5f);
            knobRt.anchorMax = new Vector2(0.5f, 0.5f);
            knobRt.pivot = new Vector2(0.5f, 0.5f);
            knobRt.anchoredPosition = new Vector2(55, 0); // Right side when ON
            knobRt.sizeDelta = new Vector2(60, 60); // Square knob

            Image knobImage = knobObj.AddComponent<Image>();
            knobImage.color = Color.white;
            knobImage.raycastTarget = false;

            // ON/OFF text label on the track
            GameObject statusTextObj = new GameObject("StatusText");
            statusTextObj.transform.SetParent(switchContainer.transform, false);

            RectTransform statusTextRt = statusTextObj.AddComponent<RectTransform>();
            statusTextRt.anchorMin = Vector2.zero;
            statusTextRt.anchorMax = Vector2.one;
            statusTextRt.offsetMin = Vector2.zero;
            statusTextRt.offsetMax = Vector2.zero;

            TextMeshProUGUI statusTextTmp = statusTextObj.AddComponent<TextMeshProUGUI>();
            statusTextTmp.text = "ON";
            statusTextTmp.fontSize = 28;
            statusTextTmp.fontStyle = FontStyles.Bold;
            statusTextTmp.alignment = TextAlignmentOptions.MidlineLeft;
            statusTextTmp.margin = new Vector4(15, 0, 0, 0); // Left padding
            statusTextTmp.color = Color.white;
            statusTextTmp.raycastTarget = false;

            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                statusTextTmp.font = GameUI.Instance.SharedFont;
            }

            // Toggle component on the container
            toggle = switchContainer.AddComponent<Toggle>();
            toggle.targetGraphic = trackBg;
            toggle.graphic = null;
            toggle.isOn = true;

            // Store references for animation
            toggleImage = trackBg;
            knob = knobRt;
            statusText = statusTextTmp;

            // Local references for the listener closure
            Image currentTrack = trackBg;
            RectTransform currentKnob = knobRt;
            TextMeshProUGUI currentStatusText = statusTextTmp;

            // Update visual on value change with animation
            toggle.onValueChanged.AddListener((isOn) => {
                UpdateCustomSwitchVisual(currentTrack, currentKnob, currentStatusText, isOn);
            });
        }

        /// <summary>
        /// Updates the custom horizontal switch visual with sliding animation.
        /// </summary>
        private void UpdateCustomSwitchVisual(Image track, RectTransform knob, TextMeshProUGUI statusText, bool isOn)
        {
            if (track == null || knob == null) return;

            // Animate knob position
            float targetX = isOn ? 55f : -55f; // Right when ON, Left when OFF
            knob.DOAnchorPosX(targetX, 0.2f).SetEase(Ease.OutCubic);

            // Animate track color
            Color targetColor = isOn
                ? new Color(0.2f, 0.6f, 0.3f, 1f)  // Green when ON
                : new Color(0.4f, 0.4f, 0.45f, 1f); // Gray when OFF
            track.DOColor(targetColor, 0.2f);

            // Update status text
            if (statusText != null)
            {
                statusText.text = isOn ? "ON" : "OFF";
                statusText.alignment = isOn ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.MidlineRight;
                statusText.margin = isOn ? new Vector4(15, 0, 0, 0) : new Vector4(0, 0, 15, 0);
            }
        }


        /// <summary>
        /// Creates the close button using GUI package styling.
        /// </summary>
        private void CreateCloseButton(Transform parent)
        {
            GameObject buttonObj = new GameObject("CloseButton");
            buttonObj.transform.SetParent(parent, false);

            RectTransform rt = buttonObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 35); // More spacing from bottom
            rt.sizeDelta = new Vector2(220, 70); // Larger button

            Image bg = buttonObj.AddComponent<Image>();
            if (closeButtonSprite != null)
            {
                bg.sprite = closeButtonSprite;
                bg.type = Image.Type.Sliced;
                bg.color = Color.white;
            }
            else
            {
                bg.color = new Color(0.3f, 0.7f, 0.4f, 1f);
            }

            closeButton = buttonObj.AddComponent<Button>();
            closeButton.targetGraphic = bg;

            ColorBlock colors = closeButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            closeButton.colors = colors;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(0, 5); // Slight offset for button shape
            textRt.offsetMax = new Vector2(0, 5);

            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Close";
            buttonText.fontSize = 36; // Larger text
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;

            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                buttonText.font = GameUI.Instance.SharedFont;
            }

            GameUI.ApplyTextOutline(buttonText, 0.2f);
        }

        /// <summary>
        /// Sets up button and toggle listeners.
        /// </summary>
        private void SetupListeners()
        {
            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }

            if (sfxToggle != null)
            {
                sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);
            }

            if (musicToggle != null)
            {
                musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
            }

            if (hapticsToggle != null)
            {
                hapticsToggle.onValueChanged.AddListener(OnHapticsToggleChanged);
            }
        }

        /// <summary>
        /// Called when settings button is clicked.
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            HapticManager.Instance?.LightHaptic();
            Toggle();
        }

        /// <summary>
        /// Loads settings from PlayerPrefs and applies to toggles.
        /// </summary>
        private void LoadSettings()
        {
            // Load SFX setting
            bool sfxEnabled = PlayerPrefs.GetInt("SfxEnabled", 1) == 1;
            if (sfxToggle != null)
            {
                sfxToggle.SetIsOnWithoutNotify(sfxEnabled);
                InitializeCustomSwitchVisual(sfxToggleImage, sfxKnob, sfxStatusText, sfxEnabled);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SfxEnabled = sfxEnabled;
            }

            // Load Music setting
            bool musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            if (musicToggle != null)
            {
                musicToggle.SetIsOnWithoutNotify(musicEnabled);
                InitializeCustomSwitchVisual(musicToggleImage, musicKnob, musicStatusText, musicEnabled);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.MusicEnabled = musicEnabled;
            }

            // Load Haptics setting
            bool hapticsEnabled = PlayerPrefs.GetInt("HapticsEnabled", 1) == 1;
            if (hapticsToggle != null)
            {
                hapticsToggle.SetIsOnWithoutNotify(hapticsEnabled);
                InitializeCustomSwitchVisual(hapticsToggleImage, hapticsKnob, hapticsStatusText, hapticsEnabled);
            }
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.HapticsEnabled = hapticsEnabled;
            }
        }

        /// <summary>
        /// Initializes the custom switch visual without animation (for startup).
        /// </summary>
        private void InitializeCustomSwitchVisual(Image track, RectTransform knob, TextMeshProUGUI statusText, bool isOn)
        {
            if (track == null || knob == null) return;

            // Set knob position immediately (no animation)
            float targetX = isOn ? 55f : -55f;
            knob.anchoredPosition = new Vector2(targetX, 0);

            // Set track color immediately
            track.color = isOn
                ? new Color(0.2f, 0.6f, 0.3f, 1f)  // Green when ON
                : new Color(0.4f, 0.4f, 0.45f, 1f); // Gray when OFF

            // Update status text
            if (statusText != null)
            {
                statusText.text = isOn ? "ON" : "OFF";
                statusText.alignment = isOn ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.MidlineRight;
                statusText.margin = isOn ? new Vector4(15, 0, 0, 0) : new Vector4(0, 0, 15, 0);
            }
        }

        /// <summary>
        /// Toggles the settings popup.
        /// </summary>
        public void Toggle()
        {
            if (isOpen)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        /// <summary>
        /// Shows the settings popup.
        /// </summary>
        public void Show()
        {
            if (popupPanel == null) return;

            isOpen = true;
            popupPanel.SetActive(true);

            // Animate in
            if (popupCanvasGroup != null)
            {
                popupCanvasGroup.alpha = 0f;
                popupCanvasGroup.DOFade(1f, 0.2f);
            }

            if (popupContent != null)
            {
                popupContent.localScale = Vector3.one * 0.8f;
                popupContent.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            }

            AudioManager.Instance?.PlayButtonClickSound();
        }

        /// <summary>
        /// Hides the settings popup.
        /// </summary>
        public void Hide()
        {
            if (popupPanel == null || !isOpen) return;

            isOpen = false;

            // Animate out
            Sequence hideSeq = DOTween.Sequence();

            if (popupContent != null)
            {
                hideSeq.Append(popupContent.DOScale(0.8f, 0.15f).SetEase(Ease.InBack));
            }

            if (popupCanvasGroup != null)
            {
                hideSeq.Join(popupCanvasGroup.DOFade(0f, 0.15f));
            }

            hideSeq.OnComplete(() =>
            {
                popupPanel.SetActive(false);
            });

            AudioManager.Instance?.PlayButtonClickSound();
        }

        /// <summary>
        /// Called when SFX toggle changes.
        /// </summary>
        private void OnSfxToggleChanged(bool isOn)
        {
            if (!isInitialized) return;

            PlayerPrefs.SetInt("SfxEnabled", isOn ? 1 : 0);
            PlayerPrefs.Save();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SfxEnabled = isOn;
            }

            HapticManager.Instance?.SelectionHaptic();

            if (isOn)
            {
                AudioManager.Instance?.PlayButtonClickSound();
            }
        }

        /// <summary>
        /// Called when Music toggle changes.
        /// </summary>
        private void OnMusicToggleChanged(bool isOn)
        {
            if (!isInitialized) return;

            PlayerPrefs.SetInt("MusicEnabled", isOn ? 1 : 0);
            PlayerPrefs.Save();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.MusicEnabled = isOn;
            }

            HapticManager.Instance?.SelectionHaptic();
            AudioManager.Instance?.PlayButtonClickSound();
        }

        /// <summary>
        /// Called when Haptics toggle changes.
        /// </summary>
        private void OnHapticsToggleChanged(bool isOn)
        {
            if (!isInitialized) return;

            PlayerPrefs.SetInt("HapticsEnabled", isOn ? 1 : 0);
            PlayerPrefs.Save();

            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.HapticsEnabled = isOn;
            }

            AudioManager.Instance?.PlayButtonClickSound();

            if (isOn)
            {
                HapticManager.Instance?.SuccessHaptic();
            }
        }
    }
}
