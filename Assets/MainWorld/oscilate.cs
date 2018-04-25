using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class oscilate : MonoBehaviour {
	public GameObject camera;
	public Vector3 matchPoint = new Vector3(0,0,0);
	public float scaleFactor = 1.0f;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		Debug.Log (Mathf.Sin(Time.time));
		transform.localScale = new Vector3(1,1,  0.25f + (1 + Mathf.Sin(Time.time)) /2 * scaleFactor );

	}
}
