using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockTransformTo : MonoBehaviour
{
    public Transform target;

    public bool lockRotation;
    public bool lockPosition;

    // Update is called once per frame
    void LateUpdate()
    {
        if (lockRotation)
        {
            Quaternion rot = target.rotation;
            transform.rotation = target.rotation;
        }
        if (lockPosition)
        {
            Vector3 pos = target.position;
            transform.position = target.position;
        }
    }
}
