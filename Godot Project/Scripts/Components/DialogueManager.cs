using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

public class DialogueOption {
	public string text { get; set; }
	public string next { get; set; }
}

public class DialogueNode {
	public string id { get; set; }
	public string text { get; set; }
	public List<DialogueOption> options { get; set; }
}

public class Dialogue {
	public string speaker { get; set; }
	public List<DialogueNode> dialogue { get; set; }
}

public partial class DialogueManager : Control {
	public static DialogueManager Instance { get; private set; }
	
	private Dictionary<string, DialogueNode> dialogueTree;
	private Label dialogueText;
	private VBoxContainer optionsContainer;
	private PackedScene optionButtonScene;
	private TaskCompletionSource<string> nextNodeSource;
	private string currentSpeaker;

	public override void _Ready() {
		Instance = this;
		var panel = GetNode<Panel>("DialoguePanel");
		
		panel.ZIndex = 100;
		panel.SetZAsRelative(false);
		panel.Visible = false;
	}
	
	public async Task StartDialogue(string name) {
		await StartDialogue(name, "start");
	}
	
	public async Task StartDialogue(string name, string startNode) {
		dialogueTree = new();
		dialogueText = GetNode<Label>("DialoguePanel/DialogueText");
		optionsContainer = GetNode<VBoxContainer>("DialoguePanel/OptionsContainer");

		// show dialogue box
		var panel = GetNode<Panel>("DialoguePanel");
		panel.Visible = true;
		
		// load dialogue from json
		var file = FileAccess.Open($"res://Dialogue/{name}.json", FileAccess.ModeFlags.Read);
		var jsonText = file.GetAsText();
		file.Close();

		var dialogue = JsonSerializer.Deserialize<Dialogue>(jsonText);
		currentSpeaker = dialogue.speaker;
		
		var sprite = GetNode<Sprite2D>("DialoguePanel/Person");
			
		// display speaker portrait if not player
		if (currentSpeaker == "self") {
			sprite.Visible = false;
			dialogueText.Size = new Vector2(490, dialogueText.Size.Y);
			dialogueText.Position = new Vector2(5, dialogueText.Position.Y);
			optionsContainer.Size = new Vector2(490, optionsContainer.Size.Y);
			optionsContainer.Position = new Vector2(5, optionsContainer.Position.Y);
		} else {
			sprite.Visible = true;
			dialogueText.Size = new Vector2(390, dialogueText.Size.Y);
			dialogueText.Position = new Vector2(105, dialogueText.Position.Y);
			optionsContainer.Size = new Vector2(390, optionsContainer.Size.Y);
			optionsContainer.Position = new Vector2(105, optionsContainer.Position.Y);
			var texture = GD.Load<Texture2D>($"res://Assets/Character Designs/{dialogue.speaker}/portrait.png");
			sprite.Texture = texture;
		}
		
		// build dialogue tree
		foreach (var node in dialogue.dialogue) {
			node.options ??= new List<DialogueOption>();
			dialogueTree[node.id] = node;
		}
		
		await RunDialogue(startNode);
		panel.Visible = false;
	}

	private async Task RunDialogue(string startId) {
		string currentId = startId;

		while (dialogueTree.ContainsKey(currentId)) {
			var node = dialogueTree[currentId];

			ClearOptions();
			dialogueText.Text = currentSpeaker == "self" ? "" : $"{currentSpeaker}: ";

			// load current message one word at a time
			foreach (string word in node.text.Split(' ')) {
				dialogueText.Text += word + " ";
				await ToSignal(GetTree().CreateTimer(0.05), "timeout");
			}

			if (node.options.Count == 0) {
				await ToSignal(GetTree().CreateTimer(2), "timeout");
				break;
			}

			nextNodeSource = new TaskCompletionSource<string>();

			for (int i = 0; i < node.options.Count; i++) {
				var option = node.options[i];
				var button = new Button {
					Text = option.text,
					Name = i.ToString()
				};
				
				string targetId = option.next;
				button.Pressed += () => nextNodeSource.TrySetResult(targetId);
				optionsContainer.AddChild(button);
				button.FocusMode = FocusModeEnum.All;
				button.GrabFocus();
			}

			currentId = await nextNodeSource.Task;
			if (currentId == "end") {
				break;
			}
		}

		ClearOptions();
	}

	private void ClearOptions() {
		foreach (Node child in optionsContainer.GetChildren()) {
			child.QueueFree();
		}
	}
}
