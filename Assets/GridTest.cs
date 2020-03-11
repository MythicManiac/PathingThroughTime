using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mythic.Zilean;

public class GridTest : MonoBehaviour
{
	public float cellSize = 1;
	public int gridSize = 1;
	public float duration = 4;
	public float timeStep = 0.5f;

	private PredictionGrid _grid;

    void Start()
    {
		_grid = new PredictionGrid(
			cellSize, gridSize, duration, timeStep, new Vector2(
				transform.position.x, transform.position.y
			)
		);
		_grid.RecreateGrid();
    }

	void FixedUpdate()
	{
		if (_grid == null)
			return;

		_grid.RecreateGrid();

		var linears = FindObjectsOfType<LinearProjectile>();
		foreach(var linear in linears)
		{
			var predictor = linear.GetPrediction();
			var predictionCount = predictor.GetLerpLength(duration, 1);
			for (var i = 0; i < predictionCount; i++)
			{
				var time = i / predictionCount * duration;
				_grid.MapToGrid(predictor, time);
			}
		}

		var waves = FindObjectsOfType<WaveProjectile>();
		foreach (var wave in waves)
		{
			var predictor = wave.GetPrediction();
			var predictionCount = predictor.GetLerpLength(duration, 1);
			for (var i = 0; i < predictionCount; i++)
			{
				var time = i / predictionCount * duration;
				_grid.MapToGrid(predictor, time + wave.ElapsedTime);
			}
		}
	}

	void OnDrawGizmos()
	{
		if(_grid != null)
			_grid.DrawGizmos();
	}
}
