using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Valve.VR.InteractionSystem
{

	public class ElevatorButtons : MonoBehaviour
	{
	
		public GameObject mainCanvas;

		GameObject[] buttons;


		int selectedLevel;
		string startEmptyName;


		GameObject eleControl;

		void Start ()
		{

			eleControl = GameObject.Find ("elevator_controlPrefab");
			if (mainCanvas == null) {
				mainCanvas = GameObject.FindGameObjectWithTag ("Canvas");
			}

			buttons = GameObject.FindGameObjectsWithTag ("eleButtons");
			foreach (GameObject button in buttons) {
				button.SetActive (false);
			}
		}

		void OnTriggerEnter (Collider other)
		{
			if (other.gameObject.tag == ("Player")) {
				Text t = mainCanvas.GetComponentInChildren<Text> ();
				t.text = "Floor 1 = y \n Floor 2 = u \n Floor 3 = i \n Open Door = o ";
				t.enabled = true;
			}
		}

		void OnTriggerStay (Collider other)
		{
			if (other.gameObject.tag == ("Player")) {
				if (Input.GetKeyDown (KeyCode.I)) {
					selectedLevel = 2;
					startEmptyName = "f3_startElevator1";
					GetComponent<Animation> ().Play ();
				}
				if (Input.GetKeyDown (KeyCode.U)) {
					selectedLevel = 1;
					startEmptyName =	"f2_startElevator1";
					GetComponent<Animation> ().Play ();
				}
				if (Input.GetKeyDown (KeyCode.Y)) {
					selectedLevel = 0;
					startEmptyName =	"f1_startElevator1";
					GetComponent<Animation> ().Play ();
				}
				if (Input.GetKeyDown (KeyCode.O)) {
					eleControl.GetComponent<elev_control> ().openElevator ();

				}
			}
		}

		void OnTriggerExit (Collider other)
		{
			if (other.gameObject.tag == ("Player")) {


				Text t = mainCanvas.GetComponentInChildren<Text> ();
				t.text = "";
				t.enabled = false;
			}

		}

		void playAudio ()
		{
			GetComponent<AudioSource> ().Play ();
		}

		void loadNextLevel ()
		{

			SceneGlobals.setLoadingLocation (startEmptyName);
			//SceneGlobals.updatePlayerTransform ();
			Application.LoadLevel (selectedLevel);
		}


	}
}
