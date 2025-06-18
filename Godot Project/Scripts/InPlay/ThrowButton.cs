using Godot;
using System;

public partial class ThrowButton : TextureButton {
	[Signal]
	public delegate void PressedEventHandler();

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
