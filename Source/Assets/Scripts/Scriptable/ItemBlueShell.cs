using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "MKDS/Items/Blue Shell")]
public class ItemBlueShell : Item
{
    public override void UseItem(Player p)
    {
        BlueShell Bshell = Instantiate(prefab, p.v.firePoint.position, Quaternion.identity, null).GetComponent<BlueShell>();
        Bshell.item.owner = p;
        Bshell.SpawnShell(p.v.foward.forward, p.ai.currentWaypoint);
        p.item.OnSpawnObject(Bshell.item);
        p.item.held = null;
        p.item.hasItem = false;
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
}
