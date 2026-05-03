using Godot;
using PlayerBody;
using System;

public partial class SkestersHearts : Node2D
{
    private CharacterBody2D _skester;
    private Area2D _endTeleport;
    private WorldStateManager _worldStateManager;
    public override void _Ready()
    {
        //var dialogue = GD.Load<Resource>("res://Dungeon_floor_2/scenes/Levels/SkestersHearts.dialogue");
        //DialogueManagerRuntime.DialogueManager.ShowExampleDialogueBalloon(dialogue, "start", [ this ]);

        _worldStateManager = GetNode<WorldStateManager>("/root/WorldStateManager");
        
        _skester = GetNode<CharacterBody2D>("Skester");
        _endTeleport = GetNode<Area2D>("End");

        _endTeleport.BodyEntered += OnEndTeleportEntered;
        _endTeleport.Hide();
        AlterLevel();
    }


    private void AlterLevel()
    {
        if (_worldStateManager.BossesWon.Contains("skester") && true)
        {
            if (IsInstanceValid(_skester))
                _skester.QueueFree();

            _endTeleport.Show();

            GD.Print("Skester defeated, end teleport activated.");
        }

    }

    private void OnEndTeleportEntered(Node body)
    {
        GD.Print("Body entered end teleport: " + body.Name);
        if (body is Player player)
        {
            GD.Print("Player entered end teleport, game over!");
            SceneManager.Instance.GameOver();
        }
    }
}
