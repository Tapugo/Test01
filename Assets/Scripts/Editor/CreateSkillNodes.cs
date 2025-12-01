using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Incredicer.Core;
using Incredicer.Skills;

namespace Incredicer.Editor
{
    public static class CreateSkillNodes
    {
        private const string SKILL_NODES_PATH = "Assets/Data/SkillNodes";

        [MenuItem("Incredicer/Create Sample Skill Nodes")]
        public static void Execute()
        {
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
            {
                AssetDatabase.CreateFolder("Assets", "Data");
            }
            if (!AssetDatabase.IsValidFolder(SKILL_NODES_PATH))
            {
                AssetDatabase.CreateFolder("Assets/Data", "SkillNodes");
            }

            List<SkillNodeData> createdNodes = new List<SkillNodeData>();

            // ===== CORE =====
            var darkMatterCore = CreateNode(
                SkillNodeId.CORE_DarkMatterCore,
                "Dark Matter Core",
                "The source of all power. Unlocks the skill tree.",
                SkillBranch.Core,
                0,
                Vector2.zero,
                0 // Free - first node
            );
            createdNodes.Add(darkMatterCore);

            // ===== MONEY ENGINE =====
            var looseChange = CreateNode(
                SkillNodeId.ME_LooseChange,
                "Loose Change",
                "All dice earn +10% more money.",
                SkillBranch.MoneyEngine,
                1,
                new Vector2(-200, -100),
                1,
                new List<SkillNodeId> { SkillNodeId.CORE_DarkMatterCore },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.GlobalMoneyMultiplicative, value = 1.1f }
                }
            );
            createdNodes.Add(looseChange);

            var compoundInterest = CreateNode(
                SkillNodeId.ME_CompoundInterest,
                "Compound Interest",
                "All dice earn +25% more money.",
                SkillBranch.MoneyEngine,
                2,
                new Vector2(-200, -200),
                3,
                new List<SkillNodeId> { SkillNodeId.ME_LooseChange },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.GlobalMoneyMultiplicative, value = 1.25f }
                }
            );
            createdNodes.Add(compoundInterest);

            var jackpotChance = CreateNode(
                SkillNodeId.ME_JackpotChance,
                "Lucky Streak",
                "5% chance for rolls to be jackpots (2x value).",
                SkillBranch.MoneyEngine,
                3,
                new Vector2(-200, -300),
                5,
                new List<SkillNodeId> { SkillNodeId.ME_CompoundInterest },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.JackpotChance, value = 0.05f }
                }
            );
            createdNodes.Add(jackpotChance);

            var bigPayouts = CreateNode(
                SkillNodeId.ME_BigPayouts,
                "Big Payouts",
                "Jackpots are now worth 3x instead of 2x.",
                SkillBranch.MoneyEngine,
                4,
                new Vector2(-200, -400),
                10,
                new List<SkillNodeId> { SkillNodeId.ME_JackpotChance },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.JackpotMultiplier, value = 1.5f }
                }
            );
            createdNodes.Add(bigPayouts);

            // ===== DICE EVOLUTION =====
            var bronzeDice = CreateNode(
                SkillNodeId.DE_BronzeDice,
                "Bronze Dice",
                "Unlock Bronze dice for purchase.",
                SkillBranch.DiceEvolution,
                1,
                new Vector2(200, -100),
                2,
                new List<SkillNodeId> { SkillNodeId.CORE_DarkMatterCore },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.UnlockDiceType, targetDiceType = DiceType.Bronze }
                }
            );
            createdNodes.Add(bronzeDice);

            var silverDice = CreateNode(
                SkillNodeId.DE_SilverDice,
                "Silver Dice",
                "Unlock Silver dice for purchase.",
                SkillBranch.DiceEvolution,
                2,
                new Vector2(200, -200),
                5,
                new List<SkillNodeId> { SkillNodeId.DE_BronzeDice },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.UnlockDiceType, targetDiceType = DiceType.Silver }
                }
            );
            createdNodes.Add(silverDice);

            var goldDice = CreateNode(
                SkillNodeId.DE_GoldDice,
                "Gold Dice",
                "Unlock Gold dice for purchase.",
                SkillBranch.DiceEvolution,
                3,
                new Vector2(200, -300),
                15,
                new List<SkillNodeId> { SkillNodeId.DE_SilverDice },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.UnlockDiceType, targetDiceType = DiceType.Gold }
                }
            );
            createdNodes.Add(goldDice);

            // ===== SKILLS & UTILITY =====
            var quickFlick = CreateNode(
                SkillNodeId.SK_QuickFlick,
                "Quick Flick",
                "Manual rolls earn +50% more money.",
                SkillBranch.SkillsUtility,
                1,
                new Vector2(0, -150),
                2,
                new List<SkillNodeId> { SkillNodeId.CORE_DarkMatterCore },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.ManualMoneyMultiplier, value = 1.5f }
                }
            );
            createdNodes.Add(quickFlick);

            var longReach = CreateNode(
                SkillNodeId.SK_LongReach,
                "Long Reach",
                "Increase click radius to roll dice from further away.",
                SkillBranch.SkillsUtility,
                2,
                new Vector2(0, -250),
                3,
                new List<SkillNodeId> { SkillNodeId.SK_QuickFlick },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.CursorRollRadiusAdd, value = 0.5f }
                }
            );
            createdNodes.Add(longReach);

            var rapidCooldown = CreateNode(
                SkillNodeId.SK_RapidCooldown,
                "Rapid Cooldown",
                "Active skills recharge 25% faster.",
                SkillBranch.SkillsUtility,
                3,
                new Vector2(0, -350),
                8,
                new List<SkillNodeId> { SkillNodeId.SK_LongReach },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.SkillCooldownMultiplier, value = 0.75f }
                }
            );
            createdNodes.Add(rapidCooldown);

            // ===== DARK MATTER =====
            var darkDividends = CreateNode(
                SkillNodeId.ME_DarkDividends,
                "Dark Dividends",
                "Gain +50% more Dark Matter from all sources.",
                SkillBranch.MoneyEngine,
                2,
                new Vector2(-350, -200),
                5,
                new List<SkillNodeId> { SkillNodeId.ME_LooseChange },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.DarkMatterGainMultiplier, value = 1.5f }
                }
            );
            createdNodes.Add(darkDividends);

            // Refresh asset database
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Try to assign to SkillTreeManager
            SkillTreeManager stm = Object.FindObjectOfType<SkillTreeManager>();
            if (stm != null)
            {
                SerializedObject so = new SerializedObject(stm);
                SerializedProperty nodesProp = so.FindProperty("allSkillNodes");
                nodesProp.ClearArray();

                for (int i = 0; i < createdNodes.Count; i++)
                {
                    nodesProp.InsertArrayElementAtIndex(i);
                    nodesProp.GetArrayElementAtIndex(i).objectReferenceValue = createdNodes[i];
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(stm);
                Debug.Log($"[CreateSkillNodes] Assigned {createdNodes.Count} nodes to SkillTreeManager");
            }

            Debug.Log($"[CreateSkillNodes] Created {createdNodes.Count} skill nodes at {SKILL_NODES_PATH}");
        }

        private static SkillNodeData CreateNode(
            SkillNodeId id,
            string displayName,
            string description,
            SkillBranch branch,
            int tier,
            Vector2 position,
            double cost,
            List<SkillNodeId> prerequisites = null,
            List<SkillEffect> effects = null)
        {
            string path = $"{SKILL_NODES_PATH}/{id}.asset";

            // Check if already exists
            SkillNodeData existing = AssetDatabase.LoadAssetAtPath<SkillNodeData>(path);
            if (existing != null)
            {
                Debug.Log($"[CreateSkillNodes] Updating existing: {id}");
                existing.nodeId = id;
                existing.displayName = displayName;
                existing.description = description;
                existing.branch = branch;
                existing.tier = tier;
                existing.treePosition = position;
                existing.darkMatterCost = cost;
                existing.prerequisites = prerequisites ?? new List<SkillNodeId>();
                existing.effects = effects ?? new List<SkillEffect>();
                EditorUtility.SetDirty(existing);
                return existing;
            }

            // Create new
            SkillNodeData node = ScriptableObject.CreateInstance<SkillNodeData>();
            node.nodeId = id;
            node.displayName = displayName;
            node.description = description;
            node.branch = branch;
            node.tier = tier;
            node.treePosition = position;
            node.darkMatterCost = cost;
            node.prerequisites = prerequisites ?? new List<SkillNodeId>();
            node.effects = effects ?? new List<SkillEffect>();

            AssetDatabase.CreateAsset(node, path);
            Debug.Log($"[CreateSkillNodes] Created: {id}");
            return node;
        }
    }
}
