using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class DistanceParameters : MonoBehaviour {

	public float size = 5.0f; 
	public int type = 0;

	public Vector3 offset = new Vector3(0, 0, 0);

	void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + offset, size);
    }
}