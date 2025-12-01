using System;
using System.Collections.Generic;
using UnityEngine;
using Incredicer.Core;

namespace Incredicer.Skills
{
    /// <summary>
    /// Represents a single effect applied by a skill node.
    /// </summary>
    [Serializable]
    public class SkillEffect
    {
        public SkillEffectType effectType;
        public float value;
        public DiceType targetDiceType; // For dice-specific effects
        public ActiveSkillType unlockSkill; // For skill unlocks
        public SpecialFlagType specialFlag; // For special flags
    }

    /// <summary>
    /// ScriptableObject defining a skill node in the skill tree.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkillNode", menuName = "Incredicer/Skill Node")]
    public class SkillNodeData : ScriptableObject
    {
        [Header("Identity")]
        public SkillNodeId nodeId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("Tree Position")]
        public SkillBranch branch;
        public int tier; // 0 = root, higher = further from center
        public Vector2 treePosition; // Position in skill tree UI

        [Header("Cost")]
        public double darkMatterCost = 1;

        [Header("Requirements")]
        public List<SkillNodeId> prerequisites = new List<SkillNodeId>();

        [Header("Effects")]
        public List<SkillEffect> effects = new List<SkillEffect>();

        /// <summary>
        /// Applies all effects of this skill node to game stats.
        /// </summary>
        public void ApplyEffects()
        {
            if (GameStats.Instance == null) return;

            foreach (var effect in effects)
            {
                ApplyEffect(effect);
            }
        }

        /// <summary>
        /// Removes all effects of this skill node from game stats.
        /// </summary>
        public void RemoveEffects()
        {
            if (GameStats.Instance == null) return;

            foreach (var effect in effects)
            {
                RemoveEffect(effect);
            }
        }

        private void ApplyEffect(SkillEffect effect)
        {
            var stats = GameStats.Instance;

            switch (effect.effectType)
            {
                case SkillEffectType.GlobalMoneyMultiplicative:
                    stats.GlobalMoneyMultiplier *= effect.value;
                    break;

                case SkillEffectType.GlobalMoneyAdditive:
                    stats.GlobalMoneyMultiplier += effect.value;
                    break;

                case SkillEffectType.ManualMoneyMultiplier:
                    stats.ManualMoneyMultiplier *= effect.value;
                    break;

                case SkillEffectType.IdleMoneyMultiplier:
                    stats.IdleMoneyMultiplier *= effect.value;
                    break;

                case SkillEffectType.JackpotChance:
                    stats.JackpotChance += effect.value;
                    break;

                case SkillEffectType.JackpotMultiplier:
                    stats.JackpotMultiplier *= effect.value;
                    break;

                case SkillEffectType.DarkMatterGainMultiplier:
                    stats.DarkMatterGainMultiplier *= effect.value;
                    break;

                case SkillEffectType.HelperHandSpeedMultiplier:
                    stats.HelperHandSpeedMultiplier *= effect.value;
                    break;

                case SkillEffectType.HelperHandExtraRollsAdd:
                    stats.HelperHandExtraRolls += (int)effect.value;
                    break;

                case SkillEffectType.SkillCooldownMultiplier:
                    stats.SkillCooldownMultiplier *= effect.value;
                    break;

                case SkillEffectType.ActiveSkillDurationMultiplier:
                    stats.ActiveSkillDurationMultiplier *= effect.value;
                    break;

                case SkillEffectType.CursorRollRadiusAdd:
                    stats.CursorRollRadius += effect.value;
                    break;

                case SkillEffectType.SpecialFlag:
                    ApplySpecialFlag(effect.specialFlag, true);
                    break;

                case SkillEffectType.UnlockDiceType:
                    UnlockDice(effect.targetDiceType);
                    break;

                case SkillEffectType.UnlockActiveSkill:
                    UnlockSkill(effect.unlockSkill);
                    break;
            }
        }

        private void RemoveEffect(SkillEffect effect)
        {
            var stats = GameStats.Instance;

            switch (effect.effectType)
            {
                case SkillEffectType.GlobalMoneyMultiplicative:
                    if (effect.value != 0) stats.GlobalMoneyMultiplier /= effect.value;
                    break;

                case SkillEffectType.GlobalMoneyAdditive:
                    stats.GlobalMoneyMultiplier -= effect.value;
                    break;

                case SkillEffectType.ManualMoneyMultiplier:
                    if (effect.value != 0) stats.ManualMoneyMultiplier /= effect.value;
                    break;

                case SkillEffectType.IdleMoneyMultiplier:
                    if (effect.value != 0) stats.IdleMoneyMultiplier /= effect.value;
                    break;

                case SkillEffectType.JackpotChance:
                    stats.JackpotChance -= effect.value;
                    break;

                case SkillEffectType.JackpotMultiplier:
                    if (effect.value != 0) stats.JackpotMultiplier /= effect.value;
                    break;

                case SkillEffectType.DarkMatterGainMultiplier:
                    if (effect.value != 0) stats.DarkMatterGainMultiplier /= effect.value;
                    break;

                case SkillEffectType.HelperHandSpeedMultiplier:
                    if (effect.value != 0) stats.HelperHandSpeedMultiplier /= effect.value;
                    break;

                case SkillEffectType.HelperHandExtraRollsAdd:
                    stats.HelperHandExtraRolls -= (int)effect.value;
                    break;

                case SkillEffectType.SkillCooldownMultiplier:
                    if (effect.value != 0) stats.SkillCooldownMultiplier /= effect.value;
                    break;

                case SkillEffectType.ActiveSkillDurationMultiplier:
                    if (effect.value != 0) stats.ActiveSkillDurationMultiplier /= effect.value;
                    break;

                case SkillEffectType.CursorRollRadiusAdd:
                    stats.CursorRollRadius -= effect.value;
                    break;

                case SkillEffectType.SpecialFlag:
                    ApplySpecialFlag(effect.specialFlag, false);
                    break;
            }
        }

        private void ApplySpecialFlag(SpecialFlagType flag, bool enable)
        {
            var stats = GameStats.Instance;

            switch (flag)
            {
                case SpecialFlagType.IdleKing:
                    stats.IdleKingActive = enable;
                    break;
                case SpecialFlagType.TimeDilation:
                    stats.TimeDilationActive = enable;
                    break;
            }
        }

        private void UnlockDice(DiceType type)
        {
            if (Dice.DiceManager.Instance != null)
            {
                Dice.DiceManager.Instance.UnlockDiceType(type);
            }
        }

        private void UnlockSkill(ActiveSkillType skill)
        {
            if (SkillTreeManager.Instance != null)
            {
                SkillTreeManager.Instance.UnlockActiveSkill(skill);
            }
        }
    }
}
