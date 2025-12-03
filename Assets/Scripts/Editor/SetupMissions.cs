#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Incredicer.Missions;

namespace Incredicer.Editor
{
    /// <summary>
    /// Editor utility to set up the Mission system in the scene.
    /// </summary>
    public class SetupMissions : MonoBehaviour
    {
        [MenuItem("Incredicer/Setup Mission System")]
        public static void Setup()
        {
            // Create mission definition assets if they don't exist
            CreateMissionDefinitions();

            // Find or create Managers object
            GameObject managers = GameObject.Find("Managers");
            if (managers == null)
            {
                managers = new GameObject("Managers");
                Debug.Log("[SetupMissions] Created Managers object");
            }

            // Add MissionManager
            var existingManager = managers.GetComponent<MissionManager>();
            if (existingManager == null)
            {
                managers.AddComponent<MissionManager>();
                Debug.Log("[SetupMissions] Added MissionManager");
            }

            // Find GameCanvas
            GameObject canvas = GameObject.Find("GameCanvas");
            if (canvas == null)
            {
                Debug.LogWarning("[SetupMissions] No GameCanvas found! Create one first.");
                return;
            }

            // Add MissionsUI to canvas
            var existingUI = canvas.GetComponent<MissionsUI>();
            if (existingUI == null)
            {
                existingUI = canvas.AddComponent<MissionsUI>();
                Debug.Log("[SetupMissions] Added MissionsUI to GameCanvas");
            }

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[SetupMissions] Mission system setup complete!");
        }

        private static void CreateMissionDefinitions()
        {
            // Ensure Missions folder exists
            string missionFolder = "Assets/Resources/Missions";
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if (!AssetDatabase.IsValidFolder(missionFolder))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Missions");
            }

            // Create Daily missions
            CreateDailyMission("daily_roll_50", "Roll 50 Dice", "Roll dice 50 times today.",
                MissionType.RollDice, 50, MissionRewardType.Money, 500);
            CreateDailyMission("daily_earn_1000", "Earn 1,000 Money", "Earn 1,000 money today.",
                MissionType.EarnMoney, 1000, MissionRewardType.Money, 250);
            CreateDailyMission("daily_buy_3", "Buy 3 Dice", "Purchase 3 new dice today.",
                MissionType.BuyDice, 3, MissionRewardType.DarkMatter, 5);
            CreateDailyMission("daily_skill_1", "Unlock 1 Skill", "Unlock a skill node today.",
                MissionType.UnlockSkillNodes, 1, MissionRewardType.Money, 300);
            CreateDailyMission("daily_jackpot_1", "Hit a Jackpot", "Get a jackpot roll today.",
                MissionType.EarnJackpots, 1, MissionRewardType.DarkMatter, 10);

            // Create Weekly missions
            CreateWeeklyMission("weekly_roll_500", "Roll 500 Dice", "Roll dice 500 times this week.",
                MissionType.RollDice, 500, MissionRewardType.DarkMatter, 25);
            CreateWeeklyMission("weekly_earn_50k", "Earn 50,000 Money", "Earn 50,000 money this week.",
                MissionType.EarnMoney, 50000, MissionRewardType.DarkMatter, 30);
            CreateWeeklyMission("weekly_dm_100", "Earn 100 Dark Matter", "Earn 100 dark matter this week.",
                MissionType.EarnDarkMatter, 100, MissionRewardType.Money, 5000);
            CreateWeeklyMission("weekly_buy_10", "Buy 10 Dice", "Purchase 10 dice this week.",
                MissionType.BuyDice, 10, MissionRewardType.DarkMatter, 20);
            CreateWeeklyMission("weekly_skill_5", "Unlock 5 Skills", "Unlock 5 skill nodes this week.",
                MissionType.UnlockSkillNodes, 5, MissionRewardType.DarkMatter, 50);
            CreateWeeklyMission("weekly_jackpot_5", "Hit 5 Jackpots", "Get 5 jackpot rolls this week.",
                MissionType.EarnJackpots, 5, MissionRewardType.DarkMatter, 40);
            CreateWeeklyMission("weekly_spend_10k", "Spend 10,000 Money", "Spend 10,000 money this week.",
                MissionType.SpendMoney, 10000, MissionRewardType.DarkMatter, 15);

            AssetDatabase.SaveAssets();
            Debug.Log("[SetupMissions] Created mission definition assets");
        }

        private static void CreateDailyMission(string id, string name, string desc,
            MissionType type, double target, MissionRewardType rewardType, double rewardAmount)
        {
            string path = $"Assets/Resources/Missions/{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<MissionDefinition>(path);
            if (existing != null) return;

            var mission = ScriptableObject.CreateInstance<MissionDefinition>();
            mission.missionId = id;
            mission.displayName = name;
            mission.description = desc;
            mission.isDaily = true;
            mission.missionType = type;
            mission.targetAmount = target;
            mission.rewards = new MissionReward[]
            {
                new MissionReward { type = rewardType, amount = rewardAmount }
            };

            AssetDatabase.CreateAsset(mission, path);
        }

        private static void CreateWeeklyMission(string id, string name, string desc,
            MissionType type, double target, MissionRewardType rewardType, double rewardAmount)
        {
            string path = $"Assets/Resources/Missions/{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<MissionDefinition>(path);
            if (existing != null) return;

            var mission = ScriptableObject.CreateInstance<MissionDefinition>();
            mission.missionId = id;
            mission.displayName = name;
            mission.description = desc;
            mission.isDaily = false;
            mission.missionType = type;
            mission.targetAmount = target;
            mission.rewards = new MissionReward[]
            {
                new MissionReward { type = rewardType, amount = rewardAmount }
            };

            AssetDatabase.CreateAsset(mission, path);
        }
    }
}
#endif
