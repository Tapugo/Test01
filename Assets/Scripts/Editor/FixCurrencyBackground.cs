using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace Incredicer.Editor
{
    public static class FixCurrencyBackground
    {
        [MenuItem("Incredicer/Fix Currency Background")]
        public static void Execute()
        {
            GameObject bg = GameObject.Find("CurrencyPanel/Background");
            if (bg == null)
            {
                Debug.LogError("Background not found!");
                return;
            }

            Image img = bg.GetComponent<Image>();
            if (img == null)
            {
                Debug.LogError("Image component not found!");
                return;
            }

            // Use Unity's built-in background sprite
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            img.type = Image.Type.Sliced;
            img.color = new Color(0.12f, 0.12f, 0.18f, 0.95f);

            EditorUtility.SetDirty(bg);
            Debug.Log("[FixCurrencyBackground] Background fixed!");
        }
    }
}
