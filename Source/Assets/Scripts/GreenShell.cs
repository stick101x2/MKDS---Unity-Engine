using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenShell : Shell
{
    public int maxBounces = 5;
    int bounces;
    public EntityWallBounce bounce { get; protected set; }
    ContactPoint[] hit = new ContactPoint[1];
    // Start is called before the first frame update
    protected override void Awake()
    {
        if (destroyEfx == null || destroyEfx == "")
        {
            destroyEfx = Constants.G_SHELL_HIT;
        }

        base.Awake();
        bounce = GetComponent<EntityWallBounce>();
    }

    // Update is called once per frame
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (grounded)
        {
            Vector3 eul = foward.localPosition;
            eul.x = 0f;
            foward.localPosition = eul;
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if(collision.gameObject.CompareTag("Wall"))
        {
            bounces++;
            if(bounces >= maxBounces)
            {
                
                Destroyed();
                return;
            }

            collision.GetContacts(hit);

            foward.forward = bounce.Bounce(foward.forward, hit[0].normal);
        }

    }
}
