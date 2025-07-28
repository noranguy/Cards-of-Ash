using Godot;
using System;
using System.Reflection;
using System.Threading.Tasks;

public partial class Task1 : Node2D
{
	bool over_phone;
	bool over_radio;
	bool in_radio_game;
	bool in_phone_game;

	bool radio_fixed;
	bool phone_fixed;

	float channel;

	float goal_channel = 810f;

	RadioGame radio_game;
	PhoneGame phone_game;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		radio_fixed = false;
		phone_fixed = false;

		over_phone = false;
		over_radio = false;

		in_radio_game = false;
		in_phone_game = false;

		radio_game = GetNode<RadioGame>("RadioGame");
		phone_game = GetNode<PhoneGame>("PhoneGame");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("click"))
		{
			if (over_radio && !in_phone_game)
			{
				GetNode<Control>("RadioGame").Visible = true;
				in_radio_game = true;
			}
		}

		if (Input.IsActionJustPressed("click"))
		{
			if (over_phone && !in_radio_game)
			{
				GetNode<Control>("PhoneGame").Visible = true;
				in_phone_game = true;
			}
		}

		if (in_radio_game)
		{
			radio_fixed = radio_game.PlayGame(goal_channel);
		}

		if (in_phone_game)
		{
			phone_fixed = phone_game.PlayGame();
		}

		if (!in_phone_game && !in_radio_game)
		{
			checkCompleted();
		}
	}

	private void _on_radio_mouse_entered()
	{
		over_radio = true;
	}

	private void _on_radio_mouse_exited()
	{
		over_radio = false;
	}

	private void _on_phone_mouse_entered()
	{
		over_phone = true;
	}

	private void _on_phone_mouse_exited()
	{
		over_phone = false;
	}

	private void _on_radio_exit_pressed()
	{
		GetNode<Control>("RadioGame").Visible = false;
		GetNode<AudioStreamPlayer>("RadioGame/RadioSound").Stop();
        GetNode<AudioStreamPlayer>("RadioGame/StaticSound").Stop();
		in_radio_game = false;
	}

	private void _on_phone_exit_pressed()
	{
		GetNode<Control>("PhoneGame").Visible = false;
		in_phone_game = false;
	}

	private float RangeLerp(float val, float min1, float max1, float min2, float max2)
	{
		float normal = (val - min1) / (max1 - min1);
		return min2 + (max2 - min2) * normal;
	}

	private void checkCompleted()
	{
		if (radio_fixed && phone_fixed)
		{
			GlobalState.Instance.MissionCompleted(1);
			GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("safehouse.tscn");
		}
	}

	private async Task playDialogueAsync()
	{
		await DialogueManager.Instance.StartDialogue("InTask/Task1/completed", false);
	}
}
