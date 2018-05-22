using System.Collections.Generic;
using UnityEngine;

namespace SKStudios.Portals {
    [RequireComponent(typeof(Camera))]
    public class AutomaticCameraLayerSet : MonoBehaviour {
        private new Camera camera;
        public List<string> ExcludedLayers;

        // Use this for initialization
        private void Start() {
            camera = gameObject.GetComponent<Camera>();
            var mask = camera.cullingMask;
            foreach (var l in ExcludedLayers) mask &= ~(1 << LayerMask.NameToLayer(l));
            camera.cullingMask = mask;
        }
    }
}