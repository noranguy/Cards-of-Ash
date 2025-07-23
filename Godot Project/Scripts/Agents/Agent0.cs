using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Agent0 : Agent {
	private readonly Random Rand = new Random();
	
	private List<Card> hand;
	private List<Card> playerTable;
	private List<Card> enemyTable;
	private int round;
	
	public override List<(string, string)> GetHandCards() {
		return new List<(string, string)> {
			("tsunami", "basic"),
			("volcano", "basic"),
			("earthquake", "basic"),
			("tsunami", "basic"),
			("volcano", "basic"),
			("earthquake", "basic"),
			("tsunami", "basic"),
			("volcano", "basic"),
			("earthquake", "basic")
		};
	}
	
	public override List<(string, string)> GetTableCards() {
		return new List<(string, string)> {
			("tsunami", "basic"),
			("volcano", "basic"),
			("tsunami", "basic"),
			("earthquake", "basic"),
			("volcano", "basic"),
			("earthquake", "basic")
		};
	}
	
	public override void Init(List<Card> hand, List<Card> playerTable, List<Card> enemyTable) {
		round = 0;
		
		this.hand = hand;
		this.playerTable = playerTable;
		this.enemyTable = enemyTable;
	}
	
	// Mode 1: Targets a random unflipped card on the player's table
	// Mode 2: Targets a random flipped card on the enemy's table
	// Mode 3: Targets a random card on the player's table
	// 
	// Rounds 0-2: Mode 1.
	// Rounds 3-5: Mode 2. If enemy table has no flipped cards, Mode 1.
	// Rounds 6-8: Mode 1. If player table has only flipped cards, Mode 3.
	public override (Card, Card, Card) Move() {
		Card throwingCard = hand[Rand.Next(hand.Count)];
		
		List<Card> unflippedPlayerTable = playerTable.Where(x => !x.visible).ToList();
		List<Card> flippedEnemyTable = enemyTable.Where(x => x.visible).ToList();
			
		Card tableCard;
		
		if (round == 0) {
			tableCard = playerTable[0];
		} else if (round >= 3 && round < 6 && flippedEnemyTable.Count > 0) {
			tableCard = flippedEnemyTable[Rand.Next(flippedEnemyTable.Count)];
		} else if (unflippedPlayerTable.Count > 0) {
			tableCard = unflippedPlayerTable[Rand.Next(unflippedPlayerTable.Count)];
		} else {
			tableCard = enemyTable[Rand.Next(enemyTable.Count)];
		}
		return (throwingCard, tableCard, null);
	}
	
	public override void Backward() {
		round++;
	}
}
