using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PointCloudLoader : MonoBehaviour
{

    private Mesh mesh;
    int numPoints = 60000;

    // Use this for initialization
    void Start()
    {
        mesh = new Mesh();

        GetComponent<MeshFilter>().mesh = mesh;
        CreateMesh();
    }

    void CreateMesh()
    {
        Vector3[] points = new Vector3[numPoints];
        int[] indicies = new int[numPoints];
        Color[] colors = new Color[numPoints];
        for (int i = 0; i < points.Length; ++i)
        {
            points[i] = new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10));
            indicies[i] = i;
            colors[i] = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
            //mesh.normals[i] = new Vector3(0, 0, 1);
        }

        mesh.vertices = points;
        mesh.colors = colors;
        mesh.SetIndices(indicies, MeshTopology.Points, 0);

    }

    void Update()
    {

    }
}