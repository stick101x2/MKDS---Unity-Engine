using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Kart : MonoBehaviour
{
    public KartSettings settings;
    [Header("Steering")]
    public Transform front_wheel_right;
    public Transform front_wheel_left;
    [Header("Moving")]
    public Transform wheel_front_R;
    public Transform wheel_front_L;
    public Transform wheel_back_R;
    public Transform wheel_back_L;
    [Header("Drifting")]
    public Transform drift_r;
    public Transform drift_l;
    [Header("References")]
    public KartAudio audo;
    public Animator anim;
    public KartParticles particles;
    [Header("Textures")]
    public KartType type;
    public Renderer model;
    public Renderer[] wheels;
    public int bodyIndex = 2;
    public int ignoreIndex = 1;
    public int emblemIndex = 0;

    public enum KartType
    {
        UNUSED = -1,
        STANDARD
    }
}
