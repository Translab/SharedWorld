using UnityEngine;
using System.Collections;

public class footSteps : MonoBehaviour {
	
		private int numMoveKeysDown=0;
		
		// Update is called once per frame
		void Update () {
					
			if (Input.GetKeyDown (KeyCode.W)) {numMoveKeysDown++;}
			if (Input.GetKeyDown (KeyCode.A)) {numMoveKeysDown++;}
			if (Input.GetKeyDown (KeyCode.S)) {numMoveKeysDown++;}
			if (Input.GetKeyDown (KeyCode.D)) {numMoveKeysDown++;}
			
			if (Input.GetKeyUp (KeyCode.W)) {numMoveKeysDown--;}
			if (Input.GetKeyUp (KeyCode.A)) {numMoveKeysDown--;}
			if (Input.GetKeyUp (KeyCode.S)) {numMoveKeysDown--;}
			if (Input.GetKeyUp (KeyCode.D)) {numMoveKeysDown--;}
			
			if (numMoveKeysDown > 0) {
				gameObject.GetComponent<AudioSource>().pitch = (Random.value + 0.5f);
				GetComponent<AudioSource>().mute = false;
			} else {
				
				GetComponent<AudioSource>().mute = true;
			}
		}
	}

