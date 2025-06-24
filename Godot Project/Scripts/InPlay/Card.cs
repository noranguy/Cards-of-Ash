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
	public static readonly Vector2 SIZE2 = new Vector2(40, 60);
	
	public bool locked = false;
	public bool visible;
	public bool isPlayer;
	public double durability = 1;
	public string type;
	public string clas;
	public int index;
	private Polygon2D sprite;
	
	private Sprite2D indicator;
	private float indicatorMin = -30;
	private float indicatorMax = -10;
	private float indicatorDir = 1;
	private float indicatorX;
	
	public override void _Ready() {
		sprite = GetNode<Polygon2D>("CardImage");
		indicator.Visible = false;
		indicatorX = indicator.Position.X;
		indicator.Position = new Vector2(indicatorX, indicatorMin);
	}
	
	public override void _Process(double delta) {
		indicator.Position += new Vector2(0, 1 / (2 * (float)delta) * indicatorDir * (float)delta);
		if (indicator.Position.Y > indicatorMax) {
			indicatorDir = -1;
		} else if (indicator.Position.Y < indicatorMin) {
			indicatorDir = 1;
		}
	}
	
	public void Focus() {
		indicator.Visible = true;
	}
	
	public void Unfocus() {
		indicator.Visible = false;
	}
	
	public void Init(Vector2[] vertices, string type, string clas, bool visible, bool isPlayer, int index) {
		sprite = GetNode<Polygon2D>("CardImage");
		indicator = GetNode<Sprite2D>("Indicator");
		
		var collisionPolygon = GetNode<CollisionPolygon2D>("Area2D/CollisionPolygon2D");
		this.type = type;
		this.clas = clas;
		this.visible = visible;
		this.isPlayer = isPlayer;
		this.index = index;
		
		sprite.UV = DEFAULT_VERTICES;
		if (index == -1) {
			collisionPolygon.Polygon = sprite.Polygon = DEFAULT_VERTICES;
		} else {
			collisionPolygon.Polygon = sprite.Polygon = vertices;
			indicatorMin += vertices[0].Y;
			indicatorMax += vertices[0].Y;
			indicator.Position = new Vector2((vertices[0].X + vertices[1].X) / 2, indicatorMin);
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
