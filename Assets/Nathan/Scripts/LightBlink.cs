using UnityEngine;
using System.Collections;

public class LightBlink : MonoBehaviour {


	float timer = 1;
	void Start () {
		GetComponent<Light>().enabled = false;
	}

	public IEnumerator blink(){

		while (true) {
			yield return  new WaitForSeconds(timer);
			GetComponent<Light>().enabled = true;
			yield return new WaitForSeconds(0.02f);
			GetComponent<Light>().enabled = false;
		}
	}
}
