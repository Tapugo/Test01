using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.Skills;
using Incredicer.Dice;

namespace Incredicer.UI
{
    /// <summary>
    /// Complete skill tree UI with all nodes properly displayed in a branching tree layout.
    /// Blocks game input when open.
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

        [Header("Skill Tree Area")]
        [SerializeField] private RectTransform treeContainer;
        [SerializeField] private RectTransform nodeContainer;
        [SerializeField] private RectTransform connectionContainer;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Info Panel (Below Tree)")]
        [SerializeField] private RectTransform infoPanelRect;
        [SerializeField] private GameObject nodeInfoPanel;
        [SerializeField] private TextMeshProUGUI nodeNameText;
        [SerializeField] private TextMeshProUGUI nodeDescriptionText;
        [SerializeField] private TextMeshProUGUI nodeCostText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TextMeshProUGUI purchaseButtonText;

        [Header("Node Visual Settings")]
        [SerializeField] private float nodeSize = 60f;
        [SerializeField] private float nodeSpacingX = 100f;
        [SerializeField] private float nodeSpacingY = 80f;
        [SerializeField] private float connectionWidth = 3f;
        [SerializeField] private Color unlockedColor = new Color(0.3f, 0.9f, 0.4f);
        [SerializeField] private Color availableColor = new Color(1f, 0.85f, 0.3f);
        [SerializeField] private Color lockedColor = new Color(0.35f, 0.35f, 0.4f);
        [SerializeField] private Color connectionColorUnlocked = new Color(0.3f, 0.9f, 0.4f, 0.8f);
        [SerializeField] private Color connectionColorLocked = new Color(0.4f, 0.4f, 0.45f, 0.5f);

        // Internal skill node definitions (in case ScriptableObjects are missing)
        private class SkillNodeDef
        {
            public SkillNodeId id;
            public string name;
            public string description;
            public string initials;
            public double cost;
            public Vector2 position;
            public SkillBranch branch;
            public List<SkillNodeId> prerequisites;
            public Color branchColor;

            public SkillNodeDef(SkillNodeId id, string name, string desc, string initials, double cost, Vector2 pos, SkillBranch branch, Color color, params SkillNodeId[] prereqs)
            {
                this.id = id;
                this.name = name;
                this.description = desc;
                this.initials = initials;
                this.cost = cost;
                this.position = pos;
                this.branch = branch;
                this.branchColor = color;
                this.prerequisites = new List<SkillNodeId>(prereqs);
            }
        }

        private Dictionary<SkillNodeId, SkillNodeDef> allNodes = new Dictionary<SkillNodeId, SkillNodeDef>();
        private Dictionary<SkillNodeId, NodeVisual> nodeVisuals = new Dictionary<SkillNodeId, NodeVisual>();
        private Dictionary<SkillNodeId, List<Image>> nodeConnections = new Dictionary<SkillNodeId, List<Image>>();
        private SkillNodeDef selectedNode;
        private bool isOpen;
        private bool isInitialized;

        public bool IsOpen => isOpen;

        private class NodeVisual
        {
            public RectTransform rectTransform;
            public Button button;
            public Image background;
            public Image glow;
            public TextMeshProUGUI label;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeSkillNodeDefinitions();

            if (skillTreePanel != null)
            {
                skillTreePanel.SetActive(false);
            }
        }

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }

            if (SkillTreeManager.Instance != null)
            {
                SkillTreeManager.Instance.OnSkillUnlocked += OnSkillUnlocked;
            }

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnDarkMatterChanged += UpdateDarkMatterDisplay;
            }

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
        /// Defines all skill nodes with proper positions for the skill tree layout.
        /// </summary>
        private void InitializeSkillNodeDefinitions()
        {
            allNodes.Clear();

            // Branch colors
            Color coreColor = new Color(0.6f, 0.4f, 0.9f);      // Purple
            Color moneyColor = new Color(0.3f, 0.9f, 0.4f);     // Green
            Color autoColor = new Color(0.4f, 0.7f, 1f);        // Blue
            Color diceColor = new Color(1f, 0.7f, 0.3f);        // Orange
            Color skillColor = new Color(1f, 0.5f, 0.7f);       // Pink

            // === CORE (Center) ===
            AddNode(SkillNodeId.CORE_DarkMatterCore, "Dark Matter Core", "Unlocks the skill tree", "DM", 0, new Vector2(0, 0), SkillBranch.Core, coreColor);

            // === MONEY ENGINE (Top) ===
            AddNode(SkillNodeId.ME_LooseChange, "Loose Change", "+10% to all money gains", "LC", 5, new Vector2(-1.5f, 1), SkillBranch.MoneyEngine, moneyColor, SkillNodeId.CORE_DarkMatterCore);
            AddNode(SkillNodeId.ME_TableTax, "Table Tax", "1% chance for bonus coin", "TT", 10, new Vector2(-0.5f, 1), SkillBranch.MoneyEngine, moneyColor, SkillNodeId.CORE_DarkMatterCore);
            AddNode(SkillNodeId.ME_CompoundInterest, "Compound Interest", "+5% money per owned dice", "CI", 15, new Vector2(-2, 2), SkillBranch.MoneyEngine, moneyColor, SkillNodeId.ME_LooseChange);
            AddNode(SkillNodeId.ME_TipJar, "Tip Jar", "Idle earnings +20%", "TJ", 15, new Vector2(-1, 2), SkillBranch.MoneyEngine, moneyColor, SkillNodeId.ME_LooseChange);
            AddNode(SkillNodeId.ME_BigPayouts, "Big Payouts", "Jackpot multiplier x1.5", "BP", 20, new Vector2(0, 2), SkillBranch.MoneyEngine, moneyColor, SkillNodeId.ME_TableTax);
            AddNode(SkillNodeId.ME_JackpotChance, "Jackpot Chance", "+5% jackpot chance", "JC", 30, new Vector2(-1.5f, 3), SkillBranch.MoneyEngine, moneyColor, SkillNodeId.ME_CompoundInterest, SkillNodeId.ME_TipJar);
            AddNode(SkillNodeId.ME_DarkDividends, "Dark Dividends", "+25% DM from rolls", "DD", 40, new Vector2(-0.5f, 3), SkillBranch.MoneyEngine, moneyColor, SkillNodeId.ME_BigPayouts);
            AddNode(SkillNodeId.ME_InfiniteFloat, "Infinite Float", "All money gains x2", "IF", 100, new Vector2(-1, 4), SkillBranch.MoneyEngine, moneyColor, SkillNodeId.ME_JackpotChance, SkillNodeId.ME_DarkDividends);

            // === AUTOMATION (Left) ===
            AddNode(SkillNodeId.AU_FirstAssistant, "First Assistant", "Unlock Helper Hand", "FA", 5, new Vector2(-2, 0), SkillBranch.Automation, autoColor, SkillNodeId.CORE_DarkMatterCore);
            AddNode(SkillNodeId.AU_GreasedGears, "Greased Gears", "Helpers 25% faster", "GG", 10, new Vector2(-3, 0.5f), SkillBranch.Automation, autoColor, SkillNodeId.AU_FirstAssistant);
            AddNode(SkillNodeId.AU_MoreHands, "More Hands", "+1 max helper", "MH", 15, new Vector2(-3, -0.5f), SkillBranch.Automation, autoColor, SkillNodeId.AU_FirstAssistant);
            AddNode(SkillNodeId.AU_TwoAtOnce, "Two at Once", "Helpers roll 2 dice", "2X", 25, new Vector2(-4, 0.5f), SkillBranch.Automation, autoColor, SkillNodeId.AU_GreasedGears);
            AddNode(SkillNodeId.AU_Overtime, "Overtime", "Helpers 50% faster", "OT", 30, new Vector2(-4, -0.5f), SkillBranch.Automation, autoColor, SkillNodeId.AU_MoreHands);
            AddNode(SkillNodeId.AU_PerfectRhythm, "Perfect Rhythm", "Helpers sync for combos", "PR", 50, new Vector2(-5, 0), SkillBranch.Automation, autoColor, SkillNodeId.AU_TwoAtOnce, SkillNodeId.AU_Overtime);
            AddNode(SkillNodeId.AU_AssemblyLine, "Assembly Line", "+2 max helpers", "AL", 75, new Vector2(-5, -1), SkillBranch.Automation, autoColor, SkillNodeId.AU_Overtime);
            AddNode(SkillNodeId.AU_IdleKing, "Idle King", "Helpers earn bonus DM", "IK", 150, new Vector2(-5.5f, -0.5f), SkillBranch.Automation, autoColor, SkillNodeId.AU_PerfectRhythm, SkillNodeId.AU_AssemblyLine);

            // === DICE EVOLUTION (Right) ===
            AddNode(SkillNodeId.DE_BronzeDice, "Bronze Dice", "Unlock Bronze tier", "BD", 10, new Vector2(2, 0), SkillBranch.DiceEvolution, diceColor, SkillNodeId.CORE_DarkMatterCore);
            AddNode(SkillNodeId.DE_PolishedBronze, "Polished Bronze", "Bronze +15% money", "PB", 15, new Vector2(3, 0.5f), SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_BronzeDice);
            AddNode(SkillNodeId.DE_SilverDice, "Silver Dice", "Unlock Silver tier", "SD", 25, new Vector2(3, -0.5f), SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_BronzeDice);
            AddNode(SkillNodeId.DE_SilverVeins, "Silver Veins", "Silver +20% money", "SV", 35, new Vector2(4, 0.5f), SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_PolishedBronze, SkillNodeId.DE_SilverDice);
            AddNode(SkillNodeId.DE_GoldDice, "Gold Dice", "Unlock Gold tier", "GD", 50, new Vector2(4, -0.5f), SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_SilverDice);
            AddNode(SkillNodeId.DE_GoldRush, "Gold Rush", "Gold +25% DM", "GR", 75, new Vector2(5, 0), SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_SilverVeins, SkillNodeId.DE_GoldDice);
            AddNode(SkillNodeId.DE_EmeraldDice, "Emerald Dice", "Unlock Emerald tier", "ED", 100, new Vector2(5, -1), SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_GoldDice);
            AddNode(SkillNodeId.DE_GemSynergy, "Gem Synergy", "All gems +10%", "GS", 150, new Vector2(6, -0.5f), SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_GoldRush, SkillNodeId.DE_EmeraldDice);
            AddNode(SkillNodeId.DE_RubyDice, "Ruby Dice", "Unlock Ruby tier", "RD", 200, new Vector2(6, -1.5f), SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_EmeraldDice);
            AddNode(SkillNodeId.DE_DiamondDice, "Diamond Dice", "Unlock Diamond tier", "DD", 500, new Vector2(7, -1), SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_GemSynergy, SkillNodeId.DE_RubyDice);

            // === SKILLS & UTILITY (Bottom) ===
            AddNode(SkillNodeId.SK_QuickFlick, "Quick Flick", "Unlock Roll Burst", "QF", 10, new Vector2(0.5f, -1), SkillBranch.SkillsUtility, skillColor, SkillNodeId.CORE_DarkMatterCore);
            AddNode(SkillNodeId.SK_LongReach, "Long Reach", "+25% click radius", "LR", 10, new Vector2(1.5f, -1), SkillBranch.SkillsUtility, skillColor, SkillNodeId.CORE_DarkMatterCore);
            AddNode(SkillNodeId.SK_RollBurstII, "Roll Burst II", "Roll Burst x2 rolls", "R2", 25, new Vector2(0, -2), SkillBranch.SkillsUtility, skillColor, SkillNodeId.SK_QuickFlick);
            AddNode(SkillNodeId.SK_RapidCooldown, "Rapid Cooldown", "-20% skill cooldown", "RC", 25, new Vector2(1, -2), SkillBranch.SkillsUtility, skillColor, SkillNodeId.SK_QuickFlick);
            AddNode(SkillNodeId.SK_FocusedGravity, "Focused Gravity", "Dice cluster together", "FG", 35, new Vector2(2, -2), SkillBranch.SkillsUtility, skillColor, SkillNodeId.SK_LongReach);
            AddNode(SkillNodeId.SK_PrecisionAim, "Precision Aim", "Hold to attract dice", "PA", 50, new Vector2(2.5f, -3), SkillBranch.SkillsUtility, skillColor, SkillNodeId.SK_FocusedGravity);
            AddNode(SkillNodeId.SK_Hyperburst, "Hyperburst", "Unlock mega burst", "HB", 75, new Vector2(0.5f, -3), SkillBranch.SkillsUtility, skillColor, SkillNodeId.SK_RollBurstII, SkillNodeId.SK_RapidCooldown);
            AddNode(SkillNodeId.SK_TimeDilation, "Time Dilation", "2x DM during skills", "TD", 100, new Vector2(1.5f, -3), SkillBranch.SkillsUtility, skillColor, SkillNodeId.SK_RapidCooldown);
        }

        private void AddNode(SkillNodeId id, string name, string desc, string initials, double cost, Vector2 pos, SkillBranch branch, Color color, params SkillNodeId[] prereqs)
        {
            allNodes[id] = new SkillNodeDef(id, name, desc, initials, cost, pos, branch, color, prereqs);
        }

        private void BuildSkillTree()
        {
            if (isInitialized) return;
            isInitialized = true;

            // Ensure we have containers
            if (nodeContainer == null || connectionContainer == null)
            {
                CreateContainers();
            }

            // Clear existing
            nodeVisuals.Clear();
            nodeConnections.Clear();

            foreach (Transform child in nodeContainer)
                Destroy(child.gameObject);
            foreach (Transform child in connectionContainer)
                Destroy(child.gameObject);

            // Create all connections first (so they're behind nodes)
            foreach (var kvp in allNodes)
            {
                CreateNodeConnections(kvp.Value);
            }

            // Create all nodes
            foreach (var kvp in allNodes)
            {
                CreateNodeVisual(kvp.Value);
            }

            Debug.Log($"[SkillTreeUI] Built skill tree with {nodeVisuals.Count} nodes");
        }

        private void CreateContainers()
        {
            // === CREATE FULL UI LAYOUT FOR MOBILE ===
            // Layout from top to bottom:
            // - Header (8%): Title, DM, Close
            // - Tree Area (62%): Scrollable skill tree
            // - Info Panel (18%): Selected skill info
            // - Purchase Button (12%): Big tappable button

            // === HEADER ===
            if (darkMatterText == null)
            {
                GameObject headerObj = new GameObject("Header");
                headerObj.transform.SetParent(skillTreePanel.transform, false);
                RectTransform headerRt = headerObj.AddComponent<RectTransform>();
                headerRt.anchorMin = new Vector2(0, 0.92f);
                headerRt.anchorMax = new Vector2(1, 1);
                headerRt.offsetMin = new Vector2(8, 4);
                headerRt.offsetMax = new Vector2(-8, -4);

                Image headerBg = headerObj.AddComponent<Image>();
                headerBg.color = new Color(0.12f, 0.1f, 0.18f, 0.98f);

                // Title
                GameObject titleObj = new GameObject("Title");
                titleObj.transform.SetParent(headerObj.transform, false);
                RectTransform titleRt = titleObj.AddComponent<RectTransform>();
                titleRt.anchorMin = new Vector2(0, 0);
                titleRt.anchorMax = new Vector2(0.35f, 1);
                titleRt.offsetMin = new Vector2(12, 0);
                titleRt.offsetMax = Vector2.zero;

                TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
                titleText.text = "SKILLS";
                titleText.fontSize = 26;
                titleText.fontStyle = FontStyles.Bold;
                titleText.alignment = TextAlignmentOptions.Left;
                titleText.color = Color.white;

                // DM Display
                GameObject dmObj = new GameObject("DarkMatter");
                dmObj.transform.SetParent(headerObj.transform, false);
                RectTransform dmRt = dmObj.AddComponent<RectTransform>();
                dmRt.anchorMin = new Vector2(0.35f, 0);
                dmRt.anchorMax = new Vector2(0.75f, 1);
                dmRt.offsetMin = Vector2.zero;
                dmRt.offsetMax = Vector2.zero;

                darkMatterText = dmObj.AddComponent<TextMeshProUGUI>();
                darkMatterText.text = "◆ 0 DM";
                darkMatterText.fontSize = 20;
                darkMatterText.alignment = TextAlignmentOptions.Center;
                darkMatterText.color = new Color(0.7f, 0.5f, 1f);

                // Close Button
                GameObject closeObj = new GameObject("CloseButton");
                closeObj.transform.SetParent(headerObj.transform, false);
                RectTransform closeRt = closeObj.AddComponent<RectTransform>();
                closeRt.anchorMin = new Vector2(0.88f, 0.1f);
                closeRt.anchorMax = new Vector2(0.98f, 0.9f);
                closeRt.offsetMin = Vector2.zero;
                closeRt.offsetMax = Vector2.zero;

                Image closeBg = closeObj.AddComponent<Image>();
                closeBg.color = new Color(0.9f, 0.25f, 0.25f);

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
                closeText.text = "✕";
                closeText.fontSize = 28;
                closeText.fontStyle = FontStyles.Bold;
                closeText.alignment = TextAlignmentOptions.Center;
                closeText.color = Color.white;
            }

            // === TREE AREA (Scrollable) ===
            if (treeContainer == null)
            {
                GameObject treeAreaObj = new GameObject("TreeArea");
                treeAreaObj.transform.SetParent(skillTreePanel.transform, false);
                treeContainer = treeAreaObj.AddComponent<RectTransform>();
                treeContainer.anchorMin = new Vector2(0, 0.30f);
                treeContainer.anchorMax = new Vector2(1, 0.92f);
                treeContainer.offsetMin = new Vector2(8, 6);
                treeContainer.offsetMax = new Vector2(-8, -6);

                Image treeBg = treeAreaObj.AddComponent<Image>();
                treeBg.color = new Color(0.05f, 0.05f, 0.08f, 0.98f);

                Mask treeMask = treeAreaObj.AddComponent<Mask>();
                treeMask.showMaskGraphic = true;

                scrollRect = treeAreaObj.AddComponent<ScrollRect>();
                scrollRect.horizontal = true;
                scrollRect.vertical = true;
                scrollRect.movementType = ScrollRect.MovementType.Elastic;
                scrollRect.elasticity = 0.1f;
                scrollRect.inertia = true;
                scrollRect.decelerationRate = 0.135f;
                scrollRect.scrollSensitivity = 25f;
            }

            // Create content container for scroll
            Transform existingContent = treeContainer.Find("Content");
            RectTransform contentRect;
            if (existingContent == null)
            {
                GameObject contentObj = new GameObject("Content");
                contentObj.transform.SetParent(treeContainer, false);
                contentRect = contentObj.AddComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0.5f, 0.5f);
                contentRect.anchorMax = new Vector2(0.5f, 0.5f);
                contentRect.pivot = new Vector2(0.5f, 0.5f);
                contentRect.sizeDelta = new Vector2(1400, 800);
                contentRect.anchoredPosition = Vector2.zero;
                scrollRect.content = contentRect;
            }
            else
            {
                contentRect = existingContent.GetComponent<RectTransform>();
            }

            // Create connection container
            if (connectionContainer == null)
            {
                GameObject connObj = new GameObject("Connections");
                connObj.transform.SetParent(contentRect, false);
                connectionContainer = connObj.AddComponent<RectTransform>();
                connectionContainer.anchorMin = Vector2.zero;
                connectionContainer.anchorMax = Vector2.one;
                connectionContainer.offsetMin = Vector2.zero;
                connectionContainer.offsetMax = Vector2.zero;
            }

            // Create node container
            if (nodeContainer == null)
            {
                GameObject nodeObj = new GameObject("Nodes");
                nodeObj.transform.SetParent(contentRect, false);
                nodeContainer = nodeObj.AddComponent<RectTransform>();
                nodeContainer.anchorMin = Vector2.zero;
                nodeContainer.anchorMax = Vector2.one;
                nodeContainer.offsetMin = Vector2.zero;
                nodeContainer.offsetMax = Vector2.zero;
            }

            // === INFO PANEL (Below Tree) ===
            if (nodeInfoPanel == null)
            {
                GameObject infoObj = new GameObject("InfoPanel");
                infoObj.transform.SetParent(skillTreePanel.transform, false);
                RectTransform infoRt = infoObj.AddComponent<RectTransform>();
                infoRt.anchorMin = new Vector2(0, 0.12f);
                infoRt.anchorMax = new Vector2(1, 0.30f);
                infoRt.offsetMin = new Vector2(8, 4);
                infoRt.offsetMax = new Vector2(-8, -4);

                Image infoBg = infoObj.AddComponent<Image>();
                infoBg.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);

                nodeInfoPanel = infoObj;
                infoPanelRect = infoRt;

                // Skill Name (top of info panel)
                GameObject nameObj = new GameObject("SkillName");
                nameObj.transform.SetParent(infoObj.transform, false);
                RectTransform nameRt = nameObj.AddComponent<RectTransform>();
                nameRt.anchorMin = new Vector2(0, 0.55f);
                nameRt.anchorMax = new Vector2(0.65f, 1);
                nameRt.offsetMin = new Vector2(16, 0);
                nameRt.offsetMax = new Vector2(0, -8);

                nodeNameText = nameObj.AddComponent<TextMeshProUGUI>();
                nodeNameText.text = "Select a Skill";
                nodeNameText.fontSize = 24;
                nodeNameText.fontStyle = FontStyles.Bold;
                nodeNameText.alignment = TextAlignmentOptions.Left;
                nodeNameText.color = Color.white;

                // Cost (top right)
                GameObject costObj = new GameObject("Cost");
                costObj.transform.SetParent(infoObj.transform, false);
                RectTransform costRt = costObj.AddComponent<RectTransform>();
                costRt.anchorMin = new Vector2(0.65f, 0.55f);
                costRt.anchorMax = new Vector2(1, 1);
                costRt.offsetMin = new Vector2(0, 0);
                costRt.offsetMax = new Vector2(-16, -8);

                nodeCostText = costObj.AddComponent<TextMeshProUGUI>();
                nodeCostText.text = "";
                nodeCostText.fontSize = 20;
                nodeCostText.fontStyle = FontStyles.Bold;
                nodeCostText.alignment = TextAlignmentOptions.Right;
                nodeCostText.color = new Color(0.7f, 0.5f, 1f);

                // Description (bottom of info panel)
                GameObject descObj = new GameObject("Description");
                descObj.transform.SetParent(infoObj.transform, false);
                RectTransform descRt = descObj.AddComponent<RectTransform>();
                descRt.anchorMin = new Vector2(0, 0);
                descRt.anchorMax = new Vector2(1, 0.55f);
                descRt.offsetMin = new Vector2(16, 8);
                descRt.offsetMax = new Vector2(-16, 0);

                nodeDescriptionText = descObj.AddComponent<TextMeshProUGUI>();
                nodeDescriptionText.text = "Tap a skill node above to see its details.";
                nodeDescriptionText.fontSize = 16;
                nodeDescriptionText.alignment = TextAlignmentOptions.TopLeft;
                nodeDescriptionText.color = new Color(0.75f, 0.75f, 0.8f);
            }

            // === PURCHASE BUTTON (Bottom - Large for mobile) ===
            if (purchaseButton == null)
            {
                GameObject btnObj = new GameObject("PurchaseButton");
                btnObj.transform.SetParent(skillTreePanel.transform, false);
                RectTransform btnRt = btnObj.AddComponent<RectTransform>();
                btnRt.anchorMin = new Vector2(0.15f, 0.01f);
                btnRt.anchorMax = new Vector2(0.85f, 0.11f);
                btnRt.offsetMin = Vector2.zero;
                btnRt.offsetMax = Vector2.zero;

                Image btnBg = btnObj.AddComponent<Image>();
                btnBg.color = new Color(0.25f, 0.75f, 0.35f);

                purchaseButton = btnObj.AddComponent<Button>();
                purchaseButton.onClick.AddListener(OnPurchaseClicked);

                ColorBlock colors = purchaseButton.colors;
                colors.normalColor = new Color(0.25f, 0.75f, 0.35f);
                colors.highlightedColor = new Color(0.35f, 0.85f, 0.45f);
                colors.pressedColor = new Color(0.2f, 0.6f, 0.3f);
                colors.disabledColor = new Color(0.35f, 0.35f, 0.4f);
                purchaseButton.colors = colors;

                // Button text
                GameObject btnTextObj = new GameObject("Text");
                btnTextObj.transform.SetParent(btnObj.transform, false);
                RectTransform btnTextRt = btnTextObj.AddComponent<RectTransform>();
                btnTextRt.anchorMin = Vector2.zero;
                btnTextRt.anchorMax = Vector2.one;
                btnTextRt.offsetMin = Vector2.zero;
                btnTextRt.offsetMax = Vector2.zero;

                purchaseButtonText = btnTextObj.AddComponent<TextMeshProUGUI>();
                purchaseButtonText.text = "SELECT A SKILL";
                purchaseButtonText.fontSize = 26;
                purchaseButtonText.fontStyle = FontStyles.Bold;
                purchaseButtonText.alignment = TextAlignmentOptions.Center;
                purchaseButtonText.color = Color.white;

                purchaseButton.interactable = false;
            }
        }

        private void CreateNodeVisual(SkillNodeDef nodeDef)
        {
            if (nodeContainer == null) return;

            GameObject nodeObj = new GameObject($"Node_{nodeDef.id}");
            nodeObj.transform.SetParent(nodeContainer, false);

            RectTransform rt = nodeObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(nodeSize, nodeSize);
            // Position with fixed center offset (content is 1400x800)
            Vector2 centerOffset = new Vector2(700f, 320f);
            rt.anchoredPosition = new Vector2(nodeDef.position.x * nodeSpacingX, nodeDef.position.y * nodeSpacingY) + centerOffset;

            // Background (circular shape via rounded corners appearance)
            Image bg = nodeObj.AddComponent<Image>();
            bg.color = lockedColor;
            bg.raycastTarget = true;

            // Button
            Button btn = nodeObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
            btn.colors = colors;

            SkillNodeDef captured = nodeDef;
            btn.onClick.AddListener(() => OnNodeClicked(captured));

            // Inner circle
            GameObject innerObj = new GameObject("Inner");
            innerObj.transform.SetParent(nodeObj.transform, false);
            RectTransform innerRt = innerObj.AddComponent<RectTransform>();
            innerRt.sizeDelta = new Vector2(nodeSize - 6, nodeSize - 6);
            innerRt.anchoredPosition = Vector2.zero;

            Image innerImg = innerObj.AddComponent<Image>();
            innerImg.color = new Color(0.12f, 0.12f, 0.16f);
            innerImg.raycastTarget = false;

            // Label with initials
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(innerObj.transform, false);
            RectTransform labelRt = labelObj.AddComponent<RectTransform>();
            labelRt.sizeDelta = new Vector2(nodeSize - 10, nodeSize - 10);
            labelRt.anchoredPosition = Vector2.zero;

            TextMeshProUGUI labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
            labelTmp.text = nodeDef.initials;
            labelTmp.fontSize = 16;
            labelTmp.fontStyle = FontStyles.Bold;
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.color = nodeDef.branchColor;
            labelTmp.raycastTarget = false;

            // Glow effect (for available nodes)
            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(nodeObj.transform, false);
            glowObj.transform.SetAsFirstSibling();
            RectTransform glowRt = glowObj.AddComponent<RectTransform>();
            glowRt.sizeDelta = new Vector2(nodeSize + 16, nodeSize + 16);
            glowRt.anchoredPosition = Vector2.zero;

            Image glowImg = glowObj.AddComponent<Image>();
            glowImg.color = new Color(nodeDef.branchColor.r, nodeDef.branchColor.g, nodeDef.branchColor.b, 0f);
            glowImg.raycastTarget = false;
            glowObj.SetActive(false);

            nodeVisuals[nodeDef.id] = new NodeVisual
            {
                rectTransform = rt,
                button = btn,
                background = bg,
                glow = glowImg,
                label = labelTmp
            };
        }

        private void CreateNodeConnections(SkillNodeDef nodeDef)
        {
            if (connectionContainer == null) return;
            if (nodeDef.prerequisites.Count == 0) return;

            List<Image> connections = new List<Image>();
            Vector2 centerOffset = new Vector2(700, 320); // Center offset for connections

            Vector2 endPos = new Vector2(nodeDef.position.x * nodeSpacingX, nodeDef.position.y * nodeSpacingY) + centerOffset;

            foreach (var prereqId in nodeDef.prerequisites)
            {
                if (!allNodes.TryGetValue(prereqId, out var prereqDef)) continue;

                Vector2 startPos = new Vector2(prereqDef.position.x * nodeSpacingX, prereqDef.position.y * nodeSpacingY) + centerOffset;

                GameObject lineObj = new GameObject($"Line_{prereqId}_{nodeDef.id}");
                lineObj.transform.SetParent(connectionContainer, false);

                RectTransform lineRt = lineObj.AddComponent<RectTransform>();

                Vector2 direction = endPos - startPos;
                float distance = direction.magnitude;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                // Shorten line to not overlap with nodes
                float shortenBy = nodeSize * 0.6f;
                float lineLength = Mathf.Max(0, distance - shortenBy);

                lineRt.sizeDelta = new Vector2(lineLength, connectionWidth);
                lineRt.anchoredPosition = (startPos + endPos) / 2f;
                lineRt.localRotation = Quaternion.Euler(0, 0, angle);

                Image lineImg = lineObj.AddComponent<Image>();
                lineImg.color = connectionColorLocked;
                lineImg.raycastTarget = false;

                connections.Add(lineImg);
            }

            nodeConnections[nodeDef.id] = connections;
        }

        public void Show()
        {
            if (isOpen) return;
            isOpen = true;

            BuildSkillTree();

            if (skillTreePanel != null)
            {
                skillTreePanel.SetActive(true);

                if (panelCanvasGroup != null)
                {
                    panelCanvasGroup.alpha = 0f;
                    panelCanvasGroup.DOFade(1f, 0.2f);
                    panelCanvasGroup.blocksRaycasts = true;
                    panelCanvasGroup.interactable = true;
                }
            }

            UpdateDisplay();
            ClearSelection();

            // Block dice input
            SetDiceInputBlocked(true);

            Debug.Log("[SkillTreeUI] Opened");
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
                    if (skillTreePanel != null)
                        skillTreePanel.SetActive(false);
                });
                panelCanvasGroup.blocksRaycasts = false;
                panelCanvasGroup.interactable = false;
            }
            else if (skillTreePanel != null)
            {
                skillTreePanel.SetActive(false);
            }

            Debug.Log("[SkillTreeUI] Closed");
        }

        public void Toggle()
        {
            if (isOpen) Hide();
            else Show();
        }

        /// <summary>
        /// Blocks or unblocks dice rolling input.
        /// </summary>
        private void SetDiceInputBlocked(bool blocked)
        {
            if (DiceRollerController.Instance != null)
            {
                DiceRollerController.Instance.enabled = !blocked;
            }
        }

        private void UpdateDisplay()
        {
            if (CurrencyManager.Instance != null)
            {
                UpdateDarkMatterDisplay(CurrencyManager.Instance.DarkMatter);
            }

            if (skillPointsText != null && SkillTreeManager.Instance != null)
            {
                int points = SkillTreeManager.Instance.GetTotalSkillPoints();
                skillPointsText.text = $"Skills: {points}";
            }

            UpdateAllNodeVisuals();
        }

        private void UpdateDarkMatterDisplay(double amount)
        {
            if (darkMatterText != null)
            {
                darkMatterText.text = $"<color=#9966FF>◆</color> {GameUI.FormatNumber(amount)} DM";
            }
        }

        private void UpdateAllNodeVisuals()
        {
            foreach (var kvp in allNodes)
            {
                UpdateNodeVisual(kvp.Key);
            }
        }

        private void UpdateNodeVisual(SkillNodeId nodeId)
        {
            if (!nodeVisuals.TryGetValue(nodeId, out var visual)) return;
            if (!allNodes.TryGetValue(nodeId, out var nodeDef)) return;

            bool unlocked = IsNodeUnlocked(nodeId);
            bool canPurchase = CanPurchaseNode(nodeId);
            bool prereqsMet = ArePrerequisitesMet(nodeId);

            // Update background color
            if (unlocked)
            {
                visual.background.color = unlockedColor;
                visual.label.color = Color.white;
            }
            else if (canPurchase)
            {
                visual.background.color = availableColor;
                visual.label.color = Color.white;
            }
            else
            {
                visual.background.color = lockedColor;
                visual.label.color = prereqsMet ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.4f, 0.4f, 0.4f);
            }

            // Glow for purchasable
            if (visual.glow != null)
            {
                bool showGlow = canPurchase && !unlocked;
                visual.glow.gameObject.SetActive(showGlow);

                if (showGlow)
                {
                    visual.glow.color = new Color(availableColor.r, availableColor.g, availableColor.b, 0.3f);
                    visual.glow.DOKill();
                    visual.glow.DOFade(0.6f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                }
                else
                {
                    visual.glow.DOKill();
                }
            }

            visual.button.interactable = unlocked || prereqsMet;

            // Update connections
            if (nodeConnections.TryGetValue(nodeId, out var connections))
            {
                Color connColor = unlocked ? connectionColorUnlocked : connectionColorLocked;
                foreach (var conn in connections)
                {
                    if (conn != null)
                        conn.color = connColor;
                }
            }
        }

        private bool IsNodeUnlocked(SkillNodeId nodeId)
        {
            if (SkillTreeManager.Instance != null)
                return SkillTreeManager.Instance.IsNodeUnlocked(nodeId);

            // Core node is always "unlocked" for display purposes
            return nodeId == SkillNodeId.CORE_DarkMatterCore;
        }

        private bool ArePrerequisitesMet(SkillNodeId nodeId)
        {
            if (!allNodes.TryGetValue(nodeId, out var nodeDef)) return false;
            if (nodeDef.prerequisites.Count == 0) return true;

            foreach (var prereq in nodeDef.prerequisites)
            {
                if (!IsNodeUnlocked(prereq)) return false;
            }
            return true;
        }

        private bool CanPurchaseNode(SkillNodeId nodeId)
        {
            if (IsNodeUnlocked(nodeId)) return false;
            if (!ArePrerequisitesMet(nodeId)) return false;

            if (!allNodes.TryGetValue(nodeId, out var nodeDef)) return false;
            if (CurrencyManager.Instance == null) return false;

            return CurrencyManager.Instance.DarkMatter >= nodeDef.cost;
        }

        private void OnNodeClicked(SkillNodeDef nodeDef)
        {
            if (nodeDef == null) return;

            selectedNode = nodeDef;
            ShowNodeInfo(nodeDef);

            // Pulse animation
            if (nodeVisuals.TryGetValue(nodeDef.id, out var visual))
            {
                visual.rectTransform.DOKill();
                visual.rectTransform.localScale = Vector3.one;
                visual.rectTransform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0.5f);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRollSound();
            }
        }

        private void ShowNodeInfo(SkillNodeDef nodeDef)
        {
            if (nodeInfoPanel == null) return;

            nodeInfoPanel.SetActive(true);
            nodeInfoPanel.transform.DOKill();
            nodeInfoPanel.transform.localScale = Vector3.one * 0.9f;
            nodeInfoPanel.transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack);

            if (nodeNameText != null)
            {
                nodeNameText.text = nodeDef.name;
                nodeNameText.color = nodeDef.branchColor;
            }

            if (nodeDescriptionText != null)
            {
                nodeDescriptionText.text = nodeDef.description;
            }

            if (nodeCostText != null)
            {
                nodeCostText.text = $"Cost: {GameUI.FormatNumber(nodeDef.cost)} DM";
            }

            UpdatePurchaseButton();
        }

        private void UpdatePurchaseButton()
        {
            if (purchaseButton == null || selectedNode == null) return;

            bool unlocked = IsNodeUnlocked(selectedNode.id);
            bool canPurchase = CanPurchaseNode(selectedNode.id);

            purchaseButton.interactable = canPurchase;

            if (purchaseButtonText != null)
            {
                if (unlocked)
                {
                    purchaseButtonText.text = "OWNED";
                    purchaseButton.interactable = false;
                }
                else if (canPurchase)
                {
                    purchaseButtonText.text = "PURCHASE";
                }
                else if (!ArePrerequisitesMet(selectedNode.id))
                {
                    purchaseButtonText.text = "LOCKED";
                }
                else
                {
                    purchaseButtonText.text = "NEED DM";
                }
            }
        }

        private void OnPurchaseClicked()
        {
            if (selectedNode == null) return;

            if (TryPurchaseNode(selectedNode.id))
            {
                UpdateDisplay();
                UpdatePurchaseButton();

                if (purchaseButton != null)
                {
                    purchaseButton.transform.DOKill();
                    purchaseButton.transform.localScale = Vector3.one;
                    purchaseButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 5, 0.5f);
                }

                if (GameUI.Instance != null)
                {
                    GameUI.Instance.ShowFloatingText(Vector3.zero, $"Skill Unlocked!\n{selectedNode.name}", unlockedColor);
                }

                // Spawn particles
                if (VisualEffectsManager.Instance != null)
                {
                    VisualEffectsManager.Instance.SpawnSkillUnlockEffect(Camera.main.transform.position);
                }

                Debug.Log($"[SkillTreeUI] Purchased: {selectedNode.name}");
            }
            else
            {
                if (purchaseButton != null)
                {
                    purchaseButton.transform.DOKill();
                    purchaseButton.transform.DOShakePosition(0.2f, 3f, 15);
                }
            }
        }

        private bool TryPurchaseNode(SkillNodeId nodeId)
        {
            if (!CanPurchaseNode(nodeId)) return false;
            if (!allNodes.TryGetValue(nodeId, out var nodeDef)) return false;

            // Spend dark matter
            if (!CurrencyManager.Instance.SpendDarkMatter(nodeDef.cost)) return false;

            // Use SkillTreeManager if available
            if (SkillTreeManager.Instance != null)
            {
                SkillTreeManager.Instance.UnlockNode(nodeId);
            }

            return true;
        }

        private void ClearSelection()
        {
            selectedNode = null;
            if (nodeInfoPanel != null)
            {
                nodeInfoPanel.SetActive(false);
            }
        }

        private void OnSkillUnlocked(SkillNodeId nodeId)
        {
            UpdateDisplay();

            if (nodeVisuals.TryGetValue(nodeId, out var visual) && visual != null)
            {
                visual.rectTransform.DOKill();
                visual.rectTransform.localScale = Vector3.one;

                Sequence seq = DOTween.Sequence();
                seq.Append(visual.rectTransform.DOScale(1.4f, 0.15f).SetEase(Ease.OutBack));
                seq.Append(visual.rectTransform.DOScale(1f, 0.1f).SetEase(Ease.InOutQuad));

                if (visual.background != null)
                {
                    visual.background.DOColor(Color.white, 0.1f).SetLoops(2, LoopType.Yoyo);
                }
            }
        }
    }
}
