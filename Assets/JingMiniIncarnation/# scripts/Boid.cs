using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {
	public float maxforce = 0.01f;    // Maximum steering force
	public float maxspeed = 0.5f;    // Maximum speed
	public float minspeed = -0.4f;
	public GameObject targetObj;     // Target object
	public GameObject[] targetObjs;  
	public float desiredseparation = 20.0f;
	public float neighbourdist = 50.0f;
	public float target_senseRadius = 30.0f;
	public bool onPlane = false;

	Vector3 acceleration = new Vector3 (0,0,0);
	Vector3 velocity = new Vector3 (0,0,0);
	Vector3 target = new Vector3(0,0,0);
	Vector3[] targets;

	Boid(){

	}

	public void run(List<Boid> boids){

		target = targetObj.transform.position;
		Vector3 targetDir = new Vector3 (0, 0, 0);

		targets = new Vector3[targetObjs.Length];
		for (int i = 0; i < targets.Length; i++) {
			targets [i] = targetObjs [i].transform.position;
		}

		// if there are multiple targets at the same time, calculate the nearest one and set direction
		if (targets.Length > 1) {
			int index = 0;
			float[] dis = new float[targets.Length];
			float minDis = 1000.0f;

			for (int i = 0; i < dis.Length; i++) {
				dis [i] = Vector3.Distance (transform.position, targets [i]);
				if (dis [i] < minDis) {
					minDis = dis [i];
					index = i;
				}
			}
			Debug.Log ("index "+index);
			targetDir = targets [index] - transform.position;
		} else { // if there is only one target at the same time
			targetDir = target - transform.position;
		}
			
		//rotate to target direction with transition
		//float step = Mathf.PI;
		float step = Mathf.PingPong (Time.deltaTime * 0.3f , 1);
		Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
		transform.rotation = Quaternion.LookRotation(newDir);

		flock (boids);
		update ();
	}

	void applyForce(Vector3 force){
		acceleration += force;	
	}

	void update () {
		velocity += acceleration;
		if (onPlane) {
			transform.position += new Vector3 (velocity.x, 0, velocity.z);
		} else {
			transform.position += velocity;
		}

		acceleration *= 0;
	}

	void flock(List<Boid> boids){
		Vector3 sep = seperation (boids);
		Vector3 ali = alignment (boids);
		Vector3 coh = cohesion (boids);

		Vector3 seekTarget = seek (target); // single target
		Vector3 arr = arrive (target);
		Vector3[] seekTargets = new Vector3[targetObjs.Length]; // multi targets
		Vector3[] arrs = new Vector3[targetObjs.Length];
		if (targets.Length > 1) {
			for (int i = 0; i < targetObjs.Length; i++) {
				seekTargets [i] = seek (targets [i]);
				arrs [i] = arrive (targets [i]);
			}
		}

		Vector3 gdDetect = groundDetection (2.0f, 50.0f); //first is height threshold, second is ground height
		gdDetect *= 1.5f;
		sep *= 1.5f;
		ali *= 1.0f;
		coh *= 1.0f;

		if (targets.Length > 1) { // multi targets
			for (int i = 0; i < targetObjs.Length; i++) {
				seekTargets [i] *= 0.8f;
				arrs [i] *= 1.0f;
			}
		} else { // single target
				seekTarget *= 0.8f;
				arr *= 1.0f;
		}

		applyForce (gdDetect);
		applyForce (sep);
		applyForce (ali);
		applyForce (coh);

		if (targets.Length > 1) { // multi targets
			for (int i = 0; i < targetObjs.Length; i++) {
				applyForce (seekTargets[i]);
				applyForce (arrs[i]);
			}
		} else { // single target
				applyForce (seekTarget);
				applyForce (arr);
		}
	}

	Vector3 seek(Vector3 target){
		Vector3 desired = target - transform.position;
	    desired = Vector3.Normalize(desired);
		desired *= maxspeed;
		Vector3 steer = desired - velocity;
		if (steer.magnitude > maxforce) {
			steer = Vector3.ClampMagnitude (steer, maxforce);
		}
		return steer;
	}
	Vector3 groundDetection(float heightThreshold, float groundHeight){
		float height = transform.position.y - groundHeight;
		Vector3 steer = new Vector3 (0, 0, 0);
		if (height <= heightThreshold) {
			steer = new Vector3 (0, Mathf.Abs(1 / height), 0);
			if (steer.magnitude > 0) {
				steer = Vector3.Normalize (steer);
				steer *= maxspeed;
				steer -= velocity;

				if (steer.magnitude > maxforce) {
					steer = Vector3.ClampMagnitude (steer, maxforce);
				}
			}
		} 
		return steer;
	}

	Vector3 seperation(List<Boid> others){
		Vector3 steer = new Vector3 (0, 0, 0);
		float count = 0.0f;

		foreach(Boid b in others){
			float distance = Vector3.Distance (transform.position, b.transform.position);
			if (distance > 0 && distance < desiredseparation) {
				Vector3 diff = transform.position - b.transform.position;
				diff = Vector3.Normalize (diff);
				diff /= distance;
				steer += diff;
				count++;
			}
		}

		if (count > 0.0f) {
			steer /= count;
		}
		if (steer.magnitude > 0) {
			steer = Vector3.Normalize (steer);
			steer *= maxspeed;
			steer -= velocity;

			if (steer.magnitude > maxforce) {
				steer = Vector3.ClampMagnitude (steer, maxforce);
			}
		}
		return steer;
	}

	Vector3 alignment(List<Boid> others){
		Vector3 sum = new Vector3 (0,0,0);
		float count = 0;
	
		foreach (Boid b in others) {
			float distance = Vector3.Distance (transform.position, b.transform.position);
			if (distance < neighbourdist) {
				sum += b.velocity;
				count++;
			}
		}
		if (count > 0) {
			sum = Vector3.Normalize (sum);
			sum *= maxspeed;
			Vector3 steer = sum - velocity;
			if (steer.magnitude > maxforce) {
				steer = Vector3.ClampMagnitude (steer, maxforce);
			}
			return steer;
		} else {
			return new Vector3 (0,0,0);
		}
	}

	Vector3 cohesion(List<Boid> others){
		float count = 0;
		Vector3 sum = new Vector3 (0,0,0);
		
		foreach (Boid b in others) {
			float distance = Vector3.Distance (transform.position, b.transform.position);

			if (distance < neighbourdist) {
				sum += b.transform.position;
				count++;
			}
		}
		if (count > 0) {
			sum /= count;
			return seek (sum);
		} else {
			return new Vector3 (0, 0, 0);
		}
	}

	// arrive code

	Vector3 arrive(Vector3 target){
		Vector3 desired = target - transform.position;
		float d = desired.magnitude;
		desired = Vector3.Normalize (desired);
		if (target_senseRadius > 0 && d < target_senseRadius) {
			float m = minspeed + (maxspeed - minspeed) * d / target_senseRadius;
			desired *= m;
		} else {
			desired *= maxspeed;
		}
		Vector3 steer = desired - velocity; // not sure
		if (steer.magnitude > maxforce) {
			steer = Vector3.ClampMagnitude (steer, maxforce);
		}
		return steer;
	}

}
