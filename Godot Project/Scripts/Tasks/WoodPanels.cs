using Godot;
using Microsoft.VisualBasic;
using System;
using System.Runtime.CompilerServices;

public partial class WoodPanels : TextureRect
{
	[Signal]
	public delegate void DragStartEventHandler();
	[Signal]
	public delegate void DragEndedEventHandler();
	private Texture2D og_texture;
	private Vector2 og_position;

	public bool can_drag = true;
	public bool can_drop = true;
	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (!can_drag)
		{
			return new Variant();
		}
		og_position = atPosition;
		EmitSignal(SignalName.DragStart);
		var preview_text = new TextureRect();
		preview_text.Texture = Texture;
		og_texture = preview_text.Texture;
		preview_text.ExpandMode = ExpandModeEnum.IgnoreSize;
		preview_text.Size = new Vector2(30, 30);
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
		EmitSignal(SignalName.DragEnded);
	}

}
