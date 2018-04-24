using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scatter : MonoBehaviour {

	//three main things here, component(s) as reference unit for the cloud, targets for morphing, clones of components to formulate cloud
	public GameObject[] components;
	public GameObject[] geo_targets;
	GameObject clones;

	//2D list to save vertices data list
	private List<List<Vector3>> world_targets = new List<List<Vector3>>();

	//list of matrices for converting vertices from local to world
	private List<Matrix4x4> matrices = new List<Matrix4x4>();

	//list of copied objects to move around as cloud
	private List<GameObject> copies = new List<GameObject>();
	private List<int> skip_step = new List<int> (); // for geometries that have more vertices than original mesh.

	//animation, lerp value, seed
	private float lerp_value = 0;
	//private float cycle = 10;
	public float lerp_speed = 0.005f;
	private Quaternion rotation = Quaternion.Euler(0,0,0);
	private float seed;
	private int morph_step = 0;
	public bool skip_first_target = true;
	public float morph_stay_time = 0.5f;
	public bool simulate = true;
	public bool moving = true;
	public float geo_scale = 2.0f;
	public float comp_scale = 6.0f;
	private GameObject placeholder;

	void Start () {
		seed = Random.Range (0, 100); // can take fixed seed

//		//create placeholder / parent
		placeholder = new GameObject();
		placeholder.name = "Morphing Components";

		//check targets number and store their vertices lists into a list, also transform local vertices into world positions
		for (int i = 0; i < geo_targets.Length; i++) {
			//scale
			//geo_targets [i].transform.localScale *= scaleFactor;

			world_targets.Add(new List<Vector3> ());
			//Matrix4x4 m = geo_targets[i].transform.localToWorldMatrix;
			//can be done similiarly like this, with more specific transformation parameters of rotation and scale
			Matrix4x4 m = Matrix4x4.TRS (geo_targets [i].transform.position, geo_targets [i].transform.rotation, geo_targets [i].transform.localScale * geo_scale);
			matrices.Add(m);
			for (int j = 0; j < geo_targets [i].GetComponent<MeshFilter>().sharedMesh.vertices.Length; j++) {
				world_targets[i].Add(matrices[i].MultiplyPoint3x4 (geo_targets[i].GetComponent<MeshFilter>().sharedMesh.vertices[j]));
			} 
		}
	
		//create copies of components based on the first target vertices positions and numbers
		for (int i = 0; i < world_targets[0].Count; i++) {
			int dice = (int)(Mathf.PerlinNoise(i/50.0f + seed, i/50.0f + 100 - seed) * components.Length);
			rotation.eulerAngles = new Vector3(Random.Range(0.0f,90.0f), Random.Range(0.0f,90.0f), Random.Range(0.0f, 90.0f));
			for (int j = 0; j < components.Length; j++) {
				if (dice == j) {
					clones = Instantiate (components [j], world_targets[0][i], rotation);
					clones.transform.localScale *= comp_scale;
					clones.transform.SetParent (placeholder.transform);
				}
			}
			copies.Add (clones);
		}

		//calculate the difference between first target and other targets for a rough match with different numbers of their faces
		for (int i = 0; i < geo_targets.Length; i++) {
			skip_step.Add((int) Mathf.Floor (world_targets[i].Count / world_targets[0].Count));
			//Debug.Log("i = " + i + "skip_step = " + skip_step[i] + " now");
		}

		//check if skip first target
		if (skip_first_target) {
			morph_step = 1;
		}

	}

	void Update () {
		//lerp_value = Mathf.PingPong (Time.time * 2.0f / cycle, 1);
		if (lerp_value < 1 + morph_stay_time) {
			if (simulate) {
				lerp_value += lerp_speed;
			} else {
			}

		} else if (lerp_value >= 1 + morph_stay_time && morph_step != geo_targets.Length - 1) {
			lerp_value = 0;
			morph_step += 1;
			if (moving){
				if (morph_step + 1 != geo_targets.Length) {
					Matrix4x4 m = Matrix4x4.TRS (geo_targets [morph_step + 1].transform.position, geo_targets [morph_step + 1].transform.rotation, geo_targets [morph_step + 1].transform.localScale * geo_scale);
					for (int i = 0; i < copies.Count; i++) {
						world_targets [morph_step + 1] [i * skip_step [morph_step + 1]] = m.MultiplyPoint3x4 (geo_targets [morph_step + 1].GetComponent<MeshFilter> ().sharedMesh.vertices [i * skip_step [morph_step + 1]]);
					}
				} else {
					if (!skip_first_target) {
						Matrix4x4 m = Matrix4x4.TRS (geo_targets [0].transform.position, geo_targets [0].transform.rotation, geo_targets [0].transform.localScale * geo_scale);
						for (int i = 0; i < copies.Count; i++) {
							world_targets [0] [i * skip_step [0]] = m.MultiplyPoint3x4 (geo_targets [0].GetComponent<MeshFilter> ().sharedMesh.vertices [i * skip_step [0]]);
						}
					} else {
						Matrix4x4 m = Matrix4x4.TRS (geo_targets [1].transform.position, geo_targets [1].transform.rotation, geo_targets [1].transform.localScale * geo_scale);
						for (int i = 0; i < copies.Count; i++) {
							world_targets [1] [i * skip_step [1]] = m.MultiplyPoint3x4 (geo_targets [1].GetComponent<MeshFilter> ().sharedMesh.vertices [i * skip_step [1]]);
						}
					}
				}
			}
		} else if (lerp_value >= 1 + morph_stay_time && morph_step == geo_targets.Length - 1) {
			if (skip_first_target) {
				morph_step = 1;

			} else {
				morph_step = 0;
			}
			if (moving){
				Matrix4x4 m = Matrix4x4.TRS (geo_targets [morph_step + 1].transform.position, geo_targets [morph_step + 1].transform.rotation, geo_targets [morph_step + 1].transform.localScale * geo_scale);
				for (int i = 0; i < copies.Count; i++) {
					world_targets [morph_step + 1] [i * skip_step [morph_step + 1]] = m.MultiplyPoint3x4 (geo_targets [morph_step + 1].GetComponent<MeshFilter> ().sharedMesh.vertices [i * skip_step [morph_step + 1]]);
				}
			}
			lerp_value = 0;
		}
		if (morph_step == geo_targets.Length - 1) {
			if (skip_first_target) {
				for (int i = 0; i < copies.Count; i++) {
					copies [i].transform.position = Vector3.Lerp (world_targets [geo_targets.Length - 1] [i * skip_step [geo_targets.Length - 1]], world_targets [1] [i * skip_step [1]], lerp_value);
				}
			} else {
				for (int i = 0; i < copies.Count; i++) {
					copies [i].transform.position = Vector3.Lerp (world_targets [geo_targets.Length - 1] [i * skip_step [geo_targets.Length - 1]], world_targets [0] [i * skip_step [0]], lerp_value);
				}
			}
		} else {
			for (int i = 0; i < copies.Count; i++) {
				copies [i].transform.position = Vector3.Lerp (world_targets [morph_step] [i * skip_step [morph_step]], world_targets [morph_step + 1] [i * skip_step [morph_step + 1]], lerp_value);
			}
		}

	}


		
}
