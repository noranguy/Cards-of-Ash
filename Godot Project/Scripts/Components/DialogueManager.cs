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
	private Button skipButton;
	private Panel panel;
	private RichTextLabel nameText;
	private TextureRect mainBox;
	private TextureRect portrait;
	private TextureRect namePlate;
	private TextureRect optionsTexture;
	private TextureRect skipTexture;
	
	
	private TaskCompletionSource<string> nextNodeSource;
	private string currentSpeaker;
	
	private bool _inPlay = false;
	
	private FontFile font;

	public override void _Ready() {
		Instance = this;
		dialogueText = GetNode<Label>("CanvasLayer/DialoguePanel/DialogueText");
		optionsContainer = GetNode<VBoxContainer>("CanvasLayer/DialoguePanel/optionsTexture/OptionsContainer");
		panel = GetNode<Panel>("CanvasLayer/DialoguePanel");
		nameText = GetNode<RichTextLabel>("CanvasLayer/DialoguePanel/namePlate/NameText");
		namePlate = GetNode<TextureRect>("CanvasLayer/DialoguePanel/namePlate");
		portrait = GetNode<TextureRect>("CanvasLayer/DialoguePanel/portrait");
		optionsTexture = GetNode<TextureRect>("CanvasLayer/DialoguePanel/optionsTexture");
		mainBox = GetNode<TextureRect>("CanvasLayer/DialoguePanel/mainBox");
		panel.ZIndex = 100;
		panel.SetZAsRelative(false);
		panel.Visible = false;
		optionsTexture.Visible = false;
		font = new FontFile();
		font.LoadDynamicFont("res://Fonts/m5x7.ttf");
	}
	
	public async Task StartDialogue(string name, bool inPlay) {
		await StartDialogue(name, inPlay, "start");
	}
	
	public async Task StartDialogue(string name, bool inPlay, string startNode) {
		_inPlay = inPlay;
		dialogueTree = new();
		
		// show dialogue box
		panel.Visible = true;
		
		// load dialogue from json
		var file = FileAccess.Open($"res://Dialogue/{name}.json", FileAccess.ModeFlags.Read);
		var jsonText = file.GetAsText();
		file.Close();
		
		var dialogue = JsonSerializer.Deserialize<Dialogue>(jsonText);
		currentSpeaker = dialogue.speaker;
		
		var sprite = GetNode<Sprite2D>("CanvasLayer/DialoguePanel/portrait/Person");

		// display speaker portrait if not player
		if (inPlay)
		{
			dialogueText.Scale = new Vector2(0.5f, 0.5f);
			optionsTexture.Scale = new Vector2(1, 1);
			portrait.Scale = new Vector2(1.4f, 1.4f);
			namePlate.Scale = new Vector2(1.4f, 1.4f);
			panel.Position = new Vector2(70, 224);
			panel.Size = new Vector2(500, 120);
			mainBox.Size = new Vector2(505, 125);
			//mainBox.Position = new Vector2(-5, 5);
			portrait.Position = new Vector2(8, 8);
			dialogueText.Position = new Vector2(110, 10);
			optionsTexture.Position = new Vector2(110, 70);
			optionsTexture.Visible = false;
		}
		else
		{
			panel.Position = new Vector2(35, 112);
			panel.Size = new Vector2(250, 60);
			dialogueText.Scale = new Vector2(0.25f, 0.25f);
			optionsTexture.Scale = new Vector2(0.5f, 0.5f);
			portrait.Scale = new Vector2(0.65f, 0.65f);
			//portrait.Position = new Vector2(30, 30);

			if (currentSpeaker == "self")
			{
				//sprite.Visible = false;
				portrait.Visible = false;
				namePlate.Visible = false;
				dialogueText.Size = new Vector2(960, 5);
				dialogueText.Position = new Vector2(5, 5);
				optionsTexture.Size = new Vector2(440, 20);
				optionsTexture.Position = new Vector2(5, 45);
				optionsContainer.Size = new Vector2(440, 20);
			}
			else
			{
				//sprite.Visible = true;
				portrait.Visible = true;
				namePlate.Visible = true;
				dialogueText.Size = new Vector2(760, 5);
				dialogueText.Position = new Vector2(60, 0);
				optionsTexture.Size = new Vector2(320, 20);
				optionsContainer.Size = new Vector2(320, 20);
				optionsTexture.Position = new Vector2(60, 42);
				var texture = GD.Load<Texture2D>($"res://Assets/Character Designs/{dialogue.speaker}/portrait.png");
				sprite.Texture = texture;
			}
		}

		if (currentSpeaker != "self") {
			nameText.Text = currentSpeaker;
			await ToSignal(GetTree().CreateTimer(0.05), "timeout");
			namePlate.Size = nameText.Size + new Vector2(8,0);
		}
		
		// build dialogue tree
		foreach (var node in dialogue.dialogue)
		{
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
			dialogueText.Text = currentSpeaker == "self" ? "" : $"";

			nextNodeSource = new TaskCompletionSource<string>();

			if (node.options.Count == 1)
			{
				if (_inPlay)
				{
					skipButton = new Button
					{
						Text = "Skip",
						
						Size = new Vector2(40, 30),
						
					};
					skipTexture = new TextureRect
					{
						Texture = GD.Load<Texture2D>("res://Assets/Dialogue/options.png"),
						Size = new Vector2(40, 30),
						Position = new Vector2(445, 70),
						Visible = true,
					};
					skipButton.AddThemeFontOverride("font", font);
				}
				else
				{
					skipButton = new Button
					{
						Text = "Skip",
						Visible = true,
						Size = new Vector2(40, 30),
						
					};
					skipTexture = new TextureRect
					{
						Texture = GD.Load<Texture2D>("res://Assets/Dialogue/options.png"),
						Size = new Vector2(40, 30),
						Visible = true,
						Position = new Vector2(228, 43),
						Scale = new Vector2(0.5f, 0.5f)
					};
					skipButton.AddThemeFontOverride("font", font);
				}
				string targetId = node.options[0].next;
				skipButton.Pressed += () => nextNodeSource.TrySetResult("end");
				skipTexture.AddChild(skipButton);
				panel.AddChild(skipTexture);
			}
			
			// load current message one word at a time
			foreach (string word in node.text.Split(' ')) {
				if (nextNodeSource.Task.IsCompleted) break;
				dialogueText.Text += word + " ";
				await ToSignal(GetTree().CreateTimer(0.05), "timeout");
			}

			if (node.options.Count == 0) {
				await ToSignal(GetTree().CreateTimer(2), "timeout");
				break;
			}

			for (int i = 0; i < node.options.Count; i++) {
				var option = node.options[i];
				Button button;
				
				if (_inPlay) {
					button = new Button {
						Text = option.text,
						Name = i.ToString()
					};
				} else {
					button = new Button
					{
						Text = option.text,
						Name = i.ToString(),
						Size = new Vector2(40, 30),
						Scale = new Vector2(0.25f, 0.25f)
						
					};
					button.AddThemeFontOverride("font", font);
					button.AddThemeFontSizeOverride("font_size", 16);
				}
				GD.Print(button.Size);
				string targetId = option.next;
				button.Pressed += () => nextNodeSource.TrySetResult(targetId);
				optionsContainer.AddChild(button);
				optionsTexture.Size = optionsContainer.Size;
				optionsTexture.Visible = true;
				button.FocusMode = FocusModeEnum.All;
				button.GrabFocus();
			}
			
			currentId = await nextNodeSource.Task;
			if (skipButton != null) {
				skipButton.QueueFree();
				skipButton = null;
			}
			if (skipTexture != null) {
				skipTexture.QueueFree();
				skipTexture = null;
			}
			if (currentId == "end")
			{
				break;
			}
		}

		ClearOptions();
	}

	private void ClearOptions()
	{
		foreach (Node child in optionsContainer.GetChildren())
		{
			child.QueueFree();
		}
		optionsTexture.Visible = false;
	}
}
