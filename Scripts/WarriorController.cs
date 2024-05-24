using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace BlindedSoulsBuild.Scripts
{
	public partial class WarriorController : Node2D
	{
		private (int i, int j) Casern;
		private Queue<(int, int)> pathList;
		private Vector2I nextStep;
		private bool onTheWay;

		public override void _Ready()
		{
			onTheWay = false;
			Casern =((int)Position.Y / TileMapController.tileSize.Y,
				(int)(Position.X - TileMapController.tileSize.X / 2) / TileMapController.tileSize.X);
		}

		public override void _Process(double delta)
		{
			if (!onTheWay)
			{
				(int i, int j) currentTile = ((int)(Position.Y - TileMapController.tileSize.Y / 2) / TileMapController.tileSize.Y,
					(int)(Position.X - TileMapController.tileSize.X / 2) / TileMapController.tileSize.X);
				(int i, int j) target = TileMovement.FindTarget(TileMapController.getEventMap(), currentTile, 7, Casern);
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
					TileMapController.removeCells.Enqueue(pathList.Dequeue());
					onTheWay = false;
					return;
				}
			}
			Position = Position.MoveToward(nextStep, 1);
		}
	}
}
