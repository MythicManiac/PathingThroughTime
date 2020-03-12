using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mythic.Zilean;

public class LinearProjectile : Projectile, IPredictorEnabled
{
	public float speed;
	public float lifetime;

	public override void SetProperties(Vector3 target, float speed, float lifetime)
	{
		Direction = (target - transform.position).normalized;
		Direction = new Vector3(Direction.x, 0, Direction.z);
		this.speed = speed;
		this.lifetime = lifetime;
	}

	public Vector3 Direction { get; private set; }

	void Start()
    {
		if (Direction == null)
			SetProperties(Vector3.zero, speed, lifetime);
    }

    void FixedUpdate()
    {
		transform.position += Direction * speed * Time.fixedDeltaTime;
		lifetime -= Time.fixedDeltaTime;
		if(lifetime <= 0)
		{
			Destroy(gameObject);
		}
    }

	public IPredictionProjectile GetPrediction()
	{
		return new LinearProjectilePrediction(
			origin: new Vector2(
				transform.position.x,
				transform.position.z
			),
			direction: new Vector2(
				Direction.x,
				Direction.z
			),
			speed: speed,
			radius: 0.5f
		);
	}
}
