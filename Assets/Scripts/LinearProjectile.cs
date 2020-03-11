using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mythic.Zilean;

public class LinearProjectile : MonoBehaviour, IPredictorEnabled
{
	public float speed;
	public float lifetime;

	public Vector3 Direction { get; private set; }

	void Start()
    {
		var player = GameObject.FindGameObjectWithTag("Player");
		if(player)
		{
			Direction = (player.transform.position - transform.position).normalized;
			Direction = new Vector3(Direction.x, 0, Direction.z);
		}
		else
		{
			Direction = (-transform.position).normalized;
			Direction = new Vector3(Direction.x, 0, Direction.z);
		}
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
