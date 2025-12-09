using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Incredicer.UI;
using System.Collections.Generic;

namespace Incredicer.Editor
{
    /// <summary>
    /// Editor script to apply GUI-CasualFantasy sprites to all UI elements in the scene.
    /// </summary>
    public static class ApplyGUISpritesToScene
    {
        private static GUISpriteAssets guiAssets;
        private static int updatedCount;

        [MenuItem("Incredicer/Setup/Apply GUI Sprites to Scene")]
        public static void ApplySprites()
        {
            // Load the GUI sprite assets
            guiAssets = AssetDatabase.LoadAssetAtPath<GUISpriteAssets>("Assets/Resources/GUISpriteAssets.asset");
            if (guiAssets == null)
            {
                Debug.LogError("[ApplyGUISprites] GUISpriteAssets not found! Run 'Incredicer/Setup/GUI Sprite Assets' first.");
                return;
            }

            updatedCount = 0;

            // Find all canvases in the scene
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
            foreach (Canvas canvas in canvases)
            {
                ProcessTransform(canvas.transform);
            }

            // Mark scene as dirty
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log($"[ApplyGUISprites] Applied GUI sprites to {updatedCount} UI elements!");
        }

        private static void ProcessTransform(Transform transform)
        {
            // Process this GameObject
            ProcessGameObject(transform.gameObject);

            // Process children
            foreach (Transform child in transform)
            {
                ProcessTransform(child);
            }
        }

        private static void ProcessGameObject(GameObject go)
        {
            string nameLower = go.name.ToLower();

            // Process buttons
            Button button = go.GetComponent<Button>();
            if (button != null)
            {
                ApplyButtonSprite(go, button);
            }

            // Process images (panels, frames, etc.)
            Image image = go.GetComponent<Image>();
            if (image != null && button == null) // Don't process button images twice
            {
                ApplyImageSprite(go, image);
            }

            // Process toggles
            Toggle toggle = go.GetComponent<Toggle>();
            if (toggle != null)
            {
                ApplyToggleSprites(go, toggle);
            }

            // Process sliders
            Slider slider = go.GetComponent<Slider>();
            if (slider != null)
            {
                ApplySliderSprites(go, slider);
            }
        }

        private static void ApplyButtonSprite(GameObject go, Button button)
        {
            Image targetImage = button.targetGraphic as Image;
            if (targetImage == null) return;

            string nameLower = go.name.ToLower();
            Sprite newSprite = null;

            // Determine button style by name
            if (nameLower.Contains("close") || nameLower.Contains("cancel") || nameLower.Contains("no"))
            {
                newSprite = guiAssets.buttonRed;
            }
            else if (nameLower.Contains("confirm") || nameLower.Contains("ok") || nameLower.Contains("yes") ||
                     nameLower.Contains("buy") || nameLower.Contains("purchase") || nameLower.Contains("claim"))
            {
                newSprite = guiAssets.buttonGreen;
            }
            else if (nameLower.Contains("upgrade") || nameLower.Contains("level") || nameLower.Contains("skill"))
            {
                newSprite = guiAssets.buttonBlue;
            }
            else if (nameLower.Contains("special") || nameLower.Contains("premium") || nameLower.Contains("rare"))
            {
                newSprite = guiAssets.buttonPurple;
            }
            else if (nameLower.Contains("warning") || nameLower.Contains("caution"))
            {
                newSprite = guiAssets.buttonYellow;
            }
            else if (nameLower.Contains("settings") || nameLower.Contains("option") || nameLower.Contains("menu"))
            {
                newSprite = guiAssets.buttonGray;
            }
            else if (nameLower.Contains("back") || nameLower.Contains("return"))
            {
                newSprite = guiAssets.buttonGray;
            }
            else
            {
                // Default to blue for action buttons
                newSprite = guiAssets.buttonBlue;
            }

            if (newSprite != null)
            {
                Undo.RecordObject(targetImage, "Apply GUI Sprite");
                targetImage.sprite = newSprite;
                targetImage.type = Image.Type.Sliced;
                targetImage.color = Color.white;
                EditorUtility.SetDirty(targetImage);
                updatedCount++;
            }

            // Update button colors for sprite-based buttons
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
        }

        private static void ApplyImageSprite(GameObject go, Image image)
        {
            string nameLower = go.name.ToLower();
            Sprite newSprite = null;

            // Popup/Panel backgrounds
            if (nameLower.Contains("popup") || nameLower.Contains("dialog") || nameLower.Contains("modal"))
            {
                newSprite = guiAssets.popupBackground;
            }
            else if (nameLower.Contains("panel") && !nameLower.Contains("content"))
            {
                newSprite = guiAssets.popupBackgroundAlt;
            }
            // Frames
            else if (nameLower.Contains("frame") || nameLower.Contains("border"))
            {
                if (nameLower.Contains("item") || nameLower.Contains("slot") || nameLower.Contains("icon"))
                {
                    newSprite = guiAssets.itemFrameGray;
                }
                else if (nameLower.Contains("card"))
                {
                    newSprite = guiAssets.cardFrame;
                }
                else
                {
                    newSprite = guiAssets.frameSquareOutline;
                }
            }
            // Item/Slot backgrounds
            else if (nameLower.Contains("slot") || nameLower.Contains("itembox") || nameLower.Contains("dicebox"))
            {
                newSprite = guiAssets.itemFrameGray;
            }
            // Status bar backgrounds
            else if (nameLower.Contains("statusbar") || nameLower.Contains("currency"))
            {
                newSprite = guiAssets.statusBarBackground;
            }
            // Tab backgrounds
            else if (nameLower.Contains("tab"))
            {
                newSprite = nameLower.Contains("active") || nameLower.Contains("selected") ?
                    guiAssets.tabMenuFocus : guiAssets.tabMenuBackground;
            }
            // Close icons
            else if (nameLower.Contains("close") && image.sprite == null)
            {
                newSprite = guiAssets.iconClose;
            }
            // Lock icons
            else if (nameLower.Contains("lock"))
            {
                newSprite = guiAssets.iconLock;
            }
            // Star icons
            else if (nameLower.Contains("star"))
            {
                newSprite = guiAssets.iconStar;
            }
            // Coin icons
            else if (nameLower.Contains("coin") || nameLower.Contains("gold") || nameLower.Contains("money"))
            {
                if (!nameLower.Contains("text") && !nameLower.Contains("label"))
                    newSprite = guiAssets.iconCoin;
            }
            // Background dimming
            else if (nameLower.Contains("dim") || nameLower.Contains("overlay") || nameLower.Contains("backdrop"))
            {
                if (guiAssets.panelDimmed != null)
                    newSprite = guiAssets.panelDimmed;
            }

            if (newSprite != null)
            {
                Undo.RecordObject(image, "Apply GUI Sprite");
                image.sprite = newSprite;
                image.type = Image.Type.Sliced;

                // Keep color white for sprite-based images (sprite has its own colors)
                if (image.color.a > 0.5f) // Don't change semi-transparent overlays
                    image.color = Color.white;

                EditorUtility.SetDirty(image);
                updatedCount++;
            }
        }

        private static void ApplyToggleSprites(GameObject go, Toggle toggle)
        {
            // Find the toggle graphic (checkmark/background)
            Image checkmark = toggle.graphic as Image;
            Image background = toggle.targetGraphic as Image;

            if (checkmark != null && guiAssets.toggleCheckOn != null)
            {
                Undo.RecordObject(checkmark, "Apply GUI Sprite");
                checkmark.sprite = guiAssets.toggleCheckOn;
                checkmark.type = Image.Type.Simple;
                EditorUtility.SetDirty(checkmark);
                updatedCount++;
            }

            if (background != null && guiAssets.toggleOff != null)
            {
                Undo.RecordObject(background, "Apply GUI Sprite");
                background.sprite = guiAssets.toggleOff;
                background.type = Image.Type.Simple;
                EditorUtility.SetDirty(background);
                updatedCount++;
            }
        }

        private static void ApplySliderSprites(GameObject go, Slider slider)
        {
            // Find slider components
            Transform backgroundTransform = go.transform.Find("Background");
            Transform fillAreaTransform = go.transform.Find("Fill Area");
            Transform handleTransform = go.transform.Find("Handle Slide Area/Handle");

            if (backgroundTransform != null)
            {
                Image bgImage = backgroundTransform.GetComponent<Image>();
                if (bgImage != null && guiAssets.sliderBackground != null)
                {
                    Undo.RecordObject(bgImage, "Apply GUI Sprite");
                    bgImage.sprite = guiAssets.sliderBackground;
                    bgImage.type = Image.Type.Sliced;
                    EditorUtility.SetDirty(bgImage);
                    updatedCount++;
                }
            }

            if (fillAreaTransform != null)
            {
                Transform fillTransform = fillAreaTransform.Find("Fill");
                if (fillTransform != null)
                {
                    Image fillImage = fillTransform.GetComponent<Image>();
                    if (fillImage != null && guiAssets.sliderFill != null)
                    {
                        Undo.RecordObject(fillImage, "Apply GUI Sprite");
                        fillImage.sprite = guiAssets.sliderFill;
                        fillImage.type = Image.Type.Sliced;
                        EditorUtility.SetDirty(fillImage);
                        updatedCount++;
                    }
                }
            }

            if (handleTransform != null)
            {
                Image handleImage = handleTransform.GetComponent<Image>();
                if (handleImage != null && guiAssets.sliderHandle != null)
                {
                    Undo.RecordObject(handleImage, "Apply GUI Sprite");
                    handleImage.sprite = guiAssets.sliderHandle;
                    handleImage.type = Image.Type.Simple;
                    EditorUtility.SetDirty(handleImage);
                    updatedCount++;
                }
            }
        }

        [MenuItem("Incredicer/Setup/Apply GUI Sprites to Selected")]
        public static void ApplySpritesToSelected()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogWarning("[ApplyGUISprites] No GameObject selected!");
                return;
            }

            // Load the GUI sprite assets
            guiAssets = AssetDatabase.LoadAssetAtPath<GUISpriteAssets>("Assets/Resources/GUISpriteAssets.asset");
            if (guiAssets == null)
            {
                Debug.LogError("[ApplyGUISprites] GUISpriteAssets not found! Run 'Incredicer/Setup/GUI Sprite Assets' first.");
                return;
            }

            updatedCount = 0;
            ProcessTransform(Selection.activeGameObject.transform);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[ApplyGUISprites] Applied GUI sprites to {updatedCount} UI elements in {Selection.activeGameObject.name}!");
        }
    }
}
