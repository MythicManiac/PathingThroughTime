using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mythic.Zilean;

public class GridTest : MonoBehaviour
{
	public float cellSize = 1;
	public int gridSize = 1;
	public float duration = 8;
	public float timeStep = 1f;
	public float predictionResolution = 0.5f;

	public bool debug = true;
	public bool drawGridEmpty = false;
	public bool drawGridFull = true;
	public bool drawGridBounds = true;
	public bool drawPredictions = true;
	public bool drawGridPredictions = true;

	private PredictionGrid _grid;

    void Start()
    {
		_grid = new PredictionGrid(
			cellSize, gridSize, duration, timeStep, new Vector2(
				transform.position.x, transform.position.z
			)
		);
    }

	void FixedUpdate()
	{
		if (_grid == null)
			_grid = new PredictionGrid(
				cellSize, gridSize, duration, timeStep, new Vector2(
					transform.position.x, transform.position.z
				)
			);

		_grid.RebuildGrid(
			cellSize, gridSize, duration, timeStep, new Vector2(
				transform.position.x, transform.position.z
			));

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
	}

	void OnDrawGizmos()
	{
		if(_grid != null && debug)
			_grid.DrawGizmos(
				drawGridFull, drawGridEmpty, drawGridBounds,
				drawPredictions, drawGridPredictions
			);
	}
}
