using Godot;
using KittyCrawler.TELT;
using PlayerBody;
using System;
using System.Threading.Tasks;

public partial class SkestersHearts : Node2D
{
    [Export] private BossData _skesterBossData;

    private CharacterBody2D _skester;
    private Area2D _endTeleport;
    private WorldStateManager _worldStateManager;
    private AnimatedSprite2D _sprite;

    private bool _postBattleHandled = false;

    public override void _Ready()
    {
        _worldStateManager = GetNode<WorldStateManager>("/root/WorldStateManager");
        
        _skester = GetNode<CharacterBody2D>("Skester");
        _endTeleport = GetNode<Area2D>("End");
        _sprite = GetNode<AnimatedSprite2D>("Skester/AnimatedSprite2D");

        _endTeleport.BodyEntered += OnEndTeleportEntered;
        _endTeleport.Hide();

        _ = AlterLevel();
    }


    private async Task AlterLevel()
    {
        if (!_worldStateManager.BossesWon.Contains("skester"))
            return;

        if (_postBattleHandled)
            return;

        _postBattleHandled = true;

        if (_skester != null && IsInstanceValid(_skester))
        {

            _sprite.Play("skester_dies");
                await ToSignal(_sprite, AnimatedSprite2D.SignalName.AnimationFinished);

                _skester.Hide();
        }

        WorldDialogueManager.Instance.ShowBossPostBattleDialogue(_skesterBossData);

        await ToSignal(WorldDialogueManager.Instance, WorldDialogueManager.SignalName.DialogueEnded);

        if (_skester != null && IsInstanceValid(_skester))
            _skester.QueueFree();

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        _endTeleport.Show();

        GD.Print("Skester defeated, end teleport activated.");
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
