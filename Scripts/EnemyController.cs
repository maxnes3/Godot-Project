using Godot;
using System;
using System.Collections.Generic;

namespace BlindedSoulsBuild.Scripts
{
	public partial class EnemyController : Node2D
	{
		private Queue<(int, int)> pathList;
		private Vector2I nextStep;
		private bool onTheWay;

		public override void _Ready()
		{
			onTheWay = false;
		}

		public override void _Process(double delta)
		{
			if (!onTheWay)
			{
				(int i, int j) currentTile = ((int)(Position.Y - TileMapController.tileSize.Y / 2) / TileMapController.tileSize.Y,
					(int)(Position.X - TileMapController.tileSize.X / 2) / TileMapController.tileSize.X);
				(int i, int j) target = TileMovement.FindTarget(TileMapController.getEventMap(), currentTile, 8, (-1, -1));
				pathList = new Queue<(int, int)>(TileMovement.FindShortestPath(TileMapController.getFieldMap(), currentTile, target, new List<int> { 1 }));
				ChangeStep();
				onTheWay = true;
			}
			MoveToTile();
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
					var removedCell = pathList.Dequeue();
					TileMapController.removeCells.Enqueue(removedCell);
					if (TileMapController.getEventMap()[removedCell.Item1, removedCell.Item2] != 0)
						QueueFree();
					onTheWay = false;
					return;
				}
			}
			Position = Position.MoveToward(nextStep, 1);
		}
	}
}
