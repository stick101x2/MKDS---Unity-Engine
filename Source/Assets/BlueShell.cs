using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueShell : Entity
{
    public Sound3D sound;
    public float rotateSpeed = 2f;
    public float speedUp = 1f;
    public float nTurnSpeed = 5f;
    public float ySpeed = 2f;
    public float minDistanceToTarget = 4f;
    public float lockOnSpeed = 10f;
    public Transform foward;
    public Transform pivot;
    public ItemObject item { get; protected set; }
    public DamagePlayer damage { get; protected set; }
    public EntityCollision col { get; protected set; }
    public Collider colldier;
    public Animator animMain;
    public Animator animSub;
    Rigidbody rig;
    public EntityWaypointer way { get; private set; }
    Player target;
    FindTargetInRadius find;
    bool atTarget;
    float lockSpeed = 1f;
    private void Awake()
    {
        OnAwake();

        foward = transform.GetChild(0);
        pivot = transform.GetChild(1);

        rig = GetComponent<Rigidbody>();
        col = GetComponent<EntityCollision>();
        find = GetComponent<FindTargetInRadius>();
        way = GetComponent<EntityWaypointer>();
        damage = GetComponent<DamagePlayer>();
        item = GetComponent<ItemObject>();
    }
    private void Start()
    {
        AudioManager.instance.Play(sound, transform.position, sound, transform);
    }
    private void OnDestroy()
    {
        AudioManager.instance.Stop(sound);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if(atTarget)
        {
            Target();
            return;
        }
        Homing();
        grounded = col.GroundCheck() && Vely < 0.1f;
        Move();
        Anim();
    }
    public void Anim()
    {
        pivot.forward = foward.forward;
    }
    public void Target()
    {
        LockOnTarget(target);
    }
    float dis;
    public void OnDrift()
    {
        Vector3 targetP = target.transform.position;
        Vector3 currentP = transform.position;

        targetP.y = 0;
        currentP.y = 0;

        dis = Vector3.Distance(targetP, currentP);

        if (dis > 6f)
            return;

    }
    public void Explode()
    {
        Explosion ex = ObjectPooler.instance.SpawnPoolObject("blueExplosion", target.transform.position, Quaternion.identity).GetComponent<Explosion>();
        ex.Spawn();
        Destroy(gameObject);
    }
    public void CanDodge()
    {
        target.drift.OnDrift -= OnDrift;
        target.drift.OnDrift += OnDrift;
    }
    protected virtual void Move()
    {
        Vector3 fowardVector = foward.forward;

        Vector3 velocity = fowardVector * speed;
        velocity.y *= ySpeed;

        Velocity = velocity;
    }
    public void Homing()
    {
        Vector3 targetPos = Vector3.zero;
        if (!target)
        {
            Player p = find.FindPlayer();
            target =  p == GameManager.instance.players[0] ? p : null ;
            if (target)
            {


                timer = 0f;
                rotateSpeed = nTurnSpeed;
            }
        }
        if (target != null)
        {
            Vector3 targetP = target.transform.position;
            Vector3 currentP = transform.position;

            // targetP.y = 0;
            // currentP.y = 0;

            dis = Vector3.Distance(targetP, currentP);

            if (dis < minDistanceToTarget)
            {
                colldier.enabled = false;
                rig.isKinematic = true;
                atTarget = true;
                animMain.SetBool("attack",true);
                animSub.SetBool("attack", true);
                return;
            }

        //    maxSpeed = target.maxSpeed + 5f;
          //  speed = Mathf.Lerp(speed, target.maxSpeed + 100f, Time.deltaTime * speedUp);
            targetPos = target.transform.position + (Vector3.one * 1f);
       //     foward.LookAt(targetPos);
        }
        else
        {
            targetPos = way.currentWaypoint.transform.position;
            if (col.hitsC[0].collider != null)
                targetPos.y = (col.hitsC[0].point + (Vector3.one * 1f)).y;
            else
                targetPos.y = transform.position.y;
        }

        way.FaceTargetDirect(foward, targetPos, rotateSpeed);
    }
    float timer = 0f;
    public void LockOnTarget(Player p)
    {
        if(timer < 1f)
        {
            timer += Time.deltaTime * lockOnSpeed;
            if(timer > 1f)
            {
                transform.position = p.transform.position;
                transform.parent = p.v.mainRotator;
                transform.localPosition = Vector3.zero;
               // transform.localEulerAngles = Vector3.zero;
               // pivot.localEulerAngles = new Vector3(0f, 0f, 0f);

                
            }
        }
        if (timer > 1f)
        {
            return;
        }
        transform.position = Vector3.Lerp(transform.position, p.transform.position, timer);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AiWaypoint"))
        {
            WaypointAi current = other.GetComponent<WaypointAi>();
            WaypointAi next = current.next as WaypointAi;
            way.SetWaypoint(next);
        }

    }
    public void ShellOnCollisionEnter(Collision collision)
    {
        if (item.ownerIsInnume)
        {
            if (item.owner != null)
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
            return;
        }
    }
    protected virtual void OnCollisionEnter(Collision collision)
    {
        ShellOnCollisionEnter(collision);
    }

    public void SpawnShell(Vector3 direction, WaypointAi next)
    {
        foward.forward = direction;
        way.SetWaypoint(next);
    }
}
