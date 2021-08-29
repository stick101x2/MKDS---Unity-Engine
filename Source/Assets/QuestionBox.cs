using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionBox : ItemBox
{
    public Sound3D breakSfx;
    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Player p = other.GetComponent<Player>();
            p.item.GetNewItem();
            AudioManager.instance.Play(breakSfx, transform.position, breakSfx);
            ObjectPooler.instance.SpawnPoolObject("itemBoxBreak", transform.position, Quaternion.identity);
        }
    }
}
