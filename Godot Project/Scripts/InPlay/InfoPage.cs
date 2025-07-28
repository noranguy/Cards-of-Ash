using Godot;
using System;

public partial class InfoPage : Node2D {
	public override void _Ready()
	{

		string[] types = new string[] { "Tsunami", "Volcano", "Earthquake" };
		Info info = GlobalState.Instance.GetInfo();
		string titleText = info.Title;
		string clas = info.Class;
		string descriptionText = info.Description;

		foreach (string type in types) {
			var sprite = GetNode<Sprite2D>(type);
			var texture = GD.Load<Texture2D>($"res://Assets/Cards/{type.ToLower()}_{clas}.png");
			sprite.Texture = texture;
		}

		var title = GetNode<RichTextLabel>("Title");
		title.Text = titleText;
		var description = GetNode<RichTextLabel>("Description");
		description.Text = descriptionText;
	}
	
	public override void _Input(InputEvent @event) {
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Space) {
			//GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("safehouse.tscn");
			//return;
			if (GlobalState.Instance.GetDay() < 2) {
				GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("menko_game.tscn");
			} else {
				GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("deck_builder.tscn");
			}
		}
	}
}
