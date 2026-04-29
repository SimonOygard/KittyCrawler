using ChestInteractions;
using Godot;
using Interaction;
using PlayerBody;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
using static Godot.RenderingDevice;
using DialogueManagerRuntime;
using System.Linq;
using System.Threading.Tasks;
public partial class MimicPuzzle : CharacterBody2D
{
    public Area2D _interactionArea;
    private AnimatedSprite2D _sprite;
    private MimicManager _manager;
    private StaticBody2D _staticBody;
    private CollisionShape2D _blockerShape;
    private CollisionShape2D _interactionShape;
    private CollisionShape2D _bossCollisionShape;

    // EmitSignal -> Send damage til Playerdata (DO NOT ADD YET FØR PR GODKJENT)
    // -> Send Card recieved til Playerdata(DO NOT ADD YET FØR PR GODKJENT)
    //-> Send en score[10 total points possible - 3 points for every wrong attempt f.eks] (DO NOT ADD YET FØR PR GODKJENT)
    public Chest _chosenChest = null;
    public Chest _openedChest = null;
    public Chest _finalChest = null;
    private Player player;

    public bool _firstInteraction = true;
    public bool _isPuzzleCompleted = false;

    [Export]
    private Marker2D _destination;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _interactionArea = GetNode<Area2D>("InteractionArea");
        _interactionShape = _interactionArea.GetNode<CollisionShape2D>("CollisionShape2D");
        _bossCollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        _interactionArea.BodyEntered += OnBodyEntered;
        _sprite.Play("Default");

        _manager = GetNode<MimicManager>("Chests");
        GD.Print(_manager.Name);

        _staticBody = GetNode<StaticBody2D>("StaticBody2D");
        _blockerShape = _staticBody.GetNode<CollisionShape2D>("BlockerShape");
    }
    public void OnBodyEntered(Node body)
    {
        if (body is Player player && !_isPuzzleCompleted)
        {
            GD.Print("Player entered Mimic Puzzle area!");
            var dialogue = GD.Load<Resource>("res://Dungeon_floor_2/scenes/Levels/MimicPuzzle.dialogue"); //fix this
            DialogueManager.ShowExampleDialogueBalloon(dialogue, "start", [ this ]);

            this.player = player;
        }

        _firstInteraction = false;
    }
    
    public void SelectChest(string chestId)
    {
        _chosenChest = GetNode<Chest>($"Chests/{chestId}");
        GD.Print($"Chosen chest set to: {_chosenChest.Name}");
    }

    public void OpenUnselectedChestMimic()
    {
        var chests = new List<Chest>();

        foreach (Node node in GetTree().GetNodesInGroup("Chests"))
        {
            GD.Print($"Found node in group 'Chests': {node.Name} of type {node.GetType()}");
            if (node is Chest chest && chest.Name != _chosenChest.Name)
            {
                chests.Add(chest);
            }
        }

        var mimicOpened = chests.FirstOrDefault(x => x.IsMimic);

        if (mimicOpened != null)
        {
            mimicOpened.Interact();
            _openedChest = mimicOpened;
            GD.Print($"Opened chest set to: {_openedChest.Name}");
            _finalChest = chests.FirstOrDefault(x => x != _chosenChest && x != _openedChest);
            GD.Print($"Final chest set to: {_finalChest.Name}");
        }
    }

    public void SelectNewChest(bool changedChest)
    {
        if (changedChest)
        {
           // change Chest
           var oldChest = _chosenChest;
            _chosenChest = _finalChest;
            _finalChest = oldChest;
        }
    }


    private async void RestartLevel(Player player)
    {
        // takedamage
        _chosenChest.Interact();
        _finalChest.Interact();

        await ToSignal(GetTree().CreateTimer(3.0f), "timeout");

        var start = _destination;
        player.TeleportTo(_destination.GlobalPosition + new Vector2(0, 16));
    }

    private void FinishLevel(Player player)
    {
        _chosenChest.Interact();
        _finalChest.Interact();

        _sprite.Play("Death");
        _isPuzzleCompleted = true;

        _interactionArea.Monitoring = false;
        _interactionShape.SetDeferred("disabled", true);

        _blockerShape.SetDeferred("disabled", true);
        _bossCollisionShape.SetDeferred("disabled", true);

        CollisionLayer = 0;
        CollisionMask = 0;

        _staticBody.CollisionLayer = 0;
        _staticBody.CollisionMask = 0;

        // give reward

    }

    public void EndGame()
    {
        if (_chosenChest.IsMimic)
        {
            RestartLevel(player);
            _manager.ResetMimics();
        }
        else if (!_chosenChest.IsMimic)
        {
            FinishLevel(player);
        }
    }
}
