using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float speed;

	private Vector3 waypoint;

	public void SetWaypoint(Vector3 newWaypoint)
	{
		waypoint = newWaypoint;
	}

    void FixedUpdate()
    {
		//var direction = Vector3.zero;
		//if (Input.GetKey("w"))
		//{
		//	direction += new Vector3(0, 0, 1);
		//}
		//if (Input.GetKey("s"))
		//{
		//	direction += new Vector3(0, 0, -1);
		//}
		//if (Input.GetKey("a"))
		//{
		//	direction += new Vector3(-1, 0, 0);
		//}
		//if (Input.GetKey("d"))
		//{
		//	direction += new Vector3(1, 0, 0);
		//}
		//var movement = direction.normalized;
		if(waypoint != null)
		{
			var waypointZeroed = new Vector3(waypoint.x, 0, waypoint.z);
			var positionZeroed = new Vector3(transform.position.x, 0, transform.position.z);
			var direction = (waypointZeroed - positionZeroed).normalized;
			var distance = Vector3.Distance(waypointZeroed, positionZeroed);
			var movementDistance = Mathf.Min(speed * Time.fixedDeltaTime, distance);
			var movement = direction * movementDistance;
			transform.position += movement;
		}
    }
}
