using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyLimitBound : MonoBehaviour
{
    //the idea of this fly limit is that, you use a collider set as trigger to check if 
    public GameObject controller;

    private VRTK.ControllerFly controllerfly;

    public float speed_limit = 0.5f;
    private float original_speed = 1.0f;
	private Vector3 origin;
	private Vector3 currentPos;
	private float dist;
	public float bound_size = 300.0f;
	private bool triggered = false;
    // Use this for initialization
    void Start()
    {
        controllerfly = controller.GetComponent<VRTK.ControllerFly>();
        //collision_state = controllerfly.collision_detection;
        original_speed = controllerfly.fly_speed;
		origin = transform.position;
	}

    // Update is called once per frame
    void Update()
    {
		currentPos = controllerfly.transform.position;
		dist = Vector3.Distance(currentPos, origin);
		//Debug.Log();
        if (dist > bound_size && !triggered)
        {
            Debug.Log("overlimit");
			Debug.Log((float)controllerfly.fly_speed);
			controllerfly.fly_speed = speed_limit;
			triggered = true;
        }
        else if (dist <= bound_size && triggered)
        {
            controllerfly.fly_speed = original_speed;
			triggered = false;
        }
        //Debug.Log((bool)controllerfly.collision_detection);

    }

    
}