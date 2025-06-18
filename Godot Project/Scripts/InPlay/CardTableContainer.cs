using Godot;
using System;
using System.Collections.Generic;

public partial class CardTableContainer : CardContainer
{
	public override void _Ready()
	{
		SpawnInitialCards(numCards, false, 100, false);
	}
}
