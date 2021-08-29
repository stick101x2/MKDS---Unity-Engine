using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindTargetInRadius : MonoBehaviour
{
    public Transform findpoint;
    public Vector3 offset;
    public float radius = 3f;
    public LayerMask layer;

    Collider[] found = new Collider[1];
    private void OnDrawGizmos()
    {
        Vector3 center = findpoint.position + offset;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, radius);
    }

    public Player FindPlayer()
    {
       
        Vector3 center = findpoint.position + offset;
        found = new Collider[1];

        if (Physics.OverlapSphereNonAlloc(center,radius, found,layer) > 0)
        {
            if (found[0].CompareTag("Player"))
            {
                Player p = found[0].GetComponent<Player>();
                if(p != null)
                {
                    return p;
                }
            }
            
        }
        return null;
    }
}
