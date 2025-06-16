using Godot;
using System;

//Splash screen that shows up for a few seconds when the game starts
public partial class SplashScreen : TextureRect
{
	public override void _Ready()
	{
		//Once the timer runs out go to the title screen
		GetNode<Timer>("Timer").Timeout += ()
		=> GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("title_screen.tscn");
	}
}
