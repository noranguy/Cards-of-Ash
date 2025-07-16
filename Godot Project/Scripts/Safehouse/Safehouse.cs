using Godot;
using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;

// Safehouse area
public partial class Safehouse : StaticBody2D
{

	private CharacterBody2D _player;

	//RayCast2D _ray; - May come back to this, for now ignore all the ray stuff

	private int _day_num; // Keep track of day, mornings will be whole numbers, nights will be X.5

	// Flags to see if the player is in an interactable area 
	private bool _in_bed;
	private bool _at_door;
	private bool _at_table;

	// If the character is in a prompt screen
	public bool in_prompt;

	//List that follows the day order for when characters show up
	private String[] _character_order = ["OldManTutorial", "Kaishain", "Mom", "Kid", "Foreigner", "OldManEndGame"];
	private String[] _dialogue_order = ["old_man_tutorial", "kaishain", "mom", "kid", "foreigner", "old_man_end_game"];
	private bool[] _inhabitants;

	// Flags to keep track of safehouse state
	private bool _player_has_cards;
	private bool _npc_waiting;
	private bool _game_ready;
	private bool _day_over;

	// Will be prompt nodes
	private Control _end_day_prompt;
	private Control _open_door_prompt;
	private Control _start_game_prompt;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Set all flags
		_player_has_cards = GlobalState.Instance.DoesPlayerHaveCards();
		_npc_waiting = false;
		_game_ready = false;
		_day_over = GlobalState.Instance.GetPostGame();
		_in_bed = false;
		_at_door = false;
		_at_table = false;

		_inhabitants = GlobalState.Instance.GetInhabitants();

		// Get all the prompt nodes
		_end_day_prompt = GetNode<Control>("EndDayPrompt");
		_open_door_prompt = GetNode<Control>("OpenDoorPrompt");
		_start_game_prompt = GetNode<Control>("StartGamePrompt");

		_player = GetNode<CharacterBody2D>("PlayerCharacter");
		_player.Visible = true;

		if (!_player_has_cards)
		{
			GetNode<Node2D>("MenkoCards").Visible = true;
			GetNode<AnimationPlayer>("MenkoCards/AnimationPlayer").Play("bounce");
		}

		_day_num = GlobalState.Instance.GetDay();

		_ = Start_dayAsync();
		GetNode<PlayerCharacter>("PlayerCharacter")._set_movable(true);

		//_ray = GetNode<RayCast2D>("PlayerCharacter/RayCast2D");
		Label dayLabel = GetNode<Label>("DayLabel");
		dayLabel.Text = $"Day {GlobalState.Instance.GetDay() + 1}";
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
				if (!_player_has_cards)
				{
					GetNode<Node2D>("MenkoCards").Visible = false;
					GetNode<AnimationPlayer>("MenkoCards/AnimationPlayer").Stop();
					_player_has_cards = true;
					GlobalState.Instance.PlayerGetsCards();
					Knock_at_door();
					_npc_waiting = true;
				}

				else if (_game_ready)
				{
					GetNode<TextureRect>("MenkoTable/Interact").Visible = false;
					_start_game_prompt.Visible = true;
					GetNode<PlayerCharacter>("PlayerCharacter")._set_movable(false);
				}
			}

			else if (_in_bed && _day_over)
			{
				_end_day_prompt.Visible = true;
				GetNode<PlayerCharacter>("PlayerCharacter")._set_movable(false);
			}

			else if (_at_door && _npc_waiting)
			{
				_open_door_prompt.Visible = true;
				GetNode<PlayerCharacter>("PlayerCharacter")._set_movable(false);
			}
		}
	}

	private void InhabitSafehouse()
	{
		if (_inhabitants[1] == true)
		{
			// Show Kaishain
		}
		if (_inhabitants[2] == true)
		{
			// Show Mom
		}
		if (_inhabitants[3] == true)
		{
			// Show Kid
		}
	}

	// Signals from interactable areas
	private void _on_bed_body_entered(Node2D body)
	{
		_in_bed = true;
	}

	private void _on_bed_body_exited(Node2D body)
	{
		_in_bed = false;
	}

	private void _on_door_body_entered(Node2D body)
	{
		_at_door = true;
	}

	private void _on_door_body_exited(Node2D body)
	{
		_at_door = false;
	}

	private void _on_menko_table_body_entered(Node2D body)
	{
		_at_table = true;
	}

	private void _on_menko_table_body_exited(Node2D body)
	{
		_at_table = false;
	}

	// Get rid of all prompts
	private void _on_cancel_pressed()
	{
		_end_day_prompt.Visible = false;
		_open_door_prompt.Visible = false;
		_start_game_prompt.Visible = false;
		GetNode<PlayerCharacter>("PlayerCharacter")._set_movable(true);
	}

	// When the player opens the door for the NPC
	private void _on_open_door_pressed()
	{
		_day_over = false;
		_npc_waiting = false;
		_game_ready = true;
		GetNode<Node2D>("Knock").Visible = false;
		GetNode<TextureRect>("Door/Interact").Visible = false;
		GetNode<AnimationPlayer>("Knock/AnimationPlayer").Stop();
		GetNode<AnimationPlayer>("FadeToBlack/AnimationPlayer").Play("fade_to_black_dialogue");
	}

	// Start Menko Game
	private void _on_start_game_pressed()
	{
		GlobalState.Instance.SetPostGame(true);
		GetNode<AnimationPlayer>("FadeToBlack/AnimationPlayer").Play("fade_to_game");
	}

	private void _on_end_day_pressed()
	{
		GetNode<AnimationPlayer>("FadeToBlack/AnimationPlayer").Play("fade_to_night");
	}

	private void _on_animation_player_animation_finished(StringName anim_name)
	{
		if (anim_name == "fade_to_game")
		{
			_start_game_prompt.Visible = false;
			GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("info_page.tscn");
		}

		else if (anim_name == "fade_to_black_dialogue")
		{
			_ = Dialogue_setupAsync();
			GetNode<AnimationPlayer>("FadeToBlack/AnimationPlayer").Play("fade_to_dialogue");
		}

		else if (anim_name == "fade_to_night")
		{
			GlobalState.Instance.NextDay();
			GlobalState.Instance.SetPostGame(false);
			GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("safehouse.tscn");
		}
	}

	private void Knock_at_door()
	{
		if (_day_num == 0)
		{
			GetNode<TextureRect>("Door/Interact").Visible = true;
			GetNode<AnimationPlayer>("Door/AnimationPlayer").Play("bounce");
		}

		GetNode<Node2D>("Knock").Visible = true;
		GetNode<AnimationPlayer>("Knock/AnimationPlayer").Play("knocking");
		_npc_waiting = true;
		_day_over = false;
		_game_ready = false;
	}

	private async Task Dialogue_setupAsync()
	{
		string _character = _character_order[_day_num];
		string dialogue = _dialogue_order[_day_num];
		_open_door_prompt.Visible = false;
		GetNode<CharacterBody2D>(_character).Visible = true;
		GetNode<CharacterBody2D>(_character).CollisionLayer = 1;
		_player.Position = new Vector2(105, 85);

		await DialogueManager.Instance.StartDialogue(dialogue, false);

		if (_day_num == 0)
		{
			GetNode<TextureRect>("MenkoTable/Interact").Visible = true;
			GetNode<AnimationPlayer>("MenkoTable/AnimationPlayer").Play("bounce");
		}

		GetNode<PlayerCharacter>("PlayerCharacter")._set_movable(true);
		_npc_waiting = false;
		_day_over = false;
		_game_ready = true;
		
		//GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene($"Dialogue/{_dialogue_order[(int)_day_num]}.tscn"); for when theres a whole scene for dialogue
	}

	private async Task Start_dayAsync()
	{
		if (!_day_over)
		{
			switch (_day_num)
			{
				case 0:
					await DialogueManager.Instance.StartDialogue("StartDay/pick_up_card_prompt", false);
					break;
				case 1:
					await DialogueManager.Instance.StartDialogue("StartDay/check_door_prompt", false);
					Knock_at_door();
					break;
				case 2:
					await DialogueManager.Instance.StartDialogue("StartDay/check_door_prompt", false);
					Knock_at_door();
					break;
				case 3:
					await DialogueManager.Instance.StartDialogue("StartDay/check_door_prompt", false);
					Knock_at_door();
					break;
				case 4:
					await DialogueManager.Instance.StartDialogue("StartDay/check_door_prompt", false);
					Knock_at_door();
					break;
			}
		}
		else
		{
			switch (_day_num)
			{
				case 0:
					await DialogueManager.Instance.StartDialogue("EndDay/sleep_prompt", false);
					break;
				case 1:
					await DialogueManager.Instance.StartDialogue("EndDay/sleep_prompt", false);
					break;
				case 2:
					await DialogueManager.Instance.StartDialogue("EndDay/sleep_prompt", false);
					break;
				case 3:
					await DialogueManager.Instance.StartDialogue("EndDay/sleep_prompt", false);
					break;
				case 4:
					await DialogueManager.Instance.StartDialogue("EndDay/sleep_prompt", false);
					break;
			}
		}
	}
}
