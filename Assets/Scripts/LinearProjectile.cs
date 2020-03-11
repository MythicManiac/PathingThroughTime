using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mythic.Zilean;

public class LinearProjectile : MonoBehaviour
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
			Debug.Log(Direction);
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

	private void OnDrawGizmos()
	{
		return;
		var predictor = new LinearProjectilePrediction(
			origin: new Vector2(
				transform.position.x,
				transform.position.z
			),
			direction: new Vector2(
				Direction.x,
				Direction.z
			),
			speed: speed
		);
		var duration = 2;
		var predictionCount = predictor.GetLerpLength(duration, 1);

		Gizmos.color = Color.blue;
		for (var i = 0; i < predictionCount; i++)
		{
			var time = i / (float)predictionCount * duration;
			var prediction = predictor.GetPosition(time);
			Gizmos.DrawWireSphere(
				new Vector3(prediction.x, time * 10, prediction.y), 1 / Mathf.PI * 2
			);
		}
	}
}
