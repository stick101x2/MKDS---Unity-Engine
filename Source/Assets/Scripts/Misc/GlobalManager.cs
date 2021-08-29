using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        ItemBox.all_quesiton_position = ItemBox.GetItemBoxPosition();
        ItemBox.all_rotation = ItemBox.GetItemBoxRotation();
    }

    void Start()
    {
        PlayerAi.waypoints = AiWaypointManager.GetWaypoints();
    }

   
}
