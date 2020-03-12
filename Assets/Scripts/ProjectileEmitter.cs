using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEmitter : MonoBehaviour
{
	public float spawnInterval = 2f;
	public float spawnRadius = 15f;
	public int spawnAmount = 1;
	public bool randomSpawnAmount = false;

	public float projectileSpeed = 12f;
	public float projectileLifetime = 10f;

	private float timeSinceLastSpawn = 0f;

	public Projectile[] projectilePrefabs;

    public void FixedUpdate()
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
		if (spawnAmount == 0)
			return;

		var toSpawn = spawnAmount;

		if (randomSpawnAmount)
			toSpawn = Mathf.CeilToInt(Random.value * spawnAmount);

		var choice = Mathf.RoundToInt(Random.value * (projectilePrefabs.Length - 1));
		var prefab = projectilePrefabs[choice];

		var sectionSize = Mathf.Deg2Rad * 360 / toSpawn;
		var spawnOffset = Mathf.Deg2Rad * 360 * Random.value;

		for (var i = 0; i < toSpawn; i++)
		{
			var spawnPoint = spawnOffset + sectionSize * i;
			var x = Mathf.Cos(spawnPoint) * spawnRadius;
			var z = Mathf.Sin(spawnPoint) * spawnRadius;
			var proj = Instantiate(
				prefab,
				new Vector3(x + transform.position.x, 0, z + transform.position.z),
				Quaternion.identity
			);
			proj.SetProperties(transform.position, projectileSpeed, projectileLifetime);
		}
	}
}
