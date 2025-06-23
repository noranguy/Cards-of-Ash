using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Agent0 : Agent {
	private Random rand;
	private List<Card> hand;
	private List<Card> playerTable;
	private List<Card> enemyTable;
	private int round;
	
	public override (List<string>, List<string>) GetHandCards() {
		List<string> types = new List<string> {
			"tsunami",
			"volcano",
			"earthquake",
			"tsunami",
			"volcano",
			"earthquake",
			"tsunami",
			"volcano",
			"earthquake"
		};
		
		List<string> classes = new List<string> {
			"basic",
			"basic",
			"basic",
			"basic",
			"basic",
			"basic",
			"basic",
			"basic",
			"basic"
		};
		
		return (types, classes);
	}
	
	public override (List<string>, List<string>) GetTableCards() {
		List<string> types = new List<string> {
			"volcano",
			"tsunami",
			"earthquake",
			"tsunami",
			"volcano",
			"earthquake",
		};
		
		List<string> classes = new List<string> {
			"basic",
			"basic",
			"basic",
			"basic",
			"basic",
			"basic"
		};
		
		return (types, classes);
	}
	
	public override void Init(List<Card> hand, List<Card> playerTable, List<Card> enemyTable) {
		round = 0;
		rand = new Random();	
		
		this.hand = hand;
		this.playerTable = playerTable;
		this.enemyTable = enemyTable;
	}
	
	public override (Card, Card) Move() {
		Card throwingCard = hand[rand.Next(hand.Count)];
		
		List<Card> unflippedPlayerTable = playerTable.Where(x => !x.visible).ToList();
		List<Card> flippedEnemyTable = enemyTable.Where(x => x.visible).ToList();
			
		Card tableCard;
		
		if (round >= 3 && round < 6 && flippedEnemyTable.Count > 0) {
			tableCard = flippedEnemyTable[rand.Next(flippedEnemyTable.Count)];
		} else if (unflippedPlayerTable.Count > 0) {
			tableCard = unflippedPlayerTable[rand.Next(unflippedPlayerTable.Count)];
		} else {
			tableCard = enemyTable[rand.Next(enemyTable.Count)];
		}
		return (throwingCard, tableCard);
	}
	
	public override void Backward() {
		round++;
	}
}
