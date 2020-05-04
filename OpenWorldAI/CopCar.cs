using UnityEngine;
using System.Collections;

//future bike


/*
 * Cop car

Smart Reversing

Follow Player

Primitive Driving

Smart Turning / Lane Switching

*/

public class CopCar : MonoBehaviour {

	public GameObject audio_siren;

	public GameObject player1;
	public GameObject player_rigidbody;


	public GameObject Sensor_right;
	public GameObject Sensor_left;
	public GameObject Sensor_front_right;
	public GameObject Sensor_front_left;
	public GameObject Sensor_front_bumper;
	public GameObject Sensor_left_peripheral;
	public GameObject Sensor_right_peripheral;

	
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

	private float front_angle;
	private int steering_decision = 1;
	private float reversing = 0;

	public bool in_pursuit = false;
	public bool parked;

	public float torqueConstant = 1;

	private GameObject my_collider;

	private AudioSource sirens;
	
	// Use this for initialization
	void Start () {
		FR = Wheel_FR.GetComponent<WheelCollider>();
		FL = Wheel_FL.GetComponent<WheelCollider>();
		BR = Wheel_BR.GetComponent<WheelCollider>();
		BL = Wheel_BL.GetComponent<WheelCollider>();
		
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.centerOfMass = com;

		StartCoroutine(choose_steering_direction());

		BoxCollider[] child_objs = GetComponentsInChildren<BoxCollider>();
		foreach( BoxCollider obj in child_objs){
			if( obj.name == "Vehicle_Collider"){
				my_collider = obj.gameObject;
				print ("found the box collider!");
			}
		}

		sirens = audio_siren.GetComponent<AudioSource>();
		sirens.Stop();
	}

	IEnumerator choose_steering_direction(){
		for(;;){
			if(FR.steerAngle == 0){//make sure you are not already turning
				if(Random.value >= 0.5){
					steering_decision = 1; //turning right preference
				}
				else{
					steering_decision = -1; //turning left
				}
			}
			yield return new WaitForSeconds(1);
		}
	}

	//have a seperate sensor only checking for lanes and controlling steering
	//or have a seperate counter that only detects lane intersections
	
	void check_sensors(){
		
		
		//bumper
		Cop_Sensor bumper_sensor = Sensor_front_bumper.GetComponent<Cop_Sensor>();
		int bumper_triggers = bumper_sensor.trigger_count;

		//forward facing
		Cop_Sensor left_sensor = Sensor_front_left.GetComponent<Cop_Sensor>();
		int fr_left_triggers = left_sensor.trigger_count;
		Cop_Sensor right_sensor = Sensor_front_right.GetComponent<Cop_Sensor>();
		int fr_right_triggers = right_sensor.trigger_count;

		//sides
		Cop_Sensor right_side = Sensor_right.GetComponent<Cop_Sensor>();
		int right_triggers = right_side.trigger_count;
		Cop_Sensor left_side = Sensor_left.GetComponent<Cop_Sensor>();
		int left_triggers = left_side.trigger_count;

		//peripherals
		Cop_Sensor left_peripheral = Sensor_left_peripheral.GetComponent<Cop_Sensor>();
		bool left_periph = left_peripheral.trigger_count > 0;
		
		Cop_Sensor right_peripheral = Sensor_right_peripheral.GetComponent<Cop_Sensor>();
		bool right_periph = right_peripheral.trigger_count > 0;

		bool bumper = (bumper_triggers > 0);
		bool fr_right = fr_right_triggers > 0;
		bool fr_left = fr_left_triggers > 0;
		bool left = left_triggers > 0;
		bool right = right_triggers > 0;


		if(bumper)
		{
			//stop
			FR.motorTorque = 0;
			FL.motorTorque = 0;
			BR.motorTorque = 0;
			BL.motorTorque = 0;
			
			FR.brakeTorque = 100;
			FL.brakeTorque = 100;
			BR.brakeTorque = 100;
			BL.brakeTorque = 100;
			float speed = GetComponent<Rigidbody>().velocity.sqrMagnitude;
			if( speed < 0.01f && reversing <= 0){
				reversing = 3;
			}
		}

		else{
			if( fr_left && !fr_right){
				//turn right
				FL.steerAngle = 30;
				FR.steerAngle = 30;
			}
			else if( fr_right && !fr_left){
				//turn left
				FL.steerAngle = -30;
				FR.steerAngle = -30;
			}
			else if( fr_right && fr_left ){
				if( left && !right){
					//turn right
					FL.steerAngle = 30;
					FR.steerAngle = 30;
				}
				else if( right && !left ){
					//turn left
					FL.steerAngle = -30;
					FR.steerAngle = -30;
				}
				else if( right && left){
					//stop
					FR.motorTorque = 0;
					FL.motorTorque = 0;
					BR.motorTorque = 0;
					BL.motorTorque = 0;
					
					FR.brakeTorque = 100;
					FL.brakeTorque = 100;
					BR.brakeTorque = 100;
					BL.brakeTorque = 100;
				}
				else if( !right && !left){
					//when in doubt, follow the beacon
					//dont adjust steering at all

					//you know you have seen a wall
					//check if the beacon is on the other side of the wall
					//turn to go around the obstacle 50 50 chance
					//turn right
					if( front_angle < 45){
						//the target is directly ahead
						FL.steerAngle = 30 * steering_decision; //sometimes choose right, other times choose left
						FR.steerAngle = 30 * steering_decision;
					}
				}
			}
			//dont fight yourself
			else if( right_periph && FR.steerAngle > 0){
				//stop turning right
				FR.steerAngle = -1;
				FL.steerAngle = -1;
			}
			else if( left_periph && FR.steerAngle < 0){
				//stop turning left
				FR.steerAngle = 1;
				FL.steerAngle = 1;
			}
		}
	}

	void follow_player(){
		Vector3 player_heading = player1.transform.position - transform.position;
		player_heading.y = 0;
		player_heading.Normalize();

		Vector3 my_heading = transform.forward * -1; //object is facing backwards
		my_heading.y = 0;
		my_heading.Normalize();

		Vector3 my_right_heading = transform.right * -1;
		Vector3 my_left_heading = my_right_heading * -1;
		my_right_heading.y = 0;
		my_right_heading.Normalize();
		my_left_heading.y = 0;
		my_left_heading.Normalize();

		float r_angle = Vector3.Angle(player_heading, my_right_heading);
		float l_angle = Vector3.Angle(player_heading, my_left_heading);
		float f_angle = Vector3.Angle(player_heading, my_heading);
		front_angle = f_angle; //update a global variable

		float dist = Vector3.Distance( player1.transform.position , transform.position);


		//if the player is directly in front of you (or close enough) drive forward
		if( r_angle > 85 && r_angle < 95 && l_angle > 85 && l_angle < 95 && f_angle < 15)
		{
		}

		//if player is not in front, but is very close
		//stop to try and block him
		//TODO make sure player is moving slow!!!
		else if( f_angle > 25 &&  dist < 4){
			FR.motorTorque = 0;
			FL.motorTorque = 0;
			BR.motorTorque = 0;
			BL.motorTorque = 0;
			
			
			FR.brakeTorque = 30;
			FL.brakeTorque = 30;
			BR.brakeTorque = 30;
			BL.brakeTorque = 30;
		}

		else if( r_angle < l_angle){
			//turn right
			FL.steerAngle = 30;
			FR.steerAngle = 30;
		}
		else if( l_angle < r_angle){
			//turn left
			FL.steerAngle = -30;
			FR.steerAngle = -30;
		}
		//float angle = Mathf.Acos(Vector2.Dot(player_heading, my_heading));

	}

	void detect_suspicous_activity(){
		Vector3 player_heading = player1.transform.position - transform.position;
		player_heading.y = 0;
		player_heading.Normalize();
		
		Vector3 my_heading = transform.forward * -1; //object is facing backwards
		my_heading.y = 0;
		my_heading.Normalize();

		float f_angle = Vector3.Angle(player_heading, my_heading);
		float dist = Vector3.Distance( player1.transform.position , transform.position);

		//if player is ahead of you
		if( f_angle < 25 && dist < 15){
			//if player breaking the speed limit
			if( player_rigidbody.GetComponent<Rigidbody>().velocity.sqrMagnitude > 10){
				print ("We have detected the player");
				in_pursuit = true;
				parked = false;
			}
		}

		//if player hit the car
		if( my_collider.GetComponent<CopCarCollider>().colliding_with_player > 0){
			in_pursuit = true;
			parked = false;
			print ("We have detected the player via a collision");
		}


	}
	
	// Update is called once per frame
	void Update () {
		
			//initial values
		FL.steerAngle = 0;
		FR.steerAngle = 0;

		FR.motorTorque = -30 * torqueConstant;
		FL.motorTorque = -30 * torqueConstant;
		BR.motorTorque = -30 * torqueConstant;
		BL.motorTorque = -30 * torqueConstant;

		
		FR.brakeTorque = 0;
		FL.brakeTorque = 0;
		BR.brakeTorque = 0;
		BL.brakeTorque = 0;

		if(reversing <= 0){
			if(in_pursuit){
				//print ("in pursuit, following player");
				if(!sirens.isPlaying){
				sirens.Play();
				}
				follow_player();
			}
			if(!parked){
				//print ("not parked, checking sensors");
				check_sensors();//to avoid obstacles
			}
			if( !in_pursuit){
				//print ("not in pursuit, looking for suspects");
				detect_suspicous_activity();
			}
			if(parked){
				FR.motorTorque = 0;
				FL.motorTorque = 0;
				BR.motorTorque = 0;
				BL.motorTorque = 0;

				FR.brakeTorque = 30;
				FL.brakeTorque = 30;
				BR.brakeTorque = 30;
				BL.brakeTorque = 30;
			}
		}
		else if(reversing > 1){
			reversing -= Time.deltaTime;
			FR.motorTorque = 30;
			FL.motorTorque = 30;
			BR.motorTorque = 30;
			BL.motorTorque = 30;
		}
		else if(reversing <= 1){
			reversing -= Time.deltaTime;
			FR.brakeTorque = 100;
			FL.brakeTorque = 100;
			BR.brakeTorque = 100;
			BL.brakeTorque = 100;
		}
		
		float speed = GetComponent<Rigidbody>().velocity.sqrMagnitude;
		if( speed > MaxSpeed){
			FR.motorTorque = 0;
			FL.motorTorque = 0;
			BR.motorTorque = 0;
			BL.motorTorque = 0;
		}
	}
}
