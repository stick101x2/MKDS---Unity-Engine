using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disable : Destroy
{
    public override void LifeTimeEnd()
    {
        gameObject.SetActive(false);
    }
}
