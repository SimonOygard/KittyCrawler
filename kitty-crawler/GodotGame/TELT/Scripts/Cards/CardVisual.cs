using Godot;

namespace KittyCrawler.TELT;

public partial class CardVisual : Control
{
    // ── Noder (settes opp i scenen) ───────────────────────────────────
    [Export] private TextureRect _cardTexture;
    [Export] private Label _cardNameLabel;
    [Export] private Panel _statsContainer;
    [Export] private Label _statsLabel;
    [Export] private Label _abilityLabel;


    // ── Data ──────────────────────────────────────────────────────────
    private CardData _cardData;
    public CardData CardData => _cardData;

    // ── Signals ───────────────────────────────────────────────────────
    [Signal] public delegate void CardClickedEventHandler(CardVisual card);
    [Signal] public delegate void CardHoveredEventHandler(CardVisual card);

    // ── Tilstand ──────────────────────────────────────────────────────
    private bool _isSelected = false;
    private bool _isPlayable = true;
    private Vector2 _originalPosition;

    private const float HoverLift = -20f;
    private const float SelectLift = -35f;

    // ── Init ──────────────────────────────────────────────────────────
    public void Setup(CardData data)
    {
        _cardData = data;

        if (_cardTexture != null)
            _cardTexture.Texture = data.Texture;

        if (_cardNameLabel != null)
            _cardNameLabel.Text = data.CardName;

        if (_statsLabel != null)
            _statsLabel.Text = data.GetCurrentDamage().ToString(); // ← må være GetCurrentDamage

        if (_abilityLabel != null)
            _abilityLabel.Text = data.AbilityDescription;
    }

    public override void _Ready()
    {
        _originalPosition = Position;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;

    }

    // ── Klikk ─────────────────────────────────────────────────────────
    public void SetAsSlotCard()
    {
        MouseFilter = MouseFilterEnum.Pass;
        // Deaktiver GuiInput så Panel får klikket
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (MouseFilter == MouseFilterEnum.Pass) return; // ← ikke fang input

        if (@event is InputEventMouseButton mouseEvent
            && mouseEvent.ButtonIndex == MouseButton.Left
            && mouseEvent.Pressed
            && _isPlayable)
        {
            SetSelected(!_isSelected);
            EmitSignal(SignalName.CardClicked, this);
        }
    }

    // ── Hover ─────────────────────────────────────────────────────────
    private void OnMouseEntered()
    {
        if (_isSelected) return;
        AnimateTo(Position with { Y = _originalPosition.Y + HoverLift });
        EmitSignal(SignalName.CardHovered, this);
    }

    private void OnMouseExited()
    {
        if (_isSelected) return;
        AnimateTo(Position with { Y = _originalPosition.Y });
    }

    // ── Seleksjon ─────────────────────────────────────────────────────
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        float targetY = selected
            ? _originalPosition.Y + SelectLift
            : _originalPosition.Y;
        AnimateTo(Position with { Y = targetY });
    }

    public void SetPlayable(bool playable)
    {
        _isPlayable = playable;
        Modulate = playable ? Colors.White : new Color(0.5f, 0.5f, 0.5f);
    }

    public void UpdateOriginalPosition()
    {
        _originalPosition = Position;
    }

    // ── Tween ─────────────────────────────────────────────────────────
    private void AnimateTo(Vector2 targetPosition)
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "position", targetPosition, 0.1f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
    }

    //Deaktiver card visul i grid view discard pile
    public void SetStatic()
    {
        MouseFilter = MouseFilterEnum.Ignore;
    }


    //Highlight under targeting
    public void SetHighlighted(bool highlighted)
    {
        var existing = GetNodeOrNull<Panel>("HighlightBorder");
        if (existing != null) existing.QueueFree();

        if (!highlighted)
        {
            Modulate = Colors.White;
            return;
        }

        Modulate = new Color(1.12f, 1.12f, 1.12f, 1f); // ← lysere

        var border = new Panel();
        border.Name = "HighlightBorder";
        border.Position = Vector2.Zero;
        border.Size = new Vector2(128, 192);
        border.MouseFilter = MouseFilterEnum.Ignore;
        border.ZIndex = 10;

        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0, 0, 0, 0);
        styleBox.BorderColor = new Color(0.9f, 0.5f, 0.1f, 0.6f);
        styleBox.SetBorderWidthAll(4);
        border.AddThemeStyleboxOverride("panel", styleBox);

        AddChild(border);
    }


}
