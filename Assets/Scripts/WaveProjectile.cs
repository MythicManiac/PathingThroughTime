﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mythic.Zilean;

public class WaveProjectile : Projectile, IPredictorEnabled
{
	public float lifetime;
	public float timeToCompleteWave = 1;
	public float amplitude = 1;
	public float length = 1;
	public float angle;
	
	private float _starTime = 0;
	private Vector2 _startPosition;
	private Vector2 _direction;

	public override void SetProperties(Vector3 target, float speed, float lifetime)
	{
		var ownPos = new Vector2(transform.position.x, transform.position.z);
		var targetPos = new Vector2(target.x, target.z);
		var direction = targetPos - ownPos;
		_direction = direction.normalized;
		angle = Mathf.Atan2(direction.y, direction.x);

		timeToCompleteWave = Mathf.PI * 2 / speed;
		this.lifetime = lifetime;
	}

	private void Start()
	{
		_starTime = Time.fixedTime;
		_startPosition = new Vector2(
			transform.position.x,
			transform.position.z
		);
		if(_direction == null)
		{
			_direction = -_startPosition.normalized;
			angle = Mathf.Atan2(-_startPosition.y, -_startPosition.x);
		}
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

	public IPredictionProjectile GetPrediction()
	{
		return new WaveProjectilePrediction(
			origin: _startPosition,
			angle: angle,
			elapsedTime: ElapsedTime,
			waveDuration: timeToCompleteWave,
			waveAmplitude: amplitude,
			waveLength: length,
			radius: 0.5f
		);
	}
}
