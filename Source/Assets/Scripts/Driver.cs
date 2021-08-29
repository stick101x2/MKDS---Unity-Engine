using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Driver : MonoBehaviour
{
    public enum DriverId
    {
        UNUSED = -1,
        MARIO,
        LUIGI
    }
 //   public DriverId id;
    public DriverSettings settings;

    public Animator anim;
    public Renderer model;
    public SingleAudioManager saudio;
    [Header("Audio")]
    public SoundSingle Attack;
    public SoundSingle Victory;
    public SoundSingle Miss;
    public SoundSingle Lost;
    public SoundSingle Nice;
    public SoundSingle Good;
    public SoundSingle Great;
    public SoundSingle Damage;

    private void Awake()
    {
        Victory.priorty = 255;
        Miss.priorty = 255;
        Lost.priorty = 255;
        Great.priorty = 255;

        Good.playDelay = 1f;

        Good.priorty = 192;
        Damage.priorty = 192;
    }
}
