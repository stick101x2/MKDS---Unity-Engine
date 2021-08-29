using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class EntityGravity : MonoBehaviour
{
    private Entity entity;
    public float gravity = 5f;
    public float maxFallSpeed = 20f;
    public Transform gravityDirector;
    public Transform gravityTarget;
    // Start is called before the first frame update
    void Start()
    {
        entity = GetComponent<Entity>();
    }

    public void Gravity()
    {
        bool direction = gravityTarget != null;
        if(direction)
        {
            Directional();
        }
        else
        {
            Downwards();
        }
    }

    public void Directional()
    {
        gravityDirector.LookAt(gravityTarget);

        if (!entity.grounded)
        {
            entity.Velocity -= gravityDirector.forward * gravity;
        }
    }

    public void Downwards()
    {
        if (!entity.grounded)
        {
            entity.Vely -= gravity;
        }

        if (entity.Vely < -maxFallSpeed)
        {
            entity.Vely = -maxFallSpeed;
        }
    }

}
