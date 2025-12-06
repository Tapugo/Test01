using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using Incredicer.Core;
using Incredicer.UI;

namespace Incredicer.Editor
{
    public static class CreateGlobalEventPrefab
    {
        [MenuItem("Incredicer/Create Global Event Panel Prefab")]
        public static void Execute()
        {
            // Load GUI assets
            var guiAssets = AssetDatabase.LoadAssetAtPath<GUISpriteAssets>("Assets/Resources/GUISpriteAssets.asset");

            // Create the overlay root
            GameObject overlayRoot = CreateOverlayRoot();
            GameObject mainPanel = CreateMainPanel(overlayRoot.transform, guiAssets);

            // Create close button (on top)
            CreateCloseButton(mainPanel.transform, guiAssets);

            // Create header section
            CreateHeader(mainPanel.transform, guiAssets);

            // Create progress section
            CreateProgressSection(mainPanel.transform, guiAssets);

            // Create contributors section
            CreateContributorsSection(mainPanel.transform, guiAssets);

            // Create tiers section
            CreateTiersSection(mainPanel.transform, guiAssets);

            // Create no event panel
            CreateNoEventPanel(mainPanel.transform, guiAssets);

            // Ensure directory exists in Resources folder for runtime loading
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Prefabs");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Prefabs/UI"))
            {
                AssetDatabase.CreateFolder("Assets/Resources/Prefabs", "UI");
            }

            // Save as prefab in Resources folder
            string prefabPath = "Assets/Resources/Prefabs/UI/GlobalEventPanel.prefab";

            // Delete existing if present
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            PrefabUtility.SaveAsPrefabAsset(overlayRoot, prefabPath);
            Object.DestroyImmediate(overlayRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CreateGlobalEventPrefab] Created prefab at {prefabPath}");
        }

        private static GameObject CreateOverlayRoot()
        {
            GameObject overlayRoot = new GameObject("GlobalEventOverlay");

            RectTransform rt = overlayRoot.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Semi-transparent overlay background
            Image bgImage = overlayRoot.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.85f);

            // Canvas group for fading
            overlayRoot.AddComponent<CanvasGroup>();

            return overlayRoot;
        }

        private static GameObject CreateMainPanel(Transform parent, GUISpriteAssets guiAssets)
        {
            GameObject mainPanel = new GameObject("GlobalEventPanel");
            mainPanel.transform.SetParent(parent, false);

            RectTransform rect = mainPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.02f);
            rect.anchorMax = new Vector2(0.98f, 0.98f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Use popup background
            Image bg = mainPanel.AddComponent<Image>();
            if (guiAssets != null && guiAssets.popupBackground != null)
            {
                bg.sprite = guiAssets.popupBackground;
                bg.type = Image.Type.Sliced;
                bg.color = Color.white;
            }
            else
            {
                bg.color = new Color(0.08f, 0.1f, 0.15f, 0.98f);
            }

            // Canvas group for panel animations
            mainPanel.AddComponent<CanvasGroup>();

            return mainPanel;
        }

        private static void CreateCloseButton(Transform parent, GUISpriteAssets guiAssets)
        {
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(parent, false);

            RectTransform closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 1);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.sizeDelta = new Vector2(70, 70);
            closeRect.anchoredPosition = new Vector2(-20, -20);

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

            Button closeButton = closeObj.AddComponent<Button>();
            closeButton.targetGraphic = closeImage;

            ColorBlock colors = closeButton.colors;
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
                xText.fontSize = 48;
                xText.fontStyle = FontStyles.Bold;
                xText.color = Color.white;
                xText.alignment = TextAlignmentOptions.Center;
            }
        }

        private static void CreateHeader(Transform parent, GUISpriteAssets guiAssets)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.88f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.offsetMin = new Vector2(40, 0);
            headerRect.offsetMax = new Vector2(-100, -20);

            // Title ribbon
            GameObject titleRibbonObj = new GameObject("TitleRibbon");
            titleRibbonObj.transform.SetParent(header.transform, false);
            RectTransform ribbonRect = titleRibbonObj.AddComponent<RectTransform>();
            ribbonRect.anchorMin = new Vector2(0.5f, 0.6f);
            ribbonRect.anchorMax = new Vector2(0.5f, 0.6f);
            ribbonRect.sizeDelta = new Vector2(550, 100);
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
                ribbonBg.color = new Color(0.3f, 0.5f, 0.8f);
            }

            // Title text inside ribbon
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(titleRibbonObj.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(15, 5);
            titleRect.offsetMax = new Vector2(-15, -5);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "COMMUNITY EVENT";
            titleText.fontSize = 56;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 36;
            titleText.fontSizeMax = 64;

            // Description text - narrower to not overlap with time container
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(header.transform, false);

            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0);
            descRect.anchorMax = new Vector2(0.52f, 0.35f);
            descRect.offsetMin = new Vector2(40, 0);
            descRect.offsetMax = Vector2.zero;

            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Work together with the community!";
            descText.fontSize = 36;
            descText.color = new Color(0.7f, 0.7f, 0.8f);
            descText.alignment = TextAlignmentOptions.Left;

            // Time remaining container - wider to fit "remaining" text
            GameObject timeContainerObj = new GameObject("TimeContainer");
            timeContainerObj.transform.SetParent(header.transform, false);

            RectTransform timeContainerRect = timeContainerObj.AddComponent<RectTransform>();
            timeContainerRect.anchorMin = new Vector2(0.55f, 0);
            timeContainerRect.anchorMax = new Vector2(1, 0.45f);
            timeContainerRect.offsetMin = new Vector2(20, 0);
            timeContainerRect.offsetMax = new Vector2(-20, 0);

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
            timeRect.offsetMin = new Vector2(10, 0);
            timeRect.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI timeText = timeObj.AddComponent<TextMeshProUGUI>();
            timeText.text = "2d 12h remaining";
            timeText.fontSize = 32;
            timeText.fontStyle = FontStyles.Bold;
            timeText.color = Color.white;
            timeText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateProgressSection(Transform parent, GUISpriteAssets guiAssets)
        {
            GameObject progressSection = new GameObject("ProgressSection");
            progressSection.transform.SetParent(parent, false);

            RectTransform sectionRect = progressSection.AddComponent<RectTransform>();
            // Move progress section up slightly to avoid overlap with contributors
            sectionRect.anchorMin = new Vector2(0, 0.76f);
            sectionRect.anchorMax = new Vector2(1, 0.88f);
            sectionRect.offsetMin = new Vector2(40, 25);
            sectionRect.offsetMax = new Vector2(-40, 0);

            // Progress bar background
            GameObject progressBg = new GameObject("ProgressBarBg");
            progressBg.transform.SetParent(progressSection.transform, false);

            RectTransform bgRect = progressBg.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.5f);
            bgRect.anchorMax = new Vector2(1, 0.5f);
            bgRect.sizeDelta = new Vector2(-80, 70);
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

            // Fill mask
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

            Image fillImage = progressFill.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.6f, 1f);

            // Player contribution marker
            GameObject markerObj = new GameObject("PlayerMarker");
            markerObj.transform.SetParent(progressBg.transform, false);

            RectTransform markerRect = markerObj.AddComponent<RectTransform>();
            markerRect.anchorMin = new Vector2(0, 0.5f);
            markerRect.anchorMax = new Vector2(0, 0.5f);
            markerRect.pivot = new Vector2(0.5f, 0.5f);
            markerRect.sizeDelta = new Vector2(6, 80);
            markerRect.anchoredPosition = Vector2.zero;

            Image markerImage = markerObj.AddComponent<Image>();
            markerImage.color = new Color(1f, 0.85f, 0.2f);

            // Progress text
            GameObject progressTextObj = new GameObject("ProgressText");
            progressTextObj.transform.SetParent(progressSection.transform, false);

            RectTransform textRect = progressTextObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(500, 45);
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI progressText = progressTextObj.AddComponent<TextMeshProUGUI>();
            progressText.text = "0 / 1,000,000";
            progressText.fontSize = 40;
            progressText.fontStyle = FontStyles.Bold;
            progressText.color = Color.white;
            progressText.alignment = TextAlignmentOptions.Center;

            // Player contribution text - positioned below progress text with more spacing
            GameObject contribTextObj = new GameObject("ContributionText");
            contribTextObj.transform.SetParent(progressSection.transform, false);

            RectTransform contribRect = contribTextObj.AddComponent<RectTransform>();
            contribRect.anchorMin = new Vector2(0.5f, 0);
            contribRect.anchorMax = new Vector2(0.5f, 0);
            contribRect.pivot = new Vector2(0.5f, 1f);
            contribRect.sizeDelta = new Vector2(500, 40);
            contribRect.anchoredPosition = new Vector2(0, -5);

            TextMeshProUGUI contribText = contribTextObj.AddComponent<TextMeshProUGUI>();
            contribText.text = "Your contribution: 0 (0%)";
            contribText.fontSize = 32;
            contribText.color = new Color(1f, 0.85f, 0.2f);
            contribText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateContributorsSection(Transform parent, GUISpriteAssets guiAssets)
        {
            GameObject contributorsSection = new GameObject("ContributorsSection");
            contributorsSection.transform.SetParent(parent, false);

            RectTransform sectionRect = contributorsSection.AddComponent<RectTransform>();
            // Adjusted to avoid overlap - more space from progress section above
            sectionRect.anchorMin = new Vector2(0, 0.54f);
            sectionRect.anchorMax = new Vector2(1, 0.74f);
            sectionRect.offsetMin = new Vector2(40, 0);
            sectionRect.offsetMax = new Vector2(-40, -5);

            // Section label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(contributorsSection.transform, false);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.pivot = new Vector2(0.5f, 1);
            labelRect.sizeDelta = new Vector2(0, 40);
            labelRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = "RECENT CONTRIBUTORS";
            labelText.fontSize = 36;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = new Color(0.3f, 0.6f, 1f);
            labelText.alignment = TextAlignmentOptions.Center;

            // Contributors list container
            GameObject contributorsContainer = new GameObject("ContributorsList");
            contributorsContainer.transform.SetParent(contributorsSection.transform, false);

            RectTransform contRect = contributorsContainer.AddComponent<RectTransform>();
            contRect.anchorMin = Vector2.zero;
            contRect.anchorMax = Vector2.one;
            contRect.offsetMin = new Vector2(60, 10);
            contRect.offsetMax = new Vector2(-60, -45);

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
            layout.padding = new RectOffset(25, 25, 18, 18);
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private static void CreateTiersSection(Transform parent, GUISpriteAssets guiAssets)
        {
            GameObject tiersSection = new GameObject("TiersSection");
            tiersSection.transform.SetParent(parent, false);

            RectTransform sectionRect = tiersSection.AddComponent<RectTransform>();
            // Adjusted to match the new contributors section position
            sectionRect.anchorMin = new Vector2(0, 0);
            sectionRect.anchorMax = new Vector2(1, 0.54f);
            sectionRect.offsetMin = new Vector2(40, 80);
            sectionRect.offsetMax = new Vector2(-40, -5);

            // Section label - centered between contributors section above and tiers scroll below
            GameObject tiersLabelObj = new GameObject("TiersLabel");
            tiersLabelObj.transform.SetParent(tiersSection.transform, false);

            RectTransform tiersLabelRect = tiersLabelObj.AddComponent<RectTransform>();
            tiersLabelRect.anchorMin = new Vector2(0, 1);
            tiersLabelRect.anchorMax = new Vector2(1, 1);
            tiersLabelRect.pivot = new Vector2(0.5f, 1);
            tiersLabelRect.sizeDelta = new Vector2(0, 40);
            tiersLabelRect.anchoredPosition = new Vector2(0, 20);

            TextMeshProUGUI tiersLabelText = tiersLabelObj.AddComponent<TextMeshProUGUI>();
            tiersLabelText.text = "REWARD TIERS";
            tiersLabelText.fontSize = 36;
            tiersLabelText.fontStyle = FontStyles.Bold;
            tiersLabelText.color = new Color(1f, 0.85f, 0.2f);
            tiersLabelText.alignment = TextAlignmentOptions.Center;

            // Scroll view - positioned below the label with proper spacing
            GameObject scrollView = new GameObject("TiersScroll");
            scrollView.transform.SetParent(tiersSection.transform, false);

            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = new Vector2(0, -25);

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
            viewportRect.offsetMin = new Vector2(10, 10);
            viewportRect.offsetMax = new Vector2(-10, -10);

            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            scroll.viewport = viewportRect;

            // Content
            GameObject tiersContainer = new GameObject("TiersContent");
            tiersContainer.transform.SetParent(viewport.transform, false);

            RectTransform contentRect = tiersContainer.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = Vector2.zero;
            contentRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup tierLayout = tiersContainer.AddComponent<VerticalLayoutGroup>();
            tierLayout.spacing = 20;
            tierLayout.padding = new RectOffset(10, 10, 10, 10);
            tierLayout.childForceExpandWidth = true;
            tierLayout.childForceExpandHeight = false;
            tierLayout.childControlWidth = true;
            tierLayout.childControlHeight = false;

            ContentSizeFitter fitter = tiersContainer.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRect;
        }

        private static void CreateNoEventPanel(Transform parent, GUISpriteAssets guiAssets)
        {
            GameObject noEventPanel = new GameObject("NoEventPanel");
            noEventPanel.transform.SetParent(parent, false);

            RectTransform noEventRect = noEventPanel.AddComponent<RectTransform>();
            noEventRect.anchorMin = Vector2.zero;
            noEventRect.anchorMax = Vector2.one;
            noEventRect.offsetMin = new Vector2(40, 80);
            noEventRect.offsetMax = new Vector2(-40, -130);

            // Icon
            GameObject iconObj = new GameObject("NoEventIcon");
            iconObj.transform.SetParent(noEventPanel.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.7f);
            iconRect.anchorMax = new Vector2(0.5f, 0.7f);
            iconRect.sizeDelta = new Vector2(140, 140);

            Image iconImg = iconObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.iconStar != null)
            {
                iconImg.sprite = guiAssets.iconStar;
                iconImg.color = new Color(0.4f, 0.4f, 0.5f);
            }
            else
            {
                iconImg.color = new Color(0.4f, 0.4f, 0.5f);
            }

            // Message
            GameObject msgObj = new GameObject("Message");
            msgObj.transform.SetParent(noEventPanel.transform, false);

            RectTransform msgRect = msgObj.AddComponent<RectTransform>();
            msgRect.anchorMin = new Vector2(0.5f, 0.5f);
            msgRect.anchorMax = new Vector2(0.5f, 0.5f);
            msgRect.sizeDelta = new Vector2(700, 120);

            TextMeshProUGUI msgText = msgObj.AddComponent<TextMeshProUGUI>();
            msgText.text = "No active community event.\nCheck back later for the next challenge!";
            msgText.fontSize = 40;
            msgText.color = new Color(0.5f, 0.5f, 0.6f);
            msgText.alignment = TextAlignmentOptions.Center;

            // Start event button
            GameObject btnObj = new GameObject("StartEventButton");
            btnObj.transform.SetParent(noEventPanel.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.3f);
            btnRect.anchorMax = new Vector2(0.5f, 0.3f);
            btnRect.sizeDelta = new Vector2(360, 80);

            Image btnImage = btnObj.AddComponent<Image>();
            if (guiAssets != null && guiAssets.buttonBlue != null)
            {
                btnImage.sprite = guiAssets.buttonBlue;
                btnImage.type = Image.Type.Sliced;
                btnImage.color = Color.white;
            }
            else
            {
                btnImage.color = new Color(0.3f, 0.5f, 0.8f);
            }

            Button startBtn = btnObj.AddComponent<Button>();
            startBtn.targetGraphic = btnImage;

            ColorBlock colors = startBtn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            startBtn.colors = colors;

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);

            RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "START EVENT";
            btnText.fontSize = 38;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;

            // Start with NoEventPanel active
            noEventPanel.SetActive(true);
        }
    }
}
