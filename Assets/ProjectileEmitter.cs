using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEmitter : MonoBehaviour
{
	public float spawnInterval = 2f;
	public float spawnRadius = 15f;

	private float timeSinceLastSpawn = 0f;

	public GameObject[] projectilePrefabs;

    public void Update()
    {
		timeSinceLastSpawn += Time.fixedDeltaTime;

		if (timeSinceLastSpawn > spawnInterval)
		{
			SpawnProjectile();
			timeSinceLastSpawn -= spawnInterval;
		}
    }

	public void SpawnProjectile()
	{
		if (projectilePrefabs.Length == 0)
			return;

		var choice = (int)(Random.value * projectilePrefabs.Length - 1);
		var prefab = projectilePrefabs[choice];

		var spawnPoint = (int)(Mathf.PI * spawnRadius * Random.value);
		var x = Mathf.Sin(spawnPoint) * spawnRadius;
		var z = Mathf.Cos(spawnPoint) * spawnRadius;
		GameObject.Instantiate(prefab, new Vector3(x, 1, z), Quaternion.identity);
	}
}
