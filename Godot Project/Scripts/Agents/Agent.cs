using Godot;
using System;
using System.Collections.Generic;

public abstract class Agent {
	public abstract void Init(List<Card> hand, List<Card> playerTable, List<Card> enemyTable);
	
	public abstract (List<string>, List<string>) GetHandCards();
	
	public abstract (List<string>, List<string>) GetTableCards();
	
	public abstract (Card, Card) Move();
	
	public abstract void Backward();
}
