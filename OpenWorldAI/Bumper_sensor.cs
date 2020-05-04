using UnityEngine;
using System.Collections;

public class Bumper_sensor : MonoBehaviour {

	public bool is_triggered = false;
	public int trigger_count = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other)
	{
		is_triggered = true;
		trigger_count++;
	}

	void OnTriggerExit(Collider other)
	{
		trigger_count--;
		if(trigger_count == 0)
		{
			is_triggered = false;
		}
	}
}
