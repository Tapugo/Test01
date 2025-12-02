using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.Skills;
using Incredicer.Dice;

namespace Incredicer.UI
{
    /// <summary>
    /// Mobile-friendly skill tree UI with vertical scrollable list grouped by branch.
    /// Styled with Layer Lab/GUI-CasualFantasy assets.
    /// </summary>
    public class SkillTreeUI : MonoBehaviour
    {
        public static SkillTreeUI Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private GameObject skillTreePanel;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private RectTransform panelRect;

        [Header("References")]
        [SerializeField] private TextMeshProUGUI darkMatterText;
        [SerializeField] private Button closeButton;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentContainer;

        [Header("GUI Assets")]
        [SerializeField] private GUISpriteAssets guiAssets;

        [Header("Visual Settings")]
        [SerializeField] private float itemHeight = 280f; // MUCH bigger for mobile - 3x size
        [SerializeField] private float headerHeight = 120f; // MUCH bigger branch headers
        [SerializeField] private Color unlockedColor = new Color(0.3f, 0.9f, 0.4f);
        [SerializeField] private Color availableColor = new Color(1f, 0.85f, 0.3f);
        [SerializeField] private Color lockedColor = new Color(0.4f, 0.4f, 0.45f);

        // Skill node data
        private class SkillNodeDef
        {
            public SkillNodeId id;
            public string name;
            public string description;
            public double cost;
            public SkillBranch branch;
            public Color branchColor;
            public List<SkillNodeId> prerequisites;

            public SkillNodeDef(SkillNodeId id, string name, string desc, double cost, SkillBranch branch, Color color, params SkillNodeId[] prereqs)
            {
                this.id = id;
                this.name = name;
                this.description = desc;
                this.cost = cost;
                this.branch = branch;
                this.branchColor = color;
                this.prerequisites = new List<SkillNodeId>(prereqs);
            }
        }

        private class SkillItemUI
        {
            public GameObject gameObject;
            public Button button;
            public Image background;
            public TextMeshProUGUI nameText;
            public TextMeshProUGUI descText;
            public TextMeshProUGUI costText;
            public Button buyButton;
            public TextMeshProUGUI buyButtonText;
            public Image buyButtonBg;
            public Image statusIcon;
            public Image lockIcon;
        }

        private Dictionary<SkillNodeId, SkillNodeDef> allNodes = new Dictionary<SkillNodeId, SkillNodeDef>();
        private Dictionary<SkillNodeId, SkillItemUI> skillItems = new Dictionary<SkillNodeId, SkillItemUI>();
        private bool isOpen;
        private bool isInitialized;

        public bool IsOpen => isOpen;

        // Branch display order and colors
        private readonly SkillBranch[] branchOrder = { SkillBranch.Core, SkillBranch.MoneyEngine, SkillBranch.Automation, SkillBranch.DiceEvolution, SkillBranch.SkillsUtility };
        private readonly Dictionary<SkillBranch, string> branchNames = new Dictionary<SkillBranch, string>
        {
            { SkillBranch.Core, "CORE" },
            { SkillBranch.MoneyEngine, "MONEY ENGINE" },
            { SkillBranch.Automation, "AUTOMATION" },
            { SkillBranch.DiceEvolution, "DICE EVOLUTION" },
            { SkillBranch.SkillsUtility, "SKILLS & UTILITY" }
        };
        private readonly Dictionary<SkillBranch, Color> branchColors = new Dictionary<SkillBranch, Color>
        {
            { SkillBranch.Core, new Color(0.6f, 0.4f, 0.9f) },
            { SkillBranch.MoneyEngine, new Color(0.3f, 0.9f, 0.4f) },
            { SkillBranch.Automation, new Color(0.4f, 0.7f, 1f) },
            { SkillBranch.DiceEvolution, new Color(1f, 0.7f, 0.3f) },
            { SkillBranch.SkillsUtility, new Color(1f, 0.5f, 0.7f) }
        };

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

            InitializeSkillNodeDefinitions();
        }

        private void OnEnable()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (SkillTreeManager.Instance != null)
                SkillTreeManager.Instance.OnSkillUnlocked += OnSkillUnlocked;

            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnDarkMatterChanged += UpdateDarkMatterDisplay;

            if (skillTreePanel != null && skillTreePanel.activeSelf)
                skillTreePanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (SkillTreeManager.Instance != null)
                SkillTreeManager.Instance.OnSkillUnlocked -= OnSkillUnlocked;
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnDarkMatterChanged -= UpdateDarkMatterDisplay;
        }

        private void InitializeSkillNodeDefinitions()
        {
            allNodes.Clear();
            Color coreColor = branchColors[SkillBranch.Core];
            Color moneyColor = branchColors[SkillBranch.MoneyEngine];
            Color autoColor = branchColors[SkillBranch.Automation];
            Color diceColor = branchColors[SkillBranch.DiceEvolution];
            Color skillColor = branchColors[SkillBranch.SkillsUtility];

            // Core - costs 1 DM to unlock the skill tree
            AddNode(SkillNodeId.CORE_DarkMatterCore, "Dark Matter Core", "Unlocks the skill tree. Your journey begins here.", 1, SkillBranch.Core, coreColor);

            // Money Engine
            AddNode(SkillNodeId.ME_LooseChange, "Loose Change", "+10% to all money gains", 5, SkillBranch.MoneyEngine, moneyColor, SkillNodeId.CORE_DarkMatterCore);
            AddNode(SkillNodeId.ME_TableTax, "Table Tax", "1% chance for bonus coin on each roll", 10, SkillBranch.MoneyEngine, moneyColor, SkillNodeId.CORE_DarkMatterCore);
            AddNode(SkillNodeId.ME_CompoundInterest, "Compound Interest", "+5% money per owned dice", 15, SkillBranch.MoneyEngine, moneyColor, SkillNodeId.ME_LooseChange);
            AddNode(SkillNodeId.ME_TipJar, "Tip Jar", "Idle earnings +20%", 15, SkillBranch.MoneyEngine, moneyColor, SkillNodeId.ME_LooseChange);
            AddNode(SkillNodeId.ME_BigPayouts, "Big Payouts", "Jackpot multiplier x1.5", 20, SkillBranch.MoneyEngine, moneyColor, SkillNodeId.ME_TableTax);
            AddNode(SkillNodeId.ME_JackpotChance, "Jackpot Chance", "+5% jackpot chance", 30, SkillBranch.MoneyEngine, moneyColor, SkillNodeId.ME_CompoundInterest, SkillNodeId.ME_TipJar);
            AddNode(SkillNodeId.ME_DarkDividends, "Dark Dividends", "+25% Dark Matter from rolls", 40, SkillBranch.MoneyEngine, moneyColor, SkillNodeId.ME_BigPayouts);
            AddNode(SkillNodeId.ME_InfiniteFloat, "Infinite Float", "All money gains x2", 100, SkillBranch.MoneyEngine, moneyColor, SkillNodeId.ME_JackpotChance, SkillNodeId.ME_DarkDividends);

            // Automation
            AddNode(SkillNodeId.AU_FirstAssistant, "First Assistant", "Unlock Helper Hand automation", 5, SkillBranch.Automation, autoColor, SkillNodeId.CORE_DarkMatterCore);
            AddNode(SkillNodeId.AU_GreasedGears, "Greased Gears", "Helpers roll 25% faster", 10, SkillBranch.Automation, autoColor, SkillNodeId.AU_FirstAssistant);
            AddNode(SkillNodeId.AU_MoreHands, "More Hands", "+1 max helper hand", 15, SkillBranch.Automation, autoColor, SkillNodeId.AU_FirstAssistant);
            AddNode(SkillNodeId.AU_TwoAtOnce, "Two at Once", "Each helper rolls 2 dice", 25, SkillBranch.Automation, autoColor, SkillNodeId.AU_GreasedGears);
            AddNode(SkillNodeId.AU_Overtime, "Overtime", "Helpers 50% faster", 30, SkillBranch.Automation, autoColor, SkillNodeId.AU_MoreHands);
            AddNode(SkillNodeId.AU_PerfectRhythm, "Perfect Rhythm", "Helpers sync for combo bonuses", 50, SkillBranch.Automation, autoColor, SkillNodeId.AU_TwoAtOnce, SkillNodeId.AU_Overtime);
            AddNode(SkillNodeId.AU_AssemblyLine, "Assembly Line", "+2 max helper hands", 75, SkillBranch.Automation, autoColor, SkillNodeId.AU_Overtime);
            AddNode(SkillNodeId.AU_IdleKing, "Idle King", "Helpers earn bonus Dark Matter", 150, SkillBranch.Automation, autoColor, SkillNodeId.AU_PerfectRhythm, SkillNodeId.AU_AssemblyLine);

            // Dice Evolution
            AddNode(SkillNodeId.DE_BronzeDice, "Bronze Dice", "Unlock Bronze tier dice", 10, SkillBranch.DiceEvolution, diceColor, SkillNodeId.CORE_DarkMatterCore);
            AddNode(SkillNodeId.DE_PolishedBronze, "Polished Bronze", "Bronze dice +15% money", 15, SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_BronzeDice);
            AddNode(SkillNodeId.DE_SilverDice, "Silver Dice", "Unlock Silver tier dice", 25, SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_BronzeDice);
            AddNode(SkillNodeId.DE_SilverVeins, "Silver Veins", "Silver dice +20% money", 35, SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_PolishedBronze, SkillNodeId.DE_SilverDice);
            AddNode(SkillNodeId.DE_GoldDice, "Gold Dice", "Unlock Gold tier dice", 50, SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_SilverDice);
            AddNode(SkillNodeId.DE_GoldRush, "Gold Rush", "Gold dice +25% Dark Matter", 75, SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_SilverVeins, SkillNodeId.DE_GoldDice);
            AddNode(SkillNodeId.DE_EmeraldDice, "Emerald Dice", "Unlock Emerald tier dice", 100, SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_GoldDice);
            AddNode(SkillNodeId.DE_GemSynergy, "Gem Synergy", "All gem dice +10% bonus", 150, SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_GoldRush, SkillNodeId.DE_EmeraldDice);
            AddNode(SkillNodeId.DE_RubyDice, "Ruby Dice", "Unlock Ruby tier dice", 200, SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_EmeraldDice);
            AddNode(SkillNodeId.DE_DiamondDice, "Diamond Dice", "Unlock Diamond tier dice", 500, SkillBranch.DiceEvolution, diceColor, SkillNodeId.DE_GemSynergy, SkillNodeId.DE_RubyDice);

            // Skills & Utility
            AddNode(SkillNodeId.SK_QuickFlick, "Quick Flick", "Unlock Roll Burst ability", 10, SkillBranch.SkillsUtility, skillColor, SkillNodeId.CORE_DarkMatterCore);
            AddNode(SkillNodeId.SK_LongReach, "Long Reach", "+25% click radius", 10, SkillBranch.SkillsUtility, skillColor, SkillNodeId.CORE_DarkMatterCore);
            AddNode(SkillNodeId.SK_RollBurstII, "Roll Burst II", "Roll Burst triggers x2 rolls", 25, SkillBranch.SkillsUtility, skillColor, SkillNodeId.SK_QuickFlick);
            AddNode(SkillNodeId.SK_RapidCooldown, "Rapid Cooldown", "-20% skill cooldown", 25, SkillBranch.SkillsUtility, skillColor, SkillNodeId.SK_QuickFlick);
            AddNode(SkillNodeId.SK_FocusedGravity, "Focused Gravity", "Dice cluster together", 35, SkillBranch.SkillsUtility, skillColor, SkillNodeId.SK_LongReach);
            AddNode(SkillNodeId.SK_PrecisionAim, "Precision Aim", "Hold to attract dice to cursor", 50, SkillBranch.SkillsUtility, skillColor, SkillNodeId.SK_FocusedGravity);
            AddNode(SkillNodeId.SK_Hyperburst, "Hyperburst", "Unlock mega burst ability", 75, SkillBranch.SkillsUtility, skillColor, SkillNodeId.SK_RollBurstII, SkillNodeId.SK_RapidCooldown);
            AddNode(SkillNodeId.SK_TimeDilation, "Time Dilation", "2x Dark Matter during skills", 100, SkillBranch.SkillsUtility, skillColor, SkillNodeId.SK_RapidCooldown);
        }

        private void AddNode(SkillNodeId id, string name, string desc, double cost, SkillBranch branch, Color color, params SkillNodeId[] prereqs)
        {
            allNodes[id] = new SkillNodeDef(id, name, desc, cost, branch, color, prereqs);
        }

        private void BuildUI()
        {
            if (isInitialized) return;
            isInitialized = true;

            if (skillTreePanel == null) return;

            // Ensure skill definitions are initialized
            if (allNodes.Count == 0)
            {
                InitializeSkillNodeDefinitions();
            }

            // Clear existing
            skillItems.Clear();
            if (contentContainer != null)
            {
                foreach (Transform child in contentContainer)
                    Destroy(child.gameObject);
            }

            // Create UI structure if needed
            CreateUIStructure();

            // Create skill items grouped by branch
            foreach (var branch in branchOrder)
            {
                CreateBranchHeader(branch);
                foreach (var kvp in allNodes)
                {
                    if (kvp.Value.branch == branch)
                        CreateSkillItem(kvp.Value);
                }
            }

            // Calculate content height manually as fallback
            float totalHeight = 20; // padding top + bottom
            int branchCount = 5;
            int skillCount = skillItems.Count;
            totalHeight += branchCount * (headerHeight + 8); // branch headers + spacing
            totalHeight += skillCount * (itemHeight + 8); // skill items + spacing

            if (contentContainer != null)
            {
                // Force set the content height
                contentContainer.sizeDelta = new Vector2(0, totalHeight);
            }

            // Use coroutine to scroll after layout is applied
            StartCoroutine(ScrollToTopAfterLayout());

            Debug.Log($"[SkillTreeUI] Built UI with {skillItems.Count} skill items, Content height: {totalHeight}");
        }

        private IEnumerator ScrollToTopAfterLayout()
        {
            yield return null; // Wait one frame for layout to be applied
            yield return null; // Extra frame for good measure

            if (contentContainer != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);
            }
            Canvas.ForceUpdateCanvases();

            // Scroll to bottom (which shows the TOP of the content in a vertical scroll)
            // normalizedPosition = 1 means scrolled to TOP of content
            // For vertical scroll with content anchored at top, we need to set it properly
            if (scrollRect != null)
            {
                // Try scrolling to the very bottom first (normalizedPosition = 0), then to top
                scrollRect.verticalNormalizedPosition = 0f;
                yield return null;
                scrollRect.verticalNormalizedPosition = 1f;
                scrollRect.velocity = Vector2.zero;
            }

            // Log final positions for debugging
            Debug.Log($"[SkillTreeUI] After scroll - Content anchoredPos: {contentContainer?.anchoredPosition}, localPos: {contentContainer?.localPosition}, scrollPos: {scrollRect?.verticalNormalizedPosition}");
        }

        private void CreateUIStructure()
        {
            // Clear existing dynamic children first
            foreach (Transform child in skillTreePanel.transform)
            {
                Destroy(child.gameObject);
            }

            // Reset references so they get recreated
            scrollRect = null;
            contentContainer = null;
            darkMatterText = null;
            closeButton = null;

            // Create panel background if needed
            if (panelRect == null)
            {
                panelRect = skillTreePanel.GetComponent<RectTransform>();
                if (panelRect == null)
                    panelRect = skillTreePanel.AddComponent<RectTransform>();
            }

            // Ensure panel fills FULL screen (no margins)
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Add background if missing
            Image panelBg = skillTreePanel.GetComponent<Image>();
            if (panelBg == null)
            {
                panelBg = skillTreePanel.AddComponent<Image>();
                panelBg.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);
            }

            // Add canvas group if missing
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = skillTreePanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                    panelCanvasGroup = skillTreePanel.AddComponent<CanvasGroup>();
            }

            // Create header (always recreated after clearing)
            CreateHeader();

            // Create scroll area (always recreated after clearing)
            CreateScrollArea();
        }

        private void CreateHeader()
        {
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(skillTreePanel.transform, false);
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
                headerBg.color = new Color(0.25f, 0.2f, 0.35f);
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
            titleText.text = "SKILLS";
            titleText.fontSize = 72; // 3x bigger for mobile
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = Color.white;

            // Dark Matter display
            GameObject dmObj = new GameObject("DarkMatter");
            dmObj.transform.SetParent(headerObj.transform, false);
            RectTransform dmRt = dmObj.AddComponent<RectTransform>();
            dmRt.anchorMin = new Vector2(0.35f, 0);
            dmRt.anchorMax = new Vector2(0.8f, 1);
            dmRt.offsetMin = Vector2.zero;
            dmRt.offsetMax = Vector2.zero;

            darkMatterText = dmObj.AddComponent<TextMeshProUGUI>();
            darkMatterText.text = "0 DM";
            darkMatterText.fontSize = 56; // 3x bigger for mobile
            darkMatterText.fontStyle = FontStyles.Bold;
            darkMatterText.alignment = TextAlignmentOptions.Center;
            darkMatterText.color = new Color(0.8f, 0.6f, 1f);

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
            closeText.fontSize = 72; // 3x bigger for mobile
            closeText.fontStyle = FontStyles.Bold;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.color = Color.white;
        }

        private void CreateScrollArea()
        {
            // Create scroll area container
            GameObject scrollObj = new GameObject("ScrollArea");
            scrollObj.transform.SetParent(skillTreePanel.transform, false);
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
            vlg.spacing = 8;
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

        private void CreateBranchHeader(SkillBranch branch)
        {
            if (contentContainer == null) return;

            GameObject headerObj = new GameObject($"Branch_{branch}");
            headerObj.transform.SetParent(contentContainer, false);

            RectTransform rt = headerObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, headerHeight);

            LayoutElement le = headerObj.AddComponent<LayoutElement>();
            le.minHeight = headerHeight;
            le.preferredHeight = headerHeight;

            Image bg = headerObj.AddComponent<Image>();
            bg.color = branchColors[branch];

            // Header text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(headerObj.transform, false);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(20, 5);
            textRt.offsetMax = new Vector2(-20, -5);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = branchNames[branch];
            text.fontSize = 56; // 3x bigger for mobile
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Left;
            text.color = Color.white;
        }

        private void CreateSkillItem(SkillNodeDef nodeDef)
        {
            if (contentContainer == null) return;

            GameObject itemObj = new GameObject($"Skill_{nodeDef.id}");
            itemObj.transform.SetParent(contentContainer, false);

            RectTransform rt = itemObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, itemHeight);

            LayoutElement le = itemObj.AddComponent<LayoutElement>();
            le.minHeight = itemHeight;
            le.preferredHeight = itemHeight;

            Image bg = itemObj.AddComponent<Image>();
            // Use GUI list frame sprite if available
            if (guiAssets != null && guiAssets.listFrame != null)
            {
                bg.sprite = guiAssets.listFrame;
                bg.type = Image.Type.Sliced;
                bg.color = new Color(0.2f, 0.2f, 0.25f);
            }
            else
            {
                bg.color = new Color(0.15f, 0.15f, 0.2f, 0.98f);
            }

            Button btn = itemObj.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;

            // Left color bar (branch indicator)
            GameObject barObj = new GameObject("BranchBar");
            barObj.transform.SetParent(itemObj.transform, false);
            RectTransform barRt = barObj.AddComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0, 0);
            barRt.anchorMax = new Vector2(0, 1);
            barRt.pivot = new Vector2(0, 0.5f);
            barRt.anchoredPosition = Vector2.zero;
            barRt.sizeDelta = new Vector2(12, 0);

            Image barImg = barObj.AddComponent<Image>();
            barImg.color = nodeDef.branchColor;

            // === LEFT SIDE: Skill Info (wider for more description space) ===
            // Skill name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(itemObj.transform, false);
            RectTransform nameRt = nameObj.AddComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0.58f);
            nameRt.anchorMax = new Vector2(0.56f, 1);  // Adjusted for wider button
            nameRt.offsetMin = new Vector2(24, 0);
            nameRt.offsetMax = new Vector2(-5, -12);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = nodeDef.name;
            nameText.fontSize = 46;
            nameText.fontStyle = FontStyles.Bold;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.color = Color.white;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = 32;
            nameText.fontSizeMax = 46;

            // Description - BIGGER text, more space
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(itemObj.transform, false);
            RectTransform descRt = descObj.AddComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0, 0.08f);
            descRt.anchorMax = new Vector2(0.56f, 0.58f);  // Adjusted for wider button
            descRt.offsetMin = new Vector2(24, 10);
            descRt.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = nodeDef.description;
            descText.fontSize = 38;
            descText.alignment = TextAlignmentOptions.TopLeft;
            descText.color = new Color(0.75f, 0.75f, 0.8f);
            descText.enableAutoSizing = true;
            descText.fontSizeMin = 28;
            descText.fontSizeMax = 38;

            // === RIGHT SIDE: Bigger Buy Button with Price ===
            GameObject buyObj = new GameObject("BuyButton");
            buyObj.transform.SetParent(itemObj.transform, false);
            RectTransform buyRt = buyObj.AddComponent<RectTransform>();
            buyRt.anchorMin = new Vector2(0.58f, 0.1f);  // Wider button (was 0.67f)
            buyRt.anchorMax = new Vector2(0.98f, 0.9f);
            buyRt.offsetMin = new Vector2(5, 10);
            buyRt.offsetMax = new Vector2(-10, -10);

            Image buyBg = buyObj.AddComponent<Image>();
            // Use GUI button sprite if available
            if (guiAssets != null && guiAssets.buttonYellow != null)
            {
                buyBg.sprite = guiAssets.buttonYellow;
                buyBg.type = Image.Type.Sliced;
                buyBg.color = Color.white;
            }
            else
            {
                buyBg.color = availableColor;
            }

            Button buyBtn = buyObj.AddComponent<Button>();
            SkillNodeId capturedId = nodeDef.id;
            buyBtn.onClick.AddListener(() => OnBuyClicked(capturedId));

            GameObject buyTextObj = new GameObject("Text");
            buyTextObj.transform.SetParent(buyObj.transform, false);
            RectTransform buyTextRt = buyTextObj.AddComponent<RectTransform>();
            buyTextRt.anchorMin = Vector2.zero;
            buyTextRt.anchorMax = Vector2.one;
            buyTextRt.offsetMin = Vector2.zero;
            buyTextRt.offsetMax = Vector2.zero;

            TextMeshProUGUI buyText = buyTextObj.AddComponent<TextMeshProUGUI>();
            // Price displayed directly on the button
            string priceText = nodeDef.cost > 0 ? $"{GameUI.FormatNumber(nodeDef.cost)} DM" : "FREE";
            buyText.text = $"UNLOCK\n<size=80%>{priceText}</size>";
            buyText.fontSize = 42;
            buyText.fontStyle = FontStyles.Bold;
            buyText.alignment = TextAlignmentOptions.Center;
            buyText.color = Color.white;
            buyText.richText = true;
            buyText.enableAutoSizing = true;
            buyText.fontSizeMin = 28;
            buyText.fontSizeMax = 42;

            // Lock icon overlay for locked skills - positioned to the right of the text
            GameObject lockObj = new GameObject("LockIcon");
            lockObj.transform.SetParent(buyObj.transform, false);
            RectTransform lockRt = lockObj.AddComponent<RectTransform>();
            // Anchor to right side, vertically centered with the text
            lockRt.anchorMin = new Vector2(1f, 0.5f);
            lockRt.anchorMax = new Vector2(1f, 0.5f);
            lockRt.sizeDelta = new Vector2(40, 40);
            lockRt.anchoredPosition = new Vector2(-15, 0); // 15px from right edge

            Image lockImg = lockObj.AddComponent<Image>();
            // Use GUI lock icon if available
            if (guiAssets != null && guiAssets.iconLock != null)
            {
                lockImg.sprite = guiAssets.iconLock;
            }
            lockImg.color = new Color(0.5f, 0.5f, 0.55f);
            lockImg.raycastTarget = false;
            lockObj.SetActive(false); // Hidden by default

            skillItems[nodeDef.id] = new SkillItemUI
            {
                gameObject = itemObj,
                button = btn,
                background = bg,
                nameText = nameText,
                descText = descText,
                costText = null,
                buyButton = buyBtn,
                buyButtonText = buyText,
                buyButtonBg = buyBg,
                lockIcon = lockImg
            };
        }

        public void Show()
        {
            if (isOpen) return;
            isOpen = true;

            isInitialized = false;
            BuildUI();

            // Apply shared font to all text for consistency
            ApplySharedFontToPanel();

            // Apply black outlines to all text for readability
            ApplyTextOutlinesToPanel();

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
            SetDiceInputBlocked(true);
            Debug.Log("[SkillTreeUI] Opened");
        }

        public void Hide()
        {
            if (!isOpen) return;
            isOpen = false;

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

        private void SetDiceInputBlocked(bool blocked)
        {
            if (DiceRollerController.Instance != null)
                DiceRollerController.Instance.enabled = !blocked;
        }

        /// <summary>
        /// Applies black outlines to all text in the skill tree panel.
        /// </summary>
        private void ApplyTextOutlinesToPanel()
        {
            if (skillTreePanel == null) return;
            TextMeshProUGUI[] allTexts = skillTreePanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTexts)
            {
                GameUI.ApplyTextOutline(tmp);
            }
        }

        /// <summary>
        /// Applies the shared game font to all text in the skill tree panel.
        /// </summary>
        private void ApplySharedFontToPanel()
        {
            if (skillTreePanel == null) return;
            if (GameUI.Instance == null) return;

            TMP_FontAsset sharedFont = GameUI.Instance.SharedFont;
            if (sharedFont == null) return;

            TextMeshProUGUI[] allTexts = skillTreePanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTexts)
            {
                if (tmp != null)
                {
                    tmp.font = sharedFont;
                }
            }
        }

        private void UpdateDisplay()
        {
            if (CurrencyManager.Instance != null)
                UpdateDarkMatterDisplay(CurrencyManager.Instance.DarkMatter);

            UpdateAllItems();
        }

        private void UpdateDarkMatterDisplay(double amount)
        {
            if (darkMatterText != null)
                darkMatterText.text = $"{GameUI.FormatNumber(amount)} DM";
        }

        private void UpdateAllItems()
        {
            double dm = CurrencyManager.Instance?.DarkMatter ?? 0;

            foreach (var kvp in skillItems)
            {
                UpdateItem(kvp.Key, kvp.Value, dm);
            }
        }

        private void UpdateItem(SkillNodeId nodeId, SkillItemUI item, double currentDM)
        {
            if (item == null) return;
            if (!allNodes.TryGetValue(nodeId, out var nodeDef)) return;

            bool unlocked = IsNodeUnlocked(nodeId);
            bool prereqsMet = ArePrerequisitesMet(nodeId);
            bool canAfford = currentDM >= nodeDef.cost;
            bool canPurchase = !unlocked && prereqsMet && canAfford;

            // Update button state - keep interactable for feedback, but disable for already owned
            if (item.buyButton != null)
                item.buyButton.interactable = !unlocked;

            if (item.buyButtonText != null)
            {
                string priceText = nodeDef.cost > 0 ? $"{GameUI.FormatNumber(nodeDef.cost)} DM" : "FREE";
                if (unlocked)
                    item.buyButtonText.text = "<b>OWNED</b>";
                else if (!prereqsMet)
                    item.buyButtonText.text = "<b>LOCKED</b>";
                else if (canAfford)
                    item.buyButtonText.text = $"<b>UNLOCK</b>\n<size=85%>{priceText}</size>";
                else
                    item.buyButtonText.text = $"<color=#FF6666>{priceText}</color>\n<size=80%>NEED DM</size>";
            }

            // Show/hide lock icon overlay
            if (item.lockIcon != null)
            {
                item.lockIcon.gameObject.SetActive(!unlocked && !prereqsMet);
            }

            if (item.buyButtonBg != null)
            {
                // Update button sprite and color based on state
                if (guiAssets != null)
                {
                    if (unlocked && guiAssets.buttonGreen != null)
                    {
                        item.buyButtonBg.sprite = guiAssets.buttonGreen;
                        item.buyButtonBg.color = Color.white;
                    }
                    else if (canPurchase && guiAssets.buttonYellow != null)
                    {
                        item.buyButtonBg.sprite = guiAssets.buttonYellow;
                        item.buyButtonBg.color = Color.white;
                    }
                    else if (guiAssets.buttonGray != null)
                    {
                        item.buyButtonBg.sprite = guiAssets.buttonGray;
                        item.buyButtonBg.color = Color.white;
                    }
                    else
                    {
                        item.buyButtonBg.color = unlocked ? unlockedColor : (canPurchase ? availableColor : lockedColor);
                    }
                }
                else
                {
                    if (unlocked)
                        item.buyButtonBg.color = unlockedColor;
                    else if (canPurchase)
                        item.buyButtonBg.color = availableColor;
                    else
                        item.buyButtonBg.color = lockedColor;
                }
            }

            // Update background
            if (item.background != null)
            {
                if (unlocked)
                    item.background.color = new Color(0.15f, 0.25f, 0.15f, 0.98f);
                else if (prereqsMet)
                    item.background.color = new Color(0.18f, 0.18f, 0.22f, 0.98f);
                else
                    item.background.color = new Color(0.12f, 0.12f, 0.15f, 0.98f);
            }

            // Update name color
            if (item.nameText != null)
            {
                if (unlocked)
                    item.nameText.color = unlockedColor;
                else if (prereqsMet)
                    item.nameText.color = Color.white;
                else
                    item.nameText.color = new Color(0.5f, 0.5f, 0.55f);
            }
        }

        private bool IsNodeUnlocked(SkillNodeId nodeId)
        {
            if (SkillTreeManager.Instance != null)
                return SkillTreeManager.Instance.IsNodeUnlocked(nodeId);
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

        private void OnBuyClicked(SkillNodeId nodeId)
        {
            if (!allNodes.TryGetValue(nodeId, out var nodeDef)) return;
            if (IsNodeUnlocked(nodeId)) return;

            // Check prerequisites - show locked feedback
            if (!ArePrerequisitesMet(nodeId))
            {
                ShowInsufficientFeedback("Unlock prerequisites first!", nodeId);
                return;
            }

            double dm = CurrencyManager.Instance?.DarkMatter ?? 0;

            // Check if can afford - show not enough DM feedback
            if (dm < nodeDef.cost)
            {
                ShowInsufficientFeedback("Not enough DM!", nodeId);
                return;
            }

            if (!CurrencyManager.Instance.SpendDarkMatter(nodeDef.cost)) return;

            if (SkillTreeManager.Instance != null)
                SkillTreeManager.Instance.UnlockNode(nodeId);

            UpdateDisplay();

            // Satisfying unlock animation
            if (skillItems.TryGetValue(nodeId, out var item) && item.buyButton != null)
            {
                Transform btnTransform = item.buyButton.transform;
                btnTransform.DOKill();
                btnTransform.localScale = Vector3.one;

                // Squeeze then bounce animation
                Sequence unlockSeq = DOTween.Sequence();
                unlockSeq.Append(btnTransform.DOScale(0.8f, 0.05f).SetEase(Ease.InQuad));
                unlockSeq.Append(btnTransform.DOScale(1.25f, 0.15f).SetEase(Ease.OutBack));
                unlockSeq.Append(btnTransform.DOScale(1f, 0.1f).SetEase(Ease.InOutSine));

                // Flash the button green briefly
                if (item.buyButtonBg != null)
                {
                    item.buyButtonBg.DOColor(new Color(0.3f, 1f, 0.4f), 0.1f)
                        .OnComplete(() => item.buyButtonBg.DOColor(Color.white, 0.2f));
                }
            }

            // Show floating text
            if (GameUI.Instance != null)
                GameUI.Instance.ShowFloatingText(Vector3.zero, $"Unlocked: {nodeDef.name}!", unlockedColor);

            // Play skill unlock sound
            if (Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlaySkillUnlockSound();
            }

            // Spawn skill unlock particle effect
            if (Core.VisualEffectsManager.Instance != null)
            {
                Core.VisualEffectsManager.Instance.SpawnSkillUnlockEffect(Vector3.zero);
            }

            // Subtle screen shake for tactile feedback
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.DOKill();
                cam.transform.DOShakePosition(0.15f, 0.02f, 15, 90f, false, true);
            }

            Debug.Log($"[SkillTreeUI] Purchased: {nodeDef.name}");
        }

        /// <summary>
        /// Shows feedback when player can't afford or prerequisites not met.
        /// </summary>
        private void ShowInsufficientFeedback(string message, SkillNodeId nodeId)
        {
            // Shake the button
            if (skillItems.TryGetValue(nodeId, out var item) && item.buyButton != null)
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

        private void OnSkillUnlocked(SkillNodeId nodeId)
        {
            UpdateDisplay();
        }
    }
}
