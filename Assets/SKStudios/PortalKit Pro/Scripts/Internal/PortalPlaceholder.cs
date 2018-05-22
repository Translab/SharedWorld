using SKStudios.Common.Utils;
using SKStudios.Portals;
using SKStudios.ProtectedLibs.Rendering;
using UnityEngine;

/// <summary>
///     Placeholder for Portals. Currently unused.
/// </summary>
public class PortalPlaceholder : MonoBehaviour {
    private Camera _camera;
    private SKSRenderLib _cameraLib;

    private Material _material;
    private bool _placeholderRendered;
    private Portal _portal;
    private RenderTexture placeholderTex;

    public void Instantiate(Portal portal) {
        _portal = portal;
        _cameraLib = _portal.EffectCamera.GetComponent<SKSRenderLib>();

        var cameraParent = new GameObject();

        cameraParent.transform.parent = _portal.EffectCamera.RenderingCameraParent;
        cameraParent.transform.localPosition = Vector3.zero;
        cameraParent.transform.localRotation = Quaternion.identity;

        _camera = cameraParent.AddComponent<Camera>();
        _camera.cullingMask &= ~(1 << Keywords.Layers.CustomRenderer);
        _camera.cullingMask &= ~(1 << Keywords.Layers.CustomRendererPlaceholder);
        _camera.name = "Portal Placeholder Camera";
        _camera.enabled = false;

        placeholderTex =
            new RenderTexture(100, 100, 24, RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Default);
    }

    private void Awake() {
        _material = gameObject.GetComponent<Renderer>().sharedMaterial;
    }

    private void Update() {
        _placeholderRendered = false;
    }
}