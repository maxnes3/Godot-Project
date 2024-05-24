using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BlindedSoulsBuild.Scripts
{
	public partial class WorkerController : Node2D
	{
		private Vector2I sawmill;
		private bool full;
		private List<(int, int)> pathList;
		private Vector2I nextStep;

		public override void _Ready()
		{
			sawmill = new Vector2I((int)Position.X, (int)(Position.Y - TileMapController.tileSize.Y));
			pathList = new List<(int, int)>();
			full = false;
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{ 
			//MoveToTile();
		}

		private void CreateNewPath()
		{

		}

		private void MoveToTile()
		{
			Position.MoveToward(nextStep, 1);
		}

		/*private (int x, int y) FindNearestTree()
		{
			
		}*/
	}
}
