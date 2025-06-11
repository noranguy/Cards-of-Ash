using Godot;
using System;

public partial class TitleScreen : TextureRect
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Button>("ButtonContainer/ButtonBox/NewGame").Pressed +=
			() => GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("settings_window.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
