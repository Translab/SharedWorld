using UnityEngine;
using System.Collections;

public class PrintNooks : MonoBehaviour {



	string names = "";
	 void Start(){

		GameObject[] meshToPrint = GameObject.FindGameObjectsWithTag ("nooks");

		foreach (GameObject go in meshToPrint) {
			names += go.name + " = " + 1 +  "\n";


				}
		GameObject[] meshToPrint2 = GameObject.FindGameObjectsWithTag ("doors");
		
		foreach (GameObject go in meshToPrint2) {
			names += go.name + " = " + 1 +  "\n";
			
			
		}

		//Debug.Log (names);


//		GameObject nooksEmpty = GameObject.Find ("NooksEmpty");
//
//		Transform[] nooks = 	nooksEmpty.GetComponentsInChildren<Transform> ();
//
//		foreach (Transform n in nooks) {
//			//Debug.Log (n.name);
//			names += n.name + " = " + 1 +  "\n";
//
//
//				}
//
//		Debug.Log (names);
	}

}
