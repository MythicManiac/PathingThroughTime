using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mythic.Zilean
{
	public interface IPredictionProjectile
	{
		int GetLerpLength(float time, float resolution);
		Vector2 GetPosition(float time);
		bool DoesOverlapWithCell(Vector3 own, Vector3 other, Vector3 cellSize);
		float BoundingBoxWidth { get; }
	}

	public class Collision
	{
		public static bool DoesSphereCollideWithCube(
			Vector3 sPos, float sRadius,
			Vector3 cPos, Vector3 cSize
		)
		{
			// This actually ignores the Y/height axis, as that's time in our
			// case and doesn't follow the same rules

			var cRadius = cSize / 2;
			if (!(Mathf.Abs(sPos.x - cPos.x) <= sRadius + cRadius.x &&
				//Mathf.Abs(sPos.y - cPos.y) <= sRadius + cRadius.y &&
				Mathf.Abs(sPos.z - cPos.z) <= sRadius + cRadius.z
			))
				return false;

			var corner = new Vector3(
				Mathf.Max(
					cPos.x - cRadius.x, Mathf.Min(sPos.x, cPos.x + cRadius.z)),
				0,
				//Mathf.Max(
				//	cPos.y - cRadius.y, Mathf.Min(sPos.y, cPos.y + cRadius.y)),
				Mathf.Max(
					cPos.z - cRadius.z, Mathf.Min(sPos.z, cPos.z + cRadius.z))
			);
			var sphere = new Vector3(sPos.x, 0, sPos.z);
			return Vector3.Distance(corner, sphere) <= sRadius;
		}
	}

	public class StaticProjectilePrediction: IPredictionProjectile
	{
		public Vector2 Origin { get; private set; }
		public float Radius { get; private set; }
		
		public StaticProjectilePrediction(Vector2 origin, float radius)
		{
			Origin = origin;
			Radius = radius;
		}

		public float BoundingBoxWidth { get { return Radius * 2; } }

		public int GetLerpLength(float time, float resolution)
		{
			return 0;
		}

		public Vector2 GetPosition(float time)
		{
			return Origin;
		}

		public bool DoesOverlapWithCell(
			Vector3 projectile, Vector3 cell, Vector3 cellSize)
		{
			return Collision.DoesSphereCollideWithCube(
				projectile, Radius, cell, cellSize
			);
		}
	}

	public class LinearProjectilePrediction: IPredictionProjectile
	{
		public Vector2 Origin { get; private set; }
		public Vector2 Direction { get; private set; }
		public float Speed { get; private set; }
		public float Radius { get; private set; }

		public LinearProjectilePrediction(
			Vector2 origin, Vector2 direction, float speed,
			float radius)
		{
			Origin = origin;
			Direction = direction;
			Speed = speed;
			Radius = radius;
		}

		public float BoundingBoxWidth { get { return Radius * 2; } }

		public int GetLerpLength(float time, float resolution)
		{
			var _finalPosition = GetPosition(time);
			var distance = Vector2.Distance(_finalPosition, Origin);
			return Mathf.CeilToInt(distance / resolution);
		}

		public Vector2 GetPosition(float time)
		{
			return Speed * Direction * time + Origin;
		}

		public bool DoesOverlapWithCell(
			Vector3 projectile, Vector3 cell, Vector3 cellSize)
		{
			return Collision.DoesSphereCollideWithCube(
				projectile, Radius, cell, cellSize
			);
		}
	}

	public class WaveProjectilePrediction: IPredictionProjectile
	{
		public Vector2 Origin { get; private set; }
		public float TravelDirection { get; private set; }
		public float WaveDuration { get; private set; }
		public float WaveAmplitude { get; private set; }
		public float WaveLength { get; private set; }
		public float ElapsedTime { get; private set; }
		public float Radius { get; private set; }

		public WaveProjectilePrediction(
			Vector2 origin, Vector2 direction, float elapsedTime,
			float waveDuration, float waveAmplitude, float waveLength,
			float radius)
		{
			Origin = origin;
			ElapsedTime = elapsedTime;

			var relativeDirection = (direction - origin);
			TravelDirection = Mathf.Atan2(
				relativeDirection.y,
				relativeDirection.x
			);
			WaveDuration = waveDuration;
			WaveAmplitude = waveAmplitude;
			WaveLength = waveLength;
			Radius = radius;
		}

		public float BoundingBoxWidth { get { return Radius * 2; } }

		public int GetLerpLength(float time, float resolution)
		{
			var points = Mathf.PI * 2 * WaveAmplitude * WaveLength / resolution;
			return Mathf.CeilToInt(time / WaveDuration * points);
		}

		public Vector2 GetPosition(float time)
		{
			time = time + ElapsedTime;
			var progress = time / WaveDuration * Mathf.PI * 2;
			var wave = new Vector2(
				progress * WaveLength,
				Mathf.Sin(progress) * WaveAmplitude
			);
			var distance = Vector2.Distance(Vector2.zero, wave);
			var angle = Mathf.Atan2(wave.y, wave.x);
			var direction = TravelDirection + angle;
			var result = new Vector2(
				Mathf.Cos(direction) * distance + Origin.x,
				Mathf.Sin(direction) * distance + Origin.y
			);
			return result;
		}

		public bool DoesOverlapWithCell(Vector3 projectile, Vector3 cell, Vector3 cellSize)
		{
			return Collision.DoesSphereCollideWithCube(
				projectile, Radius, cell, cellSize
			);
		}
	}
}
