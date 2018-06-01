using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidateLoop : MonoBehaviour {

	// Use this for initialization
	void OnValidate() {
		 GameObject[] allGameObjects = FindObjectsOfType<GameObject>();
 			foreach (GameObject go in allGameObjects)
 		{
     	go.hideFlags = HideFlags.None;
     	Component[] components = go.GetComponents(typeof(Component));
     	for (int i = 0; i < components.Length; i++)
     	{
         components[i].hideFlags = HideFlags.None;
     	}
		}
	}
	
	
}
