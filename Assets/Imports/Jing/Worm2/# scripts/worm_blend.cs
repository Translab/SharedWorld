//Using C#

using UnityEngine;
using System.Collections;

public class worm_blend : MonoBehaviour
{

	int blendShapeCount;
	SkinnedMeshRenderer skinnedMeshRenderer;
	Mesh skinnedMesh;
	float blendOne = 0f;
	float blendTwo = 0f;
	public float blendSpeed = 1.0f;
	bool blendOneFinished = false;
	float distance = 100.0f;
//	public GameObject VRTK_SDK;
//	public GameObject CameraRig;
	SkinnedMeshRenderer skinRenderer;
	public float maxDistance=100.0f;
	public bool UseSimulator = true;

	void Awake ()
	{
		skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer> ();
		skinnedMesh = GetComponent<SkinnedMeshRenderer> ().sharedMesh;
		skinRenderer = GetComponent<SkinnedMeshRenderer> ();
		UseSimulator = true;

//		skinnedMeshRenderer.skinnedMotionVectors = false;
//		skinnedMeshRenderer.updateWhenOffscreen = false;
//		skinnedMeshRenderer.receiveShadows = false;
//		skinnedMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
//		skinnedMeshRenderer.quality = SkinQuality.Bone1;

	}

	void Start ()
	{

//		if (UseSimulator) {
//			//Debug.Log ("searching");
//			VRTK_SDK = GameObject.Find("VRTK_SDK_Setup");
//			CameraRig = VRTK_SDK.transform.Find("Simulator/VRSimulatorCameraRig").gameObject;
//
//		} else {
//			VRTK_SDK = GameObject.Find("VRTK_SDK_Setup");
//			CameraRig = VRTK_SDK.transform.Find("SteamVR/[CameraRig]").gameObject;
//			//CameraRig = GameObject.Find ("VRTK_SDK_Setup/SteamVR/[CameraRig]");
//
//		}


		blendShapeCount = skinnedMesh.blendShapeCount; 
		//print ("blendShapeCount " + blendShapeCount);
	}

	void Update ()
	{	
		//distance = Vector3.Distance (this.transform.position, CameraRig.transform.position);
		//if (distance < maxDistance && skinRenderer.isVisible) {
			blendOne += blendSpeed;
			skinnedMeshRenderer.SetBlendShapeWeight (0, blendOne);
			if (blendOne > 100f || blendOne < 0.0f) {
				blendSpeed = -blendSpeed;
			}
		//}
	}
}