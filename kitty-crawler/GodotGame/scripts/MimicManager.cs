using Godot;
using System;
using ChestInteractions;
using System.Collections.Generic;

public partial class MimicManager : Node2D
{
    public override void _Ready()
    {
        var chests = new List<Chest>();

        foreach (Node node in GetTree().GetNodesInGroup("Chests"))
        {
            GD.Print ($"Found node in group 'Chests': {node.Name} of type {node.GetType()}");
            if (node is Chest chest)
            {
                chests.Add(chest);
            }

        }

        if (chests.Count == 0)
        {
            return;
        }

        var rng = new RandomNumberGenerator();
        rng.Randomize();

        int index = rng.RandiRange(0, chests.Count - 1);
        chests[index].IsMimic = true;

        GD.Print($"Mimic assigned to chest at index {index}");

    }
}
