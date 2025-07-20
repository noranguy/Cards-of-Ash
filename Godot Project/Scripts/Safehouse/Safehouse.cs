using Godot;
using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;

// Safehouse area
public partial class Safehouse : StaticBody2D
{

	private CharacterBody2D _player;
	private TileMapLayer window_day; //safehouse object
	private TileMapLayer window_night;
	private Node2D night_safehouse_lighting;

	//RayCast2D _ray; - May come back to this, for now ignore all the ray stuff

	private int _day_num; // Keep track of day number, gotten from global state

	// Flags to see if the player is in an interactable area 
	private bool _in_bed;
	private bool _at_door;
	private bool _at_table;

	// Flags to see if the player is trying to talk to an npc, order [kaishain, mom, kid, foreigner]
	private bool[] _talking_to = {false, false, false, false};

	// Flags for what dialogue should be shown to the player
	private bool[] _mission_completed;
	private bool[] _dialogue_exhausted = {false, false, false, false};

	private string[] characters = ["kaishain", "mom", "kid", "foreigner"];

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
		InhabitSafehouse();

		for (int i = 0; i < 4; i++)
		{
			_dialogue_exhausted[i] = false;
			_talking_to[i] = false;
		}

		_mission_completed = GlobalState.Instance.GetCompletedMission();
		
		// Get all the prompt nodes
		_end_day_prompt = GetNode<Control>("CanvasLayer/EndDayPrompt");
		_open_door_prompt = GetNode<Control>("CanvasLayer/OpenDoorPrompt");
		_start_game_prompt = GetNode<Control>("CanvasLayer/StartGamePrompt");

		_player = GetNode<CharacterBody2D>("PlayerCharacter");
		_player.Visible = true;

		window_day = GetNode<TileMapLayer>("Background/TileMap/window/day");
		window_night = GetNode<TileMapLayer>("Background/TileMap/window/night");
		night_safehouse_lighting = GetNode<Node2D>("Background/night_lighting");

		if (!_player_has_cards)
		{
			GetNode<Node2D>("MenkoCards").Visible = true;
			GetNode<AnimationPlayer>("MenkoCards/AnimationPlayer").Play("bounce");
		}

		_day_num = GlobalState.Instance.GetDay();
		Label dayLabel = GetNode<Label>("DayLabel");
		dayLabel.Text = $"Day {GlobalState.Instance.GetDay() + 1}";

		_ = Start_dayAsync();
		GetNode<PlayerCharacter>("PlayerCharacter")._set_movable(true);

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

			// If the player is trying to talk to an npc, start the dialogue, maybe different (smaller) dialogues for each day 
			for (int i = 0; i < 4; i++)
			{
				if (_talking_to[i])
				{
					string partial_dialogue_path = $"Day{_day_num + 1}/{characters[i]}";
					if (_mission_completed[i])
					{
						if (_dialogue_exhausted[i])
						{
							_ = InSafeHouse_Dialogue_setupAsync($"ExhaustedMission/{partial_dialogue_path}", i);
							_dialogue_exhausted[i] = true;
						}

						else
						{
							_ = InSafeHouse_Dialogue_setupAsync($"Mission/{partial_dialogue_path}", i);
						}
					}

					else if (_dialogue_exhausted[i])
					{
						_ = InSafeHouse_Dialogue_setupAsync($"ExhaustedInSH/{partial_dialogue_path}", i);
					}

					else
					{
						_ = InSafeHouse_Dialogue_setupAsync($"InSafeHouse/{partial_dialogue_path}", i);
						_dialogue_exhausted[i] = true;
					}

					break;
				}
			}
		}
	}

	private void InhabitSafehouse()
	{
		if (_inhabitants[1])
		{
			GetNode<Area2D>("KaishainArea").Visible = true;
			GetNode<Area2D>("KaishainArea").CollisionLayer = 3;
			GetNode<CharacterBody2D>("KaishainArea/Kaishain").CollisionLayer = 1;

		}
		if (_inhabitants[2])
		{
			GetNode<Area2D>("MomArea").Visible = true;
			GetNode<Area2D>("MomArea").CollisionLayer = 3;
			GetNode<CharacterBody2D>("MomArea/Mom").CollisionLayer = 1;
		}
		if (_inhabitants[3])
		{
			GetNode<Area2D>("KidArea").Visible = true;
			GetNode<Area2D>("KidArea").CollisionLayer = 3;
			GetNode<CharacterBody2D>("KidArea/Kid").CollisionLayer = 1;
		}
		if (_inhabitants[4])
		{
			GetNode<Area2D>("ForeignerArea").Visible = true;
			GetNode<Area2D>("ForeignerArea").CollisionLayer = 3;
			GetNode<CharacterBody2D>("ForeignerArea/Foreigner").CollisionLayer = 1;
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

	private void _on_kaishain_area_body_entered(Node2D body)
	{
		_talking_to[0] = true;
	}

	private void _on_kaishain_area_body_exited(Node2D body)
	{
		_talking_to[0] = false;
	}

	private void _on_mom_area_body_entered(Node2D body)
	{
		_talking_to[1] = true;
	}

	private void _on_mom_area_body_exited(Node2D body)
	{
		_talking_to[1] = false;
	}

	private void _on_kid_area_body_entered(Node2D body)
	{
		_talking_to[2] = true;
	}

	private void _on_kid_area_body_exited(Node2D body)
	{
		_talking_to[2] = false;
	}

	private void _on_foreigner_area_body_entered(Node2D body)
	{
		_talking_to[3] = true;
	}

	private void _on_foreigner_area_body_exited(Node2D body)
	{
		_talking_to[3] = false;
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
			_ = DayStart_Dialogue_setupAsync();
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

	private async Task DayStart_Dialogue_setupAsync()
	{
		string _character = _character_order[_day_num];
		string dialogue = $"BeforeGame/{_dialogue_order[_day_num]}";
		_open_door_prompt.Visible = false;
		GetNode<CharacterBody2D>(_character).Visible = true;
		GetNode<CharacterBody2D>(_character).CollisionLayer = 1;
		_player.Position = new Vector2(105, 77);

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

	private async Task InSafeHouse_Dialogue_setupAsync(string dialogue_path, int character_num)
	{
		GetNode<PlayerCharacter>("PlayerCharacter")._set_movable(false);

		await DialogueManager.Instance.StartDialogue(dialogue_path, false);

		_dialogue_exhausted[character_num] = true;

		GetNode<PlayerCharacter>("PlayerCharacter")._set_movable(true);
	}

	private async Task Start_dayAsync()
	{
		if (!_day_over)
		{
			window_day.Visible = true;
			window_night.Visible = false;
			night_safehouse_lighting.Visible = false;
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
			window_day.Visible = false;
			window_night.Visible = true;
			night_safehouse_lighting.Visible = true;
			switch (_day_num)
			{
				case 0:
					await DialogueManager.Instance.StartDialogue("EndDay/sleep_prompt_0", false);
					break;
				case 1:
					await DialogueManager.Instance.StartDialogue("EndDay/sleep_prompt_1", false);
					break;
				case 2:
					await DialogueManager.Instance.StartDialogue("EndDay/sleep_prompt_2", false);
					break;
				case 3:
					await DialogueManager.Instance.StartDialogue("EndDay/sleep_prompt_3", false);
					break;
				case 4:
					await DialogueManager.Instance.StartDialogue("EndDay/sleep_prompt_4", false);
					break;
			}
		}
	}
}
