//using UnityEngine;
//using System.Collections;
//
//public class Rotation : MonoBehaviour {
//
//
//	public GameObject sphere;
//	public GameObject sphere2;
//	public GameObject rotationPoint;
//
//	public Vector3 rotAxis;
//	public Vector3 rotAxis2;
//	// Use this for initialization
//	void Start () {
//		//rotAxis = rotationPoint.transform.up;
//
//		rotAxis = Vector3.Cross (sphere.transform.position, rotationPoint.transform.position);
//
//		rotAxis2 = Vector3.Cross (sphere2.transform.position, sphere.transform.position);
//
//	
//	}
//	
//	// Update is called once per frame
//	void Update () {
//	//	Vector3.
//		//rotAxis = Vector3.Cross (sphere.transform.position, rotationPoint.transform.position);
//		//rotAxis = new Vector3 (rotAxis.x-3.0f, rotAxis.y+ 5.0f,rotAxis.z+ 5.0f).normalized;
//		//Debug.Log (rotAxis);
//		//Vector3.r
//		sphere.transform.RotateAround (rotationPoint.transform.position, rotAxis, 5);
//
//		sphere2.transform.RotateAround (sphere.transform.position, Vector3.up, 5);
//	
//	}
//}
