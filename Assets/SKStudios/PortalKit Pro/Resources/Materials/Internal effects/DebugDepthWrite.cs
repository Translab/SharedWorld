using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class DebugDepthWrite : MonoBehaviour {
    private CommandBuffer cmdBuffer;

    public Material depthMat;
    public Mesh mesh;

    private void OnEnable() {
        var cam = GetComponent<Camera>();

        cmdBuffer = new CommandBuffer();
        cmdBuffer.name = "Depth Write";

        cmdBuffer.DrawMesh(mesh, transform.worldToLocalMatrix, depthMat);
        //cmdBuffer.Blit(BuiltinRenderTextureType.CurrentActive, BuiltinRenderTextureType.CurrentActive, depthMat);

        cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cmdBuffer);
    }

    private void OnDisable() {
        var cam = GetComponent<Camera>();

        cam.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, cmdBuffer);
        cmdBuffer = null;
    }
}