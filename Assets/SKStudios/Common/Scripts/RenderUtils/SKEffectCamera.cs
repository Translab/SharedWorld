using System;
using System.Collections;
using System.Collections.Generic;
using SKStudios.Common.Rendering;
using SKStudios.Common.Utils;
using SKStudios.Common.Utils.VR;
using SKStudios.Portals;
using SKStudios.ProtectedLibs.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SKStudios.Rendering {
    /// <summary>
    ///     Class handling Effects best described as "rendering through an object", i.e. Portals, Mirrors, and similar.
    /// </summary>
    public class SKEffectCamera : MonoBehaviour {
        /// <summary>
        ///     Amount of pixels to pad valid renders by to prevent aliasing artifacts
        /// </summary>
        private const int PixelPadding = 0;

        
        public static int CurrentDepth;

        //Keeps track of cameras actively rendering during this frame for image processing
        public static List<Camera> RenderingCameras = new List<Camera>();
        public static Rect LastRect;

        //Render lib
        private SKSRenderLib _cameraLib;

        private int _recursionNumber;

        private RenderData _renderDataTemplate;
        private RenderProperties _renderProperties = 0;
        public bool Initialized;
        public Camera[] RecursionCams;

        private static Camera _mainCamera;
        private static Camera MainCamera {
            get {
                if (!_mainCamera)
                    _mainCamera = Camera.main;
                return _mainCamera;
            }
        }

        private SKSRenderLib CameraLib {
            get {
                if (!_cameraLib)
                    _cameraLib = gameObject.AddComponent<SKSRenderLib>();
                return _cameraLib;
            }
        }


        public Transform RenderingCameraParent { get; set; }

        public Transform OriginTransform { get; private set; }
        public Transform DestinationTransform { get; private set; }

        public Camera PrimaryCam { get; set; }
        public Mesh BlitMesh { get; set; }
        private SKEffectCamera OtherCamera { get; set; }

        private RenderData RenderDataTemplate {
            get {
                if (_renderDataTemplate == null) {
                    if (MainCamera == null)
                        return null;

                    _renderDataTemplate = new RenderData(MainCamera.pixelRect.size,
                        SKSGlobalRenderSettings.RecursionNumber,
                        SKSGlobalRenderSettings.AdaptiveQuality,
                        PixelPadding);
                    _renderDataTemplate.InitCache();
                }

                return _renderDataTemplate;
            }
        }

        /// <summary>
        ///     Initialize this SKEffectCamera
        /// </summary>
        /// <param name="other">The Sister SKSEffectCamera</param>
        /// <param name="material">The Material to be rendered to</param>
        /// <param name="sourceRenderer">Source renderer</param>
        /// <param name="targetRenderer">Target Renderer</param>
        /// <param name="mesh">Mesh to be rendered to</param>
        public void Initialize(SKEffectCamera other, Material material, MeshRenderer sourceRenderer,
            MeshRenderer targetRenderer, Mesh mesh, Transform originTransform, Transform destinationTransform) {
            StartCoroutine(InitializeRoutine(other, material, sourceRenderer, targetRenderer, mesh, originTransform,
                destinationTransform));
        }

        private IEnumerator InitializeRoutine(SKEffectCamera other, Material material, MeshRenderer sourceRenderer,
            MeshRenderer targetRenderer, Mesh mesh, Transform originTransform, Transform destinationTransform) {
            while (RenderDataTemplate == null) yield return WaitCache.Frame;
            OtherCamera = other;
            PrimaryCam = GetComponent<Camera>();
            PrimaryCam.enabled = false;
            CameraLib.Initialize(originTransform, destinationTransform);
            OriginTransform = originTransform;
            DestinationTransform = destinationTransform;
            RenderDataTemplate.Material = material;
            RenderDataTemplate.SourceRenderer = sourceRenderer;
            RenderDataTemplate.TargetRenderer = targetRenderer;
            RenderDataTemplate.SourceCollider = sourceRenderer.GetComponent<Collider>();
            RenderDataTemplate.TargetCollider = targetRenderer.GetComponent<Collider>();
            RenderDataTemplate.Mesh = mesh;

            //Init cache
            for (var i = 0; i < (int) TextureTargetEye.Count; i++)
                RenderDataTemplate.RenderDataCache[i] = RenderDataTemplate.Clone();

            RenderDataTemplate.InitCache();
            UpdateMaterial(material);

            InstantiateRecursion(SKSGlobalRenderSettings.RecursionNumber);
            Initialized = true;
        }

        /// <summary>
        ///     Updates the material to be rendered to
        /// </summary>
        /// <param name="m"></param>
        public void UpdateMaterial(Material m) {
            StartCoroutine(UpdateMaterialRoutine(m));
        }

        private IEnumerator UpdateMaterialRoutine(Material m) {
            while (RenderDataTemplate == null)
                yield return WaitCache.Frame;
            RenderDataTemplate.Material = m;
            for (var i = 0; i < (int) TextureTargetEye.Count; i++)
                RenderDataTemplate.RenderDataCache[i].Material = m;
        }

        /// <summary>
        ///     Updates the target renderer
        /// </summary>
        /// <param name="targetRenderer"></param>
        public void UpdateTargetRenderer(MeshRenderer targetRenderer) {
            RenderDataTemplate.TargetRenderer = targetRenderer;
            for (var i = 0; i < (int) TextureTargetEye.Count; i++)
                RenderDataTemplate.RenderDataCache[i].TargetRenderer = targetRenderer;
        }

        /// <summary>
        ///     Updates the mesh to be rendered to
        /// </summary>
        /// <param name="mesh"></param>
        public void UpdateMesh(Mesh mesh) {
            RenderDataTemplate.Mesh = mesh;
            for (var i = 0; i < (int) TextureTargetEye.Count; i++)
                RenderDataTemplate.RenderDataCache[i].Mesh = mesh;
        }

        /// <summary>
        ///     Instantiation is done on awake
        /// </summary>
        private void Awake() {
            enabled = false;
        }

        private void LateUpdate() {
            _recursionNumber = 0;
        }

        /// <summary>
        ///     Instantiate recursion cameras
        /// </summary>
        /// <param name="count"></param>
        private void InstantiateRecursion(int count) {
            StartCoroutine(InstantiateRecursionEnumerator(count));
        }

        private IEnumerator InstantiateRecursionEnumerator(int count) {
            while (RenderDataTemplate == null) yield return WaitCache.Frame;
            count++;
            RecursionCams = new Camera[count + 1];
            var name = this.name + Random.value;
            var mainMarker = gameObject.AddComponent<CameraMarker>();
            mainMarker.Initialize(CameraLib, RenderDataTemplate.SourceRenderer);
            for (var i = 1; i < count; i++) {
                var cam = InstantiateCamera(i);
                cam.name = name + "Recursor " + i;
            }

            RecursionCams[0] = PrimaryCam;
        }

        /// <summary>
        ///     Sets up and returns a recursion camera at index i
        /// </summary>
        /// <param name="index">index</param>
        /// <returns></returns>
        private Camera InstantiateCamera(int index) {
            var cameraRecursor = new GameObject();
#if !SKS_DEV
            cameraRecursor.hideFlags = HideFlags.HideAndDontSave;
#endif
            cameraRecursor.transform.SetParent(RenderingCameraParent);
            cameraRecursor.transform.localPosition = Vector3.zero;
            cameraRecursor.transform.localRotation = Quaternion.identity;

            var marker = cameraRecursor.AddComponent<CameraMarker>();
            marker.Initialize(CameraLib, RenderDataTemplate.SourceRenderer);
            var newCam = cameraRecursor.AddComponent<Camera>();

            newCam.cullingMask = PrimaryCam.cullingMask;

            newCam.renderingPath = PrimaryCam.renderingPath;

            newCam.useOcclusionCulling = PrimaryCam.useOcclusionCulling;
            newCam.depthTextureMode = PrimaryCam.depthTextureMode;
            newCam.enabled = false;

            newCam.ResetProjectionMatrix();
            newCam.ResetWorldToCameraMatrix();
            newCam.ResetCullingMatrix();

            RecursionCams[index] = newCam;
            return newCam;
        }


        public void ForceResetRender() {
            CurrentDepth = 0;
            _recursionNumber = 0;
        }

        /// <summary>
        ///     Renders the view of a given Renderer as if it were through another renderer. Returns true if successful.
        /// </summary>
        /// <param name="headCam">The origin camera</param>
        /// <param name="material">The Material to modify</param>
        /// <param name="sourceRenderer">The Source renderer</param>
        /// <param name="targetRenderer">The Target renderer</param>
        /// <param name="mesh">The Mesh of the source Renderer</param>
        /// <param name="obliquePlane">Will the projection matrix be clipped at the near plane?</param>
        /// <param name="is3d">Is the renderer not being treated as two-dimenstional?</param>
        /// <param name="isMirror">Is the renderer rendering through itself?</param>
        /// <param name="isSSR">Is the renderer a low quality effect renderer, similar to ssr?</param>
        public bool RenderIntoMaterial(Camera headCam, Material material, MeshRenderer sourceRenderer,
            MeshRenderer targetRenderer, Mesh mesh, bool obliquePlane = true, bool is3d = false, bool isMirror = false,
            bool isSSR = false) {
           
            if (!Initialized) return false;
#if !SKS_VR
            headCam.stereoTargetEye = StereoTargetEyeMask.None;
#endif

            _renderDataTemplate = RenderDataTemplate;
            RenderDataTemplate.ScreenSize = new Vector2(MainCamera.pixelWidth, MainCamera.pixelHeight);
#if !SKS_PORTALS
//if (camera.transform.parent == transform.parent)
//    return;
#endif

            var firstRender = false;
            var renderingCamera = RecursionCams[CurrentDepth];

            //Render Placeholder if max depth hit
            if (CurrentDepth > SKSGlobalRenderSettings.RecursionNumber) return false;

            var renderTarget = headCam.targetTexture;
            var marker = CameraMarker.GetMarker(headCam);

            if (marker)
                if (marker.Owner == OtherCamera)
                    return false;

            Graphics.SetRenderTarget(renderTarget);

            //Sets up the Render Properties for this render
            var renderProps = new RenderProperties();

            //Is this the first time that the IsMirror is being rendered this frame?
            if (headCam == MainCamera)
                firstRender = true;

            renderProps |= firstRender ? RenderProperties.FirstRender : 0;
            //todo: reenable
            renderProps |= RenderProperties.Optimize;
            renderProps |= CurrentDepth < 1
                ? (obliquePlane ? RenderProperties.ObliquePlane : 0)
                : RenderProperties.ObliquePlane;
            renderProps |= isMirror ? RenderProperties.Mirror : 0;
            renderProps |= isSSR ? RenderProperties.IsSSR : 0;
            renderProps |= SKSGlobalRenderSettings.CustomSkybox ? RenderProperties.RipCustomSkybox : 0;
            renderProps |= SKSGlobalRenderSettings.AggressiveRecursionOptimization
                ? RenderProperties.AggressiveOptimization
                : 0;
            if (firstRender) {
                renderProps |= SKSGlobalRenderSettings.Inverted ? RenderProperties.InvertedCached : 0;
                renderProps |= SKSGlobalRenderSettings.UvFlip ? RenderProperties.UvFlipCached : 0;
            }

#if SKS_VR
            renderProps |= RenderProperties.VR;
            renderProps |= SKSGlobalRenderSettings.SinglePassStereo ? RenderProperties.SinglePass : 0;
#endif
            //renderProps &= ~RenderProperties.Optimize;
            _recursionNumber++;

            //Renders the IsMirror itself to the rendertexture
            transform.SetParent(RenderingCameraParent);

            CurrentDepth++;

            renderingCamera.renderingPath = headCam.renderingPath;
            renderingCamera.cullingMask = headCam.cullingMask;
            renderingCamera.cullingMask |= 1 << Keywords.Layers.CustomRendererOnly;

            renderingCamera.stereoTargetEye = StereoTargetEyeMask.None;

            renderingCamera.enabled = false;
            RenderingCameras.Add(headCam);

            //Set up the RenderData for the current frame
            RenderDataTemplate.OriginCamera = headCam;
            RenderDataTemplate.RenderingCamera = renderingCamera;
            RenderDataTemplate.CurrentDepth = CurrentDepth;

            //Copy per-frame values
            for (var i = 0; i < (int) TextureTargetEye.Count; i++)
                RenderDataTemplate.RenderDataCache[i].CopyFrameData(RenderDataTemplate);
#if SKS_VR
//Stereo Rendering
            if (headCam.stereoTargetEye == StereoTargetEyeMask.Both) {
                RenderingCameraParent.rotation = DestinationTransform.rotation *
                    (Quaternion.Inverse(OriginTransform.rotation) *
                    (headCam.transform.rotation));


                //Todo: Figure out why this optimization doesn't work in VR mode
                //var tempDataLeft = RenderDataTemplate.RenderDataCache[(int)TextureTargetEye.Left];
                //var tempDataRight = RenderDataTemplate.RenderDataCache[(int)TextureTargetEye.Right];
                var tempDataLeft = RenderDataTemplate.Clone();
                var tempDataRight = RenderDataTemplate.Clone();

                //Left eye
                tempDataLeft.Position = -SkvrEyeTracking.EyeOffset(headCam);
                tempDataLeft.ProjectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                tempDataLeft.TargetEye = TextureTargetEye.Left;
                tempDataLeft.RenderProperties = renderProps;
                _cameraLib.RenderCamera(tempDataLeft);
                Debug.DrawRay(tempDataLeft.Position, headCam.transform.forward, Color.magenta, 1);
                //Right eye
                tempDataRight.Position = SkvrEyeTracking.EyeOffset(headCam);
                tempDataRight.ProjectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                tempDataRight.TargetEye = TextureTargetEye.Right;
                tempDataRight.RenderProperties = renderProps;
                //Debug.DrawRay(tempDataRight.Position, headCam.transform.forward, Color.cyan, 1);

                if (!SKSGlobalRenderSettings.SinglePassStereo)
                {
                    _cameraLib.RenderCamera(tempDataRight);
                }
                else
                {
                    renderingCamera.stereoTargetEye = StereoTargetEyeMask.Right;
                    _cameraLib.RenderCamera(tempDataRight);
                }
            }

            else
            {
                //Non-VR rendering with VR enabled
                //Todo: Figure out why this optimization doesn't work in VR mode
                /*
                var tempData =
                    RenderDataTemplate.RenderDataCache[
                        (int)TextureTargetEyeMethods.StereoTargetToTextureTarget(
                            renderingCamera.stereoTargetEye
                            )
                    ];*/
                var tempData = RenderDataTemplate.Clone();

                renderProps &= ~RenderProperties.SinglePass;
                renderProps &= ~RenderProperties.VR;

                tempData.RenderProperties = renderProps;
                renderingCamera.transform.rotation = DestinationTransform.rotation *
                    (Quaternion.Inverse(OriginTransform.rotation) *
                    (headCam.transform.rotation));

                tempData.ProjectionMatrix = headCam.projectionMatrix;
                tempData.Position = renderingCamera.transform.position;

                if (renderingCamera.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    tempData.TargetEye = TextureTargetEye.Left;
                    _cameraLib.RenderCamera(tempData);
                }
                else
                {
                    tempData.TargetEye = TextureTargetEye.Right;
                    _cameraLib.RenderCamera(tempData);
                }
            }

#else
            //Non-stereo rendering
            //RenderData.Position = camera.transform.position;
            var tempData = RenderDataTemplate.RenderDataCache[(int) TextureTargetEye.Right];
            tempData.ProjectionMatrix = headCam.projectionMatrix;
            tempData.RenderProperties = renderProps;
            renderingCamera.transform.rotation = DestinationTransform.rotation *
                                                 (Quaternion.Inverse(OriginTransform.rotation) *
                                                  headCam.transform.rotation);
            CameraLib.RenderCamera(tempData);
#endif
            CurrentDepth--;

            RenderingCameras.Remove(headCam);
            if (RenderingCameras.Count == 0)
                try {
                    //_cameraLib.TerminateRender();
                    //SKSRenderLib.ClearUnwinder();
                }
                catch (NullReferenceException e) {
                    Debug.LogWarning("Attempted to render without proper setup");
                }

            return true;
        }


        private void OnDisable() {
            //if (_cameraLib)
            //    Destroy(_cameraLib);
            CameraMarker marker;
            if (marker = gameObject.GetComponent<CameraMarker>()) Destroy(marker);

            foreach (var c in RecursionCams)
                if (c && c.gameObject)
                    if (c != PrimaryCam)
                        Destroy(c.gameObject);
        }
    }
}