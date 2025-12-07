using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Editor script to fix EventSystem for the new Input System.
/// Run via menu: Tools > Fix EventSystem for Input System
/// </summary>
public class FixEventSystem
{
    [MenuItem("Tools/Fix EventSystem for Input System")]
    public static void Fix()
    {
        // Find EventSystem in scene
        EventSystem eventSystem = Object.FindObjectOfType<EventSystem>();

        if (eventSystem == null)
        {
            Debug.LogError("No EventSystem found in scene!");
            return;
        }

        // Remove old StandaloneInputModule if present
        StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (oldModule != null)
        {
            Undo.DestroyObjectImmediate(oldModule);
            Debug.Log("Removed StandaloneInputModule");
        }

        // Add new InputSystemUIInputModule if not present
        InputSystemUIInputModule newModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (newModule == null)
        {
            Undo.AddComponent<InputSystemUIInputModule>(eventSystem.gameObject);
            Debug.Log("Added InputSystemUIInputModule");
        }
        else
        {
            Debug.Log("InputSystemUIInputModule already present");
        }

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(eventSystem.gameObject.scene);

        Debug.Log("EventSystem fixed for new Input System!");
    }
}
