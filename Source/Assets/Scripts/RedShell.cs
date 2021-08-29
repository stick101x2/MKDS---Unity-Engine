using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedShell : GreenShell
{
    public float rotateSpeed = 2f;
    public float speedUp = 1f;
    bool isHoming = true;
    EntityWaypointer way;
    Player target;
    FindTargetInRadius find;
    // Start is called before the first frame update
    protected override void Awake()
    {
        if(destroyEfx == null || destroyEfx == "")
        {
            destroyEfx = Constants.R_SHELL_HIT;
        }

        base.Awake();

        find = GetComponent<FindTargetInRadius>();
        bounce = GetComponent<EntityWallBounce>();
        way = GetComponent<EntityWaypointer>();
    }
    public void OnDrift()
    {
        Vector3 targetP = target.transform.position;
        Vector3 currentP = transform.position;

        targetP.y = 0;
        currentP.y = 0;

        float dis = Vector3.Distance(targetP, currentP);

        if (dis > 6f)
            return;

        isHoming = false;
    }
    private void OnDisable()
    {
        if(target != null)
        {
            target.drift.OnDrift -= OnDrift;
        }
    }
    // Update is called once per frame
    protected override void FixedUpdate()
    {
        if (isHoming)
            Homing();

        base.FixedUpdate();
    }
    public void Homing()
    {
        Vector3 targetPos = Vector3.zero;
        if (!target)
        {

            target = find.FindPlayer();
            if (target)
            {
                target.drift.OnDrift -= OnDrift;
                target.drift.OnDrift += OnDrift;

                rotateSpeed *= 2f;
            }
        }
        if (target != null)
        {
            maxSpeed = target.maxSpeed + 5f;
            speed = Mathf.Lerp(speed, target.maxSpeed + 5f, Time.deltaTime * speedUp);
            targetPos = target.transform.position;
        }
        else
        {
            targetPos = way.currentWaypoint.transform.position;
        }

        way.FaceTarget(foward, targetPos, rotateSpeed);
    }
    protected override void OnCollisionEnter(Collision collision)
    {
        ShellOnCollisionEnter(collision);

        if (collision.gameObject.CompareTag("Wall"))
        {
            Destroyed();
        }

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

    public void SpawnShell(float speed, Vector3 direction, WaypointAi next)
    {
        base.SpawnShell(speed, direction);
        way.SetWaypoint(next);
    }
}
