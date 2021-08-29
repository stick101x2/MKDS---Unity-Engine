using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSteer : MonoBehaviour, IPlayer
{
    Vector3 fowardVector;
    Player p;

    public float steerDirection;

    public float steerC; // CURRENT
    public float steerMod = 10;
    public float steerTMod = 1;
    float steerAmount = 0;
    float turnSpeedModifier;
    public void Setup(Player p)
    {
        this.p = p;
    }
    public void OnFixedUpdate()
    {
        Steer();
    }
    void Steer()
    {
        steerDirection = p.input.x;

        float traction = 1f;
        
        if (p.drift.isDrifting)
        {
            if (p.drift.driftDirection < 0)
            {
                steerDirection = p.input.x < 0 ? -1.5f : -0.5f;

                if (p.input.x < 0)
                {
                    steerDirection *= p.drift.driftModifierHARD;
                }
                else if (p.input.x > 0)
                {
                    steerDirection /= p.drift.driftModifierBOARD;
                }

                if (Mathf.Abs(p.input.x) < 0.1f)
                    steerDirection = -1;
            }
            else
            {
               

                steerDirection = p.input.x > 0 ? 1.5f : 0.5f;

                if(p.input.x > 0)
                {
                    steerDirection *= p.drift.driftModifierHARD;
                }
                else if (p.input.x < 0)
                {
                    steerDirection /= p.drift.driftModifierBOARD;
                }
                if (Mathf.Abs(p.input.x) < 0.1f)
                    steerDirection = 1;
            }

            p.v.pivot.localRotation = Quaternion.Lerp(p.v.pivot.localRotation, Quaternion.Euler(0, p.drift.driftTilt * p.drift.driftDirection, 0), p.v.modelturnSpeed * Time.fixedDeltaTime);

            p.AddForce(p.v.mainRotator.right * -p.drift.outerwardDriftForce * p.drift.driftDirection, ForceMode.Acceleration);
        }
        else
        {
            p.v.pivot.localRotation = Quaternion.Lerp(p.v.pivot.localRotation, Quaternion.Euler(0, 0, 0), p.v.modelturnSpeed * Time.fixedDeltaTime);
        }

        

        turnSpeedModifier = Mathf.Abs(p.v.realSpeed) / p.maxSpeed * steerTMod + 1f; // highspeed slower turns

        steerAmount = 
            p.input.accelHeld && p.input.dccelHeld && Mathf.Abs(p.speed) < 1f ? 
            50f * steerDirection :
            p.v.realSpeed / turnSpeedModifier * steerDirection;
        steerAmount *= traction;

        // turn slower in air
        if (p.grounded)
            steerC = Mathf.Lerp(steerC, steerAmount, Time.fixedDeltaTime * steerMod);
        else
        {
            steerC = Mathf.Lerp(steerC, steerAmount, Time.fixedDeltaTime * 0.333f);
        }

        p.v.mainRotator.Rotate(Vector3.up, (steerC * 4f) * Time.fixedDeltaTime, Space.Self);
    }
}
