using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTextPick : MonoBehaviour {

	private TextMesh textMesh;
	private float lerp_value = 0.0f;
	private float lerp_speed = 0.01f;
	private Color colorA;
	private Color colorB;

	// Use this for initialization
	void Start () {
		textMesh = GetComponent<TextMesh> ();
		string myString = PoemString.words [Random.Range (0, PoemString.words.Length)];
		textMesh.text = myString;
		textMesh.characterSize = 0.2f;
		textMesh.fontSize = Random.Range (60, 120);
		colorA = Random.ColorHSV (0.0f, 1.0f,0.0f,1.0f);
		colorB = Random.ColorHSV (0.0f, 1.0f,0.0f,1.0f);
	}
	
	// Update is called once per frame
	void Update () {
		lerp_value = Mathf.PingPong ((float)Time.time / 10.0f, 1.0f);
//		lerp_value += lerp_speed;
//		if (lerp_value >= 1.0f || lerp_value <= 0.0f) {
//			lerp_speed = -lerp_speed;
//		} 
//		if (lerp_value >= 1) {
//			colorA = Random.ColorHSV (0.0f, 1.0f,0.0f,1.0f);
//		} else if (lerp_value <= 0) {
//			colorB = Random.ColorHSV (0.0f, 1.0f,0.0f,1.0f);
//		}
		textMesh.color = Color.Lerp (colorA, colorB, lerp_value);
	}
}
