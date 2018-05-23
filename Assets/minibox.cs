using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minibox : MonoBehaviour {
	private float amp = 0.1f;
	private float speed = 0.5f;
	private float rand;
	private Vector3 startpos;
	// Use this for initialization
	void Start () {
		startpos = transform.localPosition;
		rand = Random.Range (0.0f, 1000.0f);
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = startpos + (new Vector3 (0.0f, Mathf.Sin (Time.time*speed+rand) * amp, 0.0f));
	}
}
