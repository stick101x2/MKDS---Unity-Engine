using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerVariables : MonoBehaviour
{
    public bool debugAi;
    public bool isAi;
    public bool wasAi;
    [Space(10)]
    public float acceleration;
    [Space(5)]
    public float realMaxSpeed;
    public float realSpeed { get; set; }
    [Space(5)]
    public float modelturnSpeed;
    [Space(5)]
    public bool jumped;
    [Space(5)]
    public bool finalLap;
    public bool playingFinalLap;
    public bool isHurt;
    public bool isImmuneToDamage;
    public bool bumped;
    public bool raceOver;
    public bool isBoosting;
    public bool atWall;
    [Space(5)]
    public float lastTMod;
    public float lastAccelerate;
    public float lastMaxSpeed;

    public float currentTMod;
    public float currentAccelerate;
    public float currentMaxSpeed;
    [Space(5)]
    public GameObject listner;
    [Space(5)]
    public Transform lookat;
    public Transform foward;
    public Transform holdPoint;
    public Transform firePoint;
    public Transform mainRotator;
    public Transform pivot;
    public Transform allMove;
    [Space(5)]
    public CinemachineVirtualCamera main;
    public CinemachineVirtualCamera goal;
}
