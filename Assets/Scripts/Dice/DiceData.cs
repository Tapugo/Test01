using UnityEngine;
using Incredicer.Core;

namespace Incredicer.Dice
{
    /// <summary>
    /// ScriptableObject containing all data for a dice type.
    /// </summary>
    [CreateAssetMenu(menuName = "Incredicer/DiceData", fileName = "NewDiceData")]
    public class DiceData : ScriptableObject
    {
        [Header("Identity")]
        public DiceType type;
        public string displayName;
        
        [Header("Payouts")]
        [Tooltip("Base money earned per roll")]
        public double basePayout = 1;
        
        [Tooltip("Base dark matter earned per roll (0 for low tiers)")]
        public float dmPerRoll = 0;

        [Header("Visuals")]
        public Sprite sprite;
        public Color tintColor = Color.white;

        [Header("Shop")]
        [Tooltip("Base cost to buy this dice in the shop")]
        public int shopBaseCost = 10;
        
        [Tooltip("Cost multiplier per owned dice of this type (e.g., 1.15 = 15% increase)")]
        public float shopCostGrowth = 1.15f;

        [Header("Rolling")]
        [Tooltip("Minimum time between rolls for this dice")]
        public float rollCooldown = 0.5f;

        /// <summary>
        /// Calculates the current shop price based on how many of this type the player owns.
        /// </summary>
        public double GetCurrentPrice(int ownedCount)
        {
            return shopBaseCost * System.Math.Pow(shopCostGrowth, ownedCount);
        }
    }
}
