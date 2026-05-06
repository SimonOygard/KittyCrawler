using Godot;
using Godot.Collections;

namespace KittyCrawler.TELT;

[GlobalClass]
public partial class BossData : Resource
{
    // Navn som vises f.eks "Mio"
    [Export] public string BossName { get; set; } = "";
    //intern boss navn f.eks "mio" 
    [Export] public string NpcId { get; set; } = "";
    [Export] public Array<CardData> Deck { get; set; } = new();
    [Export] public CardData RewardCard { get; set; } = null;
    [Export] public Resource DialogueResource { get; set; }
}
