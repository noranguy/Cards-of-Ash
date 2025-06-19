using Godot;
using System;

//Node used to change scenes
//Call the node's ChangeToScene function to change scenes
public partial class SceneLoader : Node
{
	public void ChangeToScene(string sceneName)
	{
		GetTree().ChangeSceneToFile($"res://Scenes/{sceneName}");
	}
}
