using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaypointManager), true)]
public class EditorWaypointManger : Editor
{
    WaypointManager waypoints;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(waypoints == null)
            waypoints = target as WaypointManager;

        if (GUILayout.Button("Auto Waypoints"))
        {
            waypoints.AutoSetWaypoints();
        }
    }
}
