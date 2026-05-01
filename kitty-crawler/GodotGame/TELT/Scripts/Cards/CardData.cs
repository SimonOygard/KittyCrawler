using Godot;

namespace KittyCrawler.TELT;

[GlobalClass]
public partial class CardData : Resource
{
    [Export] public string CardName { get; set; } = "";
    [Export] public int Damage { get; set; } = 0;
    [Export] public Rarity CardRarity { get; set; } = Rarity.Common;
    [Export] public Texture2D Texture { get; set; }
    [Export] public string AbilityDescription { get; set; } = "";
    [Export] public AbilityType Ability { get; set; } = AbilityType.None;

    // buff/debuff
    public bool IsPoisoned { get; set; } = false;
    public bool IsEnraged { get; set; } = false;
    public bool HasStatus => IsPoisoned || IsEnraged;

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare
    }

    public enum AbilityType
    {
        None,
        NoExceedTortoise,       // Tortoise
        GiveMinusOneStat,       // Skeleton
        GiveMinusTwoStats,      // common
        GivePlusOneStat,        //
        GivePlusTwoStats,       // common (+2)
        GivePlusMinusThree,     // Druid (velg +3 eller -3)
        RemoveUnit,             // Horror, Puzzle Master
        DrawCard,               // Elemental
        DrawTwoCards,           // Dryad
        DiscardDraw,            // Watcher
        DiscardGainStats,       // Sludge
        AnySlot,                // Skester
        AllEnemyMinusStat,      // Eve
        AllAllyPlusStat,        // Croxy
        OpponentDiscards,       //
        CopyStat,               // Cat
        ResetStat,              // Wraith
        ApplyPoison,            // Snake
        ApplyRage,              // Minotaur
        SwitchSlots,            // Hilda
        RemoveGainStats,        // Mio
        DealThreeDamage,
        HealThree,
    }

    public int CurrentDamage { get; set; } = -1;

    public int GetCurrentDamage()
    {
        if (CurrentDamage == -1) CurrentDamage = Damage;
        return CurrentDamage;
    }

    public void ResetCurrentDamage()
    {
        CurrentDamage = -1;
    }

}
