using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class PlayerCharacter : CharacterBody2D
{
	// Flag to see if the player should be able to move, false when a prompt is on screen
	public bool moveable = true;
	// Player speed
	public const float Speed = 150.0f;
	public RayCast2D _ray; // Ignore ray for now
	// If the player is in an interactable area
	bool _in_area = false;

	public override void _Ready()
	{
		_ray = GetNode<RayCast2D>("RayCast2D");
	}

	public override void _Process(double delta)
	{
		// If the player is trying to interact with something
		if (Input.IsActionJustPressed("interact"))
		{
			GD.Print("Mreow");
			// If they were in an interactable area, then they are now in a prompt screen and shouldnt be able to move
			if (_in_area)
			{
				moveable = false;
			}
		}
	}

	// Player character movement
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		if (moveable && direction.X != 0)
		{
			velocity.Y = 0;
			velocity.X = direction.X * Speed;

			if (direction.X > 0)
			{
				GetNode<Sprite2D>("Sprite2D").FlipH = false;
				_ray.RotationDegrees = 270;
			}

			else
			{
				GetNode<Sprite2D>("Sprite2D").FlipH = true;
				_ray.RotationDegrees = 90;

			}
		}

		else if (moveable && direction.Y != 0)
		{
			velocity.X = 0;
			velocity.Y = direction.Y * Speed;

			if (direction.Y > 0)
			{
				_ray.RotationDegrees = 0;
			}

			else
			{
				_ray.RotationDegrees = 180;
			}
		}

		else
		{
			velocity.X = 0;
			velocity.Y = 0;
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	// Signals from interactable areas
	void _on_bed_body_entered(Node2D body)
	{
		_in_area = true;
	}

	void _on_bed_body_exited(Node2D body)
	{
		_in_area = false;
	}

	void _on_bed_door_entered(Node2D body)
	{
		_in_area = true;
	}

	void _on_bed_door_exited(Node2D body)
	{
		_in_area = false;
	}

	void _on_menko_table_body_entered(Node2D body)
	{
		_in_area = true;
	}

	void _on_menko_table_body_exited(Node2D body)
	{
		_in_area = false;
	}

	// If the players not in a prompt anymore, they can move
	void _on_cancel_pressed()
	{
		moveable = true;
	}
}
