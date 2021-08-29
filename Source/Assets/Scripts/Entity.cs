using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Entity : MonoBehaviour
{
    private Vector3 velocity;
    private Rigidbody rid;

    public bool grounded;
    public float speed;
    public float maxSpeed;
    public Vector3 Velocity
    {
        get
        {
            
            return velocity;
        }
        set
        {

 //           Debug.Log("set " + value);
            velocity = value;
            rid.velocity = velocity;
        }
    }
    public float Vely
    {
        get
        {
           
            return velocity.y;
        }
        set
        {
            velocity.y = value;
            rid.velocity = velocity;
        }
    }
    public float Velz
    {
        get
        {
            
            return velocity.z;
        }
        set
        {
            velocity.z = value;
            rid.velocity = velocity;
        }
    }
    public float Velx
    {
        get
        {
           
            return velocity.x;
        }
        set
        {
            velocity.x = value;
            rid.velocity = velocity;
        }
    }
    public void AddForce(Vector3 force)
    {
        rid.AddForce(force);
        velocity = rid.velocity;
    }
    public void AddForce(Vector3 force, ForceMode mode)
    {
        rid.AddForce(force, mode);
        velocity = rid.velocity;
    }
    public Vector3 GetVelocity()
    {
        return velocity;
    }
    public Vector3 GetRealVelocity()
    {
        return rid.velocity;
    }
    public bool isMoving
    {
        get
        {
            if (isMovingY || isMovingXZ)
            {
                return true;
            }
            return false;
        }
    }
    public bool isMovingY
    {
        get
        {
            if (Mathf.Abs(velocity.y) > 0.1f)
            {
                return true;
            }
            return false;
        }
    }
    public bool isMovingXZ
    {
        get
        {
            if (Mathf.Abs(velocity.x) > 0.1f || Mathf.Abs(velocity.z) > 0.1f)
            {
                return true;
            }
            return false;
        }
    }
    public void SetVelocityWithoutY(Vector3 velocity)
    {
        float y = Velocity.y;
        velocity.y = y;
        Velocity = velocity;
    }

    protected void OnAwake()
    {
        rid = GetComponent<Rigidbody>();
    }
}
