using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUi : MonoBehaviour
{
    public ItemRoulette roulette;
    public LapUi lap;
    public Player p;
    private void Awake()
    {
        roulette = GetComponentInChildren<ItemRoulette>();
        lap = GetComponentInChildren<LapUi>();
    }
}
