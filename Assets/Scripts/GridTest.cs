using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mythic.Zilean;

public class GridTest : MonoBehaviour
{
	public bool updatePath = true;
	public float cellSize = 1;
	public int gridSize = 1;
	public float duration = 8;
	public float timeStep = 1f;
	public float predictionResolution = 0.5f;
	public float pathMovementSpeed = 1f;

	public float gridRefreshRate = 1f;
	public float pathRefreshRate = 1f;

	public bool debug = true;
	public bool drawGridEmpty = false;
	public bool drawGridFull = true;
	public bool drawGridBounds = true;
	public bool drawPredictions = true;
	public bool drawGridPredictions = true;
	public bool drawPath = true;
	public float visualHeightscale = 1f;

	private PredictionGrid _grid;
	private SpacetimePathFindNode _path;
	private GridCoords _start;
	private GridCoords _target;
	private float _timeSinceLastGridRefresh;
	private float _timeSinceLastPathRefresh;

    void Start()
    {
		_grid = new PredictionGrid(
			cellSize, gridSize, duration, timeStep, new Vector2(
				transform.position.x, transform.position.z
			), visualHeightscale
		);
		_start = new GridCoords(
			0,
			0,
			0
		);
		_target = new GridCoords(
			_grid.GridSideLength,
			_grid.TimeStepCount - 1,
			_grid.GridSideLength
		);
	}

	void FixedUpdate()
	{
		RefreshGrid();
		if (updatePath)
			RefreshPath();
	}

	private void RefreshPath()
	{
		_timeSinceLastPathRefresh += Time.fixedDeltaTime;

		if (_timeSinceLastPathRefresh < pathRefreshRate)
			return;

		_target = new GridCoords(
			_grid.GridSideLength,
			_grid.TimeStepCount - 1,
			_grid.GridSideLength
		);
		_path = _grid.FindPath(_start, _target, pathMovementSpeed);
		//var current = _path;
		//while(current != null)
		//{
		//	if (current.pos.T == 0)
		//	{
		//		_start = current.pos;
		//		break;
		//	}
		//	current = current.parent;
		//}

		_timeSinceLastPathRefresh -= pathRefreshRate;
	}

	private void RefreshGrid()
	{
		_timeSinceLastGridRefresh += Time.fixedDeltaTime;

		if (_timeSinceLastGridRefresh < gridRefreshRate)
			return;

		if (_grid == null)
			_grid = new PredictionGrid(
				cellSize, gridSize, duration, timeStep, new Vector2(
					transform.position.x, transform.position.z
				), visualHeightscale
			);

		_grid.RebuildGrid(
			cellSize, gridSize, duration, timeStep, new Vector2(
				transform.position.x, transform.position.z
			), visualHeightscale);

		var projectileTypes = new IPredictorEnabled[][] {
			FindObjectsOfType<LinearProjectile>(),
			FindObjectsOfType<WaveProjectile>(),
			FindObjectsOfType<StaticProjectile>()
		};

		foreach(var type in projectileTypes)
		{
			foreach (var projectile in type)
			{
				var predictor = projectile.GetPrediction();
				_grid.LerpMapToGrid(
					predictor, duration, predictionResolution, debug
				);
			}
		}
		_timeSinceLastGridRefresh -= gridRefreshRate;
	}

	private void OnDrawGizmos()
	{
		if (_grid != null && debug)
		{
			_grid.DrawGizmos(
				drawGridFull, drawGridEmpty, drawGridBounds,
				drawPredictions, drawGridPredictions
			);

			if (drawPath)
				DrawPath();
		}
	}

	private void DrawPath()
	{
		Gizmos.color = Color.cyan;
		var current = _path;
		var size = _grid.ScaleVisual(
			new Vector3(_grid.CellSize, _grid.TimeStep, _grid.CellSize)
		);
		while (current != null)
		{
			Gizmos.DrawCube(
				_grid.ScaleVisual(_grid.GridCoordsToGlobal(current.pos)),
				size
			);
			current = current.parent;
		}
	}
}
