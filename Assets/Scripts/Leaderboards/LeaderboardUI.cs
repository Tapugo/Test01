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
            panelBg.color = new Color(0, 0, 0, 0.9f);

            panelCanvasGroup = panel.AddComponent<CanvasGroup>();

            // Main content panel - fullscreen with padding
            mainPanel = new GameObject("MainPanel");
            mainPanel.transform.SetParent(panel.transform, false);

            RectTransform mainRect = mainPanel.AddComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.02f, 0.02f);
            mainRect.anchorMax = new Vector2(0.98f, 0.98f);
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;

            Image mainBg = mainPanel.AddComponent<Image>();
            mainBg.color = new Color(0.06f, 0.05f, 0.1f, 0.98f);

            // Add outline for polish
            Outline outline = mainPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.8f, 0.6f, 0.2f, 0.6f);
            outline.effectDistance = new Vector2(3, -3);

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
            headerRect.sizeDelta = new Vector2(0, 100);
            headerRect.anchoredPosition = Vector2.zero;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(header.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(30, 0);
            titleRect.offsetMax = new Vector2(-100, 0);

            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "LEADERBOARDS";
            titleText.fontSize = 56;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = new Color(0.8f, 0.6f, 0.2f);
            titleText.alignment = TextAlignmentOptions.Left;
            ApplySharedFont(titleText);
        }

        private void CreateTabs()
        {
            GameObject tabsSection = new GameObject("TabsSection");
            tabsSection.transform.SetParent(mainPanel.transform, false);

            RectTransform sectionRect = tabsSection.AddComponent<RectTransform>();
            sectionRect.anchorMin = new Vector2(0, 1);
            sectionRect.anchorMax = new Vector2(1, 1);
            sectionRect.pivot = new Vector2(0.5f, 1);
            sectionRect.sizeDelta = new Vector2(0, 80);
            sectionRect.anchoredPosition = new Vector2(0, -100);

            // Horizontal scroll for tabs
            GameObject scrollView = new GameObject("TabsScroll");
            scrollView.transform.SetParent(tabsSection.transform, false);

            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(20, 5);
            scrollRect.offsetMax = new Vector2(-20, -5);

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
            layout.spacing = 15;
            layout.padding = new RectOffset(10, 10, 5, 5);
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
            tabBg.color = new Color(0.15f, 0.15f, 0.2f);

            Button tabBtn = tabObj.AddComponent<Button>();
            tabBtn.targetGraphic = tabBg;
            LeaderboardType capturedType = type;
            tabBtn.onClick.AddListener(() => SelectTab(capturedType));

            // Button colors
            var colors = tabBtn.colors;
            colors.highlightedColor = new Color(0.25f, 0.25f, 0.35f);
            colors.pressedColor = new Color(0.2f, 0.3f, 0.5f);
            tabBtn.colors = colors;

            tabButtons[type] = tabBtn;

            // Tab text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(tabObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI tabText = textObj.AddComponent<TextMeshProUGUI>();
            tabText.text = LeaderboardManager.GetLeaderboardDisplayName(type);
            tabText.fontSize = 20;
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
            sectionRect.offsetMin = new Vector2(30, 120);  // Leave 120px for player rank section
            sectionRect.offsetMax = new Vector2(-30, -220);  // Leave 220px for header (100px) + tabs (80px) + more padding

            // Scroll view
            GameObject scrollViewObj = new GameObject("EntriesScroll");
            scrollViewObj.transform.SetParent(entriesSection.transform, false);

            RectTransform scrollViewRect = scrollViewObj.AddComponent<RectTransform>();
            scrollViewRect.anchorMin = Vector2.zero;
            scrollViewRect.anchorMax = Vector2.one;
            scrollViewRect.offsetMin = Vector2.zero;
            scrollViewRect.offsetMax = Vector2.zero;

            ScrollRect scroll = scrollViewObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 30f;

            Image scrollBg = scrollViewObj.AddComponent<Image>();
            scrollBg.color = new Color(0.05f, 0.05f, 0.08f, 0.5f);

            scrollViewObj.AddComponent<Mask>().showMaskGraphic = true;

            this.scrollRect = scroll;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollViewObj.transform, false);

            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            scroll.viewport = viewportRect;

            // Content
            entriesContainer = new GameObject("EntriesContent");
            entriesContainer.transform.SetParent(viewport.transform, false);

            RectTransform contentRect = entriesContainer.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            contentRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layout = entriesContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4;
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            ContentSizeFitter fitter = entriesContainer.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRect;
        }

        private void CreatePlayerRankDisplay()
        {
            GameObject rankSection = new GameObject("PlayerRankSection");
            rankSection.transform.SetParent(mainPanel.transform, false);

            RectTransform sectionRect = rankSection.AddComponent<RectTransform>();
            sectionRect.anchorMin = new Vector2(0, 0);
            sectionRect.anchorMax = new Vector2(1, 0);
            sectionRect.pivot = new Vector2(0.5f, 0);
            sectionRect.sizeDelta = new Vector2(0, 100);
            sectionRect.anchoredPosition = Vector2.zero;

            Image sectionBg = rankSection.AddComponent<Image>();
            sectionBg.color = new Color(0.1f, 0.12f, 0.18f);

            // Player rank text
            GameObject rankObj = new GameObject("PlayerRank");
            rankObj.transform.SetParent(rankSection.transform, false);

            RectTransform rankRect = rankObj.AddComponent<RectTransform>();
            rankRect.anchorMin = Vector2.zero;
            rankRect.anchorMax = Vector2.one;
            rankRect.offsetMin = new Vector2(30, 15);
            rankRect.offsetMax = new Vector2(-30, -15);

            playerRankText = rankObj.AddComponent<TextMeshProUGUI>();
            playerRankText.text = "Your Rank: #--";
            playerRankText.fontSize = 36;
            playerRankText.fontStyle = FontStyles.Bold;
            playerRankText.color = new Color(0.3f, 0.9f, 1f);
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
            closeRect.sizeDelta = new Vector2(80, 80);
            closeRect.anchoredPosition = new Vector2(-20, -20);

            Image closeImage = closeObj.AddComponent<Image>();
            closeImage.color = new Color(0.5f, 0.3f, 0.3f, 0.9f);

            closeButton = closeObj.AddComponent<Button>();
            closeButton.targetGraphic = closeImage;
            closeButton.onClick.AddListener(Hide);

            // Button colors
            var colors = closeButton.colors;
            colors.highlightedColor = new Color(0.7f, 0.4f, 0.4f);
            colors.pressedColor = new Color(0.4f, 0.2f, 0.2f);
            closeButton.colors = colors;

            // Add outline
            Outline btnOutline = closeObj.AddComponent<Outline>();
            btnOutline.effectColor = new Color(0, 0, 0, 0.5f);
            btnOutline.effectDistance = new Vector2(2, -2);

            GameObject closeText = new GameObject("X");
            closeText.transform.SetParent(closeObj.transform, false);

            RectTransform xRect = closeText.AddComponent<RectTransform>();
            xRect.anchorMin = Vector2.zero;
            xRect.anchorMax = Vector2.one;
            xRect.offsetMin = Vector2.zero;
            xRect.offsetMax = Vector2.zero;

            TextMeshProUGUI xText = closeText.AddComponent<TextMeshProUGUI>();
            xText.text = "X";
            xText.fontSize = 48;
            xText.fontStyle = FontStyles.Bold;
            xText.color = Color.white;
            xText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(xText);
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
                    tabBg.color = new Color(0.3f, 0.5f, 0.7f);
                }
                else
                {
                    tabBg.color = new Color(0.15f, 0.15f, 0.2f);
                }
            }

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

            // Initialize fake players if not done
            if (fakePlayers.Count == 0)
            {
                InitializeFakePlayers();
            }

            // Select first tab (which will refresh entries)
            SelectTab(LeaderboardType.LifetimeMoney);

            panelCanvasGroup.alpha = 0;
            mainPanel.transform.localScale = Vector3.one * 0.9f;
            panelCanvasGroup.DOFade(1, 0.2f);
            mainPanel.transform.DOScale(1, 0.2f).SetEase(Ease.OutBack);

            AudioManager.Instance?.PlayButtonClickSound();
        }

        public void Hide()
        {
            panelCanvasGroup.DOFade(0, 0.15f);
            panel.transform.DOScale(0.9f, 0.15f).OnComplete(() =>
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

            // Add outline effect
            text.fontMaterial.EnableKeyword("OUTLINE_ON");
            text.outlineWidth = 0.2f;
            text.outlineColor = Color.black;
        }

        private void InitializeFakePlayers()
        {
            fakePlayers.Clear();

            // Create 30 fake players with varying scores
            double baseScore = 1000000; // 1 million base
            for (int i = 0; i < 30; i++)
            {
                string name = fakePlayerNames[i % fakePlayerNames.Length];
                if (i >= fakePlayerNames.Length)
                {
                    name += (i / fakePlayerNames.Length + 1).ToString();
                }

                // Higher ranked players have higher scores
                double score = baseScore * Math.Pow(0.85, i) * UnityEngine.Random.Range(0.9f, 1.1f);

                fakePlayers.Add(new FakePlayer
                {
                    name = name,
                    score = score,
                    rank = i + 1
                });
            }

            // Sort by score descending
            fakePlayers.Sort((a, b) => b.score.CompareTo(a.score));

            // Assign ranks
            for (int i = 0; i < fakePlayers.Count; i++)
            {
                fakePlayers[i].rank = i + 1;
            }
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

            for (int i = 0; i < displayCount; i++)
            {
                var fakePlayer = fakePlayers[i];

                // Check if player should be inserted here
                if (!playerShown && playerRank == fakePlayer.rank)
                {
                    CreateEntryRowTMP("YOU", playerScore, playerRank, true);
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
        }

        private void CreateFakePlayerRow(FakePlayer player, int index)
        {
            GameObject row = new GameObject($"Entry_{player.rank}");
            row.transform.SetParent(entriesContainer.transform, false);

            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0, 60);

            Image rowBg = row.AddComponent<Image>();
            if (player.rank <= 3)
            {
                Color[] podiumColors = {
                    new Color(1f, 0.84f, 0f, 0.3f),   // Gold
                    new Color(0.75f, 0.75f, 0.75f, 0.3f), // Silver
                    new Color(0.8f, 0.5f, 0.2f, 0.3f)  // Bronze
                };
                rowBg.color = podiumColors[player.rank - 1];
            }
            else
            {
                rowBg.color = new Color(0.1f, 0.1f, 0.15f, 0.5f);
            }

            // Rank
            GameObject rankObj = new GameObject("Rank");
            rankObj.transform.SetParent(row.transform, false);

            RectTransform rankRect = rankObj.AddComponent<RectTransform>();
            rankRect.anchorMin = new Vector2(0, 0);
            rankRect.anchorMax = new Vector2(0.12f, 1);
            rankRect.offsetMin = new Vector2(15, 0);
            rankRect.offsetMax = Vector2.zero;

            TextMeshProUGUI rankText = rankObj.AddComponent<TextMeshProUGUI>();
            rankText.text = $"#{player.rank}";
            rankText.fontSize = player.rank <= 3 ? 28 : 24;
            rankText.fontStyle = player.rank <= 3 ? FontStyles.Bold : FontStyles.Normal;

            if (player.rank == 1) rankText.color = new Color(1f, 0.84f, 0f);
            else if (player.rank == 2) rankText.color = new Color(0.8f, 0.8f, 0.85f);
            else if (player.rank == 3) rankText.color = new Color(0.8f, 0.5f, 0.2f);
            else rankText.color = Color.white;

            rankText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(rankText);

            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(row.transform, false);

            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.12f, 0);
            nameRect.anchorMax = new Vector2(0.55f, 1);
            nameRect.offsetMin = new Vector2(15, 0);
            nameRect.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = player.name;
            nameText.fontSize = 24;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Left;
            ApplySharedFont(nameText);

            // Score
            GameObject scoreObj = new GameObject("Score");
            scoreObj.transform.SetParent(row.transform, false);

            RectTransform scoreRect = scoreObj.AddComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.55f, 0);
            scoreRect.anchorMax = new Vector2(1, 1);
            scoreRect.offsetMin = new Vector2(10, 0);
            scoreRect.offsetMax = new Vector2(-20, 0);

            TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreText.text = FormatScore(player.score);
            scoreText.fontSize = 24;
            scoreText.fontStyle = FontStyles.Bold;
            scoreText.color = new Color(1f, 0.9f, 0.5f);
            scoreText.alignment = TextAlignmentOptions.Right;
            ApplySharedFont(scoreText);

            // Store reference for dynamic updates
            player.scoreText = scoreText;
            player.rowObject = row;
        }

        private void CreateEntryRowTMP(string name, double score, int rank, bool isPlayer)
        {
            GameObject row = new GameObject($"PlayerEntry");
            row.transform.SetParent(entriesContainer.transform, false);

            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0, 70);

            Image rowBg = row.AddComponent<Image>();
            rowBg.color = new Color(0.2f, 0.4f, 0.6f, 0.9f);

            // Add highlight border
            Outline border = row.AddComponent<Outline>();
            border.effectColor = new Color(0.3f, 0.9f, 1f, 0.8f);
            border.effectDistance = new Vector2(3, -3);

            // Rank
            GameObject rankObj = new GameObject("Rank");
            rankObj.transform.SetParent(row.transform, false);

            RectTransform rankRect = rankObj.AddComponent<RectTransform>();
            rankRect.anchorMin = new Vector2(0, 0);
            rankRect.anchorMax = new Vector2(0.12f, 1);
            rankRect.offsetMin = new Vector2(15, 0);
            rankRect.offsetMax = Vector2.zero;

            TextMeshProUGUI rankText = rankObj.AddComponent<TextMeshProUGUI>();
            rankText.text = $"#{rank}";
            rankText.fontSize = 28;
            rankText.fontStyle = FontStyles.Bold;
            rankText.color = new Color(0.3f, 0.9f, 1f);
            rankText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(rankText);

            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(row.transform, false);

            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.12f, 0);
            nameRect.anchorMax = new Vector2(0.55f, 1);
            nameRect.offsetMin = new Vector2(15, 0);
            nameRect.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = name + " (YOU)";
            nameText.fontSize = 26;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = new Color(0.3f, 0.9f, 1f);
            nameText.alignment = TextAlignmentOptions.Left;
            ApplySharedFont(nameText);

            // Score
            GameObject scoreObj = new GameObject("Score");
            scoreObj.transform.SetParent(row.transform, false);

            RectTransform scoreRect = scoreObj.AddComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.55f, 0);
            scoreRect.anchorMax = new Vector2(1, 1);
            scoreRect.offsetMin = new Vector2(10, 0);
            scoreRect.offsetMax = new Vector2(-20, 0);

            TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreText.text = FormatScore(score);
            scoreText.fontSize = 26;
            scoreText.fontStyle = FontStyles.Bold;
            scoreText.color = new Color(1f, 0.95f, 0.6f);
            scoreText.alignment = TextAlignmentOptions.Right;
            ApplySharedFont(scoreText);
        }

        private void CreateSeparatorRowTMP()
        {
            GameObject separator = new GameObject("Separator");
            separator.transform.SetParent(entriesContainer.transform, false);

            RectTransform sepRect = separator.AddComponent<RectTransform>();
            sepRect.sizeDelta = new Vector2(0, 40);

            TextMeshProUGUI sepText = separator.AddComponent<TextMeshProUGUI>();
            sepText.text = "• • •";
            sepText.fontSize = 24;
            sepText.color = new Color(0.5f, 0.5f, 0.5f);
            sepText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(sepText);
        }

        #endregion
    }
}
