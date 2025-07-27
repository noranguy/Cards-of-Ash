using Godot;
using System;
using System.Threading.Tasks;

public partial class Task3 : Node2D
{
	private bool fak_found;
	private bool cans_found;
	private bool water_found;
	private bool garbo_found;


	private bool near_cans;
	private bool near_water;
	private bool near_fak;
	private bool near_garbo;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		fak_found = false;
		cans_found = false;
		water_found = false;
		garbo_found = false;

		near_cans = false;
		near_water = false;
		near_fak = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void playGame()
	{
		GetNode<Area2D>("Cans").Visible = true;
		GetNode<Area2D>("Water").Visible = true;
		GetNode<Area2D>("FirstAidKit").Visible = true;
		GetNode<Area2D>("Garbage").Visible = true;

		GetNode<Area2D>("Cans").CollisionLayer = 3;
		GetNode<Area2D>("Water").CollisionLayer = 3;
		GetNode<Area2D>("FirstAidKit").CollisionLayer = 3;
		GetNode<Area2D>("Garbage").CollisionLayer = 3;

		GetNode<AnimationPlayer>("Cans/CansBlink").Play("blink");
		GetNode<AnimationPlayer>("Water/WaterBlink").Play("blink");
		GetNode<AnimationPlayer>("FirstAidKit/FirstAidKitBlink").Play("blink");
		GetNode<AnimationPlayer>("Garbage/GarbageBlink").Play("blink");
	}

	public string checkStars()
	{
		if (near_cans && !cans_found)
		{
			//Play dialogue -- didnt find the thing
			GetNode<Area2D>("Cans").Visible = false;
			GetNode<Area2D>("Cans").CollisionLayer = 0;
			GetNode<AnimationPlayer>("Cans/CansBlink").Stop();
			cans_found = true;
			return "cans";
		}

		else if (near_water && !water_found)
		{
			// Play dialogue
			GetNode<Area2D>("Water").Visible = false;
			GetNode<Area2D>("Water").CollisionLayer = 0;
			GetNode<AnimationPlayer>("Water/WaterBlink").Stop();
			water_found = true;
			return "water";
		}

		else if (near_fak && !fak_found)
		{
			// Play dialogue
			GetNode<Area2D>("FirstAidKit").Visible = false;
			GetNode<AnimationPlayer>("FirstAidKit/FirstAidKitBlink").Stop();
			GetNode<Area2D>("FirstAidKit").CollisionLayer = 0;
			fak_found = true;
			return "fak";
		}

		else if (near_garbo && !garbo_found)
		{
			// Play dialogue
			GetNode<Area2D>("Garbage").Visible = false;
			GetNode<Area2D>("Garbage").CollisionLayer = 0;
			GetNode<AnimationPlayer>("Garbage/GarbageBlink").Stop();
			garbo_found = true;
			return "garbage";
		}

		return null;
	}

	private void _on_cans_body_entered(Node2D body)
	{
		near_cans = true;
	}


	private void _on_cans_body_exited(Node2D body)
	{
		near_cans = false;
	}

	private void _on_water_body_entered(Node2D body)
	{
		near_water = true;
	}

	private void _on_water_body_exited(Node2D body)
	{
		near_water = false;
	}

	private void _on_first_aid_kit_body_entered(Node2D body)
	{
		near_fak = true;
	}

	private void _on_first_aid_kit_body_exited(Node2D body)
	{
		near_fak = false;
	}

	private void _on_garbage_body_entered(Node2D body)
	{
		near_garbo = true;
	}

	private void _on_garbage_body_exited(Node2D body)
	{
		near_garbo = false;
	}
}
