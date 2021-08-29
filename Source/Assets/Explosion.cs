using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    DamagePlayer damage;
    public Sound3D hit;
    public Sound3D dist;
    public void Spawn()
    {
        AudioManager.instance.Play(hit, transform.position, hit);
        AudioManager.instance.Play(dist, transform.position, dist);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!damage)
            damage = GetComponent<DamagePlayer>();

        damage.Hit(other);
    }
}
