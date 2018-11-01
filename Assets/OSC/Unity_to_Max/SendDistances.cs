using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendDistances : MonoBehaviour {

	public GameObject player;

	[SerializeField]private int MAX_TYPE = 5;

	private int[] minIndices;
	private float[] minDistances;

	private OSC osc;	
	private string OSCHandle = "/subpac";
	private DistanceParameters[] objects; 


	void Awake(){
		osc = GameObject.FindObjectOfType<OSC>();

		minIndices = new int[MAX_TYPE];
		minDistances = new float[MAX_TYPE];
	}

	void Start() {
		objects = FindObjectsOfType<DistanceParameters>();
	}
	
	// Update is called once per frame
	void Update () {
		if(player != null) {
			// reset
			for (int i = 0; i < MAX_TYPE; i++) {
				minIndices[i] = int.MaxValue;
				minDistances[i] = float.MaxValue;
			}

			// update
			for (int j = 0; j < objects.Length; j++) {
				float dist = Vector3.Distance(player.transform.position, 
											  objects[j].gameObject.transform.position + objects[j].offset);
				
				if (objects[j].size > dist) {
					int _t = objects[j].type;

					if(dist / objects[j].size < minDistances[_t]) {
						minIndices[_t] = j; 
						minDistances[_t] = dist / objects[j].size;
					}
				}
			}
			
			string msg = "";
			for (int i = 0; i < MAX_TYPE; i++)
			{
				msg += i + ") " +  minDistances[i] + " ," ;

				OscMessage message = new OscMessage();
				message.address = OSCHandle + i;
				message.values.Add(minDistances[i]);
				osc.Send(message);
			}
			Debug.Log(msg);
		}
	}
}
