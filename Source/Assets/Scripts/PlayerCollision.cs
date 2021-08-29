using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour, IPlayer
{
    
    Player p;
    EntityCollision col;
    DamagePlayer damage;
    public float angle;
    public float angleF;
    public float minAngle = 30f;

    public float returnFoward = 10f;
    public float bumpPower = 1f;
    public float bumpStrenght = 25f;
    public float bumpDur = 0.25f;
    public float bumpTim = 0.25f;

    public float spe;
    public float fSpeed;
    public float oFSpeed;

    public float oSpe;
    public float oSpeed;
    public float oNSpeed;
    public void Setup(Player p)
    {
        this.p = p;
        p.v.foward = p.v.mainRotator.GetChild(0);
        damage = GetComponent<DamagePlayer>();
        bumpStrenght = p.stats.weight;

        col = GetComponent<EntityCollision>();
    }
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            p.v.atWall = false;
        }
    }
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            if (Wall(other))
            {
                if (!(p.input.accelHeld || p.input.dccelHeld))
                    return;

                float s = p.maxSpeed * angleF;
                if (Mathf.Abs( p.speed) < 10f)
                    return;
                p.speed = s;
               
            }
        }

        if (other.gameObject.CompareTag("Player"))
        {
            Bump(other);
        }
    }
    public bool Wall(Collision other)
    {
        p.v.atWall = false;
        ContactPoint c = col.WallCheck(other);
        Vector3 n = c.normal;
        n.y = 0f;
        p.move.wallNormal = n;
        if (n != Vector3.zero)
        {
            Vector3 cur = transform.position;
            Vector3 nor = c.point;

            nor.y = 0f;
            cur.y = 0f;

            angle = Vector3.Angle(p.v.mainRotator.forward, (nor - cur).normalized);
            angleF = Func.Remap(angle, 0f, 180f, 0f, 1f);

            p.v.atWall = true;
            
        }

        return p.v.atWall;
    }
    private void OnCollisionEnter(Collision other)
    {
       
        if (other.gameObject.CompareTag("Wall"))
        { 
            if( Wall(other))
            {
                
                float s = p.speed * angleF;
                p.speed = s;
            }
            
        }

        if (other.gameObject.CompareTag("Player"))
        {
            StarmanHit(other);
        }
    }
    private void FixedUpdate()
    {
        
        if (p.v.bumped)
        {
            bumpTim -= Time.fixedDeltaTime;
            if(bumpTim < 0f)
            {
                p.v.bumped = false;
                bumpTim = bumpDur;
            }
        }
        
    }
  
    public void Bump(Collision other)
    {
        if (p.v.bumped)
            return;
        if (p.v.isImmuneToDamage)
            return;

        Player o = other.gameObject.GetComponent<Player>();
        PlayerCollision pcol = other.gameObject.GetComponent<PlayerCollision>();
        if (o.v.isImmuneToDamage)
            return;

        if (o.v.bumped)
            return;

        if (o.speed > p.speed)
            return;

        if (o.stats.weight > p.stats.weight)
            return;
        

        Vector3 dir = Vector3.zero;
        dir = p.v.mainRotator.InverseTransformPoint(o.transform.position);

        Vector3 dir2 = Vector3.zero;
        dir2 = o.v.mainRotator.InverseTransformPoint(p.transform.position);

        spe = Mathf.Abs(p.speed);
        fSpeed = Func.Remap(spe, 0f, p.maxSpeed, 0f, 1f);
        fSpeed = Mathf.Clamp(fSpeed,0f, 1f);

        oSpe = Mathf.Abs(o.speed);
        oSpeed = Func.Remap(oSpe, 0f, o.maxSpeed, 0f, 1f);
        oSpeed = Mathf.Clamp(oSpeed, 0f, 1f);

       


    

        oFSpeed = fSpeed * bumpStrenght * bumpPower;
        pcol.bumpTim = 0.1f;
        o.v.bumped = true;




        oNSpeed = oSpeed * bumpStrenght * bumpPower;
        bumpTim = 0.1f;
        p.v.bumped = true;



        if (dir.x > 0f)
            o.v.allMove.forward = o.v.mainRotator.right;
        else
            o.v.allMove.forward = -o.v.mainRotator.right;

        o.move.sideVelocity = oFSpeed;

        if (dir2.x > 0f)
            p.v.allMove.forward = p.v.mainRotator.right;
        else
            p.v.allMove.forward = -p.v.mainRotator.right;

        p.move.sideVelocity = oNSpeed;
    }

    public void StarmanHit(Collision other)
    {
        if (p.v.isImmuneToDamage)
        {
            Player o = other.gameObject.GetComponent<Player>();

            if (o.v.isImmuneToDamage)
                return;
            p.driver.saudio.Play(p.driver.Good);
            damage.Hit(other.collider);
        }
    }
}
