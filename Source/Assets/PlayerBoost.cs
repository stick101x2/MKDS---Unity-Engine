using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBoost : MonoBehaviour, IPlayer
{
    Player p;
    float boostSpeed;
    float timer;
    public void Setup(Player p)
    {
        this.p = p;
        boostSpeed = p.maxSpeed;
        enabled = false;
    }
    void FixedUpdate()
    {
        Boosting();
        if(timer < 0)
        {
            EndBoost();
        }
    }
    void Boosting()
    {
        p.maxSpeed = boostSpeed;
        p.speed = p.maxSpeed;
        timer -= Time.deltaTime;
    }
    void EndBoost()
    {
        boostSpeed = p.v.lastMaxSpeed;
        p.maxSpeed = p.v.lastMaxSpeed;
        p.v.isBoosting = false;
        enabled = false;
    }
    public void Boost(float duration, float speed, Sound3D boostSound = null)
    {
        EndBoost();

        p.kart.particles.Flame(duration);
        boostSpeed = speed;
        timer = duration;
        if(boostSound != null)
        {
            Sound s = boostSound;
            if (!p.v.wasAi)
                s.canBeOverriten = false;
            AudioManager.instance.Play(boostSound, transform.position, s, transform);
        }

        p.v.isBoosting = true;

        enabled = true;
    }
}
