using Godot;
using System;
using System.ComponentModel;

// Safehouse area
public partial class Safehouse : StaticBody2D
{
	CharacterBody2D _player;

	//RayCast2D _ray; - May come back to this, for now ignore all the ray stuff

	float _day_num; // Keep track of day, mornigns will be whole numbers, nights will be X.5

	// Flags to see if the player is in an interactable area
	bool _in_bed = false;
	bool _at_door = false;
	bool _at_table = false;

	// Will be prompt nodes
	Control _end_day_prompt;
	Control _go_outside_prompt;
	Control _start_game_prompt;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Get all the prompt nodes
		_end_day_prompt = GetNode<Control>("EndDayPrompt");
		_go_outside_prompt = GetNode<Control>("GoOutsidePrompt");
		_start_game_prompt = GetNode<Control>("StartGamePrompt");

		//_ray = GetNode<RayCast2D>("PlayerCharacter/RayCast2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// If the player is trying to interact with something
		if (Input.IsActionJustPressed("interact"))
		{
			GD.Print("Mreow");

			// If the player is in an interactable area, then show the prompt for that object
			if (_at_table)
			{
				_start_game_prompt.Visible = true;
			}

			else if (_in_bed)
			{
				_end_day_prompt.Visible = true;
			}

			else if (_at_door)
			{
				_go_outside_prompt.Visible = true;
			}
		}
	}

	// Signals from interactable areas
	void _on_bed_body_entered(Node2D body)
	{
		_in_bed = true;
	}

	void _on_bed_body_exited(Node2D body)
	{
		_in_bed = false;
	}

	void _on_door_body_entered(Node2D body)
	{
		_at_door = true;
	}

	void _on_door_body_exited(Node2D body)
	{
		_at_door = false;
	}

	void _on_menko_table_body_entered(Node2D body)
	{
		_at_table = true;
	}

	void _on_menko_table_body_exited(Node2D body)
	{
		_at_table = false;
	}

	// Get rid of all prompts
	void _on_cancel_pressed()
	{
		_end_day_prompt.Visible = false;
		_go_outside_prompt.Visible = false;
		_start_game_prompt.Visible = false;
	}
}
