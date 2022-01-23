//
//  Rogue Legacy Randomizer - Skill.cs
//  Last Modified 2022-01-23
//
//  This project is based on the modified disassembly of Rogue Legacy's engine, with permission to do so by its
//  original creators. Therefore, the former creators' copyright notice applies to the original disassembly.
//
//  Original Source - © 2011-2015, Cellar Door Games Inc.
//  Rogue Legacy™ is a trademark or registered trademark of Cellar Door Games Inc. All Rights Reserved.
//

namespace RogueCastle.Enums
{
    public enum Skill
    {
        Null,
        Filler,
        HealthUp,
        InvulnerabilityTimeUp,
        DeathDodge,
        AttackUp,
        DownStrikeUp,
        CritChanceUp,
        CritDamageUp,
        MagicDamageUp,
        ManaUp,
        ManaCostDown,
        Smithy,
        Enchanter,
        Architect,
        EquipUp,
        ArmorUp,
        GoldGainUp,
        PricesDown,
        PotionUp,
        RandomizeChildren,
        LichUnlock,
        BankerUnlock,
        SpellswordUnlock,
        NinjaUnlock,
        KnightUnlock,
        KnightUp,
        MageUnlock,
        MageUp,
        AssassinUnlock,
        AssassinUp,
        BankerUp,
        BarbarianUnlock,
        BarbarianUp,
        LichUp,
        NinjaUp,
        SpellSwordUp,
        SuperSecret,
        Traitorous,
        Divider,
        AttackSpeedUp,
        InvulnerabilityAttackUp,
        HealthUpFinal,
        EquipUpFinal,
        DamageUpFinal,
        ManaUpFinal,
        XpGainUp,
        GoldFlatBonus,
        ManaRegenUp,
        Run,
        Block,
        Cartographer,
        EnvDamageDown,
        GoldLossDown,
        VampireUp,
        StoutHeart,
        QuickOfBreath,
        BornToRun,
        OutTheGate,
        Perfectionist,
        Guru,
        IronLung,
        SwordMaster,
        Tank,
        Vampire,
        SecondChance,
        PeaceOfMind,
        CartographyNinja,
        StrongMan,
        Suicidalist,
        CritBarbarian,
        Magician,
        Keymaster,
        OneTimeOnly,
        CuttingOutEarly,
        Quaffer,
        SpellSword,
        Sorcerer,
        WellEndowed,
        TreasureHunter,
        MortarMaster,
        ExplosiveExpert,
        Icicle,
        Ender,
        Manor,

        // Manor Pieces
        ManorGroundRoad,
        ManorMainBase,
        ManorMainWindowBottom,
        ManorMainWindowTop,
        ManorMainRoof,
        ManorLeftWingBase,
        ManorLeftWingWindow,
        ManorLeftWingRoof,
        ManorLeftBigBase,
        ManorLeftBigUpper1,
        ManorLeftBigUpper2,
        ManorLeftBigWindows,
        ManorLeftBigRoof,
        ManorLeftFarBase,
        ManorLeftFarRoof,
        ManorLeftExtension,
        ManorLeftTree1,
        ManorLeftTree2,
        ManorRightWingBase,
        ManorRightWingWindow,
        ManorRightWingRoof,
        ManorRightBigBase,
        ManorRightBigUpper,
        ManorRightBigRoof,
        ManorRightHighBase,
        ManorRightHighUpper,
        ManorRightHighTower,
        ManorRightExtension,
        ManorRightTree,
        ManorObservatoryBase,
        ManorObservatoryTelescope
    }

    public static class SkillExtensions
    {
        public static float GetSkillStat(this Skill skill)
        {
            return skill switch
            {
                Skill.HealthUp              => Game.ScreenManager.Player.MaxHealth,
                Skill.HealthUpFinal         => Game.ScreenManager.Player.MaxHealth,
                Skill.InvulnerabilityTimeUp => Game.ScreenManager.Player.InvincibilityTime,
                Skill.DeathDodge            => SkillSystem.GetSkill(Skill.DeathDodge).ModifierAmount * 100f,
                Skill.AttackUp              => Game.ScreenManager.Player.Damage,
                Skill.DamageUpFinal         => Game.ScreenManager.Player.Damage,
                Skill.DownStrikeUp          => SkillSystem.GetSkill(Skill.DownStrikeUp).ModifierAmount * 100f,
                Skill.CritChanceUp          => Game.ScreenManager.Player.TotalCritChance,
                Skill.CritDamageUp          => Game.ScreenManager.Player.TotalCriticalDamage * 100f,
                Skill.MagicDamageUp         => Game.ScreenManager.Player.TotalMagicDamage,
                Skill.ManaUp                => Game.ScreenManager.Player.MaxMana,
                Skill.ManaUpFinal           => Game.ScreenManager.Player.MaxMana,
                Skill.ManaCostDown          => SkillSystem.GetSkill(Skill.ManaCostDown).ModifierAmount * 100f,
                Skill.EquipUp               => Game.ScreenManager.Player.MaxWeight,
                Skill.EquipUpFinal          => Game.ScreenManager.Player.MaxWeight,
                Skill.ArmorUp               => Game.ScreenManager.Player.TotalArmor,
                Skill.GoldGainUp            => Game.ScreenManager.Player.TotalGoldBonus,
                Skill.PricesDown            => SkillSystem.GetSkill(Skill.PricesDown).ModifierAmount * 100f,
                Skill.PotionUp              => (0.1f + SkillSystem.GetSkill(Skill.PotionUp).ModifierAmount) * 100f,
                Skill.AttackSpeedUp         => SkillSystem.GetSkill(Skill.AttackSpeedUp).ModifierAmount * 10f,
                Skill.XpGainUp              => Game.ScreenManager.Player.TotalXPBonus,
                Skill.ManaRegenUp           => Game.ScreenManager.Player.ManaGain,
                _                           => -1f
            };
        }
    }
}
