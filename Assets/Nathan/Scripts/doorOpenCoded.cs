using UnityEngine;
using System.Collections;

public class doorOpenCoded : MonoBehaviour
{

		public GameObject rot;
		public float startLocalRotY = 0;
		public float endLocalRotY = 260;

		void OnTriggerEnter (Collider other)
		{
				if (other.gameObject.tag == "Player") {
						StartCoroutine (openDoor (1));
				}
		}

		void OnTriggerExit (Collider other)
		{
				if (other.gameObject.tag == "Player") {
						StartCoroutine (openDoor (0));
				}
		}

	//	int counter = 0;

		IEnumerator openDoor (int direction)
		{
				Debug.Log ("inCor dir = " + direction);
				Debug.Log (rot.transform.eulerAngles.y);

				if (direction == 1) {
						while (rot.transform.eulerAngles.y != endLocalRotY && Mathf.Abs(rot.transform.eulerAngles.y-endLocalRotY)> 10) {
								yield return new WaitForSeconds (0.0001f);
								rot.transform.RotateAround (rot.transform.position, Vector3.up, -5);
						}
				} else if (direction == 0) {
						while (rot.transform.eulerAngles.y != startLocalRotY && Mathf.Abs(rot.transform.eulerAngles.y-startLocalRotY)> 10) {
								yield return new WaitForSeconds (0.0001f);
								rot.transform.RotateAround (rot.transform.position, Vector3.up, 5);
						}
				}

				Debug.Log (rot.transform.eulerAngles.y);

		}

}
