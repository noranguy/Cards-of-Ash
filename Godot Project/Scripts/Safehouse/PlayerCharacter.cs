using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class PlayerCharacter : CharacterBody2D
{
	// Flag to see if the player should be able to move, false when a prompt is on screen

	bool _moveable = false;

	// Player speed
	public const float Speed = 80;
	public RayCast2D _ray; // Ignore ray for now

	public override void _Ready()
	{
		_ray = GetNode<RayCast2D>("RayCast2D");
	}

	public override void _Process(double delta)
	{

	}

	// Player character movement
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		if (_moveable && direction.X != 0)
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

		else if (_moveable && direction.Y != 0)
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

	// setter for the players mobility
	public void _set_movable(bool can_move)
	{
		_moveable = can_move;
	}
}
