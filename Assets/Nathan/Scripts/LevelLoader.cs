using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//namespace Valve.VR.InteractionSystem
//{
//
//	[RequireComponent (typeof(Interactable))]

public class LevelLoader : MonoBehaviour {

	// Use this for initialization
	public int floorIndexToLoad;
	public string startEmptyName;
	private GameObject sceneFader;

	void Start(){

		sceneFader = GameObject.Find ("SceneFader");
	}

//		private void HandHoverUpdate (Hand hand)
//		{
//			//Debug.Log ("hand Hovering");
//			if (hand.GetStandardInteractionButtonDown ()) {
//				//Debug.Log ("hand clicked");
//				PrepareNewScene ();
//			}
//		}

	void OnTriggerEnter(Collider other){

		if (other.gameObject.tag == ("Player")) {
			PrepareNewScene ();
		}

	}

		void PrepareNewScene(){
			SceneGlobals.setLoadingLocation(startEmptyName);
			sceneFader.GetComponent<SceneFade>().setLevel(floorIndexToLoad);
			sceneFader.GetComponent<SceneFade>().EndScene();
		}

	}
//}