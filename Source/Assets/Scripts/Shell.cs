using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : Entity
{
    public EntityGravity grav { get; protected set; }
    public EntityCollision col { get; protected set; }
    public EntityGround normal { get; protected set; }
    public DamagePlayer damage { get; protected set; }
    public ItemObject item { get; protected set; }
    public float animRotateSpeed = 10f;
    protected Transform pivot;
    protected Transform foward;
    public string destroyEfx;
    public Sound3D itemBreak;
    // Start is called before the first frame update
    protected virtual void Awake()
    {
        base.OnAwake();

        foward = transform.GetChild(0);
        pivot = transform.GetChild(1);

        grav = GetComponent<EntityGravity>();
        col = GetComponent<EntityCollision>();
        normal = GetComponent<EntityGround>();
        damage = GetComponent<DamagePlayer>();
        item = GetComponent<ItemObject>();
    }

    // Update is called once per frame
    protected virtual void FixedUpdate()
    {
        grav.Gravity();
        grounded = col.GroundCheck() && Vely < 0.1f;
        normal.OnFixedUpdate();
        Move();
        Anim();
    }
    protected virtual void Anim()
    {
        Vector3 eul = pivot.eulerAngles;
        eul.y += animRotateSpeed;
        eul.x = 0;
        eul.z = 0;
        pivot.eulerAngles = eul;
    }
    protected virtual void Move()
    {
        Vector3 fowardVector = foward.forward;

        Vector3 velocity = fowardVector * speed;

        if (grounded)
        {
            velocity += -transform.up * normal.groundPullForce;

            Velocity = velocity;
        }
        else
            SetVelocityWithoutY(velocity);
    }
    public void ShellOnCollisionEnter(Collision collision)
    {
        if (item.ownerIsInnume)
        {
            if(item.owner != null)
            {
                if (item.owner.gameObject == collision.gameObject)
                    return;
            }
        }
        int hitPlayer = damage.Hit(collision.collider);
        if (hitPlayer > -1)
        {
            if (item.owner != null && hitPlayer > 0)
            {
                if (item.owner.gameObject != collision.gameObject)
                {
                    item.OnHitTarget();
                }
            }
            
            Destroyed();
            return;
        }
    }
    protected virtual void OnCollisionEnter(Collision collision)
    {
        ShellOnCollisionEnter(collision);
    }

    public void Destroyed()
    {
        if (destroyEfx != null || destroyEfx != "")
        {
            ObjectPooler.instance.SpawnPoolObject(destroyEfx, transform.position, Quaternion.identity);
        }
        AudioManager.instance.Play(itemBreak,transform.position, itemBreak, null);
        Destroy(gameObject);
    }

    public virtual void SpawnShell(float speed, Vector3 direction)
    {
        this.speed = speed;
        maxSpeed = speed;

        foward.forward = direction;
    }
}
