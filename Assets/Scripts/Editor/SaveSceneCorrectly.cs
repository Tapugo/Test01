using UnityEditor;
using UnityEditor.SceneManagement;

namespace Incredicer.Editor
{
    public static class SaveSceneCorrectly
    {
        public static string Execute()
        {
            EditorSceneManager.SaveScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene(),
                "Assets/Scenes/Game.unity"
            );
            return "Saved scene to Assets/Scenes/Game.unity";
        }
    }
}
