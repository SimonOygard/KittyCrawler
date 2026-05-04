using Godot;
using System;

public partial class DiamondMap : Node2D
{
    public override void _Ready()
    {
        var floorLayer = GetNode<TileMapLayer>("Floor");
        GetNode<AudioManager>("/root/AudioManager").SetFloorLayer(floorLayer);
    }

}
