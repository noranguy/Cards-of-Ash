using Godot;
using System;
using System.Numerics;

public partial class PhoneGame : Control
{
    bool in_red_start;
    bool in_yellow_start;
    bool in_green_start;
    bool in_blue_start;

    bool in_red_end;
    bool in_yellow_end;
    bool in_green_end;
    bool in_blue_end;

    bool got_red = false;
    bool got_yellow = false;
    bool got_green = false;
    bool got_blue = false;

    bool drawing_red;
    bool drawing_yellow;
    bool drawing_green;
    bool drawing_blue;

    Line2D red_line;
    Line2D yellow_line;
    Line2D green_line;
    Line2D blue_line;

    Godot.Vector2 origin = Godot.Vector2.Zero;

    public bool PlayGame()
    {
        red_line = GetNode<Line2D>("RedLine");
        yellow_line = GetNode<Line2D>("YellowLine");
        green_line = GetNode<Line2D>("GreenLine");
        blue_line = GetNode<Line2D>("BlueLine");

        if (Input.IsActionJustPressed("click"))
        {
            GD.Print("Clicked");
            if (in_red_start)
            {
                drawing_red = true;
            }

            if (in_yellow_start)
            {
                drawing_yellow = true;
            }

            if (in_green_start)
            {
                drawing_green = true;
            }

            if (in_blue_start)
            {
                drawing_blue = true;
            }
        }

        else if (Input.IsActionJustReleased("click"))
        {
            if (in_red_end && drawing_red)
            {
                red_line.ClearPoints();
                red_line.AddPoint(origin);
                red_line.AddPoint(new Godot.Vector2(160, 90));
                got_red = true;
            }

            if (in_yellow_end && drawing_yellow)
            {
                yellow_line.ClearPoints();
                yellow_line.AddPoint(origin);
                yellow_line.AddPoint(new Godot.Vector2(160, 0));
                got_yellow = true;
            }

            if (in_green_end && drawing_green)
            {
                green_line.ClearPoints();
                green_line.AddPoint(origin);
                green_line.AddPoint(new Godot.Vector2(160, -60));
                got_green = true;
            }

            if (in_blue_end && drawing_blue)
            {
                blue_line.ClearPoints();
                blue_line.AddPoint(origin);
                blue_line.AddPoint(new Godot.Vector2(160, -30));
                got_blue = true;
            }


            if (!got_red)
            {
                red_line.ClearPoints();
                red_line.AddPoint(origin);
            }

            if (!got_yellow)
            {
                yellow_line.ClearPoints();
                yellow_line.AddPoint(origin);
            }

            if (!got_green)
            {
                green_line.ClearPoints();
                green_line.AddPoint(origin);
            }

            if (!got_blue)
            {
                blue_line.ClearPoints();
                blue_line.AddPoint(origin);
            }

            drawing_red = false;
            drawing_yellow = false;
            drawing_blue = false;
            drawing_green = false;
        }

        Godot.Vector2 mouse_pos = GetGlobalMousePosition();
        mouse_pos.X += 76.5f;

        if (drawing_red && !got_red)
        {
            mouse_pos.Y += 47;

            mouse_pos = ClampVector(mouse_pos, -20, 178, -28, 128);

            red_line.ClearPoints();
            red_line.AddPoint(origin);
            red_line.AddPoint(mouse_pos);
        }

        if (drawing_yellow && !got_yellow)
        {
            mouse_pos.Y += 15;

            mouse_pos = ClampVector(mouse_pos, -20, 178, -58, 98);

            yellow_line.ClearPoints();
            yellow_line.AddPoint(origin);
            yellow_line.AddPoint(mouse_pos);
        }

        if (drawing_green && !got_green)
        {
            mouse_pos.Y -= 15;

            mouse_pos = ClampVector(mouse_pos, -20, 178, -88, 68);

            green_line.ClearPoints();
            green_line.AddPoint(origin);
            green_line.AddPoint(mouse_pos);
        }

        if (drawing_blue && !got_blue)
        {
            mouse_pos.Y -= 47;

            mouse_pos = ClampVector(mouse_pos, -20, 178, -118, 38);

            blue_line.ClearPoints();
            blue_line.AddPoint(origin);
            blue_line.AddPoint(mouse_pos);
        }

        if (got_red && got_yellow && got_green && got_blue)
        {
            return true;
        }

        else
        {
            return false;
        }
    }

    private Godot.Vector2 ClampVector(Godot.Vector2 v, float minx, float maxx, float miny, float maxy)
    {
        Godot.Vector2 temp = v;
        if (temp.X < minx)
        {
            temp.X = minx;
        }

        if (temp.X > maxx)
        {
            temp.X = maxx;
        }

        if (temp.Y < miny)
        {
            temp.Y = miny;
        }

        if (temp.Y > maxy)
        {
            temp.Y = maxy;
        }

        return temp;
    }

    private void _on_red_start_area_mouse_entered()
    {
        in_red_start = true;
    }

    private void _on_red_start_area_mouse_exited()
    {
        in_red_start = false;
    }

    private void _on_yellow_start_area_mouse_entered()
    {
        in_yellow_start = true;
    }

    private void _on_yellow_start_area_mouse_exited()
    {
        in_yellow_start = false;
    }

    private void _on_green_start_area_mouse_entered()
    {
        in_green_start = true;
    }

    private void _on_green_start_area_mouse_exited()
    {
        in_green_start = false;
    }

    private void _on_blue_start_area_mouse_entered()
    {
        in_blue_start = true;
    }

    private void _on_blue_start_area_mouse_exited()
    {
        in_blue_start = false;
    }

    private void _on_red_end_area_mouse_entered()
    {
        in_red_end = true;
    }

    private void _on_red_end_area_mouse_exited()
    {
        in_red_end = false;
    }

    private void _on_yellow_end_area_mouse_entered()
    {
        in_yellow_end = true;
    }

    private void _on_yellow_end_area_mouse_exited()
    {
        in_yellow_end = false;
    }

    private void _on_green_end_area_mouse_entered()
    {
        in_green_end = true;
    }

    private void _on_green_end_area_mouse_exited()
    {
        in_green_end = false;
    }

    private void _on_blue_end_area_mouse_entered()
    {
        in_blue_end = true;
    }

    private void _on_blue_end_area_mouse_exited()
    {
        in_blue_end = false;
    }

}
