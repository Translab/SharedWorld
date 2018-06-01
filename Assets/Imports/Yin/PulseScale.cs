using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseScale : MonoBehaviour {
    private float scaleFactor;
	public float amplitude = 0.2f;
    public float frequency = 1.0f;
	public float originScale = 300.0f;
	public float baseScale = 1.0f;
	private Vector3 scaleVector = new Vector3(300, 300, 300);
    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        scaleFactor = baseScale + Mathf.Sin(Time.time * frequency) * amplitude;
		scaleVector = new Vector3(scaleFactor * originScale, scaleFactor * originScale, scaleFactor * originScale);
		transform.localScale = scaleVector;

	}
}
