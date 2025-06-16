using Godot;
using System;

public partial class Safehouse : StaticBody2D
{
	CharacterBody2D _player;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		GetNode<Control>("EndDayPrompt").Visible = false;
		_player = GetNode<CharacterBody2D>("PlayerCharacter");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	
	}

	void _on_bed_body_entered(Node2D body)
	{
		GD.Print("Mreow");
		if (Input.IsActionPressed("interact"))
		{
			GetNode<Control>("EndDayPrompt").Visible = true;
		}
	}

	void _on_cancel_pressed()
	{
		GetNode<Control>("EndDayPrompt").Visible = false;
	}
}
