using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class LapPointManager : WaypointManager
{
    static LapPointManager instance;
    List<LapPoint> lapPoints;

    public static int keys = 4;
    public static int laps = 4;
    //public 
    private void Awake()
    {
        if (!instance)
            instance = this;
        else
        {
            Destroy(this);
            return;
        }
        lapPoints = new List<LapPoint>(waypoints.Count);
        for (int i = 0; i < instance.waypoints.Count; i++)
        { 
            
            instance.lapPoints.Add(instance.waypoints[i] as LapPoint);
        }
    }
    public static List<LapPoint> GetWaypoints()
    {
        return instance.lapPoints;
    }
}
