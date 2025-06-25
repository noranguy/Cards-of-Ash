using Godot;
using System;
using System.Collections.Generic;

public partial class BetterButton : TextureButton {
	[Export] public Texture2D texture;
	
	[Signal]
	public delegate void PressedEventHandler();
	
	private Sprite2D indicator;
	private float indicatorMin;
	private float indicatorMax;
	private float indicatorDir = 1;
	private float indicatorX;
	
	public override void _Ready() {
		Sprite2D sprite = GetNode<Sprite2D>("Image");
		sprite.Texture = texture;
		indicatorMin = -texture.GetHeight() / 2 - 30;
		indicatorMax = -texture.GetHeight() / 2 - 10;
		indicator = GetNode<Sprite2D>("Indicator");
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

	public void OnInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
		if (
			@event is InputEventMouseButton mouseEvent &&
			mouseEvent.Pressed &&
			mouseEvent.ButtonIndex == MouseButton.Left
		) {
			EmitSignal(SignalName.Pressed);
		}
	}
}
