using Godot;
using System;
using System.Collections.Generic;

public abstract class Agent {
	public abstract void Init(List<Card> hand, List<Card> playerTable, List<Card> enemyTable);
	
	public abstract (List<string>, List<string>) GetHandCards();
	
	public abstract (List<string>, List<string>) GetTableCards();
	
	// returns the [throwingCard, tableCard1, tableCard2] move that the agent wants to make
	public abstract (Card, Card, Card) Move();
	
	// reveal a card to the agent
	public virtual void RevealCard(Card card, int idx) { return; }
	
	// post-move processing
	public abstract void Backward();
}
