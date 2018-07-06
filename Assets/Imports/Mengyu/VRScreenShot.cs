using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Unity.Collections;
using System.IO;
using System.Collections.Generic;

[RequireComponent(typeof(VRTK.VRTK_ControllerEvents))]
public class VRScreenShot : MonoBehaviour {
    string imgName;
    string imgTimeMark;
    public string namePrefix = "myCapture";
    bool captured = false;
    
    void Start(){
        //GetComponent<VRTK.VRTK_ControllerEvents>().GripAxisChanged += new VRTK.ControllerInteractionEventHandler(DoGripAxisChanged);
        GetComponent<VRTK.VRTK_ControllerEvents>().GripPressed += new VRTK.ControllerInteractionEventHandler(DoGripPressed);
    }
    
    public void takeScreenShot()
    {
        imgTimeMark = System.DateTime.Now.ToString("h:mm:ss tt");
        imgName = Application.dataPath + namePrefix + imgTimeMark + ".png";
        ScreenCapture.CaptureScreenshot(imgName);
    }

    // private void DoGripAxisChanged(object sender, VRTK.ControllerInteractionEventArgs e){
    //  if (e.buttonPressure > 0 && captured == false){
    //      takeScreenShot();
    //      Debug.Log("ScreenCaptured!");
    //      captured = true;
    //  } else if (e.buttonPressure == 0){
    //      captured = false;
    //  }
    // }
    private void DoGripPressed(object sender, VRTK.ControllerInteractionEventArgs e){
            takeScreenShot();
            Debug.Log("ScreenCaptured!");
    }
}