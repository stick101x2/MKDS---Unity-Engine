using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapPoint : Waypoint
{
    public int keyIndex = -1;
    public LapPoint last;
    public override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {


           
            PlayerLap p = other.GetComponent<Player>().lap;

            if(Id != 0)
            {
                if (p.region < Id - 2 || p.region > Id + 2)
                    return;
            }else
            {
                if(p.HasAllKeys())
                {
                    p.GetLap();
                }
            }
           

            p.SetWaypoint(next as LapPoint, keyIndex);

        }
    }

  
}
