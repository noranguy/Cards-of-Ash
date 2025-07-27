using Godot;
using System;
using System.Threading.Tasks;

public partial class Task3 : Node2D
{
	private bool fak_found;

	private bool near_cans;
	private bool near_water;
	private bool near_fak;
	private bool near_garbo;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		fak_found = false;
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

	public void checkStars()
	{
		GD.Print(near_cans);
		if (near_cans)
		{
			//Play dialogue -- didnt find the thing
			GetNode<Area2D>("Cans").Visible = false;
			GetNode<AnimationPlayer>("Cans/CansBlink").Stop();
			_ = InSafeHouse_Dialogue_setupAsync("cans");
		}

		else if (near_water)
		{
			// Play dialogue
			GetNode<Area2D>("Water").Visible = false;
			GetNode<AnimationPlayer>("Water/WaterBlink").Stop();
			_ = InSafeHouse_Dialogue_setupAsync("water");
		}

		else if (near_fak)
		{
			// Play dialogue
			GetNode<Area2D>("FirstAidKit").Visible = false;
			GetNode<AnimationPlayer>("FirstAidKit/FirstAidKitBlink").Stop();
			_ = InSafeHouse_Dialogue_setupAsync("fak");
			fak_found = true;
		}

		else if (near_garbo)
		{
			// Play dialogue
			GetNode<Area2D>("Garbage").Visible = false;
			GetNode<AnimationPlayer>("Garbage/GarbageBlink").Stop();
			_ = InSafeHouse_Dialogue_setupAsync("garbage");
		}
	}

	public bool isGameWon()
	{
		return fak_found;
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

	private async Task InSafeHouse_Dialogue_setupAsync(string item)
	{
		await DialogueManager.Instance.StartDialogue($"Tasks/Task3/{item}_found", false);
	}
}
