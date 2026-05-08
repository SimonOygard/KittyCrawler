using Godot;
using System;
using System.Collections.Generic;
using KittyCrawler.TELT;

namespace Game.Core
{
    public partial class Globals : Node
    {
        public static Globals Instance { get; private set; }

        [ExportCategory("Gameplay")] [Export] public int GRID_SIZE = 16;

        // 25 kort — startdeck
        private static readonly List<string> StartDeck = new()
        {
            CardLibrary.Bat, CardLibrary.Bat, CardLibrary.Bat,
            CardLibrary.Elemental, CardLibrary.Elemental,
            CardLibrary.Goblin, CardLibrary.Goblin,
            CardLibrary.Imp, CardLibrary.Imp,
            CardLibrary.Minotaur, CardLibrary.Minotaur,
            CardLibrary.Skeleton, CardLibrary.Skeleton, CardLibrary.Skeleton,
            CardLibrary.Snake, CardLibrary.Snake,
            CardLibrary.Spider, CardLibrary.Spider,
            CardLibrary.Tortoise, CardLibrary.Tortoise,
            CardLibrary.Watcher, CardLibrary.Watcher,
            CardLibrary.Wraith,
            CardLibrary.Yeti, CardLibrary.Yeti,
        };

        // 5 kort — starter i inventory, ikke i deck
        private static readonly List<string> StartInventory = new()
        {
            CardLibrary.Angel, CardLibrary.Bat,
            CardLibrary.Cat,
            CardLibrary.Druid,
            CardLibrary.Drake,
            CardLibrary.Demon,
        };

        public override void _Ready()
        {
            Instance = this;
            Logger.Info("Loading Globals ...");
            PlayerData.LoadScore();

            if (PlayerData.OwnedCards.Count == 0)
                InitializeStartingCards();
        }

        public static void InitializeStartingCards()
        {
            PlayerData.ResetForNewGame();

            foreach (var card in StartDeck)
                PlayerData.AddCardToInventory(card);

            foreach (var card in StartInventory)
                PlayerData.AddCardToInventory(card);

            PlayerData.SaveDeck(StartDeck);
        }
    }
}
