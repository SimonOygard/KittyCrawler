using Godot;
using System;
using KittyCrawler.TELT;

namespace Game.Core
{
    public partial class Globals : Node
    {

        public static Globals Instance { get; private set; }

        [ExportCategory("Gameplay")]
        [Export] public int GRID_SIZE = 16;

        public override void _Ready()
        {
            Instance = this;

            Logger.Info("Loading Globals ...");
            PlayerData.LoadScore();
        }
    }
}
