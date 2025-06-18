using Godot;
using System;

public partial class Card : Node2D
{
	[Signal]
	public delegate void CardClickedEventHandler(Card card);
	
	public bool locked = false;
	public bool visible;
	public bool isHand;
	public string type;
	private Sprite2D sprite;
	
	public override void _Ready() {
		sprite = GetNode<Sprite2D>("CardImage");
	}
	
	public void Init(string cardType, bool cardVisible, bool cardIsHand) {
		if (sprite == null) {
			sprite = GetNode<Sprite2D>("CardImage");
		}
		type = cardType;
		visible = cardVisible;
		isHand = cardIsHand;
		
		if (visible) {
			var texture = GD.Load<Texture2D>($"res://Assets/Cards/{type}.png");
			sprite.Texture = texture;
		} else {
			var texture = GD.Load<Texture2D>($"res://Assets/Cards/back.png");
			sprite.Texture = texture;
		}
	}
	
	public void Reveal() {
		visible = true;
		var texture = GD.Load<Texture2D>($"res://Assets/Cards/{type}.png");
		sprite.Texture = texture;
	}

	public void OnInputEvent(Node viewport, InputEvent @event, int shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent &&
			mouseEvent.Pressed &&
			mouseEvent.ButtonIndex == MouseButton.Left)
		{
			EmitSignal(SignalName.CardClicked, this);
		}
	}
	
	public void Highlight()
	{
		var sprite = GetNode<Sprite2D>("CardImage");
		string shaderCode = @"
shader_type canvas_item;

uniform vec4 glow_color : source_color = vec4(1.0, 1.0, 0.0, 1.0);
uniform float glow_strength = 1.5;
uniform float glow_radius = 0.02;

void fragment() {
	vec2 uv = UV;
	float dist_to_edge = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
	float glow = smoothstep(glow_radius, 0.0, dist_to_edge) * glow_strength;

	vec4 tex_color = texture(TEXTURE, uv);
	COLOR = mix(glow_color, tex_color, 1.0 - glow);
}";

		Shader shader = new Shader { Code = shaderCode };
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
		if (visible == isHand) {
			Highlight();
		}
	}
	
	public void OnMouseExited() {
		if (visible == isHand) {
			Unhighlight();
		}
	}
}
