using Godot;
using Godot.Collections;

namespace KittyCrawler.TELT;

[GlobalClass]
public partial class BossData : Resource
{
    [Export] public string BossName { get; set; } = "";
    [Export] public string NpcId { get; set; } = "";
    [Export] public Array<CardData> Deck { get; set; } = new();
    [Export] public CardData RewardCard { get; set; } = null;
}
