using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Checkpoint : MonoBehaviour {

	public int id = 0;
	public int num_triggers = 0;
	public LapHandler laphandler;
	int num_checkpoints = 1;

	// Use this for initialization
	void Start () {
		num_checkpoints = laphandler.num_checkpoints;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other){
		num_triggers++;
	}

	void OnTriggerExit(Collider other){
		GameObject go = other.transform.parent.gameObject;
		Rigidbody rb = go.GetComponent<Rigidbody>();
		if(go.GetComponent<HoverObj5>() != null){
			HoverObj5 hover_bike = go.GetComponent<HoverObj5>();
			//check if moving in correct direction
			Vector3 velocity_xz = new Vector3(rb.velocity.x, 0 , rb.velocity.z);
			float angle_btw = Vector3.Angle(transform.right, velocity_xz);
			if(angle_btw < 90){//moving forward
				hover_bike.state = id;
				if(id == 0){//finish line
					hover_bike.lap++;
				}
			}
			else{//moving backward
				if(id == 0){//finishline
					hover_bike.lap--;
					hover_bike.state = num_checkpoints-1;
				}
				else{
					hover_bike.state = id-1;
				}
			}
		}
		else{
			RacerAI2 hover_bike = go.GetComponent<RacerAI2>();
			//check if moving in correct direction
			Vector3 velocity_xz = new Vector3(rb.velocity.x, 0 , rb.velocity.z);
			float angle_btw = Vector3.Angle(transform.right, velocity_xz);
			if(angle_btw < 90){//moving forward
				hover_bike.state = id;
				if(id == 0){//finish line
					hover_bike.lap++;
				}
			}
			else{//moving backward
				if(id == 0){//finishline
					hover_bike.lap--;
					hover_bike.state = num_checkpoints-1;
				}
				else{
					hover_bike.state = id-1;
				}
			}
		}
		num_triggers--;
	}

	//col.velocity is in line with transform.right (x+ direction)  -> num_fwd_collisions++
	//col.velocity is in line with transform.left (-x direction) -> num_bwd_collisitons++


	//state-1 -> gate0 -> state0 -> gate1 -> state1 -> gate2 -> state2 -> gate0 -> state0
}
