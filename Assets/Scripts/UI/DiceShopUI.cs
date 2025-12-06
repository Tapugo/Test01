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
        [SerializeField] private GameObject panelPrefab;  // Assign prefab in inspector
        private GameObject shopPanel;
        private CanvasGroup panelCanvasGroup;
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
            public Image lockIcon;
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

            // Instantiate from prefab if needed
            if (shopPanel == null && panelPrefab != null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas == null) canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    shopPanel = Instantiate(panelPrefab, canvas.transform);
                    shopPanel.name = "DiceShopPanel";
                }
            }

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
            // Fill FULL screen (no margins)
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

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

            // Clean up existing dynamic children to avoid duplicates
            Transform existingHeader = shopPanel.transform.Find("Header");
            if (existingHeader != null)
            {
                Destroy(existingHeader.gameObject);
            }
            Transform existingScrollArea = shopPanel.transform.Find("ScrollArea");
            if (existingScrollArea != null)
            {
                Destroy(existingScrollArea.gameObject);
            }

            // Reset references so they get recreated
            scrollRect = null;
            contentContainer = null;
            moneyDisplayText = null;
            closeButton = null;

            // Create header (always recreated after clearing)
            CreateHeader();

            // Create scroll area (always recreated after clearing)
            CreateScrollArea();
        }

        private void CreateHeader()
        {
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(shopPanel.transform, false);
            RectTransform headerRt = headerObj.AddComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 0.88f); // Taller header
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
            titleText.fontSize = UIDesignSystem.FontSizeHero;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = UIDesignSystem.MoneyGreen;

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
            moneyDisplayText.fontSize = UIDesignSystem.FontSizeTitle;
            moneyDisplayText.fontStyle = FontStyles.Bold;
            moneyDisplayText.alignment = TextAlignmentOptions.Center;
            moneyDisplayText.color = UIDesignSystem.MoneyGreen;

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
            closeText.fontSize = UIDesignSystem.FontSizeSubtitle;
            closeText.fontStyle = FontStyles.Bold;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.color = UIDesignSystem.TextPrimary;
        }

        private void CreateScrollArea()
        {
            // Create scroll area container
            GameObject scrollObj = new GameObject("ScrollArea");
            scrollObj.transform.SetParent(shopPanel.transform, false);
            RectTransform scrollRt = scrollObj.AddComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0, 0);
            scrollRt.anchorMax = new Vector2(1, 0.88f); // Match header
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

            // Create item container - MUCH BIGGER for mobile (3x)
            GameObject itemObj = new GameObject($"Item_{type}");
            itemObj.transform.SetParent(contentContainer, false);

            RectTransform rt = itemObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 280); // 3x bigger for mobile

            LayoutElement le = itemObj.AddComponent<LayoutElement>();
            le.minHeight = 280;
            le.preferredHeight = 280;
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

            // === LEFT SIDE: Dice Preview ===
            GameObject previewObj = new GameObject("Preview");
            previewObj.transform.SetParent(itemObj.transform, false);
            RectTransform previewRt = previewObj.AddComponent<RectTransform>();
            previewRt.anchorMin = new Vector2(0, 0.1f);
            previewRt.anchorMax = new Vector2(0, 0.9f);
            previewRt.pivot = new Vector2(0, 0.5f);
            previewRt.anchoredPosition = new Vector2(15, 0);
            previewRt.sizeDelta = new Vector2(180, 0);

            Image previewImg = previewObj.AddComponent<Image>();
            previewImg.sprite = CreateDiceFaceSprite(6, data.tintColor);
            previewImg.color = Color.white;
            previewImg.raycastTarget = false;
            previewImg.preserveAspect = true;

            // === CENTER: Dice Info ===
            // Name with multiplier
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(itemObj.transform, false);
            RectTransform nameRt = nameObj.AddComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0.55f);
            nameRt.anchorMax = new Vector2(0.52f, 1);
            nameRt.offsetMin = new Vector2(210, 0);
            nameRt.offsetMax = new Vector2(-5, -15);

            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            string multiplierText = data.basePayout >= 1000 ? $"{data.basePayout/1000}K" : data.basePayout.ToString();
            nameTmp.text = $"{data.displayName}";
            nameTmp.fontSize = UIDesignSystem.FontSizeSubtitle;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.alignment = TextAlignmentOptions.Left;
            nameTmp.color = data.tintColor;
            nameTmp.raycastTarget = false;
            nameTmp.enableAutoSizing = true;
            nameTmp.fontSizeMin = UIDesignSystem.FontSizeBody;
            nameTmp.fontSizeMax = UIDesignSystem.FontSizeSubtitle;

            // Stats: Multiplier and DM bonus
            GameObject ownedObj = new GameObject("Stats");
            ownedObj.transform.SetParent(itemObj.transform, false);
            RectTransform ownedRt = ownedObj.AddComponent<RectTransform>();
            ownedRt.anchorMin = new Vector2(0, 0.08f);
            ownedRt.anchorMax = new Vector2(0.52f, 0.55f);
            ownedRt.offsetMin = new Vector2(210, 10);
            ownedRt.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI ownedTmp = ownedObj.AddComponent<TextMeshProUGUI>();
            string dmBonus = data.dmPerRoll > 0 ? $" • <color=#9966FF>+{data.dmPerRoll} DM</color>" : "";
            ownedTmp.text = $"<color=#FFD700>{multiplierText}x Money</color>{dmBonus}\nOwned: 0";
            ownedTmp.fontSize = UIDesignSystem.FontSizeBody;
            ownedTmp.alignment = TextAlignmentOptions.TopLeft;
            ownedTmp.color = UIDesignSystem.TextSecondary;
            ownedTmp.raycastTarget = false;
            ownedTmp.richText = true;
            ownedTmp.enableAutoSizing = true;
            ownedTmp.fontSizeMin = UIDesignSystem.FontSizeSmall;
            ownedTmp.fontSizeMax = UIDesignSystem.FontSizeBody;

            // === RIGHT SIDE: Big Buy Button with Price ===
            GameObject buyBtnObj = new GameObject("BuyButton");
            buyBtnObj.transform.SetParent(itemObj.transform, false);
            RectTransform buyBtnRt = buyBtnObj.AddComponent<RectTransform>();
            buyBtnRt.anchorMin = new Vector2(0.54f, 0.08f);
            buyBtnRt.anchorMax = new Vector2(0.98f, 0.92f);
            buyBtnRt.offsetMin = new Vector2(5, 10);
            buyBtnRt.offsetMax = new Vector2(-10, -10);

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
            double initialPrice = DiceManager.Instance != null ? DiceManager.Instance.GetCurrentPrice(type) : 0;
            buyTextTmp.text = $"BUY\n${GameUI.FormatNumber(initialPrice)}";
            buyTextTmp.fontSize = UIDesignSystem.FontSizeButtonLarge;
            buyTextTmp.fontStyle = FontStyles.Bold;
            buyTextTmp.alignment = TextAlignmentOptions.Center;
            buyTextTmp.color = UIDesignSystem.TextPrimary;
            buyTextTmp.raycastTarget = false;
            buyTextTmp.richText = true;
            buyTextTmp.enableAutoSizing = true;
            buyTextTmp.fontSizeMin = UIDesignSystem.FontSizeLabel;
            buyTextTmp.fontSizeMax = UIDesignSystem.FontSizeButtonLarge;

            // Lock icon overlay for locked dice - positioned to the right of the text
            GameObject lockObj = new GameObject("LockIcon");
            lockObj.transform.SetParent(buyBtnObj.transform, false);
            RectTransform lockRt = lockObj.AddComponent<RectTransform>();
            // Anchor to right side, vertically centered with the text
            lockRt.anchorMin = new Vector2(1f, 0.5f);
            lockRt.anchorMax = new Vector2(1f, 0.5f);
            lockRt.sizeDelta = new Vector2(40, 40);
            lockRt.anchoredPosition = new Vector2(-15, 0); // 15px from right edge

            Image lockImg = lockObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.iconLock != null)
            {
                lockImg.sprite = guiAssets.iconLock;
            }
            lockImg.color = new Color(0.5f, 0.5f, 0.55f);
            lockImg.raycastTarget = false;
            lockObj.SetActive(false); // Hidden by default

            shopItems[type] = new DiceShopItem
            {
                gameObject = itemObj,
                button = btn,
                background = bg,
                nameText = nameTmp,
                priceText = null,
                ownedText = ownedTmp,
                dicePreview = previewImg,
                buyButton = buyBtn,
                buyButtonText = buyTextTmp,
                buyButtonBg = buyBtnBg,
                lockIcon = lockImg
            };
        }

        public void Show()
        {
            if (isOpen) return;
            isOpen = true;

            // Force rebuild each time to ensure content is populated
            isInitialized = false;
            BuildShop();

            // Apply shared font to all text for consistency
            ApplySharedFontToPanel();

            // Apply black outlines to all text for readability
            ApplyTextOutlinesToPanel();

            // Apply button polish for press/release animations
            if (UIPolishManager.Instance != null)
            {
                UIPolishManager.Instance.PolishButtonsInPanel(shopPanel);
            }

            if (shopPanel != null)
            {
                shopPanel.SetActive(true);

                // Ensure popup is rendered on top of other UI elements (like menu button)
                shopPanel.transform.SetAsLastSibling();

                // Ensure popup renders above effects
                Canvas popupCanvas = shopPanel.GetComponent<Canvas>();
                if (popupCanvas == null)
                {
                    popupCanvas = shopPanel.AddComponent<Canvas>();
                    popupCanvas.overrideSorting = true;
                    shopPanel.AddComponent<GraphicRaycaster>();
                }
                popupCanvas.sortingOrder = 200; // Above screen effects (50) and main UI (100)

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

            // Register with PopupManager
            if (PopupManager.Instance != null)
                PopupManager.Instance.RegisterPopupOpen("DiceShopUI");

            Debug.Log("[DiceShopUI] Opened");
        }

        public void Hide()
        {
            if (!isOpen) return;
            isOpen = false;

            // Unregister from PopupManager
            if (PopupManager.Instance != null)
                PopupManager.Instance.RegisterPopupClosed("DiceShopUI");

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

        /// <summary>
        /// Applies black outlines to all text in the dice shop panel.
        /// </summary>
        private void ApplyTextOutlinesToPanel()
        {
            if (shopPanel == null) return;
            TextMeshProUGUI[] allTexts = shopPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTexts)
            {
                GameUI.ApplyTextOutline(tmp);
            }
        }

        /// <summary>
        /// Applies the shared game font to all text in the dice shop panel.
        /// </summary>
        private void ApplySharedFontToPanel()
        {
            if (shopPanel == null) return;
            if (GameUI.Instance == null) return;

            TMP_FontAsset sharedFont = GameUI.Instance.SharedFont;
            if (sharedFont == null) return;

            TextMeshProUGUI[] allTexts = shopPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTexts)
            {
                if (tmp != null)
                {
                    tmp.font = sharedFont;
                }
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
            DiceData data = DiceManager.Instance.GetDiceData(type);

            // Update owned count text
            if (item.ownedText != null)
            {
                string multiplierText = (data != null && data.basePayout >= 1000) ? $"{data.basePayout/1000}K" : (data?.basePayout.ToString() ?? "1");
                string dmBonus = (data != null && data.dmPerRoll > 0) ? $" • <color=#9966FF>+{data.dmPerRoll} DM</color>" : "";
                item.ownedText.text = $"<color=#FFD700>{multiplierText}x Money</color>{dmBonus}\nOwned: {owned}";
            }

            // Update buy button - keep interactable for feedback (click handler shows messages)
            if (item.buyButton != null)
            {
                item.buyButton.interactable = true;
            }

            // Update button text with price directly on button
            if (item.buyButtonText != null)
            {
                string priceStr = $"${GameUI.FormatNumber(price)}";
                if (!isUnlocked)
                    item.buyButtonText.text = "<b>LOCKED</b>";
                else if (canAfford)
                    item.buyButtonText.text = $"<b>BUY</b>\n{priceStr}";
                else
                    item.buyButtonText.text = $"<color=#FF6666>{priceStr}</color>\nNEED $";
            }

            // Show/hide lock icon overlay
            if (item.lockIcon != null)
            {
                item.lockIcon.gameObject.SetActive(!isUnlocked);
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

            // Check if dice type is locked
            bool isUnlocked = DiceManager.Instance.IsDiceTypeUnlocked(type);
            if (!isUnlocked)
            {
                ShowInsufficientFeedback("Dice type locked!", type);
                return;
            }

            // Check if can afford
            double price = DiceManager.Instance.GetCurrentPrice(type);
            double currentMoney = CurrencyManager.Instance?.Money ?? 0;

            if (currentMoney < price)
            {
                ShowInsufficientFeedback("Not enough $!", type);
                return;
            }

            bool success = DiceManager.Instance.TryBuyDice(type);

            if (success)
            {
                UpdateAllItems();

                // Satisfying purchase animation
                if (shopItems.TryGetValue(type, out var item) && item.buyButton != null)
                {
                    Transform btnTransform = item.buyButton.transform;
                    btnTransform.DOKill();
                    btnTransform.localScale = Vector3.one;

                    // Squeeze then bounce animation
                    Sequence purchaseSeq = DOTween.Sequence();
                    purchaseSeq.Append(btnTransform.DOScale(0.85f, 0.05f).SetEase(Ease.InQuad));
                    purchaseSeq.Append(btnTransform.DOScale(1.2f, 0.12f).SetEase(Ease.OutBack));
                    purchaseSeq.Append(btnTransform.DOScale(1f, 0.1f).SetEase(Ease.InOutSine));
                }

                // Show floating text with dice name
                if (GameUI.Instance != null)
                {
                    DiceData data = DiceManager.Instance.GetDiceData(type);
                    string diceName = data != null ? data.displayName : type.ToString();
                    GameUI.Instance.ShowFloatingText(Vector3.zero, $"+1 {diceName}!", new Color(0.3f, 1f, 0.4f));
                }

                // Play purchase sound
                if (Core.AudioManager.Instance != null)
                {
                    Core.AudioManager.Instance.PlayPurchaseSound();
                }

                // Spawn purchase particle effect
                if (Core.VisualEffectsManager.Instance != null)
                {
                    Core.VisualEffectsManager.Instance.SpawnPurchaseEffect(Vector3.zero);
                }

                // Subtle screen shake for tactile feedback
                Camera cam = Camera.main;
                if (cam != null)
                {
                    cam.transform.DOKill();
                    cam.transform.DOShakePosition(0.1f, 0.015f, 12, 90f, false, true);
                }
            }
        }

        /// <summary>
        /// Shows feedback when player can't afford or dice is locked.
        /// </summary>
        private void ShowInsufficientFeedback(string message, DiceType type)
        {
            // Shake the button
            if (shopItems.TryGetValue(type, out var item) && item.buyButton != null)
            {
                item.buyButton.transform.DOKill();
                item.buyButton.transform.DOShakePosition(0.3f, 5f, 20);
            }

            // Show floating text
            if (GameUI.Instance != null)
            {
                GameUI.Instance.ShowFloatingText(Vector3.zero, message, new Color(1f, 0.4f, 0.4f));
            }
        }

        /// <summary>
        /// Creates a dice face sprite with dots for the shop preview.
        /// </summary>
        private Sprite CreateDiceFaceSprite(int faceValue, Color tintColor)
        {
            int size = 128;
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];

            // Fill with tinted background and rounded corners
            Color bgColor = new Color(
                Mathf.Lerp(0.95f, tintColor.r, 0.3f),
                Mathf.Lerp(0.95f, tintColor.g, 0.3f),
                Mathf.Lerp(0.95f, tintColor.b, 0.3f)
            );
            Color borderColor = new Color(
                tintColor.r * 0.6f,
                tintColor.g * 0.6f,
                tintColor.b * 0.6f
            );
            int cornerRadius = 16;
            int borderWidth = 5;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = IsInsideRoundedRect(x, y, size, size, cornerRadius);
                    bool border = inside && !IsInsideRoundedRect(x, y, size, size, cornerRadius, borderWidth);

                    if (border)
                        pixels[y * size + x] = borderColor;
                    else if (inside)
                        pixels[y * size + x] = bgColor;
                    else
                        pixels[y * size + x] = Color.clear;
                }
            }

            // Draw dots based on face value
            DrawDots(pixels, size, faceValue, tintColor);

            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;

            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128f);
        }

        private bool IsInsideRoundedRect(int x, int y, int width, int height, int radius, int inset = 0)
        {
            int left = inset;
            int right = width - 1 - inset;
            int bottom = inset;
            int top = height - 1 - inset;
            int r = radius - inset;

            if (r <= 0) r = 1;

            // Check corners
            if (x < left + r && y < bottom + r)
                return (x - (left + r)) * (x - (left + r)) + (y - (bottom + r)) * (y - (bottom + r)) <= r * r;
            if (x > right - r && y < bottom + r)
                return (x - (right - r)) * (x - (right - r)) + (y - (bottom + r)) * (y - (bottom + r)) <= r * r;
            if (x < left + r && y > top - r)
                return (x - (left + r)) * (x - (left + r)) + (y - (top - r)) * (y - (top - r)) <= r * r;
            if (x > right - r && y > top - r)
                return (x - (right - r)) * (x - (right - r)) + (y - (top - r)) * (y - (top - r)) <= r * r;

            // Check main rect
            return x >= left && x <= right && y >= bottom && y <= top;
        }

        private void DrawDots(Color[] pixels, int size, int faceValue, Color tintColor)
        {
            int dotRadius = 12;
            Color dotColor = new Color(tintColor.r * 0.3f, tintColor.g * 0.3f, tintColor.b * 0.3f);

            Vector2[] positions = GetDotPositions(faceValue);

            foreach (Vector2 pos in positions)
            {
                int cx = Mathf.RoundToInt(pos.x * (size - 36) + 18);
                int cy = Mathf.RoundToInt(pos.y * (size - 36) + 18);

                // Draw filled circle with anti-aliasing
                for (int y = -dotRadius - 1; y <= dotRadius + 1; y++)
                {
                    for (int x = -dotRadius - 1; x <= dotRadius + 1; x++)
                    {
                        float dist = Mathf.Sqrt(x * x + y * y);
                        if (dist <= dotRadius + 0.5f)
                        {
                            int px = cx + x;
                            int py = cy + y;
                            if (px >= 0 && px < size && py >= 0 && py < size)
                            {
                                float alpha = Mathf.Clamp01(dotRadius + 0.5f - dist);
                                Color existing = pixels[py * size + px];
                                pixels[py * size + px] = Color.Lerp(existing, dotColor, alpha);
                            }
                        }
                    }
                }
            }
        }

        private Vector2[] GetDotPositions(int face)
        {
            switch (face)
            {
                case 1:
                    return new Vector2[] { new Vector2(0.5f, 0.5f) };
                case 2:
                    return new Vector2[] { new Vector2(0.25f, 0.75f), new Vector2(0.75f, 0.25f) };
                case 3:
                    return new Vector2[] { new Vector2(0.25f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(0.75f, 0.25f) };
                case 4:
                    return new Vector2[] { new Vector2(0.25f, 0.25f), new Vector2(0.25f, 0.75f),
                                          new Vector2(0.75f, 0.25f), new Vector2(0.75f, 0.75f) };
                case 5:
                    return new Vector2[] { new Vector2(0.25f, 0.25f), new Vector2(0.25f, 0.75f),
                                          new Vector2(0.5f, 0.5f),
                                          new Vector2(0.75f, 0.25f), new Vector2(0.75f, 0.75f) };
                case 6:
                    return new Vector2[] { new Vector2(0.25f, 0.2f), new Vector2(0.25f, 0.5f), new Vector2(0.25f, 0.8f),
                                          new Vector2(0.75f, 0.2f), new Vector2(0.75f, 0.5f), new Vector2(0.75f, 0.8f) };
                default:
                    return new Vector2[] { new Vector2(0.5f, 0.5f) };
            }
        }
    }
}
