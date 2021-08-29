using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiWaypointManager : WaypointManager
{
    static AiWaypointManager instance;

    //public 
    private void Awake()
    {
        if (!instance)
            instance = this;
        else
        {
            Destroy(this);
        }
    }

    public static List<Waypoint> GetWaypoints()
    {
        return instance.waypoints;
    }
    public static float GetOffset(int prevWaypoint, Vector3 currentPosition)
    {
        int n = prevWaypoint + 1;
        if (prevWaypoint + 1 >= instance.waypoints.Count)
            n = 0;
        Waypoint nextWaypoint = instance.waypoints[n];

        WaypointAi next = nextWaypoint as WaypointAi;

        Vector3 targetpos_relative = instance.waypoints[prevWaypoint].transform.InverseTransformPoint(currentPosition);
        targetpos_relative.z = Mathf.Clamp(targetpos_relative.z, next.sizeL, next.sizeR);
        return targetpos_relative.z;
    }
}
