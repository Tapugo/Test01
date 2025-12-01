using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Incredicer.Core;
using Incredicer.Dice;

namespace Incredicer.Editor
{
    public static class SetupDiceManager
    {
        public static string Execute()
        {
            // Find DiceManager in scene
            DiceManager diceManager = GameObject.FindObjectOfType<DiceManager>();
            if (diceManager == null)
            {
                return "Error: DiceManager not found in scene!";
            }

            // Load all dice data assets
            List<DiceData> allDiceData = new List<DiceData>();
            string[] guids = AssetDatabase.FindAssets("t:DiceData", new[] { "Assets/ScriptableObjects/Dice" });
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DiceData data = AssetDatabase.LoadAssetAtPath<DiceData>(path);
                if (data != null)
                {
                    allDiceData.Add(data);
                }
            }

            // Sort by dice type
            allDiceData.Sort((a, b) => a.type.CompareTo(b.type));

            // Use SerializedObject to set the private field
            SerializedObject so = new SerializedObject(diceManager);
            SerializedProperty diceDataProp = so.FindProperty("allDiceData");
            
            diceDataProp.ClearArray();
            for (int i = 0; i < allDiceData.Count; i++)
            {
                diceDataProp.InsertArrayElementAtIndex(i);
                diceDataProp.GetArrayElementAtIndex(i).objectReferenceValue = allDiceData[i];
            }
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(diceManager);

            return $"Assigned {allDiceData.Count} dice data assets to DiceManager: {string.Join(", ", allDiceData.ConvertAll(d => d.type.ToString()))}";
        }
    }
}
