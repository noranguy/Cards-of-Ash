using Godot;
using System;
using System.Collections.Generic;

public abstract partial class CardContainer : Node2D {
	[Signal]
	public delegate void ActiveCardEventHandler(Card newCard);

	[Export] public PackedScene cardScene;
	public Card activeCard = null;
	protected List<Card> cards = new();
	protected float spacing = 10f;
	protected float cardWidth = 50f;
	public readonly int numCards = 6;
	
	public virtual void SpawnInitialCards(int count, bool visible, float y, bool isHand) {
		string[] types = {"light", "regular", "heavy"};
		int[] typeOrder = GenerateTypeOrder(count);
		for (int i = 0; i < count; i++) {
			Card card = cardScene.Instantiate<Card>();
			card.Name = $"Card{i}";
			card.Position = Vector2.Zero;
			card.Init(types[typeOrder[i]], visible, isHand);
			card.Connect(Card.SignalName.CardClicked, new Callable(this, nameof(OnCardClicked)));

			AddChild(card);
			cards.Add(card);
		}

		float totalWidth = cards.Count * cardWidth + (cards.Count - 1) * spacing;
		float startX = (cardWidth - totalWidth) / 2f;
		UpdateCardPositions(startX, y, totalWidth);
	}
	
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
	
	public virtual void UpdateCardPositions(float startX, float y, float width) {
		for (int i = 0; i < cards.Count; i++) {
			float x = startX + i * (cardWidth + spacing);
			cards[i].Position = new Vector2(x, y);
		}
	}
	
	public virtual void OnCardClicked(Card card) {
		if (card.visible != card.isHand || activeCard == card) {
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
	
	public List<Card> GetCards() {
		return cards;
	}
}
