using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Agent0 : Agent {
	private Random rand;
	private List<int> unflippedCards;
	
	public override void Init() {
		rand = new Random();	
		unflippedCards = Enumerable.Range(0, 6).ToList();
	}
	
	public override (Card, int) Move(List<Card> hand) {
		Card throwingCard = hand[rand.Next(hand.Count)];
		int tableCardIdx = unflippedCards[rand.Next(unflippedCards.Count)];
		GD.Print($"{throwingCard.type} {tableCardIdx}");
		return (throwingCard, tableCardIdx);
	}
	
	public override void Backward(List<int> indices, List<int> types) {
		if (indices.Count > 0) {
			unflippedCards.Remove(indices[0]);
		}
	}
}
