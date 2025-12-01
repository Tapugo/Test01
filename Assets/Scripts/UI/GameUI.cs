using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.Dice;

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
        [SerializeField] private TextMeshProUGUI darkMatterText;
        [SerializeField] private GameObject darkMatterPanel;

        [Header("Shop Buttons")]
        [SerializeField] private Button buyDiceButton;
        [SerializeField] private TextMeshProUGUI buyDiceButtonText;
        [SerializeField] private Button upgradeDiceButton;
        [SerializeField] private TextMeshProUGUI upgradeDiceButtonText;

        [Header("Skill Tree Button")]
        [SerializeField] private Button skillTreeButton;

        [Header("Ascend Button")]
        [SerializeField] private Button ascendButton;
        [SerializeField] private TextMeshProUGUI ascendButtonText;

        [Header("Floating Text")]
        [SerializeField] private Transform floatingTextContainer;
        [SerializeField] private TMP_FontAsset floatingTextFont;

        [Header("Settings")]
        [SerializeField] private float currencyPunchScale = 1.15f;
        [SerializeField] private float currencyPunchDuration = 0.15f;

        // Upgrade tracking
        private double diceValueUpgradeCost = 25;
        private double diceValueUpgradeCostMultiplier = 1.8;

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

            // Initial UI update (delayed to ensure managers are ready)
            Invoke(nameof(UpdateAllUI), 0.15f);
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

        private void OnDestroy()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
                CurrencyManager.Instance.OnDarkMatterChanged -= UpdateDarkMatterDisplay;
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
                darkMatterPanel.SetActive(DiceManager.Instance.DarkMatterUnlocked);
            }
        }

        /// <summary>
        /// Updates button texts with current prices.
        /// </summary>
        private void UpdateButtonTexts()
        {
            // Buy dice button
            if (buyDiceButtonText != null && DiceManager.Instance != null)
            {
                double price = DiceManager.Instance.GetCurrentPrice(DiceType.Basic);
                buyDiceButtonText.text = $"Buy Extra Dice\n<size=70%>${FormatNumber(price)}</size>";
            }

            // Upgrade dice button - shows current level and bonus
            if (upgradeDiceButtonText != null)
            {
                int currentLevel = GameStats.Instance != null ? GameStats.Instance.DiceValueUpgradeLevel : 0;
                string bonusText = currentLevel > 0 ? $"+{currentLevel}" : "";
                upgradeDiceButtonText.text = $"Upgrade Dice {bonusText}\n<size=70%>${FormatNumber(diceValueUpgradeCost)}</size>";
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
                // Shake button to indicate can't afford
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

            if (PrestigeManager.Instance.CanPrestige())
            {
                bool success = PrestigeManager.Instance.DoPrestige();
                if (success)
                {
                    // Animate button
                    ascendButton.transform.DOKill();
                    ascendButton.transform.localScale = Vector3.one;
                    ascendButton.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f);

                    // Show floating text
                    double earnedDM = PrestigeManager.Instance.TotalDarkMatterEarned;
                    ShowFloatingText(Vector3.zero, $"ASCENDED!\n+{FormatNumber(earnedDM)} DM", new Color(0.8f, 0.5f, 1f));

                    UpdateAllUI();
                }
            }
            else
            {
                // Shake to indicate can't ascend yet
                ascendButton.transform.DOKill();
                ascendButton.transform.DOShakePosition(0.2f, 3f, 15);

                Debug.Log($"[GameUI] Cannot ascend yet. Need: ${FormatNumber(PrestigeManager.Instance.GetMoneyRequiredForPrestige())}");
            }
        }

        /// <summary>
        /// Updates the ascend button text.
        /// </summary>
        private void UpdateAscendButton()
        {
            if (ascendButtonText == null || PrestigeManager.Instance == null) return;

            bool canAscend = PrestigeManager.Instance.CanPrestige();
            double potentialDM = PrestigeManager.Instance.CalculatePotentialDarkMatter();

            if (canAscend && potentialDM > 0)
            {
                ascendButtonText.text = $"Ascend\n<size=70%>+{FormatNumber(potentialDM)} DM</size>";
            }
            else
            {
                double required = PrestigeManager.Instance.GetMoneyRequiredForPrestige();
                ascendButtonText.text = $"Ascend\n<size=60%>${FormatNumber(required)}</size>";
            }
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
                // Shake button to indicate can't afford
                upgradeDiceButton.transform.DOKill();
                upgradeDiceButton.transform.DOShakePosition(0.2f, 3f, 15);
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
