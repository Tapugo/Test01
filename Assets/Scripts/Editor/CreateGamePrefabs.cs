using UnityEngine;
using UnityEditor;
using Incredicer.Helpers;
using MoreMountains.Feedbacks;

// Type aliases to avoid namespace conflict
using DiceComponent = Incredicer.Dice.Dice;
using DiceManagerClass = Incredicer.Dice.DiceManager;

namespace Incredicer.Editor
{
    public static class CreateGamePrefabs
    {
        private const string PREFABS_DICE_PATH = "Assets/Prefabs/Dice";
        private const string PREFABS_HELPERS_PATH = "Assets/Prefabs/Helpers";
        private const string PREFABS_EFFECTS_PATH = "Assets/Prefabs/Effects";

        [MenuItem("Incredicer/Create All Prefabs")]
        public static string Execute()
        {
            // Ensure directories exist
            EnsureDirectoryExists(PREFABS_DICE_PATH);
            EnsureDirectoryExists(PREFABS_HELPERS_PATH);
            EnsureDirectoryExists(PREFABS_EFFECTS_PATH);

            string result = "";

            // Create Dice prefab
            result += CreateDicePrefab() + "\n";

            // Create HelperHand prefab
            result += CreateHelperHandPrefab() + "\n";

            // Assign prefabs to managers
            result += AssignPrefabsToManagers() + "\n";

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return result;
        }

        private static string CreateDicePrefab()
        {
            string prefabPath = $"{PREFABS_DICE_PATH}/DicePrefab.prefab";

            // Check if already exists
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                return "Dice prefab already exists, skipping creation.";
            }

            // Create dice GameObject
            GameObject diceObj = new GameObject("DicePrefab");

            // Add SpriteRenderer
            SpriteRenderer sr = diceObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1;
            sr.color = Color.white;

            // Add CircleCollider2D
            CircleCollider2D collider = diceObj.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;

            // Add Dice component
            diceObj.AddComponent<DiceComponent>();

            // Create RollFeedback child
            GameObject rollFeedbackObj = new GameObject("RollFeedback");
            rollFeedbackObj.transform.SetParent(diceObj.transform);
            rollFeedbackObj.transform.localPosition = Vector3.zero;
            MMF_Player rollFeedback = rollFeedbackObj.AddComponent<MMF_Player>();

            // Create JackpotFeedback child
            GameObject jackpotFeedbackObj = new GameObject("JackpotFeedback");
            jackpotFeedbackObj.transform.SetParent(diceObj.transform);
            jackpotFeedbackObj.transform.localPosition = Vector3.zero;
            MMF_Player jackpotFeedback = jackpotFeedbackObj.AddComponent<MMF_Player>();

            // Save as prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(diceObj, prefabPath);
            Object.DestroyImmediate(diceObj);

            return $"Created Dice prefab at {prefabPath}";
        }

        private static string CreateHelperHandPrefab()
        {
            string prefabPath = $"{PREFABS_HELPERS_PATH}/HelperHandPrefab.prefab";

            // Check if already exists
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                return "HelperHand prefab already exists, skipping creation.";
            }

            // Create hand GameObject
            GameObject handObj = new GameObject("HelperHandPrefab");

            // Add SpriteRenderer
            SpriteRenderer sr = handObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            sr.color = new Color(0.95f, 0.8f, 0.6f); // Skin-like color

            // Add HelperHand component
            handObj.AddComponent<HelperHand>();

            // Create RollFeedback child
            GameObject rollFeedbackObj = new GameObject("RollFeedback");
            rollFeedbackObj.transform.SetParent(handObj.transform);
            rollFeedbackObj.transform.localPosition = Vector3.zero;
            rollFeedbackObj.AddComponent<MMF_Player>();

            // Save as prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(handObj, prefabPath);
            Object.DestroyImmediate(handObj);

            return $"Created HelperHand prefab at {prefabPath}";
        }

        private static string AssignPrefabsToManagers()
        {
            string result = "";

            // Find managers in scene
            DiceManagerClass diceManager = Object.FindObjectOfType<DiceManagerClass>();
            HelperHandManager helperHandManager = Object.FindObjectOfType<HelperHandManager>();

            if (diceManager == null)
            {
                return "DiceManager not found in scene!";
            }

            // Load prefabs
            GameObject dicePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFABS_DICE_PATH}/DicePrefab.prefab");
            GameObject helperHandPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFABS_HELPERS_PATH}/HelperHandPrefab.prefab");

            // Load effect prefabs from Epic Toon FX
            GameObject rollEffect = FindEffectPrefab("SparkleArea", "Yellow");
            GameObject jackpotEffect = FindEffectPrefab("LevelupNova", "Yellow");
            GameObject coinEffect = FindEffectPrefab("GoldCoinBlast", "");

            // Assign to DiceManager
            SerializedObject dmSO = new SerializedObject(diceManager);
            SerializedProperty dicePrefabProp = dmSO.FindProperty("dicePrefab");
            dicePrefabProp.objectReferenceValue = dicePrefab;
            dmSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(diceManager);
            result += "Assigned dicePrefab to DiceManager. ";

            // Assign to HelperHandManager
            if (helperHandManager != null)
            {
                SerializedObject hhmSO = new SerializedObject(helperHandManager);
                SerializedProperty helperHandPrefabProp = hhmSO.FindProperty("helperHandPrefab");
                helperHandPrefabProp.objectReferenceValue = helperHandPrefab;

                // Set maxHands to 1 so we can test helper hands
                SerializedProperty maxHandsProp = hhmSO.FindProperty("maxHands");
                maxHandsProp.intValue = 1;

                hhmSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(helperHandManager);
                result += "Assigned helperHandPrefab to HelperHandManager. ";
            }

            // Update the Dice prefab with effect references
            if (dicePrefab != null && (rollEffect != null || jackpotEffect != null))
            {
                string dicePrefabPath = AssetDatabase.GetAssetPath(dicePrefab);
                GameObject dicePrefabInstance = PrefabUtility.LoadPrefabContents(dicePrefabPath);

                DiceComponent diceComponent = dicePrefabInstance.GetComponent<DiceComponent>();
                if (diceComponent != null)
                {
                    SerializedObject diceSO = new SerializedObject(diceComponent);

                    if (rollEffect != null)
                    {
                        SerializedProperty rollEffectProp = diceSO.FindProperty("rollEffectPrefab");
                        rollEffectProp.objectReferenceValue = rollEffect;
                    }

                    if (jackpotEffect != null)
                    {
                        SerializedProperty jackpotEffectProp = diceSO.FindProperty("jackpotEffectPrefab");
                        jackpotEffectProp.objectReferenceValue = jackpotEffect;
                    }

                    diceSO.ApplyModifiedProperties();
                }

                PrefabUtility.SaveAsPrefabAsset(dicePrefabInstance, dicePrefabPath);
                PrefabUtility.UnloadPrefabContents(dicePrefabInstance);
                result += "Assigned effect prefabs to Dice prefab. ";
            }

            return result;
        }

        private static GameObject FindEffectPrefab(string nameContains, string colorSuffix)
        {
            string searchPattern = string.IsNullOrEmpty(colorSuffix)
                ? $"t:Prefab {nameContains}"
                : $"t:Prefab {nameContains}{colorSuffix}";

            string[] guids = AssetDatabase.FindAssets(searchPattern, new[] { "Assets/Epic Toon FX" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains(nameContains))
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
            }

            // Fallback: try without color suffix
            if (!string.IsNullOrEmpty(colorSuffix))
            {
                return FindEffectPrefab(nameContains, "");
            }

            return null;
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] parts = path.Split('/');
                string currentPath = parts[0];

                for (int i = 1; i < parts.Length; i++)
                {
                    string newPath = currentPath + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    }
                    currentPath = newPath;
                }
            }
        }
    }
}
