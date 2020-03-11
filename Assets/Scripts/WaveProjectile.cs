using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mythic.Zilean;

public class WaveProjectile : MonoBehaviour
{
	public float lifetime;
	public float timeToCompleteWave = 1;
	public float amplitude = 1;
	public float length = 1;
	public float angle;
	
	private float _starTime = 0;
	private Vector2 _startPosition;

	private void Start()
	{
		_starTime = Time.fixedTime;
		_startPosition = new Vector2(
			transform.position.x,
			transform.position.z
		);
		angle = Mathf.Atan2(-_startPosition.y, -_startPosition.x);
	}

	public float ElapsedTime
	{
		get { return Time.fixedTime - _starTime; }
	}


    private void FixedUpdate()
    {
		var _currentProgress = (
			ElapsedTime / timeToCompleteWave
			* Mathf.PI * 2
		);

		var wavePosition = new Vector2(
			_currentProgress * length,
			Mathf.Sin(_currentProgress) * amplitude
		);
		var distance = Vector2.Distance(Vector2.zero, wavePosition);

		var waveAngle = Mathf.Atan2(wavePosition.y, wavePosition.x);
		var direction = waveAngle + angle;

		transform.position = new Vector3(
			Mathf.Cos(direction) * distance + _startPosition.x,
			0,
			Mathf.Sin(direction) * distance + _startPosition.y
		);


		lifetime -= Time.fixedDeltaTime;
		if (lifetime <= 0)
		{
			Destroy(gameObject);
		}
	}

	public WaveProjectilePrediction GetPrediction()
	{
		return new WaveProjectilePrediction(
			origin: _startPosition,
			direction: Vector2.zero,
			waveDuration: timeToCompleteWave,
			waveAmplitude: amplitude,
			waveLength: length
		);
	}

	private void OnDrawGizmos()
	{
		var predictor = GetPrediction();
		var duration = 2;
		var predictionCount = predictor.GetLerpLength(duration, 1);

		Gizmos.color = Color.blue;
		for (var i = 0; i < predictionCount; i++)
		{
			var time = i / (float)predictionCount * duration;
			var prediction = predictor.GetPosition(ElapsedTime + time);
			Gizmos.DrawWireSphere(
				new Vector3(prediction.x, time, prediction.y), 1 / Mathf.PI * 2
			);
		}
	}
}
