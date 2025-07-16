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
	}
	
	// Tracks the types of the cards in the player's hand deck
	private List<string> humanHandTypes = new List<string> {
		"tsunami",
		"volcano",
		"earthquake",
		"tsunami",
		"volcano",
		"earthquake",
		"tsunami",
		"volcano",
		"earthquake"
	};
	
	// Tracks the classes of the cards in the player's hand deck
	private List<string> humanHandClasses = new List<string> {
		"basic",
		"basic",
		"basic",
		"basic",
		"basic",
		"basic",
		"basic",
		"basic",
		"basic"
	};
	
	// Tracks the classes of the cards in the player's table deck
	private List<string> humanTableClasses = new List<string> {
		"basic",
		"basic",
		"basic",
		"basic",
		"basic",
		"basic"
	};
	
	public (List<string>, List<string>) GetHandCards() {
		return (humanHandTypes, humanHandClasses);
	}
	
	public List<string> GetTableClasses() {
		return humanTableClasses;
	}
	
	private void AddHandCard(string type, string clas) {
		humanHandTypes.Add(type);
		humanHandClasses.Add(clas);
	}
	
	private void AddTableCard(string clas) {
		humanTableClasses.Add(clas);
	}
	
	// Base probabilities of flipping a card against [stronger, equal, weaker] type
	public readonly float[] FlipProb = new float[] {0.05f, 0.5f, 0.95f};
	
	public readonly Dictionary<string, int> TypeMap = new() {
		{ "volcano", 0 },
		{ "tsunami", 1 },
		{ "earthquake", 2 }
	};
	
	// Probability multiplier of flipping adjacent cards when throwing a ceramic card
	public readonly double CeramicProb = 0.25;
	
	// Probability multiplier when throwing an elastic card
	public readonly double ElasticProb = 0.75;
	
	// Probability multiplier when throwing at a defense card
	public readonly double DefenseProb = 0.75;
	
	// Keep track of in game day
	private int day = 0;

	// Keep track of where in the day the game is in, either pre or post game
	private bool post_game = false;

	// Flag for the begining when the player takes the cards off the table
	private bool player_has_cards = false;

	// List to keep track of which npcs are in the safehouse
	private bool[] inhabitants = new bool[5] {false, false, false, false, false};
	
	private List<Func<Agent>> AgentFactories = new List<Func<Agent>> {
		() => new Agent0(),
		() => new Agent1(),
		() => new Agent2(),
		() => new Agent3(),
		() => new Agent4(),
	};
	
	private static string spacer = "\n\u00A0\n";
	private Info[] Infos = new Info[] {
new Info("Card Types", "basic", 
$@"Each player begins with 9 throwing cards and 6 table cards, evenly split between three types: Tsunami, Volcano, and Earthquake. These types follow a rock-paper-scissors system.
{spacer}
Throwing a card:
- Strong against the table card → 95% flip chance
- Same type → 50% flip chance
- Weak against → 5% flip chance
{spacer}
Press SPACE to continue.
"),
new Info("Ceramic Class", "ceramic",
$@"Throwing Ability: (3 in opponent’s hand)
When thrown, this card also attempts to flip the table cards adjacent to the target, but at 25% of the original flip chance. For example, a tsunami ceramic card thrown at a volcano table card with an adjacent tsunami table card, has 95% and 12.5% chances to flip, respectively.
{spacer}

Table Ability: (2 on opponent’s table)
When flipped, this card causes adjacent cards to deteriorate, reducing the probability of them flipping by 20% in future attempts.
{spacer}
Press SPACE to continue.
"),
new Info("Defense Class", "defense",
$@"Throwing Ability: (3 in opponent’s hand)
Instead of attempting to flip the target, this card deteriorates it, reducing its future flip chance by 20%.
{spacer}

Table Ability: (2 on opponent’s table)
Has 75% of the normal chance to be flipped.
{spacer}
Press SPACE to continue.
"),
new Info("Elastic Class", "elastic",
$@"Throwing Ability: (3 in opponent’s hand)
Targets two table cards instead of one, each at 75% of the original flip chance.
{spacer}

Table Ability: (2 on opponent’s table)
When flipped, it randomly chooses an unflipped card on the opposing table to be thrown at.
{spacer}
Press SPACE to continue.
"),
new Info("Vision Class", "vision",
$@"Throwing Ability: (3 in opponent’s hand)
Flips a table card twice (without deterioration) to reveal its type.
{spacer}

Table Ability: (2 on opponent’s table)
After 3 rounds, each Vision card randomly swaps places with another table card.
{spacer}
Press SPACE to continue.
"),
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

	public void NewInhabitant(int day)
	{
		inhabitants[day] = true;
	}

	public bool[] GetInhabitants()
	{
		return inhabitants;
	}
	
	// Updates the player's decks and increments day counter
	public void NextDay()
	{
		if (day > 0)
		{
			var clas = GetInfo().Class;
			//AddHandCard("tsunami", clas);
			//AddHandCard("volcano", clas);
			//AddHandCard("earthquake", clas);
			//AddTableCard(clas);
			//AddTableCard(clas);
		}
		day++;
	}
	
	public Info GetInfo() {
		return Infos[day];
	}
}
