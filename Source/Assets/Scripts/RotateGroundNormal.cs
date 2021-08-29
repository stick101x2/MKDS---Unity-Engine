using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateGroundNormal : MonoBehaviour
{
    public float rotateSpeed = 10f;
    public float returnSpeed = 10f;
    public float groundRotateSpeed = 10f;
    public float groundPullForce = -10f;
    public bool smooth = false;
    public LayerMask groundLayer;
    protected void GroundNormalRotation()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position + transform.up * 0.5f, -transform.up, out hit, 2f, groundLayer))
        {
            Debug.DrawRay(transform.position + transform.up * 0.5f, -transform.up * 2f, Color.green);
            /*
            
            Quaternion lerped = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up * 2, hit.normal) * transform.rotation, groundRotateSpeed * Time.deltaTime);
            transform.rotation = lerped;*/

           
            transform.up = Vector3.Lerp(transform.up, hit.normal, groundRotateSpeed * Time.deltaTime);

        }
        else
        {
            Debug.DrawRay(transform.position + transform.up * 0.5f, -transform.up * 2f, Color.red);
            Return();
        }
    }

    void Return()
    {
        /*
        if (v.isGrounded)
        {
            Vector3 localEularAngles = mainRot.localEulerAngles;
            localEularAngles += transform.eulerAngles;
            localEularAngles.x = 0f;
            localEularAngles.z = 0f;

            mainRot.localEulerAngles = localEularAngles;
        }*/
        transform.up = Vector3.Lerp(transform.up, Vector3.up, rotateSpeed * Time.deltaTime);

        Rot();
    }

    protected void Rot()
    {
        if (!smooth)
        {
            rotateSpeed = 0f;
        }
        else
        {
            rotateSpeed += Time.deltaTime * returnSpeed;
            if (rotateSpeed > 10f)
                rotateSpeed = 10f;
        }
    }
}
