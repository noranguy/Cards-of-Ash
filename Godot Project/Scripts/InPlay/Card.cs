using Godot;
using System;
using System.Threading.Tasks;

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
	public Polygon2D sprite;
	public ProgressBar durabilityBar;
	
	private Tween tween;
	public Vector2 upperPosition;
	public Vector2 lowerPosition;
	
	public override void _Ready() {
		sprite = GetNode<Polygon2D>("CardImage");
		indicator.Visible = false;
		indicatorX = indicator.Position.X;
		indicator.Position = new Vector2(indicatorX, indicatorMin);
	}
	
	private Sprite2D indicator;
	private float indicatorMin = -30;
	private float indicatorMax = -10;
	private float indicatorDir = 1;
	private float indicatorX;
	
	public override void _Process(double delta) {
		indicator.Position += new Vector2(0, 1 / (2 * (float)delta) * indicatorDir * (float)delta);
		if (indicator.Position.Y > indicatorMax) {
			indicatorDir = -1;
		} else if (indicator.Position.Y < indicatorMin) {
			indicatorDir = 1;
		}
	}
	
	public void ReduceDurability() {
		durability -= 0.2;
		durabilityBar.Value = durability * 100;
	}
	
	public void Focus() {
		indicator.Visible = true;
	}
	
	public void Unfocus() {
		indicator.Visible = false;
	}
	
	public void Init(Vector2[] vertices, string type, string clas, bool visible, bool isPlayer, int index) {
		indicator = GetNode<Sprite2D>("Indicator");
		sprite = GetNode<Polygon2D>("CardImage");
		durabilityBar = GetNode<ProgressBar>("DurabilityBar");
		
		var collisionPolygon = GetNode<CollisionPolygon2D>("Area2D/CollisionPolygon2D");
		this.type = type;
		this.clas = clas;
		this.visible = visible;
		this.isPlayer = isPlayer;
		this.index = index;
		
		sprite.UV = DEFAULT_VERTICES;
		if (index == -1) {
			collisionPolygon.Polygon = sprite.Polygon = DEFAULT_VERTICES;
			Position = isPlayer ? new Vector2(-180, 125) : new Vector2(-180, 50);
		} else {
			collisionPolygon.Polygon = sprite.Polygon = vertices;
			indicatorMin += vertices[0].Y;
			indicatorMax += vertices[0].Y;
			indicator.Position = new Vector2((vertices[0].X + vertices[1].X) / 2, indicatorMin);
		}
		upperPosition = Position + new Vector2(0, -10);
		lowerPosition = Position;
		
		UpdateTexture();
	}
	
	public async Task UpdatePosition(Vector2 position) {
		tween = GetTree().CreateTween();
		tween.TweenProperty(this, "position", position, 0.1f);
	}
	
	public async Task UpdatePosition(Vector2 position, Vector2 scale) {
		tween = GetTree().CreateTween();
		tween.TweenProperty(this, "position", position, 0.1f);
		tween.TweenProperty(this, "scale", scale, 0.1f);
		await ToSignal(tween, "finished");
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
		ReduceDurability();
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
		Shader shader = GD.Load<Shader>("res://Shaders/card_highlight.gdshader");
		ShaderMaterial mat = new ShaderMaterial { Shader = shader };
		sprite.Material = mat;
		
		if (index != -1) return;

		if (tween != null && tween.IsRunning()) {
			return;
		}
		tween = GetTree().CreateTween();
		tween.TweenProperty(this, "position", upperPosition, 0.05f);
	}
	
	public void Unhighlight() {
		if ((index == -1 && !isPlayer) || locked) return;
		sprite.Material = null;
		
		if (index != -1) return;
		
		tween = GetTree().CreateTween();
		tween.TweenProperty(this, "position", lowerPosition, 0.05f);
	}

	public void OnMouseEntered() {
		Highlight();
	}
	
	public void OnMouseExited() {
		Unhighlight();
	}
}
