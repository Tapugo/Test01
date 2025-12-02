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

            // Assign sprites
            // Popup backgrounds
            asset.popupBackground = LoadSprite("Popup/popup02_Demo1.png");
            asset.popupBackgroundAlt = LoadSprite("Popup/popup02_Demo2.png");

            // Buttons
            asset.buttonGreen = LoadSprite("Button/Button01_Demo_Green.png");
            asset.buttonBlue = LoadSprite("Button/Button01_Demo_Blue.png");
            asset.buttonYellow = LoadSprite("Button/Button01_Demo_Yellow.png");
            asset.buttonGray = LoadSprite("Button/Button01_Demo_Gray.png");
            asset.buttonRed = LoadSprite("Button/Button01_Demo_Red.png");
            asset.buttonPurple = LoadSprite("Button/Button01_Demo_Purple.png");

            // Frames
            asset.listFrame = LoadSprite("Frame/ListFrame01_White2.png");
            asset.itemFrame = LoadSprite("Frame/ItemFrame03_Demo.png");
            asset.cardFrame = LoadSprite("Frame/CardFrame01_White1.png");
            asset.horizontalFrame = LoadSprite("Frame/HorizontalFrame01_Demo.png");

            // Icons
            asset.iconClose = LoadSprite("IconMisc/Icon_Add03.png"); // X-like icon
            asset.iconLock = LoadSprite("IconMisc/Icon_Lock01.png");
            asset.iconStar = LoadSprite("IconMisc/Icon_Star03_m.png");
            asset.iconAdd = LoadSprite("IconMisc/Icon_Add03.png");

            // Try to find coin icon from item icons
            asset.iconCoin = LoadSprite("Icon_ItemIcons/128/ItemIcon_Star.png");

            // Labels/Ribbons
            asset.ribbonYellow = LoadSprite("Label/Label_Ribbon01_Yellow.png");
            asset.ribbonGreen = LoadSprite("Label/Label_Ribbon01_Green.png");
            asset.ribbonBlue = LoadSprite("Label/Label_Ribbon01_Blue.png");
            asset.ribbonPurple = LoadSprite("Label/Label_Ribbon01_Magenta.png");

            // Sliders
            asset.sliderBackground = LoadSprite("Slider/Slider_Diagonal70_White_Bg.png");
            asset.sliderFill = LoadSprite("Slider/Slider_Diagonal70_Demo_Icon02.png");

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[GUISpriteAssetsSetup] GUI Sprite Assets setup complete!");
        }

        private static Sprite LoadSprite(string relativePath)
        {
            string fullPath = $"{GUI_PATH}/{relativePath}";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);

            if (sprite == null)
            {
                Debug.LogWarning($"[GUISpriteAssetsSetup] Could not load sprite: {fullPath}");
            }

            return sprite;
        }
    }
}
