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
        /// Updates button texts with current prices and disabled states.
        /// </summary>
        private void UpdateButtonTexts()
        {
            double currentMoney = CurrencyManager.Instance != null ? CurrencyManager.Instance.Money : 0;

            // Buy dice button
            if (buyDiceButton != null && DiceManager.Instance != null)
            {
                double price = DiceManager.Instance.GetCurrentPrice(DiceType.Basic);
                bool canAfford = currentMoney >= price;
                buyDiceButton.interactable = canAfford;

                if (buyDiceButtonText != null)
                {
                    buyDiceButtonText.text = $"Buy Dice\n<size=60%>${FormatNumber(price)}</size>";
                }
            }

            // Upgrade dice button - shows current level and bonus
            if (upgradeDiceButton != null)
            {
                bool canAfford = currentMoney >= diceValueUpgradeCost;
                upgradeDiceButton.interactable = canAfford;

                if (upgradeDiceButtonText != null)
                {
                    int currentLevel = GameStats.Instance != null ? GameStats.Instance.DiceValueUpgradeLevel : 0;
                    string bonusText = currentLevel > 0 ? $" +{currentLevel}" : "";
                    upgradeDiceButtonText.text = $"Upgrade{bonusText}\n<size=60%>${FormatNumber(diceValueUpgradeCost)}</size>";
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

            bool success = DiceManager.Instance.TryBuyDice(DiceType.Basic);
            Debug.Log($"[GameUI] Buy dice result: {success}");

            if (success)
            {
                // Animate button
                buyDiceButton.transform.DOKill();
                buyDiceButton.transform.localScale = Vector3.one;
                buyDiceButton.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0.5f);
                UpdateButtonTexts();
            }
            else
            {
                // Show feedback and shake
                ShowNotEnoughFeedback("Not enough money!");
                buyDiceButton.transform.DOKill();
                buyDiceButton.transform.DOShakePosition(0.2f, 3f, 15);
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
                    // Animate button
                    ascendButton.transform.DOKill();
                    ascendButton.transform.localScale = Vector3.one;
                    ascendButton.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f);

                    // Show floating text - DM is now unlocked!
                    ShowFloatingText(Vector3.zero, "ASCENDED!\nDark Matter Unlocked!", new Color(0.8f, 0.5f, 1f));

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
                return;
            }

            bool hasRollBurst = SkillTreeManager.Instance.IsActiveSkillUnlocked(ActiveSkillType.RollBurst);
            bool hasHyperburst = SkillTreeManager.Instance.IsActiveSkillUnlocked(ActiveSkillType.Hyperburst);

            if (!hasRollBurst && !hasHyperburst)
            {
                activeSkillButton.gameObject.SetActive(false);
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
                    }
                    else
                    {
                        activeSkillButtonText.text = $"{skillName}\n<size=60%>{cooldown:F1}s</size>";
                    }
                }
            }
        }

        /// <summary>
        /// Updates the ascend button text and disabled state.
        /// </summary>
        private void UpdateAscendButton()
        {
            if (PrestigeManager.Instance == null) return;

            bool hasAscended = PrestigeManager.Instance.HasAscended;
            bool canAscend = PrestigeManager.Instance.CanPrestige();
            double required = PrestigeManager.Instance.GetMoneyRequiredForPrestige();

            // Update interactable state
            if (ascendButton != null)
            {
                ascendButton.interactable = canAscend && !hasAscended;
            }

            if (ascendButtonText != null)
            {
                if (hasAscended)
                {
                    ascendButtonText.text = "Ascended\n<size=60%>DM Active!</size>";
                    ascendButton.interactable = false;
                }
                else if (canAscend)
                {
                    ascendButtonText.text = $"Ascend\n<size=60%>${FormatNumber(required)}</size>";
                }
                else
                {
                    ascendButtonText.text = $"Ascend\n<size=60%>${FormatNumber(required)}</size>";
                }
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

            if (CurrencyManager.Instance.SpendMoney(diceValueUpgradeCost))
            {
                // Increase the dice value upgrade level
                GameStats.Instance.DiceValueUpgradeLevel++;
                Debug.Log($"[GameUI] Upgrade successful! New level: {GameStats.Instance.DiceValueUpgradeLevel}");

                // Increase cost for next upgrade
                diceValueUpgradeCost *= diceValueUpgradeCostMultiplier;

                // Animate button
                upgradeDiceButton.transform.DOKill();
                upgradeDiceButton.transform.localScale = Vector3.one;
                upgradeDiceButton.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0.5f);
                UpdateButtonTexts();
            }
            else
            {
                Debug.Log("[GameUI] Cannot afford upgrade - shaking button");
                // Show feedback and shake
                ShowNotEnoughFeedback("Not enough money!");
                upgradeDiceButton.transform.DOKill();
                upgradeDiceButton.transform.DOShakePosition(0.2f, 3f, 15);
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
    }
}
