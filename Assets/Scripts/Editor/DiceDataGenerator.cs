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
            CreateDiceData(DiceType.Basic, "Basic Dice", 1, 0, 10, 1.15f, new Color(0.9f, 0.9f, 0.9f));
            CreateDiceData(DiceType.Bronze, "Bronze Dice", 2, 0, 50, 1.15f, new Color(0.8f, 0.5f, 0.2f));
            CreateDiceData(DiceType.Silver, "Silver Dice", 5, 0.01f, 200, 1.15f, new Color(0.75f, 0.75f, 0.8f));
            CreateDiceData(DiceType.Gold, "Gold Dice", 20, 0.05f, 1000, 1.15f, new Color(1f, 0.84f, 0f));
            CreateDiceData(DiceType.Emerald, "Emerald Dice", 100, 0.2f, 5000, 1.15f, new Color(0.31f, 0.78f, 0.47f));
            CreateDiceData(DiceType.Ruby, "Ruby Dice", 500, 1f, 25000, 1.15f, new Color(0.88f, 0.07f, 0.37f));
            CreateDiceData(DiceType.Diamond, "Diamond Dice", 2000, 5f, 100000, 1.15f, new Color(0.73f, 0.95f, 1f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Dice Data assets generated successfully!");
        }

        private static void CreateDiceData(DiceType type, string displayName, double basePayout, float dmPerRoll, int shopBaseCost, float shopCostGrowth, Color tintColor)
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
                asset.rollCooldown = 0.5f;

                AssetDatabase.CreateAsset(asset, path);
            }
        }
    }
}
