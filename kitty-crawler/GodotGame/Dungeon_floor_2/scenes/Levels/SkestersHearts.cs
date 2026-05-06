using Godot;
using PlayerBody;
using System;
using System.Threading.Tasks;

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
        _ = AlterLevel();
    }


    private async Task AlterLevel()
    {
        if (_worldStateManager.BossesWon.Contains("skester"))
        {
            _skester.QueueFree();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

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
            _worldStateManager.GameEnded = true;
            _worldStateManager.SaveGame(); // final correct save
            SceneManager.Instance.GameOver();
        }
    }
}
