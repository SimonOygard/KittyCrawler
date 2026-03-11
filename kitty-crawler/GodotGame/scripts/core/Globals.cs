using Godot;

namespace Game.Core
{
    public partial class Globals : Node
    {

        public static Globals Instance { get; private set; }

        [ExportCategory("Gameplay")] [Export] public int GRID_SIZE = 16;

        public override void _Ready()
        {
            GD.Print("_Ready kjører!");
            Instance = this;

            Logger.Debug("Loading Globals ...");
            Logger.Info("Loading Globals ...");
            Logger.Warning("Loading Globals ...");
            Logger.Error("Loading Globals ...");
        }
    }
}
