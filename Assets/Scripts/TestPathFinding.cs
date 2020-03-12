using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mythic.Zilean;
using UnityEditor;

public class TestPathFinding : MonoBehaviour
{
	public int height;
	public int size;

	[Range(0, 100)]
	public float obstacleFrequency;

	private bool[,,] _grid = new bool[0, 0, 0];
	private GridCoords _start;
	private GridCoords _end;
	private SpacetimePathFinder.PathFindNode _path;

	private void Start()
	{
		Rebuild();
	}

	public void Rebuild()
	{
		_grid = new bool[size, height, size];
		for (var x = 0; x < _grid.GetLength(0); x++)
			for (var t = 0; t < _grid.GetLength(1); t++)
				for (var y = 0; y < _grid.GetLength(2); y++)
				{
					if (Random.value * 100 < obstacleFrequency)
						_grid[x, t, y] = true;
				}

		var center = Mathf.RoundToInt(size / 2);
		_start = new GridCoords(0, 0, 0);
		_end = new GridCoords(size - 1, height - 1, size - 1);

		if (size > 0 && height > 0)
		{
			_path = SpacetimePathFinder.FindPath(
				_grid, _start, _end, 1, 1, 1
			);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		for (var x = 0; x < _grid.GetLength(0); x++)
			for (var t = 0; t < _grid.GetLength(1); t++)
				for (var y = 0; y < _grid.GetLength(2); y++)
				{
					if(_grid[x, t, y])
						Gizmos.DrawWireCube(new Vector3(
							x, t, y
						), Vector3.one);
				}

		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(
			new Vector3(size / 2f - 0.5f, height / 2f - 0.5f, size / 2f - 0.5f),
			new Vector3(size, height, size)
		);

		Gizmos.color = Color.red;
		var current = _path;

		while(current != null)
		{
			Gizmos.DrawCube(current.pos.ToVector3(), Vector3.one);
			current = current.parent;
		}

		Gizmos.color = Color.cyan;
		Gizmos.DrawCube(_start.ToVector3(), Vector3.one);
		Gizmos.DrawCube(_end.ToVector3(), Vector3.one);
	}
}

[CustomEditor(typeof(TestPathFinding))]
public class TestPathFindingEditor: Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("Refresh grid"))
			((TestPathFinding)target).Rebuild();
	}
}

