using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Task2_minigame : Node2D
{
	private List<WoodPanels> wood_panels_storer = new();
	private List<WoodPanels> wood_panels_placer = new();
	private WoodPanels wood_panel;
	private WoodPanels wood_panel_empty;
	private Panel placed_wood_panel_0;
	private Panel placed_wood_panel_1;
	private bool alphaVisible = false;
	public readonly int num_panels = 2; // number of possible places

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for (int i = 0; i < num_panels; i++)
		{
			wood_panel = GetNode<WoodPanels>($"stored_wood/wood_panels_{i}");
			wood_panel_empty = GetNode<WoodPanels>($"placed_wood/wood_panels_{i}");
			initWoodPanel(wood_panel, false, i, "stored_wood");
			initWoodPanel(wood_panel_empty, true, i, "placed_wood");
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (wood_panels_storer[1].Texture == null)
		{
			wood_panels_storer[1].Visible = false;
		}

	}
	public void initWoodPanel(WoodPanels wood, bool empty, int num, string path)
	{
		Panel panel = GetNode<Panel>($"{path}/wood_panels_{num}/Panel");
		changeAlpha(panel, false);
		if (empty)
		{
			wood_panels_placer.Add(wood);
		}
		else //wood texture is present
		{
			wood.Texture = GD.Load<Texture2D>("res://Assets/In Play Safe House/Tasks/wood panel.png");
			wood_panels_storer.Add(wood);
			wood.Connect(WoodPanels.SignalName.DragStart, new Callable(this, nameof(OnDragStart)));
			wood.Connect(WoodPanels.SignalName.DragEnded, new Callable(this, nameof(OnDragEnd)));
		}

	}
	public void changeAlpha(Panel panel, bool visible)
	{
		float alpha = visible ? 1.0f : 0f;
		if (!visible)
		{
			panel.Modulate = new Color(panel.Modulate.R, panel.Modulate.G, panel.Modulate.B, alpha);
		}
		else
		{
			panel.Modulate = new Color(panel.Modulate.R, panel.Modulate.G, panel.Modulate.B, alpha);
		}
	}
	public void OnDragStart()
	{
		if (!alphaVisible)
		{
			placed_wood_panel_0 = wood_panels_placer[0].GetNode<Panel>("Panel");
			placed_wood_panel_1 = wood_panels_placer[1].GetNode<Panel>("Panel");
			changeAlpha(placed_wood_panel_0, true);
			changeAlpha(placed_wood_panel_1, true);
			alphaVisible = true;
		}
	}
	public void OnDragEnd()
	{
		if (alphaVisible)
		{
			changeAlpha(placed_wood_panel_0, false);
			changeAlpha(placed_wood_panel_1, false);
			alphaVisible = false;
		}
	}
}
