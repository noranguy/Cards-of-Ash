using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class DeckBuilder : Control {
	[Export] public PackedScene CardScene;
	HBoxContainer handBench;
	HBoxContainer tableBench;
	TextureButton doneButton;
	HBoxContainer handSlots;
	HBoxContainer tableSlots;
	Label tableLabel;
	Label handLabel;
	
	List<(string, string)> handCards;
	List<(string, string)> tableCards;
	List<(string, string)> handDeck;
	List<(string, string)> tableDeck;
	
	Card[] activeCards = new Card[] {null, null, null, null};

	public override void _Ready() {
		handBench = GetNode<HBoxContainer>("Hand/Bench/BenchCards");
		tableBench = GetNode<HBoxContainer>("Table/Bench/BenchCards");
		doneButton = GetNode<TextureButton>("DoneButton");
		handSlots = GetNode<HBoxContainer>("Hand/HandSlots");
		tableSlots = GetNode<HBoxContainer>("Table/TableSlots");
		tableLabel = GetNode<Label>("TableLabel");
		handLabel = GetNode<Label>("HandLabel");
		
		handDeck = GlobalState.Instance.GetHandDeck();
		tableDeck = GlobalState.Instance.GetTableDeck();
		
		handCards = GlobalState.Instance.GetHandCards();
		tableCards = GlobalState.Instance.GetTableCards();
		
		int numHandCards = GlobalState.Instance.NumHandCards;
		int numTableCards = GlobalState.Instance.NumTableCards;
		
		for (int i = 0; i < numHandCards; i++) {
			var card = CardScene.Instantiate<Card>();
			card.Init(Card.DEFAULT_VERTICES, handCards[i].Item1, handCards[i].Item2, true, true, 0);
			card.ready = true;
			card.Connect(Card.SignalName.CardClicked, new Callable(this, nameof(OnCardClicked)));
			handSlots.GetNode<CenterContainer>($"HandSlot{i+1}").AddChild(card);
		}
		
		for (int i = 0; i < numTableCards; i++) {
			var card = CardScene.Instantiate<Card>();
			card.Init(Card.DEFAULT_VERTICES, tableCards[i].Item1, tableCards[i].Item2, true, true, 2);
			card.ready = true;
			card.Connect(Card.SignalName.CardClicked, new Callable(this, nameof(OnCardClicked)));
			tableSlots.GetNode<CenterContainer>($"TableSlot{i+1}").AddChild(card);
		}
		
		foreach ((string type, string clas) in handDeck) {
			var card = CardScene.Instantiate<Card>();
			card.Init(Card.DEFAULT_VERTICES, type, clas, true, true, 1);
			card.ready = true;
			card.Connect(Card.SignalName.CardClicked, new Callable(this, nameof(OnCardClicked)));
			handBench.AddChild(card);
		}
		
		foreach ((string type, string clas) in tableDeck) {
			var card = CardScene.Instantiate<Card>();
			card.Init(Card.DEFAULT_VERTICES, type, clas, true, true, 3);
			card.ready = true;
			card.Connect(Card.SignalName.CardClicked, new Callable(this, nameof(OnCardClicked)));
			tableBench.AddChild(card);
		}
		
		doneButton.Pressed += Done;
	}
	
	public void Done() {
		handDeck = new List<(string, string)>();
		tableDeck = new List<(string, string)>();
		
		foreach (Node child in handBench.GetChildren()) {
			if (child is Card card) {
				handDeck.Add((card.type, card.clas));
			}
		}
		
		foreach (Node child in tableBench.GetChildren()) {
			if (child is Card card) {
				tableDeck.Add((card.type, card.clas));
			}
		}
		
		GlobalState.Instance.UpdateOmamori(GetNode<OmamoriSelector>("OmamoriSelector").currentName);
		
		GlobalState.Instance.UpdateHandCards(handCards);
		GlobalState.Instance.UpdateTableCards(tableCards);
		GlobalState.Instance.UpdateHandDeck(handDeck);
		GlobalState.Instance.UpdateTableDeck(tableDeck);
		
		GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("menko_game.tscn");
	}
	
	public void CheckValid() {
		doneButton.Disabled = true;
		doneButton.Modulate = new Color(1, 1, 1, 0.4f);
		
		handCards = new();
		tableCards = new();
		int[] typeFreq = new int[3];
		
		foreach (Node child in handSlots.GetChildren()) {
			if (child.GetChildren().Count > 1 && child.GetChildren()[1] is Card card) {
				handCards.Add((card.type, card.clas));
			} else {
				return;
			}
		}
		
		foreach (Node child in tableSlots.GetChildren()) {
			if (child.GetChildren().Count > 1 && child.GetChildren()[1] is Card card) {
				tableCards.Add((card.type, card.clas));
				typeFreq[GlobalState.Instance.TypeMap[card.type]]++;
			} else {
				return;
			}
		}
		
		if (typeFreq.All(x => x == 2)) {
			doneButton.Disabled = false;
			doneButton.Modulate = Colors.White;
		}
	}
	
	public async void OnCardClicked(Card card) {
		int idx = card.index;
		
		if (activeCards[idx] == card) {
			return;
		}
		
		if (activeCards[idx] != null) {
			activeCards[idx].locked = false;
			activeCards[idx].Unhighlight();
		}
		card.locked = true;
		card.Highlight();
		activeCards[idx] = card;
		
		int otherIdx = idx % 2 == 0 ? (idx + 1) : (idx - 1);
		Card otherCard = activeCards[otherIdx];
		if (otherCard != null) {
			activeCards[idx].locked = false;
			activeCards[idx].Unhighlight();
			activeCards[idx] = null;
			activeCards[otherIdx].locked = false;
			activeCards[otherIdx].Unhighlight();
			activeCards[otherIdx] = null;
			
			Vector2 firstPosition = card.GlobalPosition;
			Vector2 secondPosition = otherCard.GlobalPosition;
			
			var firstParent = card.GetParent();
			var secondParent = otherCard.GetParent();
			var rootParent = GetTree().Root.GetNode<Control>("DeckBuilder");
			
			firstParent.RemoveChild(card);
			rootParent.AddChild(card);
			secondParent.RemoveChild(otherCard);
			rootParent.AddChild(otherCard);
			card.GlobalPosition = firstPosition;
			otherCard.GlobalPosition = secondPosition;
			
			await card.SwapPositions(otherCard);
			SwapCardFields(card, otherCard);
			
			rootParent.RemoveChild(card);
			firstParent.AddChild(card);
			rootParent.RemoveChild(otherCard);
			secondParent.AddChild(otherCard);
			
			card.GlobalPosition = firstPosition;
			otherCard.GlobalPosition = secondPosition;
		}
	}
	
	private void SwapCardFields(Card a, Card b) {
		(a.type, b.type) = (b.type, a.type);
		(a.clas, b.clas) = (b.clas, a.clas);
		(a.sprite.Texture, b.sprite.Texture) = (b.sprite.Texture, a.sprite.Texture);
	}
}
