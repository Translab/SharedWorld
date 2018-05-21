using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyLimit : MonoBehaviour
{
    //the idea of this fly limit is that, you use a collider set as trigger to check if 
    public GameObject controller;

    private VRTK.ControllerFly controllerfly;

    public float speed_limit = 0.5f;
    private float original_limit = 1.0f;
    public bool entered = false;
    public bool staying = false;
    private bool collision_state;

    // Use this for initialization
    void Start()
    {
        controllerfly = controller.GetComponent<VRTK.ControllerFly>();
        collision_state = controllerfly.collision_detection;
        original_limit = controllerfly.fly_speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (entered)
        {
            controllerfly.fly_speed = speed_limit;
        }
        else
        {
            controllerfly.fly_speed = original_limit;
        }
        Debug.Log((bool)controllerfly.collision_detection);

    }

    void OnTriggerEnter(Collider other)
    {
        if (VRTK.VRTK_PlayerObject.IsPlayerObject(other.gameObject))
        {
            if (collision_state == true){
                controllerfly.collision_detection = false;
            }
            entered = true;
        }
        Debug.Log ("speed limit zone entered");
    }
    void OnTriggerStay(Collider other)
    {
        if (VRTK.VRTK_PlayerObject.IsPlayerObject(other.gameObject))
        {
           //Debug.Log ("stayinnnnn");
           if (collision_state == true){
                controllerfly.collision_detection = false;
           }
            //staying = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (VRTK.VRTK_PlayerObject.IsPlayerObject(other.gameObject))
        {
            entered = false;
            if (collision_state == true){
                controllerfly.collision_detection = true;
            }
            Debug.Log ("speed limit zone left");
        }
        
    }

}