using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItem : MonoBehaviour, IPlayer
{
    public bool hasItem;
    public bool currentlyGettingItem;
    public Item held;
    public List<Item> current;
    public Item[] testAsset;
    Player p;

    
    public bool throwBack;

    float aiTestTimer;

    public event System.Action OnItemUse;

    public void Setup(Player p)
    {
        current = new List<Item>();

        p.v.lastTMod = p.steer.steerTMod;
        p.v.lastMaxSpeed = p.maxSpeed;
        p.v.lastAccelerate = p.v.acceleration;

        p.v.currentTMod = p.v.lastTMod;
        p.v.currentMaxSpeed = p.v.lastMaxSpeed;
        p.v.currentAccelerate = p.v.lastAccelerate;

        aiTestTimer = Random.Range(0f, 12f);
        GetTestItem();

        this.p = p;
    }

    public void GetTestItem()
    {
        int random = Random.Range(0, testAsset.Length);
        held = Object.Instantiate(testAsset[random]);
    }
    public void Ai()
    {
        aiTestTimer -= Time.deltaTime;

        if (aiTestTimer < 0)
        {
            int ran = Random.Range(0, 7);
            if(ran < 4)
            {
                aiTestTimer = Random.Range(0.5f, 2f);
            }else
            {
                aiTestTimer = Random.Range(0f, 12f);
            }

            if (!CanUseItem())
                return;

            p.input.itemDown = true;
        }
    }
    bool CanUseItem()
    {
        if (p.v.raceOver)
            return false;
        if (!hasItem)
            return false;
        if (p.v.isHurt)
            return false;
        return true;
    }
    private void FixedUpdate()
    {
        if(current.Count > 0)
        {
            for (int i = 0; i < current.Count; i++)
            {
                current[i].UsingItem(p);
            }
        }

        if(p.v.isAi)
        {
            Ai();
        }

        if (!CanUseItem())
            return;

        if(p.input.itemDown)
        {
            p.input.itemDown = false;
            UseItem();
            p.driver.saudio.Play(p.driver.Attack); 
        }
    }
    public void UseItem()
    {
        throwBack = false;
        if (p.v.isAi)
        {
            int b = Random.Range(0, 10);
            int placing = p.lap.Placing;
            if(placing < 3)
            {
                throwBack = b > 5;
                if (placing < 1)
                {
                    throwBack = b > 1;
                }
            }
        }else
        {
            throwBack = p.input.y < -0.5f;
        }

        held.UseItem(p);
        OnItemUse?.Invoke();
    }
    public void OnSpawnObject(ItemObject iObject)
    {
        iObject.onHitTarget -= SuccesfulHit;
        iObject.onHitTarget += SuccesfulHit;
    }

    public void SuccesfulHit()
    {
        if (p.v.raceOver)
            return;

        p.anim.Good();
       p.driver.saudio.Play(p.driver.Good);
    }
    public void GotNewItem()
    {
        currentlyGettingItem = false;
        hasItem = true;
    }
    public bool StopStarmanMusic()
    {
        foreach (Item i in p.item.current)
        {
            if (i is ItemStarman)
            {
                ItemStarman star = i as ItemStarman;
                AudioManager.instance.Stop(star.music);
                return true;
            }
        }
        return false;
    }
    public bool PlayStarmanMusic(float pitch = 1f)
    {
        foreach (Item i in p.item.current)
        {
            if (i is ItemStarman)
            {
                ItemStarman star = i as ItemStarman;
                star.music.pitch = pitch;

                AudioManager.instance.Stop(star.music);
                AudioManager.instance.PlayMusic(star.music);
                return true;
            }
        }
        return false;
    }
    public void GetNewItem()
    {
        if (p.v.raceOver)
            return;
        if (currentlyGettingItem)
            return;
        if (hasItem)
            return;

        if (p.ui == null)
        {
            StartCoroutine(AIGetNewItem());
            return;
        }


        currentlyGettingItem = true;
        p.ui.roulette.GetItem(p);
    }

    public IEnumerator AIGetNewItem()
    {
        currentlyGettingItem = true;
        held = Object.Instantiate(ItemManager.instance.table.GetItem(p.lap.Placing));
        yield return new WaitForSeconds(4f);
        GotNewItem();
    }
}
