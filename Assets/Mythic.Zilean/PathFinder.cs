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
		public float distanceToStart;
		public float distanceToTarget;

		public float Cost
		{
			get { return distanceToStart + distanceToTarget; }
		}
	}

	public static class SpacetimePathFinder
	{
		public static List<GridCoords> GetNeighbourNodes(
			in bool[,,] grid, GridCoords pos,
			Dictionary<GridCoords, SpacetimePathFindNode> evaluated
		)
		{
			var startX = pos.X - 1;
			var endX = pos.X + 1;
			var startY = pos.Y - 1;
			var endY = pos.Y + 1;

			var result = new List<GridCoords>();

			for (var x = startX; x <= endX; x++)
				for (var y = startY; y <= endY; y++)
				{
					if (!Utils.IsWithinBounds(grid, x, pos.T, y))
						continue;

					// TODO: Move away from here in case
					// we want to just associate a higher
					// cost for going through projectiles
					if (grid[x, pos.T, y])
						continue;

					var coords = new GridCoords(x, pos.T, y);
					if (evaluated.ContainsKey(coords))
						continue;

					result.Add(coords);
				}
		
			// Travel to next timestep
			var next = new GridCoords(pos.X, pos.T + 1, pos.Y);
			if (Utils.IsWithinBounds(grid, next) && !evaluated.ContainsKey(next))
			{
				// TODO: Move away from here in case
				// we want to just associate a higher
				// cost for going through projectiles
				if (!grid[next.X, next.T, next.Y])
				{
					result.Add(next);
				}
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
				distanceToTarget = start.Distance(target)
			};

			pendingNodes.Add(startNode.Cost, startNode);
			pendingNodesByCoords.Add(startNode.pos, startNode);

			var currentStep = 0;

			while(pendingNodes.Count > 0 && currentStep < maxIterations)
			{
				var current = pendingNodes.First().Value;
				pendingNodes.RemoveAt(0);
				pendingNodesByCoords.Remove(current.pos);
				evaluatedNodes[current.pos] = current;

				if(current.pos == target)
					return current;

				var neighbours = GetNeighbourNodes(
					grid, current.pos, evaluatedNodes);

				foreach(var pos in neighbours)
				{
					var distanceToStartViaCurrent = (
						current.pos.Distance(pos) + current.distanceToStart
					);

					SpacetimePathFindNode neighbour = null;
					if (pendingNodesByCoords.ContainsKey(pos))
					{
						neighbour = pendingNodesByCoords[pos];
						if (distanceToStartViaCurrent < neighbour.distanceToStart)
						{
							neighbour.parent = current;
							neighbour.distanceToStart = distanceToStartViaCurrent;
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
							distanceToStart = distanceToStartViaCurrent,
							distanceToTarget = target.Distance(pos),
						};
						pendingNodes.Add(neighbour.Cost, neighbour);
						pendingNodesByCoords.Add(pos, neighbour);
					}
				}

				currentStep += 1;
			}

			return null;
		}
	}
}
