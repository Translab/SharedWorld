using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPortals : MonoBehaviour {
	private GameObject player;
	public GameObject simulater;
	public GameObject cameraRig;
	public bool useSimulater = true;
	public GameObject[] exits;
	public int index = 0;
	public bool isRandom = false;
	private RaycastHit hit;
	private Vector3 fwd;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		fwd = player.transform.TransformDirection(Vector3.forward);
		Ray ray = new Ray(player.transform.position, fwd);
		if (Physics.Raycast(ray, out hit, 1)){
			//Debug.Log("hitted");
			if (hit.transform.tag == "localPortal"){
				//Debug.Log("yesportal");
				if (!isRandom) {
					player.transform.position = exits[index].transform.position;
					if (index < exits.Length - 1){
						index ++;
					} else {
						index = 0;
					}
				} else {
					index = Random.Range (0, exits.Length);
					player.transform.position = exits[index].transform.position;
				}
			} // hit tag
		} // physics

	} //update

}
