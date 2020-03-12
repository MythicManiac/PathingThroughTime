using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mythic.Zilean;

public interface IPredictorEnabled
{
	IPredictionProjectile GetPrediction();
}

public abstract class Projectile : MonoBehaviour
{
	public abstract void SetProperties(Vector3 target, float speed, float lifetime);
}

public class StaticProjectile : Projectile, IPredictorEnabled
{
	public float lifetime;

	public override void SetProperties(Vector3 target, float speed, float lifetime)
	{
		this.lifetime = lifetime;
	}

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
