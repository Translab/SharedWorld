#if SKS_VR
using SKStudios.Portals;
using System.Collections;
using SKStudios.Portals;
using UnityEngine;
using VRTK;

[RequireComponent(typeof(VRTK_ControllerEvents))]
public class VRTKHandPassthrough : TeleportableScript {
    VRTK_InteractGrab grab;
    // Use this for initialization
    void Start() {
        grab = gameObject.GetComponent<VRTK_InteractGrab>();
    }

    // Update is called once per frame
    public override void CustomUpdate() {
        base.CustomUpdate();
    }

    public override void OnTeleport() {
        
        if (!this.isActiveAndEnabled)
            return;

        Teleportable grabTele;

        if (grab.GetGrabbedObject() && 
            (grabTele = grab.GetGrabbedObject().GetComponent<Teleportable>())) {
            grabTele.UpdateDoppleganger();

            if(grab && grab.GetGrabbedObject() && teleportable.Doppleganger && currentPortal)
            PortalUtils.TeleportObject(grab.GetGrabbedObject(), currentPortal.Origin, currentPortal.ArrivalTarget, teleportable.Doppleganger.transform, null, false);
        }
        if (grab) {
            grab.ForceRelease();
            StartCoroutine(OnPostTeleport());
            //grab.AttemptGrab();
        }

       
       
    }

    private IEnumerator OnPostTeleport() {
        
        yield return new WaitForFixedUpdate();
        if (grab) {
            grab.AttemptGrab();
        }
    }
   
}
#endif