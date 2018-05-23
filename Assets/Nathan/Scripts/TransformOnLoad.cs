using UnityEngine;
using System.Collections;

public class TransformOnLoad : MonoBehaviour
{


	string startposition;
	public  float yOffset = 0.0f;

	void Start ()
	{
		startposition = SceneGlobals.getLodingLocation ();

		if (startposition != null) {
			if (tag == "VRPlayer") {
				if (startposition == "f1_startStairf2") {
					GameObject startEmpty = GameObject.Find (startposition);
					Vector3 startpos = startEmpty.transform.position;
					gameObject.transform.position = new Vector3 (startpos.x, 5.888391f, startpos.z);
					gameObject.transform.rotation = startEmpty.transform.rotation;
				} else if (startposition == "f1_startStairf3") {
					GameObject startEmpty = GameObject.Find (startposition);
					Vector3 startpos = startEmpty.transform.position;
					gameObject.transform.position = new Vector3 (startpos.x, 12.77867f, startpos.z);
					gameObject.transform.rotation = startEmpty.transform.rotation;


				} else {
					Debug.Log ("StartPosition " + startposition);
					GameObject startEmpty = GameObject.Find (startposition);
					Vector3 startpos = startEmpty.transform.position;
//				gameObject.transform.position = new Vector3 (startpos.x, startpos.y + yOffset, startpos.z);
					gameObject.transform.position = new Vector3 (startpos.x, gameObject.transform.position.y, startpos.z);
//						gameObject.transform.position = startEmpty.transform.position;
					gameObject.transform.rotation = startEmpty.transform.rotation;
				}

			} else if (tag == "Player") {
				GameObject startEmpty = GameObject.Find (startposition);
				Vector3 startpos = startEmpty.transform.position;
				//				gameObject.transform.position = new Vector3 (startpos.x, startpos.y + yOffset, startpos.z);
				//gameObject.transform.position = new Vector3 (startpos.x, gameObject.transform.position.y, startpos.z);
				gameObject.transform.position = startEmpty.transform.position;
				gameObject.transform.rotation = startEmpty.transform.rotation;

			}
		}
	}
}
