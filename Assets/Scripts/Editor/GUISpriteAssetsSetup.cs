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
        private const string DEMO_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Demo";
        private const string ASSET_PATH = "Assets/Resources/GUISpriteAssets.asset";

        [MenuItem("Incredicer/Setup/GUI Sprite Assets")]
        public static void SetupGUISpriteAssets()
        {
            // Check if GUI-CasualFantasy exists
            if (!Directory.Exists(Application.dataPath + "/Layer Lab/GUI-CasualFantasy"))
            {
                Debug.LogError("[GUISpriteAssetsSetup] GUI-CasualFantasy package not found! Please import it first.");
                return;
            }

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

            int assignedCount = 0;

            // ===== POPUP/PANEL BACKGROUNDS =====
            assignedCount += AssignSprite(ref asset.popupBackground, "Popup/Popup01_Demo.png");
            assignedCount += AssignSprite(ref asset.popupBackgroundAlt, "Popup/popup02_Demo1.png");
            assignedCount += AssignSprite(ref asset.slidePopup, "Popup/SlidePopup_Demo.png");
            assignedCount += AssignSpriteFromDemo(ref asset.panelDimmed, "Demo_Background/Panel_Dimed.png");

            // ===== BUTTONS - PRIMARY =====
            assignedCount += AssignSprite(ref asset.buttonGreen, "Button/Button01_Demo_Teal.png");
            assignedCount += AssignSprite(ref asset.buttonBlue, "Button/Button01_Demo_Blue.png");
            assignedCount += AssignSprite(ref asset.buttonYellow, "Button/Button01_Demo_Yellow.png");
            assignedCount += AssignSprite(ref asset.buttonGray, "Button/Button01_Demo_Gray.png");
            assignedCount += AssignSprite(ref asset.buttonRed, "Button/Button01_Demo_Red.png");
            assignedCount += AssignSprite(ref asset.buttonPurple, "Button/Button01_Demo_Rose.png");
            assignedCount += AssignSprite(ref asset.buttonPink, "Button/Button01_Demo_Pink.png");
            assignedCount += AssignSprite(ref asset.buttonBlack, "Button/Button01_Demo_Black.png");
            assignedCount += AssignSprite(ref asset.buttonSky, "Button/Button01_Demo_Sky.png");

            // ===== BUTTONS - SQUARE =====
            assignedCount += AssignSprite(ref asset.buttonSquare, "Button/Button_Square02_Demo.png");
            assignedCount += AssignSprite(ref asset.buttonSquareOutline, "Button/Button_SquareLIne01.png");
            assignedCount += AssignSprite(ref asset.buttonSquareSolid, "Button/Button_SquareSolid01_Demo.png");
            assignedCount += AssignSprite(ref asset.buttonCircle, "Button/Button_Circle01.png");

            // ===== FRAMES - ITEM =====
            assignedCount += AssignSprite(ref asset.itemFrameGray, "Frame/ItemFrame01_Demo_Gray.png");
            assignedCount += AssignSprite(ref asset.itemFrameGreen, "Frame/ItemFrame01_Demo_Green.png");
            assignedCount += AssignSprite(ref asset.itemFrameBlue, "Frame/ItemFrame01_Demo_Sky.png");
            assignedCount += AssignSprite(ref asset.itemFrameYellow, "Frame/ItemFrame01_Demo_Yellow.png");
            assignedCount += AssignSprite(ref asset.itemFrameRed, "Frame/ItemFrame01_Demo_Red.png");
            assignedCount += AssignSprite(ref asset.itemFramePurple, "Frame/ItemFrame01_Demo_Teal.png");
            assignedCount += AssignSprite(ref asset.itemFrameSky, "Frame/ItemFrame01_Demo_Sky.png");

            // ===== FRAMES - BASIC =====
            assignedCount += AssignSprite(ref asset.frameSquareSolid, "Frame/BasicFrame_SquareSolid01_Demo01.png");
            assignedCount += AssignSprite(ref asset.frameSquareOutline, "Frame/BasicFrame_SquareOutline02_Demo01.png");
            assignedCount += AssignSprite(ref asset.frameOctagon, "Frame/BasicFrame_Octagon01_Demo.png");
            assignedCount += AssignSprite(ref asset.frameCircle, "Frame/BasicFrame_CircleOutline01_Demo_Gray.png");

            // ===== FRAMES - CARD =====
            assignedCount += AssignSprite(ref asset.cardFrame, "Frame/CardFrame01_Demo_BottomBg.png");
            assignedCount += AssignSprite(ref asset.cardFrameBottom, "Frame/CardFrame01_Demo_BottomBg.png");
            assignedCount += AssignSprite(ref asset.splitFrame, "Frame/SplitFrame02_Demo.png");

            // ===== ICONS - NAVIGATION =====
            assignedCount += AssignSprite(ref asset.iconClose, "IconMisc/Icon_Close01.png");
            assignedCount += AssignSprite(ref asset.iconBack, "IconMisc/Icon_Arrow_Back.png");
            assignedCount += AssignSprite(ref asset.iconMenu, "IconMisc/Icon_HamburgerMenu.png");
            assignedCount += AssignSpriteSearch(ref asset.iconSettings, "IconMisc", "Setting");
            assignedCount += AssignSpriteSearch(ref asset.iconAdd, "IconMisc", "Add");

            // ===== ICONS - STATUS =====
            assignedCount += AssignSprite(ref asset.iconLock, "IconMisc/Icon_Lock01.png");
            assignedCount += AssignSpriteSearch(ref asset.iconCheck, "UI_Etc", "Chenk");
            assignedCount += AssignSprite(ref asset.iconStar, "IconMisc/Icon_Star01_s.png");
            assignedCount += AssignSprite(ref asset.iconHeart, "IconMisc/Icon_Heart01.png");
            assignedCount += AssignSprite(ref asset.iconTrophy, "IconMisc/Icon_Trophy_s.png");
            assignedCount += AssignSprite(ref asset.iconFire, "IconMisc/Icon_Fire01_512.png");

            // ===== ICONS - CURRENCY =====
            assignedCount += AssignSprite(ref asset.iconCoin, "IconMisc/Icon_Gold.png");
            assignedCount += AssignSprite(ref asset.iconGem, "UI_Etc/Statusbar_Demo_Icon_Gem.png");
            assignedCount += AssignSprite(ref asset.iconEnergy, "UI_Etc/Statusbar_Demo_Icon_Energy.png");

            // ===== ICONS - GAME =====
            assignedCount += AssignSprite(ref asset.iconSword, "IconMisc/Icon_Sword01_512.png");
            assignedCount += AssignSprite(ref asset.iconShield, "IconMisc/Icon_Shield01.png");
            assignedCount += AssignSprite(ref asset.iconHelmet, "IconMisc/Icon_Helmet02_256.png");
            assignedCount += AssignSprite(ref asset.iconBoots, "IconMisc/Icon_Boots02_512.png");

            // ===== TOGGLES & SWITCHES =====
            assignedCount += AssignSprite(ref asset.toggleOn, "UI_Etc/Toggle01_White_On.png");
            assignedCount += AssignSprite(ref asset.toggleOff, "UI_Etc/Toggle01_White_Off.png");
            assignedCount += AssignSprite(ref asset.toggleCheckOn, "UI_Etc/Toggle01_Demo_ChenkIcon_Green.png");
            assignedCount += AssignSprite(ref asset.toggleCheckOff, "UI_Etc/Toggle01_White_ChenkIcon.png");
            assignedCount += AssignSprite(ref asset.switchOn, "UI_Etc/Switch01_Demo_Bg_On.png");
            assignedCount += AssignSprite(ref asset.switchOff, "UI_Etc/Switch01_Demo_Bg_Off.png");
            assignedCount += AssignSprite(ref asset.switchButton, "UI_Etc/Switch01_Demo_Button_On.png");

            // ===== LABELS/RIBBONS =====
            assignedCount += AssignSpriteSearch(ref asset.ribbonYellow, "Label", "Yellow");
            assignedCount += AssignSpriteSearch(ref asset.ribbonGreen, "Label", "Green");
            assignedCount += AssignSpriteSearch(ref asset.ribbonBlue, "Label", "Sky");
            assignedCount += AssignSpriteSearch(ref asset.ribbonPurple, "Label", "Pink");
            assignedCount += AssignSpriteSearch(ref asset.ribbonPink, "Label", "Rose");
            assignedCount += AssignSpriteSearch(ref asset.ribbonRed, "Label", "Red");

            // ===== ALERTS/BADGES =====
            assignedCount += AssignSprite(ref asset.alertDotRed, "UI_Etc/Alert_Dot_Red.png");
            assignedCount += AssignSprite(ref asset.alertDotWhite, "UI_Etc/Alert_Dot_White.png");
            assignedCount += AssignSprite(ref asset.alertCountRed, "UI_Etc/Alert_Count_Red.png");
            assignedCount += AssignSprite(ref asset.alertCountGreen, "UI_Etc/Alert_Count_Green.png");
            assignedCount += AssignSprite(ref asset.alertTextYellow, "UI_Etc/Alert_Text_Yellow.png");
            assignedCount += AssignSprite(ref asset.alertTextRed, "UI_Etc/Alert_Text_Red.png");

            // ===== SLIDERS =====
            assignedCount += AssignSpriteSearch(ref asset.sliderBackground, "Slider", "Bg");
            assignedCount += AssignSpriteSearch(ref asset.sliderFill, "Slider", "Fill");
            assignedCount += AssignSpriteSearch(ref asset.sliderFillGreen, "Slider", "Green");
            assignedCount += AssignSpriteSearch(ref asset.sliderFillBlue, "Slider", "Blue");
            assignedCount += AssignSpriteSearch(ref asset.sliderFillYellow, "Slider", "Yellow");
            assignedCount += AssignSpriteSearch(ref asset.sliderFillRed, "Slider", "Red");
            assignedCount += AssignSpriteSearch(ref asset.sliderHandle, "Slider", "Handle");

            // ===== STATUS BAR =====
            assignedCount += AssignSprite(ref asset.statusBarBackground, "UI_Etc/Statusbar_Demo_Bg.png");
            assignedCount += AssignSprite(ref asset.statusBarButton, "UI_Etc/Statusbar_Demo_Button.png");

            // ===== TAB MENU =====
            assignedCount += AssignSprite(ref asset.tabMenuBackground, "UI_Etc/TabMenu_Top_Demo_Bg.png");
            assignedCount += AssignSprite(ref asset.tabMenuFocus, "UI_Etc/TabMenu_Top_Demo_Focus.png");

            // ===== DECORATIONS =====
            assignedCount += AssignSpriteFromDemo(ref asset.decorGlow, "Demo_Image/Glow01.png");
            assignedCount += AssignSpriteFromDemo(ref asset.decorLeaf, "Demo_Image/Image_Leaf.png");
            assignedCount += AssignSpriteFromDemo(ref asset.decorCrown, "Demo_Image/Image_Crown.png");
            assignedCount += AssignSpriteFromDemo(ref asset.decorLight, "Demo_Image/Image_Light.png");

            // ===== LEGACY/COMPATIBILITY =====
            assignedCount += AssignSprite(ref asset.listFrame, "Frame/ItemFrame03_White1.png");
            assignedCount += AssignSprite(ref asset.itemFrame, "Frame/ItemFrame01_Demo_Gray.png");
            assignedCount += AssignSprite(ref asset.horizontalFrame, "Frame/SplitFrame01_Demo2.png");

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GUISpriteAssetsSetup] GUI Sprite Assets setup complete! Assigned {assignedCount} sprites.");
        }

        private static int AssignSprite(ref Sprite field, string relativePath)
        {
            string fullPath = $"{GUI_PATH}/{relativePath}";
            Sprite sprite = LoadSpriteFromPath(fullPath);
            if (sprite != null)
            {
                field = sprite;
                return 1;
            }
            return 0;
        }

        private static int AssignSpriteFromDemo(ref Sprite field, string relativePath)
        {
            string fullPath = $"{DEMO_PATH}/{relativePath}";
            Sprite sprite = LoadSpriteFromPath(fullPath);
            if (sprite != null)
            {
                field = sprite;
                return 1;
            }
            return 0;
        }

        private static int AssignSpriteSearch(ref Sprite field, string folder, string searchTerm)
        {
            string directory = $"{GUI_PATH}/{folder}";
            if (!Directory.Exists(directory))
            {
                Debug.LogWarning($"[GUISpriteAssetsSetup] Directory not found: {directory}");
                return 0;
            }

            string[] files = Directory.GetFiles(directory, "*.png");
            foreach (string file in files)
            {
                if (Path.GetFileName(file).ToLower().Contains(searchTerm.ToLower()))
                {
                    string assetPath = file.Replace(Application.dataPath, "Assets");
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    if (sprite != null)
                    {
                        field = sprite;
                        return 1;
                    }
                }
            }

            Debug.LogWarning($"[GUISpriteAssetsSetup] No sprite found in {folder} matching '{searchTerm}'");
            return 0;
        }

        private static Sprite LoadSpriteFromPath(string fullPath)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);

            if (sprite == null)
            {
                // Try case-insensitive search
                string directory = Path.GetDirectoryName(fullPath);
                string fileName = Path.GetFileName(fullPath).ToLower();

                if (Directory.Exists(directory))
                {
                    string[] files = Directory.GetFiles(directory, "*.png");
                    foreach (string file in files)
                    {
                        if (Path.GetFileName(file).ToLower() == fileName)
                        {
                            string assetPath = file.Replace(Application.dataPath, "Assets");
                            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                            if (sprite != null)
                            {
                                return sprite;
                            }
                        }
                    }
                }
            }

            return sprite;
        }
    }
}
