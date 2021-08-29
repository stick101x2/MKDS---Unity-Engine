using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PlayerDrift : MonoBehaviour, IPlayer
{
    Player p;
    public float returnFoward = 3f;
    public float driftTilt = 20;
    public float driftModifierBOARD = 1;
    public float driftModifierHARD = 1;
    public float driftTapDelay = 3f;
    public float outerwardDriftForce;
    public Sound3D boost;
    public Sound3D powerSlide;
    public Sound3D powerSlideGood;
    [Header("Ignore")]
    public bool isDrifting;
    public float driftDirection;
    public int driftCount;
    public float driftTimer;
    public float drift;

    public event Action OnDrift;
    public void Setup(Player p)
    {
        this.p = p;
    }
    public void OnFixedUpdate()
    {
        float traction = 1f;

        if (p.input.driftDown)
        {
            if (p.grounded)
                Hop();
           p.input.driftDown = false;
        }
        if (isDrifting)
        {
            if (!p.input.driftHeld || p.speed < (p.maxSpeed * 0.4f) * traction)
                EndDrift();
        }
        

        if (p.input.driftHeld && p.grounded && p.speed > (p.maxSpeed * 0.4f) * traction && isDrifting)
        {
            if (!p.v.isAi)
                DriftAction();
            else
                DriftActionAi();

            if (!p.kart.particles.BwheelDustL.isPlaying)
            {
                p.kart.particles.BwheelDustL.Play();
                p.kart.particles.BwheelDustR.Play();

                if (driftCount >= 4)
                {
                    p.kart.particles.driftL.Play();
                    p.kart.particles.driftR.Play();
                }
            }



        }
        if (!p.grounded)
        {
            DisableDriftEffects();
        }
    }
    void DriftActionAi()
    {
        driftTimer += Time.deltaTime;
        if (driftTimer > UnityEngine.Random.Range(0.15f, 0.5f) + (driftCount * 0.1f))
        {
            driftTimer = 0;
            driftCount++;
            if (driftCount == 2)
                Drifted(1);
            if (driftCount == 4)
                Drifted(2);
            if (driftCount == 6)
                Drifted(3);
        }
    }
    void DriftAction()
    {
        float lastDrift = drift;
        float s = Func.Remap(Mathf.Abs(p.steer.steerDirection), 0.5f, 1.5f, -1f, 1f) * driftDirection;
        if (s != 0)
            drift = s;
        if (driftTimer >= 1f)
        {
            if (lastDrift > 0 && drift < 0 || lastDrift < 0 && drift > 0)
            {
                driftTimer = 0;
                driftCount++;
                if (driftCount == 2)
                    Drifted(1);
                if (driftCount == 4)
                    Drifted(2);
                if (driftCount == 6)
                    Drifted(3);
            }
        }

        if (Mathf.Abs(drift) > 0)
        {
            driftTimer += Time.fixedDeltaTime * driftTapDelay;
            if (driftTimer > 1f)
                driftTimer = 1f;
        }
        else
            driftTimer = 0;
    }
    void StartDrift()
    {
        if (Mathf.Abs(p.steer.steerDirection) < 0.5f)
            return;
        if (!p.input.driftHeld)
            return;
        //do hop
        p.move.smoothToReturn = returnFoward;
        isDrifting = true;
     
        driftDirection = Mathf.Sign(p.steer.steerDirection);
        driftCount = 0;
        driftTimer = 0;
    }
    void Drifted(int power)
    {
        p.kart.particles.driftL.Stop();
        p.kart.particles.driftL.Clear();
        p.kart.particles.driftR.Stop();
        p.kart.particles.driftR.Clear();
        switch (power)
        {
            case 1:
                p.kart.particles.driftLBurst.SetColor(p.kart.particles.driftOrange);
                p.kart.particles.driftRBurst.SetColor(p.kart.particles.driftOrange);
                if (!p.v.wasAi)
                {
                    AudioManager.instance.Stop(powerSlide);
                    AudioManager.instance.Play(powerSlide, transform.position, powerSlide, transform);
                }
                break;
            case 2:
                p.kart.particles.driftLBurst.SetColor(p.kart.particles.driftBlue);
                p.kart.particles.driftRBurst.SetColor(p.kart.particles.driftBlue);
                p.kart.particles.driftL.SetColor(p.kart.particles.driftBlue);
                p.kart.particles.driftR.SetColor(p.kart.particles.driftBlue);
                p.kart.particles.driftL.Play();
                p.kart.particles.driftR.Play();
                if (!p.v.wasAi)
                {
                    AudioManager.instance.Stop(powerSlideGood);
                    AudioManager.instance.Play(powerSlideGood, transform.position, powerSlideGood, transform);
                }
                
                break;
            default:
                p.kart.particles.driftLBurst.SetColor(p.kart.particles.driftPurple);
                p.kart.particles.driftRBurst.SetColor(p.kart.particles.driftPurple);
                p.kart.particles.driftL.SetColor(p.kart.particles.driftPurple);
                p.kart.particles.driftR.SetColor(p.kart.particles.driftPurple);
                p.kart.particles.driftL.Play();
                p.kart.particles.driftR.Play();
                if(!p.v.wasAi)
                {
                    AudioManager.instance.Stop(powerSlideGood);
                    Sound s1 = powerSlideGood;
                    s1.pitch = 1.25f;
                    AudioManager.instance.Play(powerSlideGood, transform.position, s1, transform);
                }
                
                
                break;
        }
        p.kart.particles.driftLBurst.Play();
        p.kart.particles.driftRBurst.Play();
        //  Dev.Log("Drifted");
    }
    void DisableDriftEffects()
    {
        p.kart.particles.BwheelDustL.Stop();
        p.kart.particles.BwheelDustR.Stop();
        p.kart.particles.driftL.Stop();
        p.kart.particles.driftR.Stop();
    }
    void EndDrift()
    {
        OnDrift?.Invoke();
        if (driftCount >= 2)
        {
            if (driftCount >= 4 && driftCount < 6)
                p.boost.Boost(0.1f,110f, boost);
            if (driftCount >= 6)
                p.boost.Boost(0.25f, 110f, boost);
        }
        //  p.ai.EndDrift();
        //  move.Timer(false);
        CancelDrift();
    }
    public void CancelDrift()
    {

        AudioManager.instance.Stop(powerSlideGood);
        DisableDriftEffects();
        driftDirection = 0;
        isDrifting = false;
    }
    void Hop()
    {
        Sound3D s = p.audo.Hop;
        s.pitch = UnityEngine.Random.Range(0.95f, 1.05f);

        AudioManager.instance.Play(p.audo.Hop, transform.position, s, transform);
        p.anim.KartJump();
        p.v.jumped = true;
    }
    public void JumpEnd()
    {
        p.v.jumped = false;
        StartDrift();
    }
}
