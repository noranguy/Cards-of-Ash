using Godot;
using System;
using System.Collections.Generic;

public partial class OmamoriSelector : Control {
	private HFlowContainer optionsBox;
	private TextureRect options;
	private OmamoriOption current;
	public string currentName;
	private Dictionary<string, OmamoriOption> omamoriMap;
	
	public override void _Ready() {
		omamoriMap = new Dictionary<string, OmamoriOption>();
		
		var optionsBox = GetNode<HFlowContainer>("Options/OptionsBox");
		current = GetNode<OmamoriOption>("Current");
		options = GetNode<TextureRect>("Options");
		
		options.Visible = false;
		
		current.ActiveSet += (card) => options.Visible = !options.Visible;
		
		List<string> omamories = GlobalState.Instance.Omamories;
		foreach (string name in omamories) {
			omamoriMap[name] = optionsBox.GetNode<OmamoriOption>(name);
			omamoriMap[name].ActiveSet += UpdateActive;
		}
		
		 //for (int i = Math.Max(1, GlobalState.Instance.GetDay()); i < omamories.Count; i++) {
		 	//omamoriMap[omamories[i]].Lock();
		 //}
		
		currentName = GlobalState.Instance.GetOmamori();
		UpdateActive(omamoriMap[currentName]);
	}
	
	private void UpdateActive(OmamoriOption omamori) {
		omamoriMap[currentName].Deselect();
		
		currentName = omamori.Name;
		var texture = GD.Load<Texture2D>($"res://Assets/Omamori/{currentName}.png");
		current.GetNode<TextureRect>("Design").Texture = texture;
		
		options.Visible = false;
	}
}
