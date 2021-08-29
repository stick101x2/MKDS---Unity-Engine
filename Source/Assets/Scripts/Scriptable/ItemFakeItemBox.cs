using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "MKDS/Items/Fake Item Box")]
public class ItemFakeItemBox : Item
{
    public float spawnSpeed = 20f;
    public float spawnHeight = 10f;
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
    public override void UseItem(Player p)
    {
        bool throwBack = p.item.throwBack;

        Transform spawn = throwBack ? p.v.holdPoint : p.v.firePoint;

        FakeItemBoxEntity fake = Instantiate(prefab, spawn.position, Quaternion.identity, null).GetComponent<FakeItemBoxEntity>();

        Vector3 dir = throwBack ? -p.v.mainRotator.forward : p.v.mainRotator.forward;

        fake.item.owner = p;
        float speed = throwBack ? 1f :  spawnSpeed * 3.5f;
        speed = Mathf.Clamp(speed, spawnSpeed * 3.5f, p.maxSpeed + spawnSpeed);
        float height = throwBack ? 1f : spawnHeight * 3.5f;
        fake.main.Spawn(speed, dir, height);
        p.item.OnSpawnObject(fake.item);
        p.item.held = null;
        p.item.hasItem = false;
    }
}
