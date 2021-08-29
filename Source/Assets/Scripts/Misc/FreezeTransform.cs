using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeTransform : MonoBehaviour
{
    Quaternion rot;
    Vector3 pos;
    Vector3 scale;
    public bool local = true;
    public bool lockRotation;
    public bool lockPosition;
    public bool lockScale;
    // Start is called before the first frame update
    void Start()
    {
        if(local)
        {
            rot = transform.localRotation;
            pos = transform.localPosition;
            scale = transform.localScale;
            return;
        }

        rot = transform.rotation;
        pos = transform.position;
        scale = transform.localScale;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (local)
        {
            if(lockRotation)
            transform.localRotation = rot;
            if (lockPosition)
                transform.localPosition = pos;
            if (lockScale)
                transform.localScale  = scale;
            return;

        }
        if (lockRotation)
            transform.rotation = rot;
        if (lockPosition)
            transform.position = pos;
        if (lockScale)
            transform.localScale = scale;
    }
}
