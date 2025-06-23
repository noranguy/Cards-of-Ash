using Godot;
using System;

public partial class InPlay : Node2D
{
	[Export] public PackedScene cardScene;
	private  int day;


	public AnimatedSprite2D opp;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		opp = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		day = GlobalState.Instance.GetDay();

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (day == 0)
		{
			opp.Play("old_man_idle");

		}
		else if (day == 1)
		{
			opp.Play("kaishain_idle");
		}
	}
}
