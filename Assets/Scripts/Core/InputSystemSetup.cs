using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Incredicer.Core
{
    /// <summary>
    /// Ensures the EventSystem is properly configured for the new Input System.
    /// This script automatically replaces StandaloneInputModule with InputSystemUIInputModule
    /// when the game starts, ensuring UI buttons work with the new Input System.
    /// </summary>
    public class InputSystemSetup : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeScene()
        {
            Debug.Log("[InputSystemSetup] Initializing before scene load...");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            Debug.Log("[InputSystemSetup] Checking EventSystem configuration...");

            // Check if EventSystem exists
            var eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                Debug.LogWarning("[InputSystemSetup] No EventSystem found in scene! Creating one...");
                var go = new GameObject("EventSystem");
                eventSystem = go.AddComponent<EventSystem>();
            }

            // Check for any existing input modules
            var standaloneModule = eventSystem.GetComponent<StandaloneInputModule>();
            var inputSystemModule = eventSystem.GetComponent<InputSystemUIInputModule>();

            // Log current state
            Debug.Log($"[InputSystemSetup] Found StandaloneInputModule: {standaloneModule != null}, InputSystemUIInputModule: {inputSystemModule != null}");

            // Remove StandaloneInputModule if present
            if (standaloneModule != null)
            {
                Debug.Log("[InputSystemSetup] Destroying StandaloneInputModule...");
                DestroyImmediate(standaloneModule);
            }

            // Add InputSystemUIInputModule if not present
            if (inputSystemModule == null)
            {
                Debug.Log("[InputSystemSetup] Adding InputSystemUIInputModule...");
                inputSystemModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }

            // Force enable and activate
            if (inputSystemModule != null)
            {
                inputSystemModule.enabled = true;
                Debug.Log($"[InputSystemSetup] InputSystemUIInputModule configured and enabled on {eventSystem.gameObject.name}");
            }

            // Verify the EventSystem is working
            Debug.Log($"[InputSystemSetup] EventSystem.current: {EventSystem.current?.name ?? "NULL"}");
            Debug.Log($"[InputSystemSetup] EventSystem enabled: {eventSystem.enabled}");
            Debug.Log($"[InputSystemSetup] Current input module: {eventSystem.currentInputModule?.GetType().Name ?? "NULL"}");
        }
    }
}
