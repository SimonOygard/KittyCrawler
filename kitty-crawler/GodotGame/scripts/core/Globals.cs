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
                PlayerData.AddCardToInventory(CardLibrary.Angel);
                PlayerData.AddCardToInventory(CardLibrary.Bat);
                PlayerData.AddCardToInventory(CardLibrary.Cat);
                PlayerData.AddCardToInventory(CardLibrary.Demon);
                PlayerData.AddCardToInventory(CardLibrary.Drake);
                PlayerData.AddCardToInventory(CardLibrary.Druid);
                PlayerData.AddCardToInventory(CardLibrary.Dryad);
                PlayerData.AddCardToInventory(CardLibrary.Elemental);
                PlayerData.AddCardToInventory(CardLibrary.EyeOfDespair);
                PlayerData.AddCardToInventory(CardLibrary.EyeOfHope);
                PlayerData.AddCardToInventory(CardLibrary.Goblin);
                PlayerData.AddCardToInventory(CardLibrary.Goblin);
                PlayerData.AddCardToInventory(CardLibrary.Golem);
                PlayerData.AddCardToInventory(CardLibrary.Horror);
                PlayerData.AddCardToInventory(CardLibrary.Imp);
                PlayerData.AddCardToInventory(CardLibrary.Minotaur);
                PlayerData.AddCardToInventory(CardLibrary.PuzzleMaster);
                PlayerData.AddCardToInventory(CardLibrary.Skeleton);
                PlayerData.AddCardToInventory(CardLibrary.Sludge);
                PlayerData.AddCardToInventory(CardLibrary.Snake);
                PlayerData.AddCardToInventory(CardLibrary.Spider);
                PlayerData.AddCardToInventory(CardLibrary.Tortoise);
                PlayerData.AddCardToInventory(CardLibrary.Watcher);
                PlayerData.AddCardToInventory(CardLibrary.Wraith);
                PlayerData.AddCardToInventory(CardLibrary.Yeti);

                PlayerData.SaveDeck(PlayerData.OwnedCards);
            }
        }
    }
};
