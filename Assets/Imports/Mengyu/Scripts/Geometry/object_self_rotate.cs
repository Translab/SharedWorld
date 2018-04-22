using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class object_self_rotate : MonoBehaviour {

	public float rotate_factor = 2.0f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		// Rotate the object around its local X axis at 1 degree per second
		transform.Rotate(0, 0, Time.deltaTime * rotate_factor );

		// ...also rotate around the World's Y axis
		//transform.Rotate(Vector3.up * Time.deltaTime, Space.World);
		
	}
}
