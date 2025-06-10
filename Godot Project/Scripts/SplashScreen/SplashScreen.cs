using Godot;
using System;

public partial class SplashScreen : TextureRect
{
	public override void _Ready()
	{
		GetNode<Timer>("Timer").Timeout += ()
		=> GetNode<SceneLoader>("res://Scenes/SceneLoader/scene_loader.tscn/SceneLoader").ChangeToScene("title_screen.tscn");
	}
}
