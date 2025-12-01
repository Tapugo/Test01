using UnityEngine;
using UnityEditor;
using Incredicer.Core;

namespace Incredicer.Editor
{
    public static class SetupAudioManager
    {
        [MenuItem("Incredicer/Setup Audio Manager")]
        public static void Execute()
        {
            // Find or create AudioManager
            AudioManager existingManager = Object.FindObjectOfType<AudioManager>();

            if (existingManager != null)
            {
                Debug.Log("[SetupAudioManager] AudioManager already exists in scene!");
                Selection.activeGameObject = existingManager.gameObject;
                return;
            }

            // Create new AudioManager
            GameObject audioManagerObj = new GameObject("AudioManager");
            AudioManager audioManager = audioManagerObj.AddComponent<AudioManager>();

            // Mark as dirty
            EditorUtility.SetDirty(audioManagerObj);

            // Select the created object
            Selection.activeGameObject = audioManagerObj;

            Debug.Log("[SetupAudioManager] AudioManager created! Assign audio clips in the inspector.");
        }
    }
}
