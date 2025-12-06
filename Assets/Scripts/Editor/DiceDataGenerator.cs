using UnityEngine;
using UnityEditor;
using Incredicer.Core;
using Incredicer.Dice;

namespace Incredicer.Editor
{
    public static class DiceDataGenerator
    {
        [MenuItem("Incredicer/Generate Dice Data Assets")]
        public static void GenerateDiceDataAssets()
        {
            // SIGNIFICANTLY different dice tiers - each tier is a MAJOR upgrade!
            // basePayout is the MULTIPLIER for money earned (face value * basePayout)
            // Higher tiers: faster cooldowns, more DM, much higher payouts
            // Prices significantly increased and growth scaled up to slow down progression
            //                              Type              Name               Payout  DM      Cost       Growth  Color                            Cooldown
            CreateDiceData(DiceType.Basic,   "Basic Dice",     1,      0f,      50,        1.35f,  new Color(0.9f, 0.9f, 0.9f),      0.5f);   // 1x multiplier
            CreateDiceData(DiceType.Bronze,  "Bronze Dice",    3,      0f,      500,       1.40f,  new Color(0.8f, 0.5f, 0.2f),      0.45f);  // 3x multiplier
            CreateDiceData(DiceType.Silver,  "Silver Dice",    10,     0.05f,   3000,      1.45f,  new Color(0.75f, 0.75f, 0.85f),   0.4f);   // 10x multiplier
            CreateDiceData(DiceType.Gold,    "Gold Dice",      50,     0.2f,    18000,     1.48f,  new Color(1f, 0.84f, 0f),         0.35f);  // 50x multiplier!
            CreateDiceData(DiceType.Emerald, "Emerald Dice",   250,    1f,      120000,    1.50f,  new Color(0.31f, 0.78f, 0.47f),   0.3f);   // 250x multiplier!
            CreateDiceData(DiceType.Ruby,    "Ruby Dice",      1500,   5f,      800000,    1.52f,  new Color(0.88f, 0.07f, 0.37f),   0.25f);  // 1500x multiplier!
            CreateDiceData(DiceType.Diamond, "Diamond Dice",   10000,  25f,     6000000,   1.55f,  new Color(0.73f, 0.95f, 1f),      0.2f);   // 10000x multiplier!!

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Dice Data assets generated successfully! Dice tiers now have SIGNIFICANT differences.");
        }

        private static void CreateDiceData(DiceType type, string displayName, double basePayout, float dmPerRoll, int shopBaseCost, float shopCostGrowth, Color tintColor, float rollCooldown = 0.5f)
        {
            string path = $"Assets/ScriptableObjects/Dice/{type}Dice.asset";
            
            DiceData existingAsset = AssetDatabase.LoadAssetAtPath<DiceData>(path);
            if (existingAsset != null)
            {
                // Update existing
                existingAsset.type = type;
                existingAsset.displayName = displayName;
                existingAsset.basePayout = basePayout;
                existingAsset.dmPerRoll = dmPerRoll;
                existingAsset.shopBaseCost = shopBaseCost;
                existingAsset.shopCostGrowth = shopCostGrowth;
                existingAsset.tintColor = tintColor;
                existingAsset.rollCooldown = rollCooldown;
                EditorUtility.SetDirty(existingAsset);
            }
            else
            {
                // Create new
                DiceData asset = ScriptableObject.CreateInstance<DiceData>();
                asset.type = type;
                asset.displayName = displayName;
                asset.basePayout = basePayout;
                asset.dmPerRoll = dmPerRoll;
                asset.shopBaseCost = shopBaseCost;
                asset.shopCostGrowth = shopCostGrowth;
                asset.tintColor = tintColor;
                asset.rollCooldown = rollCooldown;

                AssetDatabase.CreateAsset(asset, path);
            }
        }
    }
}
