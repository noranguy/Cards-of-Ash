using Godot;
using System;

public partial class WoodPanels : TextureRect
{
	[Signal]
	public delegate void DragStartEventHandler();
	[Signal]
	public delegate void DragEndedEventHandler();
	public bool is_dragged { get; set; } = false;
	private bool was_dropped = false;
	private Texture2D og_texture;
	public override Variant _GetDragData(Vector2 atPosition)
	{
		is_dragged = true;
		was_dropped = false;
		og_texture = Texture;
		EmitSignal(SignalName.DragStart);
		var preview_text = new TextureRect();
		preview_text.Texture = Texture;
		preview_text.ExpandMode = ExpandModeEnum.IgnoreSize;
		preview_text.Size = new Vector2(20, 20);
		Control preview = new Control();
		preview.ZIndex = 5;
		preview.AddChild(preview_text);
		SetDragPreview(preview);
		Texture = null;
		return preview_text.Texture;
	}
	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		return data.VariantType == Variant.Type.Object && data.AsGodotObject() is Texture2D;
	}
	public override void _DropData(Vector2 atPosition, Variant data)
	{
		Texture = (Texture2D)data;
		is_dragged = false;
		was_dropped = true;
		EmitSignal(SignalName.DragEnded);
	}
	public override void _GuiInput(InputEvent @event)
	{
		if (is_dragged && @event is InputEventMouseButton mouseEvent && !mouseEvent.Pressed)
		{
			is_dragged = false;
			if (!was_dropped)
			{
				Texture = og_texture;
			}
			EmitSignal(SignalName.DragEnded);
		}
    }



}
