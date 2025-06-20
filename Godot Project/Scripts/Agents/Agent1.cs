using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Agent1 : Agent {
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
			"ceramic",
			"ceramic",
			"ceramic"
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
			"ceramic",
			"ceramic",
			"basic",
			"basic",
			"basic",
			"basic"
		};
		
		return (types, classes);
	}
	
	private List<List<double>> ranks;
	private List<List<int>> orders;
	private double[][] rankMod;
	private double[][] adjRankMod;
	private int[] freq;
	private int[] tableFreq;
	
	private int round;
	private int order;
	private int last;
	
	public override void Init(List<Card> hand, List<Card> playerTable, List<Card> enemyTable) {
		freq = new int[] {2, 2, 2};
		tableFreq = new int[] {2, 2, 2};
		round = 0;
		order = -1;
		last = -1;
		
		this.hand = hand;
		this.playerTable = playerTable;
		this.enemyTable = enemyTable;
		
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
		
		h = (1 - typeProb[0]) * 0.25f / 3;
		m = (1 - typeProb[1]) * 0.25f / 3;
		l = (1 - typeProb[2]) * 0.25f / 3;

		adjRankMod = new double[][] {
			new double[] { l, m, h },
			new double[] { h, l, m },
			new double[] { m, h, l }
		};
	}
	
	private double GetRank(int type, string clas, int i) {
		double center = ranks[i][type];
		double left = (i == 0 || clas != "ceramic") ? 0 : GlobalState.Instance.CeramicProb * ranks[i - 1][type];
		double right = (i == 5 || clas != "ceramic") ? 0 : GlobalState.Instance.CeramicProb * ranks[i + 1][type];
		return center + left + right;
	}
	
	private void SortOrders(string clas) {
		for (int i = 0; i < orders.Count; i++) {
			var row = orders[i];
			
			row.Sort((x, y) => {
				if (enemyTable[x].visible && !enemyTable[y].visible) return 1;
				if (!enemyTable[x].visible && enemyTable[y].visible) return -1;
				
				return GetRank(i, clas, y).CompareTo(GetRank(i, clas, x));
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
	
	public override (Card, Card) Move() {
		Card card;
		switch (round) {
			case 0:
				last = GlobalState.Instance.TypeMap[hand[^1].type];
				order = 1;
				return (hand[^1], enemyTable[1]);
			case 1:
				last = GlobalState.Instance.TypeMap[hand[^1].type];
				order = 4;
				return (hand[^1], enemyTable[4]);
			case 2:
				SortOrders("ceramic");
				last = GlobalState.Instance.TypeMap[hand[^1].type];
				order = orders[last][0];
				return (hand[^1], enemyTable[order]);
			default:
				SortOrders("basic");
				SortHand();
				
				last = GlobalState.Instance.TypeMap[hand[^1].type];
				freq[last]--;
				order = orders[last][0];
				return (hand[^1], enemyTable[order]);
		}
	}
	
	public override void Backward() {
		int sum = tableFreq.Sum();
		
		double[] oldProb = Enumerable.Range(0, 3)
			.Select(i => tableFreq[(i + 2) % 3] / (6.0 - sum))
			.ToArray();
			
		tableFreq = Enumerable.Range(0, 3)
			.Select(type => enemyTable.Count(card => !card.visible && GlobalState.Instance.TypeMap[card.type] == type))
			.ToArray();
		sum = tableFreq.Sum();
		if (sum == 0) return;
			
		double[] newProb = Enumerable.Range(0, 3)
			.Select(i => tableFreq[(i + 2) % 3] / (6.0 - sum))
			.ToArray();
			
		ranks = ranks.Select(row => 
			Enumerable.Range(0, 3).Select(x =>
				oldProb[x] != 0 ? (row[x] / oldProb[x]) * newProb[x] : 0
			).ToList()
		).ToList();
		
		for (int i = 0; i < enemyTable.Count; i++) {
			if (enemyTable[i].visible) {
				ranks[i] = new List<double> {-1e5, -1e5, -1e5};
			}
		}
		
		if (!enemyTable[order].visible) {
			for (int i = 0; i < freq.Length; i++) {
				ranks[order][i] *= rankMod[last][i];
			}
		}
		
		if (order > 0 && round < 3 && !enemyTable[order-1].visible) {
			for (int i = 0; i < freq.Length; i++) {
				ranks[order-1][i] *= adjRankMod[last][i];
			}
		}
		
		if (order < 5 && round < 3 && !enemyTable[order+1].visible) {
			for (int i = 0; i < freq.Length; i++) {
				ranks[order+1][i] *= adjRankMod[last][i];
			}
		}
		
		round++;
	}
}
