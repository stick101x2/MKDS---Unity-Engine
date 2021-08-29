using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityWallBounce : MonoBehaviour
{
    public Vector3 Bounce(Vector3 foward, Vector3 normal)
    {
        foward = Vector3.Reflect(foward, normal);
        return foward;
    }
}
