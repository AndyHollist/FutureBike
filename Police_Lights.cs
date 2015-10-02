using UnityEngine;
using System.Collections;

public class Police_Lights : MonoBehaviour {

	public GameObject parent_cop_car_obj;

	private CopCar parent_cop;
	private Light my_light;

	// Use this for initialization
	void Start () {
		my_light = GetComponent<Light>();
		parent_cop = parent_cop_car_obj.GetComponent<CopCar>();
		StartCoroutine(flash_lights());
	}

	IEnumerator flash_lights(){
		for(;;){
			if( parent_cop.in_pursuit == true){
				my_light.intensity = 1;
				if(my_light.color == Color.red){
					my_light.color = Color.blue;
				}
				else my_light.color = Color.red;
			}
			else{
				my_light.intensity = 0;

			}
			yield return new WaitForSeconds(0.4f);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
