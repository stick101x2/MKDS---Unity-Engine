using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCollision : MonoBehaviour
{
    Entity entity;

    [SerializeField]
    private Bounds col;
    [SerializeField]
    private LayerMask groundLayers;
    [SerializeField]
    private float dis = 0.1f;

    Vector3 center;
    public RaycastHit[] hits = new RaycastHit[1];
    public RaycastHit[] hitsC = new RaycastHit[1];
    Vector3 upperRight;
    Vector3 upperLeft;
    Vector3 lowerRight;
    Vector3 lowerLeft;
   
    public bool GroundCheck()
    {
        center = transform.position + transform.up * 0.1f;

        upperRight =   new Vector3(col.size.x * 0.5f, col.size.y * 0.5f, col.size.z * 0.5f) + transform.position + transform.up * 0.1f;
        upperLeft = new Vector3(-col.size.x * 0.5f, col.size.y * 0.5f, col.size.z * 0.5f) + transform.position + transform.up * 0.1f;
        lowerRight = new Vector3(col.size.x * 0.5f, col.size.y * 0.5f, -col.size.z * 0.5f) + transform.position + transform.up * 0.1f;
        lowerLeft = new Vector3(-col.size.x * 0.5f, col.size.y * 0.5f, -col.size.z * 0.5f) + transform.position + transform.up * 0.1f;
        //   RaycastHit hit;
        //  RaycastHit[] hits = new RaycastHit[1];

        float distance = dis;
        Vector3 dir = -transform.up;

       

        if (Physics.RaycastNonAlloc(center , dir, hitsC, distance, groundLayers) > 0 ||
            Physics.RaycastNonAlloc(upperRight , dir, hits, distance, groundLayers) > 0 ||
            Physics.RaycastNonAlloc(upperLeft , dir, hits, distance, groundLayers) > 0 ||
            Physics.RaycastNonAlloc(lowerRight , dir, hits, distance, groundLayers) > 0 ||
            Physics.RaycastNonAlloc(lowerLeft, dir, hits, distance, groundLayers) > 0)
        {
            Debug.DrawRay(center, dir * distance, Color.green);
            Debug.DrawRay(upperRight, dir * distance, Color.green);
            Debug.DrawRay(upperLeft, dir * distance, Color.green);
            Debug.DrawRay(lowerRight, dir * distance, Color.green);
            Debug.DrawRay(lowerLeft, dir * distance, Color.green);
            return true;
        }
        else
        {
            Debug.DrawRay(center, dir * distance, Color.red);
            Debug.DrawRay(upperRight, dir * distance, Color.red);
            Debug.DrawRay(upperLeft, dir * distance, Color.red);
            Debug.DrawRay(lowerRight, dir * distance, Color.red);
            Debug.DrawRay(lowerLeft, dir * distance, Color.red);
            return false;
        }
    }
    ContactPoint[] wPoints = new ContactPoint[4];
    public ContactPoint WallCheck(Collision collision)
    {
        ContactPoint contant = new ContactPoint();

        int pointsAmount = collision.GetContacts(wPoints);
        for (int i = 0; i < pointsAmount; i++)
        {
            if (Vector3.Angle(wPoints[i].normal, Vector3.up) > 80f)
            {
                contant = wPoints[i];
                break;
            }
        }

        return contant;
    }
}
