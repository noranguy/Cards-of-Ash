using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class PlayerCharacter : CharacterBody2D
{
	// Flag to see if the player should be able to move, false when a prompt is on screen
	
	[Export]
	bool _moveable = false;

	// Player speed
	public const float Speed = 70;
	public RayCast2D _ray; // Ignore ray for now
	AnimatedSprite2D walking;

	public override void _Ready()
	{
		walking = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
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

		if (_moveable && (direction.X != 0 || direction.Y != 0))
		{
			//velocity.Y = 0;
			velocity.X = direction.X * Speed;
			GetNode<Sprite2D>("Sprite2D").Visible = false;
			if (direction.X > 0 && direction.Y == 0)
			{
				walking.FlipH = false;
				walking.Play("side");
				_ray.RotationDegrees = 270;
			}

			if (direction.X < 0 && direction.Y == 0)
			{
				walking.FlipH = true;
				walking.Play("side");
				_ray.RotationDegrees = 90;

			}

			velocity.Y = direction.Y * Speed;

			if (direction.Y > 0 && direction.X == 0)
			{
				_ray.RotationDegrees = 0;
				walking.Play("fwrd");
			}

			if (direction.Y < 0 && direction.X == 0)
			{
				_ray.RotationDegrees = 180;
				walking.Play("back");
			}
			if (direction.Y > 0 && direction.X > 0)
			{
				walking.FlipH = false;
				walking.Play("diag_down");
			}
			if (direction.Y < 0 && direction.X > 0)
			{
				walking.FlipH = false;
				walking.Play("diag_up");
			}
			if (direction.Y > 0 && direction.X < 0)
			{
				walking.FlipH = true;
				walking.Play("diag_down");
			}
			if (direction.Y < 0 && direction.X < 0)
			{
				walking.FlipH = true;
				walking.Play("diag_up");
			}
		}

		else
		{
			velocity.X = 0;
			velocity.Y = 0;
			walking.Pause();
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
