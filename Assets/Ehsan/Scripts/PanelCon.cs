using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelCon : MonoBehaviour {
	public Transform target1;
	public Transform target2;
	public Material mat;
	public float len;
	public GameObject[] panelprefabs;
	// Use this for initialization
	void Start () {
		for (int i = 0; i < 20; i++) {
			int n = Random.Range (0, panelprefabs.Length);
			Quaternion rot = transform.rotation;
			//GameObject o = GameObject.Instantiate (panelprefabs [n], transform.position + new Vector3 (0, 0, i * len),rot) as GameObject;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (target1) {

			mat.SetVector ("target1", target1.position);

		} else {
			target1 = Camera.main.transform;
		}

		if (target2) {
			mat.SetVector ("target2", target2.position);
		}
	}
}
