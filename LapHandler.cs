using UnityEngine;
using System.Collections;

public class LapHandler : MonoBehaviour {

	public GameObject[] racers;
	public int num_laps = 3;
	public int num_checkpoints = 3;

	int curr_finish_position = 1;
	int num_racers = 1;

	// Use this for initialization
	void Start () {
		num_racers = racers.Length;
	}
	
	// Update is called once per frame
	void Update () {

			foreach (GameObject go in racers){
				//GameObject go2 = go.transform.parent.gameObject;
				//check if player or AI
				if(go.GetComponent<HoverObj5>() != null){
					HoverObj5 racer = go.GetComponent<HoverObj5>();
					if(racer.finish_position < 0 && racer.lap > num_laps){
						racer.finish_position = curr_finish_position;
						curr_finish_position++;
					}
				}
				else{
					 RacerAI2 racer = go.GetComponent<RacerAI2>();
					 if(racer.finish_position < 0 && racer.lap > num_laps){
						racer.finish_position = curr_finish_position;
						curr_finish_position++;
					}
				}

			}
		
	}
}
