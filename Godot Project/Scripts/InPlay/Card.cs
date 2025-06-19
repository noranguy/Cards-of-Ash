using Godot;
using System;

public partial class Card : Node2D {
	[Signal]
	public delegate void CardClickedEventHandler(Card card);
	
	public bool locked = false;
	public bool visible;
	public bool isHand;
	public bool isPlayer;
	public string type;
	private Sprite2D sprite;
	
	public override void _Ready() {
		sprite = GetNode<Sprite2D>("CardImage");
	}
	
	public void Init(string cardType, bool cardVisible, bool cardIsHand, bool cardIsPlayer) {
		if (sprite == null) {
			sprite = GetNode<Sprite2D>("CardImage");
		}
		type = cardType;
		visible = cardVisible;
		isHand = cardIsHand;
		isPlayer = cardIsPlayer;
		
		UpdateTexture();
	}
	
	public void UpdateTexture() {
		if (visible) {
			var texture = GD.Load<Texture2D>($"res://Assets/Cards/{type}.png");
			sprite.Texture = texture;
		} else {
			var texture = GD.Load<Texture2D>($"res://Assets/Cards/back.png");
			sprite.Texture = texture;
		}
	}
	
	public void Flip() {
		visible = !visible;
		UpdateTexture();
	}

	public void OnInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
		if (
			@event is InputEventMouseButton mouseEvent &&
			mouseEvent.Pressed &&
			mouseEvent.ButtonIndex == MouseButton.Left
		) {
			EmitSignal(SignalName.CardClicked, this);
		}
	}
	
	public void Highlight() {
		var sprite = GetNode<Sprite2D>("CardImage");
		Shader shader = GD.Load<Shader>("res://Shaders/card_highlight.gdshader");
		ShaderMaterial mat = new ShaderMaterial { Shader = shader };
		sprite.Material = mat;
	}
	
	public void Unhighlight() {
		if (!locked) {
			var sprite = GetNode<Sprite2D>("CardImage");
			sprite.Material = null;
		}
	}

	public void OnMouseEntered() {
		Highlight();
	}
	
	public void OnMouseExited() {
		Unhighlight();
	}
}
