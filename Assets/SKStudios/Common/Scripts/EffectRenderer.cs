using System;
using System.Collections;
using SKStudios.Common.Utils;
using SKStudios.Rendering;
using SK_Common.Extensions;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SKStudios.Common {
    /// <summary>
    ///     Component used on an object to trigger <see cref="SKEffectCamera"/> rendering.
    /// </summary>
    public abstract class EffectRenderer : MonoBehaviour {

        /// <summary>
        ///     Event raised when this Effect is disabled.
        /// </summary>
        public event Action OnDisabled;

        protected static Camera _headCamera;

        private Transform _arrivalTarget;

        protected int CheeseActivated = -1;

        /// <summary>
        ///     Calls the rendering of a Effect frame
        /// </summary>
        private SKEffectCamera _effectCamera;

        private MeshFilter _meshFilter;

        private MeshRenderer _meshRenderer;
        protected Vector3[] _nearClipVertsGlobal;

        protected Vector3[] _nearClipVertsLocal;

        private Transform _origin;

        private Material _renderMaterial;

        protected MaterialPropertyBlock _seamlessRecursionBlock;

        protected Renderer _seamlessRecursionRenderer;
        protected int RenderCount;
        public bool Rendering = true;

        protected static Camera HeadCamera {
            get {
                if (_headCamera == null) _headCamera = Camera.main;
                return _headCamera;
            }
        }

        /// <summary>
        ///     The collider of the Player's head
        /// </summary>
        public Collider HeadCollider { get; set; }

        /// <summary>
        ///     Should cameras use Oblique culling? (Default True)
        /// </summary>
        public bool NonObliqueOverride { get; set; }

        /// <summary>
        ///     Is the Renderer 3d, such as a crystal ball or similar effect?
        /// </summary>
        public bool Is3D { get; set; }

        /// <summary>
        ///     The transform of an object used to fix recursion issues
        /// </summary>
        public Transform SeamlessRecursionFix { get; set; }

        protected Renderer SeamlessRecursionRenderer {
            get {
                if (!_seamlessRecursionRenderer)
                    _seamlessRecursionRenderer = SeamlessRecursionFix.GetComponent<Renderer>();
                return _seamlessRecursionRenderer;
            }
        }

        /// <summary>
        ///     The Mesh Renderer used by the Renderer. Used for determining Renderer size on screen.
        /// </summary>
        public MeshRenderer MeshRenderer {
            get {
                if (!_meshRenderer) _meshRenderer = GetComponent<MeshRenderer>();
                return _meshRenderer;
            }
            set { _meshRenderer = value; }
        }

        /// <summary>
        ///     The Mesh Filter used by the Renderer. Also used for determining Renderer size on screen.
        /// </summary>
        public MeshFilter MeshFilter {
            get {
                if (!_meshFilter)
                    _meshFilter = gameObject.GetComponent<MeshFilter>();
                return _meshFilter;
            }
            set { _meshFilter = value; }
        }

        /// <summary>
        ///     The Material for the Renderer to use
        /// </summary>
        public Material RenderMaterial {
            get {
                return _renderMaterial;
            }
            set {
                _renderMaterial = value;
                MeshRenderer.material = _renderMaterial;
                if (SeamlessRecursionFix)
                    SeamlessRecursionFix.GetComponent<Renderer>().sharedMaterial = _renderMaterial;

                if (!_effectCamera)
                    InitCamera();
                EffectCamera.UpdateMaterial(_renderMaterial);
            }
        }

        /// <summary>
        ///     The target Renderer Trigger
        /// </summary>
        public virtual EffectRenderer Target { get; set; }

        ///The <see cref="SKEffectCamera"/> that this Renderer uses.
        public SKEffectCamera EffectCamera {
            get {
                if (!_effectCamera) {
                    if (!RenderMaterial)
                        return null;
                    InitCamera();
                }

                return _effectCamera;
            }
            set { _effectCamera = value; }
        }

        /// <summary>
        ///     Origin of the Renderer, facing inward
        /// </summary>
        public Transform Origin {
            get {
                if (!_origin)
                    _origin = Root.Find("Source");
                return _origin;
            }
        }

        /// <summary>
        ///     The transform attached to the target Renderer, for ref purposes. facing outward. Verbose for clarity reasons.
        /// </summary>
        public Transform ArrivalTarget {
            get {
                if (!_arrivalTarget) {
                    if (Target == null) return null;
                    _arrivalTarget = Target.Root.Find("Target");
                }
                    
                return _arrivalTarget;
            }
        }

        public Transform Root {
            get { return transform.parent.parent; }
        }

        /// <summary>
        ///     Is this renderer rendering as a mirror?
        /// </summary>
        public virtual bool IsMirror {
            get { return false; }
        }

        /// <summary>
        ///     Is this renderer rendering as a low resolution effect?
        /// </summary>
        public virtual bool IsLowqEffect {
            get { return false; }
        }

        private void InitCamera() {
            _effectCamera = transform.parent.parent.GetComponentInChildren<SKEffectCamera>(true);
            _effectCamera.Initialize(Target.EffectCamera, RenderMaterial, MeshRenderer, Target.MeshRenderer,
                MeshFilter.sharedMesh, Origin, ArrivalTarget);
        }


        protected void SetupCamera() {
            StartCoroutine(SetupCameraRoutine());
        }

        protected IEnumerator SetupCameraRoutine() {
            while (HeadCamera == null) yield return WaitCache.Frame;
            HeadCollider = HeadCamera.GetComponent<Collider>();
            _nearClipVertsLocal = HeadCamera.EyeNearPlaneDimensions();
            _nearClipVertsGlobal = new Vector3[_nearClipVertsLocal.Length];
        }

        protected void OnEnable() {
            SetupCamera();
            //Set up things to keep the seamless recursion fix updated
            SeamlessRecursionRenderer.sharedMaterial = RenderMaterial;
            _seamlessRecursionBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        ///     All Renderer updates are done after everything else has updated
        /// </summary>
        protected void LateUpdate() {
            SKEffectCamera.CurrentDepth = 0;
            RenderCount = 0;
        }

        protected virtual void FrameUpdate() { }

        private void OnWillRenderObject() {
            if (!Target) return;
            //Is the mesh renderer in the camera frustrum?
            if (MeshRenderer.isVisible) {
                RenderObjectBehavior();

                Target.FrameUpdate();

#if UNITY_EDITOR
                if (SceneView.lastActiveSceneView != null
                    && Camera.current == SceneView.lastActiveSceneView.camera)
                    return;
#endif
                TryRender(Camera.current, _nearClipVertsGlobal);
                if (Camera.current.gameObject.CompareTag("MainCamera")) {
                    SKSGlobalRenderSettings.Inverted = SKSGlobalRenderSettings.Inverted;
                    SKSGlobalRenderSettings.UvFlip = SKSGlobalRenderSettings.UvFlip;
                }

                //}
            }

            UpdateEffects();
        }

        protected abstract void RenderObjectBehavior();

        /// <summary>
        ///     Render a Renderer frame, assuming that the camera is in front of the Renderer and all conditions are met.
        /// </summary>
        /// <param name="camera">The camera rendering the Renderer</param>
        /// <param name="nearClipVerts">The vertices of the camera's near clip plane</param>
        private void TryRender(Camera camera, Vector3[] nearClipVerts) {
            if (!Target || !Rendering)
                return;
            //bool isVisible = false;
            var isVisible = false;
            //Check if the camera itself is behind the Renderer, even if the frustum isn't.

            if (!IsMirror) {
                if (!SKSGeneralUtils.IsBehind(camera.gameObject.transform.position, Origin.position, Origin.forward))
                    isVisible = true;
                else
                    foreach (var v in nearClipVerts)
                        if (!SKSGeneralUtils.IsBehind(v, Origin.position, Origin.forward)) {
                            isVisible = true;
                            break;
                        }
            }
            else {
                if (SKSGeneralUtils.IsBehind(camera.gameObject.transform.position, Origin.position, Origin.forward))
                    isVisible = true;
                else
                    foreach (var v in nearClipVerts)
                        if (SKSGeneralUtils.IsBehind(v, Origin.position, Origin.forward)) {
                            isVisible = true;
                            break;
                        }
            }


            if (isVisible || CheeseActivated != -1)
                EffectCamera.RenderIntoMaterial(
                    camera, RenderMaterial,
                    MeshRenderer, Target.MeshRenderer,
                    MeshFilter.mesh, !NonObliqueOverride ? CheeseActivated == -1 : false,
                    Is3D, IsMirror, IsLowqEffect);

            MeshRenderer.GetPropertyBlock(_seamlessRecursionBlock);
            SeamlessRecursionRenderer.SetPropertyBlock(_seamlessRecursionBlock);
        }

        /// <summary>
        ///     Sets the z rendering order to make through-wall rendering seamless, as well as disabling masks while traversing
        ///     Renderer
        /// </summary>
        protected void UpdateEffects() {
            //MeshRenderer.GetPropertyBlock(_seamlessRecursionBlock);
            if (CheeseActivated == SKEffectCamera.CurrentDepth) {
                //_seamlessRecursionBlock.SetFloat("_ZTest", (int)CompareFunction.Always);
                //MeshRenderer.SetPropertyBlock(_seamlessRecursionBlock);
                MeshRenderer.sharedMaterial.SetFloat(Keywords.ShaderKeys.ZTest, (int) CompareFunction.Always);
                MeshRenderer.sharedMaterial.SetFloat(Keywords.ShaderKeys.Mask, 0);
            }
            else {
                //_meshRenderer.material.SetFloat("_ZTest", (int)CompareFunction.Less);
                MeshRenderer.sharedMaterial.SetFloat(Keywords.ShaderKeys.Mask, 1);
            }
        }

        private void Start() {
#if SKS_DEV
            name = transform.parent.parent.parent.gameObject.name;
#endif
        }

        private void OnDrawGizmos() {
#if SKS_DEV
            var style = new GUIStyle(new GUIStyle {alignment = TextAnchor.MiddleCenter});
            style.normal.textColor = Color.white;
#if UNITY_EDITOR
            Handles.Label(transform.position, name, style);
#endif
#endif
        }
    }
}