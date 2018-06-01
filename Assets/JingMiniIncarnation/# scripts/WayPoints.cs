using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoints : MonoBehaviour {

	public GameObject[] waypoints;
	private int num = 0;
	public float minDist;
	[Tooltip("speed must be less than minDist")]
	public float speed;
	public bool rand = false;
	public bool go = true;
	public Vector3 nextRotation;
	private Vector3 rotationAngles;
	private Vector3 originPos;

	void Start()
	{
		originPos = transform.position;
	}

	void Update ()
	{
		float dist = Vector3.Distance(gameObject.transform.position, waypoints[num].transform.position);
		if (go){
			if (dist > minDist){
				Move ();
			}else{
				if (!rand){
					if (num + 1 == waypoints.Length) {
						go = false;
					} else {
						num++;
					}
				} else {
					num = Random.Range (0, waypoints.Length);
				}
			}
		} else {
			transform.position = originPos;
			num = 0;
			go = true;
		}
	}
	public void Move(){
		Vector3 temPos = waypoints [num].transform.position-gameObject.transform.position;
		Quaternion lastPos = Quaternion.LookRotation(temPos);
		transform.rotation = Quaternion.Slerp(transform.rotation,lastPos, Time.deltaTime * 1.0f);
		//gameObject.transform.LookAt(waypoints[num].transform.position);
		gameObject.transform.position += gameObject.transform.forward * speed * Time.deltaTime;
	}
}
