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
	private List<Vector3> _pathNodes = new List<Vector3>();
	private int _pathIndex;
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
	}

	void FixedUpdate()
	{
		RefreshGrid();
		if (updatePath)
			RefreshPath();
		Move();
	}

	private void Move()
	{
		if (_pathNodes.Count <= 0 || _pathIndex >= _pathNodes.Count)
		{
			Debug.Log("No path!");
			return;
		}

		var remainingDistance = pathMovementSpeed * Time.fixedDeltaTime;

		while (remainingDistance > 0)
		{
			var node = _pathNodes[_pathIndex];
			var waypointZeroed = new Vector3(node.x, 0, node.z);
			var positionZeroed = new Vector3(transform.position.x, 0, transform.position.z);
			var direction = (waypointZeroed - positionZeroed).normalized;
			var distance = Vector3.Distance(waypointZeroed, positionZeroed);
			var movementDistance = remainingDistance;
			if (distance <= remainingDistance)
			{
				_pathIndex++;
				movementDistance = distance;
				remainingDistance -= movementDistance;
			}
			else
			{
				remainingDistance = 0;
			}
			var movement = direction * movementDistance;
			transform.position += movement;
		}
	}

	private void RefreshPath()
	{
		_timeSinceLastPathRefresh += Time.fixedDeltaTime;

		if (_timeSinceLastPathRefresh < pathRefreshRate)
			return;

		_target = new GridCoords(
			gridSize,
			_grid.TimeStepCount - 1,
			gridSize
		);
		_start = _grid.GlobalCoordsToGrid(
			new Vector2(transform.position.x, transform.position.z),
			0
		);
		_path = _grid.FindPath(_start, _target, pathMovementSpeed);
		var pathNodes = new List<Vector3>();
		var current = _path;
		while(current != null)
		{
			pathNodes.Add(_grid.GridCoordsToGlobal(current.pos));
			current = current.parent;
		}

		if(pathNodes.Count > 0)
		{
			pathNodes.Reverse();
			_pathNodes = pathNodes;
			_pathIndex = 0;
		}

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
