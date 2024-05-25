using Godot;
using System;

namespace BlindedSoulsBuild.Scripts
{
	public partial class Camera2DControl : Camera2D
	{

		private readonly Vector2 MIN_ZOOM = new Vector2(1.0f, 1.0f); // min value for Zoom
		private readonly Vector2 MAX_ZOOM = new Vector2(2.5f, 2.5f); // max value for Zoom
		private readonly Vector2 ZOOM_SPEED = new Vector2(0.1f, 0.1f); // speed value for Zoom

		public override void _Ready()
		{
			Zoom = MIN_ZOOM;
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
					case MouseButton.WheelDown:
						Zoom = Zoom > MIN_ZOOM ? Zoom - ZOOM_SPEED : Zoom;
						break;

					case MouseButton.WheelUp:
						Zoom = Zoom < MAX_ZOOM ? Zoom + ZOOM_SPEED : Zoom;
						break;
				}
			}
		}
	}
}
