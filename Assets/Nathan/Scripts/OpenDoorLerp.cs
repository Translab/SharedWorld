using UnityEngine;
using System.Collections;

public class OpenDoorLerp : MonoBehaviour {


	public GameObject rot;

	public float startRot, endRot;
	Quaternion sRot;
	int counter = 0;

	private bool rotate = false;

	// Use this for initialization
	void Start () {
		sRot = rot.transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		if (rotate) {
			//rot.transform.rotation  =   Quaternion.Lerp(sRot, Quaternion.Euler(sRot.x,sRot.y,endRot), Time.time * 0.1f); //Vector3.Lerp( (0.0f, 0.0f, Mathf.Lerp (startRot, endRot, 1000.0f));
			rot.transform.RotateAround(rot.transform.position,rot.transform.forward,-1);
			counter ++;

		//	rot.transform.ro

			if(counter > 90){
				rotate = false;
				counter = 0;

			}
				}
	
	}

	void OnTriggerEnter(Collider other){
		if (other.gameObject.tag == "Player") {

			//rot.transform.rot
			//startRotation();

			rotate = true;
		}





	}


	void startRotation(){
		//Quaternion.Euler (new Vector3 (0.0f, 0.0f, Mathf.Lerp (startRot, endRot, 1.0f)));
		rot.transform.rotation  =   Quaternion.Lerp(sRot, Quaternion.Euler(sRot.x,sRot.y,endRot), Time.time * 0.1f); //Vector3.Lerp( (0.0f, 0.0f, Mathf.Lerp (startRot, endRot, 1000.0f));

	}
}
