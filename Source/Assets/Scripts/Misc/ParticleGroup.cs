using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleGroup : MonoBehaviour
{
    [SerializeField]
    private List<ParticleSystem> systems;
    [SerializeField]
    private ParticleSystem top;

    private void Start()
    {
        if (!top)
            top = GetComponent<ParticleSystem>();

        systems.AddRange(GetComponentsInChildren<ParticleSystem>());

        if(!systems.Contains(top))
        {
            systems.Add(top);
        }
    }
    public void SetDuration(float duration)
    {
        for (int i = 0; i < systems.Count; i++)
        {
            if (systems[i].isPlaying)
                continue;

            ParticleSystem.MainModule main = systems[i].main;
            main.duration = duration;
        }
    }
    public void SetColor(Color color)
    {
        for (int i = 0; i < systems.Count; i++)
        {
            ParticleSystem.MainModule main = systems[i].main;
            main.startColor = color;
        }
    }
    public void Play()
    {
        if (!top)
            top = GetComponent<ParticleSystem>();
        top.Play(true);
    }
    public void Stop()
    {
        if (!top)
            top = GetComponent<ParticleSystem>();
        top.Stop(true);
    }
    public void Clear()
    {
        if (!top)
            top = GetComponent<ParticleSystem>();
        top.Clear(true);
    }
    public bool IsPlaying()
    {
        return top.isPlaying;
    }

}
