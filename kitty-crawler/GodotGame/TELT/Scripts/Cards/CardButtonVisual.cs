using Godot;
using KittyCrawler.TELT;

public partial class CardButtonVisual : Button
{
    [Export] private TextureRect _background;
    [Export] private Label _nameLabel;

    [Export] private Texture2D _commonTexture;
    [Export] private Texture2D _uncommonTexture;
    [Export] private Texture2D _rareTexture;

    public CardData CardData { get; private set; }

    public void Setup(CardData cardData)
    {
        CardData = cardData;
        _nameLabel.Text = cardData.CardName;

        _background.Texture = cardData.CardRarity switch
        {
            CardData.Rarity.Common => _commonTexture,
            CardData.Rarity.Uncommon => _uncommonTexture,
            CardData.Rarity.Rare => _rareTexture,
            _ => _commonTexture
        };
    }
}
