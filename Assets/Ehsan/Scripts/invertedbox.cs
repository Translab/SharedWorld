using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class invertedbox : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Mesh m = GetComponent<MeshFilter> ().mesh;
		int[] newtris = new int[m.triangles.Length];
		for (int i = 0; i < m.triangles.Length/3; i++) {
			newtris [i*3] = m.triangles [i*3+2];
			newtris [i*3+1] = m.triangles [i*3+1];
			newtris [i*3+2] = m.triangles [i*3];

		}
		m.triangles = newtris;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
