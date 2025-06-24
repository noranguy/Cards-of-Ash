using Godot;
using System;
using System.Collections.Generic;

// Stores information to display before each Menko game
public struct Info {
	public string Title;
	public string Class;
	public string Description;

	public Info(string Title, string Class, string Description)
	{
		this.Title = Title;
		this.Class = Class;
		this.Description = Description;
	}
}

public partial class GlobalState : Node {
	public static GlobalState Instance { get; private set; }
	
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
	
	private int day = 0;
	
	private List<Func<Agent>> AgentFactories = new List<Func<Agent>> {
		() => new Agent0(),
		() => new Agent1()
	};
	
	private static string spacer = "\n\u00A0\n";
	private Info[] Infos = new Info[] {
new Info("Card Types", "basic", 
$@"Card Type Advantages:
- Tsunami beats Volcano
- Volcano beats Earthquake
- Earthquake beats Tsunami
{spacer}
Press SPACE to continue.
"),
new Info("Ceramic Class", "ceramic",
$@"Throwing Ability (3 in hand):
Flips adjacent cards to the target at 25% of the original chance (considering the adjacent type, not the target type).
{spacer}
Table Ability (2 in hand):
Deteriorates adjacent cards by 20% when flipped.
{spacer}
Press SPACE to continue.
")
	};
	
	public Agent GetNextAgent() {
		return AgentFactories[day]();
	}
	
	public int GetDay() {
		return day;
	}
	
	// Updates the player's decks and increments day counter
	public void NextDay() {
		if (day > 0) {
			var clas = GetInfo().Class;
			AddHandCard("tsunami", clas);
			AddHandCard("volcano", clas);
			AddHandCard("earthquake", clas);
			AddTableCard(clas);
			AddTableCard(clas);
		}
		day++;
	}
	
	public Info GetInfo() {
		return Infos[day];
	}
}
