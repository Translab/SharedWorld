using UnityEngine;
using System.Collections;

public class HeadSpinScript : MonoBehaviour {
	public float speed = 2f;

	void Update() {
		transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.World);
	}
}
