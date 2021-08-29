using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class SingleAudioManager : MonoBehaviour
{
	public AudioSource audioSource;
	public AudioMixerGroup output;

	[Range(0f, 5f)]
	public float doppler;
	[Range(0f, 100f)]
	public float maxDistance = 500;
	public AnimationCurve distanceFade;
	public float canPlayTimer;
	SoundSingle last;
	void Awake()
	{
		if (audioSource == null)
			audioSource = gameObject.AddComponent<AudioSource>();
	
		audioSource.dopplerLevel = doppler;
		audioSource.spatialBlend = 1f;
		audioSource.maxDistance = maxDistance;
		audioSource.rolloffMode = AudioRolloffMode.Custom;
		audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, distanceFade);
		audioSource.outputAudioMixerGroup = output;
	}
	private void Update()
	{
		if(canPlayTimer > 0)
		{
			canPlayTimer -= Time.deltaTime;
		}
	}
	public void Play(SoundSingle s)
	{
		if (canPlayTimer > 0)
			return;
		if(last != null)
		{
			if (s.priorty <= last.priorty && audioSource.isPlaying)
				return;
		}
		

		last = s;
		audioSource.Stop();

		int random = UnityEngine.Random.Range(0, s.clips.Length);
		audioSource.clip = s.clips[random];
		if (audioSource.clip == null)
		{
			last = null;
			return;
		}
		
		audioSource.volume = s.volume;
		if (s.setPitch)
		{
			if (!s.randomPitch)
				audioSource.pitch = s.pitch;
			else
			{
				float pRandom = UnityEngine.Random.Range(s.minPitch, s.maxPitck);
				audioSource.pitch = pRandom;
			}
		}
		
		audioSource.Play();
	}
	public void UnPause()
	{
		audioSource.UnPause();
	}
	public void Pause()
	{
		audioSource.Pause();
	}
	public void SetPitch(float value)
	{
		audioSource.pitch = value;
	}
	public void SetVolume(float value)
	{
		audioSource.volume = value;
	}
	public void Stop()
	{
		audioSource.Stop();
	}
}
[System.Serializable]
public class SoundSingle
{
	[Space(5)]
	public AudioClip[] clips;
	
	[Range(0f, 1f)]
	public float volume = 1f;
	[Range(-3f, 3f)]
	public float pitch = 1f;

	public bool setPitch = false;
	public bool randomPitch = false;
	[Range(-3f, 3f)]
	public float minPitch = 0.9f;
	[Range(-3f, 3f)]
	public float maxPitck = 1.1f;
	[Space(5)]
	public float playDelay = -0.1f;
	public byte priorty = 128;
}