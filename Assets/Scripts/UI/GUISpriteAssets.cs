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

        [Header("Buttons")]
        public Sprite buttonGreen;
        public Sprite buttonBlue;
        public Sprite buttonYellow;
        public Sprite buttonGray;
        public Sprite buttonRed;
        public Sprite buttonPurple;

        [Header("Frames")]
        public Sprite listFrame;
        public Sprite itemFrame;
        public Sprite cardFrame;
        public Sprite horizontalFrame;

        [Header("Icons")]
        public Sprite iconClose;
        public Sprite iconLock;
        public Sprite iconStar;
        public Sprite iconCoin;
        public Sprite iconAdd;

        [Header("Labels/Ribbons")]
        public Sprite ribbonYellow;
        public Sprite ribbonGreen;
        public Sprite ribbonBlue;
        public Sprite ribbonPurple;

        [Header("Misc")]
        public Sprite sliderBackground;
        public Sprite sliderFill;
    }
}
