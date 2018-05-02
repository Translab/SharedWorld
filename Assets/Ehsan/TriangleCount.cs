using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TriangleCount : MonoBehaviour {
    public int overallLimit = 100000;
	// Use this for initialization
	void Start () {
        List<Scene> scenes = new List<Scene>();
        List<int> triscount = new List<int>();
        List<MeshFilter> filters = new List<MeshFilter>();
        for(int i = 0; i < SceneManager.sceneCount;i++)
        {
            scenes.Add(SceneManager.GetSceneAt(i));
            triscount.Add(0);
            var objs = scenes[i].GetRootGameObjects();
            
            foreach (GameObject g  in objs)
            {
                var filts = g.GetComponentsInChildren<MeshFilter>();
                foreach(MeshFilter f in filts)
                {
                    filters.Add(f);
                    triscount[i]+= (f.mesh.triangles.Length / 3);
                    
                }  
            }

            int budget = overallLimit / SceneManager.sceneCount;
            if (triscount[i] <= budget)
            {
                Debug.Log(scenes[i].name + " is using " + triscount[i] + " triangles out of " + budget);
            }else
            {
                Debug.LogWarning(scenes[i].name + " is using " + triscount[i] + " triangles out of " + budget +"\n" + "Consider using lower polygon count on your objects." );
            }

        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
