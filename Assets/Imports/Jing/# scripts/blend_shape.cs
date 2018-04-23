using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blend_shape : MonoBehaviour {
	int blendShapeCount;
	SkinnedMeshRenderer skinnedMeshRenderer;
	Mesh skinnedMesh;
	float blendOne = 0f;
	float blendTwo = 0f;
	public float blendSpeed = 1f;
	bool blendOneFinished = false;

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
		blendOne += blendSpeed;
		skinnedMeshRenderer.SetBlendShapeWeight (0, blendOne);
		if (blendOne > 100f || blendOne < 0.0f) {
			blendSpeed = -blendSpeed;
		}

	}
}
