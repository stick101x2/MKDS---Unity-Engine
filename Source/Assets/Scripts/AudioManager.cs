using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
[System.Serializable]
public class AudioBank
{
    public SoundSource[] sources = new SoundSource[10];

    public void CreateBank(AudioManager ad,int id)
    {
        ad.CreateBank(sources, "SFX " + id);
    }
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public SoundSource[] music = new SoundSource[2];
    public AudioBank[] sfxBank = new AudioBank[3];
    public SoundSource[] ui = new SoundSource[5];
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        Setup();
    }
    public void DisableAllSfx()
    {
        for (int i = 0; i < sfxBank.Length; i++)
        {
            DisableAll(sfxBank[i].sources);
        }
    }
    public void DisableAll(SoundSource[] sources)
    {
        for (int i = 0; i < sources.Length; i++)
        {
            sources[i].Disable();
        }
    }
    public void CreateBank(SoundSource[] bank,string name)
    {
        for (int i = 0; i < bank.Length; i++)
        {
            bank[i] = new GameObject(name + " SoundSource " + i).AddComponent<SoundSource>();
            bank[i].transform.parent = transform;

            bank[i].Create(i, name);
            bank[i].Parent(bank[i].transform);
        }
    }
    public void Setup()
    {
        CreateBank(ui, "UI");
        CreateBank(music, "MUSIC");
        for (int i = 0; i < sfxBank.Length; i++)
        {
            sfxBank[i].CreateBank(this, i);
        } 
    }
    public void PlayMusic(Sound s, Sound settings = null)
    {
        if (s == null)
        {
            Dev.LogWarning("Unable To Play Sound Effect Of Name: ( " + name + " )");
            return;
        }
        SoundSource source = GetSoundSource(s.priorty, music);
        if (source == null)
        {
            Dev.LogWarning("No Available Sound Sources: ( " + name + " )");
            return;
        }
        source.SetSound(s);
        if (settings == null)
            source.SetSettings(s);
        else
            source.SetSettings(settings);

        source.Stop();
        //        Dev.Log(source.current.loop && source.current.loopingClip != null);
        if (source.current.loop && source.current.loopingClip != null)
        {
            source.PlayLoop();
            return;
        }
        source.Play();
    }
    public void Play2D(Sound s, Sound settings = null)
    {
        if (s == null)
        {
            Dev.LogWarning("Unable To Play Sound Effect Of Name: ( " + name + " )");
            return;
        }
        SoundSource source = GetSoundSource(s.priorty, ui);
        if (source == null)
        {
            Dev.LogWarning("No Available Sound Sources: ( " + name + " )");
            return;
        }
        source.SetSound(s);
        if (settings == null)
            source.SetSettings(s);
        else
            source.SetSettings(settings);

        source.Stop();
        //        Dev.Log(source.current.loop && source.current.loopingClip != null);
        if (source.current.loop && source.current.loopingClip != null)
        {
            source.PlayLoop();
            return;
        }
        source.Play();
    }
    public void Play(Sound s,Vector3 position = new Vector3(),Sound settings = null,Transform location = null,int bank = -1)
    {
       
        if (s == null)
        {
            Dev.LogWarning("Unable To Play Sound Effect Of Name: ( " + name +" )");
            return;
        }
        int c_bank = bank == -1 ? s.bank : bank;
        SoundSource[] sfx = GetSFXBank(c_bank);
        SoundSource source = GetSoundSource(s.priorty,sfx);
        if(source == null)
        {
            Dev.LogWarning("No Available Sound Sources: ( " + name + " )");
            return;
        }
        source.SetSound(s);
        if(settings == null)
            source.SetSettings(s);
        else
            source.SetSettings(settings);
        source.Parent(location);
        source.SetPosition(position);

        source.Stop(false);
 //       Dev.Log(source.current.loop && source.current.loopingClip != null);
        if (source.current.loop && source.current.loopingClip != null)
        {
            source.PlayLoop();
            return;
        }
        source.Play();
    }
    public void Stop(Sound s)
    {
        if (s.source == null)
            return;
        s.source.Stop(true);
    }
   
    
    SoundSource[] GetSFXBank(int id)
    {
       return sfxBank[id].sources;
    }

    SoundSource GetSoundSource(int priorty,SoundSource[] sources)
    {
        SoundSource source = null;

        foreach (SoundSource s in sources)
        {
            if(s.IsPlaying())
            {
                if(!s.current.canBeOverriten)
                    continue;

                if (!s.current.alwaysOverride)
                {
                    source = s;
                    return source;
                }

                if (priorty <= s.current.priorty)
                {
                    continue;
                }
            }

            source = s;
            break;
        }

        return source;
    }

}
public class SoundSource : MonoBehaviour
{
    public Sound current;

    public AudioSource main;
    public AudioSource loop;
    public GameObject mGO;
    public GameObject lGO;

    bool canPlay = true;
    public void SetPosition(Vector3 position)
    {
        gameObject.transform.position = position;
    }
    public void Create(int i,string name)
    {
        mGO = new GameObject(name + " Index: " + i);
        lGO = new GameObject(name + " Index: " + i);

        main = mGO.AddComponent<AudioSource>();
        main.playOnAwake = false;
        loop = lGO.AddComponent<AudioSource>();
        loop.playOnAwake = false;

        main.transform.parent = transform;
        lGO.transform.parent = transform;

        canPlay = true;
    }
    public void Stop(bool unParent = false)
    {
        StopAllCoroutines();

        main.Stop();
        loop.Stop();
        if(unParent)
            transform.parent = null;
    }
    public void PlayLoop()
    {
        if (!canPlay)
            return;

        StartCoroutine(Loop());
    }
    public void Play()
    {
        if (!canPlay)
            return;

        main.Play();
    }
    public void Parent(Transform p)
    {
        gameObject.transform.parent = p;
    }

    public void Disable()
    {
        canPlay = false;
        Stop(true); 
    }
    public void SetSound(Sound s)
    {
        if (!canPlay)
            return;

        if (current != null)
            current.source = null;

        s.source = this;
        current = s;

        main.Stop();
        loop.Stop();

        main.clip = s.mainClip;
        if (s.loopingClip == null)
            return;

        
        loop.clip = s.loopingClip;
    }
    public bool IsPlaying()
    {
        if (!canPlay)
            return false;

        if (main.isPlaying || loop.isPlaying)
        {
            return true;
        }
        return false;
    }
    public IEnumerator Loop()
    {
        if (!canPlay)
            yield break;

        main.Play();
        yield return new WaitUntil(() => !main.isPlaying);

        if (!canPlay)
            yield break;

        loop.Play();
    }
    public void SetSettings(Sound s)
    {
        if (!canPlay)
            return;
        //     Dev.Log("sound" + s);
        //     Dev.Log("main" + main);

        main.volume = s.volume;
        main.pitch = s.pitch;
        main.loop = s.loop;
        main.outputAudioMixerGroup = s.output;
        
        if(current.loopingClip != null)
        {
            loop.volume = s.volume;
            loop.pitch = s.pitch;
            loop.loop = true;
            main.loop = false;
            loop.outputAudioMixerGroup = s.output;
        }

        if (s is Sound3D)
        {
            Sound3D s3d = s as Sound3D;

            main.dopplerLevel = s3d.doppler;
            main.spatialBlend = 1f;
            main.maxDistance = s3d.maxDistance;
            main.rolloffMode = AudioRolloffMode.Custom;
            main.SetCustomCurve(AudioSourceCurveType.CustomRolloff, s3d.distanceFade);

            if (current.loopingClip != null)
            {
                loop.dopplerLevel = s3d.doppler;
                loop.spatialBlend = 1f;
                loop.maxDistance = s3d.maxDistance;
                loop.rolloffMode = AudioRolloffMode.Custom;
                loop.SetCustomCurve(AudioSourceCurveType.CustomRolloff, s3d.distanceFade);
            }
        }
    }
}
