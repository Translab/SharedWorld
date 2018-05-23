using UnityEngine;
using System.Collections;

public class LightControl : MonoBehaviour {

	// Use this for initialization
	public GameObject lightEmpty;
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void lightState(bool b){
		if (b) {
						lightEmpty.SetActive (true);

				} else {
			lightEmpty.SetActive(false);
				}



	}
}
