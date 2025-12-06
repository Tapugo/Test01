using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.UI;

namespace Incredicer.Leaderboards
{
    /// <summary>
    /// UI for displaying leaderboards with tabs and scrollable entries.
    /// Fullscreen display with fake players that dynamically change scores.
    /// </summary>
    public class LeaderboardUI : MonoBehaviour
    {
        // GUI Assets reference
        private GUISpriteAssets guiAssets;

        // UI References
        private GameObject panel;
        private GameObject mainPanel;
        private CanvasGroup panelCanvasGroup;
        private GameObject tabsContainer;
        private GameObject entriesContainer;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI playerRankText;
        private TextMeshProUGUI refreshCooldownText;
        private Button refreshButton;
        private Button closeButton;
        private ScrollRect scrollRect;
        private GameObject titleRibbonObj;
        private Image playerRowGlow;
        private GameObject trophyIcon;

        // State
        private LeaderboardType currentType = LeaderboardType.LifetimeMoney;
        private Dictionary<LeaderboardType, Button> tabButtons = new Dictionary<LeaderboardType, Button>();
        private bool isVisible = false;

        // Fake players for simulation
        private List<FakePlayer> fakePlayers = new List<FakePlayer>();
        private float fakePlayerUpdateTimer = 0f;
        private int playerInsertedRank = -1;  // Where to insert the actual player

        private class FakePlayer
        {
            public string name;
            public double score;
            public int rank;
            public GameObject rowObject;
            public TextMeshProUGUI scoreText;
        }

        private string[] fakePlayerNames = new string[]
        {
            "DiceMaster99", "LuckyRoller", "CriticalHit7", "GoldenDice", "RNGKing",
            "FortuneFavors", "HighRoller42", "DiceGoblin", "NatTwenty", "ChaosDice",
            "QuantumRoll", "InfiniteLoop", "TimeLord77", "DarkMatterX", "CosmicDice",
            "PixelPusher", "ByteMaster", "CodeNinja", "DataDruid", "AlgoWizard",
            "ShadowDice", "NeonRoller", "PhantomLuck", "StarGazer", "MoonWalker",
            "SunChaser", "CloudNine", "ThunderStrike", "IceBreaker", "FireStorm"
        };

        private void Start()
        {
            // Get GUI assets reference
            guiAssets = GUISpriteAssets.Instance;

            CreateUI();
            panel.SetActive(false);

            // Subscribe to events
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.OnLeaderboardUpdated += OnLeaderboardUpdated;
                LeaderboardManager.Instance.OnPlayerRankChanged += OnPlayerRankChanged;
            }
        }

        private void OnDestroy()
        {
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.OnLeaderboardUpdated -= OnLeaderboardUpdated;
                LeaderboardManager.Instance.OnPlayerRankChanged -= OnPlayerRankChanged;
            }
        }

        private void Update()
        {
            if (isVisible)
            {
                // Update fake player scores periodically
                fakePlayerUpdateTimer -= Time.deltaTime;
                if (fakePlayerUpdateTimer <= 0f)
                {
                    UpdateFakePlayerScores();
                    fakePlayerUpdateTimer = UnityEngine.Random.Range(2f, 5f);
                }
            }
        }

        private void CreateUI()
        {
            // Background overlay - fullscreen
            panel = new GameObject("LeaderboardPanel");
            panel.transform.SetParent(transform, false);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.92f);

            panelCanvasGroup = panel.AddComponent<CanvasGroup>();

            // Main content panel - fullscreen with padding
            mainPanel = new GameObject("MainPanel");
            mainPanel.transform.SetParent(panel.transform, false);

            RectTransform mainRect = mainPanel.AddComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.02f, 0.02f);
            mainRect.anchorMax = new Vector2(0.98f, 0.92f);  // Lower top to avoid phone notch
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;

            Image mainBg = mainPanel.AddComponent<Image>();
            if (guiAssets != null && guiAssets.popupBackground != null)
            {
                mainBg.sprite = guiAssets.popupBackground;
                mainBg.type = Image.Type.Sliced;
                mainBg.color = Color.white;
            }
            else
            {
                mainBg.color = new Color(0.06f, 0.05f, 0.1f, 0.98f);
            }

            // Close button (create first so it's on top)
            CreateCloseButton();

            // Header
            CreateHeader();

            // Tabs
            CreateTabs();

            // Entries section (includes player row)
            CreateEntriesSection();

            // Player rank display at bottom
            CreatePlayerRankDisplay();
        }

        private void CreateHeader()
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(mainPanel.transform, false);

            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.sizeDelta = new Vector2(0, 110);
            headerRect.anchoredPosition = Vector2.zero;

            // Title ribbon background
            titleRibbonObj = new GameObject("TitleRibbon");
            titleRibbonObj.transform.SetParent(header.transform, false);

            RectTransform ribbonRect = titleRibbonObj.AddComponent<RectTransform>();
            ribbonRect.anchorMin = new Vector2(0.5f, 0.5f);
            ribbonRect.anchorMax = new Vector2(0.5f, 0.5f);
            ribbonRect.sizeDelta = new Vector2(500, 80);
            ribbonRect.anchoredPosition = new Vector2(0, -10);

            Image ribbonImg = titleRibbonObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.ribbonYellow != null)
            {
                ribbonImg.sprite = guiAssets.ribbonYellow;
                ribbonImg.type = Image.Type.Sliced;
                ribbonImg.color = Color.white;
            }
            else
            {
                ribbonImg.color = new Color(0.9f, 0.7f, 0.2f);
            }

            // Trophy icon on left
            trophyIcon = new GameObject("TrophyIcon");
            trophyIcon.transform.SetParent(header.transform, false);

            RectTransform trophyRect = trophyIcon.AddComponent<RectTransform>();
            trophyRect.anchorMin = new Vector2(0, 0.5f);
            trophyRect.anchorMax = new Vector2(0, 0.5f);
            trophyRect.sizeDelta = new Vector2(70, 70);
            trophyRect.anchoredPosition = new Vector2(60, -10);

            Image trophyImg = trophyIcon.AddComponent<Image>();
            if (guiAssets != null && guiAssets.iconStar != null)
            {
                trophyImg.sprite = guiAssets.iconStar;
                trophyImg.color = new Color(1f, 0.85f, 0.3f);
            }
            else
            {
                trophyImg.color = new Color(1f, 0.84f, 0f);
            }

            // Animate trophy with gentle bob
            trophyIcon.transform.DOLocalMoveY(trophyRect.anchoredPosition.y + 5f, 1.2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            // Title text
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(titleRibbonObj.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "LEADERBOARDS";
            titleText.fontSize = UIDesignSystem.FontSizeHeader;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = UIDesignSystem.TextPrimary;
            titleText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(titleText);

            // Add glow behind ribbon
            GameObject glowObj = new GameObject("RibbonGlow");
            glowObj.transform.SetParent(header.transform, false);
            glowObj.transform.SetAsFirstSibling();

            RectTransform glowRect = glowObj.AddComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.5f, 0.5f);
            glowRect.anchorMax = new Vector2(0.5f, 0.5f);
            glowRect.sizeDelta = new Vector2(550, 120);
            glowRect.anchoredPosition = new Vector2(0, -10);

            Image glowImg = glowObj.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.8f, 0.3f, 0.3f);

            // Animate glow
            glowImg.DOFade(0.15f, 1.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        private void CreateTabs()
        {
            GameObject tabsSection = new GameObject("TabsSection");
            tabsSection.transform.SetParent(mainPanel.transform, false);

            RectTransform sectionRect = tabsSection.AddComponent<RectTransform>();
            sectionRect.anchorMin = new Vector2(0, 1);
            sectionRect.anchorMax = new Vector2(1, 1);
            sectionRect.pivot = new Vector2(0.5f, 1);
            sectionRect.sizeDelta = new Vector2(0, 85);
            sectionRect.anchoredPosition = new Vector2(0, -110);

            // Background frame for tabs section
            Image sectionBg = tabsSection.AddComponent<Image>();
            if (guiAssets != null && guiAssets.horizontalFrame != null)
            {
                sectionBg.sprite = guiAssets.horizontalFrame;
                sectionBg.type = Image.Type.Sliced;
                sectionBg.color = new Color(1f, 1f, 1f, 0.8f);
            }
            else
            {
                sectionBg.color = new Color(0.1f, 0.1f, 0.15f, 0.5f);
            }

            // Horizontal scroll for tabs
            GameObject scrollView = new GameObject("TabsScroll");
            scrollView.transform.SetParent(tabsSection.transform, false);

            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(15, 8);
            scrollRect.offsetMax = new Vector2(-15, -8);

            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.scrollSensitivity = 30f;

            // Tabs container
            tabsContainer = new GameObject("TabsContent");
            tabsContainer.transform.SetParent(scrollView.transform, false);

            RectTransform contentRect = tabsContainer.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(0, 1);
            contentRect.pivot = new Vector2(0, 0.5f);
            contentRect.anchoredPosition = Vector2.zero;

            HorizontalLayoutGroup layout = tabsContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = (int)UIDesignSystem.SpacingM;
            layout.padding = new RectOffset(8, 8, 5, 5);
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            layout.childControlWidth = false;
            layout.childControlHeight = true;

            ContentSizeFitter fitter = tabsContainer.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRect;

            // Create tabs for each leaderboard type
            foreach (LeaderboardType type in Enum.GetValues(typeof(LeaderboardType)))
            {
                CreateTab(type);
            }
        }

        private void CreateTab(LeaderboardType type)
        {
            GameObject tabObj = new GameObject($"Tab_{type}");
            tabObj.transform.SetParent(tabsContainer.transform, false);

            RectTransform tabRect = tabObj.AddComponent<RectTransform>();
            tabRect.sizeDelta = new Vector2(180, 0);

            Image tabBg = tabObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonBlue != null)
            {
                tabBg.sprite = guiAssets.buttonBlue;
                tabBg.type = Image.Type.Sliced;
                tabBg.color = new Color(0.7f, 0.7f, 0.8f); // Dimmed when not selected
            }
            else
            {
                tabBg.color = new Color(0.15f, 0.15f, 0.2f);
            }

            Button tabBtn = tabObj.AddComponent<Button>();
            tabBtn.targetGraphic = tabBg;
            LeaderboardType capturedType = type;
            tabBtn.onClick.AddListener(() => SelectTab(capturedType));

            // Button colors
            var colors = tabBtn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.2f);
            colors.pressedColor = new Color(0.9f, 0.9f, 1f);
            tabBtn.colors = colors;

            tabButtons[type] = tabBtn;

            // Tab text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(tabObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8, 0);
            textRect.offsetMax = new Vector2(-8, 0);

            TextMeshProUGUI tabText = textObj.AddComponent<TextMeshProUGUI>();
            tabText.text = LeaderboardManager.GetLeaderboardDisplayName(type);
            tabText.fontSize = UIDesignSystem.FontSizeSmall;
            tabText.fontStyle = FontStyles.Bold;
            tabText.color = Color.white;
            tabText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(tabText);

            LayoutElement le = tabObj.AddComponent<LayoutElement>();
            le.preferredWidth = 180;
        }

        private void CreateEntriesSection()
        {
            GameObject entriesSection = new GameObject("EntriesSection");
            entriesSection.transform.SetParent(mainPanel.transform, false);

            RectTransform sectionRect = entriesSection.AddComponent<RectTransform>();
            sectionRect.anchorMin = new Vector2(0, 0);
            sectionRect.anchorMax = new Vector2(1, 1);
            sectionRect.offsetMin = new Vector2(25, 130);  // Leave 130px for player rank section
            sectionRect.offsetMax = new Vector2(-25, -210);  // Leave 210px for header + tabs

            // Frame background for entries section
            Image sectionBg = entriesSection.AddComponent<Image>();
            if (guiAssets != null && guiAssets.listFrame != null)
            {
                sectionBg.sprite = guiAssets.listFrame;
                sectionBg.type = Image.Type.Sliced;
                sectionBg.color = Color.white;
            }
            else
            {
                sectionBg.color = new Color(0.05f, 0.05f, 0.08f, 0.7f);
            }

            // Scroll view
            GameObject scrollViewObj = new GameObject("EntriesScroll");
            scrollViewObj.transform.SetParent(entriesSection.transform, false);

            RectTransform scrollViewRect = scrollViewObj.AddComponent<RectTransform>();
            scrollViewRect.anchorMin = Vector2.zero;
            scrollViewRect.anchorMax = Vector2.one;
            scrollViewRect.offsetMin = new Vector2(10, 10);
            scrollViewRect.offsetMax = new Vector2(-10, -10);

            // Add Image for Mask (required for Mask component)
            Image scrollBg = scrollViewObj.AddComponent<Image>();
            scrollBg.color = new Color(0.03f, 0.03f, 0.05f, 0.8f);

            // Single Mask on the scroll view - this clips the content
            scrollViewObj.AddComponent<Mask>().showMaskGraphic = true;

            ScrollRect scroll = scrollViewObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 30f;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            this.scrollRect = scroll;

            // Content container (direct child of scroll view, no separate viewport needed)
            entriesContainer = new GameObject("EntriesContent");
            entriesContainer.transform.SetParent(scrollViewObj.transform, false);

            RectTransform contentRect = entriesContainer.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            contentRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layout = entriesContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;  // Let layout control height based on LayoutElement
            layout.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter fitter = entriesContainer.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Set scroll rect references
            scroll.content = contentRect;
            scroll.viewport = scrollViewRect;  // The scroll view itself acts as viewport
        }

        private void CreatePlayerRankDisplay()
        {
            GameObject rankSection = new GameObject("PlayerRankSection");
            rankSection.transform.SetParent(mainPanel.transform, false);

            RectTransform sectionRect = rankSection.AddComponent<RectTransform>();
            sectionRect.anchorMin = new Vector2(0, 0);
            sectionRect.anchorMax = new Vector2(1, 0);
            sectionRect.pivot = new Vector2(0.5f, 0);
            sectionRect.sizeDelta = new Vector2(0, 110);
            sectionRect.anchoredPosition = Vector2.zero;

            // Background frame
            Image sectionBg = rankSection.AddComponent<Image>();
            if (guiAssets != null && guiAssets.horizontalFrame != null)
            {
                sectionBg.sprite = guiAssets.horizontalFrame;
                sectionBg.type = Image.Type.Sliced;
                sectionBg.color = new Color(0.4f, 0.9f, 1f, 0.9f); // Cyan tint for player highlight
            }
            else
            {
                sectionBg.color = new Color(0.15f, 0.25f, 0.35f);
            }

            // Glow behind player rank
            GameObject glowObj = new GameObject("PlayerGlow");
            glowObj.transform.SetParent(rankSection.transform, false);
            glowObj.transform.SetAsFirstSibling();

            RectTransform glowRect = glowObj.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.offsetMin = new Vector2(-15, -10);
            glowRect.offsetMax = new Vector2(15, 10);

            playerRowGlow = glowObj.AddComponent<Image>();
            playerRowGlow.color = new Color(0.3f, 0.9f, 1f, 0.4f);

            // Animate the glow
            playerRowGlow.DOFade(0.2f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

            // Star icon on left
            GameObject starIcon = new GameObject("StarIcon");
            starIcon.transform.SetParent(rankSection.transform, false);

            RectTransform starRect = starIcon.AddComponent<RectTransform>();
            starRect.anchorMin = new Vector2(0, 0.5f);
            starRect.anchorMax = new Vector2(0, 0.5f);
            starRect.sizeDelta = new Vector2(50, 50);
            starRect.anchoredPosition = new Vector2(45, 0);

            Image starImg = starIcon.AddComponent<Image>();
            if (guiAssets != null && guiAssets.iconStar != null)
            {
                starImg.sprite = guiAssets.iconStar;
                starImg.color = new Color(1f, 0.9f, 0.4f);
            }
            else
            {
                starImg.color = new Color(0.3f, 0.9f, 1f);
            }

            // Rotate star
            starIcon.transform.DORotate(new Vector3(0, 0, 360), 8f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);

            // Player rank text
            GameObject rankObj = new GameObject("PlayerRank");
            rankObj.transform.SetParent(rankSection.transform, false);

            RectTransform rankRect = rankObj.AddComponent<RectTransform>();
            rankRect.anchorMin = Vector2.zero;
            rankRect.anchorMax = Vector2.one;
            rankRect.offsetMin = new Vector2(90, 10);
            rankRect.offsetMax = new Vector2(-30, -10);

            playerRankText = rankObj.AddComponent<TextMeshProUGUI>();
            playerRankText.text = "Your Rank: #--";
            playerRankText.fontSize = UIDesignSystem.FontSizeTitle;
            playerRankText.fontStyle = FontStyles.Bold;
            playerRankText.color = UIDesignSystem.TextPrimary;
            playerRankText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(playerRankText);
        }

        private void CreateCloseButton()
        {
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(mainPanel.transform, false);

            RectTransform closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 1);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.sizeDelta = new Vector2(75, 75);
            closeRect.anchoredPosition = new Vector2(-15, -15);

            Image closeImage = closeObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonRed != null)
            {
                closeImage.sprite = guiAssets.buttonRed;
                closeImage.type = Image.Type.Sliced;
                closeImage.color = Color.white;
            }
            else
            {
                closeImage.color = new Color(0.8f, 0.3f, 0.3f);
            }

            closeButton = closeObj.AddComponent<Button>();
            closeButton.targetGraphic = closeImage;
            closeButton.onClick.AddListener(Hide);

            // Button colors
            var colors = closeButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            closeButton.colors = colors;

            // Close icon
            GameObject iconObj = new GameObject("CloseIcon");
            iconObj.transform.SetParent(closeObj.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(18, 18);
            iconRect.offsetMax = new Vector2(-18, -18);

            Image iconImg = iconObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.iconClose != null)
            {
                iconImg.sprite = guiAssets.iconClose;
                iconImg.color = Color.white;
            }
            else
            {
                // Fallback: Add X text
                GameObject closeText = new GameObject("X");
                closeText.transform.SetParent(closeObj.transform, false);

                RectTransform xRect = closeText.AddComponent<RectTransform>();
                xRect.anchorMin = Vector2.zero;
                xRect.anchorMax = Vector2.one;
                xRect.offsetMin = Vector2.zero;
                xRect.offsetMax = Vector2.zero;

                TextMeshProUGUI xText = closeText.AddComponent<TextMeshProUGUI>();
                xText.text = "X";
                xText.fontSize = 40;
                xText.fontStyle = FontStyles.Bold;
                xText.color = Color.white;
                xText.alignment = TextAlignmentOptions.Center;
                ApplySharedFont(xText);
            }
        }

        #region Event Handlers

        private void OnLeaderboardUpdated(LeaderboardType type, Leaderboard lb)
        {
            if (type == currentType)
            {
                RefreshEntriesWithFakePlayers();
            }
        }

        private void OnPlayerRankChanged(LeaderboardType type, int newRank)
        {
            if (type == currentType)
            {
                UpdatePlayerRankDisplay();

                // Animate rank text
                playerRankText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 2);
            }
        }

        #endregion

        #region Tab Selection

        private void SelectTab(LeaderboardType type)
        {
            currentType = type;

            // Update tab visuals
            foreach (var kvp in tabButtons)
            {
                Image tabBg = kvp.Value.GetComponent<Image>();
                if (kvp.Key == type)
                {
                    // Selected tab: bright and full color
                    tabBg.color = Color.white;
                    kvp.Value.transform.DOScale(1.05f, 0.15f).SetEase(Ease.OutBack);
                }
                else
                {
                    // Unselected: dimmed
                    tabBg.color = new Color(0.6f, 0.6f, 0.7f);
                    kvp.Value.transform.DOScale(1f, 0.1f);
                }
            }

            // Reinitialize fake players for this category (different scores per category)
            InitializeFakePlayersForCategory(type);

            RefreshEntriesWithFakePlayers();
            UpdatePlayerRankDisplay();

            AudioManager.Instance?.PlayButtonClickSound();
        }

        #endregion

        #region UI Updates

        private void UpdatePlayerRankDisplay()
        {
            if (LeaderboardManager.Instance == null) return;

            var entry = LeaderboardManager.Instance.GetPlayerEntry(currentType);
            if (entry != null)
            {
                playerRankText.text = $"Your Rank: #{entry.rank} | Score: {FormatScore(entry.score)}";
            }
            else
            {
                playerRankText.text = "Your Rank: Not ranked yet";
            }
        }

        private string FormatScore(double score)
        {
            if (score >= 1000000000) return $"{score / 1000000000:F2}B";
            if (score >= 1000000) return $"{score / 1000000:F2}M";
            if (score >= 1000) return $"{score / 1000:F1}K";
            return score.ToString("N0");
        }

        #endregion

        #region Show/Hide

        public void Show()
        {
            panel.SetActive(true);
            isVisible = true;

            // Ensure popup is rendered on top of other UI elements (like menu button)
            panel.transform.SetAsLastSibling();

            // Always reinitialize fake players when showing to ensure fresh data
            InitializeFakePlayers();
            Debug.Log($"[LeaderboardUI] Initialized {fakePlayers.Count} fake players");

            // Select first tab (which will refresh entries)
            SelectTab(LeaderboardType.LifetimeMoney);

            // Animate in using UIDesignSystem timing
            panelCanvasGroup.alpha = 0;
            mainPanel.transform.localScale = Vector3.one * 0.9f;
            panelCanvasGroup.DOFade(1, 0.25f);
            mainPanel.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack);

            // Animate title ribbon
            if (titleRibbonObj != null)
            {
                titleRibbonObj.transform.localScale = Vector3.zero;
                titleRibbonObj.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetDelay(0.1f);
            }

            // Apply button polish for press/release animations
            if (UI.UIPolishManager.Instance != null)
            {
                UI.UIPolishManager.Instance.PolishButtonsInPanel(panel);
            }

            // Register with PopupManager
            if (PopupManager.Instance != null)
                PopupManager.Instance.RegisterPopupOpen("LeaderboardUI");

            AudioManager.Instance?.PlayButtonClickSound();
        }

        public void Hide()
        {
            // Unregister from PopupManager
            if (PopupManager.Instance != null)
                PopupManager.Instance.RegisterPopupClosed("LeaderboardUI");

            panelCanvasGroup.DOFade(0, 0.15f);
            mainPanel.transform.DOScale(0.9f, 0.15f)
                .OnComplete(() =>
                {
                    panel.SetActive(false);
                    isVisible = false;
                });

            AudioManager.Instance?.PlayButtonClickSound();
        }

        public void Toggle()
        {
            if (isVisible) Hide();
            else Show();
        }

        public bool IsVisible => isVisible;

        #endregion

        #region Helpers

        private void ApplySharedFont(TextMeshProUGUI text)
        {
            if (text == null) return;

            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                text.font = GameUI.Instance.SharedFont;
            }

            // Add outline effect using TMP's built-in properties (safer than modifying material directly)
            try
            {
                if (text.fontMaterial != null)
                {
                    text.outlineWidth = 0.15f;
                    text.outlineColor = new Color32(0, 0, 0, 180);
                }
            }
            catch (System.NullReferenceException) { }
        }

        private void InitializeFakePlayers()
        {
            // Default initialization for first tab
            InitializeFakePlayersForCategory(LeaderboardType.LifetimeMoney);
        }

        private void InitializeFakePlayersForCategory(LeaderboardType type)
        {
            fakePlayers.Clear();

            // Generate random suffixes for player names
            string[] suffixes = { "Pro", "Elite", "xX", "Xx", "99", "42", "777", "69", "2K", "HD" };
            string[] prefixes = { "", "xX", "The", "Sir", "Pro", "Mr", "Dark", "Ice", "Fire", "Neo" };

            // Determine score ranges based on category type
            double topScoreMin, topScoreMax;
            switch (type)
            {
                case LeaderboardType.LifetimeMoney:
                    topScoreMin = 50000000;  // 50M
                    topScoreMax = 500000000; // 500M
                    break;
                case LeaderboardType.LifetimeDarkMatter:
                    topScoreMin = 5000;
                    topScoreMax = 50000;
                    break;
                case LeaderboardType.LifetimeTimeShards:
                    topScoreMin = 1000;
                    topScoreMax = 20000;
                    break;
                case LeaderboardType.TotalDiceRolls:
                    topScoreMin = 100000;
                    topScoreMax = 1000000;
                    break;
                case LeaderboardType.HighestFractureLevel:
                    topScoreMin = 10;
                    topScoreMax = 100;
                    break;
                case LeaderboardType.TotalJackpots:
                    topScoreMin = 500;
                    topScoreMax = 5000;
                    break;
                case LeaderboardType.LongestLoginStreak:
                    topScoreMin = 30;
                    topScoreMax = 365;
                    break;
                case LeaderboardType.MissionsCompleted:
                    topScoreMin = 50;
                    topScoreMax = 500;
                    break;
                default:
                    topScoreMin = 1000000;
                    topScoreMax = 10000000;
                    break;
            }

            // Use a seed based on category to get consistent but different names per category
            int categorySeed = (int)type * 12345;
            UnityEngine.Random.InitState(categorySeed + System.DateTime.Now.Second);

            double topScore = UnityEngine.Random.Range((float)topScoreMin, (float)topScoreMax);

            for (int i = 0; i < 30; i++)
            {
                // Mix up the names with prefixes/suffixes for variety
                string baseName = fakePlayerNames[(i + (int)type * 7) % fakePlayerNames.Length];
                string name;

                int nameVariant = UnityEngine.Random.Range(0, 4);
                switch (nameVariant)
                {
                    case 0:
                        name = baseName + UnityEngine.Random.Range(1, 999).ToString();
                        break;
                    case 1:
                        name = prefixes[UnityEngine.Random.Range(0, prefixes.Length)] + baseName;
                        break;
                    case 2:
                        name = baseName + suffixes[UnityEngine.Random.Range(0, suffixes.Length)];
                        break;
                    default:
                        name = baseName;
                        break;
                }

                // Create exponential score curve with randomness for realistic distribution
                double rankFactor = Math.Pow(0.82, i);
                double randomFactor = UnityEngine.Random.Range(0.7f, 1.3f);
                double score = topScore * rankFactor * randomFactor;

                // Ensure minimum score based on category
                double minScore = topScoreMin * 0.01;
                score = Math.Max(score, minScore + UnityEngine.Random.Range(0, (float)(minScore * 0.5)));

                fakePlayers.Add(new FakePlayer
                {
                    name = name,
                    score = score,
                    rank = i + 1
                });
            }

            // Reset random state
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);

            // Sort by score descending
            fakePlayers.Sort((a, b) => b.score.CompareTo(a.score));

            // Assign ranks
            for (int i = 0; i < fakePlayers.Count; i++)
            {
                fakePlayers[i].rank = i + 1;
            }

            Debug.Log($"[LeaderboardUI] Initialized {fakePlayers.Count} fake players for {type}, top score: {FormatScore(topScore)}");
        }

        private void UpdateFakePlayerScores()
        {
            if (fakePlayers.Count == 0) return;

            // Pick 1-3 random players to update
            int updateCount = UnityEngine.Random.Range(1, 4);
            for (int i = 0; i < updateCount; i++)
            {
                int index = UnityEngine.Random.Range(0, fakePlayers.Count);
                var player = fakePlayers[index];

                // Increase their score by a random amount
                double increase = player.score * UnityEngine.Random.Range(0.01f, 0.05f);
                player.score += increase;

                // Update the UI if this player's row exists
                if (player.scoreText != null)
                {
                    player.scoreText.text = FormatScore(player.score);
                    player.scoreText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 2);
                }
            }

            // Re-sort and update ranks
            fakePlayers.Sort((a, b) => b.score.CompareTo(a.score));
            for (int i = 0; i < fakePlayers.Count; i++)
            {
                fakePlayers[i].rank = i + 1;
            }

            // Refresh if visible
            if (isVisible)
            {
                RefreshEntriesWithFakePlayers();
            }
        }

        private void RefreshEntriesWithFakePlayers()
        {
            // Clear existing entries
            foreach (Transform child in entriesContainer.transform)
            {
                Destroy(child.gameObject);
            }

            // Ensure fake players exist
            if (fakePlayers.Count == 0)
            {
                InitializeFakePlayers();
            }

            Debug.Log($"[LeaderboardUI] RefreshEntriesWithFakePlayers - {fakePlayers.Count} fake players");

            // Get player's current score
            double playerScore = 0;
            string playerName = "You";
            if (LeaderboardManager.Instance != null)
            {
                var playerEntry = LeaderboardManager.Instance.GetPlayerEntry(currentType);
                if (playerEntry != null)
                {
                    playerScore = playerEntry.score;
                    playerName = playerEntry.playerName;
                }
            }

            // Find where player would rank among fake players
            int playerRank = 1;
            for (int i = 0; i < fakePlayers.Count; i++)
            {
                if (playerScore < fakePlayers[i].score)
                {
                    playerRank = i + 2;
                }
            }

            // Create entries - show top 20 + around player if lower
            int displayCount = Math.Min(20, fakePlayers.Count);
            bool playerShown = false;

            Debug.Log($"[LeaderboardUI] Displaying {displayCount} entries, player rank: {playerRank}");

            for (int i = 0; i < displayCount; i++)
            {
                var fakePlayer = fakePlayers[i];

                // Check if player should be inserted here (before this fake player)
                if (!playerShown && playerRank <= fakePlayer.rank)
                {
                    CreateEntryRowTMP("You", playerScore, playerRank, true);
                    playerShown = true;
                }

                CreateFakePlayerRow(fakePlayer, i);
            }

            // If player isn't in top 20, show separator and player
            if (!playerShown)
            {
                CreateSeparatorRowTMP();
                CreateEntryRowTMP(playerName, playerScore, playerRank, true);
            }

            // Force layout rebuild
            Canvas.ForceUpdateCanvases();
            if (entriesContainer != null)
            {
                RectTransform containerRect = entriesContainer.GetComponent<RectTransform>();
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);

                // Debug: Log child count and content size
                Debug.Log($"[LeaderboardUI] entriesContainer has {entriesContainer.transform.childCount} children, size: {containerRect.sizeDelta}");

                // Log each child's size for debugging
                for (int i = 0; i < Mathf.Min(3, entriesContainer.transform.childCount); i++)
                {
                    Transform child = entriesContainer.transform.GetChild(i);
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    if (childRect != null)
                    {
                        Debug.Log($"[LeaderboardUI] Child {i} '{child.name}': sizeDelta={childRect.sizeDelta}, anchoredPos={childRect.anchoredPosition}, scale={child.localScale}");
                    }
                }
            }

            // Also rebuild scroll rect
            if (scrollRect != null && scrollRect.content != null)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
                Debug.Log($"[LeaderboardUI] scrollRect.content size: {scrollRect.content.sizeDelta}");
            }
        }

        private void CreateFakePlayerRow(FakePlayer player, int index)
        {
            GameObject row = new GameObject($"Entry_{player.rank}");
            row.transform.SetParent(entriesContainer.transform, false);

            float rowHeight = player.rank <= 3 ? 70 : 58;

            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0, rowHeight);

            // Add LayoutElement for proper sizing in VerticalLayoutGroup
            LayoutElement layoutElement = row.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = rowHeight;
            layoutElement.minHeight = rowHeight;
            layoutElement.flexibleWidth = 1;  // Allow flexible width

            Image rowBg = row.AddComponent<Image>();

            // Podium styling for top 3
            if (player.rank <= 3)
            {
                Color[] podiumColors = {
                    new Color(1f, 0.85f, 0.2f, 1f),   // Gold
                    new Color(0.85f, 0.88f, 0.95f, 1f), // Silver
                    new Color(0.85f, 0.55f, 0.25f, 1f)  // Bronze
                };

                if (guiAssets != null && guiAssets.cardFrame != null)
                {
                    rowBg.sprite = guiAssets.cardFrame;
                    rowBg.type = Image.Type.Sliced;
                    rowBg.color = podiumColors[player.rank - 1];
                }
                else
                {
                    rowBg.color = podiumColors[player.rank - 1];
                }
            }
            else
            {
                if (guiAssets != null && guiAssets.horizontalFrame != null)
                {
                    rowBg.sprite = guiAssets.horizontalFrame;
                    rowBg.type = Image.Type.Sliced;
                    rowBg.color = new Color(0.5f, 0.5f, 0.6f, 0.7f);
                }
                else
                {
                    rowBg.color = new Color(0.15f, 0.15f, 0.2f, 0.7f);
                }
            }

            // Medal/trophy icon for podium positions
            if (player.rank <= 3)
            {
                GameObject medalObj = new GameObject("Medal");
                medalObj.transform.SetParent(row.transform, false);

                RectTransform medalRect = medalObj.AddComponent<RectTransform>();
                medalRect.anchorMin = new Vector2(0, 0.5f);
                medalRect.anchorMax = new Vector2(0, 0.5f);
                medalRect.sizeDelta = new Vector2(40, 40);
                medalRect.anchoredPosition = new Vector2(35, 0);

                Image medalImg = medalObj.AddComponent<Image>();
                if (guiAssets != null && guiAssets.iconStar != null)
                {
                    medalImg.sprite = guiAssets.iconStar;
                    Color[] medalColors = {
                        new Color(1f, 0.9f, 0.3f),  // Gold
                        new Color(0.9f, 0.9f, 1f),  // Silver
                        new Color(0.9f, 0.6f, 0.3f) // Bronze
                    };
                    medalImg.color = medalColors[player.rank - 1];
                }
                else
                {
                    medalImg.color = Color.yellow;
                }
            }

            // Rank
            GameObject rankObj = new GameObject("Rank");
            rankObj.transform.SetParent(row.transform, false);

            RectTransform rankRect = rankObj.AddComponent<RectTransform>();
            rankRect.anchorMin = new Vector2(0, 0);
            rankRect.anchorMax = new Vector2(0.14f, 1);
            rankRect.offsetMin = new Vector2(player.rank <= 3 ? 55 : 12, 0);
            rankRect.offsetMax = Vector2.zero;

            TextMeshProUGUI rankText = rankObj.AddComponent<TextMeshProUGUI>();
            rankText.text = $"#{player.rank}";
            rankText.fontSize = player.rank <= 3 ? UIDesignSystem.FontSizeBody + 4 : UIDesignSystem.FontSizeBody;
            rankText.fontStyle = player.rank <= 3 ? FontStyles.Bold : FontStyles.Normal;

            if (player.rank == 1) rankText.color = new Color(1f, 0.85f, 0.2f); // Bright gold text
            else if (player.rank == 2) rankText.color = new Color(0.85f, 0.9f, 1f); // Bright silver text
            else if (player.rank == 3) rankText.color = new Color(1f, 0.65f, 0.3f); // Bright bronze text
            else rankText.color = Color.white;

            rankText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(rankText);

            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(row.transform, false);

            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.14f, 0);
            nameRect.anchorMax = new Vector2(0.55f, 1);
            nameRect.offsetMin = new Vector2(10, 0);
            nameRect.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = player.name;
            nameText.fontSize = player.rank <= 3 ? UIDesignSystem.FontSizeBody + 2 : UIDesignSystem.FontSizeBody;
            nameText.fontStyle = player.rank <= 3 ? FontStyles.Bold : FontStyles.Normal;
            // Use bright colors for top 3 names
            if (player.rank == 1) nameText.color = new Color(1f, 0.9f, 0.4f); // Bright gold
            else if (player.rank == 2) nameText.color = new Color(0.9f, 0.95f, 1f); // Bright silver
            else if (player.rank == 3) nameText.color = new Color(1f, 0.7f, 0.4f); // Bright bronze
            else nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Left;
            ApplySharedFont(nameText);

            // Score
            GameObject scoreObj = new GameObject("Score");
            scoreObj.transform.SetParent(row.transform, false);

            RectTransform scoreRect = scoreObj.AddComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.55f, 0);
            scoreRect.anchorMax = new Vector2(1, 1);
            scoreRect.offsetMin = new Vector2(10, 0);
            scoreRect.offsetMax = new Vector2(-15, 0);

            TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreText.text = FormatScore(player.score);
            scoreText.fontSize = player.rank <= 3 ? UIDesignSystem.FontSizeBody + 2 : UIDesignSystem.FontSizeBody;
            scoreText.fontStyle = FontStyles.Bold;
            // Use bright colors for top 3 scores
            if (player.rank == 1) scoreText.color = new Color(1f, 0.85f, 0.2f); // Bright gold
            else if (player.rank == 2) scoreText.color = new Color(0.85f, 0.9f, 1f); // Bright silver
            else if (player.rank == 3) scoreText.color = new Color(1f, 0.65f, 0.3f); // Bright bronze
            else scoreText.color = UIDesignSystem.AccentGold;
            scoreText.alignment = TextAlignmentOptions.Right;
            ApplySharedFont(scoreText);

            // Store reference for dynamic updates
            player.scoreText = scoreText;
            player.rowObject = row;

            // Set initial scale to visible (animation disabled for debugging)
            row.transform.localScale = Vector3.one;
        }

        private void CreateEntryRowTMP(string name, double score, int rank, bool isPlayer)
        {
            GameObject row = new GameObject($"PlayerEntry");
            row.transform.SetParent(entriesContainer.transform, false);

            float rowHeight = 80;

            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0, rowHeight);

            // Add LayoutElement for proper sizing in VerticalLayoutGroup
            LayoutElement layoutElement = row.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = rowHeight;
            layoutElement.minHeight = rowHeight;
            layoutElement.flexibleWidth = 1;  // Allow flexible width

            // Glow behind player row for emphasis
            GameObject glowObj = new GameObject("PlayerRowGlow");
            glowObj.transform.SetParent(row.transform, false);
            glowObj.transform.SetAsFirstSibling();

            RectTransform glowRect = glowObj.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.offsetMin = new Vector2(-8, -5);
            glowRect.offsetMax = new Vector2(8, 5);

            Image glowImg = glowObj.AddComponent<Image>();
            glowImg.color = new Color(0.3f, 0.9f, 1f, 0.5f);
            glowImg.DOFade(0.25f, 0.8f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

            // Row background
            Image rowBg = row.AddComponent<Image>();
            if (guiAssets != null && guiAssets.cardFrame != null)
            {
                rowBg.sprite = guiAssets.cardFrame;
                rowBg.type = Image.Type.Sliced;
                rowBg.color = new Color(0.3f, 0.85f, 1f, 1f); // Bright cyan
            }
            else
            {
                rowBg.color = new Color(0.2f, 0.6f, 0.9f, 0.95f);
            }

            // Star icon on left for player
            GameObject starObj = new GameObject("PlayerStar");
            starObj.transform.SetParent(row.transform, false);

            RectTransform starRect = starObj.AddComponent<RectTransform>();
            starRect.anchorMin = new Vector2(0, 0.5f);
            starRect.anchorMax = new Vector2(0, 0.5f);
            starRect.sizeDelta = new Vector2(45, 45);
            starRect.anchoredPosition = new Vector2(35, 0);

            Image starImg = starObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.iconStar != null)
            {
                starImg.sprite = guiAssets.iconStar;
                starImg.color = new Color(1f, 0.95f, 0.4f);
            }
            else
            {
                starImg.color = new Color(1f, 0.9f, 0.3f);
            }

            // Rotate star
            starObj.transform.DORotate(new Vector3(0, 0, -360), 6f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);

            // Rank
            GameObject rankObj = new GameObject("Rank");
            rankObj.transform.SetParent(row.transform, false);

            RectTransform rankRect = rankObj.AddComponent<RectTransform>();
            rankRect.anchorMin = new Vector2(0, 0);
            rankRect.anchorMax = new Vector2(0.14f, 1);
            rankRect.offsetMin = new Vector2(60, 0);
            rankRect.offsetMax = Vector2.zero;

            TextMeshProUGUI rankText = rankObj.AddComponent<TextMeshProUGUI>();
            rankText.text = $"#{rank}";
            rankText.fontSize = UIDesignSystem.FontSizeBody + 4;
            rankText.fontStyle = FontStyles.Bold;
            rankText.color = UIDesignSystem.TextPrimary;
            rankText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(rankText);

            // Name with "(YOU)" badge
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(row.transform, false);

            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.14f, 0);
            nameRect.anchorMax = new Vector2(0.55f, 1);
            nameRect.offsetMin = new Vector2(12, 0);
            nameRect.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = $"<color=#FFFFFF>{name}</color> <color=#FFE066><size=80%>(YOU)</size></color>";
            nameText.fontSize = UIDesignSystem.FontSizeBody + 2;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = UIDesignSystem.TextPrimary;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.richText = true;
            ApplySharedFont(nameText);

            // Score
            GameObject scoreObj = new GameObject("Score");
            scoreObj.transform.SetParent(row.transform, false);

            RectTransform scoreRect = scoreObj.AddComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.55f, 0);
            scoreRect.anchorMax = new Vector2(1, 1);
            scoreRect.offsetMin = new Vector2(10, 0);
            scoreRect.offsetMax = new Vector2(-15, 0);

            TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreText.text = FormatScore(score);
            scoreText.fontSize = UIDesignSystem.FontSizeBody + 2;
            scoreText.fontStyle = FontStyles.Bold;
            scoreText.color = UIDesignSystem.TextPrimary;
            scoreText.alignment = TextAlignmentOptions.Right;
            ApplySharedFont(scoreText);

            // Set initial scale to visible (animation disabled for debugging)
            row.transform.localScale = Vector3.one;
        }

        private void CreateSeparatorRowTMP()
        {
            GameObject separator = new GameObject("Separator");
            separator.transform.SetParent(entriesContainer.transform, false);

            float separatorHeight = 50;

            RectTransform sepRect = separator.AddComponent<RectTransform>();
            sepRect.sizeDelta = new Vector2(0, separatorHeight);

            // Add LayoutElement for proper sizing in VerticalLayoutGroup
            LayoutElement layoutElement = separator.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = separatorHeight;
            layoutElement.minHeight = separatorHeight;
            layoutElement.flexibleWidth = 1;  // Allow flexible width

            // Add horizontal line left
            GameObject lineLeft = new GameObject("LineLeft");
            lineLeft.transform.SetParent(separator.transform, false);

            RectTransform lineLeftRect = lineLeft.AddComponent<RectTransform>();
            lineLeftRect.anchorMin = new Vector2(0.1f, 0.5f);
            lineLeftRect.anchorMax = new Vector2(0.4f, 0.5f);
            lineLeftRect.sizeDelta = new Vector2(0, 3);

            Image lineLeftImg = lineLeft.AddComponent<Image>();
            lineLeftImg.color = new Color(0.5f, 0.5f, 0.6f, 0.5f);

            // Dots in center
            TextMeshProUGUI sepText = separator.AddComponent<TextMeshProUGUI>();
            sepText.text = "• • •";
            sepText.fontSize = UIDesignSystem.FontSizeBody;
            sepText.color = new Color(0.6f, 0.6f, 0.7f);
            sepText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(sepText);

            // Add horizontal line right
            GameObject lineRight = new GameObject("LineRight");
            lineRight.transform.SetParent(separator.transform, false);

            RectTransform lineRightRect = lineRight.AddComponent<RectTransform>();
            lineRightRect.anchorMin = new Vector2(0.6f, 0.5f);
            lineRightRect.anchorMax = new Vector2(0.9f, 0.5f);
            lineRightRect.sizeDelta = new Vector2(0, 3);

            Image lineRightImg = lineRight.AddComponent<Image>();
            lineRightImg.color = new Color(0.5f, 0.5f, 0.6f, 0.5f);
        }

        #endregion
    }
}
