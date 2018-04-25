using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sculpture_lighting : MonoBehaviour {
	Light Spotlight_sculpture_left;
	Light Spotlight_sculpture_right;

	// Use this for initialization
	int frameCount = 0;
	void Start () {
		Spotlight_sculpture_left = GameObject.Find("Spotlight_sculpture_left").GetComponent<Light>();
		Spotlight_sculpture_right = GameObject.Find("Spotlight_sculpture_right").GetComponent<Light>();}

	// Update is called once per frame
	void Update () {
		frameCount++;
		Spotlight_sculpture_left.intensity = Mathf.Sin (frameCount / 300.0f)+1;
		Spotlight_sculpture_right.intensity = Mathf.Cos (frameCount / 300.0f)+1;
	}
}
