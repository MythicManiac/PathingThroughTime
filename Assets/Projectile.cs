using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public float speed;
	public float lifetime;

	public Vector3 Direction { get; private set; }

	void Start()
    {
		var player = GameObject.FindGameObjectWithTag("Player");
		Direction = (player.transform.position - transform.position).normalized;
		Direction = new Vector3(Direction.x, 0, Direction.z);
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
}
