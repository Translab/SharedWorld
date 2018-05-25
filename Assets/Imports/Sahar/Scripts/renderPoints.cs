using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using UnityEngine;

public class renderPoints : MonoBehaviour {
	public KinectManager km;
	public GameObject camera;
	private Texture2D allMap;
	private Texture2D userMap;
	//private Renderer mrenderer;
	private MeshRenderer meshrenderer;
	private MeshFilter meshfilter;
	private List<Vector3> vertices = new List<Vector3>();
	int numPoints;
	Vector3[] points;
	private Mesh mesh;
	private ushort[] depth_data;
	private int canvas_width;
	private int canvas_height;
	private int index;

	private int minDepth=10;
	private int maxDepth=400;

	public Vector4 v4pos;

	GameObject[] cubes;

	// Use this for initialization
	void Start () {
		//km = GetComponent<KinectManager>();
		km = camera.GetComponent<KinectManager>();
		//mrenderer = GetComponent<MeshRenderer>();
		meshrenderer = GetComponent<MeshRenderer>();
		meshfilter= GetComponent<MeshFilter>();
		mesh = new Mesh();
		Vector3 pos = new Vector3(0,0,0);

		meshfilter.mesh = mesh;

		depth_data = km.GetRawDepthMap();
		canvas_height = KinectWrapper.GetDepthHeight() /2;
		canvas_width = KinectWrapper.GetDepthWidth() / 2;
		//numPoints = canvas_height * canvas_width;
		numPoints = canvas_height * canvas_width;
		

		meshrenderer.material = new Material(Shader.Find("Tim/PointShader"));
		//meshrenderer.SetWidth(0.02f, 0.02f); //thickness of line
		Color c_white = Color.white;
		Color c_red = Color.red;

		
		for (int i = 0; i < canvas_width; i ++){
			for (int j = 0; j < canvas_height; j ++){
				vertices.Add(new Vector3(i * 0.01f,j * 0.01f,0));
			}
		}

		points = new Vector3[numPoints];
        int[] indicies = new int[numPoints];
        Color[] colors = new Color[numPoints];
        for (int i = 0; i < points.Length; ++i)
        {
            points[i] = new Vector3(UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10));
            indicies[i] = i;
            colors[i] = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);
			//mesh.normals[i] = new Vector3(0, 0, 1);
        }

        mesh.vertices = points;
        mesh.colors = colors;
        mesh.SetIndices(indicies, MeshTopology.Points, 0);


		//depth, width, height
		
		Debug.Log(numPoints);
		Debug.Log(canvas_width);
		Debug.Log(canvas_height);
		
		Debug.Log(depth_data.Length);
		
	}
	
	// Update is called once per frame
	void Update () {

		depth_data = km.GetRawDepthMap();
		
		ushort max_depth = 0;
		ushort min_depth = 65000;
		for (int i = 0; i < canvas_width; i ++){
			for (int j = 0; j < canvas_height; j ++){

				//width upside down
				index = i * canvas_height + j;
				//numPoints-(i * canvas_height + j)-1;
				// if (depth_data[index] > max_depth){
				// 	max_depth = depth_data[index];

				// }
				// if (depth_data[index] < min_depth){
				// 	min_depth = depth_data[index];
				// }
				//Vector4 pos = KinectWrapper.NuiTransformDepthImageToSkeleton((long)i, (long)j, depth_data[i * canvas_width + j]);
				
				
				float depth = mapValue((float)km.GetDepthForPixel(i * 2,j * 2), 0.00f, 31800.00f, 0.00f, 64.00f);
				
				//ushort depthPixel = km.GetDepthForPixel(j,i);
				//Vector3 pos = new Vector3((float)i * 0.1f, (float)j * 0.1f, (float)depth_data[index] / 1000.0f);
				Vector3 pos = new Vector3((float)(canvas_width-i-1) * 0.1f, (float)(canvas_height-j-1) * 0.1f, (float)depth);
				//Vector3 pos = new Vector3((float)j * 0.1f, (float)i * 0.1f, 0 );

				if ((depth<minDepth)||(depth>maxDepth)){
				     pos=new Vector3(0.0f, 0.0f, 0.0f);
				}
				points[index] = pos;
				//points[numPoints-index-1] = pos;
			}
		}
		mesh.vertices = points;
		
		//Debug.Log(max_depth + "  maxxxxx");
		//Debug.Log(min_depth + " minnnnn");
	}

	public float mapValue(float value, float minA, float maxA, float minB, float maxB){
		return (value - minA) * ((maxB - minB) / (maxA - minA)) + minB;
	}

}
