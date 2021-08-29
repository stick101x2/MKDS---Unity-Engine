using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityWaypointer : MonoBehaviour
{
    public WaypointAi currentWaypoint;
    public WaypointAi lastWaypoint;
    public void SetWaypoint(WaypointAi next)
    {
        lastWaypoint = currentWaypoint;
        currentWaypoint = next;
    }
    public void FaceTargetDirect(Transform facer, Vector3 target, float rotateSpeed)
    {
        //steer
        Vector3 myangle = target - transform.position;
        Quaternion final = Quaternion.LookRotation(myangle);

        facer.rotation = Quaternion.RotateTowards(facer.rotation, final, rotateSpeed);
        facer.localEulerAngles = facer.localEulerAngles;
    }
    public void FaceTarget(Transform facer, Vector3 target, float rotateSpeed)
    {
        //steer
        Vector3 myangle = target - transform.position;
        Quaternion final = Quaternion.LookRotation(myangle);

        facer.rotation = Quaternion.RotateTowards(facer.rotation, final, rotateSpeed);
        float y = facer.localEulerAngles.y;
        facer.localEulerAngles = new Vector3(0f, y, 0f);
    }
}
