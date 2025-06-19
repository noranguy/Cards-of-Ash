using Godot;
using System;
using System.Collections.Generic;

public partial class CardContainer : Node2D {
	[Signal]
	public delegate void ActiveCardEventHandler(Card newCard);

	public PackedScene cardScene;
	public Card activeCard = null;
	protected float spacing = 10f;
	protected float cardWidth = 50f;
	
	public virtual int[] GenerateTypeOrder(int n) {
		int[] types = new int[n];
		
		for (int i = 0; i < n; i++) {
			types[i] = i % 3;
		}
		
		Random rand = new Random();
		for (int i = n-1; i > 0; i--) {
			int j = rand.Next(i+1);
			(types[i], types[j]) = (types[j], types[i]);
		}
		
		return types;
	}
	
	public virtual void UpdateCardPositions(List<Card> cards, float startX, float y, float width) {
		for (int i = 0; i < cards.Count; i++) {
			float x = startX + i * (cardWidth + spacing);
			cards[i].Position = new Vector2(x, y);
		}
	}
	
	public virtual void OnCardClicked(Card card) {
		if (activeCard == card) {
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
