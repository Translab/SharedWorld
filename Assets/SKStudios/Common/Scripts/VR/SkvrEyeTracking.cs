using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

namespace SKStudios.Common.Utils.VR {
    /// <summary>
    ///     Class that exposes eye offsets for custom stereoscopic rendering
    /// </summary>
    public class SkvrEyeTracking {

        private static readonly Dictionary<Camera, SkvrEyeTrackingComponent> CameraDict;

        static SkvrEyeTracking() {
            CameraDict = new Dictionary<Camera, SkvrEyeTrackingComponent>();
        }

        /// <summary>
        ///     Gets the eye offset of a given camera, scaled and rotated properly, relative to the head.
        ///     Outputs the value for the left eye. For right eye offset, multiply by negative 1.
        /// </summary>
        /// <param name="cam">Camera to get the eye offsets of</param>
        /// <returns></returns>
        public static Vector3 EyeOffset(Camera cam) {
            SkvrEyeTrackingComponent skEyeTracking;
            if (CameraDict.ContainsKey(cam)) {
                skEyeTracking = CameraDict[cam];
            }
            else {
                skEyeTracking = cam.gameObject.AddComponent<SkvrEyeTrackingComponent>();
                CameraDict.Add(cam, skEyeTracking);
            }

            return skEyeTracking.EyeOffset;
        }
    }

    public class SkvrEyeTrackingComponent : MonoBehaviour {
        private static Camera _camera;
        private static Camera CamComp {
            get {
                if (!_camera)
                    _camera = Camera.main;
                return _camera;
            }
        }

        public static Vector3 TempOffset = Vector3.zero;
        private bool _computedThisFrame;

        private Camera _mCam;

        private readonly Vector3 _mEyeOFfset;
        public SkvrEyeTrackingComponent(Vector3 mEyeOFfset) {
            _mEyeOFfset = mEyeOFfset;
        }

        private Camera Cam {
            get {
                if (!_mCam)
                    _mCam = GetComponent<Camera>();
                return _mCam;
            }
        }

        public Vector3 EyeOffset {
            get {
                if (_computedThisFrame) return _mEyeOFfset;
                var left = Quaternion.Inverse(UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightEye)) *
                           UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftEye);
                //Eyes not even being tracked through this interface, time for plan B
                if (left != Vector3.zero) {
                    var right = Quaternion.Inverse(UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftEye)) *
                                UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightEye);
                    var offset = (left - right) / 2f;
                    var m = Cam.cameraToWorldMatrix;

                    var leftWorld = m.MultiplyPoint(-offset);
                    var rightWorld = m.MultiplyPoint(offset);
                    return Quaternion.Inverse(transform.rotation) * ((leftWorld - rightWorld) / 2f);
                }
                else {
                    Vector3 offset = new Vector3(-CamComp.stereoSeparation, 0, 0);
                    if (CamComp.transform.parent != null)
                        offset = Vector3.Scale(offset, CamComp.transform.parent.lossyScale);
                    return offset * 2f;
                }
            }
        }

        public void Update() {
            _computedThisFrame = false;
        }
    }
}