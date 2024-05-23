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
			if (pathList == null)
			{
				GD.PrintErr("Path list is null");
				return;
			}
			
			if (pathList.Count == 0)
			{
				if (!full)
				{
					(int j, int i) pathTree = FindNearestTree();
					pathList = TileMovement.FindShortestPath(TileMapController.getFieldMap(), ((int)((Position.Y - TileMapController.tileSize.Y / 2) / TileMapController.tileSize.Y), 
						(int)((Position.X - TileMapController.tileSize.X / 2) / TileMapController.tileSize.X)), (pathTree.i, pathTree.j), new List<int> { 0 });
				}
				else
				{
					pathList = TileMovement.FindShortestPath(TileMapController.getFieldMap(), ((int)((Position.Y - TileMapController.tileSize.Y / 2) / TileMapController.tileSize.Y),
						(int)((Position.X - TileMapController.tileSize.X / 2) / TileMapController.tileSize.X)), 
						((sawmill.Y - TileMapController.tileSize.Y / 2) / TileMapController.tileSize.Y,
						(sawmill.X - TileMapController.tileSize.X / 2) / TileMapController.tileSize.X), new List<int> { 0 });
				}
				nextStep = new Vector2I(pathList[0].Item2 * TileMapController.tileSize.X + TileMapController.tileSize.X / 2, 
					pathList[0].Item1 * TileMapController.tileSize.Y + TileMapController.tileSize.Y / 2);
				pathList.RemoveAt(0);
			}

			MoveToTile();
		}

		private void MoveToTile()
		{
			if (Position == nextStep)
			{
				if (pathList.Count > 1) 
				{
					nextStep = new Vector2I(pathList[0].Item2 * TileMapController.tileSize.X + TileMapController.tileSize.X / 2,
											pathList[0].Item1 * TileMapController.tileSize.Y + TileMapController.tileSize.Y / 2);
					pathList.RemoveAt(0);
				}
				else
				{
					TileMapController.RemoveEventTileCell((pathList[0].Item1, pathList[0].Item2));
					pathList.Clear();
					full = !full;
					return;
				}
			}

			Position = Position.MoveToward(nextStep, 2);
		}

		private (int x, int y) FindNearestTree()
		{
			(int j, int i) currentPosWorker = (
				(int)((Position.X - TileMapController.tileSize.X / 2) / TileMapController.tileSize.X),
				(int)((Position.Y - TileMapController.tileSize.Y / 2) / TileMapController.tileSize.Y)
			);

			int[,] map = TileMapController.getEventMap();
			(int j, int i) nearestTree = (-1, -1);
			int nearestDistance = int.MaxValue;

			for (int i = 0; i < map.GetLength(0); ++i)
			{
				for (int j = 0; j < map.GetLength(1); ++j)
				{
					if (map[i, j] == 3)
					{
						int distance = Math.Abs(currentPosWorker.j - j) + Math.Abs(currentPosWorker.i - i);
						if (distance < nearestDistance)
						{
							nearestTree = (j, i);
							nearestDistance = distance;
						}
					}
				}
			}

			return nearestTree;
		}
	}
}
