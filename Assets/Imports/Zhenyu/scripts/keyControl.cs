using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keyControl : MonoBehaviour {
//	AppleSculptureControl appleSculptureControl;
	GameObject support;
	GameObject apple;

	GameObject lightGameObject1;

	int frameCount = 0;

	int flashLightMode = 0;

//	Light lightComp1;
//	Light lightComp2;

	void Start() {

		lightGameObject1 = new GameObject("light1");
		lightGameObject1.transform.position = new Vector3(AppleSculptureControl.Instance.rangeX, 10, 0);
		lightGameObject1.transform.Rotate (40, -90, 0);

//		lightComp1 = lightGameObject1.AddComponent<Light>();
//		lightComp1.color = Color.white;
//		lightComp1.intensity = 3;
//		lightComp1.range = 150f;
//		lightComp1.shadows = LightShadows.Soft;
//		lightComp1.type = LightType.Directional;


		for (int i = 0; i < 50; i++) {
			AppleSculptureControl.Instance.addSculpture ();
		}


		//ChairArrayManager.Instance.addChairs ();

		GameObject.Find ("secretRoom").transform.position += new Vector3 (0f, -300f, 0f);
	}

	void Update() {


//		lightGameObject1.transform.eulerAngles = new Vector3(toPN180Range(360f*TimeManager.Instance.getDayProgress()),toPN180Range(-180f+360f*TimeManager.Instance.getDayProgress()),0);
//
//		if (toPN180Range(360f*TimeManager.Instance.getDayProgress()) <0 ) {
//			lightComp1.intensity = 0;
//		} else {
//			lightComp1.intensity = 3;
//		}

		//print (toPN180Range(360f*TimeManager.Instance.getDayProgress()));
//		print (GameObject.Find ("clock").transform.position);
//		print ((GameObject.Find("clock").transform.position+GameObject.Find("clock").transform.up));
//		print ((-180f+360f*((TimeManager.Instance.getMinute())/60f))+" : "+(TimeManager.Instance.getMinute()));
		//print ("lightComp2.intensity = "+(8f - lightComp2.intensity));

		if (Input.GetKeyDown("space"))
			print("space key was pressed");

	

		if (Input.GetKeyDown ("e")) {
			print ("e key was pressed to test");
			AppleSculptureControl.Instance.addSculpture();
		}


		if (Input.GetKeyDown ("r")) {
			print ("r key was pressed to test");
			AppleSculptureControl.Instance.removeAll();
		}

		if (Input.GetKeyDown ("f")) {
			print ("f key was pressed to test");
			Light fl = GameObject.Find ("flashLight").GetComponent<Light> ();
			if (flashLightMode == 0) {
				fl.intensity = 0f;
			}
			else if(flashLightMode == 1) {
				fl.intensity = 5.3f;
				fl.color = Color.white;
			}

			else if(flashLightMode == 2) {
				fl.intensity = 5.3f;
				fl.color = Color.red;
			}

			if (flashLightMode < 2) {
				flashLightMode++;
			} else{
				flashLightMode = 0;
			}

		}

		frameCount++;
	}



	float toPN180Range(float a){

		if (a > 180) {
			a = 180 - a;
		}
		return a;
	}
}
