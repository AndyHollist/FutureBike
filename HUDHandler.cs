using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDHandler : MonoBehaviour {

	public HoverObj5 player1;
	public LapHandler laphandler;
	public Text lap_disp;
	public Text finish_pos_disp;
	public Text speedometer;
	public Image panel_finished;
	public WaypointContainer waypoints;
	public Text curr_position;

	int player_race_position = 1;

	// Use this for initialization
	void Start () {
		finish_pos_disp.text = "";
		panel_finished.gameObject.SetActive(false);
		player_race_position = laphandler.racers.Length;

	}

	void find_player_race_position(){
		if(player1.finish_position > 0){
			player_race_position = player1.finish_position;
		}
		else{
			
			player_race_position = 1;
			foreach(GameObject go in laphandler.racers){
				if(go.GetComponent<RacerAI2>() != null){//only if its AI, not the player
					RacerAI2 opponent = go.GetComponent<RacerAI2>();
					if(opponent.lap > player1.lap){
						player_race_position++;
					}
					else if(opponent.lap == player1.lap && opponent.state > player1.state){
						player_race_position++;
					}
					else if(opponent.lap == player1.lap && opponent.state == player1.state){
						int state = player1.state;
						int waypoint_index = state + 1;
				        if(waypoint_index >= waypoints.waypoints.Length){
				        	waypoint_index = 0;//wrap around
				        } 
				        GameObject current_waypoint = waypoints.waypoints[waypoint_index];
				        //find if the opponent is closer to the waypoint
				        if(Vector3.Distance(current_waypoint.transform.position, opponent.transform.position)
				        	< Vector3.Distance(current_waypoint.transform.position, player1.transform.position)){
				        	player_race_position++;
				        }
					}
				}
			}//loop
		}
	}

	void draw_race_position(){

		string subscript = "";
		if(player_race_position == 1)subscript = "st";
		else if(player_race_position == 2)subscript = "nd";
		else if(player_race_position == 3)subscript = "rd";
		else subscript = "th";
		curr_position.text = "" + player_race_position + subscript;
		
	}
	
	// Update is called once per frame
	void Update () {
		int curr_lap = player1.lap;
		if(curr_lap < 1)curr_lap = 1;
		if( player1.lap > laphandler.num_laps) curr_lap = laphandler.num_laps;
		lap_disp.text = "Lap " + curr_lap + "/" + laphandler.num_laps;
		if(player1.finish_position > 0){
			string subscript = "";
			if(player1.finish_position == 1)subscript = "st";
			else if(player1.finish_position == 2)subscript = "nd";
			else if(player1.finish_position == 3)subscript = "rd";
			else subscript = "th";
			finish_pos_disp.text = "Finished " + player1.finish_position + subscript + "!";
			panel_finished.gameObject.SetActive(true);
		}
		else{
			finish_pos_disp.text = "";
		}

		speedometer.text = "" + System.Math.Round(player1.GetComponent<Rigidbody>().velocity.magnitude,2);

		find_player_race_position();
		draw_race_position();
	}
}
