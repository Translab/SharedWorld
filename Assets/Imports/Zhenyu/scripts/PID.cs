using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PID{

	float reference;
	float error_p;
	float error_p_prev;
	float error_i;
	float error_d;
	float p;
	float i;
	float d;

	bool isOperational;

	public PID(float p, float i, float d){
		this.p = p;
		this.i = i;
		this.d = d;
		error_p_prev = 0;
		isOperational = false;
	}

	public void setReference(float reference){
		this.reference = reference;
		isOperational = true;
	}

	public float process(float dataIn){
		error_p = reference - dataIn;

		if(Mathf.Abs(error_p)<0.0001f){
			isOperational = false;
		}


		error_i+=error_p;
		error_d = error_p - error_p_prev;

		error_p_prev = error_p;

		return (p*error_p)+(i*error_i)+(d*error_d);
	}

	public bool isOn(){
		return isOperational;
	}

}
