using UnityEngine;
using System.Collections;




public class TriggerDoorOpen : MonoBehaviour {

	// Use this for initialization
	public string clip;

	public AudioClip openClip;
	public AudioClip closeClip;
	public float closeDelay;

	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == ("Player")){
			//gameObject.animation.Play();
			//go.animation.Play();
			Debug.Log("anim triggered");
			Component[] anims	= GetComponentsInChildren<Animation>();
			foreach(Animation anim in anims){
				anim[clip].normalizedTime = 0;
				anim[clip].speed = 1;
				anim.Play();
			}

			playAudioIfAvailable("open");
		}
	}

	void OnTriggerExit(Collider other){

		if (other.gameObject.tag == ("Player")) {
			Component[] anims	= GetComponentsInChildren<Animation>();
			foreach(Animation anim in anims){
				anim[clip].normalizedTime = 1;
				anim[clip].speed = -1;
				anim.Play();
				playAudioIfAvailable("closed");
			}
		}
	}


	void playAudioIfAvailable(string condition){
		if (gameObject.GetComponent<AudioSource>()) {

			if(condition == "open"){
			
			GetComponent<AudioSource>().clip = openClip;
			GetComponent<AudioSource>().Play();
			}else if(condition == "closed"){
				GetComponent<AudioSource>().clip = closeClip;
				GetComponent<AudioSource>().PlayDelayed(closeDelay);
			}

		}

	}
}
