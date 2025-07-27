using Godot;
using System;
using System.Collections.Generic;

// Stores information to display before each Menko game
public struct Info {
	public string Title;
	public string Class;
	public string Description;

	public Info(string Title, string Class, string Description) {
		this.Title = Title;
		this.Class = Class;
		this.Description = Description;
	}
}

public partial class GlobalState : Node {
	public static GlobalState Instance { get; private set; }
	
	public bool interactive = true;
	
	public override void _Ready() {
		Instance = this;
		
		handDeck.Add(("tsunami", "ceramic"));
	}
	
	private List<(string, string)> handDeck = new List<(string, string)>();
	private List<(string, string)> tableDeck = new List<(string, string)>();
	
	private List<(string, string)> handCards = new List<(string, string)> {
		("tsunami", "basic"),
		("volcano", "basic"),
		("earthquake", "basic"),
		("tsunami", "basic"),
		("volcano", "basic"),
		("earthquake", "basic"),
		("tsunami", "basic"),
		("volcano", "basic"),
		("earthquake", "basic"),
	};
	
	private List<(string, string)> tableCards = new List<(string, string)> {
		("tsunami", "basic"),
		("volcano", "basic"),
		("earthquake", "basic"),
		("tsunami", "basic"),
		("volcano", "basic"),
		("earthquake", "basic")
	};
	
	public readonly List<string> Omamories = new List<string> {
		"none",
		"aftershock",
		"stone_cast",
		"bag_of_sand",
		"fortune_slip"
	};
	
	public readonly Dictionary<string, string> OmamoriDescriptions = new Dictionary<string, string> {
		{"none", "No Omamori"},
		{"aftershock", "Aftershock: Hitting the same card consecutively boosts flip chance by 20%"},
		{"stone_cast", "Stone Cast: If the opponent fails to flip one of your table cards, it deteriorates by 5%"},
		{"bag_of_sand", "Bag of Sand (one-time use): The opponent loses visibility of card info"},
		{"fortune_slip", "Fortune Slip: Gain a random bonus each round"}
	};
	
	private string currentOmamori = "none";
	
	public string GetOmamori() {
		return currentOmamori;
	}
	
	public void UpdateOmamori(string next) {
		currentOmamori = next;
	}
	
	public List<(string, string)> GetHandCards() {
		return handCards;
	}
	
	public List<(string, string)> GetTableCards() {
		return tableCards;
	}
	
	public List<(string, string)> GetHandDeck() {
		return handDeck;
	}
	
	public List<(string, string)> GetTableDeck() {
		return tableDeck;
	}
	
	public void UpdateHandCards(List<(string, string)> cards) {
		handCards = cards;
	}
	
	public void UpdateTableCards(List<(string, string)> cards) {
		tableCards = cards;
	}
	
	public void UpdateHandDeck(List<(string, string)> cards) {
		handDeck = cards;
	}
	
	public void UpdateTableDeck(List<(string, string)> cards) {
		tableDeck = cards;
	}
	
	// Base probabilities of flipping a card against [stronger, equal, weaker] type
	public readonly float[] FlipProb = new float[] {0, 0.5f, 0.95f};
	
	public readonly Dictionary<string, int> TypeMap = new() {
		{ "volcano", 0 },
		{ "tsunami", 1 },
		{ "earthquake", 2 }
	};
	
	// Probability multiplier of flipping adjacent cards when throwing a ceramic card
	public readonly double CeramicProb = 0.15;
	
	// Probability multiplier when throwing an elastic card
	public readonly double ElasticProb = 0.75;
	
	// Probability multiplier when throwing at a defense card
	public readonly double DefenseProb = 0.75;
	
	public readonly int NumHandCards = 9;
	public readonly int NumTableCards = 6;
	
	// Keep track of in game day
	private int day = 0;

	// Keep track of where in the day the game is in, either pre or post game
	private bool post_game = false;

	// Flag for the begining when the player takes the cards off the table
	private bool player_has_cards = false;

	// Used in safehouse to check if coming back from a minigame, so it wont treat it as a new day
	private bool in_minigame = false;

	// List to keep track of which npcs are in the safehouse
	private bool[] inhabitants = new bool[5] { false, false, false, false, false };

	// Keep track of mission progress, which determines what characters dialogue is
	private bool[] mission_completed = new bool[5] {false, false, false, false, false };
	
	private List<Func<Agent>> AgentFactories = new List<Func<Agent>> {
		() => new Agent0(),
		() => new Agent1(),
		() => new Agent2(),
		() => new Agent3(),
		() => new Agent4(),
		() => new Agent5(),
	};
	
	private Info[] Infos = new Info[] {
new Info("Card Types", "basic", 
$@"Each player begins with 9 throwing cards and 6 table cards, evenly split between three types: Tsunami, Volcano, and Earthquake. These types follow a rock-paper-scissors system.

Throwing a card:
- Strong against the table card → 95% flip chance
- Same type → 50% flip chance
- Weak against → 0% flip chance

Press SPACE to continue.
"),
new Info("Ceramic Class", "ceramic",
$@"Throwing Ability: (3 in opponent’s hand)
When thrown, this card also attempts to flip the table cards adjacent to the target, but at 25% of the original flip chance. For example, a tsunami ceramic card thrown at a volcano table card with an adjacent tsunami table card, has 95% and 12.5% chances to flip, respectively.

Table Ability: (2 on opponent’s table)
When flipped, this card causes adjacent cards to deteriorate, reducing the probability of them flipping by 20% in future attempts.

Press SPACE to continue.
"),
new Info("Defense Class", "defense",
$@"Throwing Ability: (3 in opponent’s hand)
Instead of attempting to flip the target, this card deteriorates it, reducing its future flip chance by 20%.

Table Ability: (2 on opponent’s table)
This card has 75% of the normal chance to be flipped.

Press SPACE to continue.
"),
new Info("Elastic Class", "elastic",
$@"Throwing Ability: (3 in opponent’s hand)
This card targets two table cards instead of one, each at 75% of the original flip chance.

Table Ability: (2 on opponent’s table)
When flipped, this card randomly chooses an unflipped card on the opposing table to be thrown at.

Press SPACE to continue.
"),
new Info("Vision Class", "vision",
$@"Throwing Ability: (3 in opponent’s hand)
Flips a table card twice (without deterioration) to reveal its type.

Table Ability: (2 on opponent’s table)
After 3 rounds, each Vision card randomly swaps places with another table card.

Press SPACE to continue.
"),
	};
	
	public readonly string[] RulebookPages = new string[] {
@"[center]Menko Rulebook[/center]

Each player starts with 9 throwing cards and 6 table cards. The objective is to flip more of the opponent’s cards. The game ends after 9 rounds, or earlier if a player flips all 6 of the opponent’s table cards. The outcome (win, lose, or tie) affects your resources in the safe house.
",
@"[center]Menko Rulebook[/center]

You can throw at any table card to defend / attack. Once a card is thrown, it is discarded and can’t be thrown again.

Each card will have a type of Tsunami, Volcano, or Earthquake, forming a rock-paper-scissors relationship which affects the chance of flipping the table card.
",
@"[center]Menko Rulebook[/center]

[center][img={400}]res://Assets/Rulebook/types.png[/img][/center]
",
@"[center]Menko Rulebook[/center]

You will see the types of your throwing cards but the types of table cards remain hidden until flipped.

Throwing a card:
- Strong against the table card → 95% flip chance
- Same type → 50% flip chance
- Weak against → 5% flip chance
",
@"[center]Menko Rulebook[/center]

If a table card has been flipped, it can be flipped again to “unflip” it. However, each successful flip causes the card to deteriorate, making it 20% harder to flip in future attempts. For example, the chance of flipping a table card that is of the same type and has been flipped twice is 50% * 60% = 30%.
",
@"[center]Menko Rulebook (Ceramic)[/center]

[center][img={100}]res://Assets/Cards/tsunami_ceramic.png[/img]  [img={100}]res://Assets/Cards/earthquake_ceramic.png[/img]  [img={100}]res://Assets/Cards/volcano_ceramic.png[/img][/center]

Throwing Ability: Also attempts to flip the table cards adjacent to the target, but at 15% of the original flip chance.

Table Ability: Causes adjacent cards to deteriorate, reducing the probability of them flipping by 20%.
",
@"[center]Menko Rulebook (Defense)[/center]

[center][img={100}]res://Assets/Cards/tsunami_defense.png[/img]  [img={100}]res://Assets/Cards/earthquake_defense.png[/img]  [img={100}]res://Assets/Cards/volcano_defense.png[/img][/center]

Throwing Ability: Instead of attempting to flip the target, this card deteriorates it, reducing its future flip chance by 20%.

Table Ability: Has 75% of the normal chance to be flipped.
",
@"[center]Menko Rulebook (Elastic)[/center]

[center][img={100}]res://Assets/Cards/tsunami_elastic.png[/img]  [img={100}]res://Assets/Cards/earthquake_elastic.png[/img]  [img={100}]res://Assets/Cards/volcano_elastic.png[/img][/center]

Throwing Ability: Targets two table cards instead of one, each at 75% of the original flip chance.

Table Ability: When flipped, randomly chooses an unflipped card on the opposing table to be thrown at.
",
@"[center]Menko Rulebook (Vision)[/center]

[center][img={100}]res://Assets/Cards/tsunami_vision.png[/img]  [img={100}]res://Assets/Cards/earthquake_vision.png[/img]  [img={100}]res://Assets/Cards/volcano_vision.png[/img][/center]

Throwing Ability: Flips a table card twice (without deterioration) to reveal its type.

Table Ability: After 3 rounds, each Vision card randomly swaps places with another table card.
"
	};
	
	public Agent GetNextAgent() {
		return AgentFactories[day]();
	}
	
	public int GetDay() {
		return day;
	}

	public bool GetPostGame()
	{
		return post_game;
	}

	public void SetPostGame(bool after_game)
	{
		post_game = after_game;
	}

	public bool DoesPlayerHaveCards()
	{
		return player_has_cards;
	}

	public void PlayerGetsCards()
	{
		player_has_cards = true;
	}

	public void NewInhabitant()
	{
		inhabitants[day] = true;
	}

	public bool[] GetInhabitants()
	{
		return inhabitants;
	}

	public void MissionCompleted(int character_num)
	{
		mission_completed[character_num] = true;
	}

	public bool[] GetCompletedMission()
	{
		return mission_completed;
	}

	public void SetInMinigame(bool in_game)
	{
		in_minigame = in_game;
	}

	public bool GetInMinigame()
	{
		return in_minigame;
	}
	
	// Updates the player's decks and increments day counter
	public void NextDay()
	{
		if (day > 0)
		{
			var clas = GetInfo().Class;
			handDeck.Add(("tsunami", clas));
			handDeck.Add(("volcano", clas));
			handDeck.Add(("earthquake", clas));
			tableDeck.Add(("tsunami", clas));
			tableDeck.Add(("volcano", clas));
			tableDeck.Add(("earthquake", clas));
		}
		day++;
	}
	
	public Info GetInfo() {
		return Infos[day];
	}
	
	public int phase = 0;
}
