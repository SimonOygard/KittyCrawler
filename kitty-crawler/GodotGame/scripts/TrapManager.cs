using Godot;
using System;

public partial class TrapManager : Node
{
    private WorldStateManager _worldStateManager;

    public override void _Ready()
    {
        _worldStateManager = WorldStateManager.Instance;

        if (_worldStateManager == null)
        {
            GD.PushError("WorldStateManager.Instance is null.");
            return;
        }

        var traps = GetTree().GetNodesInGroup("traps");
        GD.Print($"Found {traps.Count} nodes in group 'traps'");

        if (traps.Count == 0)
        {
            return;
        }

        foreach (Node node in traps)
        {
            GD.Print($"Found node in group 'traps': {node.Name} of type {node.GetType()}");
            if (node is SpikeTrap trap)
            {
                trap.IsArmed = true;
                CheckTrapStatus(trap);
            }
        }

    }
    private void CheckTrapStatus(SpikeTrap trap)
    {
        var parent = trap.GetParent();
        
        if (parent.Name == "HeartsTraps" && _worldStateManager.BossesWon.Contains("eve"))
        {
            GD.Print("Eve defeated disarming traps");
            trap.IsArmed = false;
        }
        else if (parent.Name == "DiamondTraps" && _worldStateManager.BossesWon.Contains("mio"))
        {
            GD.Print("Mio defeated disarming traps");
            trap.IsArmed = false;
        }
        else if (parent.Name == "SpadeTraps" && _worldStateManager.BossesWon.Contains("croxy")) //sjekk case sensitive og spelling
        {
            GD.Print("Croxy defeated disarming traps");
            trap.IsArmed = false;
        }

    }
}
