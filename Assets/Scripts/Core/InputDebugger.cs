using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Incredicer.Core
{
    /// <summary>
    /// Debug script to diagnose input issues.
    /// Logs all clicks/touches and what UI elements are under them.
    /// </summary>
    public class InputDebugger : MonoBehaviour
    {
        private static InputDebugger instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (instance == null)
            {
                var go = new GameObject("InputDebugger");
                instance = go.AddComponent<InputDebugger>();
                DontDestroyOnLoad(go);
                Debug.Log("[InputDebugger] Created and initialized");
            }
        }

        private void Update()
        {
            // Check for mouse click
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                Vector2 pos = mouse.position.ReadValue();
                LogClickInfo(pos, "Mouse");
            }

            // Check for touch
            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            {
                Vector2 pos = touchscreen.primaryTouch.position.ReadValue();
                LogClickInfo(pos, "Touch");
            }
        }

        private void LogClickInfo(Vector2 screenPosition, string inputType)
        {
            Debug.Log($"[InputDebugger] {inputType} at screen position: {screenPosition}");

            // Check EventSystem
            if (EventSystem.current == null)
            {
                Debug.LogError("[InputDebugger] EventSystem.current is NULL!");
                return;
            }

            // Check current input module
            var inputModule = EventSystem.current.currentInputModule;
            Debug.Log($"[InputDebugger] Current input module: {(inputModule != null ? inputModule.GetType().Name : "NULL")}");

            // Check what's under the pointer
            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count == 0)
            {
                Debug.LogWarning("[InputDebugger] No UI elements found under click position!");
            }
            else
            {
                Debug.Log($"[InputDebugger] Found {results.Count} UI elements under click:");
                foreach (var result in results)
                {
                    Debug.Log($"  - {result.gameObject.name} (depth: {result.depth}, sortingLayer: {result.sortingLayer})");
                }
            }

            // Check if pointer is over a game object
            bool isOverUI = EventSystem.current.IsPointerOverGameObject();
            Debug.Log($"[InputDebugger] IsPointerOverGameObject: {isOverUI}");
        }
    }
}
