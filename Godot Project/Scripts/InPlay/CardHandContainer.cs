using Godot;
using System;
using System.Collections.Generic;

public partial class CardHandContainer : CardContainer {
	private float handY = 0;
	private List<Card> cards = new();
	public readonly int numCards = 9;
	
	public void Init(PackedScene scene, float y, bool visible, List<string> types, List<string> classes) {
		cardScene = scene;
		handY = y;
		allowActive = visible;
		SpawnCards(handY, visible, types, classes);
	}

	public virtual void SpawnCards(float y, bool visible, List<string> types, List<string> classes) {
		for (int i = 0; i < numCards; i++) {
			Card card = cardScene.Instantiate<Card>();
			card.Name = $"Card{i}";
			card.Position = Vector2.Zero;
			card.Init(types[i], classes[i], visible, visible, -1);
			card.Connect(Card.SignalName.CardClicked, new Callable(this, nameof(OnCardClicked)));

			AddChild(card);
			cards.Add(card);
		}

		float totalWidth = numCards * cardWidth + (numCards - 1) * spacing;
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
