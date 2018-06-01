using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Kvant{
	public class GrassControl : MonoBehaviour {

		public Grass grass;

		// Use this for initialization
		void Start () {
			grass = GetComponent<Grass> ();
		}
		
		// Update is called once per frame
		void Update () {
			//grass.baseScale += new Vector3(0.0f,0.1f, 0.0f);
		}
	}
}