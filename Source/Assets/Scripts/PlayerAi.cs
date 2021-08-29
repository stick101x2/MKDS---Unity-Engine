using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAi : MonoBehaviour, IPlayer
{
    Player p;
    public Color lineColor = Color.green;
    public static List<Waypoint> waypoints;
    public Transform target;

    public WaypointAi currentWaypoint;
    public WaypointAi lastWaypoint;

    public float rotateSpeed = 5f;
    public float fakeInputX = 0f;
    Vector3 myangle;
    Vector3 cross;
    float dir;

    float offset = 0;
    bool start;
    bool isDrifting;

 //   bool inputEnabled = true;
    public void Setup(Player p)
    {
        this.p = p;

        if (!p.v.isAi)
        {

            enabled = false;
        }
    }
    public void Start()
    {
        

        if (!start)
        {
            SetOffset(false);
            start = true;

            if (!p.v.isAi)
                return;

            p.drift.driftTilt *= 2f;
        }
        // input.accelHeld = true;
    }

   

   

    public void OnFixedUpdate()
    {
        TargetWaypoint();
        Steer();
      //  SetOffset();
    }

    public void SetWaypoint(WaypointAi next)
    {
        lastWaypoint = currentWaypoint;
        currentWaypoint = next;
        offset = Mathf.Clamp(offset, next.sizeL, next.sizeR);
        if (next.Id == 0)
            SetOffset();
    }

    public void SetOffset(bool vary = true)
    {
        offset = AiWaypointManager.GetOffset(lastWaypoint.Id, transform.position);

        if (!vary)
            return;
        WaypointAi next = lastWaypoint.next as WaypointAi;
        offset += Random.Range(-1f, 1f);
        offset = Mathf.Clamp(offset, next.sizeL, next.sizeR);
        // offset *= 3.5f;
    }
    public void TargetWaypoint()
    {
        Vector3 targetPos = transform.position;

        Transform c_transform = currentWaypoint.transform;


        Vector3 trueTarget = c_transform.TransformPoint(new Vector3(0, 0, offset));

     //   debug.position = trueTarget;

        targetPos = trueTarget;
        targetPos.y = transform.position.y;

        target.position = targetPos;
    }

    public void StartDrift(bool Right)
    {
        if (isDrifting)
            return;
        float dir = Right ? 1f : -1f;
        p.input.x = dir;
        p.steer.steerDirection = dir;
        p.input.driftDown = true;
        p.input.driftHeld = true;
        isDrifting = true;
    }
    public void EndDrift()
    {
        p.input.x = 0f;
        p.steer.steerDirection = 0f;
        p.input.driftHeld = false;
        isDrifting = false;
    }
    public void Steer()
    {
        //model
        Vector3 old = p.v.mainRotator.eulerAngles;

        if (p.drift.isDrifting)
        {
            p.v.pivot.localRotation = Quaternion.Lerp(p.v.pivot.localRotation, Quaternion.Euler(0, p.drift.driftTilt * p.drift.driftDirection, 0), p.v.modelturnSpeed * Time.fixedDeltaTime);

            p.AddForce(p.v.mainRotator.right * -p.drift.outerwardDriftForce * p.drift.driftDirection, ForceMode.Acceleration);
        }
        else
        {
            p.v.pivot.localRotation = Quaternion.Lerp(p.v.pivot.localRotation, Quaternion.Euler(0, 0, 0), p.v.modelturnSpeed * Time.fixedDeltaTime);
        }

        //steer
        myangle = target.position - transform.position;
        //  angle = Vector3.Cross(p.var.mainRot.forward, myangle);
        //  dir = Vector3.Dot(angle, transform.up);    
        //  p.var.mainRot.Rotate(Vector3.up, (dir * f_rotSpeed) * Time.fixedDeltaTime, Space.Self);
        Quaternion final = Quaternion.LookRotation(myangle);
        //p.var.mainRot.LookAt(target);
        p.v.mainRotator.rotation = Quaternion.RotateTowards(p.v.mainRotator.rotation, final, rotateSpeed);
        float y = p.v.mainRotator.localEulerAngles.y;
        p.v.mainRotator.localEulerAngles = new Vector3(0f, y, 0f);

        Vector3 now = p.v.mainRotator.eulerAngles;

        cross = old - now;

        if(cross.y > 0.05f)
        {
            fakeInputX = -1f;
        }
        else if (cross.y < -0.05f)
        {
            fakeInputX = 1f;
        }else
        {
            fakeInputX = 0f;
            if (isDrifting)
            {
                if(p.drift.driftDirection < 0)
                {
                    fakeInputX = 1f;
                }else
                {
                    fakeInputX = -1f;
                }
            }
        }
      
    }
    public void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        if (waypoints.Count == 0)
            return;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null)
            {
                Dev.LogError("Waypoint list has null at element " + i);
                break;
            }

            Vector3 current = waypoints[i].transform.TransformPoint(new Vector3(0, 0, offset));

            int nextIndex = i + 1;
            if (nextIndex >= waypoints.Count)
                nextIndex = 0;

            Vector3 next = waypoints[nextIndex].transform.TransformPoint(new Vector3(0, 0, offset));

            if (waypoints[nextIndex] == null)
            {
                Dev.LogError("Waypoint list has null at element " + nextIndex);
                break;
            }

            Gizmos.color = lineColor;
            Gizmos.DrawLine(current, next);
        }
    }
}
