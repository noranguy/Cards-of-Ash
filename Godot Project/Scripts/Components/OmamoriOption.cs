using Godot;
using System;

public partial class OmamoriOption : Control {
	[Signal]
	public delegate void ActiveSetEventHandler(OmamoriOption omamori);
	
	public bool active = false;
	public bool locked = false;
	
	private ColorRect hoverOverlay;
	private ColorRect tooltip;
	
	public async override void _Ready() {
		if (Name != "Current" && Name != "OmamoriOption") {
			var texture = GD.Load<Texture2D>($"res://Assets/Omamori/{Name}.png");
			GetNode<TextureRect>("Design").Texture = texture;
		}
		
		hoverOverlay = GetNode<ColorRect>("HoverOverlay");
		tooltip = GetNode<ColorRect>("Tooltip");
		
		tooltip.Visible = true;
		
		var tooltipLabel = tooltip.GetNode<RichTextLabel>("Text");
		
		if (Name != "Current" && Name != "OmamoriOption") {
			tooltipLabel.Text = GlobalState.Instance.OmamoriDescriptions[Name];
			await ToSignal(GetTree().CreateTimer(0.15f), "timeout");
			tooltip.Size = (Name == "none") ? new Vector2(68, 21) : tooltipLabel.Size + new Vector2(8, 8);
		} else {
			tooltipLabel.Text = "Current Omamori";
			tooltip.Size = new Vector2(94, 21);
		}
		
		tooltip.Visible = false;
		//tooltip.Position = Vector2.Zero;
		
		MouseEntered += () => {
			if (!locked) {
				hoverOverlay.Visible = true;
			}
			tooltip.Visible = true;
		};
		MouseExited += () => {
			if (!active) {
				hoverOverlay.Visible = false;
			}
			tooltip.Visible = false;
		};
	}
	
	public override void _Process(double delta) {
		//tooltip.GlobalPosition = GetViewport().GetMousePosition() - new Vector2(32, 183);
	}
	
	public override void _GuiInput(InputEvent @event) {
		if (
			@event is InputEventMouseButton mouseEvent &&
			mouseEvent.ButtonIndex == MouseButton.Left &&
			mouseEvent.Pressed
		) {
			if (!locked) {
				if (Name != "Current") {
					active = true;
				}
				EmitSignal(SignalName.ActiveSet, this);
			}
		}
	}
	
	public void Deselect() {
		active = false;
		hoverOverlay.Visible = false;
	}
	
	public void Lock() {
		locked = true;
		GetNode<TextureRect>("Lock").Visible = true;
		GetNode<TextureRect>("Design").Modulate = new Color(1, 1, 1, 0.4f);
	}
	
	public override Variant _GetDragData(Vector2 atPosition) {
		var dragPreview = Duplicate() as Control;
		if (dragPreview != null) {
			SetDragPreview(dragPreview);
		}
		return this;
	}
}
