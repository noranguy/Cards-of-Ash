using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Task2_minigame : Node2D {
	private List<WoodPanels> woodPanels;

	private bool alphaVisible = false;
	private int numPanels = 2; // number of possible panels
	private int numPlanks = 4;
	private int numBoards = 2;
	
	public override void _Ready() {
		GlobalState.Instance.phase = 0;
		woodPanels = new List<WoodPanels>();
		
		for (int i = 1; i <= numPanels; i++) {
			var woodPanel = GetNode<WoodPanels>($"placed_wood/WoodPanels{i}");
			woodPanels.Add(woodPanel);
		}
	}

	public override void _Process(double delta) {
		if (!Input.IsMouseButtonPressed(MouseButton.Left)) {
			OnDragEnd();
		} else {
			OnDragStart();
		}
		
		if (GlobalState.Instance.phase == 6) {
			GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("safehouse.tscn");
		}
	}

	public void changeAlpha(Panel panel, bool visible) {
		float alpha = visible ? 1.0f : 0f;
		if (!visible) {
			panel.Modulate = new Color(panel.Modulate.R, panel.Modulate.G, panel.Modulate.B, alpha);
		} else {
			panel.Modulate = new Color(panel.Modulate.R, panel.Modulate.G, panel.Modulate.B, alpha);
		}
	}
	public void OnDragStart() {
		if (!alphaVisible) {
			var placed_wood_panel_0 = woodPanels[0].GetNode<Panel>("Panel");
			var placed_wood_panel_1 = woodPanels[1].GetNode<Panel>("Panel");
			changeAlpha(placed_wood_panel_0, true);
			changeAlpha(placed_wood_panel_1, true);
			alphaVisible = true;
		}
	}
	
	public void OnDragEnd() {
		if (alphaVisible) {
			var placed_wood_panel_0 = woodPanels[0].GetNode<Panel>("Panel");
			var placed_wood_panel_1 = woodPanels[1].GetNode<Panel>("Panel");
			changeAlpha(placed_wood_panel_0, false);
			changeAlpha(placed_wood_panel_1, false);
			alphaVisible = false;
		}
	}
}
