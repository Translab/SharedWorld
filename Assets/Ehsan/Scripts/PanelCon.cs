using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelCon : MonoBehaviour {
	public Transform target;
    Mesh m;
	Vector3[] verts;
	Vector3[] lastverts;

	Color[] cols;
	// Use this for initialization
	void Start () {
		m = GetComponent<MeshFilter> ().mesh;
		lastverts = (Vector3[]) m.vertices.Clone ();
		verts = (Vector3[]) m.vertices.Clone ();
		cols = new Color[m.vertexCount];


	}
	
	// Update is called once per frame
	void Update () {
		if (target) {
			float noise=0f;
			for(int i=0;i<cols.Length;i++){
				noise = Mathf.PerlinNoise (m.vertices [i].y + Time.time, m.vertices [i].x + Time.time)*0.3f;
				cols [i] = new Color (noise+(1f/(target.position - transform.TransformPoint (m.vertices [i])).magnitude), 1f, 1f);
			}
			m.colors = cols;
		} else {
			target = Camera.main.transform;
			//target = GameObject.Find ("yin's head").transform;
		}
	}
}
