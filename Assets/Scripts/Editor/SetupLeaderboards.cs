#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Incredicer.Leaderboards;

namespace Incredicer.Editor
{
    /// <summary>
    /// Editor utility to set up the Leaderboard system in the scene.
    /// </summary>
    public class SetupLeaderboards : MonoBehaviour
    {
        [MenuItem("Incredicer/Setup Leaderboards System")]
        public static void Setup()
        {
            // Find or create Managers object
            GameObject managers = GameObject.Find("Managers");
            if (managers == null)
            {
                managers = new GameObject("Managers");
                Debug.Log("[SetupLeaderboards] Created Managers object");
            }

            // Add LeaderboardManager
            var existingManager = managers.GetComponent<LeaderboardManager>();
            if (existingManager == null)
            {
                managers.AddComponent<LeaderboardManager>();
                Debug.Log("[SetupLeaderboards] Added LeaderboardManager");
            }

            // Find GameCanvas
            GameObject canvas = GameObject.Find("GameCanvas");
            if (canvas == null)
            {
                Debug.LogWarning("[SetupLeaderboards] No GameCanvas found! Create one first.");
                return;
            }

            // Add LeaderboardUI to canvas
            var existingUI = canvas.GetComponent<LeaderboardUI>();
            if (existingUI == null)
            {
                existingUI = canvas.AddComponent<LeaderboardUI>();
                Debug.Log("[SetupLeaderboards] Added LeaderboardUI to GameCanvas");
            }

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[SetupLeaderboards] Leaderboards system setup complete!");
        }
    }
}
#endif
