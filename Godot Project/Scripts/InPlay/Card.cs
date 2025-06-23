using Godot;
using System;

public partial class Card : Control {
	[Signal]
	public delegate void CardClickedEventHandler(Card card);
	
	public static readonly Vector2 SIZE = new Vector2(32, 48);
	public static readonly Vector2[] DEFAULT_VERTICES = new Vector2[] {
		new Vector2(0, 0),
		new Vector2(SIZE.X, 0),
		new Vector2(SIZE.X, SIZE.Y),
		new Vector2(0, SIZE.Y)
	};
	
	public bool locked = false;
	public bool visible;
	public bool isPlayer;
	public double durability = 1;
	public string type;
	public string clas;
	public int index;
	private Polygon2D sprite;
	
	public override void _Ready() {
		sprite = GetNode<Polygon2D>("CardImage");
		//Init(DEFAULT_VERTICES, "volcano", "basic", true, true, 0);
	}
	
	public void Init(Vector2[] vertices, string type, string clas, bool visible, bool isPlayer, int index) {
		sprite = GetNode<Polygon2D>("CardImage");
		var collisionPolygon = GetNode<CollisionPolygon2D>("Area2D/CollisionPolygon2D");
		this.type = type;
		this.clas = clas;
		this.visible = visible;
		this.isPlayer = isPlayer;
		this.index = index;

		sprite.UV = DEFAULT_VERTICES;
		
		if (index == -1) {
			collisionPolygon.Polygon = sprite.Polygon = sprite.UV;
		} else {
			collisionPolygon.Polygon = sprite.Polygon = vertices;
		}
		
		UpdateTexture();
	}
	
	public void UpdateTexture() {
		if (visible) {
			var texture = GD.Load<Texture2D>($"res://Assets/Cards/{type}_{clas}.png");
			sprite.Texture = texture;
		} else {
			var texture = GD.Load<Texture2D>($"res://Assets/Cards/{(isPlayer ? "player" : "enemy")}_back.png");
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
		var sprite = GetNode<Polygon2D>("CardImage");
		Shader shader = GD.Load<Shader>("res://Shaders/card_highlight.gdshader");
		ShaderMaterial mat = new ShaderMaterial { Shader = shader };
		sprite.Material = mat;
	}
	
	public void Unhighlight() {
		if ((index != -1 || isPlayer) && !locked) {
			var sprite = GetNode<Polygon2D>("CardImage");
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
