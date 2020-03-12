using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mythic.Zilean;

public class TestPathFinding : MonoBehaviour
{
	public int height;
	public int size;

	private bool[,,] _grid = new bool[0, 0, 0];
	private GridCoords _start;
	private GridCoords _end;
	private SpacetimePathFinder.PathFindNode _path;

    public void Start()
    {
		_grid = new bool[size, height, size];
		var center = Mathf.RoundToInt(size / 2);
		_start = new GridCoords(center, 0, center);
		_end = new GridCoords(center, height - 1, center);

		if (size > 0 && height > 0)
		{
			_path = SpacetimePathFinder.FindPath(
				in _grid, _start, _end, 1, 1, 1
			);
			Debug.Log(_path);
		}
    }

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		for (var x = 0; x < size; x++)
			for (var t = 0; t < height; t++)
				for (var y = 0; y < size; y++)
				{
					Gizmos.DrawWireCube(new Vector3(
						x, t, y
					), Vector3.one);
				}

		Gizmos.color = Color.red;
		var current = _path;
		while(current != null)
		{
			Gizmos.DrawCube(current.pos.ToVector3(), Vector3.one);
			current = _path.parent;
		}

		Gizmos.color = Color.cyan;
		Gizmos.DrawCube(_start.ToVector3(), Vector3.one);
		Gizmos.DrawCube(_end.ToVector3(), Vector3.one);
	}
}
