using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartAudio : MonoBehaviour
{
    Player p;
    public Sound3D engine;
    public Sound3D drift;

    public float volumeChangeSpeed = 10f;
    public float minSpeed = 0.5f;
    public float pitchModifier = 1f;
    public float maxSpeed = 75;
    AudioSource[] players;
    bool isDriving = false;
    float volume;
    public void StopPlayers()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].Stop();
        }
    }
    // Start is called before the first frame update
    void Awake()
    {
        p = GetComponentInParent<Player>();
        players = new AudioSource[2]
        {
            gameObject.AddComponent<AudioSource>(),
            gameObject.AddComponent<AudioSource>()
        };

        foreach (var p in players)
        {
            p.loop = true;
            p.playOnAwake = false;
            p.volume = 0f;

            p.dopplerLevel = engine.doppler;
            p.spatialBlend = 1f;
            p.maxDistance = engine.maxDistance;
            p.rolloffMode = AudioRolloffMode.Custom;
            p.SetCustomCurve(AudioSourceCurveType.CustomRolloff, engine.distanceFade);
        }

        players[0].clip = engine.mainClip;
        players[1].clip = engine.loopingClip;

        players[0].Play();
        players[1].Play();
    }
    
    private void Update()
    {
        /*
        if(p.input.itemDown)
        {
            AudioManager.instance.Play(drift, transform.position, drift, transform);
            p.input.itemDown = false;
        }
        if (p.input.itemUp)
        {
            AudioManager.instance.Stop(drift);
            p.input.itemUp = false;
        }*/
        float speed = Mathf.Abs(p.v.realSpeed);
        players[1].pitch = minSpeed + (speed / maxSpeed) * pitchModifier;

        if(speed < 6f)
        {
            isDriving = false;
        }else
        {
            isDriving = true;
        }

        if(isDriving)
        {
            volume += Time.deltaTime * volumeChangeSpeed;
        }else
        {
            volume -= Time.deltaTime * volumeChangeSpeed;
        }
        volume = Mathf.Clamp(volume, 0f, 1f);

        float driveV = volume;
        float idleV = Func.Remap(volume, 0f, 1f, 1f, 0f);

        players[0].volume = idleV * engine.volume;
        players[1].volume = driveV * engine.volume;
    }
}
