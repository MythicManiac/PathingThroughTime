using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mythic.Zilean
{
	public struct GridCoords
	{
		public int x;
		public int t;
		public int y;
	}

	public class PredictionGrid
	{
		public const float PROJECTILE_SIZE = 1.0f;

		public float CellSize { get; private set; }
		public int GridSize { get; private set; }
		public float Duration { get; private set; }
		public float TimeStep { get; private set; }
		public Vector2 GlobalPosition { get; private set; }

		// Caches
		public float Width { get; private set; }
		public float Height { get; private set; }
		public float GlobalY { get; private set; }
		public int TimeStepCount { get; private set; }
		public Vector3 CellSize3D { get; private set; }

		// X, Time, Y
		bool[,,] _grid = new bool[0, 0, 0];
		Vector3[,,] _globalCoords = new Vector3[0, 0, 0];

		private List<Vector3> _predictions = new List<Vector3>();
		private List<GridCoords> _gridPredictions = new List<GridCoords>();

		public int GridSideLength
		{
			get { return GridSize * 2 + 1; }
		}

		public float Size
		{
			get { return GridSideLength * CellSize; }
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
			RebuildGrid(cellSize, gridSize, duration, timeStep, globalPosition);
		}

		public void RebuildGrid(float cellSize, int gridSize, float duration,
			float timeStep, Vector2 globalPosition)
		{
			CellSize = cellSize;
			GridSize = gridSize;
			Duration = duration;
			TimeStep = timeStep;
			GlobalPosition = globalPosition;
			TimeStepCount = Mathf.RoundToInt(Duration / TimeStep);
			Height = TimeStep * TimeStepCount;
			Width = GridSideLength * CellSize;
			// TODO: Define height better in general.
			// Right now we sort of assume everything to have a "height" (time dimension)
			// equivalent to their actual hitbox height, even though that's irrelevant
			// as 2d objects would not have a hitbox height. Thus, everything visual in
			// the Y axis actually represents time, and our projectiles should really be
			// cylinders or something, I'm not entirely sure either how the time stuff
			// works.
			GlobalY = Height / 2 - TimeStep / 2;
			CellSize3D = new Vector3(CellSize, TimeStep, CellSize);
			RecreateGrid();

			_globalCoords = new Vector3[GridSideLength, TimeStepCount, GridSideLength];
			for (var x = 0; x < _grid.GetLength(0); x++)
				for (var t = 0; t < _grid.GetLength(1); t++)
					for (var y = 0; y < _grid.GetLength(2); y++)
					{
						_globalCoords[x, t, y] = GridCoordsToGlobal(
							x, t, y
						);
					}
		}

		public void RecreateGrid()
		{
			_grid = new bool[GridSideLength, TimeStepCount, GridSideLength];
			_predictions = new List<Vector3>();
			_gridPredictions = new List<GridCoords>();
		}

		public bool IsWithinBounds(GridCoords coords)
		{
			return IsWithinBounds(coords.x, coords.t, coords.y);
		}

		public bool IsWithinBounds(int x, int t, int y)
		{
			return !(
				x < 0 || t < 0 || y < 0 ||
				x >= _grid.GetLength(0) ||
				t >= _grid.GetLength(1) ||
				y >= _grid.GetLength(2)
			);
		}

		public bool IsWithinCheckBounds(Vector3 pos)
		{
			return true;
			var distance = new Vector3(
				Mathf.Abs(GlobalPosition.x - pos.x),
				Mathf.Abs(GlobalY - pos.y),
				Mathf.Abs(GlobalPosition.y - pos.z)
			);
			var collisionDistXZ = (Width + CellSize) / 2;
			var collisionDistY = (Height + TimeStep) / 2;
			if (distance.x <= collisionDistXZ &&
				distance.y <= collisionDistY &&
				distance.z <= collisionDistXZ
			)
				return true;
			return false;
		}

		public Vector3 GridCoordsToGlobal(GridCoords coords)
		{
			return GridCoordsToGlobal(coords.x, coords.t, coords.y);
		}

		public Vector3 GridCoordsToGlobal(int x, int t, int y)
		{
			return new Vector3(
				(x * CellSize) + (GlobalPosition.x - Size / 2) + (CellSize / 2),
				(t * TimeStep),
				(y * CellSize) + (GlobalPosition.y - Size / 2) + (CellSize / 2)
			);
		}

		public GridCoords GlobalCoordsToGrid(Vector2 position, float time)
		{
			return new GridCoords() {
				x = Mathf.RoundToInt(
					position.x - (GlobalPosition.x - Size / 2) - (CellSize / 2)),
				t = Mathf.RoundToInt(time / TimeStep),
				y = Mathf.RoundToInt(
					position.y - (GlobalPosition.y - Size / 2) - (CellSize / 2))
			};
		}

		public void LerpMapToGrid(
			IPredictionProjectile predictor, float duration, float resolution,
			bool debug)
		{
			var predictionCount = predictor.GetLerpLength(duration, resolution);
			for (var i = 0; i < predictionCount; i++)
			{
				var time = (float)i / predictionCount * duration;
				MapToGrid(predictor, time, debug);
			}
		}

		public void MapToGrid(IPredictionProjectile projectile, float time,
			bool debug=false)
		{
			var position2d = projectile.GetPosition(time);
			var position = new Vector3(position2d.x, time, position2d.y);
			var gridPosition = GlobalCoordsToGrid(position2d, time);

			if(debug)
			{
				_predictions.Add(position);

				if(IsWithinBounds(gridPosition))
					_gridPredictions.Add(gridPosition);
			}

			if (!IsWithinCheckBounds(position))
				return;

			var areaCheckPadding = Mathf.CeilToInt(AreaCheckSize / 2.0f);
			var timeCheckPadding = Mathf.CeilToInt(TimeCheckSize / 2.0f);

			var minX = gridPosition.x - areaCheckPadding;
			var maxX = gridPosition.x + areaCheckPadding;
			var minY = gridPosition.y - areaCheckPadding;
			var maxY = gridPosition.y + areaCheckPadding;
			var minTime = gridPosition.t - timeCheckPadding;
			var maxTime = gridPosition.t + timeCheckPadding;

			for (var x = minX; x <= maxX; x++)
				for (var t = minTime; t <= maxTime; t++)
					for (var y = minY; y <= maxY; y++)
					{
						if (!IsWithinBounds(x, t, y))
							continue;

						if (_grid[x, t, y])
							continue;

						var coords = _globalCoords[x, t, y];
						var result = projectile.DoesOverlapWithCell(
							position, coords, CellSize3D
						);

						_grid[x, t, y] = result;
					}
		}

		public void DrawGizmos(
			bool drawGridFull, bool drawGridEmpty, bool drawGridBounds,
			bool drawPredictions, bool drawGridPredictions)
		{
			if (drawGridFull || drawGridEmpty)
				DrawGrid(drawGridFull, drawGridEmpty);
			if (drawGridBounds)
				DrawGridBounds();
			if (drawPredictions)
				DrawPredictions();
			if (drawGridPredictions)
				DrawGridPredictions();
		}

		public void DrawGridBounds()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(
				new Vector3(GlobalPosition.x, GlobalY, GlobalPosition.y),
				new Vector3(Width, Height, Width)
			);
		}

		public void DrawGridPredictions()
		{
			Gizmos.color = Color.red;
			foreach (var prediction in _gridPredictions)
			{
				Gizmos.DrawCube(
					_globalCoords[prediction.x, prediction.t, prediction.y],
					new Vector3(CellSize, TimeStep, CellSize)
				);
			}
		}

		public void DrawPredictions()
		{
			Gizmos.color = Color.red;
			foreach (var prediction in _predictions)
			{
				Gizmos.DrawWireSphere(prediction, 0.5f);
			}
		}

		public void DrawGrid(bool drawGridFull, bool drawGridEmpty)
		{
			Gizmos.color = Color.black;
			var size = new Vector3(
				CellSize, TimeStep, CellSize
			);

			for (var x = 0; x < _grid.GetLength(0); x++)
				for (var t = 0; t < _grid.GetLength(1); t++)
					for (var y = 0; y < _grid.GetLength(2); y++)
					{
						if (_grid[x, t, y] && drawGridFull)
							Gizmos.DrawCube(_globalCoords[x, t, y], size);
						else if (drawGridEmpty)
							Gizmos.DrawWireCube(_globalCoords[x, t, y], size);
					}
		}
	}
}
