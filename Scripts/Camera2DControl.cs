using Godot;
using System;

public partial class Camera2DControl : Camera2D
{

	private readonly Vector2 MIX_ZOOM = new Vector2(0.15f, 0.15f); // min value for Zoom
	private readonly Vector2 MAX_ZOOM = new Vector2(1.5f, 1.5f); // max value for Zoom
	private readonly Vector2 ZOOM_SPEED = new Vector2(0.1f, 0.1f); // speed value for Zoom

	public override void _Ready()
	{
		Zoom = MIX_ZOOM;
	}

	//Action after press button
	public override void _UnhandledInput(InputEvent @event)
	{
		//Move camera position
		if (@event is InputEventMouseMotion mouseMotionEvent)
		{
			if (mouseMotionEvent.ButtonMask == MouseButtonMask.Right)
			{
				Position -= mouseMotionEvent.Relative / Zoom;
			}
		}
	}

	//Action when press button
	public override void _Input(InputEvent @event)
	{
		//Change zoom scale
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			switch (mouseButtonEvent.ButtonIndex)
			{
				case MouseButton.WheelUp:
					Zoom = Zoom > MIX_ZOOM ? Zoom - ZOOM_SPEED : Zoom;
					break;

				case MouseButton.WheelDown:
					Zoom = Zoom < MAX_ZOOM ? Zoom + ZOOM_SPEED : Zoom;
					break;
			}
		}
	}
}
