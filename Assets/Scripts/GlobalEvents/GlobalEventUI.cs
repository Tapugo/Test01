using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.Core;
using Incredicer.UI;

namespace Incredicer.GlobalEvents
{
    /// <summary>
    /// UI for displaying global community events with progress bar.
    /// </summary>
    public class GlobalEventUI : MonoBehaviour
    {
        [Header("Panel Settings")]
        [SerializeField] private float panelWidth = 1000f;  // 2x larger
        [SerializeField] private float panelHeight = 900f;   // 2x larger

        // UI References
        private GameObject panel;
        private CanvasGroup panelCanvasGroup;
        private TextMeshProUGUI eventNameText;
        private TextMeshProUGUI eventDescText;
        private TextMeshProUGUI timeRemainingText;
        private Image progressBarFill;
        private Image playerContributionMarker;
        private TextMeshProUGUI progressText;
        private TextMeshProUGUI playerContributionText;
        private GameObject tiersContainer;
        private GameObject contributorsContainer;
        private GameObject noEventPanel;
        private Button startEventButton;
        private Button closeButton;

        private bool isVisible = false;

        // Fake contributors for community feel
        private List<FakeContributor> fakeContributors = new List<FakeContributor>();
        private float contributorUpdateTimer = 0f;
        private float targetProgressFill = 0f;

        private class FakeContributor
        {
            public string name;
            public double contribution;
            public TextMeshProUGUI textComponent;
        }

        private string[] fakeNames = new string[]
        {
            "DiceMaster99", "LuckyRoller", "CriticalHit7", "GoldenDice", "RNGKing",
            "FortuneFavors", "HighRoller42", "DiceGoblin", "NatTwenty", "ChaosDice",
            "QuantumRoll", "InfiniteLoop", "TimeLord77", "DarkMatterX", "CosmicDice",
            "PixelPusher", "ByteMaster", "CodeNinja", "DataDruid", "AlgoWizard"
        };

        private GameObject overlay;

        private void Start()
        {
            CreateUI();
            // Hide the overlay (parent of panel)
            overlay = panel.transform.parent.gameObject;
            overlay.SetActive(false);

            // Subscribe to events
            if (GlobalEventManager.Instance != null)
            {
                GlobalEventManager.Instance.OnEventStarted += OnEventStarted;
                GlobalEventManager.Instance.OnEventEnded += OnEventEnded;
                GlobalEventManager.Instance.OnProgressUpdated += OnProgressUpdated;
                GlobalEventManager.Instance.OnTierReached += OnTierReached;
                GlobalEventManager.Instance.OnTierClaimed += OnTierClaimed;
            }
        }

        private void OnDestroy()
        {
            if (GlobalEventManager.Instance != null)
            {
                GlobalEventManager.Instance.OnEventStarted -= OnEventStarted;
                GlobalEventManager.Instance.OnEventEnded -= OnEventEnded;
                GlobalEventManager.Instance.OnProgressUpdated -= OnProgressUpdated;
                GlobalEventManager.Instance.OnTierReached -= OnTierReached;
                GlobalEventManager.Instance.OnTierClaimed -= OnTierClaimed;
            }
        }

        private void Update()
        {
            if (isVisible && GlobalEventManager.Instance != null && GlobalEventManager.Instance.HasActiveEvent())
            {
                UpdateTimeRemaining();

                // Slowly animate progress bar fill
                if (progressBarFill != null)
                {
                    float currentFill = progressBarFill.rectTransform.anchorMax.x;
                    if (Mathf.Abs(currentFill - targetProgressFill) > 0.001f)
                    {
                        float newFill = Mathf.Lerp(currentFill, targetProgressFill, Time.deltaTime * 2f);
                        progressBarFill.rectTransform.anchorMax = new Vector2(newFill, 1);
                    }
                }

                // Update fake contributors periodically
                contributorUpdateTimer -= Time.deltaTime;
                if (contributorUpdateTimer <= 0f)
                {
                    UpdateFakeContributors();
                    contributorUpdateTimer = UnityEngine.Random.Range(1f, 3f);
                }
            }
        }

        private void CreateUI()
        {
            // Background overlay - fullscreen
            var overlay = new GameObject("EventOverlay");
            overlay.transform.SetParent(transform, false);

            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            Image overlayBg = overlay.AddComponent<Image>();
            overlayBg.color = new Color(0, 0, 0, 0.85f);

            // Main panel - fullscreen with padding
            panel = new GameObject("GlobalEventPanel");
            panel.transform.SetParent(overlay.transform, false);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.02f, 0.02f);
            panelRect.anchorMax = new Vector2(0.98f, 0.98f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.06f, 0.06f, 0.1f, 0.98f);

            // Add outline for polish
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.7f, 1f, 0.6f);
            outline.effectDistance = new Vector2(3, -3);

            panelCanvasGroup = panel.AddComponent<CanvasGroup>();

            // Close button (create first so it's on top)
            CreateCloseButton();

            // Header
            CreateHeader();

            // Progress section
            CreateProgressSection();

            // Contributors section (fake players)
            CreateContributorsSection();

            // Tiers section
            CreateTiersSection();

            // No event panel
            CreateNoEventPanel();
        }

        private void CreateHeader()
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(panel.transform, false);

            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.88f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.offsetMin = new Vector2(30, 0);
            headerRect.offsetMax = new Vector2(-100, -20);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(header.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.5f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(30, 0);
            titleRect.offsetMax = new Vector2(-100, -15);

            eventNameText = titleObj.AddComponent<TextMeshProUGUI>();
            eventNameText.text = "COMMUNITY EVENT";
            eventNameText.fontSize = 48;
            eventNameText.fontStyle = FontStyles.Bold;
            eventNameText.color = new Color(0.3f, 0.7f, 1f);
            eventNameText.alignment = TextAlignmentOptions.Left;
            ApplySharedFont(eventNameText);

            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(header.transform, false);

            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0);
            descRect.anchorMax = new Vector2(0.7f, 0.5f);
            descRect.offsetMin = new Vector2(30, 15);
            descRect.offsetMax = new Vector2(0, 0);

            eventDescText = descObj.AddComponent<TextMeshProUGUI>();
            eventDescText.text = "Work together with the community!";
            eventDescText.fontSize = 24;
            eventDescText.color = new Color(0.7f, 0.7f, 0.7f);
            eventDescText.alignment = TextAlignmentOptions.Left;
            ApplySharedFont(eventDescText);

            // Time remaining
            GameObject timeObj = new GameObject("TimeRemaining");
            timeObj.transform.SetParent(header.transform, false);

            RectTransform timeRect = timeObj.AddComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(0.7f, 0);
            timeRect.anchorMax = new Vector2(1, 0.5f);
            timeRect.offsetMin = new Vector2(0, 15);
            timeRect.offsetMax = new Vector2(-100, 0);

            timeRemainingText = timeObj.AddComponent<TextMeshProUGUI>();
            timeRemainingText.text = "2d 12h remaining";
            timeRemainingText.fontSize = 24;
            timeRemainingText.fontStyle = FontStyles.Bold;
            timeRemainingText.color = Color.white;
            timeRemainingText.alignment = TextAlignmentOptions.Right;
            ApplySharedFont(timeRemainingText);
        }

        private void CreateProgressSection()
        {
            GameObject progressSection = new GameObject("ProgressSection");
            progressSection.transform.SetParent(panel.transform, false);

            RectTransform sectionRect = progressSection.AddComponent<RectTransform>();
            sectionRect.anchorMin = new Vector2(0, 0.75f);
            sectionRect.anchorMax = new Vector2(1, 0.88f);
            sectionRect.offsetMin = new Vector2(30, 0);
            sectionRect.offsetMax = new Vector2(-30, 0);

            // Progress bar background
            GameObject progressBg = new GameObject("ProgressBarBg");
            progressBg.transform.SetParent(progressSection.transform, false);

            RectTransform bgRect = progressBg.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.5f);
            bgRect.anchorMax = new Vector2(1, 0.5f);
            bgRect.sizeDelta = new Vector2(-80, 60);
            bgRect.anchoredPosition = new Vector2(0, 15);

            Image bgImage = progressBg.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.2f);

            // Progress bar fill
            GameObject progressFill = new GameObject("ProgressBarFill");
            progressFill.transform.SetParent(progressBg.transform, false);

            RectTransform fillRect = progressFill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            progressBarFill = progressFill.AddComponent<Image>();
            progressBarFill.color = new Color(0.3f, 0.7f, 1f);

            // Player contribution marker
            GameObject markerObj = new GameObject("PlayerMarker");
            markerObj.transform.SetParent(progressBg.transform, false);

            RectTransform markerRect = markerObj.AddComponent<RectTransform>();
            markerRect.anchorMin = new Vector2(0, 0.5f);
            markerRect.anchorMax = new Vector2(0, 0.5f);
            markerRect.pivot = new Vector2(0.5f, 0.5f);
            markerRect.sizeDelta = new Vector2(8, 70);
            markerRect.anchoredPosition = new Vector2(0, 0);

            playerContributionMarker = markerObj.AddComponent<Image>();
            playerContributionMarker.color = new Color(1f, 0.8f, 0.2f);

            // Progress text
            GameObject progressTextObj = new GameObject("ProgressText");
            progressTextObj.transform.SetParent(progressSection.transform, false);

            RectTransform textRect = progressTextObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(500, 40);
            textRect.anchoredPosition = new Vector2(0, 5);

            progressText = progressTextObj.AddComponent<TextMeshProUGUI>();
            progressText.text = "0 / 1,000,000";
            progressText.fontSize = 28;
            progressText.fontStyle = FontStyles.Bold;
            progressText.color = Color.white;
            progressText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(progressText);

            // Player contribution text
            GameObject contribTextObj = new GameObject("ContributionText");
            contribTextObj.transform.SetParent(progressSection.transform, false);

            RectTransform contribRect = contribTextObj.AddComponent<RectTransform>();
            contribRect.anchorMin = new Vector2(0.5f, 0);
            contribRect.anchorMax = new Vector2(0.5f, 0);
            contribRect.pivot = new Vector2(0.5f, 1);
            contribRect.sizeDelta = new Vector2(500, 35);
            contribRect.anchoredPosition = new Vector2(0, 15);

            playerContributionText = contribTextObj.AddComponent<TextMeshProUGUI>();
            playerContributionText.text = "Your contribution: 0 (0%)";
            playerContributionText.fontSize = 22;
            playerContributionText.color = new Color(1f, 0.8f, 0.2f);
            playerContributionText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(playerContributionText);
        }

        private void CreateContributorsSection()
        {
            GameObject contributorsSection = new GameObject("ContributorsSection");
            contributorsSection.transform.SetParent(panel.transform, false);

            RectTransform sectionRect = contributorsSection.AddComponent<RectTransform>();
            sectionRect.anchorMin = new Vector2(0, 0.55f);
            sectionRect.anchorMax = new Vector2(1, 0.75f);
            sectionRect.offsetMin = new Vector2(30, 10);
            sectionRect.offsetMax = new Vector2(-30, -10);

            // Section label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(contributorsSection.transform, false);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.pivot = new Vector2(0.5f, 1);
            labelRect.sizeDelta = new Vector2(0, 35);
            labelRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = "RECENT CONTRIBUTORS";
            labelText.fontSize = 24;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = new Color(0.6f, 0.8f, 1f);
            labelText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(labelText);

            // Contributors list container
            contributorsContainer = new GameObject("ContributorsList");
            contributorsContainer.transform.SetParent(contributorsSection.transform, false);

            RectTransform contRect = contributorsContainer.AddComponent<RectTransform>();
            contRect.anchorMin = new Vector2(0, 0);
            contRect.anchorMax = new Vector2(1, 1);
            contRect.offsetMin = new Vector2(40, 10);
            contRect.offsetMax = new Vector2(-40, -40);

            Image contBg = contributorsContainer.AddComponent<Image>();
            contBg.color = new Color(0.05f, 0.05f, 0.08f, 0.6f);

            VerticalLayoutGroup layout = contributorsContainer.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(15, 15, 10, 10);
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Initialize fake contributors
            InitializeFakeContributors();
        }

        private void CreateTiersSection()
        {
            GameObject tiersSection = new GameObject("TiersSection");
            tiersSection.transform.SetParent(panel.transform, false);

            RectTransform sectionRect = tiersSection.AddComponent<RectTransform>();
            sectionRect.anchorMin = new Vector2(0, 0);
            sectionRect.anchorMax = new Vector2(1, 0.55f);
            sectionRect.offsetMin = new Vector2(30, 70);
            sectionRect.offsetMax = new Vector2(-30, -20);

            // Scroll view
            GameObject scrollView = new GameObject("TiersScroll");
            scrollView.transform.SetParent(tiersSection.transform, false);

            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;

            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 30f;

            Image scrollBg = scrollView.AddComponent<Image>();
            scrollBg.color = new Color(0.05f, 0.05f, 0.08f, 0.5f);

            scrollView.AddComponent<Mask>().showMaskGraphic = true;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            scroll.viewport = viewportRect;

            // Content
            tiersContainer = new GameObject("TiersContent");
            tiersContainer.transform.SetParent(viewport.transform, false);

            RectTransform contentRect = tiersContainer.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            contentRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layout = tiersContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            ContentSizeFitter fitter = tiersContainer.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRect;
        }

        private void CreateNoEventPanel()
        {
            noEventPanel = new GameObject("NoEventPanel");
            noEventPanel.transform.SetParent(panel.transform, false);

            RectTransform noEventRect = noEventPanel.AddComponent<RectTransform>();
            noEventRect.anchorMin = Vector2.zero;
            noEventRect.anchorMax = Vector2.one;
            noEventRect.offsetMin = new Vector2(30, 70);
            noEventRect.offsetMax = new Vector2(-30, -130);

            // Message
            GameObject msgObj = new GameObject("Message");
            msgObj.transform.SetParent(noEventPanel.transform, false);

            RectTransform msgRect = msgObj.AddComponent<RectTransform>();
            msgRect.anchorMin = new Vector2(0.5f, 0.6f);
            msgRect.anchorMax = new Vector2(0.5f, 0.6f);
            msgRect.sizeDelta = new Vector2(600, 100);

            TextMeshProUGUI msgText = msgObj.AddComponent<TextMeshProUGUI>();
            msgText.text = "No active community event.\nCheck back later for the next challenge!";
            msgText.fontSize = 32;
            msgText.color = new Color(0.6f, 0.6f, 0.6f);
            msgText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(msgText);

            // Start event button (for testing)
            GameObject btnObj = new GameObject("StartEventButton");
            btnObj.transform.SetParent(noEventPanel.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.4f);
            btnRect.anchorMax = new Vector2(0.5f, 0.4f);
            btnRect.sizeDelta = new Vector2(300, 70);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.3f, 0.7f, 1f);

            startEventButton = btnObj.AddComponent<Button>();
            startEventButton.targetGraphic = btnImage;
            startEventButton.onClick.AddListener(OnStartEventClicked);

            // Button colors
            var colors = startEventButton.colors;
            colors.highlightedColor = new Color(0.4f, 0.8f, 1f);
            colors.pressedColor = new Color(0.2f, 0.5f, 0.8f);
            startEventButton.colors = colors;

            // Add outline
            Outline btnOutline = btnObj.AddComponent<Outline>();
            btnOutline.effectColor = new Color(0, 0, 0, 0.5f);
            btnOutline.effectDistance = new Vector2(2, -2);

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);

            RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "START EVENT";
            btnText.fontSize = 28;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(btnText);

            noEventPanel.SetActive(true);
        }

        private void CreateCloseButton()
        {
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(panel.transform, false);

            RectTransform closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 1);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.sizeDelta = new Vector2(70, 70);
            closeRect.anchoredPosition = new Vector2(-15, -15);

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
            xText.fontSize = 40;
            xText.fontStyle = FontStyles.Bold;
            xText.color = Color.white;
            xText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(xText);
        }

        #region Event Handlers

        private void OnEventStarted(GlobalEventDefinition eventDef, GlobalEventProgress progress)
        {
            RefreshUI();
        }

        private void OnEventEnded(GlobalEventDefinition eventDef, GlobalEventProgress progress)
        {
            RefreshUI();
        }

        private void OnProgressUpdated(double playerContribution, double communityProgress)
        {
            UpdateProgressDisplay();
        }

        private void OnTierReached(int tierIndex, GlobalEventTierReward tier)
        {
            RefreshTiers();

            // Pulse animation on the tier
            if (tierIndex < tiersContainer.transform.childCount)
            {
                var tierObj = tiersContainer.transform.GetChild(tierIndex);
                tierObj.DOPunchScale(Vector3.one * 0.1f, 0.3f, 2);
            }
        }

        private void OnTierClaimed(int tierIndex, GlobalEventTierReward tier)
        {
            RefreshTiers();
        }

        private void OnStartEventClicked()
        {
            GlobalEventManager.Instance?.StartRandomEvent();
            AudioManager.Instance?.PlayButtonClickSound();
        }

        #endregion

        #region UI Updates

        private void RefreshUI()
        {
            bool hasEvent = GlobalEventManager.Instance != null && GlobalEventManager.Instance.HasActiveEvent();

            noEventPanel.SetActive(!hasEvent);

            if (hasEvent)
            {
                var eventDef = GlobalEventManager.Instance.GetCurrentEvent();

                eventNameText.text = eventDef.eventName;
                eventNameText.color = eventDef.eventColor;
                eventDescText.text = eventDef.description;

                UpdateProgressDisplay();
                UpdateTimeRemaining();
                RefreshTiers();
            }
        }

        private void UpdateProgressDisplay()
        {
            if (GlobalEventManager.Instance == null || !GlobalEventManager.Instance.HasActiveEvent()) return;

            var eventDef = GlobalEventManager.Instance.GetCurrentEvent();
            var progress = GlobalEventManager.Instance.GetCurrentProgress();

            float normalizedProgress = GlobalEventManager.Instance.GetCommunityProgressNormalized();
            float playerPercent = GlobalEventManager.Instance.GetPlayerContributionPercent();

            // Set target for slow fill animation (handled in Update)
            targetProgressFill = normalizedProgress;

            // Update marker position
            float barWidth = progressBarFill.transform.parent.GetComponent<RectTransform>().rect.width;
            playerContributionMarker.rectTransform.anchoredPosition = new Vector2(normalizedProgress * barWidth, 0);

            // Update text
            progressText.text = $"{FormatNumber(progress.communityProgress)} / {FormatNumber(eventDef.communityGoal)}";
            playerContributionText.text = $"Your contribution: {FormatNumber(progress.playerContribution)} ({playerPercent * 100:F1}%)";
        }

        private void UpdateTimeRemaining()
        {
            if (GlobalEventManager.Instance == null) return;

            TimeSpan remaining = GlobalEventManager.Instance.GetTimeRemaining();

            if (remaining.TotalHours >= 24)
            {
                timeRemainingText.text = $"{(int)remaining.TotalDays}d {remaining.Hours}h remaining";
            }
            else if (remaining.TotalHours >= 1)
            {
                timeRemainingText.text = $"{(int)remaining.TotalHours}h {remaining.Minutes}m remaining";
            }
            else
            {
                timeRemainingText.text = $"{remaining.Minutes}m {remaining.Seconds}s remaining";
                timeRemainingText.color = new Color(1f, 0.4f, 0.4f);
            }
        }

        private void RefreshTiers()
        {
            // Clear existing tiers
            foreach (Transform child in tiersContainer.transform)
            {
                Destroy(child.gameObject);
            }

            if (GlobalEventManager.Instance == null || !GlobalEventManager.Instance.HasActiveEvent()) return;

            var eventDef = GlobalEventManager.Instance.GetCurrentEvent();
            var progress = GlobalEventManager.Instance.GetCurrentProgress();
            float communityProgress = GlobalEventManager.Instance.GetCommunityProgressNormalized();

            for (int i = 0; i < eventDef.tierRewards.Length; i++)
            {
                CreateTierCard(eventDef.tierRewards[i], i, communityProgress, progress.highestTierClaimed);
            }
        }

        private void CreateTierCard(GlobalEventTierReward tier, int index, float currentProgress, int highestClaimed)
        {
            bool isUnlocked = currentProgress >= tier.progressThreshold;
            bool isClaimed = index <= highestClaimed;
            bool canClaim = isUnlocked && !isClaimed;

            GameObject card = new GameObject($"Tier_{index}");
            card.transform.SetParent(tiersContainer.transform, false);

            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(0, 80);

            Image cardBg = card.AddComponent<Image>();
            if (isClaimed)
                cardBg.color = new Color(0.2f, 0.3f, 0.2f, 0.9f);
            else if (isUnlocked)
                cardBg.color = new Color(0.15f, 0.2f, 0.25f, 0.9f);
            else
                cardBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Tier name and threshold
            GameObject tierNameObj = new GameObject("TierName");
            tierNameObj.transform.SetParent(card.transform, false);

            RectTransform nameRect = tierNameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.5f);
            nameRect.anchorMax = new Vector2(0.4f, 1);
            nameRect.offsetMin = new Vector2(15, 5);
            nameRect.offsetMax = new Vector2(0, -5);

            Text nameText = tierNameObj.AddComponent<Text>();
            nameText.text = tier.tierName;
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 22;
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = tier.tierColor;
            nameText.alignment = TextAnchor.MiddleLeft;

            // Threshold text
            GameObject thresholdObj = new GameObject("Threshold");
            thresholdObj.transform.SetParent(card.transform, false);

            RectTransform thresholdRect = thresholdObj.AddComponent<RectTransform>();
            thresholdRect.anchorMin = new Vector2(0, 0);
            thresholdRect.anchorMax = new Vector2(0.4f, 0.5f);
            thresholdRect.offsetMin = new Vector2(15, 5);
            thresholdRect.offsetMax = new Vector2(0, 0);

            Text thresholdText = thresholdObj.AddComponent<Text>();
            thresholdText.text = $"At {tier.progressThreshold * 100:F0}%";
            thresholdText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            thresholdText.fontSize = 16;
            thresholdText.color = new Color(0.6f, 0.6f, 0.6f);
            thresholdText.alignment = TextAnchor.MiddleLeft;

            // Rewards
            GameObject rewardsObj = new GameObject("Rewards");
            rewardsObj.transform.SetParent(card.transform, false);

            RectTransform rewardsRect = rewardsObj.AddComponent<RectTransform>();
            rewardsRect.anchorMin = new Vector2(0.4f, 0);
            rewardsRect.anchorMax = new Vector2(0.7f, 1);
            rewardsRect.offsetMin = new Vector2(10, 10);
            rewardsRect.offsetMax = new Vector2(-10, -10);

            Text rewardsText = rewardsObj.AddComponent<Text>();
            string rewards = "";
            if (tier.timeShardsReward > 0) rewards += $"⏱ {tier.timeShardsReward} Time Shards\n";
            if (tier.darkMatterReward > 0) rewards += $"◆ {tier.darkMatterReward} DM\n";
            if (!string.IsNullOrEmpty(tier.specialReward)) rewards += tier.specialReward;
            rewardsText.text = rewards.TrimEnd('\n');
            rewardsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rewardsText.fontSize = 16;
            rewardsText.color = Color.white;
            rewardsText.alignment = TextAnchor.MiddleLeft;

            // Status / Claim button
            if (isClaimed)
            {
                GameObject claimedObj = new GameObject("Claimed");
                claimedObj.transform.SetParent(card.transform, false);

                RectTransform claimedRect = claimedObj.AddComponent<RectTransform>();
                claimedRect.anchorMin = new Vector2(0.7f, 0.2f);
                claimedRect.anchorMax = new Vector2(0.95f, 0.8f);
                claimedRect.offsetMin = Vector2.zero;
                claimedRect.offsetMax = Vector2.zero;

                Text claimedText = claimedObj.AddComponent<Text>();
                claimedText.text = "✓ CLAIMED";
                claimedText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                claimedText.fontSize = 18;
                claimedText.fontStyle = FontStyle.Bold;
                claimedText.color = new Color(0.4f, 0.8f, 0.4f);
                claimedText.alignment = TextAnchor.MiddleCenter;
            }
            else if (canClaim)
            {
                GameObject claimBtn = new GameObject("ClaimButton");
                claimBtn.transform.SetParent(card.transform, false);

                RectTransform btnRect = claimBtn.AddComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(0.7f, 0.2f);
                btnRect.anchorMax = new Vector2(0.95f, 0.8f);
                btnRect.offsetMin = Vector2.zero;
                btnRect.offsetMax = Vector2.zero;

                Image btnImage = claimBtn.AddComponent<Image>();
                btnImage.color = tier.tierColor;

                Button btn = claimBtn.AddComponent<Button>();
                btn.targetGraphic = btnImage;
                int tierIndex = index;
                btn.onClick.AddListener(() => OnClaimTier(tierIndex));

                GameObject btnTextObj = new GameObject("Text");
                btnTextObj.transform.SetParent(claimBtn.transform, false);

                RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
                btnTextRect.anchorMin = Vector2.zero;
                btnTextRect.anchorMax = Vector2.one;
                btnTextRect.offsetMin = Vector2.zero;
                btnTextRect.offsetMax = Vector2.zero;

                Text btnText = btnTextObj.AddComponent<Text>();
                btnText.text = "CLAIM";
                btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                btnText.fontSize = 18;
                btnText.fontStyle = FontStyle.Bold;
                btnText.color = Color.black;
                btnText.alignment = TextAnchor.MiddleCenter;

                // Pulse animation
                claimBtn.transform.DOScale(1.05f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                // Locked
                GameObject lockedObj = new GameObject("Locked");
                lockedObj.transform.SetParent(card.transform, false);

                RectTransform lockedRect = lockedObj.AddComponent<RectTransform>();
                lockedRect.anchorMin = new Vector2(0.7f, 0.2f);
                lockedRect.anchorMax = new Vector2(0.95f, 0.8f);
                lockedRect.offsetMin = Vector2.zero;
                lockedRect.offsetMax = Vector2.zero;

                Text lockedText = lockedObj.AddComponent<Text>();
                lockedText.text = "🔒 LOCKED";
                lockedText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                lockedText.fontSize = 16;
                lockedText.color = new Color(0.5f, 0.5f, 0.5f);
                lockedText.alignment = TextAnchor.MiddleCenter;
            }
        }

        private void OnClaimTier(int tierIndex)
        {
            GlobalEventManager.Instance?.ClaimTierReward(tierIndex);
            AudioManager.Instance?.PlayButtonClickSound();
        }

        private string FormatNumber(double num)
        {
            if (num >= 1000000) return $"{num / 1000000:F2}M";
            if (num >= 1000) return $"{num / 1000:F1}K";
            return num.ToString("N0");
        }

        #endregion

        #region Show/Hide

        public void Show()
        {
            // Get the overlay (parent of panel)
            var overlay = panel.transform.parent.gameObject;
            overlay.SetActive(true);
            isVisible = true;

            RefreshUI();

            panelCanvasGroup.alpha = 0;
            panel.transform.localScale = Vector3.one * 0.9f;
            panelCanvasGroup.DOFade(1, 0.2f);
            panel.transform.DOScale(1, 0.2f).SetEase(Ease.OutBack);

            AudioManager.Instance?.PlayButtonClickSound();
        }

        public void Hide()
        {
            var overlay = panel.transform.parent.gameObject;

            panelCanvasGroup.DOFade(0, 0.15f);
            panel.transform.DOScale(0.9f, 0.15f).OnComplete(() =>
            {
                overlay.SetActive(false);
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

        private void InitializeFakeContributors()
        {
            fakeContributors.Clear();

            // Create 5 initial fake contributors
            for (int i = 0; i < 5; i++)
            {
                string name = fakeNames[UnityEngine.Random.Range(0, fakeNames.Length)];
                double contribution = UnityEngine.Random.Range(100, 5000);

                GameObject contribObj = new GameObject($"Contributor_{i}");
                contribObj.transform.SetParent(contributorsContainer.transform, false);

                RectTransform contribRect = contribObj.AddComponent<RectTransform>();
                contribRect.sizeDelta = new Vector2(0, 25);

                TextMeshProUGUI contribText = contribObj.AddComponent<TextMeshProUGUI>();
                contribText.text = $"<color=#88AAFF>{name}</color> contributed <color=#FFDD88>{FormatNumber(contribution)}</color>";
                contribText.fontSize = 18;
                contribText.alignment = TextAlignmentOptions.Left;
                contribText.color = Color.white;
                ApplySharedFont(contribText);

                fakeContributors.Add(new FakeContributor
                {
                    name = name,
                    contribution = contribution,
                    textComponent = contribText
                });
            }
        }

        private void UpdateFakeContributors()
        {
            if (fakeContributors.Count == 0 || contributorsContainer == null) return;

            // Pick a random contributor to update
            int index = UnityEngine.Random.Range(0, fakeContributors.Count);
            var contributor = fakeContributors[index];

            // Give them a new contribution
            double newContribution = UnityEngine.Random.Range(50, 2000);
            contributor.contribution += newContribution;

            // Maybe change their name occasionally
            if (UnityEngine.Random.value < 0.3f)
            {
                contributor.name = fakeNames[UnityEngine.Random.Range(0, fakeNames.Length)];
            }

            // Update the text with animation
            if (contributor.textComponent != null)
            {
                contributor.textComponent.text = $"<color=#88AAFF>{contributor.name}</color> contributed <color=#FFDD88>+{FormatNumber(newContribution)}</color>";

                // Pulse effect
                contributor.textComponent.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 2);
            }

            // Move to top of list
            if (contributor.textComponent != null)
            {
                contributor.textComponent.transform.SetAsFirstSibling();
            }
        }

        #endregion
    }
}
