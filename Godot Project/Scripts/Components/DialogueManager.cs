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

public partial class DialogueManager : Control {
	public static DialogueManager Instance { get; private set; }
	
	private Dictionary<string, DialogueNode> dialogueTree;
	public static DialogueNode currentNode;

	private Label dialogueText;
	private VBoxContainer optionsContainer;
	private PackedScene optionButtonScene;
	
	private TaskCompletionSource<string> nextNodeSource;

	public override async void _Ready() {
		Instance = this;
		var panel = GetNode<Panel>("DialoguePanel");
		panel.ZIndex = 100;
		panel.SetZAsRelative(false);
		panel.Visible = false;
	}
	
	public async Task StartDialogue(string name) {
		dialogueTree = new();
		dialogueText = GetNode<Label>("DialoguePanel/DialogueText");
		optionsContainer = GetNode<VBoxContainer>("DialoguePanel/OptionsContainer");

		var panel = GetNode<Panel>("DialoguePanel");
		panel.Visible = true;
		
		LoadDialogue($"res://Dialogue/{name}.json");
		await RunDialogue("start");
		
		panel.Visible = false;
	}

	public void LoadDialogue(string path) {
		var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		var jsonText = file.GetAsText();
		file.Close();

		var nodes = JsonSerializer.Deserialize<List<DialogueNode>>(jsonText);
		foreach (var node in nodes) {
			node.options ??= new List<DialogueOption>();
			dialogueTree[node.id] = node;
		}
	}

	public async Task RunDialogue(string startId) {
		string currentId = startId;

		while (dialogueTree.ContainsKey(currentId)) {
			var node = dialogueTree[currentId];
			currentNode = node;

			ClearOptions();
			dialogueText.Text = "";

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
