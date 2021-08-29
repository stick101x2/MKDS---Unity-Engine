using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class BillBoard : MonoBehaviour
{
    public bool tree;
    [Space(5)]
    public bool fixedScreeenSize = false;
    public Vector3 size = new Vector3(1,1,1);
    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += PreCull;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= PreCull;
    }

    private void PreCull(ScriptableRenderContext context,Camera camera)
    {
        if(!tree)
        {
            transform.LookAt(camera.transform);
            return;
        }
        transform.rotation = camera.transform.rotation;

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0f);
        if(fixedScreeenSize)
        {
            float size = (camera.transform.position - transform.position).magnitude;
            transform.localScale = this.size * size;
        }
       
    }
}
