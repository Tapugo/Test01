using UnityEngine;
using UnityEditor;
using System.IO;
using Incredicer.UI;

namespace Incredicer.Editor
{
    /// <summary>
    /// Editor script to set up GUISpriteAssets with sprites from Layer Lab/GUI-CasualFantasy.
    /// </summary>
    public static class GUISpriteAssetsSetup
    {
        private const string GUI_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components";
        private const string ASSET_PATH = "Assets/Resources/GUISpriteAssets.asset";

        [MenuItem("Incredicer/Setup GUI Sprite Assets")]
        public static void SetupGUISpriteAssets()
        {
            // Ensure Resources folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            // Load or create the asset
            GUISpriteAssets asset = AssetDatabase.LoadAssetAtPath<GUISpriteAssets>(ASSET_PATH);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<GUISpriteAssets>();
                AssetDatabase.CreateAsset(asset, ASSET_PATH);
                Debug.Log("[GUISpriteAssetsSetup] Created new GUISpriteAssets");
            }

            // Assign sprites - using actual filenames from the asset pack
            // Popup backgrounds
            asset.popupBackground = LoadSprite("Popup/popup02_Demo1.png");
            asset.popupBackgroundAlt = LoadSprite("Popup/popup02_Demo2.png");

            // Buttons - using actual filenames
            asset.buttonGreen = LoadSprite("Button/Button01_Demo_Teal.png"); // Teal is like green
            asset.buttonBlue = LoadSprite("Button/Button01_Demo_Blue.png");
            asset.buttonYellow = LoadSprite("Button/Button01_Demo_Yellow.png");
            asset.buttonGray = LoadSprite("Button/Button01_Demo_Gray.png");
            asset.buttonRed = LoadSprite("Button/Button01_Demo_Red.png");
            asset.buttonPurple = LoadSprite("Button/Button01_Demo_Pink.png"); // Pink is like purple

            // Frames - using actual filenames
            asset.listFrame = LoadSprite("Frame/ItemFrame03_White1.png");
            asset.itemFrame = LoadSprite("Frame/ItemFrame01_Demo_Gray.png");
            asset.cardFrame = LoadSprite("Frame/CardFrame01_Demo_BottomBg.png");
            asset.horizontalFrame = LoadSprite("Frame/SplitFrame02_Demo.png");

            // Icons
            asset.iconClose = LoadSpriteFromPath("Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/IconMisc/Icon_X.png");
            asset.iconLock = LoadSpriteFromPath("Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/IconMisc/Icon_Lock01.png");
            asset.iconStar = LoadSpriteFromPath("Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/IconMisc/Icon_Star03_m.png");
            asset.iconAdd = LoadSpriteFromPath("Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/IconMisc/Icon_Add02.png");
            asset.iconCoin = LoadSpriteFromPath("Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Icon_ItemIcons/64/ItemIcon_Gold.png");

            // Labels/Ribbons - try to find matching files
            asset.ribbonYellow = LoadSpriteFromPath("Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Label/Label01_Demo_Yellow.png");
            asset.ribbonGreen = LoadSpriteFromPath("Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Label/Label01_Demo_Green.png");
            asset.ribbonBlue = LoadSpriteFromPath("Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Label/Label01_Demo_Sky.png");
            asset.ribbonPurple = LoadSpriteFromPath("Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Label/Label01_Demo_Pink.png");

            // Sliders
            asset.sliderBackground = LoadSpriteFromPath("Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Slider/Slider_Rect70_White_Bg.png");
            asset.sliderFill = LoadSpriteFromPath("Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Slider/Slider_Rect70_Demo_Fill.png");

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            int assignedCount = CountAssignedSprites(asset);
            Debug.Log($"[GUISpriteAssetsSetup] GUI Sprite Assets setup complete! Assigned {assignedCount} sprites.");
        }

        private static int CountAssignedSprites(GUISpriteAssets asset)
        {
            int count = 0;
            if (asset.popupBackground != null) count++;
            if (asset.popupBackgroundAlt != null) count++;
            if (asset.buttonGreen != null) count++;
            if (asset.buttonBlue != null) count++;
            if (asset.buttonYellow != null) count++;
            if (asset.buttonGray != null) count++;
            if (asset.buttonRed != null) count++;
            if (asset.buttonPurple != null) count++;
            if (asset.listFrame != null) count++;
            if (asset.itemFrame != null) count++;
            if (asset.cardFrame != null) count++;
            if (asset.horizontalFrame != null) count++;
            if (asset.iconClose != null) count++;
            if (asset.iconLock != null) count++;
            if (asset.iconStar != null) count++;
            if (asset.iconCoin != null) count++;
            if (asset.iconAdd != null) count++;
            if (asset.ribbonYellow != null) count++;
            if (asset.ribbonGreen != null) count++;
            if (asset.ribbonBlue != null) count++;
            if (asset.ribbonPurple != null) count++;
            if (asset.sliderBackground != null) count++;
            if (asset.sliderFill != null) count++;
            return count;
        }

        private static Sprite LoadSprite(string relativePath)
        {
            string fullPath = $"{GUI_PATH}/{relativePath}";
            return LoadSpriteFromPath(fullPath);
        }

        private static Sprite LoadSpriteFromPath(string fullPath)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);

            if (sprite == null)
            {
                // Try to find a similar file
                string directory = Path.GetDirectoryName(fullPath);
                string fileName = Path.GetFileNameWithoutExtension(fullPath);

                if (Directory.Exists(directory))
                {
                    string[] files = Directory.GetFiles(directory, "*.png");
                    foreach (string file in files)
                    {
                        if (Path.GetFileNameWithoutExtension(file).Contains(fileName.Substring(0, Mathf.Min(fileName.Length, 8))))
                        {
                            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(file);
                            if (sprite != null)
                            {
                                Debug.Log($"[GUISpriteAssetsSetup] Found alternative: {file}");
                                return sprite;
                            }
                        }
                    }
                }

                Debug.LogWarning($"[GUISpriteAssetsSetup] Could not load sprite: {fullPath}");
            }

            return sprite;
        }
    }
}
