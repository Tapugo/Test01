using UnityEngine;
using UnityEditor;
using Incredicer.UI;

namespace Incredicer.Editor
{
    public static class AssignCheckIcon
    {
        [MenuItem("Incredicer/Assign Check Icon to GUI Assets")]
        public static void Execute()
        {
            // Load the GUISpriteAssets
            var guiAssets = AssetDatabase.LoadAssetAtPath<GUISpriteAssets>("Assets/Resources/GUISpriteAssets.asset");
            if (guiAssets == null)
            {
                Debug.LogError("[AssignCheckIcon] Could not find GUISpriteAssets at Assets/Resources/GUISpriteAssets.asset");
                return;
            }

            // Load the check icon sprite
            var checkIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/IconMisc/Icon_Check.png");
            if (checkIcon == null)
            {
                Debug.LogError("[AssignCheckIcon] Could not find Icon_Check.png sprite");
                return;
            }

            // Assign the sprite
            guiAssets.iconCheck = checkIcon;

            // Save the changes
            EditorUtility.SetDirty(guiAssets);
            AssetDatabase.SaveAssets();

            Debug.Log("[AssignCheckIcon] Successfully assigned Icon_Check to GUISpriteAssets.iconCheck");
        }
    }
}
