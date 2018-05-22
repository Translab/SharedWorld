using UnityEngine;

namespace SKStudios.Portals.Demos {
    public class LaserEmitter : MonoBehaviour {
        // Use this for initialization
        private void Start() { }

        // Update is called once per frame
        private void Update() {
            var ray = new Ray(transform.position, transform.forward);
            var hit = PortalUtils.TeleportableRaycast(ray, 100, ~0, QueryTriggerInteraction.Ignore);
            Debug.Log(hit);
        }
    }
}