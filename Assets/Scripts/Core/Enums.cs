namespace Incredicer.Core
{
    /// <summary>
    /// All dice types in the game, from lowest to highest tier.
    /// </summary>
    public enum DiceType
    {
        Basic,
        Bronze,
        Silver,
        Gold,
        Emerald,
        Ruby,
        Diamond
    }

    /// <summary>
    /// Skill tree branches.
    /// </summary>
    public enum SkillBranch
    {
        Core,
        MoneyEngine,
        Automation,
        DiceEvolution,
        SkillsUtility
    }

    /// <summary>
    /// All skill effect types that can be applied by skill nodes.
    /// </summary>
    public enum SkillEffectType
    {
        // Money modifiers
        GlobalMoneyAdditive,
        GlobalMoneyMultiplicative,
        ManualMoneyMultiplier,
        IdleMoneyMultiplier,

        // Jackpot
        JackpotChance,
        JackpotMultiplier,

        // Dice-specific
        DiceMoneyMultiplier,
        DiceDmPerRollAdd,

        // Helper hands
        HelperHandSpeedMultiplier,
        HelperHandMaxHandsAdd,
        HelperHandExtraRollsAdd,

        // Dark matter
        DarkMatterGainMultiplier,

        // Skills
        SkillCooldownMultiplier,
        ActiveSkillDurationMultiplier,

        // Unlocks
        UnlockDiceType,
        UnlockActiveSkill,

        // Special flags
        SpecialFlag,

        // Cursor/Rolling
        CursorRollRadiusAdd,

        // Table Tax / Tip mechanics
        TableTaxChance,
        TipJarScaling
    }

    /// <summary>
    /// Special flag types for complex skill effects.
    /// </summary>
    public enum SpecialFlagType
    {
        None,
        IdleKing,           // Helper rolls earn extra DM but not extra money
        TimeDilation,       // DM doubled while active skill is running
        FocusedGravity,     // Dice stay clustered
        PrecisionAim        // Holding mouse pulls dice toward cursor
    }

    /// <summary>
    /// Active skill types.
    /// </summary>
    public enum ActiveSkillType
    {
        None,
        RollBurst,
        Hyperburst
    }

    /// <summary>
    /// All skill node IDs for the skill tree.
    /// </summary>
    public enum SkillNodeId
    {
        // Core
        CORE_DarkMatterCore,

        // Money Engine (ME)
        ME_LooseChange,
        ME_TableTax,
        ME_CompoundInterest,
        ME_TipJar,
        ME_BigPayouts,
        ME_DarkDividends,
        ME_JackpotChance,
        ME_InfiniteFloat,

        // Automation (AU)
        AU_FirstAssistant,
        AU_GreasedGears,
        AU_MoreHands,
        AU_TwoAtOnce,
        AU_Overtime,
        AU_PerfectRhythm,
        AU_AssemblyLine,
        AU_IdleKing,

        // Dice Evolution (DE)
        DE_BronzeDice,
        DE_PolishedBronze,
        DE_SilverDice,
        DE_SilverVeins,
        DE_GoldDice,
        DE_GoldRush,
        DE_EmeraldDice,
        DE_GemSynergy,
        DE_RubyDice,
        DE_DiamondDice,

        // Skills & Utility (SK)
        SK_QuickFlick,
        SK_LongReach,
        SK_RollBurstII,
        SK_RapidCooldown,
        SK_FocusedGravity,
        SK_PrecisionAim,
        SK_Hyperburst,
        SK_TimeDilation
    }
}
