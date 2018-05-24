using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealworldTrace : MonoBehaviour {
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;
    public List<GameObject> lines;
    private bool shouldDraw;

    private LineRenderer currentLine;
    public Material linemat;
    public Transform brushtip;
    private Vector3 lastpos;
    private float sens = 0.01f;

    private List<Vector3> positions;
    // Use this for initialization
    void Start () {
        GameObject RController = GameObject.Find("Controller (right)");
        trackedObject = RController.GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObject.index);

        lines = new List<GameObject>();
        


    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger));
        

        if (device.GetTouchDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
        {
            GameObject l = new GameObject();
            currentLine =  l.AddComponent<LineRenderer>();
            currentLine.sharedMaterial = linemat;
            currentLine.startWidth = 0.03f;
            currentLine.endWidth = 0.03f;
            positions = new List<Vector3>();


        }

        //if we should draw
        if (device.GetTouch(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)){
            if (currentLine)
            {
                if ((brushtip.position - lastpos).magnitude > sens)
                {
                    positions.Add(brushtip.position);

                    currentLine.positionCount = positions.Count;
                    currentLine.SetPositions(positions.ToArray());
                    lastpos = brushtip.position;
                }
            }

        }

        if (device.GetTouchUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
        {
            lines.Add(currentLine.gameObject);

            positions.Clear();
        }


        
    }
}
