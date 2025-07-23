using Godot;
using System;

public partial class WoodPlanks : TextureRect {
	[Export]
	public bool isPlank;

	public override Variant _GetDragData(Vector2 atPosition) {
		var dragPreview = Duplicate() as Control;
		if (dragPreview != null) {
			SetDragPreview(dragPreview);
		}
		return this;
	}
}
