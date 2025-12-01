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
    /// UI controller for the skill tree panel with proper node rendering and connections.
    /// </summary>
    public class SkillTreeUI : MonoBehaviour
    {
        public static SkillTreeUI Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private GameObject skillTreePanel;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private RectTransform panelRect;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI darkMatterText;
        [SerializeField] private TextMeshProUGUI skillPointsText;
        [SerializeField] private Button closeButton;

        [Header("Skill Node Container")]
        [SerializeField] private RectTransform nodeContainer;
        [SerializeField] private RectTransform connectionContainer;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Node Info Panel (Below Tree)")]
        [SerializeField] private GameObject nodeInfoPanel;
        [SerializeField] private TextMeshProUGUI nodeNameText;
        [SerializeField] private TextMeshProUGUI nodeDescriptionText;
        [SerializeField] private TextMeshProUGUI nodeCostText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TextMeshProUGUI purchaseButtonText;

        [Header("Node Visual Settings")]
        [SerializeField] private float nodeSize = 70f;
        [SerializeField] private float nodeSpacing = 100f;
        [SerializeField] private float connectionWidth = 4f;
        [SerializeField] private Color unlockedColor = new Color(0.3f, 0.9f, 0.4f);
        [SerializeField] private Color availableColor = new Color(1f, 0.85f, 0.3f);
        [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.35f);
        [SerializeField] private Color connectionColorUnlocked = new Color(0.3f, 0.9f, 0.4f, 0.8f);
        [SerializeField] private Color connectionColorLocked = new Color(0.3f, 0.3f, 0.35f, 0.5f);

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.25f;
        [SerializeField] private float fadeOutDuration = 0.15f;

        private Dictionary<SkillNodeId, SkillNodeButton> nodeButtons = new Dictionary<SkillNodeId, SkillNodeButton>();
        private Dictionary<SkillNodeId, List<Image>> nodeConnections = new Dictionary<SkillNodeId, List<Image>>();
        private SkillNodeData selectedNode;
        private bool isOpen;
        private bool isInitialized;

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
        /// Initialize the skill tree UI by creating all node buttons.
        /// </summary>
        private void InitializeSkillTree()
        {
            if (isInitialized) return;
            if (SkillTreeManager.Instance == null) return;

            isInitialized = true;

            // Clear existing
            nodeButtons.Clear();
            nodeConnections.Clear();

            if (nodeContainer != null)
            {
                foreach (Transform child in nodeContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            if (connectionContainer != null)
            {
                foreach (Transform child in connectionContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // Get all skill nodes and create buttons for each
            var allNodes = new List<SkillNodeData>();
            foreach (SkillBranch branch in System.Enum.GetValues(typeof(SkillBranch)))
            {
                allNodes.AddRange(SkillTreeManager.Instance.GetNodesInBranch(branch));
            }

            // First pass: Create all nodes
            foreach (var nodeData in allNodes)
            {
                if (nodeData == null) continue;
                CreateNodeButton(nodeData);
            }

            // Second pass: Create connections
            foreach (var nodeData in allNodes)
            {
                if (nodeData == null) continue;
                CreateNodeConnections(nodeData);
            }

            Debug.Log($"[SkillTreeUI] Initialized with {nodeButtons.Count} nodes");
        }

        /// <summary>
        /// Creates a visual button for a skill node.
        /// </summary>
        private void CreateNodeButton(SkillNodeData nodeData)
        {
            if (nodeContainer == null) return;

            // Create node object
            GameObject nodeObj = new GameObject($"Node_{nodeData.nodeId}");
            nodeObj.transform.SetParent(nodeContainer, false);

            RectTransform rt = nodeObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(nodeSize, nodeSize);
            rt.anchoredPosition = nodeData.treePosition * nodeSpacing;

            // Background circle
            Image bgImage = nodeObj.AddComponent<Image>();
            bgImage.color = lockedColor;

            // Make it round using a circular sprite or mask
            // For now, we'll use a rounded rect appearance
            bgImage.type = Image.Type.Sliced;

            // Add button component
            Button button = nodeObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
            button.colors = colors;

            // Create inner circle for icon area
            GameObject innerObj = new GameObject("Inner");
            innerObj.transform.SetParent(nodeObj.transform, false);
            RectTransform innerRt = innerObj.AddComponent<RectTransform>();
            innerRt.sizeDelta = new Vector2(nodeSize - 8, nodeSize - 8);
            innerRt.anchoredPosition = Vector2.zero;

            Image innerImage = innerObj.AddComponent<Image>();
            innerImage.color = new Color(0.15f, 0.15f, 0.2f);

            // Create icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(innerObj.transform, false);
            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(nodeSize - 24, nodeSize - 24);
            iconRt.anchoredPosition = Vector2.zero;

            Image iconImage = iconObj.AddComponent<Image>();
            if (nodeData.icon != null)
            {
                iconImage.sprite = nodeData.icon;
            }
            else
            {
                // Default icon - just show tier number
                iconImage.color = Color.clear;
            }

            // Create tier text if no icon
            if (nodeData.icon == null)
            {
                GameObject tierTextObj = new GameObject("TierText");
                tierTextObj.transform.SetParent(innerObj.transform, false);
                RectTransform tierRt = tierTextObj.AddComponent<RectTransform>();
                tierRt.sizeDelta = new Vector2(nodeSize - 16, nodeSize - 16);
                tierRt.anchoredPosition = Vector2.zero;

                TextMeshProUGUI tierText = tierTextObj.AddComponent<TextMeshProUGUI>();
                tierText.text = GetNodeInitials(nodeData);
                tierText.fontSize = 16;
                tierText.fontStyle = FontStyles.Bold;
                tierText.alignment = TextAlignmentOptions.Center;
                tierText.color = Color.white;
            }

            // Create glow effect for available nodes
            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(nodeObj.transform, false);
            glowObj.transform.SetAsFirstSibling();
            RectTransform glowRt = glowObj.AddComponent<RectTransform>();
            glowRt.sizeDelta = new Vector2(nodeSize + 12, nodeSize + 12);
            glowRt.anchoredPosition = Vector2.zero;

            Image glowImage = glowObj.AddComponent<Image>();
            glowImage.color = new Color(1f, 0.85f, 0.3f, 0f);
            glowObj.SetActive(false);

            // Create and store the node button component
            SkillNodeButton nodeButton = nodeObj.AddComponent<SkillNodeButton>();
            nodeButton.Initialize(nodeData, button, bgImage, innerImage, iconImage, glowImage);

            button.onClick.AddListener(() => OnNodeClicked(nodeData));

            nodeButtons[nodeData.nodeId] = nodeButton;
        }

        private string GetNodeInitials(SkillNodeData nodeData)
        {
            if (string.IsNullOrEmpty(nodeData.displayName)) return "?";
            string[] words = nodeData.displayName.Split(' ');
            if (words.Length >= 2)
            {
                return $"{words[0][0]}{words[1][0]}";
            }
            return nodeData.displayName.Substring(0, Mathf.Min(2, nodeData.displayName.Length)).ToUpper();
        }

        /// <summary>
        /// Creates connection lines from a node to its prerequisites.
        /// </summary>
        private void CreateNodeConnections(SkillNodeData nodeData)
        {
            if (connectionContainer == null) return;
            if (nodeData.prerequisites == null || nodeData.prerequisites.Count == 0) return;

            if (!nodeButtons.TryGetValue(nodeData.nodeId, out var targetButton)) return;

            List<Image> connections = new List<Image>();

            foreach (var prereqId in nodeData.prerequisites)
            {
                if (!nodeButtons.TryGetValue(prereqId, out var sourceButton)) continue;

                // Create connection line
                GameObject lineObj = new GameObject($"Connection_{prereqId}_{nodeData.nodeId}");
                lineObj.transform.SetParent(connectionContainer, false);

                RectTransform lineRt = lineObj.AddComponent<RectTransform>();

                // Calculate line position and rotation
                Vector2 startPos = SkillTreeManager.Instance.GetNodeData(prereqId)?.treePosition * nodeSpacing ?? Vector2.zero;
                Vector2 endPos = nodeData.treePosition * nodeSpacing;

                Vector2 direction = endPos - startPos;
                float distance = direction.magnitude;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                lineRt.sizeDelta = new Vector2(distance - nodeSize * 0.8f, connectionWidth);
                lineRt.anchoredPosition = (startPos + endPos) / 2f;
                lineRt.localRotation = Quaternion.Euler(0, 0, angle);

                Image lineImage = lineObj.AddComponent<Image>();
                lineImage.color = connectionColorLocked;

                connections.Add(lineImage);
            }

            nodeConnections[nodeData.nodeId] = connections;
        }

        /// <summary>
        /// Shows the skill tree panel.
        /// </summary>
        public void Show()
        {
            if (isOpen) return;
            isOpen = true;

            // Initialize on first show
            InitializeSkillTree();

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
                darkMatterText.text = $"Dark Matter: {GameUI.FormatNumber(amount)}";
            }
        }

        /// <summary>
        /// Updates all node button states.
        /// </summary>
        private void UpdateAllNodeButtons()
        {
            if (SkillTreeManager.Instance == null) return;

            foreach (var kvp in nodeButtons)
            {
                var nodeId = kvp.Key;
                var button = kvp.Value;

                if (button == null) continue;

                bool unlocked = SkillTreeManager.Instance.IsNodeUnlocked(nodeId);
                bool canPurchase = SkillTreeManager.Instance.CanPurchaseNode(nodeId);
                bool prereqsMet = SkillTreeManager.Instance.ArePrerequisitesMet(nodeId);

                button.SetState(unlocked, canPurchase, prereqsMet, unlockedColor, availableColor, lockedColor);

                // Update connections
                if (nodeConnections.TryGetValue(nodeId, out var connections))
                {
                    Color connColor = unlocked ? connectionColorUnlocked : connectionColorLocked;
                    foreach (var conn in connections)
                    {
                        if (conn != null)
                        {
                            conn.color = connColor;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when a skill node button is clicked.
        /// </summary>
        public void OnNodeClicked(SkillNodeData nodeData)
        {
            if (nodeData == null) return;

            selectedNode = nodeData;
            ShowNodeInfo(nodeData);

            // Pulse animation on selected node
            if (nodeButtons.TryGetValue(nodeData.nodeId, out var button))
            {
                button.PlaySelectAnimation();
            }

            // Play click sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRollSound();
            }
        }

        /// <summary>
        /// Shows the node info panel for a skill node.
        /// </summary>
        private void ShowNodeInfo(SkillNodeData nodeData)
        {
            if (nodeInfoPanel == null) return;

            nodeInfoPanel.SetActive(true);

            // Animate panel in
            nodeInfoPanel.transform.DOKill();
            nodeInfoPanel.transform.localScale = Vector3.one * 0.9f;
            nodeInfoPanel.transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack);

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

                // Show floating text
                if (GameUI.Instance != null)
                {
                    GameUI.Instance.ShowFloatingText(Vector3.zero, $"Skill Unlocked!\n{selectedNode.displayName}", new Color(0.4f, 1f, 0.5f));
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
            UpdateAllNodeButtons();
        }
    }

    /// <summary>
    /// Component attached to each skill node button in the UI.
    /// </summary>
    public class SkillNodeButton : MonoBehaviour
    {
        private SkillNodeData nodeData;
        private Button button;
        private Image backgroundImage;
        private Image innerImage;
        private Image iconImage;
        private Image glowImage;

        private bool isUnlocked;
        private bool canPurchase;
        private Sequence pulseSequence;

        public void Initialize(SkillNodeData data, Button btn, Image bg, Image inner, Image icon, Image glow)
        {
            nodeData = data;
            button = btn;
            backgroundImage = bg;
            innerImage = inner;
            iconImage = icon;
            glowImage = glow;
        }

        private void OnDestroy()
        {
            pulseSequence?.Kill();
        }

        /// <summary>
        /// Sets the visual state of the node button.
        /// </summary>
        public void SetState(bool unlocked, bool purchasable, bool prereqsMet, Color unlockedCol, Color availableCol, Color lockedCol)
        {
            isUnlocked = unlocked;
            canPurchase = purchasable;

            if (backgroundImage != null)
            {
                if (unlocked)
                {
                    backgroundImage.color = unlockedCol;
                }
                else if (purchasable)
                {
                    backgroundImage.color = availableCol;
                }
                else
                {
                    backgroundImage.color = lockedCol;
                }
            }

            // Show glow on available nodes
            if (glowImage != null)
            {
                bool showGlow = purchasable && !unlocked;
                glowImage.gameObject.SetActive(showGlow);

                if (showGlow)
                {
                    // Start pulse animation
                    if (pulseSequence == null || !pulseSequence.IsActive())
                    {
                        pulseSequence = DOTween.Sequence();
                        pulseSequence.Append(glowImage.DOFade(0.6f, 0.5f));
                        pulseSequence.Append(glowImage.DOFade(0.2f, 0.5f));
                        pulseSequence.SetLoops(-1);
                    }
                }
                else
                {
                    pulseSequence?.Kill();
                    pulseSequence = null;
                }
            }

            // Dim icon for locked nodes
            if (iconImage != null)
            {
                iconImage.color = unlocked ? Color.white : (prereqsMet ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.4f, 0.4f, 0.4f));
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

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOScale(1.3f, 0.15f).SetEase(Ease.OutBack));
            seq.Append(transform.DOScale(1f, 0.1f).SetEase(Ease.InOutQuad));

            // Flash effect
            if (backgroundImage != null)
            {
                backgroundImage.DOColor(Color.white, 0.1f).SetLoops(2, LoopType.Yoyo);
            }
        }

        /// <summary>
        /// Plays selection animation.
        /// </summary>
        public void PlaySelectAnimation()
        {
            transform.DOKill();
            transform.localScale = Vector3.one;
            transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0.5f);
        }
    }
}
