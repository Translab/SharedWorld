using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{

public class ElevatorFloorButton : MonoBehaviour {


	public int selectedLevel;
	public string startEmptyName;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

//		private void HandHoverUpdate (Hand hand)
//		{
//			if (hand.GetStandardInteractionButtonDown ()) {
//				loadNextLevel ();
//			}
//		}

		void loadNextLevel ()
		{

			SceneGlobals.setLoadingLocation (startEmptyName);
			//SceneGlobals.updatePlayerTransform ();
			Application.LoadLevel (selectedLevel);
		}

}
}
