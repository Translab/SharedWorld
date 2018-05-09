using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_trackPlayer : MonoBehaviour {
	GameObject playerHead;
	float pitch;
	float yaw;

	// Use this for initialization
	void Start () {
		playerHead = GameObject.Find("MainCamera");
		pitch = 0f;
		yaw = 0f;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 camPosition = transform.position;
		Vector3 playPosition = playerHead.transform.position;

		Vector3 rd = playPosition - camPosition;  //relativeDistance

		float R = Mathf.Sqrt (rd.x * rd.x + rd.y * rd.y + rd.z * rd.z);

		rd = rd/R;
		//pitch = Mathf.Atan (Mathf.Sqrt (rd.x * rd.x + rd.z * rd.z) / rd.y);
		yaw = Mathf.Atan (rd.z / (rd.x));
		float sign =  1;
		print ("rd.z = " + rd.z+" rd.x = " + rd.x);
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

		//yaw = (yaw/3.14f)* 180f;
		print ("yaw = " + yaw+" sign = " + sign);
		transform.rotation = Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z);
	}
}
