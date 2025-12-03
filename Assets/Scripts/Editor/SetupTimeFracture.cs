#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Incredicer.TimeFracture;

namespace Incredicer.Editor
{
    /// <summary>
    /// Editor utility to set up the Time Fracture system in the scene.
    /// </summary>
    public class SetupTimeFracture : MonoBehaviour
    {
        [MenuItem("Incredicer/Setup Time Fracture System")]
        public static void Setup()
        {
            // Find or create Managers object
            GameObject managers = GameObject.Find("Managers");
            if (managers == null)
            {
                managers = new GameObject("Managers");
                Debug.Log("[SetupTimeFracture] Created Managers object");
            }

            // Add TimeFractureManager
            var existingManager = managers.GetComponent<TimeFractureManager>();
            if (existingManager == null)
            {
                managers.AddComponent<TimeFractureManager>();
                Debug.Log("[SetupTimeFracture] Added TimeFractureManager");
            }

            // Find GameCanvas
            GameObject canvas = GameObject.Find("GameCanvas");
            if (canvas == null)
            {
                Debug.LogWarning("[SetupTimeFracture] No GameCanvas found! Create one first.");
                return;
            }

            // Add TimeFractureUI to canvas
            var existingUI = canvas.GetComponent<TimeFractureUI>();
            if (existingUI == null)
            {
                existingUI = canvas.AddComponent<TimeFractureUI>();
                Debug.Log("[SetupTimeFracture] Added TimeFractureUI to GameCanvas");
            }

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[SetupTimeFracture] Time Fracture system setup complete!");
        }
    }
}
#endif
