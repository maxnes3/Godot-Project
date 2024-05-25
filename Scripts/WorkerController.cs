using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BlindedSoulsBuild.Scripts
{
	public partial class WorkerController : Node2D
	{
		private bool full;
		private Queue<(int, int)> pathList;
		private Vector2I nextStep;
		private int timeout = 0;
		private bool GoingOnBase = false;

		public override void _Ready()
		{
			full = false;
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			if (timeout < 1)
			{
				if (full == false)
				{
					MakeNewPath(4);
					ChangeStep();
					full = !full;
					//TO DO
				}
				MoveToTile();
			}
			else
			{
				timeout--;
			}
		}

		private void MakeNewPath(int targe)
		{
			(int i, int j) currentTile = ((int)(Position.Y - TileMapController.tileSize.Y / 2) / TileMapController.tileSize.Y,
						(int)(Position.X - TileMapController.tileSize.X / 2) / TileMapController.tileSize.X);
			(int i, int j) target = TileMovement.FindTarget(TileMapController.getEventMap(), currentTile, targe, (-1, -1));
			pathList = new Queue<(int, int)>(TileMovement.FindShortestPath(TileMapController.getFieldMap(), currentTile, target, new List<int> { 1 }));
		}

		private void ChangeStep()
		{
			var next = pathList.Dequeue();
			nextStep = new Vector2I(next.Item2 * TileMapController.tileSize.X + TileMapController.tileSize.X / 2,
				next.Item1 * TileMapController.tileSize.Y + TileMapController.tileSize.Y / 2);
		}

		private void MoveToTile()
		{
			if (Position.Equals(nextStep))
			{
				if (pathList.Count > 1)
				{
					ChangeStep();
				}
				else
				{
					if (GoingOnBase == true)
					{
						timeout = 100;
						TileMapController.createNewHouse = true;
						MakeNewPath(4);
						GoingOnBase = false;
						return;
					}
					else
					{

						timeout = 600;
						TileMapController.removeCells.Enqueue(pathList.Dequeue());
						MakeNewPath(5);
						GoingOnBase = true;
						return;
					}
				}
			}
			Position = Position.MoveToward(nextStep, 1);
		}
	}
}
