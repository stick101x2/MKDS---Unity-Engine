using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueShellAnim : MonoBehaviour
{
    public void Explode()
    {
        BlueShell s = GetComponentInParent<BlueShell>();

        s.Explode();
    }
}
