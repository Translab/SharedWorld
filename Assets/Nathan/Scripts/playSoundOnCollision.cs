using UnityEngine;
using System.Collections;

public class playSoundOnCollision : MonoBehaviour {
	
	public AudioClip[] clips;
	

	void OnCollisionEnter(Collision collision){


						if (!GetComponent<AudioSource>().isPlaying) {

								float vol = collision.relativeVelocity.magnitude;
								int clipNum = Random.Range (0, clips.Length - 1);
								GetComponent<AudioSource>().clip = clips [clipNum];

								if (vol > 1) {
										vol = 1;		
								}

								GetComponent<AudioSource>().volume = vol;
								GetComponent<AudioSource>().Play ();
						}

	}
}
