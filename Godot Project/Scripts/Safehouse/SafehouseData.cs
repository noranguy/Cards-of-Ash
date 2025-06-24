using Godot;
using System;

public partial class SafehouseData : Node
{
	public double _day_num = 0;
	public CharacterBody2D _player;

	public double get_day_num()
	{
		return _day_num;
	}

	public void inc_day_num()
	{
		_day_num += 0.5;
	}
}
