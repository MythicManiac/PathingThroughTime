using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mythic.Zilean
{
	public struct GridCoords
	{
		public readonly int X;
		public readonly int T;
		public readonly int Y;

		public GridCoords(int x, int t, int y)
		{
			X = x;
			T = t;
			Y = y;
		}

		public override bool Equals(object other)
		{
			if (other.GetType() != typeof(GridCoords))
				return false;
			var cast = (GridCoords)other;
			return (X == cast.X && T == cast.T && Y == cast.Y);
		}

		public static bool operator ==(GridCoords a, GridCoords b)
			=> Equals(a, b);

		public static bool operator !=(GridCoords a, GridCoords b)
			=> !Equals(a, b);

		public override int GetHashCode() => 
			Tuple.Create(X, T, Y).GetHashCode();

		public float Distance(GridCoords other)
		{
			return Mathf.Sqrt(
				Mathf.Pow(X - other.X, 2) +
				Mathf.Pow(T - other.T, 2) +
				Mathf.Pow(Y - other.Y, 2)
			);
		}

		public Vector3 ToVector3()
		{
			return new Vector3(X, T, Y);
		}
	}

	public static class Utils
	{
		public static bool IsWithinBounds<T>(in T[,,] grid, GridCoords coords)
		{
			return IsWithinBounds(grid, coords.X, coords.T, coords.Y);
		}

		public static bool IsWithinBounds<T>(in T[,,] grid, int x, int t, int y)
		{
			return !(
				x < 0 || t < 0 || y < 0 ||
				x >= grid.GetLength(0) ||
				t >= grid.GetLength(1) ||
				y >= grid.GetLength(2)
			);
		}
	}

	public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey: IComparable
	{
		public int Compare(TKey x, TKey y)
		{
			int result = x.CompareTo(y);
			if (result == 0)
				return 1;
			return result;
		}
	}
}
