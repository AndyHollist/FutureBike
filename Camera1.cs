using UnityEngine;
using System.Collections;

//future bike

public class Camera1 : MonoBehaviour {

	public GameObject trueCenter;
	public GameObject target;
	public GameObject follower;
	public float radius;

	private Vector3 velocity = Vector3.zero;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		float axis = Input.GetAxis("RightJoyStickXaxis");
		axis = -axis;
		if( axis < 0.1 && axis > -0.1)
			transform.position = Vector3.SmoothDamp( transform.position, follower.transform.position,
		                                        ref velocity,0.1f);
		else{
			float angle = (45 * axis)*Mathf.Deg2Rad;
			float z = radius * Mathf.Cos(angle);
			float x = radius * Mathf.Sin(angle);
			Vector3 target_pos = target.transform.localPosition;
			Vector3 world_pos = target.transform.TransformPoint( target_pos.x + x, 0, target_pos.z + z);

			transform.position = new Vector3(world_pos.x, transform.position.y, world_pos.z);
			//transform.position.z = target.transform.localPosition.z - z;
		}
	}

	void LateUpdate() {
		transform.LookAt(trueCenter.transform);
	}
}
