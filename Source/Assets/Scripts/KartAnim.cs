using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartAnim : MonoBehaviour
{
    PlayerDrift d;

    private void Awake()
    {
        d = GetComponentInParent<PlayerDrift>();
    }
    void JumpEnd()
    {
        d.JumpEnd();
    }
}
