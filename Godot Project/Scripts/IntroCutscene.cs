using Godot;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public partial class IntroCutscene : Node2D
{
	private AudioStreamPlayer trainAudio;
	private AudioStreamPlayer shibuyaAudio;
	private TextureRect background;
	private PointLight2D lightDay;
	private AnimatedSprite2D trainPlayer;
	private AnimationPlayer fade;
	private Camera2D camera;
	private Vector2 camBasePosition;
	private FastNoiseLite noise;
	private Vector2 camPosition;
	private const float RANDOM_SHAKE_STRENGTH = 5000;
	private const float SHAKE_TIME = 1;
	private float SHAKE_DELAY_RATE;


	bool trainPlaying;
	bool playFadeDay;
	int num;

	public override void _Ready()
	{
		trainAudio = GetNode<AudioStreamPlayer>("trainSound");
		shibuyaAudio = GetNode<AudioStreamPlayer>("shibuyaSound");
		background = GetNode<TextureRect>("background");
		lightDay = GetNode<PointLight2D>("lightDay");
		trainPlayer = GetNode<AnimatedSprite2D>("TrainPlayer");
		fade = GetNode<AnimationPlayer>("Fade/AnimationPlayer");
		camera = GetNode<Camera2D>("Camera2D");
	
		noise = new();
		fade.AnimationFinished += _on_animation_player_animation_finished; // fade signal 

		trainPlaying = true;
		playFadeDay = false;

		trainPlayer.Visible = true;
		background.Visible = false;


		num = 1;
		trainAudio.Play(); // train audio
	}

	public override void _Process(double delta)
	{
		if (!trainAudio.Playing && trainPlaying) // train screen
		{
			partOne();
		}
		if (!fade.IsPlaying() && playFadeDay) // loading shibuya screen
		{
			_ = partTwo(); 
		}
		if (SHAKE_DELAY_RATE > 0f)
		{
			camera.GlobalPosition = camBasePosition + new Vector2(GetNoise(0), GetNoise(1));
			SHAKE_DELAY_RATE -= (float)delta;
		}
	}
	private void partOne()
	{ 
		trainPlayer.Play("train_open");
		fade.Play("fade_to_game");
	}
	private async Task partTwo()
	{
		fade.Play("fade_to_day");

		shibuyaAudio.Play();
		await CutsceneDialogue();
		_ScreenShake();
	}
	private async Task CutsceneDialogue()
	{
		await ThoughtsDialogueAsync($"Cutscenes/intro_{num}");
		num++;
	}
	
	private void _on_animation_player_animation_finished(StringName anim_name)
	{
		if (anim_name == "fade_to_game")
		{
			background.Visible = true;
			trainPlayer.Visible = false;
			trainPlaying = false;
			playFadeDay = true;
			trainPlayer.Pause();
			
		}
		else if (anim_name == "fade_to_day")
		{
			playFadeDay = false;
		}
	}
	private async Task ThoughtsDialogueAsync(string dialogue_path)
	{
		await DialogueManager.Instance.StartDialogue(dialogue_path, false);

	}
	private void _ScreenShake()
	{
		camBasePosition = camera.GlobalPosition;
		SHAKE_DELAY_RATE = SHAKE_TIME;
	}
	private float GetNoise(int seed)
	{
		noise.Seed = seed;
		return noise.GetNoise1D(GD.Randf() * SHAKE_DELAY_RATE) * RANDOM_SHAKE_STRENGTH;
	}
}
