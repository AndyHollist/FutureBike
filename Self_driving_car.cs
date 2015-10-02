using UnityEngine;
using System.Collections;

//future bike

public class Self_driving_car : MonoBehaviour {

	public GameObject Sensor_front_right;
	public GameObject Sensor_front_left;
	public GameObject Sensor_front_bumper;


	public GameObject Wheel_FR;
	public GameObject Wheel_FL;
	public GameObject Wheel_BR;
	public GameObject Wheel_BL;

	private WheelCollider FR;
	private WheelCollider FL;
	private WheelCollider BR;
	private WheelCollider BL;

	public Vector3 com;
	public float MaxSpeed = 10f;

	// Use this for initialization
	void Start () {
		FR = Wheel_FR.GetComponent<WheelCollider>();
		FL = Wheel_FL.GetComponent<WheelCollider>();
		BR = Wheel_BR.GetComponent<WheelCollider>();
		BL = Wheel_BL.GetComponent<WheelCollider>();

		Rigidbody rb = GetComponent<Rigidbody>();
		rb.centerOfMass = com;

	}

	//have a seperate sensor only checking for lanes and controlling steering
	//or have a seperate counter that only detects lane intersections

	void check_sensors(){



		Bumper_sensor bumper_sensor = Sensor_front_bumper.GetComponent<Bumper_sensor>();
		int bumper_triggers = bumper_sensor.trigger_count;
		
		Bumper_sensor left_sensor = Sensor_front_left.GetComponent<Bumper_sensor>();
		int left_triggers = left_sensor.trigger_count;
		
		Bumper_sensor right_sensor = Sensor_front_right.GetComponent<Bumper_sensor>();
		int right_triggers = right_sensor.trigger_count;

		//good to go
		if(bumper_triggers == 0)
		{
			FR.motorTorque = -30;
			FL.motorTorque = -30;
			BR.motorTorque = -30;
			BL.motorTorque = -30;
			
			FR.brakeTorque = 0;
			FL.brakeTorque = 0;
			BR.brakeTorque = 0;
			BL.brakeTorque = 0;

			//brake ahead. slow down
			if( right_triggers > 0 && left_triggers > 0)
			{
				FL.steerAngle = 0;
				FR.steerAngle = 0;
				
				FR.motorTorque = 0;
				FL.motorTorque = 0;
				BR.motorTorque = 0;
				BL.motorTorque = 0;
				
				FR.brakeTorque = 100;
				FL.brakeTorque = 100;
				BR.brakeTorque = 100;
				BL.brakeTorque = 100;
			}

		}
		//stop
		else if(bumper_triggers > 0)
		{
			FR.motorTorque = 0;
			FL.motorTorque = 0;
			BR.motorTorque = 0;
			BL.motorTorque = 0;
			
			FR.brakeTorque = 100;
			FL.brakeTorque = 100;
			BR.brakeTorque = 100;
			BL.brakeTorque = 100;
		}
		//turn right
		if(left_triggers > 0 && right_triggers == 0)
		{
			FL.steerAngle = 30;
			FR.steerAngle = 30;
			//print ("turning right");
		}
		//turn left
		else if(right_triggers > 0 && left_triggers == 0)
		{
			FL.steerAngle = -30;
			FR.steerAngle = -30;
			//print ("turning left");
		}
		//no steer
		else if( right_triggers == 0 && left_triggers == 0)
		{
			FL.steerAngle = 0;
			FR.steerAngle = 0;

		}


	}
	
	// Update is called once per frame
	void Update () {
		check_sensors();

		float speed = GetComponent<Rigidbody>().velocity.sqrMagnitude;
		if( speed > MaxSpeed){
			FR.motorTorque = 0;
			FL.motorTorque = 0;
			BR.motorTorque = 0;
			BL.motorTorque = 0;
		}
	}
}
