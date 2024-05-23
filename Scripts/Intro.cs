using Godot;
using System;

public partial class Intro : VideoStreamPlayer
{
	private void _on_finished()
	{
		GetTree().ChangeSceneToFile("res://game.tscn");
	}
}
