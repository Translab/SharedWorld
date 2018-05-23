using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Valve.VR.InteractionSystem {

//	[RequireComponent (typeof(Interactable))]
	public class elev_control : MonoBehaviour {

		public GameObject mainCanvas;
		public GameObject elevatorDoor;

		void Start() {

//			if (mainCanvas == null) {
//				mainCanvas = GameObject.FindGameObjectWithTag("Canvas");
//			}

			string loadingLocation = SceneGlobals.getLodingLocation();
			if (loadingLocation == "f1_startElevator1" || loadingLocation == "f2_startElevator1" || loadingLocation == "f3_startElevator1" || SceneGlobals.getLodingLocation() == null) { 
				openElevator();
			}
		}

//		private void HandHoverUpdate (Hand hand)
//		{
//			if (hand.GetStandardInteractionButtonDown ()) {
//				Debug.Log ("elevator call button pressed");
//				openElevator ();
//			}
//		}





//		void OnTriggerEnter(Collider other) {
//			if (other.gameObject.tag == ("Player")) {
//				Text t = mainCanvas.GetComponentInChildren<Text>();
//				t.text = "Press P to Call The Elevator";
//				t.enabled = true;
//			}
//
//		}
//
//		void OnTriggerStay(Collider other) {
//			if (other.gameObject.tag == ("Player")) {
//				if (Input.GetKeyDown(KeyCode.P)) {
//					Text t = mainCanvas.GetComponentInChildren<Text>();
//					t.text = "";
//					t.enabled = false;
//					openElevator();
//				}
//			}
//		}
//
//		void OnTriggerExit(Collider other) {
//			if (other.gameObject.tag == ("Player")) {
//				Text t = mainCanvas.GetComponentInChildren<Text>();
//				t.text = "";
//				t.enabled = false;
//			}
//		}

		public void openElevator() {

			AudioSource audioSource = gameObject.GetComponentInChildren<AudioSource>();
			if (!audioSource.isPlaying) {
				audioSource.GetComponent<AudioSource>().Play();
			}
			elevatorDoor.gameObject.GetComponent<Animation>().Play();
		}
	}
}
