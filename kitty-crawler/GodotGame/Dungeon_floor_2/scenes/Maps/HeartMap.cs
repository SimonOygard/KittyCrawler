using Godot;
using System;

public partial class HeartMap : Node2D
{
    public override void _Ready()
    {
        var floorLayer = GetNode<TileMapLayer>("Floor");
        GetNode<AudioManager>("/root/AudioManager").SetFloorLayer(floorLayer);
    }
}
