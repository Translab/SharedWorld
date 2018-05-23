using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class pullAlarm : MonoBehaviour {

	public GameObject mainCanvas;
	public GameObject[] lightBlink;
	
	void Start () {
		mainCanvas = GameObject.FindGameObjectWithTag("Canvas");
		lightBlink = GameObject.FindGameObjectsWithTag ("FireStrobe");
	}

	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == ("Player")){
			Text t =	mainCanvas.GetComponentInChildren<Text>();
			t.text = "Press P to Activate";
			t.enabled = true;
		}
	}
	
	void OnTriggerStay(Collider other){
		if (other.gameObject.tag == ("Player")) {
			if(Input.GetKeyDown(KeyCode.P)){
				Text t =	mainCanvas.GetComponentInChildren<Text>();
				t.text = "";
				t.enabled = false;
				foreach(GameObject lb in lightBlink){
				LightBlink lightBlinkScript	= lb.GetComponent<LightBlink>();
				lightBlinkScript.StartCoroutine("blink");
				lb.GetComponentInParent<AudioSource>().Play();

				}
			}
		}
	}
	
	void OnTriggerExit(Collider other){
		if(other.gameObject.tag == ("Player")){
			//mainCanvas.GetComponent<GuiT
			Text t =	mainCanvas.GetComponentInChildren<Text>();
			t.text = "";
			t.enabled = false;
		}
	}
}
