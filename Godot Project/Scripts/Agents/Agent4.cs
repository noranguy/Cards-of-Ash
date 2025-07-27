using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Agent4 : Agent {
	private Random Rand = new Random();
	
	private List<Card> hand;
	private List<Card> playerTable;
	private List<Card> enemyTable;
	
	public override List<(string, string)> GetHandCards() {
		return new List<(string, string)> {
			("tsunami", "basic"),
			("volcano", "basic"),
			("earthquake", "basic"),
			("tsunami", "basic"),
			("volcano", "basic"),
			("earthquake", "basic"),
			("tsunami", "vision"),
			("volcano", "vision"),
			("earthquake", "vision")
		};
	}
	
	public override List<(string, string)> GetTableCards() {
		return new List<(string, string)> {
			("tsunami", "vision"),
			("volcano", "vision"),
			("earthquake", "basic"),
			("tsunami", "basic"),
			("volcano", "basic"),
			("earthquake", "basic")
		};
	}
	
	// range for table indices to be sorted, shape 3x6
	private List<int> order;
	
	private Card[] knownCards;
	
	private int round;
	
	public override void Init(List<Card> hand, List<Card> playerTable, List<Card> enemyTable) {
		round = 0;
		
		this.hand = hand;
		this.playerTable = playerTable;
		this.enemyTable = enemyTable;
		
		order = Enumerable.Range(0, 6).ToList();
		knownCards = Enumerable.Repeat((Card)null, 6).ToArray();
	}
	
	public override void RevealCard(Card card, int idx) {
		knownCards[idx] = card;
	}
	
	public override (Card, Card, Card) Move(bool blinded) {
		if (blinded) {
			round--;
			return (hand[0], playerTable[Rand.Next(playerTable.Count)], null);
		}
		
		if (round < 3) {
			return (hand[^1], playerTable[round * 2], playerTable[round * 2 + 1]);
		} else {
			return (hand.FirstOrDefault(card =>
				card.type == knownCards[order[round - 3]].type),
				playerTable[order[round - 3]], null
			);
		}
	}
	
	public override void Backward() {
		if (round == 3) {
			order.Sort((x, y) => {
				if (knownCards[x].clas == "basic" && knownCards[y].clas != "basic") {
					return -1;
				}
				if (knownCards[x].clas != "basic" && knownCards[y].clas == "basic") {
					return 1;
				}
				return 0;
			});
		}
		round++;
	}
}
