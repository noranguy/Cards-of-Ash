using Godot;
using System;

public partial class RadioGame: Control
{
    bool dial_moving;
    bool tracking;
    float lerped_angle;
    float channel = 82.5f;

    public bool PlayGame(float goal_channel)
    {
        if (Input.IsActionJustPressed("click"))
        {
            if (dial_moving)
            {
                tracking = true;
            }
        }

        else if (Input.IsActionJustReleased("click"))
        {
            tracking = false;
        }

        if (tracking)
        {
            float angle = GetGlobalMousePosition().AngleToPoint(GetNode<Node2D>("Dial").GlobalPosition) - 1.57f;
            lerped_angle = Mathf.LerpAngle(GetNode<TextureRect>("Dial/DialTexture").Rotation, angle, 1);

            Vector2 distance = GetNode<Node2D>("Dial/DialTexture/DialPoint").Position.Rotated(GetNode<TextureRect>("Dial/DialTexture").Rotation);
            float angle_to = GetNode<Node2D>("Dial/MiddlePoint").Position.AngleTo(distance);
            channel = RangeLerp(angle_to, -3.14f, 3.14f, 80, 85);
            channel = (float)Math.Round(channel, 1);
            //channel = (float)Math.Clamp(channel, 76.1, 94.9);
        }

        GetNode<TextureRect>("Dial/DialTexture").Rotation = Math.Clamp(lerped_angle, -2, 2);

        GetNode<Label>("CenterContainer/ColorRect/ChannelLabel").Text = $"{channel.ToString()} FM";

        AudioStreamPlayer radio_sound = GetNode<AudioStreamPlayer>("RadioSound");
        AudioStreamPlayer static_sound = GetNode<AudioStreamPlayer>("StaticSound");

        if (!radio_sound.Playing)
        {
            radio_sound.Play();
        }

        if (!static_sound.Playing)
        {
            static_sound.Play();
        }


        if (channel == goal_channel)
        {
            GetNode<TextureRect>("OnLight").Visible = true;
            radio_sound.VolumeDb = -10;
            static_sound.VolumeDb = -80;
        }

        else if (Math.Abs(channel - goal_channel) < .1)
        {
            GetNode<TextureRect>("OnLight").Visible = false;
            radio_sound.VolumeDb = -12;
            static_sound.VolumeDb = -9;
        }

        else if (Math.Abs(channel - goal_channel) < .2)
        {
            GetNode<TextureRect>("KindaOnLight").Visible = true;
            GetNode<TextureRect>("OnLight").Visible = false;
            radio_sound.VolumeDb = -13;
            static_sound.VolumeDb = -7;
        }

        else if (Math.Abs(channel - goal_channel) < .3)
        {
            radio_sound.VolumeDb = -15;
            static_sound.VolumeDb = -6;
        }

        else if (Math.Abs(channel - goal_channel) < .5)
        {
            GetNode<TextureRect>("BarelyOnLight").Visible = true;
            GetNode<TextureRect>("KindaOnLight").Visible = false;
            GetNode<TextureRect>("OnLight").Visible = false;
            radio_sound.VolumeDb = -17;
            static_sound.VolumeDb = -5;
        }

        else if (Math.Abs(channel - goal_channel) < 1)
        {
            GetNode<TextureRect>("KindaOffLight").Visible = true;
            GetNode<TextureRect>("BarelyOnLight").Visible = false;
            GetNode<TextureRect>("KindaOnLight").Visible = false;
            GetNode<TextureRect>("OnLight").Visible = false;

            radio_sound.VolumeDb = -20;
            static_sound.VolumeDb = -4;
        }

        else if (Math.Abs(channel - goal_channel) < 1.5)
        {
            radio_sound.VolumeDb = -23;
            static_sound.VolumeDb = -3;
        }

        else if (Math.Abs(channel - goal_channel) < 2)
        {
            radio_sound.VolumeDb = -25;
            static_sound.VolumeDb = 0;
        }

        else
        {
            GetNode<TextureRect>("OnLight").Visible = false;
            GetNode<TextureRect>("KindaOffLight").Visible = false;
            GetNode<TextureRect>("BarelyOnLight").Visible = false;
            GetNode<TextureRect>("KindaOnLight").Visible = false;
            GetNode<TextureRect>("OnLight").Visible = false;

            static_sound.VolumeDb = 3;
            radio_sound.VolumeDb = -80;
        }

        if (channel == goal_channel)
        {
            return true;
        }

        else
        {
            return false;
        }
    }

    private float RangeLerp(float val, float min1, float max1, float min2, float max2)
    {
        float normal = (val - min1) / (max1 - min1);
        return (min2 + (max2 - min2) * normal);
    }
    
    private void _on_dial_texture_mouse_entered()
	{
		dial_moving = true;
	}

	private void _on_dial_texture_mouse_exited()
	{
		dial_moving = false;
	}
}
