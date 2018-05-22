using SKStudios.Common.Utils;
using UnityEngine;

namespace SKStudios.Common.Demos {
    public class HeadsetlessVRPReview : MonoBehaviour {
        private Camera cam;

        private bool eye;
        public Material mat;

        private void Start() {
            cam = GetComponent<Camera>();
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest) {
            var test = Camera.current;
            eye = !eye;
            if (eye)
                mat.SetTexture(Keywords.ShaderKeys.LeftEye, src);
            else
                mat.SetTexture(Keywords.ShaderKeys.RightEye, src);
            Graphics.Blit(src, dest, mat);
        }
    }
}