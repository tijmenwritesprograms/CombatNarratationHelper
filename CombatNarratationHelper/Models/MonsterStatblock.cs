namespace CombatNarratationHelper.Models;

public class MonsterStatblock
{
    public string Name { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Alignment { get; set; } = string.Empty;
    public int ArmorClass { get; set; }
    public string ArmorClassDescription { get; set; } = string.Empty;
    public int HitPoints { get; set; }
    public string HitDice { get; set; } = string.Empty;
    public int Speed { get; set; }
    public string SpeedDescription { get; set; } = string.Empty;
    public int Initiative { get; set; } // New: Initiative bonus
    public int ProficiencyBonus { get; set; } // Explicit PB from statblock
    public string ProficiencyBonusDescription { get; set; } = string.Empty; // e.g. "PB +5"
    
    // Ability Scores
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Intelligence { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }
    
    // Ability Modifiers and Saves
    public int StrengthModifier => (Strength - 10) / 2;
    public int DexterityModifier => (Dexterity - 10) / 2;
    public int ConstitutionModifier => (Constitution - 10) / 2;
    public int IntelligenceModifier => (Intelligence - 10) / 2;
    public int WisdomModifier => (Wisdom - 10) / 2;
    public int CharismaModifier => (Charisma - 10) / 2;
    public Dictionary<string, int> AbilitySaves { get; set; } = new(); // e.g. { "STR": 6 }
    public Dictionary<string, int> AbilityMods { get; set; } = new(); // e.g. { "STR": 6 }
    
    // Additional properties
    public List<string> SavingThrows { get; set; } = new();
    public List<string> Skills { get; set; } = new();
    public List<string> DamageVulnerabilities { get; set; } = new();
    public List<string> DamageResistances { get; set; } = new();
    public List<string> DamageImmunities { get; set; } = new();
    public List<string> ConditionImmunities { get; set; } = new();
    public List<string> Immunities { get; set; } = new(); // New: for generic "Immunities"
    public List<string> Senses { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public string ChallengeRating { get; set; } = string.Empty;
    
    // Features and Actions
    public List<MonsterFeature> Features { get; set; } = new();
    public List<MonsterAction> Actions { get; set; } = new();
    public List<MonsterAction> BonusActions { get; set; } = new();
    public List<MonsterAction> Reactions { get; set; } = new();
    public List<MonsterAction> LegendaryActions { get; set; } = new();
    // Sectional text for Traits, Actions, Legendary Actions
    public List<MonsterFeature> Traits { get; set; } = new();
    public int LegendaryActionUses { get; set; } // e.g. 3 (4 in Lair)
    public string LegendaryActionUsesDescription { get; set; } = string.Empty;
}

public class MonsterFeature
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class MonsterAction
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AttackBonus { get; set; } = string.Empty;
    public string Damage { get; set; } = string.Empty;
    public string Range { get; set; } = string.Empty;
}