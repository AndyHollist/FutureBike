using UnityEngine;
using System.Collections;

//future bike

public class Camera2 : MonoBehaviour {

	public GameObject trueCenter;
	public GameObject target;
	public GameObject follower;
	public float radius;

	private Vector3 velocity = Vector3.zero;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float axis = Input.GetAxis("RightJoyStickXaxis");
		//axis = -axis;
		if( axis < 0.1 && axis > -0.1)// if the right stick is not touched
		{
			//transform.position = Vector3.SmoothDamp( transform.position, follower.transform.position, ref velocity,0.1f);
			transform.position = Vector3.Lerp(transform.position, follower.transform.position, 0.1f);
		}
		else{
			float angle = (45 * axis)*Mathf.Deg2Rad;
			print("Angle= " + angle);
			float z = radius * Mathf.Cos(angle);
			float x = radius * Mathf.Sin(angle);

			Vector3 target_pos = target.transform.localPosition;

			Vector3 goal_pos = new Vector3( x , 0,
			 -z);
			Vector3 world_pos = target.transform.TransformPoint(goal_pos);
			transform.position = new Vector3(world_pos.x, transform.position.y, world_pos.z);

			//Vector3 world_pos = target.transform.TransformPoint( target_pos.x, 0, target_pos.z);
			//float orig_y = transform.position.y;
			//transform.position = new Vector3(world_pos.x, orig_y, world_pos.z);
			//transform.position.z = target.transform.localPosition.z - z;
		}
	}

	void LateUpdate() {
		transform.LookAt(trueCenter.transform);
	}
}
