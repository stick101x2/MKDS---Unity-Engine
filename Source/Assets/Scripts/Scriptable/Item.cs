using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[CreateAssetMenu(menuName = "MKDS/Items/Item")]
public abstract class Item : ScriptableObject
{
    public string Name;
    public GameObject prefab;
    public Sprite icon;
    [Space(5)]
    public int uses;
    public float duration;

    public abstract void EndItem(Player p);

    public abstract void HoldItem(Player p);
 
    public abstract void DropItem(Player p);

    public abstract void UsingItem(Player p);

    public abstract void UseItem(Player p);
}
/*
    public override void UseItem(Player p)
    {

    }
    public override void EndItem(Player p)
    {

    }
    public override void HoldItem(Player p)
    {

    }
    public override void DropItem(Player p)
    {

    }
    public override void UsingItem(Player p)
    {

    }
 */
