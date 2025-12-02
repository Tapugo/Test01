using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.Dice;

namespace Incredicer.UI
{
    /// <summary>
    /// Dice Shop UI - allows players to buy different types of dice.
    /// Styled with Layer Lab/GUI-CasualFantasy assets.
    /// </summary>
    public class DiceShopUI : MonoBehaviour
    {
        public static DiceShopUI Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private Button openButton;
        [SerializeField] private TextMeshProUGUI openButtonText;
        [SerializeField] private Button closeButton;

        [Header("Content")]
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private TextMeshProUGUI moneyDisplayText;

        [Header("GUI Assets")]
        [SerializeField] private GUISpriteAssets guiAssets;

        [Header("Visual Settings")]
        [SerializeField] private Color affordableColor = new Color(0.3f, 0.8f, 0.4f);
        [SerializeField] private Color unaffordableColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.35f);

        private Dictionary<DiceType, DiceShopItem> shopItems = new Dictionary<DiceType, DiceShopItem>();
        private bool isOpen;
        private bool isInitialized;

        public bool IsOpen => isOpen;

        private class DiceShopItem
        {
            public GameObject gameObject;
            public Button button;
            public Image background;
            public TextMeshProUGUI nameText;
            public TextMeshProUGUI priceText;
            public TextMeshProUGUI ownedText;
            public Image dicePreview;
            public Button buyButton;
            public TextMeshProUGUI buyButtonText;
            public Image buyButtonBg;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Load GUI assets if not assigned
            if (guiAssets == null)
                guiAssets = GUISpriteAssets.Instance;

            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }
        }

        private void Start()
        {
            if (openButton != null)
            {
                openButton.onClick.AddListener(Toggle);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
                CurrencyManager.Instance.OnMoneyChanged += (_) => UpdateAllItems();
            }

            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.OnDiceSpawned += (_) => UpdateAllItems();
                DiceManager.Instance.OnDiceTypeUnlocked += (_) => RebuildShop();
            }
        }

        private void OnDestroy()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
            }
        }

        private void BuildShop()
        {
            if (isInitialized) return;
            isInitialized = true;

            if (shopPanel == null) return;

            // Create UI structure if needed
            CreateUIStructure();

            // Clear existing items
            shopItems.Clear();
            if (contentContainer != null)
            {
                foreach (Transform child in contentContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // Create items for each dice type
            foreach (DiceType type in System.Enum.GetValues(typeof(DiceType)))
            {
                CreateShopItem(type);
            }

            UpdateAllItems();
            Debug.Log($"[DiceShopUI] Built shop with {shopItems.Count} items");
        }

        private void CreateUIStructure()
        {
            // Get or create panel rect
            if (panelRect == null)
            {
                panelRect = shopPanel.GetComponent<RectTransform>();
                if (panelRect == null)
                    panelRect = shopPanel.AddComponent<RectTransform>();
            }

            // Ensure panel fills screen with margins
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = new Vector2(20, 20);
            panelRect.offsetMax = new Vector2(-20, -20);

            // Add background if missing
            Image panelBg = shopPanel.GetComponent<Image>();
            if (panelBg == null)
            {
                panelBg = shopPanel.AddComponent<Image>();
                panelBg.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);
            }

            // Add canvas group if missing
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = shopPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                    panelCanvasGroup = shopPanel.AddComponent<CanvasGroup>();
            }

            // Create header if money display doesn't exist
            if (moneyDisplayText == null)
            {
                CreateHeader();
            }

            // Create scroll area if it doesn't exist
            if (scrollRect == null)
            {
                CreateScrollArea();
            }
        }

        private void CreateHeader()
        {
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(shopPanel.transform, false);
            RectTransform headerRt = headerObj.AddComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 0.9f);
            headerRt.anchorMax = new Vector2(1, 1);
            headerRt.offsetMin = new Vector2(10, 5);
            headerRt.offsetMax = new Vector2(-10, -5);

            Image headerBg = headerObj.AddComponent<Image>();
            // Use GUI sprite if available
            if (guiAssets != null && guiAssets.horizontalFrame != null)
            {
                headerBg.sprite = guiAssets.horizontalFrame;
                headerBg.type = Image.Type.Sliced;
                headerBg.color = new Color(0.2f, 0.25f, 0.2f);
            }
            else
            {
                headerBg.color = new Color(0.12f, 0.12f, 0.18f, 0.98f);
            }

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerObj.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 0);
            titleRt.anchorMax = new Vector2(0.35f, 1);
            titleRt.offsetMin = new Vector2(15, 0);
            titleRt.offsetMax = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "DICE SHOP";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = Color.white;

            // Money display
            GameObject moneyObj = new GameObject("Money");
            moneyObj.transform.SetParent(headerObj.transform, false);
            RectTransform moneyRt = moneyObj.AddComponent<RectTransform>();
            moneyRt.anchorMin = new Vector2(0.35f, 0);
            moneyRt.anchorMax = new Vector2(0.8f, 1);
            moneyRt.offsetMin = Vector2.zero;
            moneyRt.offsetMax = Vector2.zero;

            moneyDisplayText = moneyObj.AddComponent<TextMeshProUGUI>();
            moneyDisplayText.text = "$0";
            moneyDisplayText.fontSize = 22;
            moneyDisplayText.alignment = TextAlignmentOptions.Center;
            moneyDisplayText.color = new Color(0.4f, 0.95f, 0.5f);

            // Close button
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(headerObj.transform, false);
            RectTransform closeRt = closeObj.AddComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(0.85f, 0.1f);
            closeRt.anchorMax = new Vector2(0.98f, 0.9f);
            closeRt.offsetMin = Vector2.zero;
            closeRt.offsetMax = Vector2.zero;

            Image closeBg = closeObj.AddComponent<Image>();
            // Use GUI button sprite if available
            if (guiAssets != null && guiAssets.buttonRed != null)
            {
                closeBg.sprite = guiAssets.buttonRed;
                closeBg.type = Image.Type.Sliced;
                closeBg.color = Color.white;
            }
            else
            {
                closeBg.color = new Color(0.9f, 0.3f, 0.3f);
            }

            closeButton = closeObj.AddComponent<Button>();
            closeButton.onClick.AddListener(Hide);

            GameObject closeTextObj = new GameObject("X");
            closeTextObj.transform.SetParent(closeObj.transform, false);
            RectTransform closeTextRt = closeTextObj.AddComponent<RectTransform>();
            closeTextRt.anchorMin = Vector2.zero;
            closeTextRt.anchorMax = Vector2.one;
            closeTextRt.offsetMin = Vector2.zero;
            closeTextRt.offsetMax = Vector2.zero;

            TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.fontSize = 28;
            closeText.fontStyle = FontStyles.Bold;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.color = Color.white;
        }

        private void CreateScrollArea()
        {
            // Create scroll area container
            GameObject scrollObj = new GameObject("ScrollArea");
            scrollObj.transform.SetParent(shopPanel.transform, false);
            RectTransform scrollRt = scrollObj.AddComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0, 0);
            scrollRt.anchorMax = new Vector2(1, 0.9f);
            scrollRt.offsetMin = new Vector2(10, 10);
            scrollRt.offsetMax = new Vector2(-10, -5);

            // ScrollRect component
            scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.scrollSensitivity = 20f;

            // Add RectMask2D directly to scroll area for clipping
            scrollObj.AddComponent<RectMask2D>();

            // Background image
            Image scrollBg = scrollObj.AddComponent<Image>();
            scrollBg.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);

            // Use the scroll object itself as viewport
            scrollRect.viewport = scrollRt;

            // Content - child of scroll area directly
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollObj.transform, false);
            contentContainer = contentObj.AddComponent<RectTransform>();

            // Anchor to top-left, stretch horizontally
            contentContainer.anchorMin = new Vector2(0, 1);
            contentContainer.anchorMax = new Vector2(1, 1);
            contentContainer.pivot = new Vector2(0.5f, 1);
            contentContainer.anchoredPosition = Vector2.zero;
            contentContainer.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentContainer;
        }

        private void CreateShopItem(DiceType type)
        {
            if (contentContainer == null)
            {
                Debug.LogError($"[DiceShopUI] contentContainer is null when creating item for {type}");
                return;
            }

            DiceData data = DiceManager.Instance?.GetDiceData(type);
            if (data == null)
            {
                Debug.LogWarning($"[DiceShopUI] No DiceData for {type}");
                return;
            }

            Debug.Log($"[DiceShopUI] Creating shop item for {type}: {data.displayName}");

            // Create item container
            GameObject itemObj = new GameObject($"Item_{type}");
            itemObj.transform.SetParent(contentContainer, false);

            RectTransform rt = itemObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 120);

            LayoutElement le = itemObj.AddComponent<LayoutElement>();
            le.minHeight = 120;
            le.preferredHeight = 120;
            le.flexibleWidth = 1;

            // Use GUI list frame sprite if available
            Image bg = itemObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.listFrame != null)
            {
                bg.sprite = guiAssets.listFrame;
                bg.type = Image.Type.Sliced;
                bg.color = new Color(0.2f, 0.22f, 0.25f);
            }
            else
            {
                bg.color = new Color(0.2f, 0.2f, 0.25f, 0.98f);
            }

            // Item acts as button
            Button btn = itemObj.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            DiceType capturedType = type;

            // Dice preview (left side)
            GameObject previewObj = new GameObject("Preview");
            previewObj.transform.SetParent(itemObj.transform, false);
            RectTransform previewRt = previewObj.AddComponent<RectTransform>();
            previewRt.anchorMin = new Vector2(0, 0);
            previewRt.anchorMax = new Vector2(0, 1);
            previewRt.pivot = new Vector2(0, 0.5f);
            previewRt.anchoredPosition = new Vector2(10, 0);
            previewRt.sizeDelta = new Vector2(50, 50);

            Image previewImg = previewObj.AddComponent<Image>();
            previewImg.color = data.tintColor;
            previewImg.raycastTarget = false;

            // Name text
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(itemObj.transform, false);
            RectTransform nameRt = nameObj.AddComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0.5f);
            nameRt.anchorMax = new Vector2(0.5f, 1);
            nameRt.offsetMin = new Vector2(70, 0);
            nameRt.offsetMax = new Vector2(0, -5);

            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.text = data.displayName;
            nameTmp.fontSize = 20;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.alignment = TextAlignmentOptions.Left;
            nameTmp.color = data.tintColor;
            nameTmp.raycastTarget = false;

            // Owned count
            GameObject ownedObj = new GameObject("Owned");
            ownedObj.transform.SetParent(itemObj.transform, false);
            RectTransform ownedRt = ownedObj.AddComponent<RectTransform>();
            ownedRt.anchorMin = new Vector2(0, 0);
            ownedRt.anchorMax = new Vector2(0.5f, 0.5f);
            ownedRt.offsetMin = new Vector2(70, 5);
            ownedRt.offsetMax = new Vector2(0, 0);

            TextMeshProUGUI ownedTmp = ownedObj.AddComponent<TextMeshProUGUI>();
            ownedTmp.text = "Owned: 0";
            ownedTmp.fontSize = 14;
            ownedTmp.alignment = TextAlignmentOptions.Left;
            ownedTmp.color = new Color(0.7f, 0.7f, 0.7f);
            ownedTmp.raycastTarget = false;

            // Price text (center)
            GameObject priceObj = new GameObject("Price");
            priceObj.transform.SetParent(itemObj.transform, false);
            RectTransform priceRt = priceObj.AddComponent<RectTransform>();
            priceRt.anchorMin = new Vector2(0.45f, 0.5f);
            priceRt.anchorMax = new Vector2(0.7f, 1);
            priceRt.offsetMin = new Vector2(0, 5);
            priceRt.offsetMax = new Vector2(0, -5);

            TextMeshProUGUI priceTmp = priceObj.AddComponent<TextMeshProUGUI>();
            priceTmp.text = "$0";
            priceTmp.fontSize = 20;
            priceTmp.fontStyle = FontStyles.Bold;
            priceTmp.alignment = TextAlignmentOptions.Center;
            priceTmp.color = affordableColor;
            priceTmp.raycastTarget = false;

            // BUY BUTTON (right side - prominent)
            GameObject buyBtnObj = new GameObject("BuyButton");
            buyBtnObj.transform.SetParent(itemObj.transform, false);
            RectTransform buyBtnRt = buyBtnObj.AddComponent<RectTransform>();
            buyBtnRt.anchorMin = new Vector2(0.72f, 0.15f);
            buyBtnRt.anchorMax = new Vector2(0.98f, 0.85f);
            buyBtnRt.offsetMin = Vector2.zero;
            buyBtnRt.offsetMax = Vector2.zero;

            Image buyBtnBg = buyBtnObj.AddComponent<Image>();
            // Use GUI button sprite if available
            if (guiAssets != null && guiAssets.buttonGreen != null)
            {
                buyBtnBg.sprite = guiAssets.buttonGreen;
                buyBtnBg.type = Image.Type.Sliced;
                buyBtnBg.color = Color.white;
            }
            else
            {
                buyBtnBg.color = affordableColor;
            }

            Button buyBtn = buyBtnObj.AddComponent<Button>();
            ColorBlock buyColors = buyBtn.colors;
            buyColors.normalColor = Color.white;
            buyColors.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
            buyColors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            buyColors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
            buyBtn.colors = buyColors;
            buyBtn.onClick.AddListener(() => OnBuyClicked(capturedType));

            // Buy button text
            GameObject buyTextObj = new GameObject("Text");
            buyTextObj.transform.SetParent(buyBtnObj.transform, false);
            RectTransform buyTextRt = buyTextObj.AddComponent<RectTransform>();
            buyTextRt.anchorMin = Vector2.zero;
            buyTextRt.anchorMax = Vector2.one;
            buyTextRt.offsetMin = Vector2.zero;
            buyTextRt.offsetMax = Vector2.zero;

            TextMeshProUGUI buyTextTmp = buyTextObj.AddComponent<TextMeshProUGUI>();
            buyTextTmp.text = "BUY";
            buyTextTmp.fontSize = 24;
            buyTextTmp.fontStyle = FontStyles.Bold;
            buyTextTmp.alignment = TextAlignmentOptions.Center;
            buyTextTmp.color = Color.white;
            buyTextTmp.raycastTarget = false;

            shopItems[type] = new DiceShopItem
            {
                gameObject = itemObj,
                button = btn,
                background = bg,
                nameText = nameTmp,
                priceText = priceTmp,
                ownedText = ownedTmp,
                dicePreview = previewImg,
                buyButton = buyBtn,
                buyButtonText = buyTextTmp,
                buyButtonBg = buyBtnBg
            };
        }

        public void Show()
        {
            if (isOpen) return;
            isOpen = true;

            // Force rebuild each time to ensure content is populated
            isInitialized = false;
            BuildShop();

            if (shopPanel != null)
            {
                shopPanel.SetActive(true);

                if (panelCanvasGroup != null)
                {
                    panelCanvasGroup.alpha = 0f;
                    panelCanvasGroup.DOFade(1f, 0.2f);
                    panelCanvasGroup.blocksRaycasts = true;
                    panelCanvasGroup.interactable = true;
                }
            }

            UpdateAllItems();
            UpdateMoneyDisplay(CurrencyManager.Instance?.Money ?? 0);

            // Block dice input
            SetDiceInputBlocked(true);

            Debug.Log("[DiceShopUI] Opened");
        }

        public void Hide()
        {
            if (!isOpen) return;
            isOpen = false;

            // Unblock dice input
            SetDiceInputBlocked(false);

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.DOFade(0f, 0.15f).OnComplete(() =>
                {
                    if (shopPanel != null)
                        shopPanel.SetActive(false);
                });
                panelCanvasGroup.blocksRaycasts = false;
                panelCanvasGroup.interactable = false;
            }
            else if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }

            Debug.Log("[DiceShopUI] Closed");
        }

        public void Toggle()
        {
            if (isOpen) Hide();
            else Show();
        }

        private void SetDiceInputBlocked(bool blocked)
        {
            if (DiceRollerController.Instance != null)
            {
                DiceRollerController.Instance.enabled = !blocked;
            }
        }

        private void UpdateMoneyDisplay(double amount)
        {
            if (moneyDisplayText != null)
            {
                moneyDisplayText.text = $"${GameUI.FormatNumber(amount)}";
            }
        }

        private void UpdateAllItems()
        {
            if (DiceManager.Instance == null || CurrencyManager.Instance == null) return;

            double currentMoney = CurrencyManager.Instance.Money;

            foreach (var kvp in shopItems)
            {
                UpdateItem(kvp.Key, kvp.Value, currentMoney);
            }
        }

        private void UpdateItem(DiceType type, DiceShopItem item, double currentMoney)
        {
            if (item == null || DiceManager.Instance == null) return;

            bool isUnlocked = DiceManager.Instance.IsDiceTypeUnlocked(type);
            double price = DiceManager.Instance.GetCurrentPrice(type);
            int owned = DiceManager.Instance.GetOwnedCount(type);
            bool canAfford = currentMoney >= price;

            // Update texts
            if (item.priceText != null)
            {
                item.priceText.text = isUnlocked ? $"${GameUI.FormatNumber(price)}" : "LOCKED";
            }

            if (item.ownedText != null)
            {
                item.ownedText.text = $"Owned: {owned}";
            }

            // Update buy button
            if (item.buyButton != null)
            {
                item.buyButton.interactable = isUnlocked && canAfford;
            }

            if (item.buyButtonText != null)
            {
                if (!isUnlocked)
                    item.buyButtonText.text = "LOCKED";
                else if (canAfford)
                    item.buyButtonText.text = "BUY";
                else
                    item.buyButtonText.text = "NEED $";
            }

            if (item.buyButtonBg != null)
            {
                // Update button sprite and color based on state
                if (guiAssets != null)
                {
                    if (!isUnlocked && guiAssets.buttonGray != null)
                    {
                        item.buyButtonBg.sprite = guiAssets.buttonGray;
                        item.buyButtonBg.color = new Color(0.6f, 0.6f, 0.6f);
                    }
                    else if (canAfford && guiAssets.buttonGreen != null)
                    {
                        item.buyButtonBg.sprite = guiAssets.buttonGreen;
                        item.buyButtonBg.color = Color.white;
                    }
                    else if (guiAssets.buttonYellow != null)
                    {
                        item.buyButtonBg.sprite = guiAssets.buttonYellow;
                        item.buyButtonBg.color = new Color(0.8f, 0.8f, 0.8f);
                    }
                    else
                    {
                        item.buyButtonBg.color = !isUnlocked ? lockedColor : (canAfford ? affordableColor : unaffordableColor);
                    }
                }
                else
                {
                    if (!isUnlocked)
                        item.buyButtonBg.color = lockedColor;
                    else if (canAfford)
                        item.buyButtonBg.color = affordableColor;
                    else
                        item.buyButtonBg.color = unaffordableColor;
                }
            }

            // Update background and price colors
            if (!isUnlocked)
            {
                if (item.background != null)
                    item.background.color = lockedColor;
                if (item.priceText != null)
                    item.priceText.color = new Color(0.5f, 0.5f, 0.5f);
            }
            else if (canAfford)
            {
                if (item.background != null)
                    item.background.color = new Color(0.15f, 0.2f, 0.15f, 0.95f);
                if (item.priceText != null)
                    item.priceText.color = affordableColor;
            }
            else
            {
                if (item.background != null)
                    item.background.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
                if (item.priceText != null)
                    item.priceText.color = unaffordableColor;
            }
        }

        private void RebuildShop()
        {
            isInitialized = false;
            if (isOpen)
            {
                BuildShop();
            }
        }

        private void OnBuyClicked(DiceType type)
        {
            if (DiceManager.Instance == null) return;

            bool success = DiceManager.Instance.TryBuyDice(type);

            if (success)
            {
                UpdateAllItems();

                // Visual feedback
                if (shopItems.TryGetValue(type, out var item) && item.buyButton != null)
                {
                    item.buyButton.transform.DOKill();
                    item.buyButton.transform.localScale = Vector3.one;
                    item.buyButton.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0.5f);
                }

                if (GameUI.Instance != null)
                {
                    DiceData data = DiceManager.Instance.GetDiceData(type);
                    string diceName = data != null ? data.displayName : type.ToString();
                    GameUI.Instance.ShowFloatingText(Vector3.zero, $"Bought {diceName}!", new Color(0.3f, 1f, 0.4f));
                }
            }
            else
            {
                // Shake feedback
                if (shopItems.TryGetValue(type, out var item) && item.buyButton != null)
                {
                    item.buyButton.transform.DOKill();
                    item.buyButton.transform.DOShakePosition(0.2f, 3f, 15);
                }
            }
        }
    }
}
