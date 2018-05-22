using System;
using System.ComponentModel.Design.Serialization;
using System.IO;
using SKStudios.Common.Editor;
using SKStudios.Common.Utils;
using UnityEditor;
using UnityEngine;
using SKStudios.Rendering;
using UnityEditor.AnimatedValues;

namespace SKStudios.Portals.Editor
{
    /// <summary>
    ///     Editor for <see cref="PortalController"/> components, which control and spawn <see cref="Portal"/>s.
    /// </summary>
    [CustomEditor(typeof(PortalController))]
    [InitializeOnLoad]
    [CanEditMultipleObjects]
    [ExecuteInEditMode]
    public class PortalControllerEditor : EffectRendererEditor
    {
        protected override string AssetName {
            get { return "PortalKit Pro"; }
        }

        private static bool _interactionFoldout = false;
        private static bool _editorFoldout = false;
        private static bool _imageFoldout = false;

        protected override GameObject Target {
            get { return TargetController.gameObject; }
        }

        public PortalControllerEditor() {

        }

        protected override bool MirroredPreview {
            get { return false; }
        }

        protected override Material TargetMaterial {
            get { return TargetController.PortalMaterial; }
            set {
                if (!TargetController)
                    return;

                Undo.RecordObject(TargetController, "Material change");
                TargetController.PortalMaterial = value;

                /*
                try
                {
                    DestroyImmediate(_matEditor, true);
                }
                catch { }*/
            }
        }

        private static Camera _sceneCameraDupe;

        private String sourceName;

        private void OnEnable()
        {

            if (Application.isPlaying) return;

            if (!TargetController.gameObject.activeInHierarchy)
                return;

            if (!TargetController.isActiveAndEnabled)
                return;
            if (!TargetController.TargetController)
                return;

          

            if (SKSGlobalRenderSettings.Preview)
            {

                //PortalController.GetComponent<Renderer>().sharedMaterial.color = Color.clear;
                Camera pokecam = TargetController.PreviewCamera;
                GameObject pokeObj = TargetController.PreviewRoot;
                Camera pokecam2 = TargetController.TargetController.PreviewCamera;
                GameObject pokeObj2 = TargetController.TargetController.PreviewRoot;
                pokecam2.enabled = false;
                pokeObj2.SetActive(false);
            }



            //EditorApplication.update -= UpdatePreview;
            //EditorApplication.update += UpdatePreview;

#if SKS_VR
            //GlobalPortalSettings.SinglePassStereo = settings.SinglePassStereoCached;
#endif

        }

        
        private void OnDisable()
        {
            CleanupTemp();

            if (!Application.isPlaying)
                return;
            //EditorApplication.update -= UpdatePreview;

           
            if (TargetController && TargetController.TargetController)
                TargetController.TargetController.CleanupTemp();
            //DestroyImmediate(_matEditor, true);

            if (Application.isPlaying)
                return;

            if (TargetController)
                TargetController.GetComponent<Renderer>().enabled = true;

            if (TargetController)
                TargetController.GetComponent<Renderer>().sharedMaterial.color = TargetController.color;
        }

        //Preview texture for portals
        private RenderTexture _previewTex;

        private RenderTexture PreviewTex {
            get {
                if (_previewTex)
                    RenderTexture.ReleaseTemporary(_previewTex);

                _previewTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 24,
                    RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Default);

                return _previewTex;
            }
        }

        private PortalController _portalController;

        protected PortalController TargetController {
            get {
                if (!_portalController)
                    _portalController = (PortalController)target;
                return _portalController;
            }
        }

        private void CleanupTemp()
        {

            if (TargetController)
            {
                MeshRenderer renderer = TargetController.GetComponent<MeshRenderer>();
                if (renderer)
                    renderer.enabled = true;
            }

            TargetController.CleanupTemp();
        }

        protected override void DrawInstanceUI() {
            foreach (var o in targets)
            {
                var p = o as PortalController;
                if (p == null) return;
                GameObjectUtility.SetStaticEditorFlags(p.gameObject,
                    GameObjectUtility.GetStaticEditorFlags(p.gameObject) & ~StaticEditorFlags.LightmapStatic);
            }

            try
            {
                GUILayout.Label("Instance settings:", EditorStyles.boldLabel);
                foreach (PortalController p in targets)
                    Undo.RecordObject(p, "Portal Controller Editor Changes");

                EditorGUI.BeginChangeCheck();
                TargetController.TargetController = (PortalController)EditorGUILayout.ObjectField(
                    new GUIContent("Target Controller", "The targetTransform of this Portal."),
                    TargetController.TargetController, typeof(PortalController), true, null);
                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                        p.TargetController = TargetController.TargetController;

                //if (!PortalController.PortalScript.PortalCamera ||
                //    !PortalController.TargetController.PortalScript.PortalCamera) return;

                EditorGUI.BeginChangeCheck();
                TargetController.PortalPrefab =
                    (GameObject)EditorGUILayout.ObjectField(
                        new GUIContent("Portal Prefab", "The Prefab to use for when the Portal is spawned"),
                        TargetController.PortalPrefab, typeof(GameObject), false, null);
                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                        p.PortalPrefab = TargetController.PortalPrefab;


                if (SKSGlobalRenderSettings.ShouldOverrideMask)
                    EditorGUILayout.HelpBox("Your Global Portal Settings are currently overriding the mask",
                        MessageType.Warning);

                EditorGUI.BeginChangeCheck();
                TargetController.Mask = (Texture2D)EditorGUILayout.ObjectField(
                    new GUIContent("Portal Mask", "The transparency mask to use on the Portal"),
                    TargetController.Mask, typeof(Texture2D), false,
                    GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));

                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                    {
                        Undo.RecordObject(p.Mask, "Texture change");
                        p.Mask = TargetController.Mask;
                        EditorUtility.SetDirty(p.Mask);
                        EditorUtility.SetDirty(p);
                    }


                EditorGUI.BeginChangeCheck();
                Material material =
                    (Material)EditorGUILayout.ObjectField(
                        new GUIContent("Portal Material", "The material to use for the Portal"),
                        TargetController.PortalMaterial, typeof(Material), false, null);
                if (EditorGUI.EndChangeCheck())
                {
                    TargetMaterial = material;
                    foreach (PortalController p in targets)
                        p.PortalMaterial = TargetController.PortalMaterial;
                }


                EditorGUI.BeginChangeCheck();
                TargetController.Enterable =
                    EditorGUILayout.Toggle(
                        new GUIContent("Enterable", "Is the Portal Enterable by Teleportable Objects?"),
                        TargetController.Enterable);
                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                        p.Enterable = TargetController.Enterable;

                EditorGUI.BeginChangeCheck();
                TargetController.Is3D =
                    EditorGUILayout.Toggle(
                        new GUIContent("Portal is 3D Object", "Is the Portal a 3d object, such as a Crystal ball?"),
                        TargetController.Is3D);
                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                        p.Is3D = TargetController.Is3D;

                EditorGUI.BeginChangeCheck();
                TargetController.Rendering = EditorGUILayout.Toggle(
                    new GUIContent("Rendering Enabled", "Is the portal going to render?"),
                    TargetController.Rendering,
                    GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));

                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                        p.Rendering = TargetController.Rendering;

                EditorGUI.BeginChangeCheck();
                TargetController.DetectionScale = EditorGUILayout.Slider(
                    new GUIContent("Detection zone Scale", "The scale of the portal detection zone."),
                    TargetController.DetectionScale, 0.1f, 10f);
                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                        p.DetectionScale = TargetController.DetectionScale;

                if (TargetController.DetectionScale < 1)
                    EditorGUILayout.HelpBox(
                        "It is recommended that you do not set the detection scale" +
                        "below 1, as lower values have a very high chance of not" +
                        "detecting objects at all as they may move too quickly",
                        MessageType.Warning);


                //Show the Portal Material Inspector
                if (Application.isPlaying)
                    return;

            }
            catch
            {
                //Just for cleanliness
            }
            finally
            {
                if (!SKSGlobalRenderSettings.Preview)
                {
                    //CleanupTemp();
                    
                }
            }

            TargetMeshRenderer.gameObject.SetActive(false);

            //Cache state of random
            UnityEngine.Random.State seed = UnityEngine.Random.state;
            //Make color deterministic based on ID
            UnityEngine.Random.InitState(TargetController.GetInstanceID());
            TargetController.color = UnityEngine.Random.ColorHSV(0, 1, 0.48f, 0.48f, 0.81f, 0.81f);
            //Reset the random
            UnityEngine.Random.state = seed; 
        }

        protected override void DrawCustomGlobalUI() {
            GUILayout.Label("Global Portal Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel = EditorGUI.indentLevel + IndentSize;
#if !SKS_VR
            if (!GlobalPortalSettings.PlayerTeleportable) {
                EditorGUILayout.HelpBox(
                    "No PlayerTeleportable set. Seamless camera passthrough will not function." +
                    " Add a PlayerTeleportable script to your teleportable player object.",
                    MessageType.Warning);
            }
            else {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Player Teleportable");
                EditorGUILayout.ObjectField(GlobalPortalSettings.PlayerTeleportable.gameObject, typeof(object), true);
                GUILayout.EndHorizontal();
            }
#endif
            if (_interactionFoldout =
                EditorGUILayout.Foldout(_interactionFoldout, "Interaction Settings", EditorStyles.foldout)) {
                EditorGUI.indentLevel = EditorGUI.indentLevel + IndentSize;

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    SKSGlobalRenderSettings.PhysicsPassthrough = GUILayout.Toggle(
                        SKSGlobalRenderSettings.PhysicsPassthrough,
                        new GUIContent("Enable Physics Passthrough",
                            "Enable collision with objects on the other side of portals"));
                }
                GUILayout.EndHorizontal();


                if (SKSGlobalRenderSettings.PhysicsPassthrough)
                    EditorGUILayout.HelpBox(
                        "This setting enables interaction with objects on the other side of portals. " +
                        "Objects can pass through portals without it, and it is not needed for most games. " +
                        "In extreme cases, it can cause a slight performance hit.",
                        MessageType.Info);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    SKSGlobalRenderSettings.PhysStyleB = GUILayout.Toggle(SKSGlobalRenderSettings.PhysStyleB,
                        new GUIContent("Enable Physics Model B (More Accurate)",
                            "Physics Model B maintains relative momentum between portals." +
                            " This may or may not be desirable when the portals move."));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    SKSGlobalRenderSettings.NonScaledRenderers = GUILayout.Toggle(
                        SKSGlobalRenderSettings.NonScaledRenderers,
                        new GUIContent("Disable Portal scaling",
                            "Disable portal scaling. This should be enabled if " +
                            "portals are never used to change object's size."));
                }
                GUILayout.EndHorizontal();

                EditorGUI.indentLevel = EditorGUI.indentLevel - IndentSize;
            }

            if (_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, "Editor Settings", EditorStyles.foldout)) {
                EditorGUI.indentLevel = EditorGUI.indentLevel + IndentSize;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    SKSGlobalRenderSettings.Visualization = GUILayout.Toggle(SKSGlobalRenderSettings.Visualization,
                        new GUIContent("Visualize Portal Connections",
                            "Visualize all portal connections in the scene"));
                }
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel = EditorGUI.indentLevel - IndentSize;
            }

            if (_imageFoldout = EditorGUILayout.Foldout(_imageFoldout, "Image Settings", EditorStyles.foldout)) {
                EditorGUI.indentLevel = EditorGUI.indentLevel + IndentSize;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    SKSGlobalRenderSettings.Clipping = GUILayout.Toggle(SKSGlobalRenderSettings.Clipping,
                        new GUIContent("Enable perfect object clipping",
                            "Enable objects clipping as they enter portals. This is usually desirable."));
                }
                GUILayout.EndHorizontal();

                /*
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    SKSGlobalRenderSettings.ShouldOverrideMask = GUILayout.Toggle(
                        SKSGlobalRenderSettings.ShouldOverrideMask,
                        "Override Masks on all PortalSpawners");
                }
                GUILayout.EndHorizontal();

                if (SKSGlobalRenderSettings.ShouldOverrideMask) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    SKSGlobalRenderSettings.Mask =
                        (Texture2D) EditorGUILayout.ObjectField(SKSGlobalRenderSettings.Mask, typeof(Texture2D),
                            false);
                    GUILayout.EndHorizontal();
                }*/
                EditorGUI.indentLevel = EditorGUI.indentLevel - IndentSize;
            }
            EditorGUI.indentLevel = EditorGUI.indentLevel - IndentSize;
        }
    }
}