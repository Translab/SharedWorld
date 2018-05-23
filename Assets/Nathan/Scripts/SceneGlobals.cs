using UnityEngine;
using System.Collections;

public class SceneGlobals : MonoBehaviour
{


		void Awake ()
		{
				DontDestroyOnLoad (this);
		}

		public static Quaternion playerTransform;
	public static string locationEmpty;
	
		public static void setLoadingLocation (string startEmpty)
		{
				locationEmpty = startEmpty;
		}

		public static string getLodingLocation ()
		{
				return locationEmpty;
		}

		public static void updatePlayerTransform ()
		{
				Debug.Log ("updatePlayerTransform was called");
				GameObject player = GameObject.FindGameObjectWithTag ("Player");
				playerTransform = player.gameObject.transform.rotation;
		}
	
		public static Quaternion getPlayerTransform ()
		{
				Debug.Log ("getPlayerTransform was called " + playerTransform);
				return playerTransform;
		}

}
