using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using UnityEngine;
public class renderPointBody : MonoBehaviour {
	public KinectManager km;
	public GameObject camera;
	//private Renderer mrenderer;
	private MeshRenderer meshrenderer;
	private MeshFilter meshfilter;
	private List<Vector3> vertices = new List<Vector3>();
	private List<Vector3> mPoints = new List<Vector3>();
	private List<Color> mColors = new List<Color>();
	private List<int> mIndices = new List<int>();
	private List<Vector3> vertices_inter = new List<Vector3>();
	private List<int> triangle_indices = new List<int>();
	private int mIndex = 0;
	private Mesh mesh;
	private ushort[] depth_data;
	private int canvas_width;
	private int canvas_height;
	private int index;
	public float minDist = 0;
	public float maxDist = 20;

	//test line generation
	private List<GameObject> lines;
	private List<LineRenderer> newLines;


	// Use this for initialization
	void Start () {
		km = camera.GetComponent<KinectManager>();
		meshrenderer = GetComponent<MeshRenderer>();
		meshfilter= GetComponent<MeshFilter>();
		mesh = new Mesh();

		meshfilter.mesh = mesh;

		depth_data = km.GetRawDepthMap();
		canvas_height = KinectWrapper.GetDepthHeight() /2;
		canvas_width = KinectWrapper.GetDepthWidth() / 2;

		meshrenderer.material = new Material(Shader.Find("Tim/PointShader"));
		
	}
	
	// Update is called once per frame
	void Update () {

		mesh.Clear();
		vertices.Clear();
		mColors.Clear();
		mIndices.Clear();
		vertices_inter.Clear();
		triangle_indices.Clear();
		// lines.Clear();
		// newLines.Clear();
		mIndex = 0;	

		depth_data = km.GetRawDepthMap();
		
		for (int i = 0; i < canvas_width; i ++){
			for (int j = 0; j < canvas_height; j ++){
				//width upside down
				index = i * canvas_height + j;
				//get depth
				float depth = mapValue((float)km.GetDepthForPixel(i * 2,j * 2), 0.00f, 31800.00f, 0.00f, 64.00f);
				
				//check if depth is beyond threshold, so its part of body not background
				if (depth > minDist && depth < maxDist){
					Vector3 pos = new Vector3((canvas_width-i-1) * 0.1f, (float)(canvas_height-j-1) * 0.1f, (float)depth);
					vertices.Add(pos);
					mColors.Add(Color.red);
					mIndices.Add(mIndex);
					
					//add lines
					// Vector3 pos2 = new Vector3((canvas_width-i-1) * 0.1f, (float)(canvas_height-j-1) * 0.1f, (float)depth + 2);
					// lines.Add(new GameObject());
             		// newLines.Add(new LineRenderer());
					// newLines[mIndex] = lines[mIndex].AddComponent<LineRenderer>();
					// newLines[mIndex].material = new Material(Shader.Find("Particles/Additive"));
             		// newLines[mIndex].SetWidth (0.1f, 0.1f);
					// newLines[mIndex].SetPosition(0, pos);
					// newLines[mIndex].SetPosition(1, pos2);
					
					mIndex ++;
				}
			}
		}

		// int tIndex = 0;
		// for (int i = 0; i < canvas_width - 1; i ++){
		// 	for (int j = 0; j < canvas_height - 1; j ++){
		// 		// index = i * canvas_height + j;
		// 		triangle_indices.Add(tIndex); // 0 , 1
		// 		triangle_indices.Add(tIndex + 1); // 1, 2
		// 		triangle_indices.Add(tIndex + canvas_height); // 240, 241
		// 		tIndex ++;
		// 	}
		// }


		// int vertexCount = vertices.Count;
		// for (int i = 0; i < vertexCount; i ++){
		// 	Vector3 pos = new Vector3(vertices[i].x + 0.3f, vertices[i].y + 0.3f, vertices[i].z + 0.25f);
		// 	vertices.Add(pos);
		// 	mColors.Add(Color.red);
		// 	mIndices.Add(mIndex);
		// 	mIndex ++;
		// 	pos = new Vector3(vertices[i].x, vertices[i].y, vertices[i].z - 0.25f);
		// 	vertices.Add(pos);
		// 	mColors.Add(Color.red);
		// 	mIndices.Add(mIndex);
		// 	mIndex ++;
		// }

		// for (int i = 0; i < vertices.Count; i += 3){
		// 	vertices_inter.Add(vertices[i]);
		// }
		// for (int i = 1; i < vertices.Count; i += 3){
		// 	vertices_inter.Add(vertices[i]);
		// }
		// for (int i = 2; i < vertices.Count; i += 3){
		// 	vertices_inter.Add(vertices[i]);
		// }
		
		mesh.vertices = vertices.ToArray();
		// mesh.vertices = vertices_inter.ToArray();
		mesh.colors = mColors.ToArray();
		mesh.triangles = triangle_indices.ToArray();
		mesh.SetIndices(mIndices.ToArray(), MeshTopology.Points, 0);
	}

	public float mapValue(float value, float minA, float maxA, float minB, float maxB){
		return (value - minA) * ((maxB - minB) / (maxA - minA)) + minB;
	}

}
