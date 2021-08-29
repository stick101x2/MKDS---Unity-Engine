using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeItemBox : ItemBox
{
    public FakeItemBoxEntity e;
    public Vector3 launch;
    public string destroyEfx;
    public void OnAwake()
    {
        isFake = true;

        e.Velocity = launch * 3.5f;
    }
    private void FixedUpdate()
    {
        e.grav.Gravity();


        e.grounded = e.col.GroundCheck() && e.Vely < -0.1f;
        
        if(e.grounded)
        {
            e.Velx = 0f;
            e.Velz = 0f;
            e.Vely = -1f;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (e.item.ownerIsInnume)
        {
            if (e.item.owner != null)
            {
                if (e.item.owner.gameObject == other.gameObject)
                    return;
            }
        }
        int hitPlayer = e.damage.Hit(other);
        if (hitPlayer > -1)
        {
            if (e.item.owner != null && hitPlayer > 0)
            {
                if (e.item.owner.gameObject != other.gameObject)
                {
                    e.item.OnHitTarget();
                }
            }
            Destroyed();
            return;
        }
    }
    public void Destroyed()
    {
        if (destroyEfx != null || destroyEfx != "")
        {
            ObjectPooler.instance.SpawnPoolObject(destroyEfx, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
    public void Spawn(float speed, Vector3 direction,float height)
    {
        Vector3 velocity = direction * speed;
        velocity.y = height;
        e.Velocity = velocity;
    }
}
