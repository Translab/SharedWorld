﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Size : MonoBehaviour
{

    Animator anim;

    // Use this for initialization
    void Start()
    {
        //which animatior will be used
        anim = gameObject.GetComponent<Animator>();


    }

    //function is happen when the trigger is enabled
    void OnTriggerEnter()
    {
        anim.SetTrigger("sizedown");
    }

    void OnTriggerExit()
    {
        anim.SetTrigger("sizeup");
    }
}