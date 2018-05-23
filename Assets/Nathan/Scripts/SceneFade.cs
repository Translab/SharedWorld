using UnityEngine;
using System.Collections;

public class SceneFade : MonoBehaviour
{


		public float fadeSpeed = 2.5f;          // Speed that the screen fades to and from black.
		private int floorIndexToLoad;
		private bool sceneStarting = true;      // Whether or not the scene is still fading in.
		private bool sceneEnding = false;
		//Color startColor;
		
		void Awake ()
		{
				// Set the texture so that it is the the size of the screen and covers it.
				GetComponent<GUITexture>().pixelInset = new Rect (0f, 0f, Screen.width, Screen.height);
			//	startColor = guiTexture.color;

		}
		
		void Update ()
		{
				// If the scene is starting...
				if (sceneStarting) {
						// ... call the StartScene function.
						StartScene ();
				} else if (sceneEnding) {

						EndScene ();
				}
		}
		
		void FadeToClear ()
		{
				//Debug.Log ("FadeToClear");
				// Lerp the colour of the texture between itself and transparent.
				GetComponent<GUITexture>().color = Color.Lerp (GetComponent<GUITexture>().color, Color.clear, fadeSpeed * Time.deltaTime);
		}
		
		void FadeToBlack ()
		{
				// Lerp the colour of the texture between itself and black.
				//Debug.Log (GetComponent<GUITexture>().color.a);
				GetComponent<GUITexture>().color = Color.Lerp (GetComponent<GUITexture>().color, Color.white, fadeSpeed * Time.deltaTime);
		}
		
		void StartScene ()
		{
				// Fade the texture to clear.
				FadeToClear ();
			
				// If the texture is almost clear...
				if (GetComponent<GUITexture>().color.a <= 0.05f) {
						// ... set the colour to clear and disable the GUITexture.
						GetComponent<GUITexture>().color = Color.clear;
						GetComponent<GUITexture>().enabled = false;
				
						// The scene is no longer starting.
						sceneStarting = false;
				}
		}

		public void setLevel (int level)
		{
				floorIndexToLoad = level;

		}
		
		public void EndScene ()
		{
				// Make sure the texture is enabled.
				GetComponent<GUITexture>().enabled = true;

				sceneEnding = true;
			
				// Start fading towards black.
				FadeToBlack ();
			
				// If the screen is almost black...
				if (GetComponent<GUITexture>().color.a >= 0.75f) {
						// ... reload the level.
						sceneEnding = false;
						Application.LoadLevel (floorIndexToLoad);
				}
		}
}
