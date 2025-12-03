#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Incredicer.GlobalEvents;

namespace Incredicer.Editor
{
    /// <summary>
    /// Editor utility to set up the Global Events system in the scene.
    /// </summary>
    public class SetupGlobalEvents : MonoBehaviour
    {
        [MenuItem("Incredicer/Setup Global Events System")]
        public static void Setup()
        {
            // Create event definition assets
            CreateEventDefinitions();

            // Find or create Managers object
            GameObject managers = GameObject.Find("Managers");
            if (managers == null)
            {
                managers = new GameObject("Managers");
                Debug.Log("[SetupGlobalEvents] Created Managers object");
            }

            // Add GlobalEventManager
            var existingManager = managers.GetComponent<GlobalEventManager>();
            if (existingManager == null)
            {
                managers.AddComponent<GlobalEventManager>();
                Debug.Log("[SetupGlobalEvents] Added GlobalEventManager");
            }

            // Find GameCanvas
            GameObject canvas = GameObject.Find("GameCanvas");
            if (canvas == null)
            {
                Debug.LogWarning("[SetupGlobalEvents] No GameCanvas found! Create one first.");
                return;
            }

            // Add GlobalEventUI to canvas
            var existingUI = canvas.GetComponent<GlobalEventUI>();
            if (existingUI == null)
            {
                existingUI = canvas.AddComponent<GlobalEventUI>();
                Debug.Log("[SetupGlobalEvents] Added GlobalEventUI to GameCanvas");
            }

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[SetupGlobalEvents] Global Events system setup complete!");
        }

        private static void CreateEventDefinitions()
        {
            // Ensure GlobalEvents folder exists
            string eventFolder = "Assets/Resources/GlobalEvents";
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if (!AssetDatabase.IsValidFolder(eventFolder))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "GlobalEvents");
            }

            // Create various event types
            CreateEvent("roll_madness", "Roll Madness", "Community challenge: Roll as many dice as possible!",
                GlobalEventContributionType.DiceRolls, 1000000, 72,
                new Color(1f, 0.8f, 0.2f),
                new GlobalEventTierReward[]
                {
                    new GlobalEventTierReward { tierName = "Bronze", progressThreshold = 0.25f, timeShardsReward = 10, tierColor = new Color(0.8f, 0.5f, 0.2f) },
                    new GlobalEventTierReward { tierName = "Silver", progressThreshold = 0.50f, timeShardsReward = 25, tierColor = new Color(0.75f, 0.75f, 0.75f) },
                    new GlobalEventTierReward { tierName = "Gold", progressThreshold = 0.75f, timeShardsReward = 50, tierColor = new Color(1f, 0.84f, 0f) },
                    new GlobalEventTierReward { tierName = "Diamond", progressThreshold = 1.0f, timeShardsReward = 100, darkMatterReward = 50, tierColor = new Color(0.6f, 0.9f, 1f) }
                });

            CreateEvent("money_rush", "Money Rush", "Collect as much money as the community can!",
                GlobalEventContributionType.MoneyEarned, 10000000, 48,
                new Color(0.2f, 0.8f, 0.2f),
                new GlobalEventTierReward[]
                {
                    new GlobalEventTierReward { tierName = "Copper", progressThreshold = 0.20f, timeShardsReward = 8, tierColor = new Color(0.72f, 0.45f, 0.2f) },
                    new GlobalEventTierReward { tierName = "Silver", progressThreshold = 0.40f, timeShardsReward = 20, tierColor = new Color(0.75f, 0.75f, 0.75f) },
                    new GlobalEventTierReward { tierName = "Gold", progressThreshold = 0.60f, timeShardsReward = 40, tierColor = new Color(1f, 0.84f, 0f) },
                    new GlobalEventTierReward { tierName = "Platinum", progressThreshold = 0.80f, timeShardsReward = 75, tierColor = new Color(0.9f, 0.9f, 0.95f) },
                    new GlobalEventTierReward { tierName = "Diamond", progressThreshold = 1.0f, timeShardsReward = 150, darkMatterReward = 100, tierColor = new Color(0.6f, 0.9f, 1f) }
                });

            CreateEvent("dark_harvest", "Dark Harvest", "Gather Dark Matter together!",
                GlobalEventContributionType.DarkMatterEarned, 50000, 72,
                new Color(0.5f, 0.2f, 0.8f),
                new GlobalEventTierReward[]
                {
                    new GlobalEventTierReward { tierName = "Shadow", progressThreshold = 0.25f, timeShardsReward = 15, tierColor = new Color(0.3f, 0.1f, 0.4f) },
                    new GlobalEventTierReward { tierName = "Void", progressThreshold = 0.50f, timeShardsReward = 35, darkMatterReward = 25, tierColor = new Color(0.4f, 0.1f, 0.6f) },
                    new GlobalEventTierReward { tierName = "Abyss", progressThreshold = 0.75f, timeShardsReward = 60, darkMatterReward = 50, tierColor = new Color(0.5f, 0.2f, 0.8f) },
                    new GlobalEventTierReward { tierName = "Cosmic", progressThreshold = 1.0f, timeShardsReward = 100, darkMatterReward = 150, tierColor = new Color(0.7f, 0.3f, 1f) }
                });

            CreateEvent("jackpot_fever", "Jackpot Fever", "Hit as many jackpots as possible!",
                GlobalEventContributionType.JackpotsHit, 10000, 48,
                new Color(0.2f, 0.9f, 0.5f),
                new GlobalEventTierReward[]
                {
                    new GlobalEventTierReward { tierName = "Lucky", progressThreshold = 0.25f, timeShardsReward = 12, tierColor = new Color(0.4f, 0.7f, 0.4f) },
                    new GlobalEventTierReward { tierName = "Fortune", progressThreshold = 0.50f, timeShardsReward = 30, tierColor = new Color(0.2f, 0.8f, 0.4f) },
                    new GlobalEventTierReward { tierName = "Blessed", progressThreshold = 0.75f, timeShardsReward = 55, tierColor = new Color(0.1f, 0.9f, 0.5f) },
                    new GlobalEventTierReward { tierName = "Legendary", progressThreshold = 1.0f, timeShardsReward = 120, darkMatterReward = 75, tierColor = new Color(1f, 0.9f, 0.2f) }
                });

            CreateEvent("destruction_derby", "Destruction Derby", "Destroy dice by overclocking!",
                GlobalEventContributionType.DiceDestroyed, 5000, 72,
                new Color(1f, 0.4f, 0.1f),
                new GlobalEventTierReward[]
                {
                    new GlobalEventTierReward { tierName = "Melted", progressThreshold = 0.25f, timeShardsReward = 15, tierColor = new Color(0.8f, 0.3f, 0.1f) },
                    new GlobalEventTierReward { tierName = "Scorched", progressThreshold = 0.50f, timeShardsReward = 35, darkMatterReward = 20, tierColor = new Color(0.9f, 0.4f, 0.1f) },
                    new GlobalEventTierReward { tierName = "Inferno", progressThreshold = 0.75f, timeShardsReward = 65, darkMatterReward = 40, tierColor = new Color(1f, 0.5f, 0.1f) },
                    new GlobalEventTierReward { tierName = "Supernova", progressThreshold = 1.0f, timeShardsReward = 125, darkMatterReward = 100, tierColor = new Color(1f, 0.8f, 0.2f) }
                });

            CreateEvent("mission_blitz", "Mission Blitz", "Complete missions together!",
                GlobalEventContributionType.MissionsCompleted, 25000, 48,
                new Color(1f, 0.6f, 0.2f),
                new GlobalEventTierReward[]
                {
                    new GlobalEventTierReward { tierName = "Recruit", progressThreshold = 0.25f, timeShardsReward = 10, tierColor = new Color(0.6f, 0.4f, 0.2f) },
                    new GlobalEventTierReward { tierName = "Veteran", progressThreshold = 0.50f, timeShardsReward = 25, tierColor = new Color(0.7f, 0.5f, 0.2f) },
                    new GlobalEventTierReward { tierName = "Elite", progressThreshold = 0.75f, timeShardsReward = 50, tierColor = new Color(0.9f, 0.6f, 0.2f) },
                    new GlobalEventTierReward { tierName = "Commander", progressThreshold = 1.0f, timeShardsReward = 100, darkMatterReward = 50, tierColor = new Color(1f, 0.7f, 0.3f) }
                });

            AssetDatabase.SaveAssets();
            Debug.Log("[SetupGlobalEvents] Created global event definition assets");
        }

        private static void CreateEvent(string id, string name, string desc,
            GlobalEventContributionType contribType, double goal, int hours,
            Color eventColor, GlobalEventTierReward[] tiers)
        {
            string path = $"Assets/Resources/GlobalEvents/{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<GlobalEventDefinition>(path);
            if (existing != null) return;

            var eventDef = ScriptableObject.CreateInstance<GlobalEventDefinition>();
            eventDef.eventId = id;
            eventDef.eventName = name;
            eventDef.description = desc;
            eventDef.contributionType = contribType;
            eventDef.communityGoal = goal;
            eventDef.durationHours = hours;
            eventDef.eventColor = eventColor;
            eventDef.tierRewards = tiers;

            AssetDatabase.CreateAsset(eventDef, path);
        }
    }
}
#endif
