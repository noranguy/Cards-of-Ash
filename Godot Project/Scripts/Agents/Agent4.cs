using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Agent4 : Agent {
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
	
	// frequency of unflipped player table cards by type
	//private int[] tableFreq;
	
	private Card[] knownCards;
	//private int[] assumedTypes;
	
	private int round;
	
	public override void Init(List<Card> hand, List<Card> playerTable, List<Card> enemyTable) {
		//tableFreq = new int[] {2, 2, 2};
		round = 0;
		
		this.hand = hand;
		this.playerTable = playerTable;
		this.enemyTable = enemyTable;
		
		order = Enumerable.Range(0, 6).ToList();
		knownCards = Enumerable.Repeat((Card)null, 6).ToArray();
		
		//assumedTypes = Enumerable.Repeat(-1, 6).ToArray();
	}
	
	public override void RevealCard(Card card, int idx) {
		knownCards[idx] = card;
	}
	
	/*
	private void FixAssumption(int type) {
		int idx = Enumerable.Range(0, 6)
			.FirstOrDefault(i >= knownCards[i] == null && assumedTypes[i] = type);
		
		int prevType = (type + 2) % 3;
		int nextType = (type + 1) % 3;
		
		if (tableFreq[prevType] == 0) {
			int knownPrevTypeCount = knownCards.Count(card =>
				card != null && GlobalState.Instance.TypeMap[card.type] == prevType);
			
			if (knownPrevTypeCount == 2) {
				assumedTypes[idx] = nextType;
				tableFreq[type] += 1;
				tableFreq[nextType] -= 1;
			} else {
				int idx2 = Enumerable.Range(0, 6)
					.FirstOrDefault(i => knownCards[i] == null && assumedTypes[i] == prevType);
				
				assumedTypes[idx2] = nextType;
				assumedTypes[idx] = prevType;
				tableFreq[type] += 1;
				tableFreq[prevType] -= 1;
			}
		} else {
			assumedTypes[idx] = prevType;
			tableFreq[type] += 1;
			tableFreq[prevType] -= 1;
		}
	}
	*/
	
	public override (Card, Card, Card) Move() {
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
		round++;
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
	}
}
