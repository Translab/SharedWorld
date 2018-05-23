using UnityEngine;
using System.Collections;

public class elevatorVertical : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void goToFloor(){
		Debug.Log ("PlayingAnim");
		gameObject.GetComponent<Animation>().Play ();


	}
}
