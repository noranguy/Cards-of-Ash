using Godot;
using System;
using System.Collections.Generic;

public abstract class Agent {
	public abstract void Init(List<Card> hand, List<Card> playerTable, List<Card> enemyTable);
	
	public abstract (List<string>, List<string>) GetHandCards();
	
	public abstract (List<string>, List<string>) GetTableCards();
	
	// returns the [throwingCard, tableCard] move that the agent wants to make
	public abstract (Card, Card) Move();
	
	// post-move processing
	public abstract void Backward();
}
