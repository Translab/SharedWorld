using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKStudios.Common;
using SKStudios.Common.Utils;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEditorInternal;
#endif
#if SKS_VR
using UnityEngine.VR;
#endif

namespace SKStudios.Rendering {
    /// <summary>
    ///     Stores settings for Mirror that are used globally
    /// </summary>
    [Serializable]
    [ExecuteInEditMode]
    [CreateAssetMenu(menuName = "ScriptableObjects/SKGlobalRenderSettings")]
    public class SKSGlobalRenderSettings : ScriptableObject {
        [SerializeField] [HideInInspector] private bool _shouldOverrideMaskCached;
        [SerializeField] [HideInInspector] private Texture2D _maskCached;
        [SerializeField] [HideInInspector] private bool _physicsPassthroughCached;
        [SerializeField] [HideInInspector] private int _importedCached;
        [SerializeField] [HideInInspector] private int _recursionNumberCached;
        [SerializeField] [HideInInspector] private bool _invertedCached;
        [SerializeField] [HideInInspector] private bool _uvFlipCached;
        [SerializeField] [HideInInspector] private bool _clippingCached;
        [SerializeField] [HideInInspector] private bool _physStyleBCached;
        [SerializeField] [HideInInspector] private bool _minimizedCached;
        [SerializeField] [HideInInspector] private bool _closedCached;
        [SerializeField] [HideInInspector] private bool _MirrorVisualizationCached;
        [SerializeField] [HideInInspector] private bool _MirrorGizmosCached;
        [SerializeField] [HideInInspector] private bool _MirrorPreviewCached;
        [SerializeField] [HideInInspector] private bool _nonscaledMirrorsCached;
        [SerializeField] [HideInInspector] private bool _adaptiveQualityCached;
        [SerializeField] [HideInInspector] private bool _aggressiveRecursionOptimizationCached;
        [SerializeField] [HideInInspector] private bool _customSkyboxCached;
        [SerializeField] [HideInInspector] private float _ipdCached = 0.00068f;
        [SerializeField] [HideInInspector] private List<int> _ignoredNotifications = new List<int>();
#if SKS_VR
        [SerializeField] [HideInInspector] public bool SinglePassStereoCached;
#endif


        private static SKSGlobalRenderSettings _instance;

        /// <summary>
        ///     Get the singleton instance of this object
        /// </summary>
        public static SKSGlobalRenderSettings Instance {
            get {
                var loaded = false;
                if (!_instance) {
#if UNITY_EDITOR
                    AssetDatabase.Refresh();
#endif
                    loaded = true;
                    _instance = Resources.Load<SKSGlobalRenderSettings>("SK Global Render Settings");

                    if (_instance)
                        _instance.Initialize();
                }

                if (!_instance) loaded = false;

                if (loaded) {
#if UNITY_EDITOR
                    EditorApplication.playmodeStateChanged -= RecompileCleanup;
                    EditorApplication.playmodeStateChanged += RecompileCleanup;
#endif
                }

                return _instance;
            }
        }

        /// <summary>
        ///     Is Physic style B enabled?
        /// </summary>
        public static bool PhysStyleB {
            get { return Instance._physStyleBCached; }
            set {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                   Undo.RecordObject(Instance, "Global Settings");
#endif

                Instance._physStyleBCached = value;
            }
        }


        /// <summary>
        ///     Should the mask for all Mirrors be overridden?
        /// </summary>
        public static bool ShouldOverrideMask {
            get { return Instance._shouldOverrideMaskCached; }
            set {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                   Undo.RecordObject(Instance, "Global Settings");
#endif
                Instance._shouldOverrideMaskCached = value;
            }
        }

        /// <summary>
        ///     Texture2D with which the masks on all Mirrors are overwritten if set
        /// </summary>
        public static Texture2D Mask {
            get { return Instance._maskCached; }
            set {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                   Undo.RecordObject(Instance, "Global Settings");
#endif
                Instance._maskCached = value;
            }
        }

        /// <summary>
        ///     Is the UV inverted? Changing this value changes the global "_InvertOverride" value.
        /// </summary>
        public static bool Inverted {
            get { return Instance._invertedCached; }
            set {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                   Undo.RecordObject(Instance, "Global Settings");
#endif
                Shader.SetGlobalFloat(Keywords.ShaderKeys.InvertOverride, value ? 1 : 0);
                Instance._invertedCached = value;
            }
        }

        /// <summary>
        ///     Is the UV flipped? Changing this value changes the global "_YFlipOverride" value.
        /// </summary>
        public static bool UvFlip {
            get { return Instance._uvFlipCached; }
            set {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                   Undo.RecordObject(Instance, "Global Settings");
#endif
                Shader.SetGlobalFloat(Keywords.ShaderKeys.YFlipOverride, value ? 1 : 0);
                Instance._uvFlipCached = value;
            }
        }

        /// <summary>
        ///     Is Object clipping enabled to make objects disappear as they enter applicable products?
        /// </summary>
        public static bool Clipping {
            get { return Instance._clippingCached; }
            set {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                   Undo.RecordObject(Instance, "Global Settings");
#endif
                //Keywords.ShaderKeys.ClipOverride
                Shader.SetGlobalFloat(Keywords.ShaderKeys.ClipOverride, value ? 0 : 1);
                Instance._clippingCached = value;
            }
        }

        /// <summary>
        ///     Are passthrough physics simulated in applicable products?
        /// </summary>
        public static bool PhysicsPassthrough {
            get { return Instance._physicsPassthroughCached; }
            set {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                   Undo.RecordObject(Instance, "Global Settings");
#endif

                Instance._physicsPassthroughCached = value;
            }
        }

        /// <summary>
        ///     Has the asset been imported?
        /// </summary>
        public static int Imported {
            get { return Instance._importedCached; }
            set {
#if UNITY_EDITOR
               Undo.RecordObject(Instance, "Global Settings");
#endif
                Instance._importedCached = value;
            }
        }

        /// <summary>
        ///     how many times should <see cref="EffectRenderer"/> recurse while rendering?
        /// </summary>
        public static int RecursionNumber {
            get { return Instance._recursionNumberCached; }
            set {
#if UNITY_EDITOR
               Undo.RecordObject(Instance, "Global Settings");
#endif
                Instance._recursionNumberCached = value;
            }
        }

        /// <summary>
        ///     Is the sceneview menu minimized?
        /// </summary>
        public static bool Minimized {
            get { return Instance._minimizedCached; }
            set {
#if UNITY_EDITOR
               Undo.RecordObject(Instance, "Global Settings");
#endif
                Instance._minimizedCached = value;
            }
        }

        /// <summary>
        ///     Should differently-sized renderers (and portals) scale?
        /// </summary>
        public static bool NonScaledRenderers {
            get { return Instance._nonscaledMirrorsCached; }
            set {
#if UNITY_EDITOR
               Undo.RecordObject(Instance, "Global Settings");
#endif
                Instance._nonscaledMirrorsCached = value;
            }
        }

        /// <summary>
        ///     Is Aggressive Recursion optimization enabled?
        /// </summary>
        public static bool AggressiveRecursionOptimization {
            get { return Instance._aggressiveRecursionOptimizationCached; }
            set {
#if UNITY_EDITOR
               Undo.RecordObject(Instance, "Global Settings");
#endif
                Instance._aggressiveRecursionOptimizationCached = value;
            }
        }

        /// <summary>
        ///     Is the MirrorMenu closed?
        /// </summary>
        public static bool MenuClosed {
            get { return Instance._closedCached; }
            set {
#if UNITY_EDITOR
               Undo.RecordObject(Instance, "Global Settings");
#endif
                Instance._closedCached = value;
            }
        }

        /// <summary>
        ///     Enable visualization of custom renderer connections
        /// </summary>
        public static bool Visualization {
            get { return Instance._MirrorVisualizationCached; }
            set {
#if UNITY_EDITOR
               Undo.RecordObject(Instance, "Global Settings");
#endif
                Instance._MirrorVisualizationCached = value;
            }
        }


        /// <summary>
        ///     Enable Custom Skybox rendering, to fix broken skybox issues induced by optimizations.
        /// </summary>
        public static bool CustomSkybox {
            get { return Instance._customSkyboxCached; }
            set {
#if UNITY_EDITOR
               Undo.RecordObject(Instance, "Global Settings");
#endif
                Instance._customSkyboxCached = value;
            }
        }

        /// <summary>
        ///     Disable showing Mirror gizmos even while selected
        /// </summary>
        public static bool Gizmos {
            get { return Instance._MirrorGizmosCached; }
            set {
#if UNITY_EDITOR
               Undo.RecordObject(Instance, "Global Settings");
#endif
                Instance._MirrorGizmosCached = value;
            }
        }


        /// <summary>
        ///     Disable showing Mirror gizmos even while selected
        /// </summary>
        public static bool Preview {
            get { return Instance._MirrorPreviewCached; }
            set {
#if UNITY_EDITOR
               Undo.RecordObject(Instance, "Global Settings");
#endif
                Instance._MirrorPreviewCached = value;
            }
        }


        /// <summary>
        ///     Is the MirrorMenu minimized?
        /// </summary>
        public static bool AdaptiveQuality {
            get { return Instance._adaptiveQualityCached; }
            set {
#if UNITY_EDITOR
               Undo.RecordObject(Instance, "Global Settings");
#endif
                Instance._adaptiveQualityCached = value;
            }
        }

        /// <summary>
        ///     Is the MirrorMenu minimized?
        /// </summary>
        public static List<int> IgnoredNotifications {
            get { return Instance._ignoredNotifications; }
            set {
#if UNITY_EDITOR
               Undo.RecordObject(Instance, "Global Settings");
#endif
                Instance._ignoredNotifications = value;
            }
        }


        public static bool LightPassthrough;

#if SKS_VR
/// <summary>
/// Is SinglePassStereo rendering enabled?
/// </summary> 
        public static bool SinglePassStereo{
            get { return Instance.SinglePassStereoCached; }
            set { 
#if UNITY_EDITOR
               Undo.RecordObject(Instance, "Global Settings");
#endif
    Instance.SinglePassStereoCached = value; 
}
        }

/// <summary>
/// IPD of eyes (temporary while SteamVR fixes itself)
/// </summary>
        public static float IPD
        {
            get { return Instance._ipdCached; }
            set { 
#if UNITY_EDITOR
               Undo.RecordObject(Instance, "Global Settings");
#endif
    Instance._ipdCached = value; 
}
        }
#endif
        /// <summary>
        ///     Initialize
        /// </summary>
        public void OnEnable() {
            Initialize();
        }

        // Use this for initialization
        private void Initialize() {
#if SKS_VR
            SinglePassStereo = SinglePassStereoCached;
            //SinglePassStereo = SinglePassStereoCached;
            //Debug.Log("Single Pass Stereo Mode: " + SinglePassStereo);
            Shader.SetGlobalFloat(Keywords.ShaderKeys.Vr, 1);
#else
            Shader.SetGlobalFloat(Keywords.ShaderKeys.Vr, 0);


#endif
        }

        public override string ToString() {
            var builder = new StringBuilder();
            builder.Append("SKSRenderSettings:{");
            builder.Append(_shouldOverrideMaskCached ? 0 : 1);
            builder.Append(_maskCached ? 0 : 1);
            builder.Append(_physicsPassthroughCached ? 0 : 1);
            builder.Append(_invertedCached ? 0 : 1);
            builder.Append(_uvFlipCached ? 0 : 1);
            builder.Append(_clippingCached ? 0 : 1);
            builder.Append(_physStyleBCached ? 0 : 1);
            builder.Append(_minimizedCached ? 0 : 1);
            builder.Append(_closedCached ? 0 : 1);
            builder.Append(_MirrorVisualizationCached ? 0 : 1);
            builder.Append(_MirrorGizmosCached ? 0 : 1);
            builder.Append(_MirrorPreviewCached ? 0 : 1);
            builder.Append(_nonscaledMirrorsCached ? 0 : 1);
            builder.Append(_adaptiveQualityCached ? 0 : 1);
            builder.Append(_aggressiveRecursionOptimizationCached ? 0 : 1);
            builder.Append(_customSkyboxCached ? 0 : 1).Append('|');
            builder.Append(_importedCached).Append('|');
            builder.Append(_recursionNumberCached);
            builder.Append("}");
            return builder.ToString();
        }

#if UNITY_EDITOR
        [DidReloadScripts]
#endif
        private static void RecompileCleanup()
        {
#if UNITY_EDITOR
if (Application.isPlaying) return;
            if (!InternalEditorUtility.tags.Contains("SKSEditorTemp")) return;
            var tempObjects = GameObject.FindGameObjectsWithTag("SKSEditorTemp");
            for (var i = 0; i < tempObjects.Length; i++) DestroyImmediate(tempObjects[i], true);

#endif
        }
    }
}