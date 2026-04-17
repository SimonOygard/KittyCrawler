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

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare
    }

    public enum AbilityType
    {
        None,
        NoExceedTortoise,    // Tortoise
        GiveMinusStat,       // Skeleton
        RemoveUnit,          // Druid
        GivePlusStat,        // Drake
        DrawCard,            // Golem
        DiscardDraw,         // Watcher
        AnySlot,             // Skester
        AllEnemyMinusStat,   // Eve
        DiscardGainStats,    // Croxy
        SwitchSlots,         // Hilda
        RemoveGainStats,     // Mio
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
