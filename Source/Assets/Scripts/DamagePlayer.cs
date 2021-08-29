using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    public bool scriptOnly;
    public string hitEfxTag = Constants.P_IMPACT;
    public PlayerDamage.Type damage;
    public Sound3D hitSound;
    public int rotatations = 1;
    public bool launchDirectionIsPosition = true;
    public Vector3 launchDirection = new Vector3(1,15,15f);
    public float decelerate = 2f;
    public float launchHeight = 100f;
   
    public void OnTriggerEnter(Collider other)
    {
        if (scriptOnly)
            return;
        Hit(other);
    }

    public int Hit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 dir = launchDirection;

            Player p = other.GetComponent<Player>();

            if (p.v.isImmuneToDamage)
                return 0;

            if (hitSound.mainClip != null)
            {
                AudioManager.instance.Play(hitSound, p.transform.position, hitSound, null);
            }

            if (launchDirectionIsPosition)
            {
                dir = (other.transform.position - transform.position).normalized;
                dir.y = 1f;
                dir.Scale(launchDirection);
            }

            if (hitEfxTag != null || hitEfxTag != "")
            {
                ObjectPooler.instance.SpawnPoolObject(hitEfxTag, other.transform.position, Quaternion.identity);
            }

            if (damage == PlayerDamage.Type.SpinOut)
            {
                p.dam.DamageSpinOut(10f, rotatations, decelerate);
                return 1;
            }
            else if (damage == PlayerDamage.Type.Hard)
            {
                p.dam.DamageHard(10f, dir);
                return 2;
            }
            else if (damage == PlayerDamage.Type.Launch)
            {
                p.dam.DamageLaunch(new Vector2(-10f, 2.5f), launchHeight);
                return 3;
            }
            else if (damage == PlayerDamage.Type.Trip)
            {
                p.dam.DamageTrip(10f, 10f);
                return 4;
            }
        }
        return -1;
    }
}
