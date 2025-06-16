using Godot;
using System;

//Title Screen with three buttons: New Game, Load game (maybe), and settings
public partial class TitleScreen : TextureRect
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Start new Game
		GetNode<TextureButton>("ButtonContainer/ButtonBox/Settings").Pressed +=
			() => GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("settings_screen.tscn");

		//Go to settings
		GetNode<Button>("ButtonContainer/ButtonBox/NewGame").Pressed +=
			() => GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("safehouse.tscn");
	}
}
