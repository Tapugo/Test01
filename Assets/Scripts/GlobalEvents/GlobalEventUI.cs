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

        [Header("GUI Assets")]
        [SerializeField] private GUISpriteAssets guiAssets;

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
        private GameObject titleRibbonObj;
        private Image progressBarGlow;

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
            // Load GUI assets if not assigned
            if (guiAssets == null)
                guiAssets = GUISpriteAssets.Instance;

            // Try to load from prefab first, fallback to runtime creation
            bool loadedFromPrefab = LoadFromPrefab();
            if (!loadedFromPrefab)
            {
                Debug.Log("[GlobalEventUI] Prefab not found, creating UI at runtime");
                CreateUI();
            }

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

        private bool LoadFromPrefab()
        {
            // Try to load prefab from Resources
            GameObject prefab = Resources.Load<GameObject>("Prefabs/UI/GlobalEventPanel");
            if (prefab == null)
            {
                return false;
            }

            // Instantiate the prefab
            overlay = Instantiate(prefab, transform);
            overlay.name = "GlobalEventOverlay";

            // Find and cache UI references from the prefab
            panel = overlay.transform.Find("GlobalEventPanel")?.gameObject;
            if (panel == null)
            {
                Debug.LogError("[GlobalEventUI] Could not find GlobalEventPanel in prefab");
                Destroy(overlay);
                return false;
            }

            panelCanvasGroup = panel.GetComponent<CanvasGroup>();

            // Find header elements
            Transform header = panel.transform.Find("Header");
            if (header != null)
            {
                titleRibbonObj = header.Find("TitleRibbon")?.gameObject;
                if (titleRibbonObj != null)
                {
                    eventNameText = titleRibbonObj.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
                }
                eventDescText = header.Find("Description")?.GetComponent<TextMeshProUGUI>();
                Transform timeContainer = header.Find("TimeContainer");
                if (timeContainer != null)
                {
                    timeRemainingText = timeContainer.Find("TimeRemaining")?.GetComponent<TextMeshProUGUI>();
                }
            }

            // Find progress section elements
            Transform progressSection = panel.transform.Find("ProgressSection");
            if (progressSection != null)
            {
                Transform progressBg = progressSection.Find("ProgressBarBg");
                if (progressBg != null)
                {
                    Transform fillMask = progressBg.Find("FillMask");
                    if (fillMask != null)
                    {
                        progressBarFill = fillMask.Find("ProgressBarFill")?.GetComponent<Image>();
                    }
                    playerContributionMarker = progressBg.Find("PlayerMarker")?.GetComponent<Image>();
                }
                progressText = progressSection.Find("ProgressText")?.GetComponent<TextMeshProUGUI>();
                playerContributionText = progressSection.Find("ContributionText")?.GetComponent<TextMeshProUGUI>();
            }

            // Find contributors section
            Transform contributorsSection = panel.transform.Find("ContributorsSection");
            if (contributorsSection != null)
            {
                contributorsContainer = contributorsSection.Find("ContributorsList")?.gameObject;
            }

            // Find tiers section
            Transform tiersSection = panel.transform.Find("TiersSection");
            if (tiersSection != null)
            {
                Transform tiersScroll = tiersSection.Find("TiersScroll");
                if (tiersScroll != null)
                {
                    Transform viewport = tiersScroll.Find("Viewport");
                    if (viewport != null)
                    {
                        tiersContainer = viewport.Find("TiersContent")?.gameObject;
                    }
                }
            }

            // Find no event panel and its button
            noEventPanel = panel.transform.Find("NoEventPanel")?.gameObject;
            if (noEventPanel != null)
            {
                startEventButton = noEventPanel.transform.Find("StartEventButton")?.GetComponent<Button>();
                if (startEventButton != null)
                {
                    startEventButton.onClick.AddListener(OnStartEventClicked);
                }
            }

            // Find close button
            closeButton = panel.transform.Find("CloseButton")?.GetComponent<Button>();
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            // Apply shared font to all text
            ApplySharedFontToAll();

            // Initialize fake contributors
            if (contributorsContainer != null)
            {
                InitializeFakeContributors();
            }

            Debug.Log("[GlobalEventUI] Successfully loaded from prefab");
            return true;
        }

        private void ApplySharedFontToAll()
        {
            // Apply shared font to all TextMeshProUGUI components in the panel
            var allTexts = panel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in allTexts)
            {
                ApplySharedFont(text);
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
            overlayBg.color = UIDesignSystem.OverlayDark;

            // Main panel - fullscreen with padding
            panel = new GameObject("GlobalEventPanel");
            panel.transform.SetParent(overlay.transform, false);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.02f, 0.02f);
            panelRect.anchorMax = new Vector2(0.98f, 0.98f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Use popup background from GUI assets
            Image panelBg = panel.AddComponent<Image>();
            if (guiAssets != null && guiAssets.popupBackground != null)
            {
                panelBg.sprite = guiAssets.popupBackground;
                panelBg.type = Image.Type.Sliced;
                panelBg.color = Color.white;
            }
            else
            {
                panelBg.color = UIDesignSystem.PanelDark;
            }

            panelCanvasGroup = panel.AddComponent<CanvasGroup>();

            // Close button (create first so it's on top)
            CreateCloseButton();

            // Header with ribbon
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
            headerRect.offsetMin = new Vector2(UIDesignSystem.SpacingL, 0);
            headerRect.offsetMax = new Vector2(-100, -UIDesignSystem.SpacingM);

            // Title ribbon
            titleRibbonObj = new GameObject("TitleRibbon");
            titleRibbonObj.transform.SetParent(header.transform, false);
            RectTransform ribbonRect = titleRibbonObj.AddComponent<RectTransform>();
            ribbonRect.anchorMin = new Vector2(0.5f, 0.6f);
            ribbonRect.anchorMax = new Vector2(0.5f, 0.6f);
            ribbonRect.sizeDelta = new Vector2(500, 90);
            ribbonRect.anchoredPosition = new Vector2(0, 10);

            Image ribbonBg = titleRibbonObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.ribbonBlue != null)
            {
                ribbonBg.sprite = guiAssets.ribbonBlue;
                ribbonBg.type = Image.Type.Sliced;
                ribbonBg.color = Color.white;
            }
            else
            {
                ribbonBg.color = UIDesignSystem.EventBlue;
            }

            // Glow behind ribbon
            GameObject ribbonGlowObj = new GameObject("RibbonGlow");
            ribbonGlowObj.transform.SetParent(header.transform, false);
            ribbonGlowObj.transform.SetSiblingIndex(0);  // Behind ribbon
            RectTransform ribbonGlowRect = ribbonGlowObj.AddComponent<RectTransform>();
            ribbonGlowRect.anchorMin = new Vector2(0.5f, 0.6f);
            ribbonGlowRect.anchorMax = new Vector2(0.5f, 0.6f);
            ribbonGlowRect.sizeDelta = new Vector2(580, 120);
            ribbonGlowRect.anchoredPosition = new Vector2(0, 10);

            Image ribbonGlowImg = ribbonGlowObj.AddComponent<Image>();
            ribbonGlowImg.color = new Color(0.3f, 0.7f, 1f, 0.3f);
            ribbonGlowObj.transform.DOScale(1.1f, UIDesignSystem.AnimGlow).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            // Title inside ribbon
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(titleRibbonObj.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(15, 5);
            titleRect.offsetMax = new Vector2(-15, -5);

            eventNameText = titleObj.AddComponent<TextMeshProUGUI>();
            eventNameText.text = "COMMUNITY EVENT";
            eventNameText.fontSize = UIDesignSystem.FontSizeTitle;
            eventNameText.fontStyle = FontStyles.Bold;
            eventNameText.color = Color.white;
            eventNameText.alignment = TextAlignmentOptions.Center;
            eventNameText.enableAutoSizing = true;
            eventNameText.fontSizeMin = 36;
            eventNameText.fontSizeMax = 64;
            ApplySharedFont(eventNameText);

            // Description below ribbon
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(header.transform, false);

            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0);
            descRect.anchorMax = new Vector2(0.65f, 0.35f);
            descRect.offsetMin = new Vector2(UIDesignSystem.SpacingL, 0);
            descRect.offsetMax = new Vector2(0, 0);

            eventDescText = descObj.AddComponent<TextMeshProUGUI>();
            eventDescText.text = "Work together with the community!";
            eventDescText.fontSize = UIDesignSystem.FontSizeLarge;
            eventDescText.color = UIDesignSystem.TextSecondary;
            eventDescText.alignment = TextAlignmentOptions.Left;
            ApplySharedFont(eventDescText);

            // Time remaining with icon-like background
            GameObject timeContainerObj = new GameObject("TimeContainer");
            timeContainerObj.transform.SetParent(header.transform, false);

            RectTransform timeContainerRect = timeContainerObj.AddComponent<RectTransform>();
            timeContainerRect.anchorMin = new Vector2(0.65f, 0);
            timeContainerRect.anchorMax = new Vector2(1, 0.4f);
            timeContainerRect.offsetMin = new Vector2(UIDesignSystem.SpacingM, 0);
            timeContainerRect.offsetMax = new Vector2(-80, 0);

            Image timeContainerBg = timeContainerObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.cardFrame != null)
            {
                timeContainerBg.sprite = guiAssets.cardFrame;
                timeContainerBg.type = Image.Type.Sliced;
                timeContainerBg.color = new Color(0.2f, 0.15f, 0.25f);
            }
            else
            {
                timeContainerBg.color = new Color(0.15f, 0.1f, 0.2f, 0.8f);
            }

            GameObject timeObj = new GameObject("TimeRemaining");
            timeObj.transform.SetParent(timeContainerObj.transform, false);

            RectTransform timeRect = timeObj.AddComponent<RectTransform>();
            timeRect.anchorMin = Vector2.zero;
            timeRect.anchorMax = Vector2.one;
            timeRect.offsetMin = new Vector2(UIDesignSystem.SpacingS, 0);
            timeRect.offsetMax = new Vector2(-UIDesignSystem.SpacingS, 0);

            timeRemainingText = timeObj.AddComponent<TextMeshProUGUI>();
            timeRemainingText.text = "2d 12h remaining";
            timeRemainingText.fontSize = UIDesignSystem.FontSizeBody;
            timeRemainingText.fontStyle = FontStyles.Bold;
            timeRemainingText.color = Color.white;
            timeRemainingText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(timeRemainingText);
        }

        private void CreateProgressSection()
        {
            GameObject progressSection = new GameObject("ProgressSection");
            progressSection.transform.SetParent(panel.transform, false);

            RectTransform sectionRect = progressSection.AddComponent<RectTransform>();
            sectionRect.anchorMin = new Vector2(0, 0.75f);
            sectionRect.anchorMax = new Vector2(1, 0.88f);
            sectionRect.offsetMin = new Vector2(UIDesignSystem.SpacingL, 0);
            sectionRect.offsetMax = new Vector2(-UIDesignSystem.SpacingL, 0);

            // Glow behind progress bar
            GameObject progressGlowObj = new GameObject("ProgressGlow");
            progressGlowObj.transform.SetParent(progressSection.transform, false);

            RectTransform glowRect = progressGlowObj.AddComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0, 0.5f);
            glowRect.anchorMax = new Vector2(1, 0.5f);
            glowRect.sizeDelta = new Vector2(-60, 80);
            glowRect.anchoredPosition = new Vector2(0, 15);

            progressBarGlow = progressGlowObj.AddComponent<Image>();
            progressBarGlow.color = new Color(0.3f, 0.7f, 1f, 0.2f);

            // Progress bar background frame
            GameObject progressBg = new GameObject("ProgressBarBg");
            progressBg.transform.SetParent(progressSection.transform, false);

            RectTransform bgRect = progressBg.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.5f);
            bgRect.anchorMax = new Vector2(1, 0.5f);
            bgRect.sizeDelta = new Vector2(-80, 60);
            bgRect.anchoredPosition = new Vector2(0, 15);

            Image bgImage = progressBg.AddComponent<Image>();
            if (guiAssets != null && guiAssets.horizontalFrame != null)
            {
                bgImage.sprite = guiAssets.horizontalFrame;
                bgImage.type = Image.Type.Sliced;
                bgImage.color = new Color(0.2f, 0.2f, 0.3f);
            }
            else
            {
                bgImage.color = new Color(0.15f, 0.15f, 0.2f);
            }

            // Progress bar fill container (mask)
            GameObject fillMask = new GameObject("FillMask");
            fillMask.transform.SetParent(progressBg.transform, false);

            RectTransform maskRect = fillMask.AddComponent<RectTransform>();
            maskRect.anchorMin = new Vector2(0.02f, 0.15f);
            maskRect.anchorMax = new Vector2(0.98f, 0.85f);
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;

            Image maskImage = fillMask.AddComponent<Image>();
            maskImage.color = new Color(0.1f, 0.1f, 0.15f);

            // Progress bar fill
            GameObject progressFill = new GameObject("ProgressBarFill");
            progressFill.transform.SetParent(fillMask.transform, false);

            RectTransform fillRect = progressFill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            progressBarFill = progressFill.AddComponent<Image>();
            // Use a gradient-like color
            progressBarFill.color = UIDesignSystem.EventBlue;

            // Shine effect on progress bar
            GameObject shineObj = new GameObject("Shine");
            shineObj.transform.SetParent(progressFill.transform, false);

            RectTransform shineRect = shineObj.AddComponent<RectTransform>();
            shineRect.anchorMin = new Vector2(0, 0.6f);
            shineRect.anchorMax = new Vector2(1, 0.9f);
            shineRect.offsetMin = Vector2.zero;
            shineRect.offsetMax = Vector2.zero;

            Image shineImage = shineObj.AddComponent<Image>();
            shineImage.color = new Color(1f, 1f, 1f, 0.2f);

            // Player contribution marker with better visibility
            GameObject markerObj = new GameObject("PlayerMarker");
            markerObj.transform.SetParent(progressBg.transform, false);

            RectTransform markerRect = markerObj.AddComponent<RectTransform>();
            markerRect.anchorMin = new Vector2(0, 0.5f);
            markerRect.anchorMax = new Vector2(0, 0.5f);
            markerRect.pivot = new Vector2(0.5f, 0.5f);
            markerRect.sizeDelta = new Vector2(6, 75);
            markerRect.anchoredPosition = new Vector2(0, 0);

            playerContributionMarker = markerObj.AddComponent<Image>();
            playerContributionMarker.color = UIDesignSystem.AccentGold;

            // Marker glow
            GameObject markerGlowObj = new GameObject("MarkerGlow");
            markerGlowObj.transform.SetParent(markerObj.transform, false);
            markerGlowObj.transform.SetAsFirstSibling();

            RectTransform markerGlowRect = markerGlowObj.AddComponent<RectTransform>();
            markerGlowRect.anchorMin = Vector2.zero;
            markerGlowRect.anchorMax = Vector2.one;
            markerGlowRect.offsetMin = new Vector2(-6, -6);
            markerGlowRect.offsetMax = new Vector2(6, 6);

            Image markerGlowImg = markerGlowObj.AddComponent<Image>();
            markerGlowImg.color = new Color(1f, 0.8f, 0.2f, 0.4f);

            // Progress text
            GameObject progressTextObj = new GameObject("ProgressText");
            progressTextObj.transform.SetParent(progressSection.transform, false);

            RectTransform textRect = progressTextObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(500, 40);
            textRect.anchoredPosition = new Vector2(0, 0);

            progressText = progressTextObj.AddComponent<TextMeshProUGUI>();
            progressText.text = "0 / 1,000,000";
            progressText.fontSize = UIDesignSystem.FontSizeSubtitle;
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
            contribRect.anchoredPosition = new Vector2(0, 10);

            playerContributionText = contribTextObj.AddComponent<TextMeshProUGUI>();
            playerContributionText.text = "Your contribution: 0 (0%)";
            playerContributionText.fontSize = UIDesignSystem.FontSizeBody;
            playerContributionText.color = UIDesignSystem.AccentGold;
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
            sectionRect.offsetMin = new Vector2(UIDesignSystem.SpacingL, UIDesignSystem.SpacingS);
            sectionRect.offsetMax = new Vector2(-UIDesignSystem.SpacingL, -UIDesignSystem.SpacingS);

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
            labelText.fontSize = UIDesignSystem.FontSizeLarge;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = UIDesignSystem.EventBlue;
            labelText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(labelText);

            // Contributors list container with frame
            contributorsContainer = new GameObject("ContributorsList");
            contributorsContainer.transform.SetParent(contributorsSection.transform, false);

            RectTransform contRect = contributorsContainer.AddComponent<RectTransform>();
            contRect.anchorMin = new Vector2(0, 0);
            contRect.anchorMax = new Vector2(1, 1);
            contRect.offsetMin = new Vector2(UIDesignSystem.SpacingXL, UIDesignSystem.SpacingS);
            contRect.offsetMax = new Vector2(-UIDesignSystem.SpacingXL, -40);

            Image contBg = contributorsContainer.AddComponent<Image>();
            if (guiAssets != null && guiAssets.listFrame != null)
            {
                contBg.sprite = guiAssets.listFrame;
                contBg.type = Image.Type.Sliced;
                contBg.color = new Color(0.15f, 0.18f, 0.25f);
            }
            else
            {
                contBg.color = new Color(0.08f, 0.08f, 0.12f, 0.8f);
            }

            VerticalLayoutGroup layout = contributorsContainer.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 15, 15);
            layout.spacing = 8;
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
            sectionRect.offsetMin = new Vector2(UIDesignSystem.SpacingL, 70);
            sectionRect.offsetMax = new Vector2(-UIDesignSystem.SpacingL, -UIDesignSystem.SpacingM);

            // Section label
            GameObject tiersLabelObj = new GameObject("TiersLabel");
            tiersLabelObj.transform.SetParent(tiersSection.transform, false);

            RectTransform tiersLabelRect = tiersLabelObj.AddComponent<RectTransform>();
            tiersLabelRect.anchorMin = new Vector2(0, 1);
            tiersLabelRect.anchorMax = new Vector2(1, 1);
            tiersLabelRect.pivot = new Vector2(0.5f, 0);
            tiersLabelRect.sizeDelta = new Vector2(0, 35);
            tiersLabelRect.anchoredPosition = new Vector2(0, 5);

            TextMeshProUGUI tiersLabelText = tiersLabelObj.AddComponent<TextMeshProUGUI>();
            tiersLabelText.text = "REWARD TIERS";
            tiersLabelText.fontSize = UIDesignSystem.FontSizeLarge;
            tiersLabelText.fontStyle = FontStyles.Bold;
            tiersLabelText.color = UIDesignSystem.AccentGold;
            tiersLabelText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(tiersLabelText);

            // Scroll view with frame
            GameObject scrollView = new GameObject("TiersScroll");
            scrollView.transform.SetParent(tiersSection.transform, false);

            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = new Vector2(0, -40);

            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 30f;

            Image scrollBg = scrollView.AddComponent<Image>();
            if (guiAssets != null && guiAssets.listFrame != null)
            {
                scrollBg.sprite = guiAssets.listFrame;
                scrollBg.type = Image.Type.Sliced;
                scrollBg.color = new Color(0.12f, 0.12f, 0.18f);
            }
            else
            {
                scrollBg.color = new Color(0.08f, 0.08f, 0.12f, 0.7f);
            }

            scrollView.AddComponent<Mask>().showMaskGraphic = true;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(8, 8);
            viewportRect.offsetMax = new Vector2(-8, -8);

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
            layout.spacing = UIDesignSystem.SpacingM;
            layout.padding = new RectOffset((int)UIDesignSystem.SpacingS, (int)UIDesignSystem.SpacingS, (int)UIDesignSystem.SpacingS, (int)UIDesignSystem.SpacingS);
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
            noEventRect.offsetMin = new Vector2(UIDesignSystem.SpacingL, 70);
            noEventRect.offsetMax = new Vector2(-UIDesignSystem.SpacingL, -130);

            // Icon for no event
            GameObject iconObj = new GameObject("NoEventIcon");
            iconObj.transform.SetParent(noEventPanel.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.7f);
            iconRect.anchorMax = new Vector2(0.5f, 0.7f);
            iconRect.sizeDelta = new Vector2(120, 120);

            Image iconImg = iconObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.iconStar != null)
            {
                iconImg.sprite = guiAssets.iconStar;
                iconImg.color = UIDesignSystem.TextMuted;
            }
            else
            {
                iconImg.color = UIDesignSystem.TextMuted;
            }

            // Slow rotation on icon
            iconObj.transform.DORotate(new Vector3(0, 0, 360), 10f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);

            // Message
            GameObject msgObj = new GameObject("Message");
            msgObj.transform.SetParent(noEventPanel.transform, false);

            RectTransform msgRect = msgObj.AddComponent<RectTransform>();
            msgRect.anchorMin = new Vector2(0.5f, 0.5f);
            msgRect.anchorMax = new Vector2(0.5f, 0.5f);
            msgRect.sizeDelta = new Vector2(600, 100);

            TextMeshProUGUI msgText = msgObj.AddComponent<TextMeshProUGUI>();
            msgText.text = "No active community event.\nCheck back later for the next challenge!";
            msgText.fontSize = UIDesignSystem.FontSizeSubtitle;
            msgText.color = UIDesignSystem.TextMuted;
            msgText.alignment = TextAlignmentOptions.Center;
            ApplySharedFont(msgText);

            // Start event button (for testing) with GUI styling
            GameObject btnObj = new GameObject("StartEventButton");
            btnObj.transform.SetParent(noEventPanel.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.3f);
            btnRect.anchorMax = new Vector2(0.5f, 0.3f);
            btnRect.sizeDelta = new Vector2(320, UIDesignSystem.ButtonHeightLarge);

            Image btnImage = btnObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonBlue != null)
            {
                btnImage.sprite = guiAssets.buttonBlue;
                btnImage.type = Image.Type.Sliced;
                btnImage.color = Color.white;
            }
            else
            {
                btnImage.color = UIDesignSystem.EventBlue;
            }

            startEventButton = btnObj.AddComponent<Button>();
            startEventButton.targetGraphic = btnImage;
            startEventButton.onClick.AddListener(OnStartEventClicked);

            var colors = startEventButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            startEventButton.colors = colors;

            // Button glow
            GameObject btnGlowObj = new GameObject("ButtonGlow");
            btnGlowObj.transform.SetParent(btnObj.transform, false);
            btnGlowObj.transform.SetAsFirstSibling();

            RectTransform btnGlowRect = btnGlowObj.AddComponent<RectTransform>();
            btnGlowRect.anchorMin = Vector2.zero;
            btnGlowRect.anchorMax = Vector2.one;
            btnGlowRect.offsetMin = new Vector2(-15, -15);
            btnGlowRect.offsetMax = new Vector2(15, 15);

            Image btnGlowImg = btnGlowObj.AddComponent<Image>();
            btnGlowImg.color = new Color(0.3f, 0.7f, 1f, 0.3f);
            btnGlowObj.transform.DOScale(1.1f, UIDesignSystem.AnimGlow).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);

            RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "START EVENT";
            btnText.fontSize = UIDesignSystem.FontSizeLarge;
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
            closeRect.sizeDelta = new Vector2(UIDesignSystem.ButtonHeightLarge, UIDesignSystem.ButtonHeightLarge);
            closeRect.anchoredPosition = new Vector2(-UIDesignSystem.SafeAreaPadding, -UIDesignSystem.SafeAreaPadding);

            Image closeImage = closeObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonRed != null)
            {
                closeImage.sprite = guiAssets.buttonRed;
                closeImage.type = Image.Type.Sliced;
                closeImage.color = Color.white;
            }
            else
            {
                closeImage.color = UIDesignSystem.ButtonDanger;
            }

            closeButton = closeObj.AddComponent<Button>();
            closeButton.targetGraphic = closeImage;
            closeButton.onClick.AddListener(Hide);

            var colors = closeButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            closeButton.colors = colors;

            // X icon or text
            GameObject closeContent = new GameObject("X");
            closeContent.transform.SetParent(closeObj.transform, false);

            RectTransform xRect = closeContent.AddComponent<RectTransform>();
            xRect.anchorMin = Vector2.zero;
            xRect.anchorMax = Vector2.one;
            xRect.offsetMin = Vector2.zero;
            xRect.offsetMax = Vector2.zero;

            if (guiAssets != null && guiAssets.iconClose != null)
            {
                Image xImg = closeContent.AddComponent<Image>();
                xImg.sprite = guiAssets.iconClose;
                xImg.preserveAspect = true;
                xImg.color = Color.white;
            }
            else
            {
                TextMeshProUGUI xText = closeContent.AddComponent<TextMeshProUGUI>();
                xText.text = "X";
                xText.fontSize = UIDesignSystem.FontSizeTitle;
                xText.fontStyle = FontStyles.Bold;
                xText.color = Color.white;
                xText.alignment = TextAlignmentOptions.Center;
                ApplySharedFont(xText);
            }
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
            cardRect.sizeDelta = new Vector2(0, 120);

            // Use card frame from GUI assets
            Image cardBg = card.AddComponent<Image>();
            if (guiAssets != null && guiAssets.cardFrame != null)
            {
                cardBg.sprite = guiAssets.cardFrame;
                cardBg.type = Image.Type.Sliced;
                if (isClaimed)
                    cardBg.color = new Color(0.25f, 0.4f, 0.25f);
                else if (isUnlocked)
                    cardBg.color = new Color(0.2f, 0.3f, 0.4f);
                else
                    cardBg.color = new Color(0.15f, 0.15f, 0.2f);
            }
            else
            {
                if (isClaimed)
                    cardBg.color = new Color(0.2f, 0.3f, 0.2f, 0.9f);
                else if (isUnlocked)
                    cardBg.color = new Color(0.15f, 0.2f, 0.25f, 0.9f);
                else
                    cardBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            }

            // Glow for claimable tiers
            if (canClaim)
            {
                GameObject glowObj = new GameObject("Glow");
                glowObj.transform.SetParent(card.transform, false);
                glowObj.transform.SetAsFirstSibling();

                RectTransform glowRect = glowObj.AddComponent<RectTransform>();
                glowRect.anchorMin = Vector2.zero;
                glowRect.anchorMax = Vector2.one;
                glowRect.offsetMin = new Vector2(-8, -8);
                glowRect.offsetMax = new Vector2(8, 8);

                Image glowImg = glowObj.AddComponent<Image>();
                glowImg.color = new Color(tier.tierColor.r, tier.tierColor.g, tier.tierColor.b, 0.3f);
                glowObj.transform.DOScale(1.03f, UIDesignSystem.AnimGlow).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }

            // Tier name with TMP
            GameObject tierNameObj = new GameObject("TierName");
            tierNameObj.transform.SetParent(card.transform, false);

            RectTransform nameRect = tierNameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.5f);
            nameRect.anchorMax = new Vector2(0.35f, 1);
            nameRect.offsetMin = new Vector2(UIDesignSystem.SpacingM, 5);
            nameRect.offsetMax = new Vector2(0, -5);

            TextMeshProUGUI nameText = tierNameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = tier.tierName;
            nameText.fontSize = UIDesignSystem.FontSizeBody;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = tier.tierColor;
            nameText.alignment = TextAlignmentOptions.Left;
            ApplySharedFont(nameText);

            // Threshold text with TMP
            GameObject thresholdObj = new GameObject("Threshold");
            thresholdObj.transform.SetParent(card.transform, false);

            RectTransform thresholdRect = thresholdObj.AddComponent<RectTransform>();
            thresholdRect.anchorMin = new Vector2(0, 0);
            thresholdRect.anchorMax = new Vector2(0.35f, 0.5f);
            thresholdRect.offsetMin = new Vector2(UIDesignSystem.SpacingM, 5);
            thresholdRect.offsetMax = new Vector2(0, 0);

            TextMeshProUGUI thresholdText = thresholdObj.AddComponent<TextMeshProUGUI>();
            thresholdText.text = $"At {tier.progressThreshold * 100:F0}%";
            thresholdText.fontSize = UIDesignSystem.FontSizeSmall;
            thresholdText.color = UIDesignSystem.TextMuted;
            thresholdText.alignment = TextAlignmentOptions.Left;
            ApplySharedFont(thresholdText);

            // Rewards with TMP
            GameObject rewardsObj = new GameObject("Rewards");
            rewardsObj.transform.SetParent(card.transform, false);

            RectTransform rewardsRect = rewardsObj.AddComponent<RectTransform>();
            rewardsRect.anchorMin = new Vector2(0.35f, 0);
            rewardsRect.anchorMax = new Vector2(0.7f, 1);
            rewardsRect.offsetMin = new Vector2(UIDesignSystem.SpacingS, UIDesignSystem.SpacingS);
            rewardsRect.offsetMax = new Vector2(-UIDesignSystem.SpacingS, -UIDesignSystem.SpacingS);

            TextMeshProUGUI rewardsText = rewardsObj.AddComponent<TextMeshProUGUI>();
            string rewards = "";
            if (tier.timeShardsReward > 0) rewards += $"<color=#AA88FF>{tier.timeShardsReward} Time Shards</color>\n";
            if (tier.darkMatterReward > 0) rewards += $"<color=#BB66FF>{tier.darkMatterReward} DM</color>\n";
            if (!string.IsNullOrEmpty(tier.specialReward)) rewards += $"<color=#FFD700>{tier.specialReward}</color>";
            rewardsText.text = rewards.TrimEnd('\n');
            rewardsText.fontSize = UIDesignSystem.FontSizeSmall;
            rewardsText.color = Color.white;
            rewardsText.alignment = TextAlignmentOptions.Left;
            rewardsText.richText = true;
            ApplySharedFont(rewardsText);

            // Status / Claim button
            if (isClaimed)
            {
                // Checkmark container
                GameObject claimedObj = new GameObject("Claimed");
                claimedObj.transform.SetParent(card.transform, false);

                RectTransform claimedRect = claimedObj.AddComponent<RectTransform>();
                claimedRect.anchorMin = new Vector2(0.7f, 0.15f);
                claimedRect.anchorMax = new Vector2(0.98f, 0.85f);
                claimedRect.offsetMin = Vector2.zero;
                claimedRect.offsetMax = Vector2.zero;

                // Background for claimed status
                Image claimedBg = claimedObj.AddComponent<Image>();
                if (guiAssets != null && guiAssets.cardFrame != null)
                {
                    claimedBg.sprite = guiAssets.cardFrame;
                    claimedBg.type = Image.Type.Sliced;
                    claimedBg.color = new Color(0.2f, 0.35f, 0.2f);
                }
                else
                {
                    claimedBg.color = new Color(0.2f, 0.3f, 0.2f, 0.8f);
                }

                TextMeshProUGUI claimedText = claimedObj.AddComponent<TextMeshProUGUI>();
                claimedText.text = "CLAIMED";
                claimedText.fontSize = UIDesignSystem.FontSizeBody;
                claimedText.fontStyle = FontStyles.Bold;
                claimedText.color = UIDesignSystem.SuccessGreen;
                claimedText.alignment = TextAlignmentOptions.Center;
                ApplySharedFont(claimedText);
            }
            else if (canClaim)
            {
                GameObject claimBtn = new GameObject("ClaimButton");
                claimBtn.transform.SetParent(card.transform, false);

                RectTransform btnRect = claimBtn.AddComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(0.7f, 0.15f);
                btnRect.anchorMax = new Vector2(0.98f, 0.85f);
                btnRect.offsetMin = Vector2.zero;
                btnRect.offsetMax = Vector2.zero;

                Image btnImage = claimBtn.AddComponent<Image>();
                if (guiAssets != null && guiAssets.buttonGreen != null)
                {
                    btnImage.sprite = guiAssets.buttonGreen;
                    btnImage.type = Image.Type.Sliced;
                    btnImage.color = Color.white;
                }
                else
                {
                    btnImage.color = tier.tierColor;
                }

                Button btn = claimBtn.AddComponent<Button>();
                btn.targetGraphic = btnImage;
                int tierIndex = index;
                btn.onClick.AddListener(() => OnClaimTier(tierIndex));

                var btnColors = btn.colors;
                btnColors.normalColor = Color.white;
                btnColors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
                btnColors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
                btn.colors = btnColors;

                GameObject btnTextObj = new GameObject("Text");
                btnTextObj.transform.SetParent(claimBtn.transform, false);

                RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
                btnTextRect.anchorMin = Vector2.zero;
                btnTextRect.anchorMax = Vector2.one;
                btnTextRect.offsetMin = Vector2.zero;
                btnTextRect.offsetMax = Vector2.zero;

                TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
                btnText.text = "CLAIM";
                btnText.fontSize = UIDesignSystem.FontSizeBody;
                btnText.fontStyle = FontStyles.Bold;
                btnText.color = Color.white;
                btnText.alignment = TextAlignmentOptions.Center;
                ApplySharedFont(btnText);

                // Pulse animation
                claimBtn.transform.DOScale(1.05f, UIDesignSystem.AnimPulse).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }
            else
            {
                // Locked status
                GameObject lockedObj = new GameObject("Locked");
                lockedObj.transform.SetParent(card.transform, false);

                RectTransform lockedRect = lockedObj.AddComponent<RectTransform>();
                lockedRect.anchorMin = new Vector2(0.7f, 0.15f);
                lockedRect.anchorMax = new Vector2(0.98f, 0.85f);
                lockedRect.offsetMin = Vector2.zero;
                lockedRect.offsetMax = Vector2.zero;

                // Background for locked status
                Image lockedBg = lockedObj.AddComponent<Image>();
                if (guiAssets != null && guiAssets.cardFrame != null)
                {
                    lockedBg.sprite = guiAssets.cardFrame;
                    lockedBg.type = Image.Type.Sliced;
                    lockedBg.color = new Color(0.12f, 0.12f, 0.15f);
                }
                else
                {
                    lockedBg.color = new Color(0.1f, 0.1f, 0.12f, 0.8f);
                }

                // Locked text on child object
                GameObject lockedTextObj = new GameObject("LockedText");
                lockedTextObj.transform.SetParent(lockedObj.transform, false);

                RectTransform lockedTextRect = lockedTextObj.AddComponent<RectTransform>();
                lockedTextRect.anchorMin = Vector2.zero;
                lockedTextRect.anchorMax = Vector2.one;
                lockedTextRect.offsetMin = Vector2.zero;
                lockedTextRect.offsetMax = Vector2.zero;

                TextMeshProUGUI lockedText = lockedTextObj.AddComponent<TextMeshProUGUI>();
                lockedText.text = "LOCKED";
                lockedText.fontSize = UIDesignSystem.FontSizeBody;
                lockedText.fontStyle = FontStyles.Bold;
                lockedText.color = UIDesignSystem.TextMuted;
                lockedText.alignment = TextAlignmentOptions.Center;
                ApplySharedFont(lockedText);
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

            // Ensure popup is rendered on top of other UI elements (like menu button)
            overlay.transform.SetAsLastSibling();

            RefreshUI();

            // Use UIDesignSystem animation
            UIDesignSystem.AnimateShowPanel(panel, panelCanvasGroup);

            // Animate title ribbon entrance
            if (titleRibbonObj != null)
            {
                titleRibbonObj.transform.localScale = new Vector3(0.8f, 1f, 1f);
                titleRibbonObj.transform.DOScale(1f, UIDesignSystem.AnimFadeIn).SetEase(Ease.OutBack);
            }

            // Apply button polish for press/release animations
            if (UI.UIPolishManager.Instance != null)
            {
                UI.UIPolishManager.Instance.PolishButtonsInPanel(panel);
            }

            // Register with PopupManager
            if (PopupManager.Instance != null)
                PopupManager.Instance.RegisterPopupOpen("GlobalEventUI");

            AudioManager.Instance?.PlayButtonClickSound();
        }

        public void Hide()
        {
            // Unregister from PopupManager
            if (PopupManager.Instance != null)
                PopupManager.Instance.RegisterPopupClosed("GlobalEventUI");

            var overlay = panel.transform.parent.gameObject;

            // Use UIDesignSystem animation
            UIDesignSystem.AnimateHidePanel(panel, panelCanvasGroup, () =>
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

            // Add outline effect using TMP's built-in properties (safer than modifying material directly)
            // Only apply outline if the font material supports it
            try
            {
                if (text.fontMaterial != null)
                {
                    text.outlineWidth = 0.15f;
                    text.outlineColor = new Color32(0, 0, 0, 180);
                }
            }
            catch (System.NullReferenceException)
            {
                // Font material doesn't support outline, skip it
            }
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
                contribRect.sizeDelta = new Vector2(0, 40);

                TextMeshProUGUI contribText = contribObj.AddComponent<TextMeshProUGUI>();
                contribText.text = $"<color=#88AAFF>{name}</color> contributed <color=#FFDD88>{FormatNumber(contribution)}</color>";
                contribText.fontSize = UIDesignSystem.FontSizeSmall;
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
