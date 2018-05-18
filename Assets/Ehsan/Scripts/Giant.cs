using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Giant : MonoBehaviour {
    Rigidbody myrigidbody;
    public float forceamount;
    public float stableheight;

	// Use this for initialization
	void Start () {
        myrigidbody = GetComponent<Rigidbody>();

    }
	
	// Update is called once per frame
	void Update () {
        myrigidbody.AddForce(new Vector3(0, (stableheight - transform.position.y)* forceamount, 0));
        myrigidbody.AddTorque(transform.right*10f);

        //myrigidbody.AddForce()
	}

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 dev;
        dev = Vector3.Cross(transform.right, transform.up);
    }
}
