using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mythic.Zilean
{
	public class SpacetimePathFindNode
	{
		public SpacetimePathFindNode parent;
		public GridCoords pos;
		public float distanceInLayer;
		public float distanceToStart;
		public float distanceToTarget;

		public float Cost
		{
			get { return (distanceToStart + distanceToTarget) * (pos.T + 1); }
		}
	}

	public static class SpacetimePathFinder
	{
		public static (int, int, int)[] PossibleNeighbours = new (int, int, int)[] {
			(-1, 0, 0),
			(1, 0, 0),
			(0, 0, -1),
			(0, 0, 1),
			(0, 1, 0)
		};

		public static List<GridCoords> GetReachableNeighbours(
			in bool[,,] grid, SpacetimePathFindNode current,
			Dictionary<GridCoords, SpacetimePathFindNode> evaluated,
			float movementSpeed, float cellSize, float cellTime
		)
		{
			var pos = current.pos;
			var result = new List<GridCoords>();

			foreach(var (x, t, y) in PossibleNeighbours)
			{
				var coords = new GridCoords(pos.X + x, pos.T + t, pos.Y + y);
				if (!Utils.IsWithinBounds(grid, coords))
					continue;

				// TODO: Move away from here in case
				// we want to just associate a higher
				// cost for going through projectiles
				if (grid[coords.X, coords.T, coords.Y])
					continue;

				if (evaluated.ContainsKey(coords))
					continue;

				var deltaDistance = current.pos.SpatialDistance(coords);
				var totalDistance = current.distanceInLayer + deltaDistance;
				var requiredSpeed = totalDistance * cellSize / cellTime;
				if (requiredSpeed >= movementSpeed)
					continue;

				result.Add(coords);
			}

			return result;
		}


		public static SpacetimePathFindNode FindPath(
			in bool[,,] grid, GridCoords start, GridCoords target,
			float cellSize, float cellTime, float movementSpeed,
			int maxIterations = 5000)
		{
			var evaluatedNodes = new Dictionary<GridCoords, SpacetimePathFindNode>();
			var pendingNodes = new SortedList<float, SpacetimePathFindNode>(
				new DuplicateKeyComparer<float>()
			);
			var pendingNodesByCoords = new Dictionary<GridCoords, SpacetimePathFindNode>();

			var startNode = new SpacetimePathFindNode()
			{
				parent = null,
				pos = start,
				distanceToStart = 0,
				distanceInLayer = 0,
				distanceToTarget = start.SpatialDistance(target)
			};

			pendingNodes.Add(startNode.Cost, startNode);
			pendingNodesByCoords.Add(startNode.pos, startNode);

			var currentStep = 0;

			SpacetimePathFindNode closest = null;

			while(pendingNodes.Count > 0 && currentStep < maxIterations)
			{
				var current = pendingNodes.First().Value;
				pendingNodes.RemoveAt(0);
				pendingNodesByCoords.Remove(current.pos);
				evaluatedNodes[current.pos] = current;

				if(current.pos == target)
					return current;
				else if (current.pos.T == target.T)
					if (
						closest == null ||
						current.distanceToTarget < closest.distanceToTarget
					)
						closest = current;

				var neighbours = GetReachableNeighbours(
					grid, current, evaluatedNodes,
					movementSpeed, cellSize, cellTime);

				foreach(var pos in neighbours)
				{
					var distanceDelta = current.pos.SpatialDistance(pos);
					var distanceInLayer = (
						current.distanceInLayer + distanceDelta
					);
					if (current.pos.T != pos.T)
						distanceInLayer = 0;

					var distanceToStartViaCurrent = (
						distanceDelta + current.distanceToStart
					);

					SpacetimePathFindNode neighbour = null;
					if (pendingNodesByCoords.ContainsKey(pos))
					{
						neighbour = pendingNodesByCoords[pos];
						if (distanceToStartViaCurrent < neighbour.distanceToStart)
						{
							neighbour.parent = current;
							neighbour.distanceToStart = distanceToStartViaCurrent;
							neighbour.distanceInLayer = distanceInLayer;
						}
						pendingNodes.RemoveAt(
							pendingNodes.IndexOfValue(neighbour)
						);
						pendingNodes.Add(neighbour.Cost, neighbour);
					}
					else
					{
						neighbour = new SpacetimePathFindNode()
						{
							parent = current,
							pos = pos,
							distanceInLayer = distanceInLayer,
							distanceToStart = distanceToStartViaCurrent,
							distanceToTarget = target.SpatialDistance(pos),
						};
						pendingNodes.Add(neighbour.Cost, neighbour);
						pendingNodesByCoords.Add(pos, neighbour);
					}
				}

				currentStep += 1;
			}

			return closest;
		}
	}
}
