using UnityEngine;
using System.Collections;

public class MicrophoneInput : MonoBehaviour {
	AudioSource aud;
    void Start() {
        foreach (string device in Microphone.devices) {
            Debug.Log("Name: " + device);
        }

		aud = GetComponent<AudioSource>();
        aud.clip = Microphone.Start("Headset (HX 831 Hands-Free)", false, 1, 44100);
		while (!(Microphone.GetPosition("Headset (HX 831 Hands-Free)") > 0)) { }
    	Debug.Log("start playing... position is " + Microphone.GetPosition("Headset (HX 831 Hands-Free)"));
        //aud.loop = true; 
		if (Microphone.IsRecording ("Headset (HX 831 Hands-Free)")) { //check that the mic is recording, otherwise you'll get stuck in an infinite loop waiting for it to start
			while (!(Microphone.GetPosition ("Headset (HX 831 Hands-Free)") > 30)) {
			} // Wait until the recording has started. 

			// Start playing the audio source
			aud.Play (); 
		} else {
			//microphone doesn't work for some reason

			Debug.Log ("Headset (HX 831 Hands-Free)" + " doesn't work!");
		}
		// aud.Play();
    }
	void Update(){
		if (!Microphone.IsRecording ("Headset (HX 831 Hands-Free)")){
			aud.Stop();
			Debug.Log("start another recording");
			aud.clip = Microphone.Start("Headset (HX 831 Hands-Free)", false, 1, 44100);
			aud.Play();
			
		}
		// if (Microphone.IsRecording("Headset (HX 831 Hands-Free)")){
		// 	Debug.Log("show me evidence");

		 //aud.clip = Microphone.Start("Headset (HX 831 Hands-Free)", true, 1, 44100);
			
	}
	void onDestroy(){
		// aud.Stop();
	}
}