using Godot;
using System;
using System.ComponentModel;

// Safehouse area
public partial class Safehouse : StaticBody2D
{
	CharacterBody2D _player;

	//RayCast2D _ray; - May come back to this, for now ignore all the ray stuff

	float _day_num; // Keep track of day, mornings will be whole numbers, nights will be X.5

	// Flags to see if the player is in an interactable area
	bool _in_bed = false;
	bool _at_door = false;
	bool _at_table = false;

	// Flags for enviroment
	bool _npc_waiting = false;
	String[] _character_order = ["old_man_tutorial_dialogue", "mom_dialogue", "Kaishain", "Kid", "OldManEnd"];

	bool _player_has_cards = false;

	// Will be prompt nodes
	Control _end_day_prompt;
	Control _open_door_prompt;
	Control _start_game_prompt;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Get all the prompt nodes
		_end_day_prompt = GetNode<Control>("EndDayPrompt");
		_open_door_prompt = GetNode<Control>("OpenDoorPrompt");
		_start_game_prompt = GetNode<Control>("StartGamePrompt");

		//_ray = GetNode<RayCast2D>("PlayerCharacter/RayCast2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// If the player is trying to interact with something
		if (Input.IsActionJustPressed("interact"))
		{

			// If the player is in an interactable area, then show the prompt for that object
			if (_at_table)
			{
				if (_player_has_cards)
				{
					_start_game_prompt.Visible = true;
				}

				else
				{
					GetNode<TextureRect>("MenkoCards").Visible = false;
					_player_has_cards = true;
				}

			}

			else if (_in_bed)
			{
				_end_day_prompt.Visible = true;
			}

			else if (_at_door)
			{
				if (!_npc_waiting)
				{
					_open_door_prompt.Visible = true;
				}
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
		_open_door_prompt.Visible = false;
		_start_game_prompt.Visible = false;
	}

	// When the player opens the door for the NPC
	void _on_open_door_pressed()
	{
		GetNode<AnimationPlayer>("FadeToBlack/AnimationPlayer").Play("fade_to_dialogue");
	}

	// Start Menko Game
	void _on_start_game_pressed()
	{
		GetNode<AnimationPlayer>("FadeToGame/AnimationPlayer").Play("fade_to_game");
	}

	void _on_animation_player_animation_finished(StringName anim_name)
	{
		if (anim_name == "fade_to_game")
		{
			GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("menko_game.tscn");
		}
		else if (anim_name == "fade_to_dialogue")
		{
			GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene($"{_character_order[(int)_day_num]}.tscn");
		}
	}

	void _knock_at_door()
	{
		_npc_waiting = true;
	}
}
