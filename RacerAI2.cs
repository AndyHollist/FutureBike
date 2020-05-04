using UnityEngine;
using System.Collections;

public class RacerAI2 : MonoBehaviour {


	public float moveForce = 1.0F;
	public float brakeForce = 0.5F;
    public float rotateTorque = 1.0F;
    public float hoverHeight = 4.0F;
    public float hoverForce = 5.0F;
    public float hoverDamp = 0.5F;
    Rigidbody rb;
    public GameObject go;
    public float orientation_rate = 1.0f;
    public float orient_damping = 0.5f;
    public GameObject model;
    public int state = -1;
    public int lap = 0;
    public int finish_position = -1;
    int waypoint_offset_x = 0;
    int waypoint_offset_z = 0;
    float reverse_time = 0;
    float distance_to_target = 0;
    public AudioSource engine_sound;
    public RespawnContainer respawns;
    public float respawn_hold_time = 0;
    float delay_after_respawn = 0;
    float current_z_rotation = 0;//for bike leaning
    public float turn_rate = 80;
    public GameObject bikeModel;//actual bike
    int stuck = 0;

    public WaypointContainer waypoints; //state-1 -> waypoint0 -> state0 -> waypoint1 -> state1 -> waypoint2
    
    void Start() {
        rb = go.GetComponent<Rigidbody>();
        rb.drag = 0.5F;
        rb.angularDrag = 0.5F;
        waypoint_offset_x = Random.Range(-3,3);
        waypoint_offset_z = Random.Range(-3,3);
        engine_sound.loop = true;
        engine_sound.Play();
    }

    void Update(){
    	//print("AI driver - state: " + state + " lap: " + lap + " finish_position " + finish_position);
        update_audio_pitch();
    }

    void check_if_stuck(){
        if(rb.velocity.sqrMagnitude < 0.1 && (go.transform.up.y <= 0.5 ) || rb.velocity.sqrMagnitude < 0.1 ){
            stuck = 1;
        }
        else stuck = 0;
    }

    public void update_respawn(){
        if(stuck == 1){
            respawn_hold_time += Time.deltaTime;
        }
        else respawn_hold_time = 0;
        if(respawn_hold_time >= 3){
            int index = 0;
            if(state == -1){
                index = respawns.spawnpoints.Length - 1;//last point
            }
            else{
                index = state;
            }
            respawn(index);
            respawn_hold_time = 0;
            delay_after_respawn = 2;
        }
    }

    void respawn(int index){
            go.transform.position = respawns.spawnpoints[index].transform.position;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            go.transform.forward = respawns.spawnpoints[index].transform.forward;
    }

    public void update_audio_pitch(){
         engine_sound.pitch = (rb.velocity.sqrMagnitude + 40) / 100.0f;
    }

    void check_for_reverse(){
        RaycastHit fronthit;
        Ray frontray = new Ray(go.transform.position, go.transform.forward);
        if(Physics.Raycast(frontray, out fronthit)){
            if(rb.velocity.magnitude < 1 && fronthit.distance < 2){
                reverse_time = 1;
            }
        }
    }

    void turn_right(float angle_diff){
        rb.AddTorque( (rotateTorque*angle_diff) * go.transform.up);
        if(current_z_rotation > -20 && stuck == 0){
            bikeModel.transform.Rotate(new Vector3(0,0,-turn_rate*Time.deltaTime));
            current_z_rotation -= turn_rate*Time.deltaTime;
        }    
    }

    void turn_left(float angle_diff){
        rb.AddTorque( (rotateTorque*angle_diff) * go.transform.up * -1);
        if(current_z_rotation < 20 && stuck == 0){
            bikeModel.transform.Rotate(new Vector3(0,0,turn_rate*Time.deltaTime));
            current_z_rotation += turn_rate*Time.deltaTime;
        }  
    }

    void straighten_up(){
        if(current_z_rotation > 0.1){
            bikeModel.transform.Rotate(new Vector3(0,0,-turn_rate*Time.deltaTime));
            current_z_rotation -= turn_rate*Time.deltaTime;
        }
        else if(current_z_rotation < -0.1){
            bikeModel.transform.Rotate(new Vector3(0,0,turn_rate*Time.deltaTime));
            current_z_rotation += turn_rate*Time.deltaTime;
        }
    }

    //move waypoint along a vector towards the next waypoint
    void get_interpolated_waypoint(){
        int waypoint_index = state + 1;
        if(waypoint_index >= waypoints.waypoints.Length){
            waypoint_index = 0;//wrap around
        } 
        GameObject current_waypoint = waypoints.waypoints[waypoint_index];
        int next_waypoint_index = waypoint_index + 1;
        if(next_waypoint_index >= waypoints.waypoints.Length){
            next_waypoint_index = 0;//wrap around
        }
        GameObject next_waypoint = waypoints.waypoints[next_waypoint_index];
        Vector3 converted_target_pos = new Vector3(current_waypoint.transform.position.x + waypoint_offset_x,
         0,  current_waypoint.transform.position.z + waypoint_offset_z);
        Vector3 converted_next_pos = new Vector3(next_waypoint.transform.position.x + waypoint_offset_x,
         0,  next_waypoint.transform.position.z + waypoint_offset_z);
        Vector3 converted_my_pos = new Vector3(go.transform.position.x, 0 , 
                                                    go.transform.position.z);
        distance_to_target = Vector3.Distance(converted_target_pos,converted_my_pos);
        
    }

    float update_steering(){
    	//check direction to the target
        int waypoint_index = state + 1;
        if(waypoint_index >= waypoints.waypoints.Length){
        	waypoint_index = 0;//wrap around
        } 
        GameObject current_waypoint = waypoints.waypoints[waypoint_index];
        //we have this set amount of waypoints, just randomly adjust to a new position
        //within 10 feet of the actual waypoint

        //only calculate random once as soon as the state changes, then save it

        //ignore y position of waypoints
        Vector3 converted_target_pos = new Vector3(current_waypoint.transform.position.x + waypoint_offset_x,
         0,	 current_waypoint.transform.position.z + waypoint_offset_z);
        Vector3 converted_my_pos = new Vector3(go.transform.position.x, 0 , 
        											go.transform.position.z);
        Vector3 dir_to_target = converted_target_pos - converted_my_pos;
        distance_to_target = Vector3.Distance(converted_target_pos,converted_my_pos);
        float angle = Vector3.Angle(dir_to_target, go.transform.right);
        float angle_from_nose = Vector3.Angle(dir_to_target, go.transform.forward);
        //angle from nose is <90 when you are facing fwd

        //print("Angle: " + angle);  
        //need to check if fwd or bwd

         //90 is straight fwd(or bwd) , >90 is left <90 is right
        	float angle_diff = Mathf.Abs(angle - 90);//how far away from the target of 90
        	if(angle > 93 || angle_from_nose > 90){//turn left
                RaycastHit lefthit;
                Ray leftray = new Ray(go.transform.position, go.transform.right*-1);
                if(Physics.Raycast(leftray,out lefthit) ){
                    if(lefthit.distance > 5){
                        turn_left(angle_diff);     
                    }
                    else if(lefthit.distance < 1){//steer away from a close wall
                        turn_right(angle_diff);
                    }
                }
                else{
        		      turn_left(angle_diff);
                }
        	}
        	else if(angle < 87 || angle_from_nose > 90){//turn right
        		//rb.AddTorque( (rotateTorque*angle_diff) * go.transform.up );

                RaycastHit righthit;
                Ray rightray = new Ray(go.transform.position, go.transform.right);
                if(Physics.Raycast(rightray,out righthit) ){
                    if(righthit.distance > 5){
                        turn_right(angle_diff);
                    }
                    else if(righthit.distance < 1){//steer away from a close wall
                        turn_left(angle_diff);
                    }
                }
                else{
                    turn_right(angle_diff);  
                }
        	}
            else{//straighten up
                straighten_up();
            }
        
        return angle_from_nose;
    }

    void update_accel(float angle_from_nose){
    	float velocity = go.GetComponent<Rigidbody>().velocity.magnitude;
        //brake if going too fast, too sharp angle, too close to target
    	if(angle_from_nose > 15 && angle_from_nose < 90 && velocity > 6){ 
    		rb.AddForce((brakeForce)*go.transform.forward*-1);
    	}
    	else if(angle_from_nose < 90)
    	   rb.AddForce(moveForce*go.transform.forward);
    }

    void reverse(){
        rb.AddForce(moveForce*go.transform.forward*-1);
    }

    void FixedUpdate() {

        check_if_stuck();
        update_respawn();
        if(delay_after_respawn <= 0){
            check_for_reverse();
            if(reverse_time > 0){
                reverse();
                reverse_time -= Time.deltaTime;
            }
            else{
                float angle = update_steering();
                update_accel(angle);
            }
        }
        else delay_after_respawn -= Time.deltaTime;

        //hovering stuff
        RaycastHit hit;
        Ray downRay = new Ray(go.transform.position, -go.transform.up);//-Vector3.up
        if (Physics.Raycast(downRay, out hit)) {
            float hoverError = hoverHeight - hit.distance;
            if (hoverError > 0) {
                float upwardSpeed = rb.velocity.y;
                float lift = hoverError * hoverForce - upwardSpeed * hoverDamp;
                rb.AddForceAtPosition(lift * Vector3.up,go.transform.position);

                //orient to match the normals of the surface below
                RaycastHit hit2;
                Ray ray2 = new Ray(go.transform.position, -Vector3.up);
                if(Physics.Raycast(ray2, out hit2)){
                    Vector3 avg_normals = Vector3.Normalize(hit.normal + hit2.normal);
                    //pick the normals that are most upright
                    avg_normals = find_most_upright(avg_normals, hit.normal, hit2.normal);

                    //rotate to normals
                    float angle_btw = Vector3.Angle(go.transform.up, avg_normals);
                    Vector3 cross = Vector3.Cross(go.transform.up, avg_normals);
                    //convert angular velocity
                        Vector3 curr_angl_vel = rb.angularVelocity;
                        Vector3 curr_dir_up = go.transform.up;
                        float angle_to_top = Vector3.Angle(curr_dir_up,Vector3.up);
                        Vector3 cross_p = Vector3.Cross(curr_dir_up, Vector3.up);
                        //convert angular velocity to as if we were sitting upright
                        curr_angl_vel = Quaternion.AngleAxis(angle_to_top, cross_p) * curr_angl_vel;
                        curr_angl_vel.y = 0;//ignore y velocity(player controlled turns)
                    float vel_magnitude = curr_angl_vel.magnitude;
                    rb.AddTorque( (orientation_rate * angle_btw - vel_magnitude * orient_damping) * cross);
                }

            }
        }

    }


    Vector3 find_most_upright(Vector3 one, Vector3 two, Vector3 three){
        Vector3 best_of_two = find_most_upright(one,two);
        return find_most_upright(best_of_two,three);
    }

    Vector3 find_most_upright(Vector3 one, Vector3 two){
        float angle1 = Vector3.Angle(Vector3.up, one);
        float angle2 = Vector3.Angle(Vector3.up, two);
        if(angle1 < angle2) return one;
        else return two;
    }
}
