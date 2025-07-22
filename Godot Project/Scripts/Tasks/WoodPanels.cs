using Godot;
using Microsoft.VisualBasic;
using System;
using System.Runtime.CompilerServices;

public partial class WoodPanels : TextureRect {
	[Signal]
	public delegate void DroppedEventHandler();
	
	int count = 0;
	
	public override bool _CanDropData(Vector2 position, Variant data) {
		WoodPlanks plank = data.As<WoodPlanks>();
		if (GlobalState.Instance.phase < 2) {
			return count < 1 && plank != null && !plank.isPlank;
		} else {
			return count < 3 && plank != null && plank.isPlank;
		}
	}

	public override void _DropData(Vector2 position, Variant data) {
		var plank = data.As<WoodPlanks>();
		if (plank != null) {
			plank.GetParent().RemoveChild(plank);
			AddChild(plank);
			if (count > 0) {
				plank.Texture = GD.Load<Texture2D>($"res://Assets/In Play Safe House/Tasks/wood_plank_{count-1}.png");
				plank.Position = new Vector2(10, 15);
			} else {
				plank.Position = Vector2.Zero;
			}
			plank.MouseFilter = Control.MouseFilterEnum.Ignore;
			plank.ZIndex = 1;
			EmitSignal(SignalName.Dropped);
			count++;
			GlobalState.Instance.phase++;
		}
	}
}
