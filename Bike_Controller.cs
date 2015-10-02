using UnityEngine;
using System.Collections;

//future bike


public class Bike_Controller : MonoBehaviour {

	public float max_speed = 50;
	public GameObject front_left_wheel;
	public GameObject front_right_wheel;

	private Rigidbody rb;
	private WheelCollider[] wheel_cols;

	private bool forward = false;
	private bool backward = false;
	private bool stopped = true;

	private float anti_roll_max = 3500;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
		wheel_cols = GetComponentsInChildren<WheelCollider>();


	}

	float avgRPM()
	{
		float total = 0f;
		float count = 0;
		foreach( WheelCollider wc in wheel_cols)
		{
			count++;
			total += wc.rpm;
		}
		return total / count;
	}
	
	// Update is called once per frame
	void Update () {


		
		WheelCollider fl = front_left_wheel.GetComponent<WheelCollider>();
		WheelCollider fr = front_right_wheel.GetComponent<WheelCollider>();

		//Vector3 speed = fl.attachedRigidbody.angularVelocity;
		Vector3 speed = GetComponent<Rigidbody>().velocity;
		float speed_mag = speed.magnitude;
		/*
		print ("x val: " + speed.x);
		print ("y val: " + speed.y);
		print ("z val: " + speed.z);
		*/

		if(speed_mag < 0.01){
			stopped = true;
			forward = false;
			backward = false;
		}

		WheelHit hit;
		//Input.GetAxis("LeftJoystickX")
		//drive forward
		if(Input.GetKey("w") || (Input.GetAxis("RightTrigger") > 0.5) )
		{
			//moving forward
			if( forward || stopped)
			{
				foreach( WheelCollider wc in wheel_cols)
				{
					wc.motorTorque = -120;
					wc.brakeTorque = 0;
				}
				//print ("here!");
				forward = true;
				stopped = false;
				backward = false;
			}
			//moving backward
			else if( stopped || backward)
			{
				foreach( WheelCollider wc in wheel_cols)
				{
					wc.motorTorque = 0;
					wc.brakeTorque = 150;
				}
				stopped = false;
				backward = true;
				forward = false;
			}



			//rb.AddForce(transform.forward  * -1 * 10);
			//accelerate
			//transform.Translate( Vector3.forward * -1 * Time.deltaTime * 10);
		}

		//get avg rpm for all wheels
		//float rpm = avgRPM();
		//brake 
		else if( (Input.GetKey("s") || (Input.GetAxis("LeftTrigger") > 0.5)) )
		{
			//moving forward
			if( forward)
			{//transform.Translate( Vector3.forward * 1 * Time.deltaTime * 10);
				foreach( WheelCollider wc in wheel_cols)
				{
					wc.brakeTorque = 150;
					wc.motorTorque = 0;
				}
				forward = true;
				stopped = false;
				backward = false;
			}
			//moving backward
			else if( stopped || backward)
			{
				foreach( WheelCollider wc in wheel_cols)
				{
					wc.brakeTorque = 0;
					wc.motorTorque = 100;
				}
				stopped = false;
				backward = true;
				forward = false;
			}
		}
		//nothing
		else{
			if( !stopped){
				foreach( WheelCollider wc in wheel_cols)
				{
					wc.motorTorque = 0;
					wc.brakeTorque = 0;
				}
			}
			else{
				foreach( WheelCollider wc in wheel_cols)
				{
					wc.motorTorque = 0;
					wc.brakeTorque = 300;
				}
			}
		}



		//turn right
		if(Input.GetKey("d") || (Input.GetAxis("LeftJoyStickXaxis") > 0.5 )  )
		{
			//transform.Rotate(Vector3.up * -1 * Time.deltaTime * 100);
			fl.steerAngle = 30;
			fr.steerAngle = 30;
		}
		//turn left
		else if(Input.GetKey("a") || (Input.GetAxis("LeftJoyStickXaxis") < -0.5 ) )
		{
			//transform.Rotate(Vector3.up * 1 * Time.deltaTime * 100);
			fl.steerAngle = -30;
			fr.steerAngle = -30;
		}
		//straight
		else
		{
			fl.steerAngle = 0;
			fr.steerAngle = 0;
		}


		//anti roll
		//caclulate suspension travel
		float travelL = 1.0f;
		bool groundedl = fl.GetGroundHit(out hit);
		if(groundedl){
			travelL = (-fl.transform.InverseTransformPoint(hit.point).y - fl.radius)
				/ fl.suspensionDistance;
		}
		float travelR = 1.0f;
		bool groundedr = fr.GetGroundHit(out hit);
		if(groundedl){
			travelR = (-fr.transform.InverseTransformPoint(hit.point).y - fr.radius)
				/ fr.suspensionDistance;
		}

		var anti_roll_force = (travelL - travelR) * anti_roll_max;

		if (groundedl){
			Vector3 dir = fl.transform.up * -anti_roll_force;
			GetComponent<Rigidbody>().AddForceAtPosition( dir.normalized, fl.transform.position); 
		}
		if (groundedr)
			GetComponent<Rigidbody>().AddForceAtPosition(fr.transform.up * anti_roll_force, fr.transform.position);

		//for testing

		/*if(forward) print ("Forward: T");
		else print("Forward: F");
		if(stopped) print ("stopped: T");
		else print("stopped: F");
		if(backward) print ("Backward: T");
		else print("Backward: F");
		*/
	}



}
