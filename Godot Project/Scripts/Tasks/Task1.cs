using Godot;
using System;
using System.Reflection;

public partial class Task1 : Node2D
{
	bool over_phone;
	bool over_radio;
	bool in_radio_game;
	bool in_phone_game;

	bool radio_fixed;
	bool phone_fixed;

	float channel;

	float goal_channel = 93.1f;

	RadioGame radio_game;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		radio_fixed = false;
		phone_fixed = false;

		over_phone = false;
		over_radio = false;

		in_radio_game = false;
		in_phone_game = false;

		channel = 85.6f;

		radio_game = GetNode<RadioGame>("RadioGame");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (Input.IsActionJustPressed("click"))
		{
			if (over_radio)
			{
				GetNode<Control>("RadioGame").Visible = true;
				in_radio_game = true;
			}
		}

		if (in_radio_game)
		{
			channel = radio_game.PlayGame(goal_channel);
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

	private void _on_exit_pressed()
	{
		GetNode<Control>("RadioGame").Visible = false;
		in_radio_game = false;
	}

	private float RangeLerp(float val, float min1, float max1, float min2, float max2)
	{
		float normal = (val - min1) / (max1 - min1);
		return (min2 + (max2 - min2) * normal);
	}
}
