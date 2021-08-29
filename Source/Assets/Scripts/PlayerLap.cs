using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLap : MonoBehaviour, IPlayer
{
    public int Placing;
    [Space(5)]
    Player p;
    public Transform position;
    

    public float racePosition;
    public int lap;
    public bool[] keys = new bool[LapPointManager.keys];
    public int region;

    public LapPoint next;
    public LapPoint current;

    public float disLastLapPoint;
    [Header("Lakitu")]
    public Texture[] textures;
    public Material signMat;
    public Animator lak;
    public void Setup(Player p)
    {
        this.p = p;

        lap = 1;
        keys = new bool[LapPointManager.keys];
        region = -1;
    }

    public void GoalLap()
    {
        if(!p.v.isHurt)
            p.anim.SetDriverAnimState(1);
    }
    public void LostLap()
    {
        if (!p.v.isHurt)
            p.anim.SetDriverAnimState(2);
    }
    public void SetWaypoint(LapPoint next, int key)
    {
        region = next.last.Id;

        current = next.last;
        this.next = next;

        if (key > -1)
        {
            int lastKey = key - 1;
            if (key - 1 < 0)
            { 
                keys[0] = true;
                return;
            }

            if(keys[lastKey] != true)
            {
                return;
            }


            keys[key] = true;

        }
    }
    int lastPlacing;
    void Update()
    {
        if(p.v.raceOver)
        {
            if(Placing < GameManager.instance.minPlace)
            {
                GoalLap();
            }
            else if(Placing >= GameManager.instance.minPlace)
            {
                LostLap();
            }

            return;
        }

        disLastLapPoint = GetDistanceToLastPoint();

        racePosition = lap * 100000 + (current.Id * 1000) + (disLastLapPoint * 3.5f);

        Placing = GameManager.instance.GetPos(p);
        if(Placing < lastPlacing)
        {
            int Rand = Random.Range(0, 5);
            if(Rand < 2)
            {
                p.driver.saudio.Play(p.driver.Nice);
            }
        }
        lastPlacing = Placing;
    }
    public bool HasAllKeys()
    {
        for (int i = 0; i < keys.Length; i++)
        {
            if(keys[i] == false)
            {
                return false;
            }
        }
        return true;
    }
    public void ResetAllKeys()
    {
        keys = new bool[LapPointManager.keys];
    }
    public void GetLap()
    {
        if (p.v.raceOver)
            return;

        if (lap < GameManager.instance.laps - 1)
        {
            signMat.mainTexture = textures[lap-1];
            if (!p.v.wasAi)
                AudioManager.instance.Play2D(p.audo.lap);
            lak.Play("lap", 0, 0);
        }
        else if(lap == GameManager.instance.laps - 1)
        {
            GameManager.instance.FinalLap(p);
            lak.Play("lap_final", 0, 0);
        }
        else if (lap >= GameManager.instance.laps)
        {
            p.v.raceOver = true;
            GameManager.instance.Goal(p);
            return;
        }

        lap++;
        ResetAllKeys();
    }
    float GetDistanceToLastPoint()
    {
        Vector3 l = new Vector3();

        l = current.transform.position;
        l.y = 0;

        Vector3 pos = transform.position - current.transform.position;
        Vector3 p = Vector3.Project(pos, current.transform.GetChild(0).up.normalized);
        Vector3 f = transform.position - p;

        float dis = Vector3.Distance(f, l) / 3.5f;
        return dis;
    }

    public void PlayGoalSound()
    {
        SoundSingle s = null;

        if(Placing <= 0)
        {
            s = p.driver.Victory;
        }else if (Placing > 0 && Placing < GameManager.instance.minPlace)
        {
            s = p.driver.Miss;
        }else if (Placing >= GameManager.instance.minPlace)
        {
            s = p.driver.Lost;
        }

        p.driver.saudio.Play(s);
    }
}
