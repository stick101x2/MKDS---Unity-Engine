using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartParticles : MonoBehaviour
{

    public ParticleGroup dustInPlace;
    public ParticleGroup smokeIdle;
    public ParticleGroup smokeDrive;
    public ParticleGroup smokeBurst;
    [Space(5)]
    public ParticleGroup driftR;
    public ParticleGroup driftRBurst;
    public ParticleSystem BwheelDustR;
    [Space(5)]
    public ParticleGroup driftL;
    public ParticleGroup driftLBurst;
    public ParticleSystem BwheelDustL;
    [Space(5)]
    public ParticleGroup flameBurst;
    public ParticleGroup flame;
    [Space(5)]
    public GameObject starman;
    [Space(5)]
    public Color driftOrange = Color.black;
    public Color driftBlue = Color.black;
    public Color driftPurple = Color.black;

    public void Flame(float duration = 1.50f)
    {
        flameBurst.Stop();
        flame.Stop();

        flame.SetDuration(duration);

        flameBurst.Play();
        flame.Play();
    }

    public void Smoke(ref bool accelD, bool accelH, bool deccelH)
    {
        if (accelD)
        {
            smokeBurst.Play();

            accelD = false;
        }
        if (accelH || deccelH)
        {
            if (smokeIdle.IsPlaying())
               smokeIdle.Stop();
        }
        else
        {
            if (!smokeIdle.IsPlaying())
                smokeIdle.Play();
        }

        if (accelH && deccelH)
        {
            if (!dustInPlace.IsPlaying())
                dustInPlace.Play();
        }
        else
        {
            if (dustInPlace.IsPlaying())
                dustInPlace.Stop();
        }
    }


}
