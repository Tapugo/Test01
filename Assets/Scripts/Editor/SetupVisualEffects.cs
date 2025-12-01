using UnityEngine;
using UnityEditor;
using Incredicer.Core;

namespace Incredicer.Editor
{
    public static class SetupVisualEffects
    {
        [MenuItem("Incredicer/Setup Visual Effects Manager")]
        public static void Execute()
        {
            // Find or create VisualEffectsManager
            VisualEffectsManager existingManager = Object.FindObjectOfType<VisualEffectsManager>();

            if (existingManager != null)
            {
                Debug.Log("[SetupVisualEffects] VisualEffectsManager already exists in scene!");
                Selection.activeGameObject = existingManager.gameObject;
                return;
            }

            // Create new VisualEffectsManager
            GameObject vfxManagerObj = new GameObject("VisualEffectsManager");
            VisualEffectsManager vfxManager = vfxManagerObj.AddComponent<VisualEffectsManager>();

            // Mark as dirty
            EditorUtility.SetDirty(vfxManagerObj);

            // Select the created object
            Selection.activeGameObject = vfxManagerObj;

            Debug.Log("[SetupVisualEffects] VisualEffectsManager created! Default particles will be created at runtime.");
        }
    }
}
