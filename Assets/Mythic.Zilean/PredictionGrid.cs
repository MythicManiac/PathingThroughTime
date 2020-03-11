using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mythic.Zilean
{
	public class PredictionGrid
	{
		public const float PROJECTILE_SIZE = 1.0f;

		public float CellSize { get; private set; }
		public int GridSize { get; private set; }
		public float Duration { get; private set; }
		public float TimeStep { get; private set; }
		public Vector2 GlobalPosition { get; private set; }

		// X, Time, Y
		bool[,,] _grid = new bool[0, 0, 0];

		public int TimeStepCount
		{
			get { return Mathf.RoundToInt(Duration / TimeStep); }
		}

		public int GridSideLength
		{
			get { return GridSize * 2 + 1; }
		}

		public float Size
		{
			get { return GridSideLength * CellSize; }
		}

		public float MaxCellRadius
		{
			get {
				return Vector2.Distance(
					Vector2.zero, new Vector2(CellSize, CellSize)
				);
			}
		}

		public int AreaCheckSize
		{
			get { return Mathf.CeilToInt(CellSize / PROJECTILE_SIZE); }
		}

		public int TimeCheckSize
		{
			get { return Mathf.CeilToInt(TimeStep / PROJECTILE_SIZE); }
		}

		public PredictionGrid(
			float cellSize, int gridSize, float duration, float timeStep,
			Vector2 globalPosition)
		{
			CellSize = cellSize;
			GridSize = gridSize;
			Duration = duration;
			TimeStep = timeStep;
			GlobalPosition = globalPosition;
		}

		public void RecreateGrid()
		{
			_grid = new bool[GridSideLength, TimeStepCount, GridSideLength];
		}

		public Vector3 GridCoordsToGlobal(int x, int t, int y)
		{
			return new Vector3(
				(x * CellSize) + (GlobalPosition.x - Size / 2) + (CellSize / 2),
				(t * TimeStep) - (TimeStep / 2),
				(y * CellSize) + (GlobalPosition.y - Size / 2) + (CellSize / 2)
			);
		}

		public Vector3 GlobalCoordsToGrid(Vector3 position, float time)
		{
			return new Vector3(
				position.x - (GlobalPosition.x - Size / 2) - (CellSize / 2),
				time,
				position.y - (GlobalPosition.y - Size / 2) - (CellSize / 2)
			);
		}

		public void MapToGrid(IPredictionProjectile projectile, float time)
		{
			var position = GlobalCoordsToGrid(projectile.GetPosition(time), time);
			var areaCheckPadding = Mathf.CeilToInt(AreaCheckSize / 2.0f);
			var timeCheckPadding = Mathf.CeilToInt(TimeCheckSize / 2.0f);

			var closestX = Mathf.RoundToInt(position.x);
			var minX = closestX - areaCheckPadding;
			var maxX = closestX + areaCheckPadding;
			var closestY = Mathf.RoundToInt(position.z);
			var minY = closestY - areaCheckPadding;
			var maxY = closestY + areaCheckPadding;
			var closestTime = Mathf.RoundToInt(time / TimeStep);
			var minTime = closestTime - timeCheckPadding;
			var maxTime = closestTime + timeCheckPadding;

			for (var x = minX; x <= maxX; x++)
				for (var t = minTime; t <= maxTime; t++)
					for (var y = minY; y <= maxY; y++)
					{
						if (
							x < 0 || t < 0 || y < 0 ||
							x >= _grid.GetLength(0) ||
							t >= _grid.GetLength(1) ||
							y >= _grid.GetLength(2)
						)
							continue;

						if (_grid[x, t, y])
							continue;

						_grid[x, t, y] = projectile.DoesOverlap(
							time,
							GridCoordsToGlobal(x, t, y),
							MaxCellRadius
						);
					}
		}

		public void DrawGizmos()
		{
			Gizmos.color = Color.black;
			var size = new Vector3(
				CellSize, TimeStep, CellSize
			);

			for (var x = 0; x < _grid.GetLength(0); x++)
				for (var t = 0; t < _grid.GetLength(1); t++)
					for (var y = 0; y < _grid.GetLength(2); y++)
					{
						if (_grid[x, t, y])
							Gizmos.DrawCube(
								GridCoordsToGlobal(x, t, y),
								size
							);
						else
							Gizmos.DrawWireCube(
								GridCoordsToGlobal(x, t, y),
								size
							);
					}
		}
	}
}
