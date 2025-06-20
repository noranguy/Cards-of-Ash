using Godot;
using System;

public partial class Card : Node2D {
	[Signal]
	public delegate void CardClickedEventHandler(Card card);
	
	public bool locked = false;
	public bool visible;
	public bool isPlayer;
	public double durability = 1;
	public string type;
	public string clas;
	public int index;
	private Sprite2D sprite;
	
	public override void _Ready() {
		sprite = GetNode<Sprite2D>("CardImage");
	}
	
	public void Init(string type, string clas, bool visible, bool isPlayer, int index) {
		if (sprite == null) {
			sprite = GetNode<Sprite2D>("CardImage");
		}
		this.type = type;
		this.clas = clas;
		this.visible = visible;
		this.isPlayer = isPlayer;
		this.index = index;
		
		UpdateTexture();
	}
	
	public void UpdateTexture() {
		if (visible) {
			var texture = GD.Load<Texture2D>($"res://Assets/Cards/{type}_{clas}.png");
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
		if (index == -1 && !isPlayer) return;
		var sprite = GetNode<Sprite2D>("CardImage");
		Shader shader = GD.Load<Shader>("res://Shaders/card_highlight.gdshader");
		ShaderMaterial mat = new ShaderMaterial { Shader = shader };
		sprite.Material = mat;
	}
	
	public void Unhighlight() {
		if ((index != -1 || isPlayer) && !locked) {
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
