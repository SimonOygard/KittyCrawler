using Godot;
using System;

public partial class ClubMap : Node2D
{
    public override void _Ready()
    {
        var floorLayer = GetNode<TileMapLayer>("Tiles/Floor");
        var audioManager = GetNode<AudioManager>("/root/AudioManager");

        audioManager.SetFloorLayer(floorLayer);
    }
}
