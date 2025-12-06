using UnityEngine;
using UnityEditor;
using Incredicer.Core;
using Incredicer.Dice;

namespace Incredicer.Editor
{
    public static class GenerateDiceDataRunner
    {
        public static string Execute()
        {
            // Prices significantly increased and growth scaled up to slow down progression
            CreateDiceData(DiceType.Basic, "Basic Dice", 1, 0, 25, 1.22f, new Color(0.9f, 0.9f, 0.9f));
            CreateDiceData(DiceType.Bronze, "Bronze Dice", 3, 0, 250, 1.28f, new Color(0.8f, 0.5f, 0.2f));
            CreateDiceData(DiceType.Silver, "Silver Dice", 10, 0.05f, 1500, 1.32f, new Color(0.75f, 0.75f, 0.85f));
            CreateDiceData(DiceType.Gold, "Gold Dice", 50, 0.2f, 8000, 1.35f, new Color(1f, 0.84f, 0f));
            CreateDiceData(DiceType.Emerald, "Emerald Dice", 250, 1f, 50000, 1.38f, new Color(0.31f, 0.78f, 0.47f));
            CreateDiceData(DiceType.Ruby, "Ruby Dice", 1500, 5f, 350000, 1.40f, new Color(0.88f, 0.07f, 0.37f));
            CreateDiceData(DiceType.Diamond, "Diamond Dice", 10000, 25f, 3000000, 1.42f, new Color(0.73f, 0.95f, 1f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return "Dice Data assets generated successfully!";
        }

        private static void CreateDiceData(DiceType type, string displayName, double basePayout, float dmPerRoll, int shopBaseCost, float shopCostGrowth, Color tintColor)
        {
            string path = $"Assets/ScriptableObjects/Dice/{type}Dice.asset";
            
            DiceData existingAsset = AssetDatabase.LoadAssetAtPath<DiceData>(path);
            if (existingAsset != null)
            {
                existingAsset.type = type;
                existingAsset.displayName = displayName;
                existingAsset.basePayout = basePayout;
                existingAsset.dmPerRoll = dmPerRoll;
                existingAsset.shopBaseCost = shopBaseCost;
                existingAsset.shopCostGrowth = shopCostGrowth;
                existingAsset.tintColor = tintColor;
                EditorUtility.SetDirty(existingAsset);
            }
            else
            {
                DiceData asset = ScriptableObject.CreateInstance<DiceData>();
                asset.type = type;
                asset.displayName = displayName;
                asset.basePayout = basePayout;
                asset.dmPerRoll = dmPerRoll;
                asset.shopBaseCost = shopBaseCost;
                asset.shopCostGrowth = shopCostGrowth;
                asset.tintColor = tintColor;
                asset.rollCooldown = 0.5f;

                AssetDatabase.CreateAsset(asset, path);
            }
        }
    }
}
