using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using UnityEngine;

public class renderUserMap : MonoBehaviour {
	public KinectManager km;
	public GameObject camera;
	private Texture2D allMap;
	private Texture2D userMap;
	private Renderer mrenderer;
	private ushort[] depth_data;
	private int canvas_width;
	private int canvas_height;

	// Use this for initialization
	void Start () {
		//km = GetComponent<KinectManager>();
		km = camera.GetComponent<KinectManager>();
		mrenderer = GetComponent<MeshRenderer>();
		

		//depth, width, height
		depth_data = km.GetRawDepthMap();
		canvas_height = KinectWrapper.GetDepthHeight();
		canvas_width = KinectWrapper.GetDepthWidth();
		// Debug.Log(canvas_height);
		// Debug.Log(canvas_width);
		// Debug.Log(depth_data.Length);
		
	}
	
	// Update is called once per frame
	void Update () {
		allMap = km.usersClrTex;
		userMap = km.usersLblTex;
		mrenderer.material.mainTexture = userMap;
		//Debug.Log(userMap.width);
	}
}
