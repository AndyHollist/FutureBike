using UnityEngine;
using System.Collections;

public class HoverBike : MonoBehaviour {

	WheelCollider FR, FL, BR, BL;
	WheelCollider[] wheelColliders;
	public GameObject bikeModel;
	GameObject[] gameObjects;
	enum tilt_state {normal, left, right};//tilting the bike left and right
	tilt_state current_tilt;
	Rigidbody rb;
	public float thrust;

	// Use this for initialization
	void Start () {
		wheelColliders = GetComponentsInChildren<WheelCollider>();
		foreach (WheelCollider wc in wheelColliders){
			if( wc.name == "Wheel_FR")FR = wc;
			else if(wc.name == "Wheel_FL")FL = wc;
			else if(wc.name == "Wheel_BR")BR = wc;
			else if(wc.name == "Wheel_BL")BL = wc;
		}
		current_tilt = tilt_state.normal;
		rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		update_steering();
		update_acceleration();
		update_thrust();
	}

	public void FixedUpdate(){

	}

	// thrust upwards 
	public void update_thrust(){
		if(Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.T) ){
			rb.AddForce(transform.up * thrust);
			print ("adding thrust");
		}
	}

	public void update_acceleration(){
		if(Input.GetKey(KeyCode.W)){
			//gas
			FR.motorTorque = 100;
			FL.motorTorque = 100;
			BR.motorTorque = 100;
			BL.motorTorque = 100;
		}
		else if(Input.GetKey(KeyCode.S)){
			//brake
			FR.brakeTorque = 100;
			FL.brakeTorque = 100;
			BR.brakeTorque = 100;
			BL.brakeTorque = 100;
		}
		else{
			FR.motorTorque = 0;
			FL.motorTorque = 0;
			BR.motorTorque = 0;
			BL.motorTorque = 0;
			
			FR.brakeTorque = 0;
			FL.brakeTorque = 0;
			BR.brakeTorque = 0;
			BL.brakeTorque = 0;
		}
	}

	public void update_steering(){
		//steer right
		if(Input.GetKey(KeyCode.D)){
			FR.steerAngle = 30;
			FL.steerAngle = 30;
			if(current_tilt != tilt_state.right){
				if(current_tilt == tilt_state.left){
					bikeModel.transform.Rotate(new Vector3(0,0,-20));
				}
				bikeModel.transform.Rotate(new Vector3(0,0,-20));
				current_tilt = tilt_state.right;
			}
		}
		//steer left
		else if(Input.GetKey(KeyCode.A)){
			FR.steerAngle = -30;
			FL.steerAngle = -30;
			if(current_tilt != tilt_state.left){
				if(current_tilt == tilt_state.right){
					bikeModel.transform.Rotate(new Vector3(0,0,20));
				}
				bikeModel.transform.Rotate(new Vector3(0,0,20));
				current_tilt = tilt_state.left;
			}
		}
		//steer straight
		else{
			FR.steerAngle = 0;
			FL.steerAngle = 0;
			if(current_tilt != tilt_state.normal){
				if(current_tilt == tilt_state.left)
					bikeModel.transform.Rotate(new Vector3(0,0,-20));
				if(current_tilt == tilt_state.right)
					bikeModel.transform.Rotate(new Vector3(0,0,20));
				current_tilt = tilt_state.normal;
			}
		}

	}
}
