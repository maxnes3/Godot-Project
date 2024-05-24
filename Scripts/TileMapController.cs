using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace BlindedSoulsBuild.Scripts
{
	public partial class TileMapController : TileMap
	{

		private readonly int mapGridSize = 251; // The size of map (X * X), where X % 2 != 0
		private static int[,] matrixFieldMap; // The matrix of map index
		private static int[,] matrixEventMap;
		public static Vector2I tileSize { private set; get; }
		private readonly int centerCount = 6; // Count of zone center\
		private List<Vector2I> bases;
		private int radius;

		private readonly List<(int x, int y)> AtlasTiles = new List<(int x, int y)>
		{
			(0, 0), //Void - 0
			(2, 0), //Field - 1
			(5, 0), //Barrier - 2
			(6, 19), //Base - 3
			(1, 1), //Tree - 4
			(19, 10), //Sawmill - 5
			(3, 20), //Casern - 6
		};

		PackedScene Worker;
		PackedScene Warrior;

		// Action after spawn this object
		public override void _Ready()
		{
			int[,] matrix = {
				{ 1, 1, 0, 0 },
				{ 1, 1, 1, 0 },
				{ 0, 1, 0, 0 },
				{ 1, 1, 1, 1 }
			};

			(int i, int j) start = (0, 0);
			(int i, int j) end = (3, 3);
			List<int> walkableIndex = new List<int> { 1 };

			var path = TileMovement.FindShortestPath(matrix, start, end, walkableIndex);
			if (path != null)
			{
				foreach (var pos in path)
				{
					GD.PrintErr($"({pos.Item1}, {pos.Item2})");
				}
			}
			else
			{
				GD.PrintErr("No path found.");
			}

			tileSize = TileSet.TileSize;

			Worker = (PackedScene)ResourceLoader.Load("res://Prefabs//worker.tscn");
			Warrior = (PackedScene)ResourceLoader.Load("res://Prefabs//warrior.tscn");

			// Generate map on matrix
			matrixFieldMap = GenerateFieldMapMatrix();

			matrixEventMap = GenerateEventMapMatrix();	

			// Draw tiles on map
			DrawTileCells(matrixFieldMap, 0);

			DrawTileCells(matrixEventMap, 1, true);

			// Change TileMap position 
			Position -= new Vector2I(tileSize.X * (mapGridSize - 1) / 2 + tileSize.X / 2,
				tileSize.Y * (mapGridSize - 1) / 2 + tileSize.Y / 2);
		}

		// Furction return complete map
		private int[,] GenerateFieldMapMatrix()
		{
			Random rnd = new Random();
			int[,] newMatrixMap = new int[mapGridSize, mapGridSize];
			int[,] barrierMap;
			int barriersPercent = 40; //Percent from 0 to 100
			int baseRadius = rnd.Next(mapGridSize / 9, mapGridSize / 7);
			int borderOffest = rnd.Next(baseRadius + 1, mapGridSize / 7);

			// Base locations (x, y) - central points of each base
			List<Vector2I> baseCenters = new List<Vector2I>
			{
				new Vector2I(borderOffest, borderOffest), // Top-left corner
				new Vector2I(mapGridSize - borderOffest, borderOffest), // Top-right corner
				new Vector2I(borderOffest, mapGridSize - borderOffest), // Bottom-left corner
				new Vector2I(mapGridSize - borderOffest, mapGridSize - borderOffest), // Bottom-right corner
				new Vector2I(mapGridSize / 2, borderOffest), // Middle-top edge
				new Vector2I(mapGridSize / 2, mapGridSize - borderOffest), // Middle-bottom edge
				new Vector2I(borderOffest, mapGridSize / 2), // Middle-left edge
				new Vector2I(mapGridSize - borderOffest, mapGridSize / 2) // Middle-right edge
			};

			List<int> walkableIndex = new List<int>
			{
				0, 1
			};

			// Function to place a base
			void FillBlock(Vector2I center)
			{
				// Generate random radii for the ellipse
				int radiusX = rnd.Next(mapGridSize / 15, mapGridSize / 13);
				int radiusY = rnd.Next(mapGridSize / 15, mapGridSize / 13);

				for (int dx = -radiusX; dx <= radiusX; ++dx)
				{
					for (int dy = -radiusY; dy <= radiusY; ++dy)
					{
						int x = center.X + dx;
						int y = center.Y + dy;
						if (x >= 0 && x < mapGridSize && y >= 0 && y < mapGridSize)
						{
							// Check if the point is within the ellipse
							if ((dx * dx) * (radiusY * radiusY) + (dy * dy) * (radiusX * radiusX) <= (radiusX * radiusX) * (radiusY * radiusY))
							{
								newMatrixMap[x, y] = 1;
							}
						}
					}
				}
			}

			// Generate random barriers cell on the barrierMap
			void GenerateRandomBarrierMap(int count)
			{
				barrierMap = new int[mapGridSize, mapGridSize];

				int rows = barrierMap.GetLength(0);
				int cols = barrierMap.GetLength(1);

				for (int i = 0; i < count; i++)
				{
					int x, y;
					do
					{
						x = rnd.Next(rows);
						y = rnd.Next(cols);
					} while (barrierMap[x, y] != 0);
					barrierMap[x, y] = 2;
				}

				foreach (Vector2I baseCenter in baseCenters)
				{
					for (int di = -1; di <= 1; ++di)
					{
						for (int dj = -1; dj <= 1; ++dj)
						{
							int i = baseCenter.X + di;
							int j = baseCenter.Y + dj;
							barrierMap[i, j] = 0;
						}
					}
				}
			}

			bool CreateBasePaths()
			{
				HashSet<int> connectedBases = new HashSet<int>();

				for (int i = 0; i < baseCenters.Count; ++i)
				{
					int startBase, endBase;
					do
					{
						startBase = rnd.Next(0, baseCenters.Count);
						endBase = rnd.Next(0, baseCenters.Count);
					} while (startBase.Equals(endBase));

					connectedBases.Add(startBase);
					connectedBases.Add(endBase);

					if (!CreateBFSPath(baseCenters[startBase], baseCenters[endBase]))
					{
						newMatrixMap = new int[mapGridSize, mapGridSize];
						return false;
					}
				}

				for (int i = 0; i < baseCenters.Count; ++i)
				{
					if (!connectedBases.Contains(i))
					{
						int secondBase;

						do
						{
							secondBase = rnd.Next(0, baseCenters.Count);
						} while (i.Equals(secondBase));

						connectedBases.Add(i);
						connectedBases.Add(secondBase);

						if (!CreateBFSPath(baseCenters[i], baseCenters[secondBase]))
						{
							newMatrixMap = new int[mapGridSize, mapGridSize];
							return false;
						}
					}
				}

				return true;
			}

			// Function to create a path between two points
			bool CreateBFSPath(Vector2I start, Vector2I end)
			{
				List<(int, int)> shortestlPath = TileMovement.FindShortestPath(barrierMap, (start.X, start.Y), (end.X, end.Y), walkableIndex);
				if (shortestlPath != null)
				{
					foreach (var cell in shortestlPath)
					{
						newMatrixMap[cell.Item1, cell.Item2] = 1;
					}
					return true;
				}
				return false;
			}

			// Create paths between two bases on the map
			do
			{
				GenerateRandomBarrierMap(mapGridSize * mapGridSize * barriersPercent / 100);
			} while (!CreateBasePaths());

			for(int i = 0; i < barrierMap.GetLength(0); ++i)
			{
				for(int j = 0; j < barrierMap.GetLength(1); ++j)
				{
					newMatrixMap[i, j] = barrierMap[i, j] != 0 ? barrierMap[i, j] : newMatrixMap[i, j];
				}
			}

			// Place all bases on the map
			foreach (Vector2I baseCenter in baseCenters)
			{
				FillBlock(baseCenter);
			}

			bases = baseCenters;
			radius = baseRadius;

			return newMatrixMap;
		}

		private int[,] GenerateEventMapMatrix()
		{
			Random rnd = new Random();
			int[,] newMapMatrix = new int[mapGridSize, mapGridSize];

			foreach (Vector2I center in bases)
			{
				newMapMatrix[center.X, center.Y] = 3;
				int signX;
				int signY;

				do
				{
					signX = rnd.Next(-1, 2);
					signY = rnd.Next(-1, 2);
				} while(signX == 0 || signY == 0);

				(int x, int y) newBuilding = (center.X + rnd.Next(1, radius / 9) * signX,
					center.Y + rnd.Next(2, radius / 9) * signY);

				newMapMatrix[newBuilding.x, newBuilding.y] = 5;

				Node2D newWorker = (Node2D)Worker.Instantiate();
				newWorker.Position = new Vector2I(newBuilding.x * TileSet.TileSize.X + TileSet.TileSize.X / 2, 
					(newBuilding.y + 1) * TileSet.TileSize.Y + TileSet.TileSize.Y / 2);
				AddChild(newWorker);

				do
				{
					signX = rnd.Next(-1, 2);
					signY = rnd.Next(-1, 2);
					newBuilding = (center.X + rnd.Next(1, radius / 9) * signX,
						center.Y + rnd.Next(2, radius / 9) * signY);
				} while (signX == 0 || signY == 0 || newMapMatrix[newBuilding.x, newBuilding.y] == 5);

				newMapMatrix[newBuilding.x, newBuilding.y] = 6;

				Node2D newWarrior = (Node2D)Warrior.Instantiate();
				newWarrior.Position = new Vector2I(newBuilding.x * TileSet.TileSize.X + TileSet.TileSize.X / 2,
					(newBuilding.y + 1) * TileSet.TileSize.Y + TileSet.TileSize.Y / 2);
				AddChild(newWarrior);

				int a = (int)(radius * 0.5f);
				int b = (int)(radius * 0.5f);
				int h = center.X;
				int k = center.Y;

				for (double theta = 0; theta < Math.PI; theta += 0.01)
				{
					int x = (int)(h + a * Math.Cos(theta));
					int y = (int)(k + b * Math.Sin(theta));

					if (x >= 0 && x < mapGridSize && y >= 0 && y < mapGridSize)
					{
						newMapMatrix[x, y] = 4;
					}
				}
			}

			return newMapMatrix;
		}

		// Draw Matrix Tiles on TileMap
		private void DrawTileCells(int[,] matrix, int layer, bool ignoreZero = false)
		{
			for (int i = 0; i < mapGridSize; ++i)
			{
				for (int j = 0; j < mapGridSize; ++j)
				{
					if (ignoreZero && matrix[i, j] == 0)
						continue;
					var currentAtlasTile = AtlasTiles[matrix[i, j]];
					SetCell(layer, new Vector2I(i, j), 0, new Vector2I(currentAtlasTile.x, currentAtlasTile.y), 0);
				}
			}
		}

		public static int[,] getFieldMap()
		{
			return matrixFieldMap;
		}

		public static int[,] getEventMap()
		{
			return matrixEventMap;
		}
	}
}
