using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CardTableContainer : CardContainer {
	public readonly int numCards = 6;
	
	private List<Card> playerCards;
	private List<Card> enemyCards;
	private Random rand;
	
	public void Init(
		PackedScene scene, List<string> playerTypes,
		List<string> playerClasses,
		List<string> enemyTypes, List<String> enemyClasses,
		float playerY, float enemyY
	) {
		rand = new Random();
		cardScene = scene;
		enemyCards = SpawnCards(playerTypes, playerClasses, enemyY, false);
		playerCards = SpawnCards(enemyTypes, enemyClasses, playerY, true);
	}
	
	private List<int> GenRandomOrder() {
		List<int> order = Enumerable.Range(0, numCards).ToList();
		
		for (int i = numCards-1; i > 0; i--) {
			int j = rand.Next(i+1);
			(order[i], order[j]) = (order[j], order[i]);
		}
		
		return order;
	}
	
	public virtual List<Card> SpawnCards(
		List<string> types, List<string> classes,
		float y, bool isPlayer
	) {
		List<int> order = GenRandomOrder();
		List<Card> cards = new List<Card>();
		for (int i = 0; i < numCards; i++) {
			Card card = cardScene.Instantiate<Card>();
			card.Name = $"Card{i}";
			card.Position = Vector2.Zero;
			card.Init(types[order[i]], classes[order[i]], false, isPlayer, i);
			card.Connect(Card.SignalName.CardClicked, new Callable(this, nameof(OnCardClicked)));

			AddChild(card);
			cards.Add(card);
		}

		float totalWidth = numCards * cardWidth + (numCards - 1) * spacing;
		float startX = (cardWidth - totalWidth) / 2f;
		UpdateCardPositions(cards, startX, y, totalWidth);
		return cards;
	}
	
	public List<Card> GetPlayerCards() {
		return playerCards;
	}
	
	public List<Card> GetEnemyCards() {
		return enemyCards;
	}
}
