using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mythic.Zilean
{
	public class FuturePredictionGrid
	{
		public const float PROJECTILE_SIZE = 1.0f;

		public float CellSize { get; private set; }
		public int GridSize { get; private set; }
		public float Duration { get; private set; }
		public float TimeStep { get; private set; }
		public Vector2 GlobalPosition { get; private set; }

		// X, Time, Y
		bool[,,] _grid;

		public int TimeStepCount
		{
			get { return Mathf.RoundToInt(Duration / TimeStep); }
		}

		public int GridSideLength
		{
			get { return GridSize * 2 + 1; }
		}

		public FuturePredictionGrid(
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

		public void MapToGrid(IPredictionProjectile projectile, float time)
		{
			var position = projectile.GetPosition(time);
			var closestX = Mathf.RoundToInt(position.x);
			var closestY = Mathf.RoundToInt(position.y);
		}
	}
}
