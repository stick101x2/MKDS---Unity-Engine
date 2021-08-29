using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum AiWaypointType
{
    None,
    StartDriftRight,
    StartDriftLeft,
    EndDrift
}
public class WaypointAi : Waypoint
{
    public float sizeL = -10f;
    public float sizeR = 10f;
    public AiWaypointType type;
    public override void OnTriggerEnter(Collider other)
    {
 //       Debug.Log("Collided");
        
        if (other.CompareTag("Player"))
        {

            PlayerAi ai = other.GetComponent<PlayerAi>();

            WaypointAi ai_next = next as WaypointAi;
            ai.SetWaypoint(ai_next);

            if (!ai.GetComponent<Player>().v.isAi)
                return;

            switch (type)
            {
                case AiWaypointType.None:
                    break;
                case AiWaypointType.StartDriftRight:
                    ai.StartDrift(true);
                    break;
                case AiWaypointType.StartDriftLeft:
                    ai.StartDrift(false);
                    break;
                case AiWaypointType.EndDrift:
                    ai.EndDrift();
                    break;
                default:
                    break;
            }
        }
    }
}
