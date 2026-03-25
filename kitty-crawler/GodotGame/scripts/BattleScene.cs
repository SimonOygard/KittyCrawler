using Godot;
using System;

public partial class BattleScene : Node2D
{
    public string EnemyName;

    public override void _Ready()
    {
        GD.Print($"Entering battle with {EnemyName}!");
    }
}
