using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Task2_minigame : Node2D
{
	private List<WoodPanels> wood_panels_storer = [];
	private List<WoodPanels> wood_panels_placer = [];
	private List<WoodPanels> wood_planks_storer = [];
	private List<WoodPanels> wood_planks_placer = [];
	private WoodPanels wood_panel;
	private WoodPanels wood_panel_empty;
	private WoodPanels wood_plank; 
	private WoodPanels wood_plank_empty;

	private bool alphaVisible = false;
	public readonly int num_panels = 2; // number of possible panels
	public readonly int num_planks = 4;
	int phase ;
	public override void _Ready()
	{
		phase = 0;
		for (int i = 0; i < num_panels; i++)
		{
			wood_panel = GetNode<WoodPanels>($"stored_wood/wood_panels_{i}");
			wood_panel_empty = GetNode<WoodPanels>($"placed_wood/wood_panels_{i}");
			initWoodPanel(wood_panel, false, i, "stored_wood", "panels");
			initWoodPanel(wood_panel_empty, true, i, "placed_wood", "panels");
		}
		for (int j = 0; j < num_planks; j++)
		{
			wood_plank = GetNode<WoodPanels>($"stored_wood/wood_planks_{j}");
			wood_plank_empty = GetNode<WoodPanels>($"placed_wood/wood_planks_{j}");
			initWoodPanel(wood_plank, false, j, "stored_wood", "planks");
			initWoodPanel(wood_plank_empty, true, j, "placed_wood", "planks");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (phase == 0)
		{ // to prevent drag issues with panels in front of each other
			if (wood_panels_storer[1].Texture == null)
			{
				wood_panels_storer[1].Visible = false;
			} // check if panels are placed and disable movement
			for (int i = 0; i < num_panels; i++)
			{
				if (wood_panels_placer[i].Texture != null)
				{
					wood_panels_placer[i].can_drag = false; //disable drag after placed on window
				}
			}
			//change phase
			if (wood_panels_placer[0].Texture != null && wood_panels_placer[1].Texture != null)
			{
				phase++;
			}
		}
		else if (phase == 1)
		{
			//run dialogue
			makePlanksVisible();
			foreach (WoodPanels child in wood_planks_storer)
			{
				if (child.Texture == null)
				{
					child.Visible = false;
				}
			}
			for (int i = 0; i < num_planks; i++)
			{
				WoodPanels child = wood_planks_placer[i];
				if (child.Texture != null && child.can_drag)
				{
					Sprite2D cross = GetNode<Sprite2D>($"wood_plank_{i}");
					cross.Visible = true;
					child.can_drag = false;
					child.Visible = false;
				}
			}
		}

	}
	public void makePlanksVisible()
	{
		foreach (WoodPanels child in wood_planks_placer)
		{
			child.Visible = true;
		}
		foreach (WoodPanels child in wood_planks_storer)
		{
			child.Visible = true;
		}
	}
	public void initWoodPanel(WoodPanels wood, bool empty, int num, string path, string type)
	{
		Panel panel = GetNode<Panel>($"{path}/wood_{type}_{num}/Panel");
		changeAlpha(panel, false);
		if (empty)
		{
			if (type == "panels")
			{
				wood_panels_placer.Add(wood);
			}
			else // planks type
			{
				Sprite2D shadow = (Sprite2D)wood.GetNode("Sprite2D"); //sprite 2d shadow
				shadow.Texture = GD.Load<Texture2D>($"res://Assets/In Play Safe House/Tasks/wood_plank_shadow_{num % 2}.png");
				wood.Visible = false;
				wood_planks_placer.Add(wood);
			}
		}
		else //wood texture is present
		{
			if (type == "panels")
			{
				wood.Texture = GD.Load<Texture2D>("res://Assets/In Play Safe House/Tasks/wood panel.png");
				wood_panels_storer.Add(wood);
			}
			else
			{
				Sprite2D wood_plank_sprite = (Sprite2D)wood.GetNode("Sprite2D");
				wood_plank_sprite.Texture = GD.Load<Texture2D>($"res://Assets/In Play Safe House/Tasks/tile0{num}.png"); // bottom to top
				wood.Visible = false;
				wood_planks_storer.Add(wood);
			}
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
			var placed_wood_panel_0 = wood_panels_placer[0].GetNode<Panel>("Panel");
			var placed_wood_panel_1 = wood_panels_placer[1].GetNode<Panel>("Panel");
			changeAlpha(placed_wood_panel_0, true);
			changeAlpha(placed_wood_panel_1, true);
			alphaVisible = true;
		}
	}
	public void OnDragEnd()
	{
		if (alphaVisible)
		{
			var placed_wood_panel_0 = wood_panels_placer[0].GetNode<Panel>("Panel");
			var placed_wood_panel_1 = wood_panels_placer[1].GetNode<Panel>("Panel");
			changeAlpha(placed_wood_panel_0, false);
			changeAlpha(placed_wood_panel_1, false);
			alphaVisible = false;
		}
	}
}
