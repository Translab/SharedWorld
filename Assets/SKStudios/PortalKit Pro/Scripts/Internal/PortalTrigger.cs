using UnityEngine;

namespace SKStudios.Portals {
    /// <summary>
    ///     Component that relays physics messages back to the associated <see cref="Portal"/>
    /// </summary>
    public class PortalTrigger : MonoBehaviour {
        //Trigger segregated from Portal to prevent scaling interaction
        public Portal portal;

        private void Awake() {
            enabled = false;
            gameObject.GetComponent<Collider>().enabled = false;
        }

        public void OnTriggerEnter(Collider col) {
            if (col.gameObject.isStatic) return;
            if (!col || !portal)
                return;
            portal.E_OnTriggerEnter(col);
        }

        public void OnTriggerStay(Collider col) {
            if (col.gameObject.isStatic) return;
            if (!col || !portal)
                return;
            portal.E_OnTriggerStay(col);
        }

        public void OnTriggerExit(Collider col) {
            if (col.gameObject.isStatic) return;
            if (!col || !portal)
                return;
            portal.E_OnTriggerExit(col);
        }

        public void OnEnable() {
            var breakpoint = true;
        }

        public void OnDisable() {
            var breakpoint = true;
        }
    }
}