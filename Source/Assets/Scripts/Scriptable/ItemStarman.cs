using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "MKDS/Items/Starman")]
public class ItemStarman : Item
{   [Space(10)]
    public Material starMat;
    public Sound music;
    public Sound3D musicAi;

    public float timer = 10f;

    public bool TypeCheck(List<Item> c)
    {
        foreach (Item i in c)
        {
            if (i is ItemStarman) return true;
        }

        return false;
    }
    public override void UseItem(Player p)
    {
        foreach (Item i in p.item.current)
        {
            if (i is ItemStarman)
            {
                ItemStarman star = i as ItemStarman;
                star.timer = star.duration;
                p.item.held = null;
                p.item.hasItem = false;
                return;
            }
        }

        p.item.current.Add(this);
        p.item.held = null;
        p.item.hasItem = false;
        timer = duration;


        p.steer.steerTMod = p.v.lastTMod;
        p.maxSpeed = p.v.lastMaxSpeed;
        p.v.acceleration = p.v.lastAccelerate;

        p.steer.steerTMod *= 0.75f;
        p.maxSpeed *= 1.1f;
        p.v.acceleration *= 1.75f;

        p.v.currentTMod = p.steer.steerTMod;
        p.v.currentMaxSpeed = p.maxSpeed;
        p.v.currentAccelerate = p.v.acceleration;

        p.anim.SetDriverAnimState(State.VICTORY);

        p.anim.SetDriverMaterial(starMat);
        p.anim.SetKartMaterial(starMat);
        p.anim.SetWheelMaterial(starMat);

        p.anim.c_kart.particles.starman.gameObject.SetActive(true);
        p.v.isImmuneToDamage = true;

        if (p.v.wasAi)
            AudioManager.instance.Play(musicAi, p.transform.position, musicAi, p.transform);
        else
        {
            if (p.v.finalLap)
                music.pitch = 1.25f;
            if (p.v.playingFinalLap)
                return;
            AudioManager.instance.Stop(music);
            AudioManager.instance.Stop(GameManager.instance.main);
            AudioManager.instance.PlayMusic(music);
        }
   
    }
    public override void UsingItem(Player p)
    {
        if (timer < 0)
            return;

        p.anim.SetDriverAnimState(State.VICTORY);
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            EndItem(p);
        }
    }
    public override void EndItem(Player p)
    {
        p.steer.steerTMod = p.v.lastTMod;
        p.maxSpeed = p.v.lastMaxSpeed;
        p.v.acceleration = p.v.lastAccelerate;

        p.v.currentTMod = p.v.lastTMod;
        p.v.currentMaxSpeed = p.v.lastMaxSpeed;
        p.v.currentAccelerate = p.v.lastAccelerate;

        p.anim.SetDriverAnimState(State.NORMAL);

        p.anim.ResetMaterial();
        p.anim.c_kart.particles.starman.gameObject.SetActive(false);

        p.v.isImmuneToDamage = false;
        p.item.current.Remove(this);

        if (p.v.wasAi)
            AudioManager.instance.Stop(musicAi);
        else
        {
            if (p.v.raceOver)
                return;
            if (p.v.playingFinalLap)
                return;
            AudioManager.instance.Stop(GameManager.instance.main);
            AudioManager.instance.Stop(music);
            AudioManager.instance.PlayMusic(GameManager.instance.main);
        }

  
    }

    public override void HoldItem(Player p)
    {

    }
    public override void DropItem(Player p)
    {

    }
}
