using Godot;
using System;
using System.Collections.Generic;

public partial class CardContainer : Node2D {
	[Signal]
	public delegate void ActiveCardEventHandler(Card newCard);

	public PackedScene cardScene;
	public Card activeCard = null;
	public bool allowActive = true;
	protected float spacing = 5f;
	protected float cardWidth = 25f;
	
	public virtual void UpdateCardPositions(List<Card> cards, float startX, float y, float width) {
		for (int i = 0; i < cards.Count; i++) {
			float x = startX + i * (cardWidth + spacing);
			cards[i].Position = new Vector2(x, y);
		}
	}
	
	public virtual void OnCardClicked(Card card) {
		if (activeCard == card || !allowActive) {
			return;
		}
		if (activeCard != null) {
			activeCard.locked = false;
			activeCard.Unhighlight();
		}
		card.locked = true;
		card.Highlight();
		activeCard = card;
		
		EmitSignal(SignalName.ActiveCard, card);
	}
}
