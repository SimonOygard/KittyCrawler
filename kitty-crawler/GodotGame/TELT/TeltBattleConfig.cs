using Godot;
using KittyCrawler.TELT;

public partial class TeltBattleConfig : Node
{
    public static TeltBattleConfig Instance { get; private set; }

    // Hvilken boss skal du slåss mot
    public BossData CurrentBoss { get; set; } = null;
    public NPCData CurrentNPC { get; set; } = null;
    public bool JustReturnedFromBattle { get; set; } = false;

    // Hvilken scene skal du tilbake til etter kampen
    public string ReturnScenePath { get; set; } = "";

    public override void _Ready()
    {
        Instance = this;
    }
}
