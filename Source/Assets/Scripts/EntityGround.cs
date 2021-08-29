using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGround : RotateGroundNormal
{
    public Entity e;
    public void OnFixedUpdate()
    {
        if (e == null)
            e = GetComponent<Entity>();

        GroundNormalRotation();

        Rot();
    }
}
