using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SKStudios.Common;
using SKStudios.Common.Utils;
using SKStudios.Portals;
using SKStudios.Portals.Editor;
using SKStudios.Rendering;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

/// <summary>
///     base Editor for <see cref="EffectRenderer"/> components
/// </summary>
public abstract class EffectRendererEditor : MeshFilterPreview
{
    protected readonly List<string> TabOptions = new List<string>();

    private int _tab = 0;
    private readonly AnimBool _globalSettingOpenAnim;
    private readonly AnimBool _instanceOpenAnim;

    private static bool _imageFoldout = false;
    private static bool _editorFoldout = false;

    [NonSerialized] private Editor _matEditor;

    protected abstract void DrawInstanceUI();
    protected virtual void DrawCustomGlobalUI() { }

    protected abstract Material TargetMaterial { get; set; }
    protected abstract GameObject Target { get;}
    protected abstract bool MirroredPreview { get; }
    protected abstract string AssetName { get; }

    protected static float BumperSize = 3;
    protected static int IndentSize = 1;
    protected EffectRendererEditor()
    {
        TabOptions.Add("Instance Settings");
        TabOptions.Add("Global Settings");
        _instanceOpenAnim = new AnimBool(true, Repaint);
        _globalSettingOpenAnim = new AnimBool(false, Repaint);      
    }

    public override void OnInspectorGUI()
    {
        EditorWindowWindowPreviewRenderer.enabled = false;
        _tab = GUILayout.Toolbar(_tab, TabOptions.ToArray());

        
        _instanceOpenAnim.target = _tab == 0;
        _globalSettingOpenAnim.target = !_instanceOpenAnim.target;
        if (EditorGUILayout.BeginFadeGroup(_instanceOpenAnim.faded))
        {
            DrawInstanceUI();
        }
        EditorGUILayout.EndFadeGroup();

        if (EditorGUILayout.BeginFadeGroup(_globalSettingOpenAnim.faded))
        {
            DrawGlobalUI();
        }
        EditorGUILayout.EndFadeGroup();

        if (!Application.isPlaying)
        {
            try
            {
                if (TargetMaterial)
                    if (_matEditor == null)
                        _matEditor = CreateEditor(TargetMaterial);


                _matEditor.DrawHeader();
                _matEditor.OnInspectorGUI();
                DestroyImmediate(_matEditor);
            }
            catch
            { }
        }
        

    }

    private Camera _editorWindowPreviewCamera;
    private Camera EditorWindowPreviewCamera {
        get {
            if (_editorWindowPreviewCamera == null)
            {
                if (!TargetMeshRenderer) return null;
                GameObject cameraObj = EditorUtility.CreateGameObjectWithHideFlags("Editor Preview Camera", 
                    HideFlags.HideAndDontSave | HideFlags.NotEditable);
                cameraObj.transform.SetParent(TargetMeshRenderer.transform);
                _editorWindowPreviewCamera = cameraObj.AddComponent<Camera>();
                _editorWindowPreviewCamera.enabled = false;
                _editorWindowPreviewCamera.cullingMask = 0;
                Skybox previewBox = EditorWindowPreviewCamera.gameObject.AddComponent<Skybox>();
                previewBox.material = Resources.Load<Material>("UI/Materials/PreviewSkybox/PreviewSkybox");
            }
            return _editorWindowPreviewCamera;
        }
    }

    private MeshRenderer _editorWindowPreviewRenderer;
    private MeshRenderer EditorWindowWindowPreviewRenderer {
        get {
            if (!_editorWindowPreviewRenderer)
            {
                GameObject editorPreviewObj = EditorUtility.CreateGameObjectWithHideFlags("Preview Object", 
                    HideFlags.HideAndDontSave | HideFlags.NotEditable);
                editorPreviewObj.tag = Keywords.Tags.SKEditorTemp;
                if(Target.activeInHierarchy)
                    editorPreviewObj.transform.SetParent(Target.transform);
                editorPreviewObj.transform.localPosition = Vector3.zero;

                _editorWindowPreviewRenderer = editorPreviewObj.AddComponent<MeshRenderer>();

                _editorWindowPreviewRenderer.sharedMaterials = new Material[2];
                Material m = new Material(Shader.Find("Standard"));
                Material[] materialArray = new Material[3];
                materialArray[0] = TargetMaterial;
                materialArray[1] = Resources.Load<Material>("UI/Materials/Background");
                materialArray[2] = Resources.Load<Material>("UI/Materials/Backdrop");
                _editorWindowPreviewRenderer.sharedMaterials = materialArray;
            }
            return _editorWindowPreviewRenderer;
        }
    }

    private MeshFilter _editorPreviewFilter;

    private MeshFilter EditorPreviewFilter {
        get {
            if (!_editorPreviewFilter)
            {
                _editorPreviewFilter = EditorWindowWindowPreviewRenderer.gameObject.AddComponent<MeshFilter>();
                _editorPreviewFilter.mesh = Resources.Load<Mesh>("UI/RendererPreview");
            }
            return _editorPreviewFilter;
        }
    }


    public override MeshFilter TargetMeshFilter {
        get {
            return EditorPreviewFilter;

        }
        set { }
    }
    public override MeshRenderer TargetMeshRenderer {
        get {
            return EditorWindowWindowPreviewRenderer;
        }
        set { }
    }


    public override bool HasPreviewGUI()
    {
        return base.HasPreviewGUI_s();
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if (Application.isPlaying) return;
        
            TargetMeshRenderer.gameObject.SetActive(true);
        TargetMeshRenderer.gameObject.transform.localPosition = Vector3.zero;

        //Update the preview cam and the cam to render with
        Transform previewTransform = base.MoveCamera();
        if (previewTransform == null) return;
            EditorWindowPreviewCamera.transform.rotation = previewTransform.rotation;
            EditorWindowPreviewCamera.transform.position = previewTransform.position;
        if (MirroredPreview) {
            EditorWindowPreviewCamera.transform.rotation =
                MirrorTransformUtils.MirrorRotate(EditorWindowPreviewCamera.transform.rotation);

            MirrorTransformUtils.MirrorMoveTransform(EditorWindowPreviewCamera.transform);
        }

        EditorWindowWindowPreviewRenderer.enabled = true;
        RenderTexture rt = null;
        try {
            if (r.width > 0 && (int) r.height > 0) {
                rt = RenderTexture.GetTemporary((int)r.width, (int)r.height, 16);
                EditorWindowPreviewCamera.targetTexture = rt;
                TargetMaterial.SetTexture(Keywords.ShaderKeys.LeftEye, rt);
                TargetMaterial.SetVector(Keywords.ShaderKeys.LeftPos, new Vector4(0, 0, 1, 1));
                TargetMaterial.SetTexture(Keywords.ShaderKeys.RightEye, rt);
                TargetMaterial.SetVector(Keywords.ShaderKeys.RightPos, new Vector4(0, 0, 1, 1));
                EditorWindowPreviewCamera.Render();
                //EditorWindowPreviewCamera.Render();
                base.OnPreviewGUI_s(r, background);
            }       
        }
        catch {
            //Unity silliness again
        }
        TargetMeshRenderer.gameObject.SetActive(false);
        if(rt != null)
            RenderTexture.ReleaseTemporary(rt);
        EditorWindowWindowPreviewRenderer.enabled = false;
    }

    private PreviewRenderUtility _previewRenderUtility;
    //protected abstract MonoBehaviour TargetController { get; }
    private void DrawGlobalUI() {
        DrawCustomGlobalUI();

        GUILayout.Label("SKS Global settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel = EditorGUI.indentLevel + IndentSize;

        GUILayout.Space(BumperSize);
        if (_imageFoldout = EditorGUILayout.Foldout(_imageFoldout, "Image Settings", EditorStyles.foldout))
        {
            EditorGUI.indentLevel = EditorGUI.indentLevel + IndentSize;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUI.indentLevel * 10);
#if SKS_VR
                GUILayout.Label("Single Pass Stereo Rendering: " + SKSGlobalRenderSettings.SinglePassStereo);
#endif
            }
            GUILayout.EndHorizontal();


            GUI.enabled = !Application.isPlaying;


#if SKS_VR
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
                GUILayout.Label("Recursion in VR is very expensive. 3 is the typically acceptable max (prefer 0 if possible)");
                GUILayout.EndHorizontal();
#endif

            SKSGlobalRenderSettings.RecursionNumber = EditorGUILayout.IntSlider(
                new GUIContent("Recursion Number",
                    "The number of times that EffectRenderers will draw through each other."),
                SKSGlobalRenderSettings.RecursionNumber, 0, 10);


            if (SKSGlobalRenderSettings.RecursionNumber > 1)
                EditorGUILayout.HelpBox(
                    "Please be aware that recursion can get very expensive very quickly." +
                    " Consider making this scale with the Quality setting of your game.",
                    MessageType.Warning);



            GUI.enabled = true;


            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.AggressiveRecursionOptimization = GUILayout.Toggle(
                    SKSGlobalRenderSettings.AggressiveRecursionOptimization,
                    new GUIContent("Enable Aggressive Optimization for Recursion",
                        "Aggressive optimization will halt recursive rendering immediately if the " +
                        "source EffectRenderer cannot raycast to the EffectRenderers it is trying to render. " +
                        "Without Occlusion Culling (due to lack of Unity Support), this is a lifesaver for " +
                        "large scenes."));
            }
            GUILayout.EndHorizontal();

            if (SKSGlobalRenderSettings.AggressiveRecursionOptimization)
                EditorGUILayout.HelpBox(
                    "Enabling this option can save some serious performance, " +
                    "but it is possible for visual bugs to arise due to portals being partially inside walls. " +
                    "If you are seeing black EffectRenderers while recursing, try turning this option off " +
                    "and see if it helps. If it does, then please make sure that your EffectRenderers are not" +
                    "inside walls.",
                    MessageType.Warning);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.AdaptiveQuality = GUILayout.Toggle(SKSGlobalRenderSettings.AdaptiveQuality,
                    new GUIContent("Enable Adaptive Quality Optimization for Recursion",
                        "Adaptive quality rapidly degrades the quality of recursively " +
                        "rendered EffectRenderers. This is usually desirable."));
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.CustomSkybox = GUILayout.Toggle(SKSGlobalRenderSettings.CustomSkybox,
                    new GUIContent("Enable Skybox Override",
                        "Enable custom skybox rendering. This is needed for skyboxes to not look strange through" +
                        "SKSEffectCameras on some platforms when optimizations are enabled."));
            }
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel = EditorGUI.indentLevel - IndentSize;
        }

        GUILayout.Space(BumperSize);
        if (_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, "Editor Settings", EditorStyles.foldout))
        {
            EditorGUI.indentLevel = EditorGUI.indentLevel + IndentSize;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.Gizmos = GUILayout.Toggle(SKSGlobalRenderSettings.Gizmos,
                    new GUIContent("Draw Gizmos",
                        "Draw SKS Gizmos when applicable assets are selected in the Editor"));
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.Preview = GUILayout.Toggle(SKSGlobalRenderSettings.Preview,
                    new GUIContent("Draw EffectRenderer Previews (experimental, buggy on many Unity versions)",
                        "Draw EffectRenderer Previews when selected in the Editor." +
                        " Experimental.")); 
            }
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel = EditorGUI.indentLevel - IndentSize;
        }

        GUILayout.Label("Something doesn't look right!/I'm getting errors!");

        SKSGlobalRenderSettings.UvFlip = GUILayout.Toggle(SKSGlobalRenderSettings.UvFlip,
            "My stuff is rendering upside down!");

        GUILayout.Label("Troubleshooting:");

        string path = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
        if (path == null)
            return;
        path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
        string studioName = "SKStudios";
        string root = path.Substring(0, path.LastIndexOf(studioName) + (studioName.Length + 1));
        string PDFPath = Path.Combine(root, AssetName);
        PDFPath = Path.Combine(PDFPath, "README.pdf");
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button(AssetName + " Manual"))
            {
                Application.OpenURL(PDFPath);
            }
            if (GUILayout.Button("Setup"))
            {
                SettingsWindow.Show();
            }
        }
        GUILayout.EndHorizontal();
        EditorGUI.indentLevel = EditorGUI.indentLevel - IndentSize;
    }

    
}
