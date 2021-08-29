using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public Color lineColor;
    public float offset;
    public List<Waypoint> waypoints;



    public void OnDrawGizmos()
    {
        if (waypoints.Count <= 0)
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

    public void AutoSetWaypoints()
    {
        Waypoint[] got = GetComponentsInChildren<Waypoint>();

        for (int i = 0; i < got.Length; i++)
        {
            Waypoint next = null;
            if (i + 1 >= got.Length)
                next = got[0];
            else
                next = got[i + 1];
            got[i].SetNextWaypoint(next);

            if(got[i] is LapPoint)
            {
                LapPoint current = got[i] as LapPoint;

                LapPoint last = null;
                if (i - 1 < 0)
                    last = got[got.Length - 1] as LapPoint;
                else
                    last = got[i - 1] as LapPoint;

                current.last = last;
            }
        }

        waypoints.AddRange(got);
    }
}
