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
	public bool ready = false;
	public double repeatMult = 1;
	public int lastRound = -2;
	
	public Tween tween;
	public Vector2 upperPosition;
	public Vector2 lowerPosition;
	
	public bool dragging = false;
	
	public override void _Ready() {
		Connect("mouse_entered", new Callable(this, nameof(OnMouseEntered)));
		Connect("mouse_exited", new Callable(this, nameof(OnMouseExited)));
		
		sprite = GetNode<Polygon2D>("CardImage");
		if (indicator != null) {
			indicator.Visible = false;
			indicatorX = indicator.Position.X;
			indicator.Position = new Vector2(indicatorX, indicatorMin);
		}
	}
	
	private Sprite2D indicator;
	private float indicatorMin = -30;
	private float indicatorMax = -10;
	private float indicatorDir = 1;
	private float indicatorX;
	
	public override void _Process(double delta) {
		if (indicator != null) {
			indicator.Position += new Vector2(0, 1 / (2 * (float)delta) * indicatorDir * (float)delta);
			if (indicator.Position.Y > indicatorMax) {
				indicatorDir = -1;
			} else if (indicator.Position.Y < indicatorMin) {
				indicatorDir = 1;
			}
		}
	}
	
	public void ReduceDurability(double amount) {
		durability -= amount;
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
	
	public async Task UpdatePosition(Vector2 position, Vector2 scale) {
		tween = GetTree().CreateTween();
		tween.TweenProperty(this, "position", position, 0.1f);
		tween.TweenProperty(this, "scale", scale, 0.1f);
		await ToSignal(tween, "finished");
	}
	
	public async Task UpdatePosition(Vector2 position) {
		tween = GetTree().CreateTween();
		tween.TweenProperty(this, "position", position, 0.1f);
		await ToSignal(tween, "finished");
	}

	public async Task SwapPositions(Card other) {
		Vector2 thisPosition = GlobalPosition;
		Vector2 otherPosition = other.GlobalPosition;
		
		tween = GetTree().CreateTween();
		tween.TweenProperty(this, "global_position", otherPosition, 0.25f);
		
		other.tween = GetTree().CreateTween();
		other.tween.TweenProperty(other, "global_position", thisPosition, 0.25f);
		
		await ToSignal(tween, "finished");
		await ToSignal(other.tween, "finished");
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
	
	public async Task Flip() {
		Vector2[] start = sprite.Polygon;
		Vector2 top = new Vector2((start[0].X + start[1].X) / 2f, start[0].Y - 15);
		Vector2 bot = new Vector2((start[2].X + start[3].X) / 2f, start[2].Y - 15);
		Vector2[] end = new Vector2[] {
			top, top, bot, bot
		};
		await UpdatePolygon(start, end, 0.1f);
		visible = !visible;
		UpdateTexture();
		await UpdatePolygon(end, start, 0.1f);
	}
	
	public async Task Shake() {
		Vector2[] mid = sprite.Polygon;
		Vector2[] left = (Vector2[])mid.Clone();
		Vector2[] right = (Vector2[])mid.Clone();
		
		for (int i = 0; i < 4; i++) {
			left[i] += new Vector2(-2, 0);
			right[i] += new Vector2(2, 0);
		}
		
		for (int i = 0; i < 3; i++) {
			await UpdatePolygon(mid, left, 0.02f);
			await UpdatePolygon(left, mid, 0.02f);
			await UpdatePolygon(mid, right, 0.02f);
			await UpdatePolygon(right, mid, 0.02f);
		}
	}
	
	private Vector2[] _start, _end;
	
	private async Task UpdatePolygon(Vector2[] start, Vector2[] end, float duration) {
		tween = CreateTween();
		_start = start;
		_end = end;
		tween.TweenMethod(
			new Callable(this, nameof(UpdatePolygonHelper)),
			0.0f, 1.0f, duration
			);
		await ToSignal(tween, "finished");
	}
	
	private void UpdatePolygonHelper(float t) {
		Vector2[] current = new Vector2[4];
		for (int i = 0; i < 4; i++) {
			current[i] = _start[i].Lerp(_end[i], t);
		}
		sprite.Polygon = current;
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
	
	public override void _GuiInput(InputEvent @event) {
		if (@event is InputEventMouseButton mouseEvent &&
			mouseEvent.Pressed &&
			mouseEvent.ButtonIndex == MouseButton.Left)
		{
			EmitSignal(SignalName.CardClicked, this);
		}
	}
	
	public void Highlight() {
		if ((index == -1 && !isPlayer) || !ready) return;
		Shader shader = GD.Load<Shader>("res://Shaders/card_highlight.gdshader");
		ShaderMaterial mat = new ShaderMaterial { Shader = shader };
		sprite.Material = mat;
		
		if (index != -1 || (tween != null && tween.IsRunning())) return;

		tween = GetTree().CreateTween();
		tween.TweenProperty(this, "position", upperPosition, 0.05f);
	}
	
	public void Unhighlight() {
		if ((index == -1 && !isPlayer) || locked || !ready) return;
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
