using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour, IPlayer
{
    Vector3 fowardVector;
    Player p;
    public float smoothToReturn = 1f;
    public float smoothToDrift = 1f;
    public float smoothToAlt = 4f;

    public float sideVelocity;
    public float sideDeccelModifier = 4f;
    public Vector3 wallNormal;
    Vector3 reflecteDir;
    public void Setup(Player p)
    {
        this.p = p;

    }
   
    // Update is called once per frame
    public void OnFixedUpdate()
    {
        Movement();
    }
    
    void Movement()
    {
        float bumped = p.v.bumped ? 1f : 1f;
        float traction = 1f;
        float isDrift = p.drift.isDrifting ? 0f : 1f;
        float isAi = p.v.isAi ? 0f : 1f ;
        float handling = 0.5f;
        float steer = Mathf.Abs(p.steer.steerC);
        sideVelocity = Mathf.Lerp(sideVelocity, 0f, Time.deltaTime * sideDeccelModifier);
        //  Smoke();
        //   float isDrift = !v.isDrifting == true ? 1 : 0;
        if (p.input.accelHeld && !p.input.dccelHeld&&!p.v.bumped)
        {
            Accelerate(ref p.speed, p.maxSpeed - ((steer * handling) * isDrift)* isAi, 1f * traction);
        }
        else if (p.input.dccelHeld && !p.input.accelHeld && !p.v.bumped)
        {
            if (p.grounded)
                Accelerate(ref p.speed, -(p.maxSpeed * 0.675f - (p.steer.steerC * handling) * isDrift), 2f * traction);
        }
        else if(p.grounded)
        {
            if (p.input.accelHeld && p.input.dccelHeld)
                Accelerate(ref p.speed, 0, 4f);
            else
                Accelerate(ref p.speed, 0, 1.5f * traction * bumped);
        }
    }
    public void SetFoward()
    {
        if (!p.v.isHurt)
        {
            if (p.drift.isDrifting)
            {
                if (p.drift.driftDirection > 0)
                    p.v.mainRotator.GetChild(0).forward = Vector3.Lerp(p.v.mainRotator.GetChild(0).forward, p.kart.drift_r.forward, Time.fixedDeltaTime * smoothToDrift);
                else
                    p.v.mainRotator.GetChild(0).forward = Vector3.Lerp(p.v.mainRotator.GetChild(0).forward, p.kart.drift_l.forward, Time.fixedDeltaTime * smoothToDrift);
            }
            else
                p.v.mainRotator.GetChild(0).forward = Vector3.Lerp(p.v.mainRotator.GetChild(0).forward, p.v.mainRotator.forward, Time.fixedDeltaTime * smoothToReturn);
        }
    }
    public void Accelerate(ref float current_speed, float targetSpeed, float modifier = 1f, bool foward = true)
    {
        p.v.realSpeed = p.v.mainRotator.InverseTransformDirection(p.GetRealVelocity()).z;

        reflecteDir = Vector3.zero;
        float acceleration = foward ? p.v.acceleration : 1f;
        current_speed = Mathf.Lerp(current_speed, targetSpeed, acceleration * (modifier * Time.fixedDeltaTime));

        SetFoward();


        Vector3 velocity = fowardVector * p.speed;

        fowardVector = p.v.mainRotator.GetChild(0).forward;

        if (p.v.isAi)
            fowardVector = p.v.mainRotator.forward;
        velocity = fowardVector * p.speed;

        if (p.grounded)
        {
            velocity += -transform.up * p.gnormal.groundPullForce;
            velocity += p.v.allMove.forward * sideVelocity;

            p.Velocity = velocity;

        }
        else
        {
            velocity += p.v.allMove.forward * sideVelocity;

            p.SetVelocityWithoutY(velocity);
        }
    }
}
