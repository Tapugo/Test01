#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Incredicer.Editor
{
    /// <summary>
    /// Editor utility to set up the Daily Login system in the scene.
    /// </summary>
    public class SetupDailyLogin : MonoBehaviour
    {
        [MenuItem("Incredicer/Setup Daily Login System")]
        public static void Setup()
        {
            // Create DailyRewardConfig asset if it doesn't exist
            CreateDailyRewardConfig();

            // Find or create Managers object
            GameObject managers = GameObject.Find("Managers");
            if (managers == null)
            {
                managers = new GameObject("Managers");
                Debug.Log("[SetupDailyLogin] Created Managers object");
            }

            // Add DailyLoginManager
            var existingManager = managers.GetComponent<DailyLogin.DailyLoginManager>();
            if (existingManager == null)
            {
                managers.AddComponent<DailyLogin.DailyLoginManager>();
                Debug.Log("[SetupDailyLogin] Added DailyLoginManager");
            }

            // Find GameCanvas
            GameObject canvas = GameObject.Find("GameCanvas");
            if (canvas == null)
            {
                Debug.LogWarning("[SetupDailyLogin] No GameCanvas found! Create one first.");
                return;
            }

            // Add DailyLoginUI to canvas
            var existingUI = canvas.GetComponent<DailyLogin.DailyLoginUI>();
            if (existingUI == null)
            {
                existingUI = canvas.AddComponent<DailyLogin.DailyLoginUI>();
                Debug.Log("[SetupDailyLogin] Added DailyLoginUI to GameCanvas");
            }

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[SetupDailyLogin] Daily Login system setup complete!");
        }

        private static void CreateDailyRewardConfig()
        {
            // Check if config already exists
            string resourcePath = "Assets/Resources/DailyRewardConfig.asset";
            var existing = AssetDatabase.LoadAssetAtPath<DailyLogin.DailyRewardConfig>(resourcePath);
            if (existing != null)
            {
                Debug.Log("[SetupDailyLogin] DailyRewardConfig already exists");
                return;
            }

            // Ensure Resources folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            // Create new config
            var config = ScriptableObject.CreateInstance<DailyLogin.DailyRewardConfig>();

            // Set up default rewards
            config.streakLength = 7;
            config.resetStreakOnMiss = false;
            config.gracePeriodDays = 2;
            config.moneyRewardMinutesWorth = 10f;
            config.baseDarkMatterReward = 5;
            config.dmYesterdayBonusPercent = 0.1f;

            config.dayRewards = new DailyLogin.DailyRewardDay[7];

            // Day 1
            config.dayRewards[0] = new DailyLogin.DailyRewardDay
            {
                dayNumber = 1,
                rewardType = DailyLogin.DailyRewardType.Money,
                baseAmount = 100,
                streakMultiplier = 1f,
                rewardTitle = "Welcome Back!",
                rewardDescription = "A small reward to start your streak."
            };

            // Day 2
            config.dayRewards[1] = new DailyLogin.DailyRewardDay
            {
                dayNumber = 2,
                rewardType = DailyLogin.DailyRewardType.Money,
                baseAmount = 200,
                streakMultiplier = 1.5f,
                rewardTitle = "Keep It Rolling!",
                rewardDescription = "Your streak grows stronger!"
            };

            // Day 3
            config.dayRewards[2] = new DailyLogin.DailyRewardDay
            {
                dayNumber = 3,
                rewardType = DailyLogin.DailyRewardType.DarkMatter,
                baseAmount = 10,
                streakMultiplier = 1f,
                rewardTitle = "Dark Matter Bonus!",
                rewardDescription = "Precious dark matter for your collection."
            };

            // Day 4
            config.dayRewards[3] = new DailyLogin.DailyRewardDay
            {
                dayNumber = 4,
                rewardType = DailyLogin.DailyRewardType.MoneyBoost,
                baseAmount = 50,
                streakMultiplier = 1f,
                boostDurationSeconds = 600,
                rewardTitle = "Money Frenzy!",
                rewardDescription = "+50% money for 10 minutes!"
            };

            // Day 5
            config.dayRewards[4] = new DailyLogin.DailyRewardDay
            {
                dayNumber = 5,
                rewardType = DailyLogin.DailyRewardType.Money,
                baseAmount = 500,
                streakMultiplier = 2f,
                rewardTitle = "Halfway There!",
                rewardDescription = "A big reward for your dedication!"
            };

            // Day 6
            config.dayRewards[5] = new DailyLogin.DailyRewardDay
            {
                dayNumber = 6,
                rewardType = DailyLogin.DailyRewardType.DMBoost,
                baseAmount = 100,
                streakMultiplier = 1f,
                boostDurationSeconds = 600,
                rewardTitle = "Dark Matter Surge!",
                rewardDescription = "+100% dark matter for 10 minutes!"
            };

            // Day 7
            config.dayRewards[6] = new DailyLogin.DailyRewardDay
            {
                dayNumber = 7,
                rewardType = DailyLogin.DailyRewardType.Money,
                baseAmount = 1000,
                streakMultiplier = 3f,
                rewardTitle = "JACKPOT DAY!",
                rewardDescription = "Maximum streak! Massive rewards!"
            };

            AssetDatabase.CreateAsset(config, resourcePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[SetupDailyLogin] Created DailyRewardConfig at " + resourcePath);
        }
    }
}
#endif
