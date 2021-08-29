using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
[System.Serializable]
public class Sound
{
	public string name;
	[Space(5)]
	public AudioClip mainClip;
	public AudioClip loopingClip;
	public bool loop;
	[Range(0f, 1f)]
    public float volume;
    [Range(-3f, 3f)]
    public float pitch;
	[Space(5)]
	public int bank = 0;
	public byte priorty = 128;
	public bool canBeOverriten = true;
	public bool alwaysOverride = false;
	public AudioMixerGroup output;
	public SoundSource source;
}
[System.Serializable]
public class Sound3D : Sound
{
	[Space(10)]
	public bool is3DSound = true;
	[Range(0f, 5f)]
	public float doppler;
	[Range(0f, 100f)]
	public float maxDistance = 500;
	public AnimationCurve distanceFade;
}