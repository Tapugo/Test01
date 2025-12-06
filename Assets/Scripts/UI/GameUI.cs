using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.Dice;
using Incredicer.Skills;

namespace Incredicer.UI
{
    /// <summary>
    /// Main game UI controller - handles currency display and shop buttons.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }

        [Header("Currency Display")]
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private RectTransform moneyCounterTarget;
        [SerializeField] private TextMeshProUGUI darkMatterText;
        [SerializeField] private RectTransform darkMatterCounterTarget;
        [SerializeField] private GameObject darkMatterPanel;

        [Header("Shop Buttons")]
        [SerializeField] private Button buyDiceButton;
        [SerializeField] private TextMeshProUGUI buyDiceButtonText;
        [SerializeField] private Button upgradeDiceButton;
        [SerializeField] private TextMeshProUGUI upgradeDiceButtonText;
        [SerializeField] private Button darkMatterGeneratorButton;
        [SerializeField] private TextMeshProUGUI darkMatterGeneratorButtonText;
        [SerializeField] private Button diceShopButton;
        [SerializeField] private TextMeshProUGUI diceShopButtonText;

        [Header("Dice Shop Panel")]
        [SerializeField] private GameObject diceShopPanel;
        [SerializeField] private Transform diceShopContent;

        [Header("Bottom Shop Panel")]
        [SerializeField] private RectTransform bottomShopPanel;

        [Header("Skill Tree Button")]
        [SerializeField] private Button skillTreeButton;

        [Header("Ascend Button")]
        [SerializeField] private Button ascendButton;
        [SerializeField] private TextMeshProUGUI ascendButtonText;

        [Header("Active Skill Button")]
        [SerializeField] private Button activeSkillButton;
        [SerializeField] private TextMeshProUGUI activeSkillButtonText;

        [Header("Floating Text")]
        [SerializeField] private Transform floatingTextContainer;
        [SerializeField] private TMP_FontAsset floatingTextFont;

        /// <summary>
        /// Gets the shared game font for consistent styling across all UI.
        /// </summary>
        public TMP_FontAsset SharedFont
        {
            get
            {
                // Try to get font from floatingTextFont first
                if (floatingTextFont != null) return floatingTextFont;

                // Fallback: try to get from any existing TextMeshProUGUI
                var existingText = GetComponentInChildren<TextMeshProUGUI>();
                if (existingText != null && existingText.font != null)
                    return existingText.font;

                return null;
            }
        }

        [Header("Settings")]
        [SerializeField] private float currencyPunchScale = 1.15f;
        [SerializeField] private float currencyPunchDuration = 0.15f;

        // Upgrade tracking
        private double diceValueUpgradeCost = 25;
        private double diceValueUpgradeCostMultiplier = 1.8;

        // Dark Matter Generator
        private const double DARK_MATTER_GENERATOR_COST = 1000;
        private bool darkMatterGeneratorPurchased = false;

        // Reference to main camera for world to screen conversion
        private Camera mainCamera;
        private Canvas canvas;
        private RectTransform canvasRect;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            canvas = GetComponent<Canvas>();
            canvasRect = GetComponent<RectTransform>();
            mainCamera = Camera.main;

            Debug.Log("[GameUI] Awake - Instance set");
        }

        private void Start()
        {
            mainCamera = Camera.main;

            // Fix bottom shop panel to fill screen edges
            FixBottomShopPanel();

            // Create floating text container if not assigned
            if (floatingTextContainer == null)
            {
                GameObject container = new GameObject("FloatingTextContainer");
                container.transform.SetParent(transform);
                RectTransform rt = container.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                floatingTextContainer = container.transform;
            }

            // Try to find TiltWarp font from GUI package
            if (floatingTextFont == null)
            {
                floatingTextFont = Resources.Load<TMP_FontAsset>("TMP_TiltWarp");
                if (floatingTextFont == null)
                {
                    // Fallback to default
                    floatingTextFont = TMP_Settings.defaultFontAsset;
                }
            }

            // Subscribe to currency changes - use Invoke to wait for other singletons
            Invoke(nameof(SetupSubscriptions), 0.1f);

            // Setup floating currency effect targets (delayed to ensure FloatingCurrencyEffect is ready)
            Invoke(nameof(SetupFloatingCurrencyTargets), 0.2f);

            // Setup button listeners
            Debug.Log($"[GameUI] Setting up buttons - buyDiceButton: {(buyDiceButton != null ? "OK" : "NULL")}, upgradeDiceButton: {(upgradeDiceButton != null ? "OK" : "NULL")}");

            if (buyDiceButton != null)
            {
                buyDiceButton.onClick.RemoveAllListeners();
                buyDiceButton.onClick.AddListener(OnBuyDiceClicked);
                Debug.Log("[GameUI] Buy dice button listener added");
            }
            else
            {
                Debug.LogError("[GameUI] buyDiceButton is NULL - cannot add listener!");
            }

            if (upgradeDiceButton != null)
            {
                upgradeDiceButton.onClick.RemoveAllListeners();
                upgradeDiceButton.onClick.AddListener(OnUpgradeDiceClicked);
                Debug.Log("[GameUI] Upgrade dice button listener added");
            }
            else
            {
                Debug.LogError("[GameUI] upgradeDiceButton is NULL - cannot add listener!");
            }

            if (skillTreeButton != null)
            {
                skillTreeButton.onClick.RemoveAllListeners();
                skillTreeButton.onClick.AddListener(OnSkillTreeClicked);
                Debug.Log("[GameUI] Skill tree button listener added");
            }

            if (ascendButton != null)
            {
                ascendButton.onClick.RemoveAllListeners();
                ascendButton.onClick.AddListener(OnAscendClicked);
                Debug.Log("[GameUI] Ascend button listener added");
            }

            if (activeSkillButton != null)
            {
                activeSkillButton.onClick.RemoveAllListeners();
                activeSkillButton.onClick.AddListener(OnActiveSkillClicked);
                Debug.Log("[GameUI] Active skill button listener added");
            }

            if (darkMatterGeneratorButton != null)
            {
                darkMatterGeneratorButton.onClick.RemoveAllListeners();
                darkMatterGeneratorButton.onClick.AddListener(OnDarkMatterGeneratorClicked);
                Debug.Log("[GameUI] Dark Matter Generator button listener added");
            }

            if (diceShopButton != null)
            {
                diceShopButton.onClick.RemoveAllListeners();
                diceShopButton.onClick.AddListener(OnDiceShopClicked);
                Debug.Log("[GameUI] Dice Shop button listener added");
            }

            // Initialize dice shop panel as hidden
            if (diceShopPanel != null)
            {
                diceShopPanel.SetActive(false);
            }

            // Initial UI update (delayed to ensure managers are ready)
            Invoke(nameof(UpdateAllUI), 0.15f);

            // Apply shared font to all buttons for consistent styling
            ApplySharedFontToAllButtons();

            // Apply black outlines to all text for better readability
            ApplyTextOutlinesToAll();
        }

        /// <summary>
        /// Applies the shared font to all button texts for consistent styling.
        /// </summary>
        private void ApplySharedFontToAllButtons()
        {
            if (floatingTextFont == null) return;

            // Apply font to dice shop button
            if (diceShopButtonText != null)
            {
                diceShopButtonText.font = floatingTextFont;
            }

            // Apply font to other button texts
            if (buyDiceButtonText != null) buyDiceButtonText.font = floatingTextFont;
            if (upgradeDiceButtonText != null) upgradeDiceButtonText.font = floatingTextFont;
            if (darkMatterGeneratorButtonText != null) darkMatterGeneratorButtonText.font = floatingTextFont;
            if (ascendButtonText != null) ascendButtonText.font = floatingTextFont;
            if (activeSkillButtonText != null) activeSkillButtonText.font = floatingTextFont;

            // Apply font to skill tree button text if it has one
            if (skillTreeButton != null)
            {
                var skillTreeButtonText = skillTreeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (skillTreeButtonText != null)
                {
                    skillTreeButtonText.font = floatingTextFont;
                }
            }

            Debug.Log("[GameUI] Applied shared font to all button texts");
        }

        /// <summary>
        /// Applies black outlines to all TextMeshProUGUI elements in the UI.
        /// </summary>
        private void ApplyTextOutlinesToAll()
        {
            TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTexts)
            {
                ApplyTextOutline(tmp);
            }
            Debug.Log($"[GameUI] Applied black outlines to {allTexts.Length} text elements");
        }

        /// <summary>
        /// Applies a thin black outline to a TextMeshProUGUI for better contrast.
        /// Safely handles cases where the font/material isn't ready yet.
        /// </summary>
        public static void ApplyTextOutline(TextMeshProUGUI tmp, float thickness = 0.15f)
        {
            if (tmp == null) return;

            // Check if font asset and material are valid before setting outline
            if (tmp.font == null || tmp.fontSharedMaterial == null)
            {
                // Skip - font or material not ready
                return;
            }

            try
            {
                tmp.outlineWidth = thickness;
                tmp.outlineColor = new Color(0f, 0f, 0f, 1f);
            }
            catch (System.Exception)
            {
                // Silently ignore if outline can't be applied (material issue)
            }
        }

        private void Update()
        {
            // Update active skill button cooldown display
            UpdateActiveSkillButton();
        }

        private void SetupSubscriptions()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
                CurrencyManager.Instance.OnDarkMatterChanged -= UpdateDarkMatterDisplay;
                CurrencyManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
                CurrencyManager.Instance.OnDarkMatterChanged += UpdateDarkMatterDisplay;
            }
        }

        private void SetupFloatingCurrencyTargets()
        {
            if (FloatingCurrencyEffect.Instance == null)
            {
                Debug.LogWarning("[GameUI] FloatingCurrencyEffect.Instance not ready, will retry...");
                return;
            }

            if (moneyCounterTarget != null)
            {
                FloatingCurrencyEffect.Instance.SetMoneyTarget(moneyCounterTarget);
            }
            else if (moneyText != null)
            {
                FloatingCurrencyEffect.Instance.SetMoneyTarget(moneyText.GetComponent<RectTransform>());
            }

            if (darkMatterCounterTarget != null)
            {
                FloatingCurrencyEffect.Instance.SetDarkMatterTarget(darkMatterCounterTarget);
            }
            else if (darkMatterText != null)
            {
                FloatingCurrencyEffect.Instance.SetDarkMatterTarget(darkMatterText.GetComponent<RectTransform>());
            }

            // Subscribe to floating currency effect events
            FloatingCurrencyEffect.Instance.OnMoneyReachedCounter -= OnFloatingMoneyReached;
            FloatingCurrencyEffect.Instance.OnDarkMatterReachedCounter -= OnFloatingDarkMatterReached;
            FloatingCurrencyEffect.Instance.OnMoneyReachedCounter += OnFloatingMoneyReached;
            FloatingCurrencyEffect.Instance.OnDarkMatterReachedCounter += OnFloatingDarkMatterReached;

            Debug.Log("[GameUI] Floating currency targets setup complete");
        }

        private void OnFloatingMoneyReached(double amount)
        {
            // Add the money when the effect reaches the counter
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddMoneyDirect(amount);
            }
        }

        private void OnFloatingDarkMatterReached(double amount)
        {
            // Add the dark matter when the effect reaches the counter
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddDarkMatterDirect(amount);
            }
        }

        private void OnDestroy()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
                CurrencyManager.Instance.OnDarkMatterChanged -= UpdateDarkMatterDisplay;
            }

            if (FloatingCurrencyEffect.Instance != null)
            {
                FloatingCurrencyEffect.Instance.OnMoneyReachedCounter -= OnFloatingMoneyReached;
                FloatingCurrencyEffect.Instance.OnDarkMatterReachedCounter -= OnFloatingDarkMatterReached;
            }
        }

        /// <summary>
        /// Fixes the bottom shop panel to fill the screen edges.
        /// </summary>
        private void FixBottomShopPanel()
        {
            // Try to find the ShopPanel if not assigned
            if (bottomShopPanel == null)
            {
                Transform shopPanelTransform = transform.Find("ShopPanel");
                if (shopPanelTransform != null)
                {
                    bottomShopPanel = shopPanelTransform.GetComponent<RectTransform>();
                }
            }

            if (bottomShopPanel != null)
            {
                // Make it stretch to fill bottom and sides
                // Panel height matches button height (150) with bottom edge at screen bottom
                bottomShopPanel.anchorMin = new Vector2(0, 0);
                bottomShopPanel.anchorMax = new Vector2(1, 0);
                bottomShopPanel.pivot = new Vector2(0.5f, 0);
                bottomShopPanel.offsetMin = new Vector2(0, 0);   // Left = 0, Bottom = 0 (at screen edge)
                bottomShopPanel.offsetMax = new Vector2(0, 150); // Right = 0 (stretch), Top = 150 (button height)

                // Update background color if needed
                Image bg = bottomShopPanel.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = new Color(0.1f, 0.1f, 0.15f, 0.92f);
                }

                Debug.Log("[GameUI] Fixed bottom shop panel to fill screen edges");
            }
            else
            {
                Debug.LogWarning("[GameUI] Could not find ShopPanel to fix");
            }
        }

        /// <summary>
        /// Updates all UI elements.
        /// </summary>
        public void UpdateAllUI()
        {
            if (CurrencyManager.Instance != null)
            {
                UpdateMoneyDisplay(CurrencyManager.Instance.Money);
                UpdateDarkMatterDisplay(CurrencyManager.Instance.DarkMatter);
            }
            UpdateButtonTexts();
            UpdateAscendButton();
            UpdateDarkMatterPanelVisibility();
            UpdateDarkMatterGeneratorButton();

            // Sync darkMatterGeneratorPurchased with actual unlock state
            if (DiceManager.Instance != null && DiceManager.Instance.DarkMatterUnlocked)
            {
                darkMatterGeneratorPurchased = true;
            }
        }

        /// <summary>
        /// Updates dark matter panel visibility based on unlock status.
        /// </summary>
        private void UpdateDarkMatterPanelVisibility()
        {
            if (darkMatterPanel != null && DiceManager.Instance != null)
            {
                darkMatterPanel.SetActive(DiceManager.Instance.DarkMatterUnlocked);
            }
        }

        /// <summary>
        /// Updates the money display with animation.
        /// </summary>
        private void UpdateMoneyDisplay(double amount)
        {
            if (moneyText == null) return;

            moneyText.text = $"${FormatNumber(amount)}";

            // Punch animation
            moneyText.transform.DOKill();
            moneyText.transform.localScale = Vector3.one;
            moneyText.transform.DOPunchScale(Vector3.one * (currencyPunchScale - 1f), currencyPunchDuration, 5, 0.5f);

            // Update button states when money changes
            UpdateButtonTexts();
            UpdateAscendButton();
        }

        /// <summary>
        /// Updates the dark matter display.
        /// </summary>
        private void UpdateDarkMatterDisplay(double amount)
        {
            if (darkMatterText == null) return;

            darkMatterText.text = $"DM: {FormatNumber(amount)}";

            // Show/hide dark matter panel based on unlock status
            if (darkMatterPanel != null && DiceManager.Instance != null)
            {
                bool shouldShow = DiceManager.Instance.DarkMatterUnlocked;
                bool wasHidden = !darkMatterPanel.activeSelf;

                darkMatterPanel.SetActive(shouldShow);

                // Punch animation when first showing
                if (shouldShow && wasHidden)
                {
                    darkMatterPanel.transform.localScale = Vector3.one;
                    darkMatterPanel.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
                }
            }
        }

        /// <summary>
        /// Updates button texts with current prices and visual states.
        /// Buttons are darkened when player can't afford them.
        /// </summary>
        private void UpdateButtonTexts()
        {
            double currentMoney = CurrencyManager.Instance != null ? CurrencyManager.Instance.Money : 0;

            // Buy dice button - keep interactable for feedback but darken if can't afford
            if (buyDiceButton != null && DiceManager.Instance != null)
            {
                double price = DiceManager.Instance.GetCurrentPrice(DiceType.Basic);
                bool canAfford = currentMoney >= price;
                buyDiceButton.interactable = true; // Always interactable for feedback

                // Darken button when can't afford
                SetButtonVisualState(buyDiceButton, canAfford);

                if (buyDiceButtonText != null)
                {
                    buyDiceButtonText.text = $"Buy Dice\n${FormatNumber(price)}";
                }
            }

            // Upgrade dice button - keep interactable for feedback but darken if can't afford
            if (upgradeDiceButton != null)
            {
                bool canAfford = currentMoney >= diceValueUpgradeCost;
                upgradeDiceButton.interactable = true; // Always interactable for feedback

                // Darken button when can't afford
                SetButtonVisualState(upgradeDiceButton, canAfford);

                if (upgradeDiceButtonText != null)
                {
                    int currentLevel = GameStats.Instance != null ? GameStats.Instance.DiceValueUpgradeLevel : 0;
                    string levelText = currentLevel > 0 ? $" +{currentLevel}" : "";
                    upgradeDiceButtonText.text = $"Upgrade{levelText}\n${FormatNumber(diceValueUpgradeCost)}";
                }
            }
        }

        /// <summary>
        /// Called when buy dice button is clicked.
        /// </summary>
        private void OnBuyDiceClicked()
        {
            Debug.Log("[GameUI] Buy Dice button clicked!");

            if (DiceManager.Instance == null)
            {
                Debug.LogWarning("[GameUI] DiceManager.Instance is null!");
                return;
            }

            // Check if can afford before attempting
            double price = DiceManager.Instance.GetCurrentPrice(DiceType.Basic);
            double currentMoney = CurrencyManager.Instance != null ? CurrencyManager.Instance.Money : 0;

            if (currentMoney < price)
            {
                // Show feedback and shake
                ShowNotEnoughFeedback("Not enough $!");
                buyDiceButton.transform.DOKill();
                buyDiceButton.transform.DOShakePosition(0.3f, 5f, 20);
                return;
            }

            bool success = DiceManager.Instance.TryBuyDice(DiceType.Basic);
            Debug.Log($"[GameUI] Buy dice result: {success}");

            if (success)
            {
                // Satisfying purchase animation
                PlayPurchaseAnimation(buyDiceButton.transform);
                UpdateButtonTexts();
            }
        }

        /// <summary>
        /// Plays a satisfying bounce animation and effects on successful purchase.
        /// </summary>
        private void PlayPurchaseAnimation(Transform buttonTransform)
        {
            if (buttonTransform == null) return;

            buttonTransform.DOKill();
            buttonTransform.localScale = Vector3.one;

            // Create satisfying sequence: quick squeeze then bounce out
            Sequence purchaseSeq = DOTween.Sequence();
            purchaseSeq.Append(buttonTransform.DOScale(0.9f, 0.05f).SetEase(Ease.InQuad));
            purchaseSeq.Append(buttonTransform.DOScale(1.15f, 0.1f).SetEase(Ease.OutBack));
            purchaseSeq.Append(buttonTransform.DOScale(1f, 0.1f).SetEase(Ease.InOutSine));

            // Subtle camera shake for tactile feedback
            if (mainCamera != null)
            {
                mainCamera.transform.DOKill();
                mainCamera.transform.DOShakePosition(0.1f, 0.02f, 15, 90f, false, true);
            }

            // Spawn purchase particle effect
            if (VisualEffectsManager.Instance != null)
            {
                Vector3 worldPos = mainCamera != null ? mainCamera.ScreenToWorldPoint(buttonTransform.position) : Vector3.zero;
                worldPos.z = 0;
                VisualEffectsManager.Instance.SpawnPurchaseEffect(worldPos);
            }

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPurchaseSound();
            }
        }

        /// <summary>
        /// Sets the visual state of a button (darkened when unavailable, normal when available).
        /// </summary>
        private void SetButtonVisualState(Button button, bool isAvailable)
        {
            if (button == null) return;

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                // Darken the button when unavailable
                Color targetColor = isAvailable ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
                buttonImage.color = targetColor;
            }

            // Also dim the text slightly when unavailable
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = isAvailable ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
            }
        }

        /// <summary>
        /// Called when skill tree button is clicked.
        /// </summary>
        private void OnSkillTreeClicked()
        {
            Debug.Log("[GameUI] Skill Tree button clicked!");

            if (SkillTreeUI.Instance != null)
            {
                SkillTreeUI.Instance.Toggle();
            }
            else
            {
                Debug.LogWarning("[GameUI] SkillTreeUI.Instance is null!");
            }
        }

        /// <summary>
        /// Called when ascend button is clicked.
        /// </summary>
        private void OnAscendClicked()
        {
            Debug.Log("[GameUI] Ascend button clicked!");

            if (PrestigeManager.Instance == null)
            {
                Debug.LogWarning("[GameUI] PrestigeManager.Instance is null!");
                return;
            }

            // Check if already ascended
            if (PrestigeManager.Instance.HasAscended)
            {
                ShowFloatingText(Vector3.zero, "Already Ascended!", new Color(0.8f, 0.5f, 1f));
                return;
            }

            if (PrestigeManager.Instance.CanPrestige())
            {
                bool success = PrestigeManager.Instance.DoPrestige();
                if (success)
                {
                    // Hide the ascend button with animation
                    if (ascendButton != null)
                    {
                        ascendButton.transform.DOKill();
                        ascendButton.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
                        {
                            ascendButton.gameObject.SetActive(false);
                        });
                    }

                    // Show Dark Matter Unlocked celebration banner!
                    ShowDarkMatterUnlockedCelebration();

                    // Screen shake for dramatic effect
                    if (mainCamera != null)
                    {
                        mainCamera.transform.DOKill();
                        mainCamera.transform.DOShakePosition(0.5f, 0.3f, 25, 90f, false, true);
                    }

                    UpdateAllUI();
                }
            }
            else
            {
                // Show feedback and shake
                double required = PrestigeManager.Instance.GetMoneyRequiredForPrestige();
                ShowNotEnoughFeedback($"Need ${FormatNumber(required)}!");
                ascendButton.transform.DOKill();
                ascendButton.transform.DOShakePosition(0.2f, 3f, 15);

                Debug.Log($"[GameUI] Cannot ascend yet. Need: ${FormatNumber(required)}");
            }
        }

        /// <summary>
        /// Shows a big celebration banner when Dark Matter is unlocked.
        /// Polished with rays, particles, and beautiful animations!
        /// </summary>
        private void ShowDarkMatterUnlockedCelebration()
        {
            // Create full-screen overlay
            GameObject overlayObj = new GameObject("DMUnlockCelebration");
            overlayObj.transform.SetParent(transform, false);

            RectTransform overlayRt = overlayObj.AddComponent<RectTransform>();
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.offsetMin = Vector2.zero;
            overlayRt.offsetMax = Vector2.zero;

            // Semi-transparent dark background with purple tint
            Image overlayBg = overlayObj.AddComponent<Image>();
            overlayBg.color = new Color(0.05f, 0f, 0.1f, 0f);
            overlayBg.raycastTarget = true;

            // === CELEBRATION RAYS (spinning light beams behind banner) ===
            GameObject raysObj = new GameObject("CelebrationRays");
            raysObj.transform.SetParent(overlayObj.transform, false);
            RectTransform raysRt = raysObj.AddComponent<RectTransform>();
            raysRt.anchorMin = new Vector2(0.5f, 0.5f);
            raysRt.anchorMax = new Vector2(0.5f, 0.5f);
            raysRt.sizeDelta = new Vector2(1200, 1200);
            raysRt.localScale = Vector3.zero;

            // Create rays as multiple rotated images
            for (int i = 0; i < 12; i++)
            {
                GameObject rayObj = new GameObject($"Ray_{i}");
                rayObj.transform.SetParent(raysObj.transform, false);
                RectTransform rayRt = rayObj.AddComponent<RectTransform>();
                rayRt.anchorMin = new Vector2(0.5f, 0.5f);
                rayRt.anchorMax = new Vector2(0.5f, 0.5f);
                rayRt.sizeDelta = new Vector2(40, 600);
                rayRt.pivot = new Vector2(0.5f, 0f);
                rayRt.localRotation = Quaternion.Euler(0, 0, i * 30);

                Image rayImg = rayObj.AddComponent<Image>();
                // Gradient from center outward - purple to transparent
                rayImg.color = new Color(0.7f, 0.4f, 1f, 0.4f);
            }

            // === GLOW CIRCLE behind banner ===
            GameObject glowObj = new GameObject("GlowCircle");
            glowObj.transform.SetParent(overlayObj.transform, false);
            RectTransform glowRt = glowObj.AddComponent<RectTransform>();
            glowRt.anchorMin = new Vector2(0.5f, 0.5f);
            glowRt.anchorMax = new Vector2(0.5f, 0.5f);
            glowRt.sizeDelta = new Vector2(600, 400);
            glowRt.localScale = Vector3.zero;

            Image glowImg = glowObj.AddComponent<Image>();
            glowImg.color = new Color(0.6f, 0.3f, 0.9f, 0.5f);

            // === BANNER CONTAINER with gradient background ===
            GameObject bannerObj = new GameObject("Banner");
            bannerObj.transform.SetParent(overlayObj.transform, false);

            RectTransform bannerRt = bannerObj.AddComponent<RectTransform>();
            bannerRt.anchorMin = new Vector2(0.05f, 0.35f);
            bannerRt.anchorMax = new Vector2(0.95f, 0.65f);
            bannerRt.offsetMin = Vector2.zero;
            bannerRt.offsetMax = Vector2.zero;
            bannerRt.localScale = Vector3.zero;

            // Beautiful gradient background (purple to dark blue)
            Image bannerBg = bannerObj.AddComponent<Image>();
            bannerBg.color = new Color(0.2f, 0.1f, 0.35f, 0.98f);

            // Add multiple outlines for glow effect
            Outline outerOutline = bannerObj.AddComponent<Outline>();
            outerOutline.effectColor = new Color(1f, 0.7f, 1f, 0.8f);
            outerOutline.effectDistance = new Vector2(6, 6);

            // Inner bright border
            GameObject borderObj = new GameObject("InnerBorder");
            borderObj.transform.SetParent(bannerObj.transform, false);
            RectTransform borderRt = borderObj.AddComponent<RectTransform>();
            borderRt.anchorMin = Vector2.zero;
            borderRt.anchorMax = Vector2.one;
            borderRt.offsetMin = new Vector2(4, 4);
            borderRt.offsetMax = new Vector2(-4, -4);
            Image borderImg = borderObj.AddComponent<Image>();
            borderImg.color = new Color(0.25f, 0.15f, 0.4f, 1f);
            Outline innerOutline = borderObj.AddComponent<Outline>();
            innerOutline.effectColor = new Color(0.9f, 0.6f, 1f, 1f);
            innerOutline.effectDistance = new Vector2(2, 2);

            // === STAR/SPARKLE DECORATIONS ===
            CreateDecorationStar(bannerObj.transform, new Vector2(-0.42f, 0.5f), 40f, new Color(1f, 0.9f, 0.5f));
            CreateDecorationStar(bannerObj.transform, new Vector2(0.42f, 0.5f), 40f, new Color(1f, 0.9f, 0.5f));
            CreateDecorationStar(bannerObj.transform, new Vector2(-0.38f, -0.4f), 30f, new Color(0.9f, 0.7f, 1f));
            CreateDecorationStar(bannerObj.transform, new Vector2(0.38f, -0.4f), 30f, new Color(0.9f, 0.7f, 1f));

            // === TITLE TEXT with gradient effect ===
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(bannerObj.transform, false);

            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 0.5f);
            titleRt.anchorMax = new Vector2(1, 1f);
            titleRt.offsetMin = new Vector2(20, 5);
            titleRt.offsetMax = new Vector2(-20, -15);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "✦ DARK MATTER ✦";
            titleText.fontSize = 68;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.95f, 0.8f, 1f);
            if (floatingTextFont != null) titleText.font = floatingTextFont;
            ApplyTextOutline(titleText, 0.25f);

            // === SUBTITLE with golden color ===
            GameObject subtitleObj = new GameObject("Subtitle");
            subtitleObj.transform.SetParent(bannerObj.transform, false);

            RectTransform subtitleRt = subtitleObj.AddComponent<RectTransform>();
            subtitleRt.anchorMin = new Vector2(0, 0);
            subtitleRt.anchorMax = new Vector2(1, 0.5f);
            subtitleRt.offsetMin = new Vector2(20, 15);
            subtitleRt.offsetMax = new Vector2(-20, -5);

            TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "UNLOCKED!";
            subtitleText.fontSize = 52;
            subtitleText.fontStyle = FontStyles.Bold;
            subtitleText.alignment = TextAlignmentOptions.Center;
            subtitleText.color = new Color(1f, 0.85f, 0.3f);
            if (floatingTextFont != null) subtitleText.font = floatingTextFont;
            ApplyTextOutline(subtitleText, 0.2f);

            // === ANIMATION SEQUENCE ===
            Sequence celebrationSeq = DOTween.Sequence();

            // Fade in background
            celebrationSeq.Append(overlayBg.DOFade(0.85f, 0.4f).SetEase(Ease.OutQuad));

            // Scale up rays with spin
            celebrationSeq.Join(raysRt.DOScale(1f, 0.6f).SetEase(Ease.OutBack));
            raysObj.transform.DORotate(new Vector3(0, 0, 360), 20f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);

            // Scale up glow
            celebrationSeq.Join(glowRt.DOScale(1.2f, 0.5f).SetEase(Ease.OutQuad));
            glowImg.DOFade(0.6f, 0.8f).SetLoops(-1, LoopType.Yoyo);

            // Pop in banner with dramatic effect
            celebrationSeq.Join(bannerRt.DOScale(1.15f, 0.35f).SetEase(Ease.OutBack));
            celebrationSeq.Append(bannerRt.DOScale(1f, 0.15f).SetEase(Ease.InOutSine));

            // Title color shimmer
            titleText.DOColor(new Color(1f, 0.9f, 1f), 0.4f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(0.5f);

            // Spawn fireworks particles
            celebrationSeq.InsertCallback(0.3f, () => SpawnCelebrationFireworks());
            celebrationSeq.InsertCallback(0.8f, () => SpawnCelebrationFireworks());
            celebrationSeq.InsertCallback(1.3f, () => SpawnCelebrationFireworks());

            // Screen flash
            celebrationSeq.InsertCallback(0.35f, () =>
            {
                if (VisualEffectsManager.Instance != null)
                {
                    VisualEffectsManager.Instance.FlashScreen(new Color(0.8f, 0.5f, 1f, 0.4f), 0.3f);
                }
            });

            // Camera shake for impact
            celebrationSeq.InsertCallback(0.35f, () =>
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    cam.transform.DOShakePosition(0.4f, 0.15f, 20, 90f, false, true);
                }
            });

            // Gentle pulse
            celebrationSeq.Append(bannerRt.DOScale(1.03f, 0.5f).SetEase(Ease.InOutSine));
            celebrationSeq.Append(bannerRt.DOScale(1f, 0.5f).SetEase(Ease.InOutSine));

            // Wait for player to appreciate
            celebrationSeq.AppendInterval(1.8f);

            // Fade out elegantly
            celebrationSeq.Append(overlayBg.DOFade(0f, 0.4f).SetEase(Ease.InQuad));
            celebrationSeq.Join(bannerRt.DOScale(0f, 0.35f).SetEase(Ease.InBack));
            celebrationSeq.Join(raysRt.DOScale(0f, 0.4f));
            celebrationSeq.Join(glowRt.DOScale(0f, 0.35f));

            // Cleanup
            celebrationSeq.OnComplete(() =>
            {
                Destroy(overlayObj);
            });
        }

        /// <summary>
        /// Creates a decorative star/sparkle for the celebration banner.
        /// </summary>
        private void CreateDecorationStar(Transform parent, Vector2 anchorPos, float size, Color color)
        {
            GameObject starObj = new GameObject("Star");
            starObj.transform.SetParent(parent, false);

            RectTransform starRt = starObj.AddComponent<RectTransform>();
            starRt.anchorMin = new Vector2(0.5f + anchorPos.x, 0.5f + anchorPos.y);
            starRt.anchorMax = starRt.anchorMin;
            starRt.sizeDelta = new Vector2(size, size);

            TextMeshProUGUI starText = starObj.AddComponent<TextMeshProUGUI>();
            starText.text = "★";
            starText.fontSize = size;
            starText.alignment = TextAlignmentOptions.Center;
            starText.color = color;

            // Twinkle animation
            starText.DOFade(0.5f, 0.3f + UnityEngine.Random.Range(0f, 0.3f))
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(UnityEngine.Random.Range(0f, 0.5f));

            starRt.DOScale(1.2f, 0.4f + UnityEngine.Random.Range(0f, 0.2f))
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(UnityEngine.Random.Range(0f, 0.3f));
        }

        /// <summary>
        /// Spawns firework particle effects for the Dark Matter celebration.
        /// </summary>
        private void SpawnCelebrationFireworks()
        {
            if (VisualEffectsManager.Instance == null) return;

            // Spawn multiple fireworks at different positions
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-3f, 2f, 0),
                new Vector3(3f, 2f, 0),
                new Vector3(-2f, -1f, 0),
                new Vector3(2f, -1f, 0),
                new Vector3(0f, 3f, 0)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                float delay = i * 0.15f;
                Vector3 pos = positions[i];
                DOVirtual.DelayedCall(delay, () =>
                {
                    VisualEffectsManager.Instance.SpawnPrestigeEffect(pos);
                });
            }

            // Play celebration sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayJackpotSound();
            }
        }

        /// <summary>
        /// Called when active skill button is clicked.
        /// </summary>
        private void OnActiveSkillClicked()
        {
            Debug.Log("[GameUI] Active Skill button clicked!");

            if (ActiveSkillManager.Instance == null)
            {
                Debug.LogWarning("[GameUI] ActiveSkillManager.Instance is null!");
                return;
            }

            // Get the best available skill
            ActiveSkillType bestSkill = ActiveSkillManager.Instance.GetBestAvailableSkill();

            if (bestSkill != ActiveSkillType.None)
            {
                bool success = ActiveSkillManager.Instance.TryActivateSkill(bestSkill);
                if (success)
                {
                    // Animate button
                    activeSkillButton.transform.DOKill();
                    activeSkillButton.transform.localScale = Vector3.one;
                    activeSkillButton.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f);

                    // Show floating text
                    string skillName = bestSkill == ActiveSkillType.Hyperburst ? "HYPERBURST!" : "ROLL BURST!";
                    ShowFloatingText(Vector3.zero, skillName, new Color(1f, 0.8f, 0.2f));
                }
            }
            else
            {
                // Check if any skill is unlocked but on cooldown
                float rollBurstCD = ActiveSkillManager.Instance.GetRemainingCooldown(ActiveSkillType.RollBurst);
                float hyperburstCD = ActiveSkillManager.Instance.GetRemainingCooldown(ActiveSkillType.Hyperburst);

                if (rollBurstCD > 0 || hyperburstCD > 0)
                {
                    float minCD = Mathf.Min(
                        rollBurstCD > 0 ? rollBurstCD : float.MaxValue,
                        hyperburstCD > 0 ? hyperburstCD : float.MaxValue
                    );
                    ShowNotEnoughFeedback($"Cooldown: {minCD:F1}s");
                }
                else
                {
                    ShowNotEnoughFeedback("Unlock Roll Burst first!");
                }

                activeSkillButton.transform.DOKill();
                activeSkillButton.transform.DOShakePosition(0.2f, 3f, 15);
            }
        }

        // Active skill button pulse state
        private bool isSkillButtonPulsing = false;
        private Tweener skillButtonPulse;

        /// <summary>
        /// Updates the active skill button text and state.
        /// </summary>
        private void UpdateActiveSkillButton()
        {
            if (activeSkillButton == null) return;

            // Hide if no skills unlocked
            if (SkillTreeManager.Instance == null)
            {
                activeSkillButton.gameObject.SetActive(false);
                StopSkillButtonPulse();
                return;
            }

            bool hasRollBurst = SkillTreeManager.Instance.IsActiveSkillUnlocked(ActiveSkillType.RollBurst);
            bool hasHyperburst = SkillTreeManager.Instance.IsActiveSkillUnlocked(ActiveSkillType.Hyperburst);

            if (!hasRollBurst && !hasHyperburst)
            {
                activeSkillButton.gameObject.SetActive(false);
                StopSkillButtonPulse();
                return;
            }

            activeSkillButton.gameObject.SetActive(true);

            // Get best skill and its cooldown
            ActiveSkillType bestSkill = hasHyperburst ? ActiveSkillType.Hyperburst : ActiveSkillType.RollBurst;
            string skillName = hasHyperburst ? "Hyperburst" : "Roll Burst";

            if (ActiveSkillManager.Instance != null)
            {
                bool isReady = ActiveSkillManager.Instance.IsSkillReady(bestSkill);
                float cooldown = ActiveSkillManager.Instance.GetRemainingCooldown(bestSkill);

                activeSkillButton.interactable = isReady;

                if (activeSkillButtonText != null)
                {
                    if (isReady)
                    {
                        activeSkillButtonText.text = $"{skillName}\n<size=60%>READY!</size>";
                        StartSkillButtonPulse();
                    }
                    else
                    {
                        activeSkillButtonText.text = $"{skillName}\n<size=60%>{cooldown:F1}s</size>";
                        StopSkillButtonPulse();
                    }
                }
            }
        }

        /// <summary>
        /// Starts a pulsing glow animation on the active skill button.
        /// </summary>
        private void StartSkillButtonPulse()
        {
            if (isSkillButtonPulsing || activeSkillButton == null) return;
            isSkillButtonPulsing = true;

            // Gentle pulse animation
            skillButtonPulse = activeSkillButton.transform.DOScale(1.08f, 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            // Also pulse the button color if it has an Image
            Image btnImage = activeSkillButton.GetComponent<Image>();
            if (btnImage != null)
            {
                Color baseColor = btnImage.color;
                btnImage.DOColor(new Color(
                    Mathf.Min(baseColor.r * 1.3f, 1f),
                    Mathf.Min(baseColor.g * 1.3f, 1f),
                    Mathf.Min(baseColor.b * 1.3f, 1f),
                    baseColor.a), 0.5f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        /// <summary>
        /// Stops the pulsing animation on the active skill button.
        /// </summary>
        private void StopSkillButtonPulse()
        {
            if (!isSkillButtonPulsing) return;
            isSkillButtonPulsing = false;

            skillButtonPulse?.Kill();

            if (activeSkillButton != null)
            {
                activeSkillButton.transform.DOKill();
                activeSkillButton.transform.localScale = Vector3.one;

                Image btnImage = activeSkillButton.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.DOKill();
                }
            }
        }

        /// <summary>
        /// Updates the ascend button text and disabled state.
        /// Hides the button after player has ascended.
        /// </summary>
        private void UpdateAscendButton()
        {
            if (ascendButton == null) return;
            if (PrestigeManager.Instance == null) return;

            bool hasAscended = PrestigeManager.Instance.HasAscended;
            bool canAscend = PrestigeManager.Instance.CanPrestige();
            double required = PrestigeManager.Instance.GetMoneyRequiredForPrestige();

            // Hide button if already ascended
            if (hasAscended)
            {
                ascendButton.gameObject.SetActive(false);
                return;
            }

            ascendButton.gameObject.SetActive(true);
            ascendButton.interactable = true; // Always interactable for feedback

            // Darken if can't afford
            SetButtonVisualState(ascendButton, canAscend);

            if (ascendButtonText != null)
            {
                ascendButtonText.text = $"Ascend\n${FormatNumber(required)}";
            }
        }

        /// <summary>
        /// Shows a "Not enough money" feedback message.
        /// </summary>
        private void ShowNotEnoughFeedback(string message)
        {
            // Show floating text at center of screen
            if (mainCamera == null) mainCamera = Camera.main;
            Vector3 centerWorld = mainCamera != null ? mainCamera.transform.position : Vector3.zero;
            centerWorld.z = 0;

            ShowFloatingText(centerWorld, message, new Color(1f, 0.4f, 0.4f));
        }

        /// <summary>
        /// Called when upgrade dice button is clicked.
        /// Increases DiceValueUpgradeLevel which adds +1 to all dice rolls.
        /// </summary>
        private void OnUpgradeDiceClicked()
        {
            Debug.Log("[GameUI] Upgrade Dice button clicked!");

            if (CurrencyManager.Instance == null)
            {
                Debug.LogWarning("[GameUI] CurrencyManager.Instance is null!");
                return;
            }

            if (GameStats.Instance == null)
            {
                Debug.LogWarning("[GameUI] GameStats.Instance is null!");
                return;
            }

            double currentMoney = CurrencyManager.Instance.Money;
            Debug.Log($"[GameUI] Current money: {currentMoney}, Upgrade cost: {diceValueUpgradeCost}");

            // Check if can afford before attempting
            if (currentMoney < diceValueUpgradeCost)
            {
                Debug.Log("[GameUI] Cannot afford upgrade - shaking button");
                // Show feedback and shake
                ShowNotEnoughFeedback("Not enough $!");
                upgradeDiceButton.transform.DOKill();
                upgradeDiceButton.transform.DOShakePosition(0.3f, 5f, 20);
                return;
            }

            if (CurrencyManager.Instance.SpendMoney(diceValueUpgradeCost))
            {
                // Increase the dice value upgrade level
                GameStats.Instance.DiceValueUpgradeLevel++;
                Debug.Log($"[GameUI] Upgrade successful! New level: {GameStats.Instance.DiceValueUpgradeLevel}");

                // Increase cost for next upgrade
                diceValueUpgradeCost *= diceValueUpgradeCostMultiplier;

                // Satisfying purchase animation
                PlayPurchaseAnimation(upgradeDiceButton.transform);
                UpdateButtonTexts();

                // Show level up feedback
                ShowFloatingText(Vector3.zero, $"Level {GameStats.Instance.DiceValueUpgradeLevel}!", new Color(0.4f, 1f, 0.4f));
            }
        }

        /// <summary>
        /// Called when Dark Matter Generator button is clicked.
        /// Unlocks Dark Matter generation and the skill tree.
        /// </summary>
        private void OnDarkMatterGeneratorClicked()
        {
            Debug.Log("[GameUI] Dark Matter Generator button clicked!");

            if (darkMatterGeneratorPurchased)
            {
                ShowNotEnoughFeedback("Already purchased!");
                return;
            }

            if (CurrencyManager.Instance == null)
            {
                Debug.LogWarning("[GameUI] CurrencyManager.Instance is null!");
                return;
            }

            if (CurrencyManager.Instance.SpendMoney(DARK_MATTER_GENERATOR_COST))
            {
                darkMatterGeneratorPurchased = true;

                // Unlock dark matter display
                if (DiceManager.Instance != null)
                {
                    DiceManager.Instance.DarkMatterUnlocked = true;
                }

                // Update UI
                UpdateDarkMatterPanelVisibility();
                UpdateDarkMatterGeneratorButton();

                // Animate button
                if (darkMatterGeneratorButton != null)
                {
                    darkMatterGeneratorButton.transform.DOKill();
                    darkMatterGeneratorButton.transform.localScale = Vector3.one;
                    darkMatterGeneratorButton.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f);
                }

                // Show feedback
                ShowFloatingText(Vector3.zero, "Dark Matter Unlocked!", new Color(0.8f, 0.5f, 1f));

                // Play sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayPurchaseSound();
                }

                Debug.Log("[GameUI] Dark Matter Generator purchased!");
            }
            else
            {
                ShowNotEnoughFeedback($"Need ${FormatNumber(DARK_MATTER_GENERATOR_COST)}!");
                if (darkMatterGeneratorButton != null)
                {
                    darkMatterGeneratorButton.transform.DOKill();
                    darkMatterGeneratorButton.transform.DOShakePosition(0.2f, 3f, 15);
                }
            }
        }

        /// <summary>
        /// Updates the Dark Matter Generator button display.
        /// </summary>
        private void UpdateDarkMatterGeneratorButton()
        {
            if (darkMatterGeneratorButton == null) return;

            if (darkMatterGeneratorPurchased)
            {
                darkMatterGeneratorButton.interactable = false;
                if (darkMatterGeneratorButtonText != null)
                {
                    darkMatterGeneratorButtonText.text = "DM Generator\n<size=60%>PURCHASED</size>";
                }
            }
            else
            {
                double currentMoney = CurrencyManager.Instance != null ? CurrencyManager.Instance.Money : 0;
                bool canAfford = currentMoney >= DARK_MATTER_GENERATOR_COST;
                darkMatterGeneratorButton.interactable = canAfford;

                if (darkMatterGeneratorButtonText != null)
                {
                    darkMatterGeneratorButtonText.text = $"DM Generator\n<size=60%>${FormatNumber(DARK_MATTER_GENERATOR_COST)}</size>";
                }
            }
        }

        /// <summary>
        /// Called when Dice Shop button is clicked.
        /// </summary>
        private void OnDiceShopClicked()
        {
            Debug.Log("[GameUI] Dice Shop button clicked!");

            if (diceShopPanel != null)
            {
                bool isActive = diceShopPanel.activeSelf;
                diceShopPanel.SetActive(!isActive);

                if (!isActive)
                {
                    // Populate dice shop when opening
                    PopulateDiceShop();
                }
            }
        }

        /// <summary>
        /// Populates the dice shop with available dice types.
        /// </summary>
        private void PopulateDiceShop()
        {
            if (diceShopContent == null || DiceManager.Instance == null) return;

            // Clear existing items
            foreach (Transform child in diceShopContent)
            {
                Destroy(child.gameObject);
            }

            // Create buttons for each unlocked dice type
            foreach (DiceType type in System.Enum.GetValues(typeof(DiceType)))
            {
                if (!DiceManager.Instance.IsDiceTypeUnlocked(type)) continue;

                CreateDiceShopButton(type);
            }
        }

        /// <summary>
        /// Creates a button for a dice type in the shop.
        /// </summary>
        private void CreateDiceShopButton(DiceType type)
        {
            DiceData data = DiceManager.Instance.GetDiceData(type);
            if (data == null) return;

            // Create button object
            GameObject buttonObj = new GameObject($"Buy_{type}");
            buttonObj.transform.SetParent(diceShopContent, false);

            RectTransform rt = buttonObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(180, 70);

            Image bgImage = buttonObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            Button button = buttonObj.AddComponent<Button>();

            // Create text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(5, 5);
            textRt.offsetMax = new Vector2(-5, -5);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.font = floatingTextFont ?? TMP_Settings.defaultFontAsset;
            tmp.fontSize = 18;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = data.tintColor;

            double price = DiceManager.Instance.GetCurrentPrice(type);
            int owned = DiceManager.Instance.GetOwnedCount(type);
            tmp.text = $"{data.displayName}\n<size=70%>${FormatNumber(price)} (Own: {owned})</size>";

            // Set button interactable based on affordability
            double currentMoney = CurrencyManager.Instance != null ? CurrencyManager.Instance.Money : 0;
            button.interactable = currentMoney >= price;

            // Add click handler
            DiceType capturedType = type;
            button.onClick.AddListener(() => OnBuySpecificDiceClicked(capturedType));
        }

        /// <summary>
        /// Called when a specific dice type buy button is clicked in the shop.
        /// </summary>
        private void OnBuySpecificDiceClicked(DiceType type)
        {
            Debug.Log($"[GameUI] Buy {type} Dice clicked!");

            if (DiceManager.Instance == null) return;

            bool success = DiceManager.Instance.TryBuyDice(type);

            if (success)
            {
                // Refresh the shop
                PopulateDiceShop();
                UpdateButtonTexts();
            }
            else
            {
                ShowNotEnoughFeedback("Not enough money!");
            }
        }

        /// <summary>
        /// Formats a number for display (K, M, B, etc.).
        /// </summary>
        public static string FormatNumber(double num)
        {
            if (num < 1000) return num.ToString("F0");
            if (num < 1000000) return (num / 1000).ToString("F1") + "K";
            if (num < 1000000000) return (num / 1000000).ToString("F2") + "M";
            if (num < 1000000000000) return (num / 1000000000).ToString("F2") + "B";
            return (num / 1000000000000).ToString("F2") + "T";
        }

        /// <summary>
        /// Shows a floating text popup at a world position using canvas UI.
        /// </summary>
        public void ShowFloatingText(Vector3 worldPosition, string text, Color color)
        {
            // Don't show floating text when a popup is open
            if (Core.PopupManager.Instance != null && Core.PopupManager.Instance.IsAnyPopupOpen) return;

            // Ensure we have camera reference
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            if (mainCamera == null) return;

            // Ensure canvasRect is set
            if (canvasRect == null)
            {
                canvasRect = GetComponent<RectTransform>();
            }
            if (canvasRect == null) return;

            // Create UI text object
            GameObject textObj = new GameObject("FloatingText");
            textObj.transform.SetParent(transform); // Parent directly to canvas

            RectTransform rt = textObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 80);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();

            // Assign font - ensure we have one
            if (floatingTextFont != null)
            {
                tmp.font = floatingTextFont;
            }
            else if (TMP_Settings.defaultFontAsset != null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
            }

            tmp.text = text;
            tmp.fontSize = 48;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.raycastTarget = false;

            // Apply black outline for readability
            ApplyTextOutline(tmp);

            // Convert world position to screen position
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition + Vector3.up * 0.5f);

            // Convert screen position to canvas local position
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera, out canvasPos);

            // Set position and initial scale
            rt.anchoredPosition = canvasPos;
            rt.localScale = Vector3.one; // Start visible

            // Animate - pop up, float up, fade out
            Sequence seq = DOTween.Sequence();

            // Initial punch scale
            seq.Append(rt.DOPunchScale(Vector3.one * 0.3f, 0.15f, 5, 0.5f));

            // Float up and fade
            seq.Append(rt.DOAnchorPosY(canvasPos.y + 100f, 0.8f).SetEase(Ease.OutQuad));
            seq.Join(tmp.DOFade(0f, 0.6f).SetEase(Ease.InQuad).SetDelay(0.3f));

            seq.OnComplete(() => Destroy(textObj));
        }

        #region Hyperburst Visual Effect

        private GameObject hyperburstOverlay;
        private TextMeshProUGUI hyperburstText;
        private Sequence hyperburstPulseSequence;

        /// <summary>
        /// Shows the x2 Hyperburst visual effect overlay.
        /// </summary>
        public void ShowHyperburstEffect(float duration)
        {
            // Create overlay if doesn't exist
            if (hyperburstOverlay == null)
            {
                CreateHyperburstOverlay();
            }

            hyperburstOverlay.SetActive(true);

            // Animate in with punch
            RectTransform rt = hyperburstOverlay.GetComponent<RectTransform>();
            rt.localScale = Vector3.zero;
            rt.DOKill();
            rt.DOScale(1f, 0.3f).SetEase(Ease.OutBack);

            // Start pulsing animation
            StartHyperburstPulse();

            // Screen flash effect
            CreateScreenFlash(new Color(1f, 0.8f, 0.2f, 0.3f), 0.2f);

            // Show floating text
            ShowFloatingText(Vector3.zero, "x2 ALL EARNINGS!", new Color(1f, 0.85f, 0.2f));

            Debug.Log("[GameUI] Hyperburst effect shown!");
        }

        /// <summary>
        /// Hides the Hyperburst visual effect.
        /// </summary>
        public void HideHyperburstEffect()
        {
            if (hyperburstOverlay == null) return;

            // Stop pulsing
            hyperburstPulseSequence?.Kill();

            // Animate out
            RectTransform rt = hyperburstOverlay.GetComponent<RectTransform>();
            rt.DOKill();
            rt.DOScale(0f, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
            {
                hyperburstOverlay.SetActive(false);
            });

            // Show end message
            ShowFloatingText(Vector3.zero, "Hyperburst ended!", new Color(0.7f, 0.7f, 0.7f));

            Debug.Log("[GameUI] Hyperburst effect hidden!");
        }

        /// <summary>
        /// Creates the Hyperburst overlay UI element.
        /// </summary>
        private void CreateHyperburstOverlay()
        {
            hyperburstOverlay = new GameObject("HyperburstOverlay");
            hyperburstOverlay.transform.SetParent(transform, false);

            RectTransform rt = hyperburstOverlay.AddComponent<RectTransform>();
            // Position in top-center of screen
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -180);
            rt.sizeDelta = new Vector2(300, 100);

            // Background with glow effect
            Image bg = hyperburstOverlay.AddComponent<Image>();
            bg.color = new Color(1f, 0.7f, 0.1f, 0.9f);
            bg.raycastTarget = false;

            // Add outline/border effect
            Outline outline = hyperburstOverlay.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 0.5f, 1f);
            outline.effectDistance = new Vector2(3, 3);

            // x2 Text
            GameObject textObj = new GameObject("x2Text");
            textObj.transform.SetParent(hyperburstOverlay.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            hyperburstText = textObj.AddComponent<TextMeshProUGUI>();
            hyperburstText.text = "x2";
            hyperburstText.fontSize = 72;
            hyperburstText.fontStyle = FontStyles.Bold;
            hyperburstText.alignment = TextAlignmentOptions.Center;
            hyperburstText.color = Color.white;
            hyperburstText.outlineWidth = 0.2f;
            hyperburstText.outlineColor = new Color(0.3f, 0.1f, 0f, 1f);

            if (floatingTextFont != null)
            {
                hyperburstText.font = floatingTextFont;
            }

            hyperburstOverlay.SetActive(false);
        }

        /// <summary>
        /// Starts the pulsing animation for the Hyperburst overlay.
        /// </summary>
        private void StartHyperburstPulse()
        {
            hyperburstPulseSequence?.Kill();

            RectTransform rt = hyperburstOverlay.GetComponent<RectTransform>();
            hyperburstPulseSequence = DOTween.Sequence();
            hyperburstPulseSequence.Append(rt.DOScale(1.1f, 0.3f).SetEase(Ease.InOutSine));
            hyperburstPulseSequence.Append(rt.DOScale(1f, 0.3f).SetEase(Ease.InOutSine));
            hyperburstPulseSequence.SetLoops(-1); // Infinite loop

            // Also pulse the text color
            if (hyperburstText != null)
            {
                hyperburstText.DOColor(new Color(1f, 1f, 0.7f), 0.3f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }

        /// <summary>
        /// Creates a brief screen flash effect.
        /// </summary>
        private void CreateScreenFlash(Color color, float duration)
        {
            GameObject flashObj = new GameObject("ScreenFlash");
            flashObj.transform.SetParent(transform, false);

            RectTransform rt = flashObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = flashObj.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;

            // Fade out
            img.DOFade(0f, duration).OnComplete(() => Destroy(flashObj));
        }

        #endregion
    }
}
