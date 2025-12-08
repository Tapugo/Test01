using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Incredicer.Editor
{
    /// <summary>
    /// Automatically sets up skill nodes and GUI assets when scripts are recompiled.
    /// </summary>
    [InitializeOnLoad]
    public static class AutoSetupSkillNodes
    {
        static AutoSetupSkillNodes()
        {
            // Run setup after a short delay to ensure everything is loaded
            EditorApplication.delayCall += RunSetup;
        }

        private static void RunSetup()
        {
            // Delete the old SK_DailyLogin node (moved to Feature Unlocks)
            string oldDailyLoginPath = "Assets/Data/SkillNodes/SK_DailyLogin.asset";
            if (AssetDatabase.LoadAssetAtPath<Object>(oldDailyLoginPath) != null)
            {
                Debug.Log("[AutoSetup] Removing old SK_DailyLogin asset (moved to Feature Unlocks)...");
                AssetDatabase.DeleteAsset(oldDailyLoginPath);
            }

            // Always run skill node creation to ensure updates are applied
            Debug.Log("[AutoSetup] Running skill node creation...");
            CreateSkillNodes.Execute();

            // Setup GUI sprite assets
            Debug.Log("[AutoSetup] Running GUI sprite assets setup...");
            GUISpriteAssetsSetup.SetupGUISpriteAssets();
        }
    }
}
