using UnityEngine;

namespace Incredicer.UI
{
    /// <summary>
    /// ScriptableObject that holds references to GUI sprite assets from Layer Lab/GUI-CasualFantasy.
    /// Assign sprites in the Unity Editor to use them in UI components.
    /// </summary>
    [CreateAssetMenu(fileName = "GUISpriteAssets", menuName = "Incredicer/GUI Sprite Assets")]
    public class GUISpriteAssets : ScriptableObject
    {
        private static GUISpriteAssets _instance;
        public static GUISpriteAssets Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GUISpriteAssets>("GUISpriteAssets");
                    if (_instance == null)
                    {
                        Debug.LogWarning("[GUISpriteAssets] No GUISpriteAssets found in Resources folder. Using default colors.");
                    }
                }
                return _instance;
            }
        }

        [Header("Popup/Panel Backgrounds")]
        public Sprite popupBackground;
        public Sprite popupBackgroundAlt;
        public Sprite slidePopup;
        public Sprite panelDimmed;

        [Header("Buttons - Primary")]
        public Sprite buttonGreen;
        public Sprite buttonBlue;
        public Sprite buttonYellow;
        public Sprite buttonGray;
        public Sprite buttonRed;
        public Sprite buttonPurple;
        public Sprite buttonPink;
        public Sprite buttonBlack;
        public Sprite buttonSky;

        [Header("Buttons - Square")]
        public Sprite buttonSquare;
        public Sprite buttonSquareOutline;
        public Sprite buttonSquareSolid;
        public Sprite buttonCircle;

        [Header("Frames - Item")]
        public Sprite itemFrameGray;
        public Sprite itemFrameGreen;
        public Sprite itemFrameBlue;
        public Sprite itemFrameYellow;
        public Sprite itemFrameRed;
        public Sprite itemFramePurple;
        public Sprite itemFrameSky;

        [Header("Frames - Basic")]
        public Sprite frameSquareSolid;
        public Sprite frameSquareOutline;
        public Sprite frameOctagon;
        public Sprite frameCircle;

        [Header("Frames - Card")]
        public Sprite cardFrame;
        public Sprite cardFrameBottom;
        public Sprite splitFrame;

        [Header("Icons - Navigation")]
        public Sprite iconClose;
        public Sprite iconBack;
        public Sprite iconMenu;
        public Sprite iconSettings;
        public Sprite iconAdd;

        [Header("Icons - Status")]
        public Sprite iconLock;
        public Sprite iconCheck;
        public Sprite iconStar;
        public Sprite iconHeart;
        public Sprite iconTrophy;
        public Sprite iconFire;

        [Header("Icons - Currency")]
        public Sprite iconCoin;
        public Sprite iconGem;
        public Sprite iconEnergy;

        [Header("Icons - Game")]
        public Sprite iconSword;
        public Sprite iconShield;
        public Sprite iconHelmet;
        public Sprite iconBoots;

        [Header("Toggles & Switches")]
        public Sprite toggleOn;
        public Sprite toggleOff;
        public Sprite toggleCheckOn;
        public Sprite toggleCheckOff;
        public Sprite switchOn;
        public Sprite switchOff;
        public Sprite switchButton;

        [Header("Labels/Ribbons")]
        public Sprite ribbonYellow;
        public Sprite ribbonGreen;
        public Sprite ribbonBlue;
        public Sprite ribbonPurple;
        public Sprite ribbonPink;
        public Sprite ribbonRed;

        [Header("Alerts/Badges")]
        public Sprite alertDotRed;
        public Sprite alertDotWhite;
        public Sprite alertCountRed;
        public Sprite alertCountGreen;
        public Sprite alertTextYellow;
        public Sprite alertTextRed;

        [Header("Sliders")]
        public Sprite sliderBackground;
        public Sprite sliderFill;
        public Sprite sliderFillGreen;
        public Sprite sliderFillBlue;
        public Sprite sliderFillYellow;
        public Sprite sliderFillRed;
        public Sprite sliderHandle;

        [Header("Status Bar")]
        public Sprite statusBarBackground;
        public Sprite statusBarButton;

        [Header("Tab Menu")]
        public Sprite tabMenuBackground;
        public Sprite tabMenuFocus;

        [Header("Decorations")]
        public Sprite decorGlow;
        public Sprite decorLeaf;
        public Sprite decorCrown;
        public Sprite decorLight;

        [Header("Legacy/Compatibility")]
        public Sprite listFrame;
        public Sprite itemFrame;
        public Sprite horizontalFrame;
    }
}
