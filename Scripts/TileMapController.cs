using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

public partial class TileMapController : TileMap
{

	private readonly int mapGridSize = 251; // The size of map (X * X), where X % 2 != 0
	private int[,] matrixMap; // The matrix of map index
	private readonly int centerCount = 6; // Count of zone center

	// Action after spawn this object
	public override void _Ready()
	{
		// Generate map on matrix
		matrixMap = GenerateMapMatrix();

		// Draw tiles on map
		DrawTileCells(matrixMap, 0);

		// Change TileMap position 
		Position -= new Vector2I(TileSet.TileSize.X * (mapGridSize - 1) / 2 + TileSet.TileSize.X / 2,
			TileSet.TileSize.Y * (mapGridSize - 1) / 2 + TileSet.TileSize.Y / 2);
	}

	// Furction return complete map
	private int[,] GenerateMapMatrix()
	{
		Random rnd = new Random();
		int[,] newMatrixMap = new int[mapGridSize, mapGridSize];
		int[,] barrierMap;
		int barriersPercent = 40; //Percent from 0 to 100
		int pathPercentWidth = 33;
		int baseRadius = rnd.Next(mapGridSize / 15, mapGridSize / 13);
		int borderOffest = rnd.Next(baseRadius + 1, mapGridSize / 11);

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
				for (int dx = -1; dx <= 1; ++dx)
				{
					for (int dy = -1; dy <= 1; ++dy)
					{
						int x = baseCenter.X + dx;
						int y = baseCenter.Y + dy;
						barrierMap[x, y] = 0;
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
			List<(int, int)> shortestlPath = FindShortestPath((start.X, start.Y), (end.X, end.Y));
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

		List<(int, int)> FindShortestPath((int x, int y) start, (int x, int y) end)
		{
			int rows = barrierMap.GetLength(0);
			int cols = barrierMap.GetLength(1);

			var directions = new (int, int)[]
			{
				(0, 1), (1, 0), (0, -1), (-1, 0),  // right, down, left, up
			};

			Queue<List<(int, int)>> queue = new Queue<List<(int, int)>>();
			HashSet<(int, int)> visited = new HashSet<(int, int)>();

			queue.Enqueue(new List<(int, int)> { start });
			visited.Add(start);

			while (queue.Count > 0)
			{
				var path = queue.Dequeue();
				(int x, int y) current = path[path.Count - 1];

				if (current == end)
				{
					return path;
				}

				foreach (var direction in directions)
				{
					(int x, int y) next = (current.x + direction.Item1, current.y + direction.Item2);
					if (next.x >= 0 && next.x < rows && next.y >= 0 && next.y < cols &&
						!visited.Contains(next) && barrierMap[next.x, next.y] != 2)
					{
						var newPath = new List<(int, int)>(path) { next };
						queue.Enqueue(newPath);
						visited.Add(next);
					}
				}
			}
			return null;
		}

		// Create paths between two bases on the map
		do
		{
			GenerateRandomBarrierMap(mapGridSize * mapGridSize * barriersPercent / 100);
		} while (!CreateBasePaths());

		// Place all bases on the map
		foreach (Vector2I baseCenter in baseCenters)
		{
			FillBlock(baseCenter);
		}

		return newMatrixMap;
	}

	// Draw Matrix Tiles on TileMap
	private void DrawTileCells(int[,] matrix, int layer)
	{
		for (int x = 0; x < mapGridSize; ++x)
		{
			for (int y = 0; y < mapGridSize; ++y)
			{
				SetCell(layer, new Vector2I(x, y), matrix[x, y], new Vector2I(0, 0), 0);
			}
		}
	}
}
