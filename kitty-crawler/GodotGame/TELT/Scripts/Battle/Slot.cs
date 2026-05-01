using Godot;

namespace KittyCrawler.TELT;

public partial class Slot : Node
{
    public enum SlotPosition { Left, MidLeft, MidRight, Right }    public enum SlotOwner { Player, Enemy }

    public SlotPosition Position { get; set; }
    public new SlotOwner Owner { get; set; }
    public CardData Card { get; private set; } = null;


    public bool IsOccupied => Card != null;
    public bool IsEmpty => Card == null;

    public bool TryPlaceCard(CardData card)
    {
        if (IsOccupied) return false;
        Card = card;
        return true;
    }

    public CardData RemoveCard()
    {
        var card = Card;
        Card = null;
        return card;
    }

    public int GetDamage()
    {
        return Card?.GetCurrentDamage() ?? 0;
    }

    public override string ToString()
    {
        return $"[{Owner} {Position}]: {(IsOccupied ? $"{Card.CardName} ({Card.Damage})" : "tom")}";
    }
}
