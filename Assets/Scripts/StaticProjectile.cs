using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mythic.Zilean;

public interface IPredictorEnabled
{
	IPredictionProjectile GetPrediction();
}

public class StaticProjectile : MonoBehaviour, IPredictorEnabled
{
	public float lifetime;

	public IPredictionProjectile GetPrediction()
	{
		return new StaticProjectilePrediction(
			new Vector2(transform.position.x, transform.position.z),
			radius: 0.5f
		);
	}
	
    private void FixedUpdate()
    {
		lifetime -= Time.fixedDeltaTime;
		if (lifetime <= 0)
			Destroy(gameObject);
	}
}
