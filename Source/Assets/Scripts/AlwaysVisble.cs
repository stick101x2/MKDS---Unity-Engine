using UnityEngine.Rendering;
using UnityEngine;

[ExecuteInEditMode]
public class AlwaysVisble : MonoBehaviour
{
    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += PreCull;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= PreCull;
    }

    private void PreCull(ScriptableRenderContext context, Camera camera)
    {
        if (camera.name == "SceneCamera")
            return;

        camera.cullingMatrix = Matrix4x4.Ortho(-99999, 99999,-99999 ,99999, 0.00f, 99999)
        *Matrix4x4.Translate(Vector3.forward * -99999 / 2f) * camera.worldToCameraMatrix;

        transform.position = camera.transform.position;
    }
}
