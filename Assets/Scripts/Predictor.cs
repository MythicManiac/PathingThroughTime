using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GridPoint
{
	public Vector3 position;
	public float size;
	public bool hit = false;
	public bool render = true;
	public (int, int) gridIndex;

	public static readonly float HITBOX_PADDING = 0.0f;

	public GridPoint(Vector3 position, float size, (int, int) gridIndex)
	{
		this.position = position;
		this.size = size;
		this.gridIndex = gridIndex;
	}

	public Vector2 AsVector2
	{
		get { return new Vector2(position.x, position.z); }
	}

	public void CheckHit(Vector3 another, float anotherSize)
	{
		if (hit)
			return;
		var own = AsVector2;

		if (own.x > 5 || own.x < -5 || own.y > 5 || own.y < -5)
		{
			hit = true;
			render = false;
			return;
		}

		var their = new Vector2(another.x, another.z);
		var collisionDistance = anotherSize / 2 + size / 2 + HITBOX_PADDING;
		var collision = (
			Mathf.Abs(own.x - their.x) <= collisionDistance &&
			Mathf.Abs(own.y - their.y) <= collisionDistance
		);
		if (collision)
			hit = true;
	}

	public void DrawGizmos(Vector3 offset, bool drawEmpty=true)
	{
		if (!render)
			return;

		var shape = new Vector3(size, 1, size);
		if (hit)
			if (drawEmpty)
				Gizmos.DrawCube(position + offset, shape);
			else
				Gizmos.DrawWireCube(position + offset, shape);
		else if(drawEmpty)
			Gizmos.DrawWireCube(position + offset, shape);
	}
}


public class Predictor : MonoBehaviour
{
	public float stepInterval;
	public int stepCount;

	public int gridSquareRadius;
	public float gridCellSize;
	public bool drawEmpty;
	public float timeSinceLast;
	public float hitboxPadding;

	public bool drawPredictions = true;
	public bool drawGrid = true;
	public bool drawWaypoints = true;

	public GameObject predictionPrefab;
	private List<Vector3>[] predictions = new List<Vector3>[0];
	private GridPoint[][,] grid = new GridPoint[0][,];
	private List<GridPoint> path = new List<GridPoint>();
	private PlayerMovement movement;

	private void Start()
	{
		movement = GetComponent<PlayerMovement>();
	}

	private float MaximumDistance
	{
		get { return movement.speed * stepInterval; }
	}

	private IEnumerable<GridPoint> GetPossibleWaypoints(GridPoint[,] options, GridPoint currentPoint)
	{
		var candidates = new List<GridPoint>();
		var position = currentPoint.AsVector2;
		for(var x = 0; x < 3; x++)
			for(var z = 0; z < 3; z++)
			{
				var gx = currentPoint.gridIndex.Item1 - 1 + x;
				var gz = currentPoint.gridIndex.Item2 - 1 + z;
				if (gx < 0 || gx >= options.GetLength(0) || gz < 0 || gz > options.GetLength(1))
					continue;
				var option = options[gx, gz];
				if (option.hit)
					continue;
				var distance = Vector2.Distance(position, option.AsVector2);
				if (distance <= MaximumDistance)
					candidates.Add(option);
			}
		return candidates.OrderBy(x => Vector2.Distance(x.AsVector2, position));
	}

	class WaypointSearchState
	{
		public WaypointSearchState previousState;
		public GridPoint waypoint;
		public int depth;
	}

	private WaypointSearchState FindPath(WaypointSearchState state)
	{
		if (state.depth == grid.Length)
			return state;

		var currentGrid = grid[state.depth];
		var options = GetPossibleWaypoints(currentGrid, state.waypoint);
		foreach(var option in options)
		{
			var result = FindPath(new WaypointSearchState()
			{
				previousState = state,
				waypoint = option,
				depth = state.depth + 1,
			});
			if (result != null)
				return result;
		}
		return null;
	}

    void FixedUpdate()
    {
		if (timeSinceLast < stepInterval)
		{
			timeSinceLast += Time.deltaTime;
			return;
		}

		predictions = new List<Vector3>[stepCount];
		grid = new GridPoint[stepCount][,];
		var gridSize = gridSquareRadius * 2 + 1;
		var projectiles = FindObjectsOfType<LinearProjectile>();

		for (var i = 0; i < stepCount; i++)
		{
			grid[i] = new GridPoint[gridSize, gridSize];
			for (var gx = 0; gx < gridSize; gx++)
				for (var gz = 0; gz < gridSize; gz++)
				{
					var x = (gx - gridSquareRadius) * gridCellSize + transform.position.x;
					var z = (gz - gridSquareRadius) * gridCellSize + transform.position.z;
					grid[i][gx, gz] = new GridPoint(new Vector3(x, 1, z), gridCellSize, (gx, gz));
				}
		}

		for (var i = 0; i < stepCount; i++)
		{
			predictions[i] = new List<Vector3>(300);
			for(var j = 0; j < projectiles.Length; j++)
			{
				var projectile = projectiles[j];
				var predictionPosition = projectile.Direction * projectile.speed * stepInterval * i;
				var nextPredictionPosition = projectile.Direction * projectile.speed * stepInterval * (i + 1);
				predictionPosition = projectile.transform.position + predictionPosition;
				nextPredictionPosition = projectile.transform.position + nextPredictionPosition;

				var lerpCount = Mathf.CeilToInt(Vector3.Distance(predictionPosition, nextPredictionPosition));
				for (var t = 0; t < lerpCount; t++)
				{
					var pos = Vector3.Lerp(predictionPosition, nextPredictionPosition, (float)t / lerpCount);
					predictions[i].Add(pos);
					for (var gx = 0; gx < grid[i].GetLength(0); gx++)
						for (var gz = 0; gz < grid[i].GetLength(1); gz++)
						{
							grid[i][gx, gz].CheckHit(pos, 1);
							//// Time axis padding
							//if (i + 1 < grid.Length)
							//	grid[i + 1][gx, gz].CheckHit(pos, 1);
							//if (i - 1 >= 0)
							//	grid[i - 1][gx, gz].CheckHit(pos, 1);
						}
				}
			}
		}

		var pathFindResult = FindPath(new WaypointSearchState()
		{
			previousState = null,
			waypoint = grid[0][gridSquareRadius, gridSquareRadius],
			depth = 0,
		});
		if (pathFindResult == null)
		{
			Debug.Break();
		}
		
		if (pathFindResult != null)
		{
			path = new List<GridPoint>();
			var state = pathFindResult;
			while(state.previousState != null)
			{
				path.Add(state.waypoint);
				state = state.previousState;
			}
			var waypoint = path[path.Count - 1].position;
			movement.SetWaypoint(waypoint);
		}

		timeSinceLast = 0;
	}

	void OnDrawGizmos()
	{
		if (drawPredictions)
			DrawPredictions();
		if (drawGrid)
			DrawGrid();
		if (drawWaypoints)
			DrawWaypoints();
	}

	void DrawPredictions()
	{
		Gizmos.color = Color.yellow;
		for (var i = 0; i < predictions.Length; i++)
			foreach (var prediction in predictions[i])
				Gizmos.DrawWireCube(prediction + new Vector3(0, i, 0), new Vector3(1, 1, 1));
	}

	void DrawGrid()
	{
		Gizmos.color = Color.blue;
		for (var i = 0; i < grid.Length; i++)
			for (var gx = 0; gx < grid[i].GetLength(0); gx++)
				for (var gz = 0; gz < grid[i].GetLength(1); gz++)
				{
					grid[i][gx, gz].DrawGizmos(new Vector3(0, i, 0), drawEmpty);
				}
	}

	void DrawWaypoints()
	{
		Gizmos.color = Color.red;
		for (var i = 0; i < path.Count; i++)
		{
			Gizmos.DrawWireCube(
				path[i].position + new Vector3(0, grid.Length - i - 1, 0),
				new Vector3(1, 1, 1)
			);
		}
	}
}
