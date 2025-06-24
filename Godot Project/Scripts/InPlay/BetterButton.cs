using Godot;
using System;
using System.Collections.Generic;

public partial class BetterButton : TextureButton {
	[Export] public Texture2D texture;
	
	[Signal]
	public delegate void PressedEventHandler();
	
	public override void _Ready() {
		Sprite2D sprite = GetNode<Sprite2D>("Image");
		sprite.Texture = texture;
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
