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

        [MenuItem("Incredicer/Create All Skill Nodes")]
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
            // Design doc: ME-01 Loose Change - 5,000 DM, +25% money
            var looseChange = CreateNode(
                SkillNodeId.ME_LooseChange,
                "Loose Change",
                "+25% money from all dice rolls.",
                SkillBranch.MoneyEngine,
                1,
                new Vector2(-200, -100),
                5000, // Tier 1: 5,000 DM
                new List<SkillNodeId> { SkillNodeId.CORE_DarkMatterCore },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.GlobalMoneyAdditive, value = 0.25f }
                }
            );
            createdNodes.Add(looseChange);

            // Design doc: ME-02 Table Tax - 10,000 DM, +1% chance for bonus coin
            var tableTax = CreateNode(
                SkillNodeId.ME_TableTax,
                "Table Tax",
                "Each roll has +1% chance to spawn a bonus coin worth +50 flat money.",
                SkillBranch.MoneyEngine,
                1,
                new Vector2(-300, -150),
                10000, // Tier 1: 10,000 DM
                new List<SkillNodeId> { SkillNodeId.CORE_DarkMatterCore },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.TableTaxChance, value = 0.01f }
                }
            );
            createdNodes.Add(tableTax);

            // Design doc: ME-03 Compound Interest - 50,000 DM, +50% money multiplicative
            var compoundInterest = CreateNode(
                SkillNodeId.ME_CompoundInterest,
                "Compound Interest",
                "+50% money from all rolls (multiplicative).",
                SkillBranch.MoneyEngine,
                2,
                new Vector2(-200, -200),
                50000, // Tier 2: 50,000 DM
                new List<SkillNodeId> { SkillNodeId.ME_LooseChange },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.GlobalMoneyMultiplicative, value = 1.5f }
                }
            );
            createdNodes.Add(compoundInterest);

            // Design doc: ME-04 Tip Jar - 50,000 DM, bonus coin is 5% of current money
            var tipJar = CreateNode(
                SkillNodeId.ME_TipJar,
                "Tip Jar",
                "When Table Tax procs, bonus coin is 5% of current money instead of +50 flat.",
                SkillBranch.MoneyEngine,
                2,
                new Vector2(-300, -250),
                50000, // Tier 2: 50,000 DM
                new List<SkillNodeId> { SkillNodeId.ME_TableTax },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.TipJarScaling, value = 0.05f }
                }
            );
            createdNodes.Add(tipJar);

            // Design doc: ME-05 Big Payouts - 250,000 DM, global money x2
            var bigPayouts = CreateNode(
                SkillNodeId.ME_BigPayouts,
                "Big Payouts",
                "Global money from all sources ×2.",
                SkillBranch.MoneyEngine,
                3,
                new Vector2(-200, -300),
                250000, // Tier 3: 250,000 DM
                new List<SkillNodeId> { SkillNodeId.ME_CompoundInterest },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.GlobalMoneyMultiplicative, value = 2.0f }
                }
            );
            createdNodes.Add(bigPayouts);

            // Design doc: ME-06 Dark Dividends - 250,000 DM, gain money when gaining DM
            var darkDividends = CreateNode(
                SkillNodeId.ME_DarkDividends,
                "Dark Dividends",
                "Whenever you gain Dark Matter, also gain money equal to 1% of lifetime DM earned.",
                SkillBranch.MoneyEngine,
                3,
                new Vector2(-350, -300),
                250000, // Tier 3: 250,000 DM
                new List<SkillNodeId> { SkillNodeId.ME_TipJar },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.DarkMatterGainMultiplier, value = 1.0f } // Custom effect
                }
            );
            createdNodes.Add(darkDividends);

            // Design doc: ME-07 Jackpot Chance - 1,500,000 DM, 3% chance for 10x money
            var jackpotChance = CreateNode(
                SkillNodeId.ME_JackpotChance,
                "Jackpot Chance",
                "Rolls have a 3% chance to pay 10× their final money value.",
                SkillBranch.MoneyEngine,
                4,
                new Vector2(-250, -400),
                1500000, // Tier 4: 1,500,000 DM
                new List<SkillNodeId> { SkillNodeId.ME_BigPayouts, SkillNodeId.ME_DarkDividends },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.JackpotChance, value = 0.03f },
                    new SkillEffect { effectType = SkillEffectType.JackpotMultiplier, value = 5.0f } // 10x total (2x base * 5)
                }
            );
            createdNodes.Add(jackpotChance);

            // Design doc: ME-08 Infinite Float - 15,000,000 DM, helper +200%, manual -20%
            var infiniteFloat = CreateNode(
                SkillNodeId.ME_InfiniteFloat,
                "Infinite Float",
                "Helper-hand (idle) rolls: +200% money. Manual rolls: –20% money.",
                SkillBranch.MoneyEngine,
                5,
                new Vector2(-250, -500),
                15000000, // Tier 5: 15,000,000 DM
                new List<SkillNodeId> { SkillNodeId.ME_JackpotChance },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.IdleMoneyMultiplier, value = 3.0f }, // +200% = 3x
                    new SkillEffect { effectType = SkillEffectType.ManualMoneyMultiplier, value = 0.8f } // -20%
                }
            );
            createdNodes.Add(infiniteFloat);

            // ===== AUTOMATION =====
            // Design doc: AU-01 First Assistant - 7,500 DM, unlock helper hands + 1 free hand
            var firstAssistant = CreateNode(
                SkillNodeId.AU_FirstAssistant,
                "First Assistant",
                "Unlocks Helper Hands system. +1 free Helper Hand.",
                SkillBranch.Automation,
                1,
                new Vector2(0, -150),
                7500, // Tier 1: 7,500 DM
                new List<SkillNodeId> { SkillNodeId.CORE_DarkMatterCore },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.HelperHandMaxHandsAdd, value = 1 }
                }
            );
            createdNodes.Add(firstAssistant);

            // Design doc: AU-02 Greased Gears - 15,000 DM, helper hands 20% faster
            var greasedGears = CreateNode(
                SkillNodeId.AU_GreasedGears,
                "Greased Gears",
                "Helper Hands roll 20% faster.",
                SkillBranch.Automation,
                1,
                new Vector2(-50, -250),
                15000, // Tier 1: 15,000 DM
                new List<SkillNodeId> { SkillNodeId.CORE_DarkMatterCore },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.HelperHandSpeedMultiplier, value = 1.2f }
                }
            );
            createdNodes.Add(greasedGears);

            // Design doc: AU-03 More Hands - 60,000 DM, +5 max helper hand cap
            var moreHands = CreateNode(
                SkillNodeId.AU_MoreHands,
                "More Hands",
                "+5 to max Helper Hand cap.",
                SkillBranch.Automation,
                2,
                new Vector2(50, -250),
                60000, // Tier 2: 60,000 DM
                new List<SkillNodeId> { SkillNodeId.AU_FirstAssistant },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.HelperHandMaxHandsAdd, value = 5 }
                }
            );
            createdNodes.Add(moreHands);

            // Design doc: AU-04 Two-at-Once - 80,000 DM, each hand rolls 2 dice per cycle
            var twoAtOnce = CreateNode(
                SkillNodeId.AU_TwoAtOnce,
                "Two-at-Once",
                "Each Helper Hand now rolls 2 different dice per cycle.",
                SkillBranch.Automation,
                2,
                new Vector2(-50, -350),
                80000, // Tier 2: 80,000 DM
                new List<SkillNodeId> { SkillNodeId.AU_GreasedGears },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.HelperHandExtraRollsAdd, value = 1 }
                }
            );
            createdNodes.Add(twoAtOnce);

            // Design doc: AU-05 Overtime - 400,000 DM, offline at 100% speed
            var overtime = CreateNode(
                SkillNodeId.AU_Overtime,
                "Overtime",
                "When game is minimized, Helper Hands operate at 100% of normal speed.",
                SkillBranch.Automation,
                3,
                new Vector2(50, -350),
                400000, // Tier 3: 400,000 DM
                new List<SkillNodeId> { SkillNodeId.AU_MoreHands },
                new List<SkillEffect> {
                    // This is a special flag - full offline earnings
                }
            );
            createdNodes.Add(overtime);

            // Design doc: AU-06 Perfect Rhythm - 600,000 DM, helper hand cooldown -30%
            var perfectRhythm = CreateNode(
                SkillNodeId.AU_PerfectRhythm,
                "Perfect Rhythm",
                "Helper Hand roll cooldown –30% (stacks with Greased Gears).",
                SkillBranch.Automation,
                3,
                new Vector2(0, -450),
                600000, // Tier 3: 600,000 DM
                new List<SkillNodeId> { SkillNodeId.AU_TwoAtOnce },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.HelperHandSpeedMultiplier, value = 1.43f } // 1/0.7 ≈ 1.43
                }
            );
            createdNodes.Add(perfectRhythm);

            // Design doc: AU-07 Assembly Line - 3,500,000 DM, +50 max helper hand cap
            var assemblyLine = CreateNode(
                SkillNodeId.AU_AssemblyLine,
                "Assembly Line",
                "+50 to max Helper Hand cap. Each new hand costs +25% more.",
                SkillBranch.Automation,
                4,
                new Vector2(100, -450),
                3500000, // Tier 4: 3,500,000 DM
                new List<SkillNodeId> { SkillNodeId.AU_Overtime, SkillNodeId.AU_PerfectRhythm },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.HelperHandMaxHandsAdd, value = 50 }
                }
            );
            createdNodes.Add(assemblyLine);

            // Design doc: AU-08 Idle King - 20,000,000 DM, helper hands +50% DM but no extra money
            var idleKing = CreateNode(
                SkillNodeId.AU_IdleKing,
                "Idle King",
                "Helper-Hand rolls generate +50% extra Dark Matter, but no extra money.",
                SkillBranch.Automation,
                5,
                new Vector2(50, -550),
                20000000, // Tier 5: 20,000,000 DM
                new List<SkillNodeId> { SkillNodeId.AU_AssemblyLine },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.SpecialFlag, specialFlag = SpecialFlagType.IdleKing }
                }
            );
            createdNodes.Add(idleKing);

            // ===== DICE EVOLUTION =====
            // Design doc: DE-01 Bronze Dice - 10,000 DM
            var bronzeDice = CreateNode(
                SkillNodeId.DE_BronzeDice,
                "Bronze Dice",
                "Unlock Bronze dice for purchase. 2x base earnings.",
                SkillBranch.DiceEvolution,
                1,
                new Vector2(200, -100),
                10000, // Tier 1: 10,000 DM
                new List<SkillNodeId> { SkillNodeId.CORE_DarkMatterCore },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.UnlockDiceType, targetDiceType = DiceType.Bronze }
                }
            );
            createdNodes.Add(bronzeDice);

            // Design doc: DE-02 Polished Bronze - 20,000 DM
            var polishedBronze = CreateNode(
                SkillNodeId.DE_PolishedBronze,
                "Polished Bronze",
                "Bronze dice earn +50% more money.",
                SkillBranch.DiceEvolution,
                1,
                new Vector2(150, -200),
                20000, // Tier 1: 20,000 DM
                new List<SkillNodeId> { SkillNodeId.DE_BronzeDice },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.DiceMoneyMultiplier, value = 1.5f, targetDiceType = DiceType.Bronze }
                }
            );
            createdNodes.Add(polishedBronze);

            // Design doc: DE-03 Silver Dice - 50,000 DM
            var silverDice = CreateNode(
                SkillNodeId.DE_SilverDice,
                "Silver Dice",
                "Unlock Silver dice for purchase. 4x base earnings.",
                SkillBranch.DiceEvolution,
                2,
                new Vector2(250, -200),
                50000, // Tier 2: 50,000 DM
                new List<SkillNodeId> { SkillNodeId.DE_BronzeDice },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.UnlockDiceType, targetDiceType = DiceType.Silver }
                }
            );
            createdNodes.Add(silverDice);

            // Design doc: DE-04 Silver Veins - 100,000 DM
            var silverVeins = CreateNode(
                SkillNodeId.DE_SilverVeins,
                "Silver Veins",
                "Silver dice earn +50% more money.",
                SkillBranch.DiceEvolution,
                2,
                new Vector2(200, -300),
                100000, // Tier 2: 100,000 DM
                new List<SkillNodeId> { SkillNodeId.DE_SilverDice },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.DiceMoneyMultiplier, value = 1.5f, targetDiceType = DiceType.Silver }
                }
            );
            createdNodes.Add(silverVeins);

            // Design doc: DE-05 Gold Dice - 250,000 DM
            var goldDice = CreateNode(
                SkillNodeId.DE_GoldDice,
                "Gold Dice",
                "Unlock Gold dice for purchase. 8x base earnings.",
                SkillBranch.DiceEvolution,
                3,
                new Vector2(300, -300),
                250000, // Tier 3: 250,000 DM
                new List<SkillNodeId> { SkillNodeId.DE_SilverDice },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.UnlockDiceType, targetDiceType = DiceType.Gold }
                }
            );
            createdNodes.Add(goldDice);

            // Design doc: DE-06 Gold Rush - 500,000 DM
            var goldRush = CreateNode(
                SkillNodeId.DE_GoldRush,
                "Gold Rush",
                "Gold dice earn +75% more money.",
                SkillBranch.DiceEvolution,
                3,
                new Vector2(250, -400),
                500000, // Tier 3: 500,000 DM
                new List<SkillNodeId> { SkillNodeId.DE_GoldDice },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.DiceMoneyMultiplier, value = 1.75f, targetDiceType = DiceType.Gold }
                }
            );
            createdNodes.Add(goldRush);

            // Design doc: DE-07 Emerald Dice - 2,000,000 DM
            var emeraldDice = CreateNode(
                SkillNodeId.DE_EmeraldDice,
                "Emerald Dice",
                "Unlock Emerald dice for purchase. 16x base earnings.",
                SkillBranch.DiceEvolution,
                4,
                new Vector2(350, -400),
                2000000, // Tier 4: 2,000,000 DM
                new List<SkillNodeId> { SkillNodeId.DE_GoldDice },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.UnlockDiceType, targetDiceType = DiceType.Emerald }
                }
            );
            createdNodes.Add(emeraldDice);

            // Design doc: DE-08 Gem Synergy - 5,000,000 DM
            var gemSynergy = CreateNode(
                SkillNodeId.DE_GemSynergy,
                "Gem Synergy",
                "All gem dice (Emerald+) earn +100% more money.",
                SkillBranch.DiceEvolution,
                4,
                new Vector2(300, -500),
                5000000, // Tier 4: 5,000,000 DM
                new List<SkillNodeId> { SkillNodeId.DE_EmeraldDice, SkillNodeId.DE_GoldRush },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.DiceMoneyMultiplier, value = 2.0f, targetDiceType = DiceType.Emerald },
                    new SkillEffect { effectType = SkillEffectType.DiceMoneyMultiplier, value = 2.0f, targetDiceType = DiceType.Ruby },
                    new SkillEffect { effectType = SkillEffectType.DiceMoneyMultiplier, value = 2.0f, targetDiceType = DiceType.Diamond }
                }
            );
            createdNodes.Add(gemSynergy);

            // Design doc: DE-09 Ruby Dice - 12,000,000 DM
            var rubyDice = CreateNode(
                SkillNodeId.DE_RubyDice,
                "Ruby Dice",
                "Unlock Ruby dice for purchase. 32x base earnings.",
                SkillBranch.DiceEvolution,
                5,
                new Vector2(400, -500),
                12000000, // Tier 5: 12,000,000 DM
                new List<SkillNodeId> { SkillNodeId.DE_EmeraldDice },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.UnlockDiceType, targetDiceType = DiceType.Ruby }
                }
            );
            createdNodes.Add(rubyDice);

            // Design doc: DE-10 Diamond Dice - 30,000,000 DM
            var diamondDice = CreateNode(
                SkillNodeId.DE_DiamondDice,
                "Diamond Dice",
                "Unlock Diamond dice for purchase. 64x base earnings.",
                SkillBranch.DiceEvolution,
                5,
                new Vector2(350, -600),
                30000000, // Tier 5: 30,000,000 DM
                new List<SkillNodeId> { SkillNodeId.DE_RubyDice, SkillNodeId.DE_GemSynergy },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.UnlockDiceType, targetDiceType = DiceType.Diamond }
                }
            );
            createdNodes.Add(diamondDice);

            // ===== SKILLS & UTILITY =====
            // Design doc: SK-01 Quick Flick - 6,000 DM
            var quickFlick = CreateNode(
                SkillNodeId.SK_QuickFlick,
                "Quick Flick",
                "Manual rolls earn +50% more money.",
                SkillBranch.SkillsUtility,
                1,
                new Vector2(-100, -100),
                6000, // Tier 1: 6,000 DM
                new List<SkillNodeId> { SkillNodeId.CORE_DarkMatterCore },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.ManualMoneyMultiplier, value = 1.5f }
                }
            );
            createdNodes.Add(quickFlick);

            // Design doc: SK-02 Long Reach - 12,000 DM
            var longReach = CreateNode(
                SkillNodeId.SK_LongReach,
                "Long Reach",
                "Increase click radius to roll dice from further away.",
                SkillBranch.SkillsUtility,
                1,
                new Vector2(-150, -200),
                12000, // Tier 1: 12,000 DM
                new List<SkillNodeId> { SkillNodeId.SK_QuickFlick },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.CursorRollRadiusAdd, value = 0.5f }
                }
            );
            createdNodes.Add(longReach);

            // Design doc: SK-03 Roll Burst - 60,000 DM
            var rollBurstII = CreateNode(
                SkillNodeId.SK_RollBurstII,
                "Roll Burst",
                "Unlock the Roll Burst active skill - roll all dice at once!",
                SkillBranch.SkillsUtility,
                2,
                new Vector2(-50, -200),
                60000, // Tier 2: 60,000 DM
                new List<SkillNodeId> { SkillNodeId.SK_QuickFlick },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.UnlockActiveSkill, unlockSkill = ActiveSkillType.RollBurst }
                }
            );
            createdNodes.Add(rollBurstII);

            // Design doc: SK-04 Rapid Cooldown - 100,000 DM
            var rapidCooldown = CreateNode(
                SkillNodeId.SK_RapidCooldown,
                "Rapid Cooldown",
                "Active skills recharge 25% faster.",
                SkillBranch.SkillsUtility,
                2,
                new Vector2(-100, -300),
                100000, // Tier 2: 100,000 DM
                new List<SkillNodeId> { SkillNodeId.SK_RollBurstII },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.SkillCooldownMultiplier, value = 0.75f }
                }
            );
            createdNodes.Add(rapidCooldown);

            // Design doc: SK-05 Focused Gravity - 350,000 DM
            var focusedGravity = CreateNode(
                SkillNodeId.SK_FocusedGravity,
                "Focused Gravity",
                "Dice tend to cluster together after rolling.",
                SkillBranch.SkillsUtility,
                3,
                new Vector2(-200, -300),
                350000, // Tier 3: 350,000 DM
                new List<SkillNodeId> { SkillNodeId.SK_LongReach },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.SpecialFlag, specialFlag = SpecialFlagType.FocusedGravity }
                }
            );
            createdNodes.Add(focusedGravity);

            // Design doc: SK-06 Precision Aim - 500,000 DM
            var precisionAim = CreateNode(
                SkillNodeId.SK_PrecisionAim,
                "Precision Aim",
                "Hold mouse to gently pull dice toward cursor.",
                SkillBranch.SkillsUtility,
                3,
                new Vector2(-200, -400),
                500000, // Tier 3: 500,000 DM
                new List<SkillNodeId> { SkillNodeId.SK_FocusedGravity },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.SpecialFlag, specialFlag = SpecialFlagType.PrecisionAim }
                }
            );
            createdNodes.Add(precisionAim);

            // Design doc: SK-07 Hyperburst - 2,500,000 DM
            var hyperburst = CreateNode(
                SkillNodeId.SK_Hyperburst,
                "Hyperburst",
                "Doubles all dice rolls for 10 seconds!",
                SkillBranch.SkillsUtility,
                4,
                new Vector2(-50, -400),
                2500000, // Tier 4: 2,500,000 DM
                new List<SkillNodeId> { SkillNodeId.SK_RapidCooldown },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.UnlockActiveSkill, unlockSkill = ActiveSkillType.Hyperburst }
                }
            );
            createdNodes.Add(hyperburst);

            // Design doc: SK-08 Time Dilation - 18,000,000 DM
            var timeDilation = CreateNode(
                SkillNodeId.SK_TimeDilation,
                "Time Dilation",
                "While an active skill is running, earn 2x Dark Matter.",
                SkillBranch.SkillsUtility,
                5,
                new Vector2(-100, -500),
                18000000, // Tier 5: 18,000,000 DM
                new List<SkillNodeId> { SkillNodeId.SK_Hyperburst, SkillNodeId.SK_PrecisionAim },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.SpecialFlag, specialFlag = SpecialFlagType.TimeDilation }
                }
            );
            createdNodes.Add(timeDilation);

            // SK-09 Daily Login - 5,000 DM (early unlock, requires Quick Flick)
            var dailyLogin = CreateNode(
                SkillNodeId.SK_DailyLogin,
                "Daily Rewards",
                "Unlock daily login bonuses - roll dice for amazing rewards every day!",
                SkillBranch.SkillsUtility,
                2,
                new Vector2(100, -200),
                5000, // Tier 2: 5,000 DM - early unlock
                new List<SkillNodeId> { SkillNodeId.SK_QuickFlick },
                new List<SkillEffect> {
                    new SkillEffect { effectType = SkillEffectType.UnlockActiveSkill, unlockSkill = ActiveSkillType.DailyLogin }
                }
            );
            createdNodes.Add(dailyLogin);

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
