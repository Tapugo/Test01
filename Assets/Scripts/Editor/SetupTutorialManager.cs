using UnityEngine;
using UnityEditor;

namespace Incredicer.Editor
{
    /// <summary>
    /// Editor utility to set up the TutorialManager in the scene.
    /// </summary>
    public static class SetupTutorialManager
    {
        [MenuItem("Incredicer/Setup/Add Tutorial Manager")]
        public static void AddTutorialManager()
        {
            // Check if TutorialManager already exists
            var existing = Object.FindObjectOfType<UI.TutorialManager>();
            if (existing != null)
            {
                Debug.Log("[SetupTutorialManager] TutorialManager already exists in scene");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            // Create TutorialManager GameObject
            GameObject tutorialObj = new GameObject("TutorialManager");
            tutorialObj.AddComponent<UI.TutorialManager>();

            // Register undo
            Undo.RegisterCreatedObjectUndo(tutorialObj, "Create TutorialManager");

            Selection.activeGameObject = tutorialObj;
            Debug.Log("[SetupTutorialManager] TutorialManager created successfully");
        }

        [MenuItem("Incredicer/Tutorial/Reset Tutorial (Test)")]
        public static void ResetTutorial()
        {
            PlayerPrefs.DeleteKey("TutorialCompleted");
            PlayerPrefs.Save();
            Debug.Log("[SetupTutorialManager] Tutorial reset - will show again on next game start");
        }
    }
}
