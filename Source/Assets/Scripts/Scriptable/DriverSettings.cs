using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
[CreateAssetMenu(fileName = "New Driver", menuName = "MKDS/Settings/Driver")]
public class DriverSettings : ScriptableObject
{
    public Stats stat;
    public Texture emblem;
    public Texture[] kartTextures;
}

[System.Serializable]
public class Stats
{
    //modifier, final = ((kart_stats + driver_stat) / 100 + 0.5)
    public int speed = 50; // modifies your max speed
    public Weight weight = Weight.Medium; // force when bumping other racers, also influcenes max speed
    public int acceleration = 50; // how fast you gain speed
    public int handling = 50; //how much speed is lost during turns
    public int drift = 50; // changes angle of drift
    public int offroad = 50; // traction how sharp your turns are
}

