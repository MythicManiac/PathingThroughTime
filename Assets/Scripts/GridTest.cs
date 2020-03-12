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
	public float visualHeightscale = 1f;

	private PredictionGrid _grid;
	private float _elapsedTime;

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
		_elapsedTime += Time.fixedDeltaTime;

		if (_elapsedTime < timeStep)
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

		_elapsedTime -= timeStep;
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
