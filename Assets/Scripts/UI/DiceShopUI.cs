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
    /// Includes proper input blocking when open.
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
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private TextMeshProUGUI moneyDisplayText;

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
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

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

            if (contentContainer == null)
            {
                CreateDefaultLayout();
            }

            // Clear existing items
            shopItems.Clear();
            foreach (Transform child in contentContainer)
            {
                Destroy(child.gameObject);
            }

            // Create items for each dice type
            foreach (DiceType type in System.Enum.GetValues(typeof(DiceType)))
            {
                CreateShopItem(type);
            }

            UpdateAllItems();
            Debug.Log($"[DiceShopUI] Built shop with {shopItems.Count} items");
        }

        private void CreateDefaultLayout()
        {
            // Create content container if not assigned
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(shopPanel.transform, false);
            contentContainer = contentObj.AddComponent<RectTransform>();
            contentContainer.anchorMin = new Vector2(0, 0);
            contentContainer.anchorMax = new Vector2(1, 0.85f);
            contentContainer.offsetMin = new Vector2(10, 10);
            contentContainer.offsetMax = new Vector2(-10, -10);

            // Add vertical layout group
            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            // Add content size fitter
            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void CreateShopItem(DiceType type)
        {
            if (contentContainer == null) return;

            DiceData data = DiceManager.Instance?.GetDiceData(type);
            if (data == null) return;

            // Create item container
            GameObject itemObj = new GameObject($"Item_{type}");
            itemObj.transform.SetParent(contentContainer, false);

            RectTransform rt = itemObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 70);

            LayoutElement le = itemObj.AddComponent<LayoutElement>();
            le.minHeight = 70;
            le.preferredHeight = 70;

            Image bg = itemObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            Button btn = itemObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f);
            btn.colors = colors;

            DiceType capturedType = type;
            btn.onClick.AddListener(() => OnBuyClicked(capturedType));

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

            // Price text (right side)
            GameObject priceObj = new GameObject("Price");
            priceObj.transform.SetParent(itemObj.transform, false);
            RectTransform priceRt = priceObj.AddComponent<RectTransform>();
            priceRt.anchorMin = new Vector2(0.5f, 0);
            priceRt.anchorMax = new Vector2(1, 1);
            priceRt.offsetMin = new Vector2(10, 10);
            priceRt.offsetMax = new Vector2(-15, -10);

            TextMeshProUGUI priceTmp = priceObj.AddComponent<TextMeshProUGUI>();
            priceTmp.text = "$0";
            priceTmp.fontSize = 22;
            priceTmp.fontStyle = FontStyles.Bold;
            priceTmp.alignment = TextAlignmentOptions.Right;
            priceTmp.color = affordableColor;
            priceTmp.raycastTarget = false;

            shopItems[type] = new DiceShopItem
            {
                gameObject = itemObj,
                button = btn,
                background = bg,
                nameText = nameTmp,
                priceText = priceTmp,
                ownedText = ownedTmp,
                dicePreview = previewImg
            };
        }

        public void Show()
        {
            if (isOpen) return;
            isOpen = true;

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

            // Update colors and interactability
            if (!isUnlocked)
            {
                if (item.background != null)
                    item.background.color = lockedColor;
                if (item.priceText != null)
                    item.priceText.color = new Color(0.5f, 0.5f, 0.5f);
                if (item.button != null)
                    item.button.interactable = false;
            }
            else if (canAfford)
            {
                if (item.background != null)
                    item.background.color = new Color(0.15f, 0.2f, 0.15f, 0.95f);
                if (item.priceText != null)
                    item.priceText.color = affordableColor;
                if (item.button != null)
                    item.button.interactable = true;
            }
            else
            {
                if (item.background != null)
                    item.background.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
                if (item.priceText != null)
                    item.priceText.color = unaffordableColor;
                if (item.button != null)
                    item.button.interactable = false;
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
                if (shopItems.TryGetValue(type, out var item) && item.button != null)
                {
                    item.button.transform.DOKill();
                    item.button.transform.localScale = Vector3.one;
                    item.button.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.5f);
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
                if (shopItems.TryGetValue(type, out var item) && item.button != null)
                {
                    item.button.transform.DOKill();
                    item.button.transform.DOShakePosition(0.2f, 3f, 15);
                }
            }
        }
    }
}
