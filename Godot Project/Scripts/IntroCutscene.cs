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
	private AudioStreamPlayer2D sfx;
	private TextureRect background;
	private PointLight2D lightDay;
	private AnimatedSprite2D trainPlayer;
	private AnimationPlayer fade;
	private Camera2D camera;
	private Vector2 camBasePosition;
	private FastNoiseLite noise;
	private Vector2 camPosition;
	private float RANDOM_SHAKE_STRENGTH = 3000;
	private const float SHAKE_TIME = 1;
	private float SHAKE_DELAY_RATE;


	bool trainPlaying;
	bool playFadeDay;
	int num = 1;

	public override void _Ready()
	{
		trainAudio = GetNode<AudioStreamPlayer>("trainSound");
		shibuyaAudio = GetNode<AudioStreamPlayer>("shibuyaSound");
		background = GetNode<TextureRect>("background");
		lightDay = GetNode<PointLight2D>("lightDay");
		trainPlayer = GetNode<AnimatedSprite2D>("TrainPlayer");
		fade = GetNode<AnimationPlayer>("Fade/AnimationPlayer");
		camera = GetNode<Camera2D>("Camera2D");
		sfx = GetNode<AudioStreamPlayer2D>("sfx");

		noise = new();
		fade.AnimationFinished += _on_animation_player_animation_finished; // fade signal 

		trainPlaying = true;

		trainPlayer.Visible = true;
		background.Visible = false;

		trainAudio.Play(); // train audio
	}

	public override void _Process(double delta)
	{
		if (SHAKE_DELAY_RATE > 0f)
		{
			camera.GlobalPosition = camBasePosition + new Vector2(GetNoise(1), GetNoise(0));
			SHAKE_DELAY_RATE -= (float)delta;
		}
	}
	private async Task playParts(int partNumber)
	{
		GD.Print(partNumber);
		switch (partNumber)
		{
			case 1:
				partOne();
				break;
			case 2:
				await partTwo();
				break;
			case 3:
				await partThree();
				break;
			case 4:
				await partFour();
				break;
			case 5:
				await partFive();
				break;
			case 6:
				GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("safehouse.tscn");
				break;
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
		lightDay.Visible = true;
		background.Visible = true;
		trainPlayer.Visible = false;
		trainPlayer.Pause();
		shibuyaAudio.Play();
		await CutsceneDialogue(1);
		_ScreenShake();
		sfx.PitchScale = 2.15f;
		sfx.Stream = GD.Load<AudioStream>("res://Assets/SFX/Earthquake sound effect no copyright.mp3");
		sfx.Play();
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		await CutsceneDialogue(2);
		RANDOM_SHAKE_STRENGTH = 5000;
		_ScreenShake();
		sfx.PitchScale = 4.15f;
		fade.Play("fade_to_game");
	}
	private async Task partThree()
	{
		fade.Play("fade_to_day");
		background.Texture = GD.Load<Texture2D>("res://Assets/Cutscenes/road.png");
		RANDOM_SHAKE_STRENGTH = 1000;
		_ScreenShake();
		await CutsceneDialogue(3);
		fade.Play("fade_to_game");
	}
	private async Task partFour()
	{
		fade.Play("fade_to_day");
		background.Texture = GD.Load<Texture2D>("res://Assets/Cutscenes/shibuya_post.png");
		await CutsceneDialogue(4);
		fade.Play("fade_to_game");
	}
	private async Task partFive()
	{
		background.Visible = false;
		sfx.PitchScale = 1f;
		sfx.Stop();
		shibuyaAudio.Stop();
		sfx.Stream = GD.Load<AudioStream>("res://Assets/SFX/running.mp3");
		sfx.Play();
		await CutsceneDialogue(5);
		sfx.Stop();
		sfx.Stream = GD.Load<AudioStream>("res://Assets/SFX/Door Opening Sound Effect.mp3");
		sfx.Play();
		fade.Play("fade_to_game");
	}
	private async Task CutsceneDialogue(int numDialogue)
	{
		await ThoughtsDialogueAsync($"Cutscenes/intro_{numDialogue}");
	}

	private void _on_animation_player_animation_finished(StringName anim_name)
	{
		if (anim_name == "fade_to_game")
		{
			num++; 
			_ = playParts(num);
		 }

	}
	private void _on_train_sound_finished()
	{
		_ = playParts(num);
	}
	// private void _on_train_player_animation_finished()
	// {

	// }
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
