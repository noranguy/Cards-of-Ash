using Godot;
using System;
using System.Collections.Generic;

public partial class CardHandContainer : CardContainer {
	private float handY = 175f;

	public override void _Ready() {
		SpawnInitialCards(numCards, true, handY, true);
	}

	public void RemoveCard(Card card) {
		if (cards.Contains(card)) {
			cards.Remove(card);
			card.QueueFree();
			float totalWidth = cards.Count * cardWidth + (cards.Count - 1) * spacing;
			float startX = (cardWidth - totalWidth) / 2f;
			UpdateCardPositions(startX, handY, totalWidth);
		}
	}
}
