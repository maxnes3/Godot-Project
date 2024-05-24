using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlindedSoulsBuild.Scripts
{
	public static class TileMovement
	{
		public static List<(int, int)> FindShortestPath(int[,] matrix, (int i, int j) start, (int i, int j) end, List<int> walkableIndex)
		{
			int rows = matrix.GetLength(0);
			int cols = matrix.GetLength(1);

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
				(int i, int j) current = path[path.Count - 1];

				if (current == end)
				{
					return path;
				}

				foreach (var direction in directions)
				{
					(int i, int j) next = (current.i + direction.Item1, current.j + direction.Item2);
					if (next.i >= 0 && next.i < rows && next.j >= 0 && next.j < cols &&
						!visited.Contains(next) && walkableIndex.Contains(matrix[next.i, next.j]))
					{
						var newPath = new List<(int, int)>(path) { next };
						queue.Enqueue(newPath);
						visited.Add(next);
					}
				}
			}
			return null;
		}

		public static (int i, int j) FindTarget(int[,] matrix, (int i, int j) current, int targetIndex, (int i, int j) ignoreCell)
		{
			int minDistance = int.MaxValue;
			(int i, int j) target = (-1, -1);

			for (int i = 0; i < matrix.GetLength(0); ++i)
			{
				for (int j = 0; j < matrix.GetLength(1); ++j)
				{
					if (matrix[i, j].Equals(targetIndex) && 
						Math.Abs(current.i + current.j - i - j) < minDistance &&
						!(ignoreCell == (i, j)))
					{
						target = (i, j);
						minDistance = i + j;
					}
				}
			}

			return target;
		}
	}
}
