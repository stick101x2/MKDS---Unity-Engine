using System.Collections.Generic;
using UnityEngine;

public abstract class Waypoint : MonoBehaviour
{
    public Waypoint next; 
    
    public int Id { get; private set; } = -1;
    private void Awake()
    {
        if (Id <= -1)
        {
            Id = transform.GetSiblingIndex();
        }
    }
    public void SetId(int id)
    {
        Id = id;
    }

    public void SetNextWaypoint(Waypoint next)
    {
        this.next = next;
    }
    public abstract void OnTriggerEnter(Collider other);
}
