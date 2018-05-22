using System;
using System.Collections;
using System.Collections.Generic;
using SKStudios.Common.Rendering;
using SKStudios.Common.Utils;
using SKStudios.ProtectedLibs.Rendering;
using SKStudios.Rendering;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SKStudios.Portals {
    /// <summary>
    ///     Class designed to spawn and control <see cref="Portal"/> instances.
    /// </summary>
    [ExecuteInEditMode]
    public class PortalController : MonoBehaviour {
        [SerializeField] private float _detectionScale;

        [SerializeField] private bool _enterable = true;

        [SerializeField] private bool _is3D;

        [SerializeField] private Texture2D _mask;

        [SerializeField] private SKEffectCamera _portalCameraScript;

        [SerializeField] private Material _portalMaterial;

        private Coroutine _portalOpeningAsyncOpRoutine;


        private Vector3 _portalOpeningSize = Vector3.one;

        private GameObject _portalRenderer;


        //The scale of this Portal, for resizing
        private float _portalScale = 1;

        private Coroutine _portalScaleAsyncOpRoutine;


        [SerializeField] private Portal _portalScript;

        private PortalTrigger _portalTrigger;

        private Camera _previewCamera;
        private GameObject _previewRoot;


        [SerializeField] private bool _rendering = true;


        private bool _setup;

        [SerializeField] private PortalController _targetController;

        /// <summary>
        ///     What is the color of the Portal in the editor?
        /// </summary>
        public Color color = Color.white;


        [HideInInspector] public bool NonObliqueOverride = false;
        [NonSerialized] public Material OriginalMaterial;

        /// <summary>
        ///     The Portal prefab to use
        /// </summary>
        public GameObject PortalPrefab;

        [NonSerialized] public Material VisLineMat;

        /// <summary>
        ///     The root for preview camera targeting
        /// </summary>
        public GameObject PreviewRoot {
            get {
                if (!gameObject.activeInHierarchy) {
                    DestroyImmediate(_previewRoot, true);
                    return null;
                }


                if (!_previewRoot) {
#if UNITY_EDITOR
                    _previewRoot = EditorUtility.CreateGameObjectWithHideFlags("Preview Root", HideFlags.HideAndDontSave | HideFlags.NotEditable);
#endif
#if !SKS_DEV
                    _previewRoot.hideFlags = HideFlags.HideAndDontSave;
#endif
                    _previewRoot.transform.localPosition = transform.position;
                    _previewRoot.transform.localRotation = Quaternion.AngleAxis(180, transform.up) * transform.rotation;
                    _previewRoot.transform.localScale = transform.lossyScale;
                    _previewRoot.transform.SetParent(transform, true);

                    _previewRoot.AddComponent<MeshRenderer>();
                    var filter = _previewRoot.AddComponent<MeshFilter>();
                    filter.mesh = Resources.Load<Mesh>("Meshes/PortalPreview");
                }

                _previewRoot.tag = Keywords.Tags.SKEditorTemp;
                return _previewRoot;
            }
        }

        /// <summary>
        ///     The Camera for Preview Rendering
        /// </summary>
        public Camera PreviewCamera {
            get {
                if (!gameObject.activeInHierarchy)
                {
                    DestroyImmediate(_previewCamera, true);
                    return null;
                }

                if (!_previewCamera)
                {
#if UNITY_EDITOR
                    var previewCamera = EditorUtility.CreateGameObjectWithHideFlags("Preview Camera", HideFlags.HideAndDontSave | HideFlags.NotEditable);
#if !SKS_DEV
                    previewCamera.hideFlags = HideFlags.HideAndDontSave;
#endif
                    previewCamera.transform.SetParent(transform);
                    previewCamera.transform.localPosition = Vector3.zero;
                    previewCamera.transform.localScale = Vector3.one;

                    var cam = previewCamera.AddComponent<Camera>();

                    cam.cullingMask |= 1 << Keywords.Layers.CustomRenderer;
                    cam.enabled = false;


                    _previewCamera = previewCamera.GetComponent<Camera>();
                    _previewCamera.useOcclusionCulling = false;

                    var lib = previewCamera.AddComponent<SKSRenderLib>();

                    var blitMat = new Material(Shader.Find("Custom/BlitWithInversion"));
                    lib.Initialize(PreviewRoot.transform, TargetController.transform);
#endif
                }

                _previewCamera.tag = Keywords.Tags.SKEditorTemp;
                return _previewCamera;
            }
        }
        /// <summary>
        ///     The Target PortalController
        /// </summary>
        public PortalController TargetController {
            get { return _targetController; }
            set {
                _targetController = value;
                if (_targetController && _targetController.TargetController == null) {
#if UNITY_EDITOR
                    Undo.RecordObject(_targetController, "Automatic Portal Linking");
#endif
                    _targetController.TargetController = this;
                }

                if (PortalScript)
                    _portalScript.Target = TargetController.GetComponentInChildren<Portal>(true);

                var breakpoint = false;
            }
        }

        public float PortalScale {
            get { return _portalScale; }
            set {
                if (!_setup && _portalScaleAsyncOpRoutine == null) {
                    _portalScaleAsyncOpRoutine = StartCoroutine(PortalScaleAsyncOp(value));
                    return;
                }

                _portalScale = value;
                _portalScript.Origin.localScale = Vector3.one * _portalScale;
                _portalScript.Target.ArrivalTarget.localScale = Vector3.one * _portalScale;
            }
        }

        //The actual unit size of the Portal opening
        [HideInInspector]
        public Vector3 PortalOpeningSize {
            get { return _portalOpeningSize; }
            set {
                if (_portalOpeningSize == value) return;
                if (!_setup && _portalOpeningAsyncOpRoutine == null) {
                    _portalOpeningAsyncOpRoutine = StartCoroutine(PortalOpeningAsyncOp(value));
                    return;
                }

                _portalOpeningSize = value;
                PortalRenderer.transform.localScale = _portalOpeningSize;
            }
        }

        /// <summary>
        ///     Does the portal have rendering enabled?
        /// </summary>
        public bool Rendering {
            get { return _rendering; }
            set {
                _rendering = value;
                if (PortalScript)
                    PortalScript.Rendering = value;
            }
        }

        /// <summary>
        ///     The Mask for the spawned portal to use. Masks control alpha for stencil and fade effects
        /// </summary>
        public Texture2D Mask {
            get {
                if (SKSGlobalRenderSettings.ShouldOverrideMask)
                    return SKSGlobalRenderSettings.Mask;

                return _mask;
            }
            set {
                if (PortalMaterial)
                    PortalMaterial.SetTexture(Keywords.ShaderKeys.AlphaTexture, value);

                if (SKSGlobalRenderSettings.ShouldOverrideMask && SKSGlobalRenderSettings.Mask &&
                    SKSGlobalRenderSettings.Mask == value)
                    return;

                if (PortalScript)
                    PortalScript.Mask = value;

                _mask = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(_mask);
#endif
            }
        }

        /// <summary>
        ///     The Material for the Portal to use
        /// </summary>
        public Material PortalMaterial {
            get {
                return _portalMaterial;
            }
            set {
                _portalMaterial = value;
                //if (PortalScript && (!Application.isPlaying || (PortalScript.PortalCamera.Initialized && PortalScript.TargetPortal.PortalCamera.Initialized)))
                if (PortalScript)
                    PortalScript.RenderMaterial = PortalMaterial;
                if (Application.isPlaying) {
                    if (_previewRoot)
                        PreviewRoot.GetComponent<MeshRenderer>().sharedMaterial = PortalMaterial;
                }
                else {
                    if (PreviewRoot)
                        PreviewRoot.GetComponent<MeshRenderer>().sharedMaterial = PortalMaterial;
                }
            }
        }

        /// <summary>
        ///     Is the Portal enterable?
        /// </summary>
        public bool Enterable {
            get { return _enterable; }
            set {
                _enterable = value;
                if (PortalScript)
                    PortalScript.Enterable = _enterable;
            }
        }

        /// <summary>
        ///     Is the Portal 3d, such as a crystal ball or similar?
        /// </summary>
        public bool Is3D {
            get { return _is3D; }
            set {
                _is3D = value;
                if (PortalScript)
                    PortalScript.Is3D = _is3D;
            }
        }

        /// <summary>
        ///     The scale of the detection zone for this portal
        /// </summary>
        public float DetectionScale {
            get { return _detectionScale; }
            set {
                _detectionScale = value;
                if (PortalTrigger) PortalTrigger.transform.localScale = new Vector3(1, 1, _detectionScale);
            }
        }

        public Portal PortalScript {
            get {
                if (!_portalScript)
                    _portalScript = GetComponentInChildren<Portal>(true);
                return _portalScript;
            }
        }

        private SKEffectCamera PortalCameraScript {
            get {
                if (!_portalCameraScript) _portalCameraScript = GetComponentInChildren<SKEffectCamera>(true);
                return _portalCameraScript;
            }
        }

        private PortalTrigger PortalTrigger {
            get {
                if (!_portalTrigger)
                    _portalTrigger = GetComponentInChildren<PortalTrigger>(true);
                return _portalTrigger;
            }
        }

        private GameObject PortalRenderer {
            get {
                if (_portalRenderer == null)
                    _portalRenderer = PortalScript.transform.parent.gameObject;
                return _portalRenderer;
            }
        }

        private IEnumerator PortalScaleAsyncOp(float value) {
            while (!_setup) yield return WaitCache.Frame;
            PortalScale = value;
        }

        private IEnumerator PortalOpeningAsyncOp(Vector3 value) {
            while (!_setup) yield return WaitCache.Frame;
            PortalOpeningSize = value;
        }


        private void Start() {
            CleanupTemp();
            if (TargetController)
                TargetController.CleanupTemp();
            GetComponent<Renderer>().enabled = true;
            if (!Application.isPlaying) return;
            SKSGlobalRenderSettings.Instance.OnEnable();
            StartCoroutine(Setup());
        }

        private IEnumerator Setup() {
            //Move all children to portal for accurate scaling
            var childTransforms = new List<Transform>();
            foreach (Transform t in transform)
                childTransforms.Add(t);


            PortalPrefab = Instantiate(PortalPrefab, transform);


            PortalPrefab.transform.localPosition = Vector3.zero;
            PortalPrefab.transform.localRotation = Quaternion.Euler(0, 180, 0);
            PortalPrefab.transform.localScale = Vector3.one;
            PortalPrefab.name = "Portal";

            Destroy(gameObject.GetComponent<MeshRenderer>());

            while (!PortalScript || !TargetController.PortalScript) yield return WaitCache.Frame;

            TargetController = TargetController;

            PortalMaterial = PortalMaterial;
            Mask = Mask;
            Enterable = Enterable;
            Is3D = Is3D;
            Rendering = Rendering;

            foreach (var t in childTransforms)
                t.SetParent(PortalScript.Origin);

            yield return WaitCache.Frame;
            PortalPrefab.SetActive(true);

            PortalCameraScript.RenderingCameraParent = PortalCameraScript.transform.parent;

            PortalScript.Is3D = Is3D;
            PortalScript.SeamlessRecursionFix = transform.Find("Portal/PortalRenderer/SeamlessRecursionFix");


            PortalScript.Mask = Mask;

            PortalScript.NonObliqueOverride = NonObliqueOverride;
            PortalScript.PhysicsPassthrough = GetComponentInChildren<PhysicsPassthrough>();
            DetectionScale = DetectionScale;

            PortalTrigger.portal = PortalScript;

            //Enable scripts
            PortalScript.enabled = true;
            PortalCameraScript.enabled = true;
            PortalTrigger.enabled = true;
            _setup = true;
            //Transfer transform values to modifiable var
            PortalOpeningSize = transform.localScale;
            transform.localScale = Vector3.one;

            PortalMaterial = new Material(PortalMaterial);
            PortalScript.RenderMaterial = PortalMaterial;
#if !SKS_DEV
            PortalPrefab.ApplyHideFlagsRecursive(HideFlags.HideAndDontSave);
#endif
            UpdateScale();
            if (gameObject.isStatic) {
                PortalScript.gameObject.isStatic = true;
                PortalTrigger.gameObject.isStatic = true;
            }
        }


        private void Update() {
            if (!TargetController) return;
            if (!Application.isPlaying) return;
            Debug.DrawLine(transform.position, TargetController.transform.position, color);

            if (_setup && !gameObject.isStatic)
                UpdateScale();
        }

        private void UpdateScale() {
            PortalScale = PortalRenderer.transform.lossyScale.x;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        ///     Draw the Portal Controller Gizmos.
        /// </summary>
        public void OnDrawGizmos() {
#if UNITY_EDITOR
            if (!SKSGlobalRenderSettings.Gizmos)
                return;
            //Change Portal colors
            Renderer renderer;

            if (renderer = gameObject.GetComponent<Renderer>()) {
                var materials = renderer.sharedMaterials;
                if (!OriginalMaterial)
                    OriginalMaterial =
                        new Material(Resources.Load<Material>("Materials/Visible effects/PortalControllerMat"));
                var material = OriginalMaterial; //new Material(OriginalMaterial);
                material.SetColor("_Color", color);
                renderer.sharedMaterial = material;
            }


            var style = new GUIStyle();
            style.normal.textColor = Color.red;


            if (!TargetController) {
                Handles.Label(transform.position, "No Target Set", style);
                return;
            }

            if (!PortalMaterial) {
                Handles.Label(transform.position, "No Portal Material Set", style);
                return;
            }

            if (!Mask && !SKSGlobalRenderSettings.ShouldOverrideMask ||
                SKSGlobalRenderSettings.ShouldOverrideMask && !SKSGlobalRenderSettings.Mask) {
                Handles.Label(transform.position, "No Mask Set", style);
                return;
            }

            if (Application.isPlaying) {
                Gizmos.color = color;
                if (PortalScript) {
                    Gizmos.matrix = PortalScript.transform.localToWorldMatrix;
                    Gizmos.matrix *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 0.1f));
                    Gizmos.DrawMesh(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cube));
                }
            }

#endif
        }

        /// <summary>
        ///     Removes preview objects from the scene
        /// </summary>
        public void CleanupTemp() {
            if (this)
                CleanupTempRecursive(transform);
        }

        private void CleanupTempRecursive(Transform targetTransform) {
            foreach (Transform t in targetTransform) {
                CleanupTempRecursive(t);

                if (t.gameObject.CompareTag(Keywords.Tags.SKEditorTemp))
                    DestroyImmediate(t.gameObject, true);
            }
        }

#if UNITY_EDITOR
        public void OnDrawGizmosSelected() {
            if (!SKSGlobalRenderSettings.Gizmos)
                return;
            if (!SKSGlobalRenderSettings.Visualization)
                return;
            if (!this || !gameObject || !transform || !TargetController)
                return;

            var defaultSize = 1f;
            //Dir vector to second portal
            var diffVector = (TargetController.transform.position - transform.position).normalized;

            //orientation indicator
            Handles.color = new Color(255, 255, 255, 0.7f);
            Handles.ArrowHandleCap(0, transform.position, transform.rotation * Quaternion.Euler(0, 180, 0),
                defaultSize * ((Mathf.Sin(Time.time * 2f) + 1) * 0.5f / 10f + 0.5f), EventType.Repaint);

            if (TargetController) {
                var iterations =
                    Mathf.RoundToInt(Vector3.Distance(transform.position, TargetController.transform.position) / 1f);
                for (var i = 0; i < iterations; i++) {
                    var timeScalar = ((float) i / iterations + Time.time / 5f) % 1f;


                    //Set the color to be between the two portal's colors
                    var arrowColor = Color.Lerp(color, TargetController.color, timeScalar);
                    arrowColor.a = 0.5f;
                    Handles.color = arrowColor;

                    var arrowSize = defaultSize;

                    //Place arrows and move them accordingly
                    var arrowPosition = Vector3.Lerp(transform.position - diffVector * arrowSize,
                        TargetController.transform.position, timeScalar);


                    //Scale arrows down as they approach destination
                    var distanceToTarget = Vector3.Distance(arrowPosition, TargetController.transform.position);
                    float distanceToOrigin;
                    if (distanceToTarget <= 1)
                        arrowSize *= distanceToTarget;
                    //Scale arrows up as they leave origin
                    else if ((distanceToOrigin =
                                 Vector3.Distance(arrowPosition + diffVector * arrowSize, transform.position)) <=
                             1) arrowSize *= distanceToOrigin;

                    //Scale arrows up as they spawn from origin

                    Handles.ArrowCap(0, arrowPosition,
                        Quaternion.LookRotation(diffVector, Vector3.up),
                        arrowSize);
                }

                //Draw children's gizmos
                foreach (Transform t in transform)
                    if (t != transform)
                        RecursiveTryDrawGizmos(t);

                //Draw detection zone preview

                //Gizmos.DrawMesh(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cube));
                if (!Application.isPlaying) {
                    Gizmos.color = new Color(1, 1, 1, 0.2f);
                    var detectionCenter = transform.position -
                                          transform.forward * (DetectionScale / 2f) * transform.lossyScale.z / 2f;
                    Gizmos.DrawWireMesh(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cube), detectionCenter,
                        transform.rotation,
                        Vector3.Scale(transform.lossyScale, new Vector3(1, 1, DetectionScale / 2f)));
                    var style = new GUIStyle(new GUIStyle {alignment = TextAnchor.MiddleCenter});
                    style.normal.textColor = Color.white;
                    Handles.Label(detectionCenter, "Portal Detection zone", style);
                }
            }
        }

        private RenderData renderData;

        /// <summary>
        ///     Handles scenveview rendering of portal previews and related editor utilities
        /// </summary>
        private void OnWillRenderObject() {
#if UNITY_EDITOR
            if (!(Selection.activeGameObject == gameObject)) return;

            if (!TargetController || !PortalMaterial ||
                !Mask || !SKSGlobalRenderSettings.Preview ||
                !this || Application.isPlaying)
                return;

            var previewRenderer = PreviewRoot.GetComponent<MeshRenderer>();
            previewRenderer.sharedMaterial = PortalMaterial;
            //previewRenderer.enabled = true;

            var lib = PreviewCamera.GetComponent<SKSRenderLib>();
            PreviewCamera.transform.localPosition = Vector3.zero;

            var sceneCam = SceneView.GetAllSceneCameras()[0];

            var cam = PreviewCamera;


            GL.Clear(true, true, Color.black);
            Graphics.SetRenderTarget(null);

            var renderProps = new RenderProperties();

            //renderState |= RenderState.FirstRender;
            renderProps |= RenderProperties.Optimize;
            renderProps |= SKSGlobalRenderSettings.Inverted ? RenderProperties.InvertedCached : 0;
            renderProps |= !SKSGlobalRenderSettings.UvFlip ? RenderProperties.UvFlipCached : 0;
            renderProps |= RenderProperties.ObliquePlane;
            renderProps |= RenderProperties.FirstRender;
            renderProps |= RenderProperties.RipCustomSkybox;

            var rend = GetComponent<MeshRenderer>();
            var rend2 = TargetController.GetComponent<MeshRenderer>();
            var mesh = PreviewRoot.GetComponent<MeshFilter>().sharedMesh;
            //TargetController.PreviewRoot.GetComponent<MeshRenderer>().enabled = false;
            //TargetController.GetComponent<MeshRenderer>().enabled = false;
            cam.transform.localPosition = Vector3.zero;
            TargetController.PreviewRoot.transform.localPosition = Vector3.zero;

            cam.transform.rotation = TargetController.PreviewRoot.transform.rotation *
                                     (Quaternion.Inverse(transform.rotation) *
                                      sceneCam.transform.rotation);

            TargetController.PreviewRoot.transform.localScale = Vector3.one;

            if (renderData == null) {
                renderData = new RenderData(renderProps, cam, sceneCam,
                    sceneCam.projectionMatrix, TextureTargetEye.Right,
                    PortalMaterial, new Vector2(Screen.currentResolution.width,
                        Screen.currentResolution.height), previewRenderer, rend2, null, null, mesh, 1, 0, false, 0);
            }
            else {
                renderData.Position = sceneCam.transform.position;
                renderData.ProjectionMatrix = sceneCam.projectionMatrix;
                renderData.ScreenSize = new Vector2(Screen.currentResolution.width,
                    Screen.currentResolution.height);
                renderData.RenderingCamera = PreviewCamera;
                renderData.SourceRenderer = previewRenderer;
            }

            try {
                lib.RenderCamera(renderData);
            }
            catch {
                //Doesn't really matter what happened here, unity editor strangeness sometimes hucks issues
                Graphics.SetRenderTarget(null);
                lib.TerminateRender();
                return;
            }


            var block = new MaterialPropertyBlock();
            previewRenderer.GetPropertyBlock(block);

            var output = (RenderTexture) block.GetTexture(TextureTargetEye.Right.Name());
            if (output)
                previewRenderer.sharedMaterial.SetTexture(TextureTargetEye.Right.Name(), output);
            if (output)
                previewRenderer.sharedMaterial.SetTexture(TextureTargetEye.Left.Name(), output);
            if (output)
                block.SetTexture(TextureTargetEye.Left.Name(), output);

            Graphics.SetRenderTarget(null);
            lib.TerminateRender();
#endif
        }

        /// <summary>
        ///     Draw all gizmos for an object and its children, even if the children
        ///     are hidden in the inspector
        /// </summary>
        /// <param name="target">Transform to target</param>
        private void RecursiveTryDrawGizmos(Transform target) {
            foreach (Transform t in target) {
                var behaviour = t.GetComponents<MonoBehaviour>();
                if (behaviour.Length > 0)
                    behaviour[0].SendMessage("OnDrawGizmosSelected", SendMessageOptions.DontRequireReceiver);
                RecursiveTryDrawGizmos(t);
            }
        }
#endif
    }
}