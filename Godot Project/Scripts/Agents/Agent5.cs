using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Agent5 : Agent {
	private Random Rand = new Random();
	
	private List<Card> hand;
	private List<Card> playerTable;
	private List<Card> enemyTable;
	private int[] handFreq;
	
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
			("earthquake", "basic"),
			("tsunami", "basic"),
			("volcano", "basic"),
			("earthquake", "basic")
		};
	}
	
	public override void Init(List<Card> hand, List<Card> playerTable, List<Card> enemyTable) {
		this.hand = hand;
		this.playerTable = playerTable;
		this.enemyTable = enemyTable;
		
		handFreq = new int[] {3, 3, 3};
	}
	
	public override (Card, Card, Card) Move(bool blinded) {
		if (blinded) {
			return (hand[Rand.Next(hand.Count)], playerTable[Rand.Next(playerTable.Count)], null);
		}
		
		List<Card> unflippedPlayerTable = playerTable.Where(x => !x.visible).ToList();
		List<Card> flippedEnemyTable = enemyTable.Where(x => x.visible).ToList();
			
		Card tableCard = null;
		Card throwingCard = null;
		
		int highestFreq = handFreq.Max();
		
		foreach (Card card in flippedEnemyTable) {
			if (handFreq[(GlobalState.Instance.TypeMap[card.type] + 1) % 3] == highestFreq) {
				tableCard = card;
				foreach (Card card2 in hand) {
					if ((GlobalState.Instance.TypeMap[card.type] + 1) % 3 == GlobalState.Instance.TypeMap[card2.type]) {
						throwingCard = card2;
						break;
					}
				}
				break;
			}
		}
		
		if (tableCard != null) {
			handFreq[GlobalState.Instance.TypeMap[throwingCard.type]]--;
			return (throwingCard, tableCard, null);
		}
		
		foreach (Card card in flippedEnemyTable) {
			if (handFreq[GlobalState.Instance.TypeMap[card.type]] == highestFreq) {
				tableCard = card;
				foreach (Card card2 in hand) {
					if (card.type == card2.type) {
						throwingCard = card2;
						break;
					}
				}
				break;
			}
		}
		
		if (tableCard != null) {
			handFreq[GlobalState.Instance.TypeMap[throwingCard.type]]--;
			return (throwingCard, tableCard, null);
		}
		
		hand.Sort((x, y) => {
			int xType = GlobalState.Instance.TypeMap[x.type];
			int yType = GlobalState.Instance.TypeMap[y.type];
			
			if (handFreq[(xType + 1) % 3] != handFreq[(yType + 1) % 3]) {
				return handFreq[(xType + 1) % 3] - handFreq[(yType + 1) % 3];
			} else {
				return handFreq[xType] - handFreq[yType];
			}
		});
		
		if (unflippedPlayerTable.Count > 0) {
			tableCard = unflippedPlayerTable[Rand.Next(unflippedPlayerTable.Count)];
		} else {
			tableCard = enemyTable[Rand.Next(enemyTable.Count)];
		}
		
		handFreq[GlobalState.Instance.TypeMap[hand[^1].type]]--;
		return (hand[^1], tableCard, null);
	}
	
	public override void Backward() {}
}
