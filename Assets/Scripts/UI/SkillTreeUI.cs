using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.Skills;

namespace Incredicer.UI
{
    /// <summary>
    /// UI controller for the skill tree panel.
    /// </summary>
    public class SkillTreeUI : MonoBehaviour
    {
        public static SkillTreeUI Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private GameObject skillTreePanel;
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI darkMatterText;
        [SerializeField] private TextMeshProUGUI skillPointsText;
        [SerializeField] private Button closeButton;

        [Header("Skill Node Container")]
        [SerializeField] private Transform nodeContainer;
        [SerializeField] private GameObject skillNodePrefab;

        [Header("Node Info Panel")]
        [SerializeField] private GameObject nodeInfoPanel;
        [SerializeField] private TextMeshProUGUI nodeNameText;
        [SerializeField] private TextMeshProUGUI nodeDescriptionText;
        [SerializeField] private TextMeshProUGUI nodeCostText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TextMeshProUGUI purchaseButtonText;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.25f;
        [SerializeField] private float fadeOutDuration = 0.15f;

        private Dictionary<SkillNodeId, SkillNodeButton> nodeButtons = new Dictionary<SkillNodeId, SkillNodeButton>();
        private SkillNodeData selectedNode;
        private bool isOpen;

        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Start hidden
            if (skillTreePanel != null)
            {
                skillTreePanel.SetActive(false);
            }
        }

        private void Start()
        {
            // Setup close button
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            // Setup purchase button
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }

            // Subscribe to skill tree events
            if (SkillTreeManager.Instance != null)
            {
                SkillTreeManager.Instance.OnSkillUnlocked += OnSkillUnlocked;
            }

            // Subscribe to dark matter changes
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnDarkMatterChanged += UpdateDarkMatterDisplay;
            }

            // Hide node info initially
            if (nodeInfoPanel != null)
            {
                nodeInfoPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (SkillTreeManager.Instance != null)
            {
                SkillTreeManager.Instance.OnSkillUnlocked -= OnSkillUnlocked;
            }

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnDarkMatterChanged -= UpdateDarkMatterDisplay;
            }
        }

        /// <summary>
        /// Shows the skill tree panel.
        /// </summary>
        public void Show()
        {
            if (isOpen) return;
            isOpen = true;

            if (skillTreePanel != null)
            {
                skillTreePanel.SetActive(true);

                if (panelCanvasGroup != null)
                {
                    panelCanvasGroup.alpha = 0f;
                    panelCanvasGroup.DOFade(1f, fadeInDuration);
                }
            }

            UpdateDisplay();
            ClearSelection();

            Debug.Log("[SkillTreeUI] Opened");
        }

        /// <summary>
        /// Hides the skill tree panel.
        /// </summary>
        public void Hide()
        {
            if (!isOpen) return;
            isOpen = false;

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.DOFade(0f, fadeOutDuration).OnComplete(() =>
                {
                    if (skillTreePanel != null)
                    {
                        skillTreePanel.SetActive(false);
                    }
                });
            }
            else if (skillTreePanel != null)
            {
                skillTreePanel.SetActive(false);
            }

            Debug.Log("[SkillTreeUI] Closed");
        }

        /// <summary>
        /// Toggles the skill tree panel visibility.
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
        /// Updates all UI elements.
        /// </summary>
        private void UpdateDisplay()
        {
            // Update dark matter display
            if (CurrencyManager.Instance != null)
            {
                UpdateDarkMatterDisplay(CurrencyManager.Instance.DarkMatter);
            }

            // Update skill points
            if (skillPointsText != null && SkillTreeManager.Instance != null)
            {
                skillPointsText.text = $"Skills: {SkillTreeManager.Instance.GetTotalSkillPoints()}";
            }

            // Update node buttons
            UpdateAllNodeButtons();
        }

        /// <summary>
        /// Updates the dark matter display.
        /// </summary>
        private void UpdateDarkMatterDisplay(double amount)
        {
            if (darkMatterText != null)
            {
                darkMatterText.text = $"DM: {GameUI.FormatNumber(amount)}";
            }
        }

        /// <summary>
        /// Updates all node button states.
        /// </summary>
        private void UpdateAllNodeButtons()
        {
            foreach (var kvp in nodeButtons)
            {
                UpdateNodeButton(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Updates a single node button state.
        /// </summary>
        private void UpdateNodeButton(SkillNodeId nodeId, SkillNodeButton button)
        {
            if (SkillTreeManager.Instance == null || button == null) return;

            bool unlocked = SkillTreeManager.Instance.IsNodeUnlocked(nodeId);
            bool canPurchase = SkillTreeManager.Instance.CanPurchaseNode(nodeId);
            bool prereqsMet = SkillTreeManager.Instance.ArePrerequisitesMet(nodeId);

            button.SetState(unlocked, canPurchase, prereqsMet);
        }

        /// <summary>
        /// Called when a skill node button is clicked.
        /// </summary>
        public void OnNodeClicked(SkillNodeData nodeData)
        {
            if (nodeData == null) return;

            selectedNode = nodeData;
            ShowNodeInfo(nodeData);
        }

        /// <summary>
        /// Shows the node info panel for a skill node.
        /// </summary>
        private void ShowNodeInfo(SkillNodeData nodeData)
        {
            if (nodeInfoPanel == null) return;

            nodeInfoPanel.SetActive(true);

            if (nodeNameText != null)
            {
                nodeNameText.text = nodeData.displayName;
            }

            if (nodeDescriptionText != null)
            {
                nodeDescriptionText.text = nodeData.description;
            }

            if (nodeCostText != null)
            {
                nodeCostText.text = $"Cost: {GameUI.FormatNumber(nodeData.darkMatterCost)} DM";
            }

            UpdatePurchaseButton();
        }

        /// <summary>
        /// Updates the purchase button state.
        /// </summary>
        private void UpdatePurchaseButton()
        {
            if (purchaseButton == null || selectedNode == null) return;

            bool unlocked = SkillTreeManager.Instance != null &&
                           SkillTreeManager.Instance.IsNodeUnlocked(selectedNode.nodeId);
            bool canPurchase = SkillTreeManager.Instance != null &&
                              SkillTreeManager.Instance.CanPurchaseNode(selectedNode.nodeId);

            purchaseButton.interactable = canPurchase;

            if (purchaseButtonText != null)
            {
                if (unlocked)
                {
                    purchaseButtonText.text = "Owned";
                    purchaseButton.interactable = false;
                }
                else if (canPurchase)
                {
                    purchaseButtonText.text = "Purchase";
                }
                else
                {
                    purchaseButtonText.text = "Locked";
                }
            }
        }

        /// <summary>
        /// Called when the purchase button is clicked.
        /// </summary>
        private void OnPurchaseClicked()
        {
            if (selectedNode == null || SkillTreeManager.Instance == null) return;

            if (SkillTreeManager.Instance.TryPurchaseNode(selectedNode.nodeId))
            {
                // Success - update UI
                UpdateDisplay();
                UpdatePurchaseButton();

                // Punch animation on purchase button
                if (purchaseButton != null)
                {
                    purchaseButton.transform.DOKill();
                    purchaseButton.transform.localScale = Vector3.one;
                    purchaseButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 5, 0.5f);
                }

                Debug.Log($"[SkillTreeUI] Purchased skill: {selectedNode.displayName}");
            }
            else
            {
                // Failed - shake button
                if (purchaseButton != null)
                {
                    purchaseButton.transform.DOKill();
                    purchaseButton.transform.DOShakePosition(0.2f, 3f, 15);
                }
            }
        }

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        private void ClearSelection()
        {
            selectedNode = null;
            if (nodeInfoPanel != null)
            {
                nodeInfoPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Called when a skill is unlocked.
        /// </summary>
        private void OnSkillUnlocked(SkillNodeId nodeId)
        {
            UpdateDisplay();

            // Animate the unlocked node
            if (nodeButtons.TryGetValue(nodeId, out var button) && button != null)
            {
                button.PlayUnlockAnimation();
            }
        }

        /// <summary>
        /// Registers a node button for tracking.
        /// </summary>
        public void RegisterNodeButton(SkillNodeId nodeId, SkillNodeButton button)
        {
            nodeButtons[nodeId] = button;
            UpdateNodeButton(nodeId, button);
        }
    }

    /// <summary>
    /// Component attached to each skill node button in the UI.
    /// </summary>
    public class SkillNodeButton : MonoBehaviour
    {
        [SerializeField] private SkillNodeData nodeData;
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject lockedOverlay;

        [Header("Colors")]
        [SerializeField] private Color unlockedColor = new Color(0.4f, 0.8f, 0.4f);
        [SerializeField] private Color availableColor = new Color(0.9f, 0.9f, 0.5f);
        [SerializeField] private Color lockedColor = new Color(0.4f, 0.4f, 0.4f);

        private void Start()
        {
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }

            // Register with UI
            if (nodeData != null && SkillTreeUI.Instance != null)
            {
                SkillTreeUI.Instance.RegisterNodeButton(nodeData.nodeId, this);
            }

            // Set icon
            if (iconImage != null && nodeData != null && nodeData.icon != null)
            {
                iconImage.sprite = nodeData.icon;
            }
        }

        private void OnClick()
        {
            if (nodeData != null && SkillTreeUI.Instance != null)
            {
                SkillTreeUI.Instance.OnNodeClicked(nodeData);
            }
        }

        /// <summary>
        /// Sets the visual state of the node button.
        /// </summary>
        public void SetState(bool unlocked, bool canPurchase, bool prereqsMet)
        {
            if (backgroundImage != null)
            {
                if (unlocked)
                {
                    backgroundImage.color = unlockedColor;
                }
                else if (canPurchase)
                {
                    backgroundImage.color = availableColor;
                }
                else
                {
                    backgroundImage.color = lockedColor;
                }
            }

            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(!unlocked && !prereqsMet);
            }

            if (button != null)
            {
                button.interactable = unlocked || prereqsMet;
            }
        }

        /// <summary>
        /// Plays the unlock animation.
        /// </summary>
        public void PlayUnlockAnimation()
        {
            transform.DOKill();
            transform.localScale = Vector3.one;
            transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f);
        }
    }
}
