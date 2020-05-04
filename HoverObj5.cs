using UnityEngine;
using System.Collections;

public class HoverObj5 : MonoBehaviour {

	public float moveForce = 1.0F;
    public float rotateTorque = 1.0F;
    public float hoverHeight = 4.0F;
    public float hoverForce = 5.0F;
    public float hoverDamp = 0.5F;
    Rigidbody rb;
    public GameObject go;
    public float orientation_rate = 1.0f;
    public float orient_damping = 0.5f;
    public GameObject model;//cube
    public GameObject bikeModel;//actual bike
    public int state = -1;
    public int lap = 0;
    public int finish_position = -1;
    public AudioSource engine_sound;
    float current_z_rotation = 0;//for bike leaning
    public float turn_rate = 80;
    public RespawnContainer respawns;
    public float respawn_hold_time = 0;
    int stuck = 0;

    void Start() {
        rb = go.GetComponent<Rigidbody>();
        rb.drag = 0.5F;
        rb.angularDrag = 0.5F;
        engine_sound.loop = true;
        engine_sound.Play();
    }

    public void update_audio_pitch(){
         engine_sound.pitch = (rb.velocity.sqrMagnitude + 40) / 100.0f;
    }

    void check_if_stuck(){
        if(rb.velocity.sqrMagnitude < 0.1 && (go.transform.up.y <= 0.5 ) ){
            stuck = 1;
        }
        else stuck = 0;
    }

    public void update_respawn(){
        if(Input.GetKey(KeyCode.R) || stuck == 1){
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
        }
    }

    void respawn(int index){
            go.transform.position = respawns.spawnpoints[index].transform.position;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            go.transform.forward = respawns.spawnpoints[index].transform.forward;
    }

    void Update(){
        //print("State = " + state + " Lap = " + lap + " Finish pos = " + finish_position);
        update_audio_pitch();
        //print("obj5 transf.up : " + go.transform.up);
    }

    void FixedUpdate() {
        check_if_stuck();
        update_respawn();
        //rb.AddForce(Input.GetAxis("Vertical") * moveForce * transform.forward);
        //rb.AddTorque(Input.GetAxis("Horizontal") * rotateTorque * Vector3.up);
        if(Input.GetKey(KeyCode.W) ){
            rb.AddForce(moveForce*go.transform.forward);
        }
        else if( Input.GetKey(KeyCode.S) ){
            rb.AddForce(moveForce*go.transform.forward * -1);
        }
        if(Input.GetKey(KeyCode.D)){//turn right
            rb.AddTorque( rotateTorque * go.transform.up);//go.transform.up
            if(current_z_rotation > -30 && stuck == 0){
                bikeModel.transform.Rotate(new Vector3(0,0,-turn_rate*Time.deltaTime));
                current_z_rotation -= turn_rate*Time.deltaTime;
            }
        }
        else if(Input.GetKey(KeyCode.A)){//turn left
            rb.AddTorque( rotateTorque * go.transform.up * -1);
            if(current_z_rotation < 30 && stuck == 0){
                bikeModel.transform.Rotate(new Vector3(0,0,turn_rate*Time.deltaTime));
                current_z_rotation += turn_rate*Time.deltaTime;
            }
        }
        else{//balance out
            if(current_z_rotation > 0.1){
                bikeModel.transform.Rotate(new Vector3(0,0,-turn_rate*Time.deltaTime));
                current_z_rotation -= turn_rate*Time.deltaTime;
            }
            else if(current_z_rotation < -0.1){
                bikeModel.transform.Rotate(new Vector3(0,0,turn_rate*Time.deltaTime));
                current_z_rotation += turn_rate*Time.deltaTime;
            }
        }
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
        /*
        print("Angular Velocity: " + rb.angularVelocity);
        Vector3 curr_angl_vel = rb.angularVelocity;
        Vector3 curr_dir_up = go.transform.up;
        float angle_to_top = Vector3.Angle(curr_dir_up,Vector3.up);
        Vector3 cross_p = Vector3.Cross(curr_dir_up, Vector3.up);
        //convert angular velocity to as if we were sitting upright
        curr_angl_vel = Quaternion.AngleAxis(angle_to_top, cross_p) * curr_angl_vel;
        print("Converted Angular Velocity: " + curr_angl_vel);
        */
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
