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
		bool DoesOverlap(float time, Vector3 point, float cellSize);
	}

	public class LinearProjectilePrediction: IPredictionProjectile
	{
		public Vector2 Origin { get; private set; }
		public Vector2 Direction { get; private set; }
		public float Speed { get; private set; }

		public LinearProjectilePrediction(
			Vector2 origin, Vector2 direction, float speed)
		{
			Origin = origin;
			Direction = direction;
			Speed = speed;
		}

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

		public bool DoesOverlap(float time, Vector3 point, float cellSize)
		{
			// TODO: Actually do collision check for circle agains a square
			// instead of just treating both as circles
			// TODO: Use different padding/cell size for the time axis
			var position = GetPosition(time);
			var vec3 = new Vector3(position.x, time, position.y);
			var collisionRadius = (
				PredictionGrid.PROJECTILE_SIZE / 2 + cellSize
			);
			return Vector3.Distance(vec3, point) <= collisionRadius;
		}
	}

	public class WaveProjectilePrediction: IPredictionProjectile
	{
		public Vector2 Origin { get; private set; }
		public float TravelDirection { get; private set; }
		public float WaveDuration { get; private set; }
		public float WaveAmplitude { get; private set; }
		public float WaveLength { get; private set; }

		public WaveProjectilePrediction(
			Vector2 origin, Vector2 direction,
			float waveDuration, float waveAmplitude, float waveLength)
		{
			Origin = origin;
			var relativeDirection = (direction - origin);
			TravelDirection = Mathf.Atan2(
				relativeDirection.y,
				relativeDirection.x
			);
			WaveDuration = waveDuration;
			WaveAmplitude = waveAmplitude;
			WaveLength = waveLength;
		}

		public int GetLerpLength(float time, float resolution)
		{
			var points = Mathf.PI * 2 * WaveAmplitude * WaveLength / resolution;
			return Mathf.CeilToInt(time / WaveDuration * points);
		}

		public Vector2 GetPosition(float time)
		{
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

		public bool DoesOverlap(float time, Vector3 point, float cellSize)
		{
			// TODO: Actually do collision check for circle agains a square
			// instead of just treating both as circles
			// TODO: Use different padding/cell size for the time axis
			var position = GetPosition(time);
			var vec3 = new Vector3(position.x, time, position.y);
			var collisionRadius = (
				PredictionGrid.PROJECTILE_SIZE / 2 + cellSize
			);
			return Vector3.Distance(vec3, point) <= collisionRadius;
		}
	}
}
