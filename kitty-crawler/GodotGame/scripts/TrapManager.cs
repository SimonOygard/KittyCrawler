using Godot;
using System;

public partial class TrapManager : Node
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
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
                //GD.Print($"SpikeTrap found: {trap.Name}");
                //if (true)
                //{
                //    trap.IsArmed = true;
                //    // logikk for å aktivere eller deaktivere feller basert på spillets tilstand kan legges her
                //}
                //else
                //{
                //    trap.IsArmed = false;
                //}
            }
        }

        // for testing randomly arm one trap

        var rng = new RandomNumberGenerator();
        rng.Randomize();


        int index = rng.RandiRange(0, traps.Count - 1);

        if (traps[index] is SpikeTrap chosenTrap)
        {
            chosenTrap.IsArmed = true;
            GD.Print($"Trap at index {index} is now armed");
        }

    }
}
