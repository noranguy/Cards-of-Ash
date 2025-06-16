using Godot;
using System;

public partial class TitleScreen : TextureRect
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<TextureButton>("ButtonContainer/ButtonBox/Settings").Pressed +=
			() => GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("settings_screen.tscn");

		GetNode<Button>("ButtonContainer/ButtonBox/NewGame").Pressed +=
			() => GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("safehouse.tscn");
	}
}
