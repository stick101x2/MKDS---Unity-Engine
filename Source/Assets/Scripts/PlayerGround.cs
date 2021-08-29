using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PlayerGround : RotateGroundNormal, IPlayer
{
    Player p;
    bool landed;
    public event Action OnLand;
    public float lastY;
    public void Setup(Player p)
    {
        this.p = p;
    }
    public void OnFixedUpdate()
    {
        lastY = p.Velocity.y;
        p.grounded = p.col.GroundCheck() && !p.v.jumped&& p.Vely < 0.1f;

        GroundNormalRotation();

        if (p.grounded)
        {
            if (!landed)
            {
                p.Vely = -5f;
                OnLand?.Invoke();
                landed = true;
            }

        }
        else
        {
            landed = false;
        }

        Rot();
    }
}
