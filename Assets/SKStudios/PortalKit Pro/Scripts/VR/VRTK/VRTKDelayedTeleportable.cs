using System.Collections;
using System.Collections.Generic;
using SKStudios.Portals;
using UnityEngine;
#if SKS_VR
using VRTK;

namespace SKStudios.Portals
{
    public class VRTKDelayedTeleportable : MonoBehaviour
    {
        private VRTK_BodyPhysics bodyPhysics;

        // Use this for initialization
        void Start()
        {
            if(gameObject.activeInHierarchy)
                StartCoroutine(DelayedSetup());
        }

        IEnumerator DelayedSetup() {
           
            GameObject headObj = null;
            while (headObj == null) {
                Transform bodytransform = VRTK_DeviceFinder.HeadsetCamera();
                if (bodytransform)
                    headObj = bodytransform.gameObject;
                yield return new WaitForEndOfFrame();
            }

            AutomaticCameraLayerSet layerSet = headObj.AddComponent<AutomaticCameraLayerSet>();
            layerSet.ExcludedLayers = new List<string>();
            layerSet.ExcludedLayers.Add("PortalOnly");
            layerSet.ExcludedLayers.Add("PortalPlaceholder");
            layerSet.ExcludedLayers.Add("RenderExclude");


            GameObject bodyObj = null;
            while (bodyObj == null)
            {
                Transform bodytransform = VRTK_DeviceFinder.PlayAreaTransform();
                if (bodytransform)
                    bodyObj = bodytransform.gameObject;
                yield return new WaitForEndOfFrame();
            }


            bodyObj.transform.position = transform.position;

            yield return new WaitForSeconds(2f);

            Teleportable teleportable = headObj.AddComponent<Teleportable>();
            teleportable.enabled = false;
            teleportable.Root = transform;
            teleportable.IsActive = true;
            teleportable.VisOnly = false;
            teleportable.enabled = true;
            //teleportable.SpecialRoot = true;

            
            Collider col;
            if (!(col = headObj.GetComponent<Collider>()))
            {
                col = headObj.AddComponent<BoxCollider>();
                ((BoxCollider)col).size = new Vector3(0.25f, 0.25f, 0.25f);
                col.isTrigger = true;
            }

            Rigidbody headRB = null;
            if (!(headRB = headObj.GetComponent<Rigidbody>()))
                headRB = headObj.AddComponent<Rigidbody>();
            headRB.isKinematic = true;


            PlayerTeleportable playerTeleportable = headObj.AddComponent<PlayerTeleportable>();

            SelfRightingPlayer selfRight = bodyObj.AddComponent<SelfRightingPlayer>();
            selfRight.moveSpeed = 4;

        }
    }
}

#endif