using UnityEngine;
using UnityEditor;
using System.IO;

namespace Incredicer.Editor
{
    public static class ClearSaveData
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "incredicer_save.json");

        [MenuItem("Incredicer/Clear Save Data")]
        public static void Execute()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log($"[ClearSaveData] Save file deleted: {SavePath}");
            }
            else
            {
                Debug.Log("[ClearSaveData] No save file found");
            }
        }

        [MenuItem("Incredicer/Open Save Folder")]
        public static void OpenSaveFolder()
        {
            string folder = Application.persistentDataPath;
            EditorUtility.RevealInFinder(folder);
        }
    }
}
