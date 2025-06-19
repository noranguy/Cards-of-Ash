using Godot;
using System;
using System.Collections.Generic;

public partial class CardHandContainer : CardContainer {
	private float handY = 0;
	protected List<Card> cards = new();
	public readonly int numCards = 9;
	
	public void Init(PackedScene scene, float y, bool visible) {
		cardScene = scene;
		handY = y;
		SpawnInitialCards(numCards, handY, visible);
	}
	
	public virtual void SpawnInitialCards(int count, float y, bool visible) {
		string[] types = {"light", "regular", "heavy"};
		int[] typeOrder = GenerateTypeOrder(count);
		for (int i = 0; i < count; i++) {
			Card card = cardScene.Instantiate<Card>();
			card.Name = $"Card{i}";
			card.Position = Vector2.Zero;
			card.Init(types[typeOrder[i]], visible, true, true);
			card.Connect(Card.SignalName.CardClicked, new Callable(this, nameof(OnCardClicked)));

			AddChild(card);
			cards.Add(card);
		}

		float totalWidth = cards.Count * cardWidth + (cards.Count - 1) * spacing;
		float startX = (cardWidth - totalWidth) / 2f;
		UpdateCardPositions(cards, startX, y, totalWidth);
	}

	public void RemoveCard(Card card) {
		if (cards.Contains(card)) {
			cards.Remove(card);
			card.QueueFree();
			float totalWidth = cards.Count * cardWidth + (cards.Count - 1) * spacing;
			float startX = (cardWidth - totalWidth) / 2f;
			UpdateCardPositions(cards, startX, handY, totalWidth);
		}
	}
	
	public List<Card> GetCards() {
		return cards;
	}
}
