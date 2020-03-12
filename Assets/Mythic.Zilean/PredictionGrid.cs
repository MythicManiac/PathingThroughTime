using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mythic.Zilean
{
	public class PredictionGrid
	{
		public float CellSize { get; private set; }
		public int GridSize { get; private set; }
		public float Duration { get; private set; }
		public float TimeStep { get; private set; }
		public float VisualHeightScale { get; private set; }
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

		public PredictionGrid(
			float cellSize, int gridSize, float duration, float timeStep,
			Vector2 globalPosition, float visualHeightScale)
		{
			RebuildGrid(cellSize, gridSize, duration,
				timeStep, globalPosition, visualHeightScale);
		}

		public void RebuildGrid(float cellSize, int gridSize, float duration,
			float timeStep, Vector2 globalPosition, float visualHeightScale)
		{
			var allMatch = (
				cellSize == CellSize &&
				gridSize == GridSize &&
				duration == Duration &&
				timeStep == TimeStep &&
				globalPosition == GlobalPosition &&
				visualHeightScale == VisualHeightScale
			);

			if (allMatch)
			{
				RecreateGrid();
				return;
			}

			CellSize = cellSize;
			GridSize = gridSize;
			Duration = duration;
			TimeStep = timeStep;
			GlobalPosition = globalPosition;
			TimeStepCount = Mathf.RoundToInt(Duration / TimeStep);
			Height = TimeStep * TimeStepCount;
			Width = GridSideLength * CellSize;
			GlobalY = Height / 2 - TimeStep / 2;
			CellSize3D = new Vector3(CellSize, TimeStep, CellSize);
			VisualHeightScale = visualHeightScale;
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
			return Utils.IsWithinBounds(in _grid, coords);
		}

		public bool IsWithinBounds(int x, int t, int y)
		{
			return Utils.IsWithinBounds(in _grid, x, t, y);
		}

		public bool IsWithinCheckBounds(Vector3 pos, float boundingBoxWidth)
		{
			var distance = new Vector3(
				Mathf.Abs(GlobalPosition.x - pos.x),
				Mathf.Abs(GlobalY - pos.y),
				Mathf.Abs(GlobalPosition.y - pos.z)
			);
			var collisionDistXZ = (Width + CellSize) / 2 + boundingBoxWidth / 2;
			var collisionDistY = (Height + TimeStep) / 2 + boundingBoxWidth / 2;
			if (distance.x <= collisionDistXZ &&
				distance.y <= collisionDistY &&
				distance.z <= collisionDistXZ
			)
				return true;
			return false;
		}

		public SpacetimePathFindNode FindPath(
			GridCoords source, GridCoords target, float movementSpeed)
		{
			return SpacetimePathFinder.FindPath(
				_grid, source, target, CellSize, TimeStep, movementSpeed
			);
		}

		public Vector3 GridCoordsToGlobal(GridCoords coords)
		{
			return GridCoordsToGlobal(coords.X, coords.T, coords.Y);
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
			return new GridCoords(
				Mathf.RoundToInt(
					(position.x - (GlobalPosition.x - Size / 2) - (CellSize / 2))
					/ CellSize),
				Mathf.RoundToInt(time / TimeStep),
				Mathf.RoundToInt(
					(position.y - (GlobalPosition.y - Size / 2) - (CellSize / 2))
					/ CellSize)
			);
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

			if (!IsWithinCheckBounds(position, projectile.BoundingBoxWidth))
				return;

			var projectileCellSize = projectile.BoundingBoxWidth / CellSize;
			var areaCheckPadding = Mathf.CeilToInt(
				projectileCellSize / 2.0f
			);

			// TODO: Figure out how this time padding stuff should be actually done
			// instead of relying on projectile bounds. Bounds might also technically
			// be a good idea, I really don't know.
			//var projectileTimeSize = projectile.BoundingBoxWidth / TimeStep;
			//var timeCheckPadding = Mathf.CeilToInt(
			//	projectileTimeSize / 2.0f
			//);
			var timeCheckPadding = 0;

			var minX = gridPosition.X - areaCheckPadding;
			var maxX = gridPosition.X + areaCheckPadding;
			var minY = gridPosition.Y - areaCheckPadding;
			var maxY = gridPosition.Y + areaCheckPadding;
			var minTime = gridPosition.T - timeCheckPadding;
			var maxTime = gridPosition.T + timeCheckPadding;

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

		public Vector3 ScaleVisual(Vector3 vector)
		{
			return new Vector3(
				vector.x,
				vector.y * VisualHeightScale,
				vector.z
			);
		}

		public void DrawGridBounds()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(
				ScaleVisual(new Vector3(GlobalPosition.x, GlobalY, GlobalPosition.y)),
				ScaleVisual(new Vector3(Width, Height, Width))
			);
		}

		public void DrawGridPredictions()
		{
			Gizmos.color = Color.red;
			var size = ScaleVisual(new Vector3(CellSize, TimeStep, CellSize));
			foreach (var prediction in _gridPredictions)
			{
				Gizmos.DrawCube(
					ScaleVisual(_globalCoords[prediction.X, prediction.T, prediction.Y]),
					size
				);
			}
		}

		public void DrawPredictions()
		{
			Gizmos.color = Color.red;
			foreach (var prediction in _predictions)
			{
				Gizmos.DrawWireSphere(ScaleVisual(prediction), 0.5f);
			}
		}

		public void DrawGrid(bool drawGridFull, bool drawGridEmpty)
		{
			Gizmos.color = Color.yellow;
			var size = ScaleVisual(new Vector3(CellSize, TimeStep, CellSize));

			for (var x = 0; x < _grid.GetLength(0); x++)
				for (var t = 0; t < _grid.GetLength(1); t++)
					for (var y = 0; y < _grid.GetLength(2); y++)
					{
						if (_grid[x, t, y] && drawGridFull)
							Gizmos.DrawCube(ScaleVisual(_globalCoords[x, t, y]), size);
						if (!_grid[x, t, y] && drawGridEmpty)
							Gizmos.DrawWireCube(ScaleVisual(_globalCoords[x, t, y]), size);
					}
		}
	}
}
