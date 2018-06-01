using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class random_move : MonoBehaviour {
	private float randX; 
	private float randZ; 
	private Vector3 nextPosition;
	private Vector3 nextRotation;
	private Vector3 originPosition;
	public float stepX = 5.0f;
	public float stepZ = 1.0f;
	public float speed = 0.05f;
	public float rangeX = 50.0f;
	private float dis;
	private float amount;

	// Use this for initialization
	void Start () {
		originPosition = transform.position;
		randX = Random.Range(0,stepX);
		randZ = Random.Range(-stepZ,stepZ);
	 	nextPosition = new Vector3 (transform.position.x+randX, transform.position.y, transform.position.z+randZ);
	}
	
	// Update is called once per frame
	void Update () {
		dis = Mathf.Sqrt (randX * randX + randZ * randZ);
		amount = Mathf.PingPong (Time.deltaTime / dis * speed , 1);

		nextRotation = nextPosition - transform.position;
		Vector3 newDirection = Vector3.RotateTowards (transform.forward, nextRotation, amount, 0.0f);
		transform.rotation = Quaternion.LookRotation(newDirection);
		transform.position = Vector3.Lerp(transform.position, nextPosition, amount); 
	
		if (Vector3.Distance (transform.position, nextPosition) < 0.01f) {
			randX = Random.Range(0,stepX);
			randZ = Random.Range(-stepZ,stepZ);
			nextPosition = new Vector3 (transform.position.x+randX, transform.position.y, transform.position.z+randZ);
		}
		if (transform.position.x > rangeX) {
			transform.position = new Vector3(originPosition.x,transform.position.y,originPosition.z);
			nextPosition = new Vector3 (transform.position.x+randX, transform.position.y, transform.position.z+randZ);
		}
	}
}
