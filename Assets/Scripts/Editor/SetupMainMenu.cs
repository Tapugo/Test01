#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Incredicer.UI;
using Incredicer.DailyLogin;
using Incredicer.Missions;
using Incredicer.Overclock;
using Incredicer.TimeFracture;
using Incredicer.Milestones;
using Incredicer.GlobalEvents;
using Incredicer.Leaderboards;

namespace Incredicer.Editor
{
    /// <summary>
    /// Editor utility to set up the complete meta retention UI system.
    /// This adds all necessary UI components and managers to the scene.
    /// </summary>
    public class SetupMainMenu : MonoBehaviour
    {
        [MenuItem("Incredicer/Setup All Meta Systems")]
        public static void SetupAll()
        {
            Debug.Log("[SetupMainMenu] Setting up all meta retention systems...");

            // Run all individual setup scripts
            SetupDailyLogin.Setup();
            SetupMissions.Setup();
            SetupMilestones.Setup();
            SetupGlobalEvents.Setup();
            SetupLeaderboards.Setup();

            // Setup Main Menu UI
            SetupMainMenuUI();

            Debug.Log("[SetupMainMenu] All meta systems setup complete!");
        }

        [MenuItem("Incredicer/Setup Main Menu UI Only")]
        public static void SetupMainMenuUI()
        {
            // Find GameCanvas
            GameObject canvas = GameObject.Find("GameCanvas");
            if (canvas == null)
            {
                Debug.LogWarning("[SetupMainMenu] No GameCanvas found! Create one first.");
                return;
            }

            // Add MainMenuUI
            var existingUI = canvas.GetComponent<MainMenuUI>();
            if (existingUI == null)
            {
                canvas.AddComponent<MainMenuUI>();
                Debug.Log("[SetupMainMenu] Added MainMenuUI to GameCanvas");
            }

            // Ensure Overclock components are present
            SetupOverclock();

            // Ensure TimeFracture components are present
            SetupTimeFracture();

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[SetupMainMenu] Main Menu UI setup complete!");
        }

        private static void SetupOverclock()
        {
            // Find or create Managers object
            GameObject managers = GameObject.Find("Managers");
            if (managers == null)
            {
                managers = new GameObject("Managers");
            }

            // Add OverclockManager
            if (managers.GetComponent<OverclockManager>() == null)
            {
                managers.AddComponent<OverclockManager>();
                Debug.Log("[SetupMainMenu] Added OverclockManager");
            }

            // Find GameCanvas
            GameObject canvas = GameObject.Find("GameCanvas");
            if (canvas != null && canvas.GetComponent<OverclockUI>() == null)
            {
                canvas.AddComponent<OverclockUI>();
                Debug.Log("[SetupMainMenu] Added OverclockUI to GameCanvas");
            }
        }

        private static void SetupTimeFracture()
        {
            // Find or create Managers object
            GameObject managers = GameObject.Find("Managers");
            if (managers == null)
            {
                managers = new GameObject("Managers");
            }

            // Add TimeFractureManager
            if (managers.GetComponent<TimeFractureManager>() == null)
            {
                managers.AddComponent<TimeFractureManager>();
                Debug.Log("[SetupMainMenu] Added TimeFractureManager");
            }

            // Find GameCanvas
            GameObject canvas = GameObject.Find("GameCanvas");
            if (canvas != null && canvas.GetComponent<TimeFractureUI>() == null)
            {
                canvas.AddComponent<TimeFractureUI>();
                Debug.Log("[SetupMainMenu] Added TimeFractureUI to GameCanvas");
            }
        }
    }
}
#endif
