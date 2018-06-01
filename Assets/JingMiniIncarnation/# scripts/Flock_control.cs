using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock_control : MonoBehaviour {

	public GameObject[] objects;
	[Tooltip("targets_time and targets_multi can only work one at a time, either multi targets at the same time, or single target switching during time.")]
	// The targets_time and targets_multi can only work one at a time, either multi targets at the same time, or single targets switching.
	public GameObject[] targets_time; // targets changing during time, this allows the flock to switch targets.
	public GameObject[] targets_multi; // targets that affects the flock at the same time, this allows the flock to have unpredictable routes. 
	public List<Boid> boids;
	public float maxforce = 0.01f;    // Maximum steering force
	public float maxspeed = 0.5f;    // Maximum speed
	public float minspeed = -0.1f;
	private GameObject targetObj;     // Target object
	private GameObject[] targetObjs;   
	public float desiredseparation = 20.0f; // seperation distance
	public float neighbourdist = 50.0f; // neighbour distance
	public float target_senseRadius = 10.0f;
	public float timephase = 100.0f;
	public bool onPlane = false;

	Flock_control(){
	}

	// Use this for initialization
	void Start () {
		//		xxx.AddComponent<Boid>
		//      boids.Add(xxx);
		targetObj = targets_time [0];
		targetObjs = new GameObject[targets_multi.Length];
		for (int j = 0; j < targets_multi.Length; j++) {
			targetObjs [j] = targets_multi [j];
		}

		for (int i = 0; i < objects.Length; i++) {
			objects [i].AddComponent<Boid>() ;
			boids.Add (objects [i].GetComponent<Boid> ());
			boids [i].maxforce = maxforce;
			boids [i].maxspeed = maxspeed;
			boids [i].minspeed = minspeed;
			boids [i].targetObj = targetObj;
			// initialize multi targets 
			boids [i].targetObjs = new GameObject[targets_multi.Length];
			for (int j = 0; j < targets_multi.Length; j++) {
				boids [i].targetObjs[j] = targetObjs[j];
			}
			boids [i].desiredseparation = desiredseparation;
			boids [i].neighbourdist = neighbourdist;
			boids [i].target_senseRadius = target_senseRadius;
			boids [i].onPlane = onPlane;
		}
	}

	// Update is called once per frame
	void Update () {
		for (int i = 0; i < boids.Count; i++) {
			boids [i].run (boids);
		}
		// switch targets during time, 5 phases
		if (Time.time % timephase < timephase/5*1) {
			targetObj = targets_time [0];
		}else if(Time.time % timephase < timephase/5*2) {
			targetObj = targets_time [1];
		}else if(Time.time % timephase < timephase/5*3) {
			targetObj = targets_time [2];
		}else if(Time.time % timephase < timephase/5*4) {
			targetObj = targets_time [3];
		}else if(Time.time % timephase < timephase) {
			targetObj = targets_time [4];
		}

		for (int i = 0; i < objects.Length; i++) {
			boids [i].targetObj = targetObj;
		}
	}
}
