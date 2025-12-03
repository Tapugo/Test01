#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Incredicer.Overclock;

namespace Incredicer.Editor
{
    /// <summary>
    /// Editor utility to set up the Overclock system in the scene.
    /// </summary>
    public class SetupOverclock : MonoBehaviour
    {
        [MenuItem("Incredicer/Setup Overclock System")]
        public static void Setup()
        {
            // Find or create Managers object
            GameObject managers = GameObject.Find("Managers");
            if (managers == null)
            {
                managers = new GameObject("Managers");
                Debug.Log("[SetupOverclock] Created Managers object");
            }

            // Add OverclockManager
            var existingManager = managers.GetComponent<OverclockManager>();
            if (existingManager == null)
            {
                managers.AddComponent<OverclockManager>();
                Debug.Log("[SetupOverclock] Added OverclockManager");
            }

            // Find GameCanvas
            GameObject canvas = GameObject.Find("GameCanvas");
            if (canvas == null)
            {
                Debug.LogWarning("[SetupOverclock] No GameCanvas found! Create one first.");
                return;
            }

            // Add OverclockUI to canvas
            var existingUI = canvas.GetComponent<OverclockUI>();
            if (existingUI == null)
            {
                existingUI = canvas.AddComponent<OverclockUI>();
                Debug.Log("[SetupOverclock] Added OverclockUI to GameCanvas");
            }

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[SetupOverclock] Overclock system setup complete!");
        }
    }
}
#endif
