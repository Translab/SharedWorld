using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_trackPlayer : MonoBehaviour {
	GameObject playerHead;
	float pitch;
	float yaw;
	float yaw_real;
	//PID pid_yaw;
	// Use this for initialization
	void Start () {
		playerHead = GameObject.Find("MainCamera");
		//pitch = 0f;
		yaw = 0f;
		//pid_yaw = new PID (1f, 0.9f, 0);
	}
	
	// Update is called once per frame
	void Update () {
		//Vector3 camPosition = transform.position;
		//Vector3 playPosition = playerHead.transform.position;
		Vector3 rd = playerHead.transform.position - transform.position;  //relativeDistance

		//float R = Mathf.Sqrt (rd.x * rd.x + rd.y * rd.y + rd.z * rd.z);
		//rd = rd/R;
		//pitch = Mathf.Atan (Mathf.Sqrt (rd.x * rd.x + rd.z * rd.z) / rd.y);
		yaw = Mathf.Atan (rd.z / (rd.x));
		//print ("rd.z = " + rd.z+" rd.x = " + rd.x);
		if (rd.z > 0) {
			if (rd.x > 0) {
				yaw = -(yaw/3.14f)* 180f;
			} else {
				yaw = ((Mathf.PI-yaw)/3.14f)* 180f;
			}
		} else { 
			if (rd.x > 0) {
				yaw = -(yaw/3.14f)* 180f;
			} else {
				yaw =((Mathf.PI-yaw)/3.14f)* 180f;
			}
		}
		//pid_yaw.setReference (yaw);
		//yaw_real = pid_yaw.process (yaw_real);
		//print ("yaw = " + yaw+" sign = " + sign);
		transform.rotation = Quaternion.Euler(transform.eulerAngles.x,yaw , transform.eulerAngles.z);
	}
}
