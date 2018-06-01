using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blend_shape : MonoBehaviour {
	int blendShapeCount;
	SkinnedMeshRenderer skinnedMeshRenderer;
	Mesh skinnedMesh;
	private int index = 0;
	private bool increasing = true;
	float blendAmount = 0f;
	public float blendSpeed = 1f;

	void Awake ()
	{
		skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer> ();
		skinnedMesh = GetComponent<SkinnedMeshRenderer> ().sharedMesh;
	}

	// Use this for initialization
	void Start () {
		blendShapeCount = skinnedMesh.blendShapeCount; 
	}
	
	// Update is called once per frame
	void Update () {

		blendAmount += blendSpeed;
		skinnedMeshRenderer.SetBlendShapeWeight (index, blendAmount);
		if (increasing) {
			if (blendAmount > 100.0f) {
				if (index < blendShapeCount - 1) {
					index++;
					blendAmount = 0;
				} else {
					blendSpeed = -blendSpeed;
					increasing = false;
				}
			}
		} else {
			if (blendAmount < 0.0f) {
				if (index > 0) {
					index--;
					blendAmount = 100.0f;
				} else {
					blendSpeed = -blendSpeed;
					increasing = true;
				}

			}
		}
	}
}
