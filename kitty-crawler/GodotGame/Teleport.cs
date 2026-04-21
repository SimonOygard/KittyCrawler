using Godot;
using Interaction;
using PlayerBody;

public partial class Teleport : Node2D, IInteractable
{
    [Export] private Node2D _destination;

    public void Interact()
    {
        var player = GetTree().CurrentScene.GetNodeOrNull<Player>("Player");

        if (player == null || _destination == null)
            return;

        player.SnapToPosition(_destination.GlobalPosition);
    }
}
