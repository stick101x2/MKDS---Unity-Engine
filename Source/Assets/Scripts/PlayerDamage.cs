using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamage : MonoBehaviour, IPlayer
{
    Player p;
    public Transform all;
    public Transform axis;
    public Vector3 dir = new Vector3(1,0,1);
    Type currentDamge;
    Vector3 intirot;
    float angle;
    float rot;
    //spinout
    public int rotations;
    public float returnSpeed = 5f;
    [Space(5)]
    public float speedX = 1f;
    public float speedY = 1f;
    public float speedZ = 1f;
    [Space(5)]
    public float decelerate = 0f;
    [Space(5)]
    public float lowBounce = 2f;
    public float bounceMod = 0.75f;
    public int maxBounces = 3;
    float timer;

    bool canBounce;
    int didRotations;
    int bounces;

    Vector3 c_rot;
    Vector3 t_rot;
    public enum Type
    {
        None,
        SpinOut, // banana
        Trip, // fake itembox
        Hard, // shell
        Launch, // bomb
        Ink
    }
    public void Setup(Player p)
    {
        this.p = p;
        p.gnormal.OnLand -= Bounce;
        p.gnormal.OnLand += Bounce;
    }
    private void OnEnable()
    {
        if(p != null)
        {
            p.gnormal.OnLand -= Bounce;
            p.gnormal.OnLand += Bounce;
        }
    }
    private void OnDisable()
    {
        p.gnormal.OnLand -= Bounce;
    }
    //

    //
    void StartBounce(int bounceAmount, float mod = 0.5f)
    {
        bounces = 0;
        bounceMod = mod;
        maxBounces = bounceAmount;
        canBounce = true;
    }
    void Bounce()
    {
        if (!canBounce)
            return;
        p.Vely = Mathf.Abs(p.gnormal.lastY) * bounceMod;
        bounces++;
        if (bounces > maxBounces)
        {
            bounces = 0;
            maxBounces = 0;
            canBounce = false;
        }else if (bounces == maxBounces)
        {
            p.Vely = lowBounce;
        }
    }
    // --

    //
    public void OnFixedUpdate()
    {
        if (bounces >= maxBounces && currentDamge != Type.SpinOut)
        {
            timer += Time.deltaTime * returnSpeed;
            if (timer > 1f)
                timer = 1f;

            all.localEulerAngles = Vector3.Lerp(c_rot, t_rot, timer);

        }
        if (!p.v.isHurt)
        {
            p.ai.SetOffset(false);
            all.localEulerAngles = Vector3.zero;
            p.anim.SetDriverAnimState(0);
            p.SetState(Player.State.Defualt);
           
        }

        switch (currentDamge)
        {
            case Type.SpinOut:
                SpinOut();
                break;
            case Type.Trip:
                Spin();
                break;
            case Type.Hard:
                Spin();
                break;
            case Type.Launch:
                Launched();
                break;
            default:
                break;
        }

    }
    //

    //inti
    public void Inti(Type t)
    {
        if (p.drift)
            p.drift.CancelDrift();
        p.SetState(Player.State.Damage);
        currentDamge = t;

        intirot = all.forward;
        p.anim.SetDriverAnimState(3);
        p.v.isHurt = true;
        p.driver.saudio.Play(p.driver.Damage);
    }
    Vector3 eul;
    float spi;
    //during
    public void SpinOut()
    {
        if(didRotations >= rotations)
        {
            if (p.v.isHurt)
            {
                p.v.isHurt = false;
            }
            return;
        }

        eul = all.localEulerAngles;
        eul.y += speedX;
        spi += Mathf.Abs(speedX);
        if(spi >= 359.5f)
        {
            didRotations++;
            eul.y = 0f;
            spi = 0f;
        }
        all.localEulerAngles = eul;
        Decelerate();
    }
    public void Launched()
    {
        if (!canBounce)
        {
            if (p.v.isHurt)
            {
                c_rot = all.localEulerAngles;
                t_rot = Vector3.zero;
                timer = 0;
                p.v.isHurt = false;
            }
            return;
        }

        rot -= Time.deltaTime;
        if (rot < 0.5f)
            rot = 0.5f;

        all.Rotate(all.parent.right, speedX * rot, Space.World);
        all.Rotate(all.up, speedY * rot, Space.World);
    }
    public void Spin()
    {
        //   all.Rotate(all.right, speed, Space.World) ;
        if(!canBounce)
        {
            if(p.v.isHurt)
            {
                c_rot = all.localEulerAngles;
                t_rot = Vector3.zero;
                timer = 0;
                p.v.isHurt = false;
            }
            return;
        }

        
        axis.rotation = Quaternion.LookRotation(dir);
        all.Rotate(axis.right, speedX, Space.World);
    }
    //

    //misc
    public void Decelerate()
    {
        if (decelerate <= 0f)
            return;
        p.move.Accelerate(ref p.speed, 0, decelerate);
    }
    //

    //damage
    public void DamageSpinOut(float rotateSpeed, int Rotations, float slowForce)
    {
        if (p.v.isImmuneToDamage)
            return;

        speedX = rotateSpeed;
        didRotations = 0;
        rotations = Rotations;
        decelerate = slowForce;
        all.localEulerAngles = Vector3.zero;
        spi = 0f;

        Inti(Type.SpinOut);
    }
    public void DamageTrip(float rotateSpeed, float launchUpForce)
    {
        if (p.v.isImmuneToDamage)
            return;

        p.speed *= 0.25f;
        p.Velocity = p.v.mainRotator.GetChild(0).forward * p.speed;
        p.Vely = launchUpForce * 3.5f;
        lowBounce = 10f;
        p.grounded = false;
        StartBounce(3, 0.5f);
        speedX = rotateSpeed;

        all.localEulerAngles = Vector3.zero;
        dir = p.Velocity.normalized;

        Inti(Type.Trip);
    }

    public void DamageHard(float rotateSpeed, Vector3 launchForce)
    {
        if (p.v.isImmuneToDamage)
            return;

        p.speed = 0f;
        p.Velocity = launchForce * 3.5f;
        p.grounded = false;
        lowBounce = 10f;
        StartBounce(3, 0.5f);
        speedX = rotateSpeed;

        all.localEulerAngles = Vector3.zero;
        dir = p.Velocity.normalized;

        Inti(Type.Hard);
    }

    public void DamageLaunch(Vector2 rotateSpeed, float launchHighet = 100f)
    {
        if (p.v.isImmuneToDamage)
            return;

        p.speed = 0f;
        p.Velocity = new Vector3(0, launchHighet,0);
        rot = 2f;
        p.grounded = false;
        lowBounce = 15f;
        StartBounce(3, 0.5f);

        speedY = rotateSpeed.y;
        speedX = rotateSpeed.x;
        all.localEulerAngles = Vector3.zero;

        Inti(Type.Launch);
    }
}
