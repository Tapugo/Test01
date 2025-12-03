#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Incredicer.Milestones;

namespace Incredicer.Editor
{
    /// <summary>
    /// Editor utility to set up the Milestone system in the scene.
    /// </summary>
    public class SetupMilestones : MonoBehaviour
    {
        [MenuItem("Incredicer/Setup Milestone System")]
        public static void Setup()
        {
            // Create milestone definition assets
            CreateMilestoneDefinitions();

            // Find or create Managers object
            GameObject managers = GameObject.Find("Managers");
            if (managers == null)
            {
                managers = new GameObject("Managers");
                Debug.Log("[SetupMilestones] Created Managers object");
            }

            // Add MilestoneManager
            var existingManager = managers.GetComponent<MilestoneManager>();
            if (existingManager == null)
            {
                managers.AddComponent<MilestoneManager>();
                Debug.Log("[SetupMilestones] Added MilestoneManager");
            }

            // Find GameCanvas
            GameObject canvas = GameObject.Find("GameCanvas");
            if (canvas == null)
            {
                Debug.LogWarning("[SetupMilestones] No GameCanvas found! Create one first.");
                return;
            }

            // Add MilestonesUI to canvas
            var existingUI = canvas.GetComponent<MilestonesUI>();
            if (existingUI == null)
            {
                existingUI = canvas.AddComponent<MilestonesUI>();
                Debug.Log("[SetupMilestones] Added MilestonesUI to GameCanvas");
            }

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[SetupMilestones] Milestone system setup complete!");
        }

        private static void CreateMilestoneDefinitions()
        {
            // Ensure Milestones folder exists
            string milestoneFolder = "Assets/Resources/Milestones";
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if (!AssetDatabase.IsValidFolder(milestoneFolder))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Milestones");
            }

            // Currency Milestones
            CreateMilestone("money_1k", "First Grand", "Earn 1,000 lifetime money",
                MilestoneType.LifetimeMoney, 1000, MilestoneRewardType.TimeShards, 5, 1, Color.yellow);
            CreateMilestone("money_10k", "Getting Rich", "Earn 10,000 lifetime money",
                MilestoneType.LifetimeMoney, 10000, MilestoneRewardType.TimeShards, 15, 2, Color.yellow);
            CreateMilestone("money_100k", "Wealthy", "Earn 100,000 lifetime money",
                MilestoneType.LifetimeMoney, 100000, MilestoneRewardType.TimeShards, 50, 3, Color.yellow);
            CreateMilestone("money_1m", "Millionaire", "Earn 1,000,000 lifetime money",
                MilestoneType.LifetimeMoney, 1000000, MilestoneRewardType.PermanentMoneyBoost, 0.05, 4, Color.yellow);

            CreateMilestone("dm_100", "Dark Collector", "Earn 100 lifetime Dark Matter",
                MilestoneType.LifetimeDarkMatter, 100, MilestoneRewardType.TimeShards, 10, 2, new Color(0.5f, 0.2f, 0.8f));
            CreateMilestone("dm_1000", "Dark Hoarder", "Earn 1,000 lifetime Dark Matter",
                MilestoneType.LifetimeDarkMatter, 1000, MilestoneRewardType.PermanentDMBoost, 0.05, 3, new Color(0.5f, 0.2f, 0.8f));

            // Dice Roll Milestones
            CreateMilestone("rolls_100", "Roller", "Roll dice 100 times",
                MilestoneType.TotalDiceRolls, 100, MilestoneRewardType.TimeShards, 5, 1, Color.white);
            CreateMilestone("rolls_1000", "Dice Master", "Roll dice 1,000 times",
                MilestoneType.TotalDiceRolls, 1000, MilestoneRewardType.TimeShards, 20, 2, Color.white);
            CreateMilestone("rolls_10000", "Roll Legend", "Roll dice 10,000 times",
                MilestoneType.TotalDiceRolls, 10000, MilestoneRewardType.PermanentMoneyBoost, 0.1, 3, Color.white);

            // Jackpot Milestones
            CreateMilestone("jackpot_10", "Lucky Streak", "Hit 10 jackpots",
                MilestoneType.TotalJackpots, 10, MilestoneRewardType.TimeShards, 10, 1, Color.green);
            CreateMilestone("jackpot_100", "Jackpot King", "Hit 100 jackpots",
                MilestoneType.TotalJackpots, 100, MilestoneRewardType.TimeShards, 50, 3, Color.green);

            // Dice Collection Milestones
            CreateMilestone("dice_5", "Small Collection", "Own 5 dice at once",
                MilestoneType.TotalDiceOwned, 5, MilestoneRewardType.TimeShards, 5, 1, Color.cyan);
            CreateMilestone("dice_20", "Dice Collector", "Own 20 dice at once",
                MilestoneType.TotalDiceOwned, 20, MilestoneRewardType.TimeShards, 25, 2, Color.cyan);
            CreateMilestone("dice_50", "Dice Hoard", "Own 50 dice at once",
                MilestoneType.TotalDiceOwned, 50, MilestoneRewardType.TimeShards, 100, 3, Color.cyan);

            // Time Fracture Milestones
            CreateMilestone("fracture_1", "First Fracture", "Perform your first Time Fracture",
                MilestoneType.TotalTimeFractures, 1, MilestoneRewardType.TimeShards, 25, 2, new Color(0.7f, 0.5f, 1f));
            CreateMilestone("fracture_5", "Time Walker", "Perform 5 Time Fractures",
                MilestoneType.TotalTimeFractures, 5, MilestoneRewardType.TimeShards, 100, 3, new Color(0.7f, 0.5f, 1f));
            CreateMilestone("fracture_level_5", "Temporal Master", "Reach Time Fracture level 5",
                MilestoneType.TimeFractureLevel, 5, MilestoneRewardType.PermanentMoneyBoost, 0.15, 4, new Color(0.7f, 0.5f, 1f));

            // Skill Tree Milestones
            CreateMilestone("skills_5", "Learner", "Unlock 5 skill nodes",
                MilestoneType.SkillNodesUnlocked, 5, MilestoneRewardType.TimeShards, 10, 1, Color.blue);
            CreateMilestone("skills_20", "Scholar", "Unlock 20 skill nodes",
                MilestoneType.SkillNodesUnlocked, 20, MilestoneRewardType.TimeShards, 40, 2, Color.blue);

            // Mission Milestones
            CreateMilestone("missions_10", "Task Master", "Complete 10 missions",
                MilestoneType.MissionsCompleted, 10, MilestoneRewardType.TimeShards, 15, 1, new Color(1f, 0.6f, 0.2f));
            CreateMilestone("missions_50", "Mission Expert", "Complete 50 missions",
                MilestoneType.MissionsCompleted, 50, MilestoneRewardType.TimeShards, 75, 2, new Color(1f, 0.6f, 0.2f));

            // Overclock Milestones
            CreateMilestone("destroy_1", "First Sacrifice", "Destroy 1 dice by overclocking",
                MilestoneType.TotalDiceDestroyed, 1, MilestoneRewardType.TimeShards, 10, 2, new Color(1f, 0.4f, 0.1f));
            CreateMilestone("destroy_10", "Overclocker", "Destroy 10 dice by overclocking",
                MilestoneType.TotalDiceDestroyed, 10, MilestoneRewardType.TimeShards, 50, 3, new Color(1f, 0.4f, 0.1f));

            // Login Milestones
            CreateMilestone("streak_7", "Weekly Warrior", "Reach a 7-day login streak",
                MilestoneType.DailyLoginStreak, 7, MilestoneRewardType.TimeShards, 20, 1, Color.magenta);
            CreateMilestone("streak_30", "Dedicated Player", "Reach a 30-day login streak",
                MilestoneType.DailyLoginStreak, 30, MilestoneRewardType.TimeShards, 100, 2, Color.magenta);

            AssetDatabase.SaveAssets();
            Debug.Log("[SetupMilestones] Created milestone definition assets");
        }

        private static void CreateMilestone(string id, string name, string desc,
            MilestoneType type, double target, MilestoneRewardType rewardType, double rewardAmount,
            int tier, Color color)
        {
            string path = $"Assets/Resources/Milestones/{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<MilestoneDefinition>(path);
            if (existing != null) return;

            var milestone = ScriptableObject.CreateInstance<MilestoneDefinition>();
            milestone.milestoneId = id;
            milestone.displayName = name;
            milestone.description = desc;
            milestone.milestoneType = type;
            milestone.targetAmount = target;
            milestone.tier = tier;
            milestone.accentColor = color;
            milestone.rewards = new MilestoneReward[]
            {
                new MilestoneReward { type = rewardType, amount = rewardAmount }
            };

            AssetDatabase.CreateAsset(milestone, path);
        }
    }
}
#endif
