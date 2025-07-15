using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Agent4 : Agent {
	private List<Card> hand;
	private List<Card> playerTable;
	private List<Card> enemyTable;
	
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
			"defense",
			"defense",
			"defense"
		};
		
		return (types, classes);
	}
	
	public override (List<string>, List<string>) GetTableCards() {
		List<string> types = new List<string> {
			"tsunami",
			"volcano",
			"earthquake",
			"tsunami",
			"volcano",
			"earthquake",
		};
		
		List<string> classes = new List<string> {
			"defense",
			"defense",
			"basic",
			"basic",
			"basic",
			"basic"
		};
		
		return (types, classes);
	}
	
	// probability of each table card being each type, shape 6x3
	private List<List<double>> ranks;
	// range for table indices to be sorted, shape 3x6
	private List<List<int>> orders;
	
	// probability modifier when failing a throw from a certain type
	// rankMod[x] = the probability of the card being each of [light, regular, heavy] type after
	//              failing a throw with card of type x
	private double[][] rankMod;
	
	// frequency of enemy basic hand cards by type
	private int[] freq;
	
	// frequency of unflipped player table cards by type
	private int[] tableFreq;
	
	private int round;
	private int order;
	private int last;
	
	public override void Init(List<Card> hand, List<Card> playerTable, List<Card> enemyTable) {
		this.hand = hand;
		this.playerTable = playerTable;
		this.enemyTable = enemyTable;
		
		freq = new int[] {2, 2, 2};
		tableFreq = new int[] {2, 2, 2};
		round = 0;
		order = -1;
		last = -1;
		
		// start with equal chance of unflipped cards being each type
		ranks = Enumerable.Range(0, 6)
			.Select(_ => new List<double> {1.0 / 3, 1.0 / 3, 1.0 / 3})
			.ToList();
		
		orders = Enumerable.Range(0, 3)
			.Select(_ => Enumerable.Range(0, 6).ToList())
			.ToList();
		
		var typeProb = GlobalState.Instance.FlipProb;
		
		double h = (1 - typeProb[0]) / 3;
		double m = (1 - typeProb[1]) / 3;
		double l = (1 - typeProb[2]) / 3;

		rankMod = new double[][] {
			new double[] { l, m, h },
			new double[] { h, l, m },
			new double[] { m, h, l }
		};
	}
	
	private void SortOrders() {
		for (int i = 0; i < orders.Count; i++) {
			orders[i].Sort((x, y) => {
				if (playerTable[x].visible && !playerTable[y].visible) return 1;
				if (!playerTable[x].visible && playerTable[y].visible) return -1;
				return ranks[y][i].CompareTo(ranks[x][i]);
			});
		}
	}
	
	private void SortHand() {
		hand.Sort((x, y) => {
			int xType = GlobalState.Instance.TypeMap[x.type];
			double xRank = ranks[orders[xType][0]][xType];
			int yType = GlobalState.Instance.TypeMap[y.type];
			double yRank = ranks[orders[yType][0]][yType];
			
			if (xRank != yRank) {
				return xRank.CompareTo(yRank);
			}
			
			return freq[xType] - freq[yType];
		});
	}
	
	public override (Card, Card, Card) Move() {
		if (round >= 3) {
			SortOrders();
			SortHand();
			
			last = GlobalState.Instance.TypeMap[hand[^1].type];
			freq[last]--;
			order = orders[last][0];
			return (hand[^1], playerTable[order], null);
		} else {
			orders[0].Sort((x, y) => {
				if (enemyTable[x].visible && !enemyTable[y].visible) return 1;
				if (!enemyTable[x].visible && enemyTable[y].visible) return -1;
				return enemyTable[y].durability.CompareTo(enemyTable[x].durability);
			});
			
			order = orders[0][0];
			
			return (hand[^1], enemyTable[orders[0][0]], null);
		}
	}
	
	public override void Backward() {
		if (round >= 3) {
			// update part of rank from using frequency of flipped player table cards
			int sum = tableFreq.Sum();
			if (sum == 0) return;
			
			double[] oldProb = Enumerable.Range(0, 3)
				.Select(i => tableFreq[(i + 2) % 3] / (double)sum)
				.ToArray();
				
			tableFreq = Enumerable.Range(0, 3)
				.Select(type => playerTable.Count(card =>
					!card.visible && GlobalState.Instance.TypeMap[card.type] == type
				)).ToArray();
			sum = tableFreq.Sum();
			if (sum == 0) return;
				
			double[] newProb = Enumerable.Range(0, 3)
				.Select(i => tableFreq[(i + 2) % 3] / (double)sum)
				.ToArray();

			ranks = ranks.Select(row => 
				Enumerable.Range(0, 3).Select(x =>
					oldProb[x] != 0 ? (row[x] / oldProb[x]) * newProb[x] : 0
				).ToList()
			).ToList();
			
			// prioritize unflipped table cards
			for (int i = 0; i < playerTable.Count; i++) {
				if (playerTable[i].visible) {
					ranks[i] = new List<double> {-1e5, -1e5, -1e5};
				}
			}
			
			// apply rank modifier if last throw failed
			if (!playerTable[order].visible) {
				for (int i = 0; i < freq.Length; i++) {
					ranks[order][i] *= rankMod[last][i];
				}
			}
		}
		
		round++;
	}
}
