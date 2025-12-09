using UnityEngine;
using UnityEditor;
using Incredicer.Core;
using System.IO;

namespace Incredicer.Editor
{
    /// <summary>
    /// Editor utility to set up Visual Effects Manager with Epic Toon FX prefabs.
    /// </summary>
    public static class SetupVisualEffects
    {
        private const string ETFX_BASE_PATH = "Assets/Epic Toon FX/Prefabs";

        [MenuItem("Incredicer/Setup/Visual Effects Manager")]
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

            Debug.Log("[SetupVisualEffects] VisualEffectsManager created! Run 'Assign Epic Toon FX Prefabs' to enable enhanced effects.");
        }

        [MenuItem("Incredicer/Setup/Assign Epic Toon FX Prefabs")]
        public static void AssignEpicToonFXPrefabs()
        {
            // Find VisualEffectsManager in scene
            var vfxManager = Object.FindObjectOfType<VisualEffectsManager>();
            if (vfxManager == null)
            {
                Debug.LogError("[SetupVisualEffects] No VisualEffectsManager found in scene! Create one first using Incredicer/Setup/Visual Effects Manager");
                return;
            }

            // Check if Epic Toon FX folder exists
            if (!Directory.Exists(Application.dataPath + "/Epic Toon FX"))
            {
                Debug.LogError("[SetupVisualEffects] Epic Toon FX package not found! Please import it from the Asset Store first.");
                return;
            }

            SerializedObject serializedVFX = new SerializedObject(vfxManager);

            int assignedCount = 0;
            int totalPrefabs = 0;

            // Sparkle effects - using SparkleExplosion which exists in Combat/Explosions
            totalPrefabs++;
            assignedCount += AssignPrefabWithFallback(serializedVFX, "etfxSparkleYellow",
                $"{ETFX_BASE_PATH}/Combat/Explosions/SparkleExplosion/SparkleExplosionYellow.prefab",
                $"{ETFX_BASE_PATH}/Interactive/Loot/ItemSparkle/ItemSparkleYellow.prefab");

            totalPrefabs++;
            assignedCount += AssignPrefabWithFallback(serializedVFX, "etfxSparkleRainbow",
                $"{ETFX_BASE_PATH}/Interactive/Loot/ItemSparkleBurst/ItemSparkleBurstRainbow.prefab",
                $"{ETFX_BASE_PATH}/Combat/Explosions/SparkleExplosion/SparkleExplosionPink.prefab");

            totalPrefabs++;
            assignedCount += AssignPrefabWithFallback(serializedVFX, "etfxSparklePurple",
                $"{ETFX_BASE_PATH}/Combat/Explosions/SparkleExplosion/SparkleExplosionPink.prefab",
                $"{ETFX_BASE_PATH}/Interactive/Loot/ItemSparkleBurst/ItemSparkleBurstPurple.prefab");

            // Level up novas - verified paths
            totalPrefabs++;
            assignedCount += AssignPrefab(serializedVFX, "etfxLevelupNovaYellow", $"{ETFX_BASE_PATH}/Interactive/Level Up/Nova/LevelupNovaYellow.prefab");
            totalPrefabs++;
            assignedCount += AssignPrefab(serializedVFX, "etfxLevelupNovaPurple", $"{ETFX_BASE_PATH}/Interactive/Level Up/Nova/LevelupNovaPurple.prefab");

            // Star explosions - verified paths
            totalPrefabs++;
            assignedCount += AssignPrefab(serializedVFX, "etfxStarExplosionOrange", $"{ETFX_BASE_PATH}/Combat/Explosions/StarExplosion/StarExplosionOrange.prefab");
            totalPrefabs++;
            assignedCount += AssignPrefab(serializedVFX, "etfxStarExplosionPink", $"{ETFX_BASE_PATH}/Combat/Explosions/StarExplosion/StarExplosionPink.prefab");

            // Magic novas - verified paths
            totalPrefabs++;
            assignedCount += AssignPrefab(serializedVFX, "etfxMagicNovaYellow", $"{ETFX_BASE_PATH}/Combat/Magic/Nova/MagicNovaYellow.prefab");
            totalPrefabs++;
            assignedCount += AssignPrefab(serializedVFX, "etfxMagicNovaGreen", $"{ETFX_BASE_PATH}/Combat/Magic/Nova/MagicNovaGreen.prefab");

            // Glow orbs - verified paths
            totalPrefabs++;
            assignedCount += AssignPrefab(serializedVFX, "etfxGlowOrbYellow", $"{ETFX_BASE_PATH}/Interactive/Loot/GlowOrb/GlowOrbYellow.prefab");
            totalPrefabs++;
            assignedCount += AssignPrefab(serializedVFX, "etfxGlowOrbPink", $"{ETFX_BASE_PATH}/Interactive/Loot/GlowOrb/GlowOrbPink.prefab");

            // Item sparkle bursts - verified paths
            totalPrefabs++;
            assignedCount += AssignPrefab(serializedVFX, "etfxItemSparkleBurstYellow", $"{ETFX_BASE_PATH}/Interactive/Loot/ItemSparkleBurst/ItemSparkleBurstYellow.prefab");
            totalPrefabs++;
            assignedCount += AssignPrefab(serializedVFX, "etfxItemSparkleBurstPurple", $"{ETFX_BASE_PATH}/Interactive/Loot/ItemSparkleBurst/ItemSparkleBurstPurple.prefab");

            // Fire nova - verified path
            totalPrefabs++;
            assignedCount += AssignPrefab(serializedVFX, "etfxNovaFire", $"{ETFX_BASE_PATH}/Combat/Nova/Fire/NovaFireRed.prefab");

            // Gold coin blast - search for it
            totalPrefabs++;
            assignedCount += AssignPrefabBySearch(serializedVFX, "etfxGoldCoinBlast", "GoldCoin", "Blast");

            // Gold coin fountain - search for it
            totalPrefabs++;
            assignedCount += AssignPrefabBySearch(serializedVFX, "etfxGoldCoinFountain", "GoldCoin", "Fountain");

            // Silver coin blast - search for it
            totalPrefabs++;
            assignedCount += AssignPrefabBySearch(serializedVFX, "etfxSilverCoinBlast", "SilverCoin", "Blast");

            // Confetti - search for it
            totalPrefabs++;
            assignedCount += AssignPrefabBySearch(serializedVFX, "etfxConfettiRainbow", "Confetti", "Rainbow");

            // Firework - search for it
            totalPrefabs++;
            assignedCount += AssignPrefabBySearch(serializedVFX, "etfxFireworkCluster", "Firework", "");

            serializedVFX.ApplyModifiedProperties();
            EditorUtility.SetDirty(vfxManager);

            // Mark scene as dirty to ensure save prompt
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(vfxManager.gameObject.scene);

            if (assignedCount == totalPrefabs)
            {
                Debug.Log($"[SetupVisualEffects] SUCCESS! All {assignedCount}/{totalPrefabs} Epic Toon FX prefabs assigned!");
            }
            else
            {
                Debug.LogWarning($"[SetupVisualEffects] Assigned {assignedCount}/{totalPrefabs} prefabs. Some prefabs not found - effects will fall back to procedural particles.");
            }
        }

        private static int AssignPrefab(SerializedObject obj, string propertyName, string path)
        {
            SerializedProperty prop = obj.FindProperty(propertyName);
            if (prop == null)
            {
                Debug.LogWarning($"[SetupVisualEffects] Property '{propertyName}' not found on VisualEffectsManager");
                return 0;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                prop.objectReferenceValue = prefab;
                Debug.Log($"[SetupVisualEffects] Assigned: {propertyName} <- {path}");
                return 1;
            }
            else
            {
                Debug.LogWarning($"[SetupVisualEffects] Prefab not found: {path}");
                return 0;
            }
        }

        private static int AssignPrefabWithFallback(SerializedObject obj, string propertyName, string primaryPath, string fallbackPath)
        {
            SerializedProperty prop = obj.FindProperty(propertyName);
            if (prop == null)
            {
                Debug.LogWarning($"[SetupVisualEffects] Property '{propertyName}' not found");
                return 0;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(primaryPath);
            if (prefab == null)
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fallbackPath);
            }

            if (prefab != null)
            {
                prop.objectReferenceValue = prefab;
                Debug.Log($"[SetupVisualEffects] Assigned: {propertyName} <- {AssetDatabase.GetAssetPath(prefab)}");
                return 1;
            }

            Debug.LogWarning($"[SetupVisualEffects] No prefab found for {propertyName}");
            return 0;
        }

        private static int AssignPrefabBySearch(SerializedObject obj, string propertyName, string searchTerm1, string searchTerm2)
        {
            SerializedProperty prop = obj.FindProperty(propertyName);
            if (prop == null)
            {
                Debug.LogWarning($"[SetupVisualEffects] Property '{propertyName}' not found");
                return 0;
            }

            // Search for prefabs matching the terms
            string[] guids = AssetDatabase.FindAssets($"t:Prefab {searchTerm1}", new[] { "Assets/Epic Toon FX" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(searchTerm2) || path.ToLower().Contains(searchTerm2.ToLower()))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                    {
                        prop.objectReferenceValue = prefab;
                        Debug.Log($"[SetupVisualEffects] Assigned (found): {propertyName} <- {path}");
                        return 1;
                    }
                }
            }

            Debug.LogWarning($"[SetupVisualEffects] No prefab found matching '{searchTerm1}' + '{searchTerm2}'");
            return 0;
        }
    }
}
