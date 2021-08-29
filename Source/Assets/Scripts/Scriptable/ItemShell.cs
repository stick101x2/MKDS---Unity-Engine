using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "MKDS/Items/Shell")]
public class ItemShell : Item
{
    public override void UseItem(Player p)
    {
        bool throwBack = p.item.throwBack;

        Transform spawn = throwBack ? p.v.holdPoint : p.v.firePoint;

        Shell shell = Instantiate(prefab, spawn.position, Quaternion.identity, null).GetComponent<Shell>();

        Vector3 dir = throwBack ? -p.v.mainRotator.forward : p.v.mainRotator.forward;

        if (shell is RedShell)
        {
            RedShell rShell = shell as RedShell;

            rShell.item.owner = p;
            rShell.SpawnShell(Mathf.Clamp(p.speed,125,p.maxSpeed + 25f), dir, p.ai.currentWaypoint);
            p.item.OnSpawnObject(rShell.item);
            p.item.held = null;
            p.item.hasItem = false;
            return;
        }
        shell.item.owner = p;
        shell.SpawnShell(Mathf.Clamp(p.speed, 125, p.maxSpeed + 25f), dir);
        p.item.OnSpawnObject(shell.item);
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
