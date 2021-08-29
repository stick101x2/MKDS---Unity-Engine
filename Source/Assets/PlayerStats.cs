using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IPlayer
{
    public float weight;
    Player p;
    public void Setup(Player p)
    {
        this.p = p;
        GetWeight();
    }
    public void GetWeight()
    {
        float kt = 0f;
        float dr = 0f;

        Weight kart = p.kart.settings.stat.weight;
        Weight driver = p.driver.settings.stat.weight;

        kt = (float)kart;
        dr = (float)driver;

        weight = kt + (dr * 0.333f);
    }
}
public enum Weight
{
    Heavy = 75,
    Medium = 50,
    Light = 25
}