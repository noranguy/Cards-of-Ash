using Godot;
using System;
using System.Collections.Generic;

public partial class CardTableContainer : CardContainer {
	protected List<Card> playerCards = new();
	protected List<Card> enemyCards = new();
	public readonly int numCards = 6;
	
	public void Init(PackedScene scene, float y) {
		cardScene = scene;
		SpawnInitialCards(playerCards, numCards, y, true);
		SpawnInitialCards(enemyCards, numCards, y-75, false);
	}
	
	public virtual void SpawnInitialCards(List<Card> cards, int count, float y, bool isPlayer) {
		string[] types = {"light", "regular", "heavy"};
		int[] typeOrder = GenerateTypeOrder(count);
		for (int i = 0; i < count; i++) {
			Card card = cardScene.Instantiate<Card>();
			card.Name = $"Card{i}";
			card.Position = Vector2.Zero;
			card.Init(types[typeOrder[i]], false, false, isPlayer);
			card.Connect(Card.SignalName.CardClicked, new Callable(this, nameof(OnCardClicked)));

			AddChild(card);
			cards.Add(card);
		}

		float totalWidth = cards.Count * cardWidth + (cards.Count - 1) * spacing;
		float startX = (cardWidth - totalWidth) / 2f;
		UpdateCardPositions(cards, startX, y, totalWidth);
	}
	
	public List<Card> GetPlayerCards() {
		return playerCards;
	}
	
	public List<Card> GetEnemyCards() {
		return enemyCards;
	}
}
