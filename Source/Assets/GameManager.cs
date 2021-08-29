using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static int quality = 0;
    public static int last;
    public List<Player> players;
    public static GameManager instance;
    [Header("Race")]
    public int laps = 3;
    public int minPlace = 4;
    [Header("Music")]
    public Sound main;
    [Space(5)]
    public Sound goal;
    public Sound finalLap;
    [Space(5)]
    public Sound Victory;
    public Sound Miss;
    public Sound Lost;
    [Space(5)]
    public Sound VictoryLap;
    public Sound MissLap;
    public Sound LostLap;
    [Header("Debug")]
    public bool randomPlayer;
    public PlayerUi p_ui;
    public float waitTillNextScene = 30f;
    private void Awake()
    {
        QualitySettings.SetQualityLevel(quality);

        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        Setup();
    }
    void Setup()
    {
        if(randomPlayer)
            DebugSetRandomPlayer();
    }
    public List<int> valided;
    void Start()
    {
        AudioManager.instance.PlayMusic(main);
    }
    public void DebugSetRandomPlayer()
    {
        Player p = null;
        for (int i = 0; i < players.Count; i++)
        {
            valided.Add(i);
        }
        int randomTest = Random.Range(0, valided.Count);
        if (randomTest > 5)
        {
            int lower = randomTest - 6;
            valided.Remove(lower);
            valided.Remove(randomTest);
        }
        else
        {
            int up = randomTest + 6;
            valided.Remove(up);
            valided.Remove(randomTest);
        }
        int random = randomTest;
        last = random;
        p = players[random];

        p_ui.p = p;
        p.ui = p_ui;
        p.Awake();
        p.v.debugAi = true;
        p.v.isAi = false;
        p.input.isAi = false;
        p.v.listner.gameObject.SetActive(true);
        p.v.main.Follow = p.v.mainRotator;
        p.v.main.LookAt = p.v.lookat;
        p.v.goal.Follow = p.v.mainRotator;
        p.v.goal.LookAt = p.v.lookat;
    }
    public void Goal(Player p)
    {
        StartCoroutine(EGoal(p));
    }

    IEnumerator EGoal(Player p)
    {
        if (!p.v.wasAi)
        {
            p.v.main.Priority = 0;
            p.v.goal.Priority = 1;
            p.item.StopStarmanMusic();
            AudioManager.instance.Stop(main);
            AudioManager.instance.Play2D(goal);
            DisableAllSounds();
            p.SetAi();
            p.input.accelHeld = true;
        }

        if(p.lap.Placing >= minPlace)
        {
            p.maxSpeed *= 0.75f;
        }else if(p.lap.Placing <= 0)
        {
            p.maxSpeed *= 1.25f;
        }

        yield return new WaitForSeconds(0.25f);
        p.lap.PlayGoalSound();
        yield return new WaitForSeconds(0.5f);
        float delay = GoalSound(p);
        yield return new WaitForSeconds(delay + 0.665f);
        GoalMusic(p);
        if(!p.v.wasAi)
        {
            yield return new WaitForSecondsRealtime(waitTillNextScene);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
            
    }
    public void DisableAllSounds()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].kart.audo.StopPlayers();
        }
        AudioManager.instance.DisableAllSfx();
    }
    public float GoalMusic(Player p)
    {
        if (p.v.wasAi)
            return 0f;

        if (p.lap.Placing <= 0)
        {
            AudioManager.instance.PlayMusic(VictoryLap);
            return VictoryLap.mainClip.length;
        }
        else if (p.lap.Placing > 0 && p.lap.Placing < minPlace)
        {
            AudioManager.instance.PlayMusic(MissLap);
            return MissLap.mainClip.length;
        }
        else if (p.lap.Placing >= minPlace)
        {
            AudioManager.instance.PlayMusic(LostLap);
            return LostLap.mainClip.length;
        }
        return 0f;
    }
    public float GoalSound(Player p)
    {
        if (p.v.wasAi)
            return 0f;

        if (p.lap.Placing <= 0)
        {
            AudioManager.instance.PlayMusic(Victory);
            return Victory.mainClip.length;
        }
        else if (p.lap.Placing > 0 && p.lap.Placing < minPlace)
        {
            AudioManager.instance.PlayMusic(Miss);
            return Miss.mainClip.length;
        }
        else if (p.lap.Placing >= minPlace)
        {
            AudioManager.instance.PlayMusic(Lost);
            return Lost.mainClip.length;
        }
        return 0f;
    }
    public void FinalLap(Player p)
    {
        p.v.finalLap = true;
        p.v.playingFinalLap = true;
        if (p.v.wasAi)
            return;
        p.item.StopStarmanMusic();
        StartCoroutine(Flap(p));
        
    }

    IEnumerator Flap(Player p)
    {
        AudioManager.instance.Stop(main);
        AudioManager.instance.PlayMusic(finalLap);
        yield return new WaitForSeconds(finalLap.mainClip.length + 0.5f);
        p.v.playingFinalLap = false;
        Sound s = main;
        s.pitch = 1.25f;
        if (p.item.PlayStarmanMusic(1.25f))
            yield break;
        AudioManager.instance.PlayMusic(main, s);
    }
    

    int frame;
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            quality += 1;
            if (quality > 2)
                quality = 0;
            QualitySettings.SetQualityLevel(quality);
        }
        frame++;
        if(frame >= 10)
        {
            SlowUpdate();
            frame = 0;
        }
    }
    void SlowUpdate()
    {
        RankPlayers();
    }

    public int GetPos(Player plr)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == plr)
            {
                return i;
            }
        }
        return -1;
    }
    public void RankPlayers()
    {
        players.Sort(delegate (Player c1, Player c2) {
            return c1.GetComponent<Player>().lap.racePosition.CompareTo
                        (c2.GetComponent<Player>().lap.racePosition);
        });
        players.Reverse();
    }
}
