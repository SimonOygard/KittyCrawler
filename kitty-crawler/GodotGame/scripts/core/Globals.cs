using Godot;
using System;
using KittyCrawler.TELT;

namespace Game.Core
{
    public partial class Globals : Node
    {

        public static Globals Instance { get; private set; }

        [ExportCategory("Gameplay")] [Export] public int GRID_SIZE = 16;

        public override void _Ready()
        {
            Instance = this;

            Logger.Info("Loading Globals ...");
            PlayerData.LoadScore();

            if (PlayerData.OwnedCards.Count == 0)
            {
                PlayerData.AddCardToInventory(CardLibrary.Bat);
                PlayerData.AddCardToInventory(CardLibrary.Bat);
                PlayerData.AddCardToInventory(CardLibrary.Bat);
                PlayerData.AddCardToInventory(CardLibrary.Elemental);
                PlayerData.AddCardToInventory(CardLibrary.Elemental);
                PlayerData.AddCardToInventory(CardLibrary.Goblin);
                PlayerData.AddCardToInventory(CardLibrary.Goblin);
                PlayerData.AddCardToInventory(CardLibrary.Imp);
                PlayerData.AddCardToInventory(CardLibrary.Imp);
                PlayerData.AddCardToInventory(CardLibrary.Minotaur);
                PlayerData.AddCardToInventory(CardLibrary.Minotaur);
                PlayerData.AddCardToInventory(CardLibrary.Skeleton);
                PlayerData.AddCardToInventory(CardLibrary.Skeleton);
                PlayerData.AddCardToInventory(CardLibrary.Skeleton);
                PlayerData.AddCardToInventory(CardLibrary.Snake);
                PlayerData.AddCardToInventory(CardLibrary.Snake);
                PlayerData.AddCardToInventory(CardLibrary.Spider);
                PlayerData.AddCardToInventory(CardLibrary.Spider);
                PlayerData.AddCardToInventory(CardLibrary.Tortoise);
                PlayerData.AddCardToInventory(CardLibrary.Tortoise);
                PlayerData.AddCardToInventory(CardLibrary.Watcher);
                PlayerData.AddCardToInventory(CardLibrary.Watcher);
                PlayerData.AddCardToInventory(CardLibrary.Wraith);
                PlayerData.AddCardToInventory(CardLibrary.Yeti);
                PlayerData.AddCardToInventory(CardLibrary.Yeti);

                PlayerData.SaveDeck(PlayerData.OwnedCards);

                // Ikke i start-decket
                PlayerData.AddCardToInventory(CardLibrary.Demon);
                PlayerData.AddCardToInventory(CardLibrary.Demon);
                PlayerData.AddCardToInventory(CardLibrary.Angel);
                PlayerData.AddCardToInventory(CardLibrary.Angel);
                PlayerData.AddCardToInventory(CardLibrary.Sludge);
            }
        }
    }
};
