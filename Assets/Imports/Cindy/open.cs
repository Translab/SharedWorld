using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class open : MonoBehaviour {
	// Use this for initialization
	private Renderer renderer;
	public GameObject doorChild;
	public bool shouldClose;
	public bool shouldOpen;
	public float speed;

	void Start () {
		renderer = doorChild.GetComponent<MeshRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log (renderer.isVisible);
		//Debug.Log (transform.eulerAngles.y);
		if (shouldOpen) {
			transform.Rotate (new Vector3 (0.0f, -speed, 0.0f));
			if (transform.eulerAngles.y < 270 && transform.eulerAngles.y>180) {
				transform.eulerAngles = new Vector3 (0f, 270f, 0f);
				shouldOpen = false;
			}
		}

		if (shouldClose) {
			transform.Rotate (new Vector3 (0.0f, speed, 0.0f));
			if (transform.eulerAngles.y > 0.0 &&transform.eulerAngles.y<90f) {
				transform.eulerAngles = Vector3.zero;
				shouldClose = false;
			}
		}

		// ...also rotate around the World's Y axis
		//transform.Rotate(Vector3.up * Time.deltaTime, Space.World);
	}



}
