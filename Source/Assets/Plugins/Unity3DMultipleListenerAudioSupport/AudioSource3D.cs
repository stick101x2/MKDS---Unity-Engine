using UnityEngine;
using UnityEngine.Audio;

namespace Reign.Audio
{
    public class AudioSource3D : MonoBehaviour
    {
        internal new Transform transform;
        public AudioClip clip;
        internal AudioClip lastClip;
        public AudioMixerGroup audioMixerGroup;
        internal AudioMixerGroup lastAudioMixerGroup;
        public bool playOnAwake;
        public bool loop;

        [Range(0, 1)]
        public float volume = 1;
        internal float lastVolume;

        public AudioRolloffMode volumeRolloff = AudioRolloffMode.Logarithmic;
        public float minDistance = 1;
        public float maxDistance = 500;

        private void Start()
        {
            lastClip = clip;
            lastAudioMixerGroup = audioMixerGroup;
            lastVolume = volume;

            transform = GetComponent<Transform>();
            AudioSystem3D.AddAudioSource(this);
        }

        private void OnDestroy()
        {
            AudioSystem3D.RemoveAudioSource(this);
        }

        public void Play()
        {
            AudioSystem3D.PlayAudio(this);
        }

        public void PlayOneShot(AudioClip clip)
        {
            AudioSystem3D.PlayAudioOneShot(this, clip);
        }

        public void Pause()
        {
            AudioSystem3D.PauseAudio(this);
        }

        public void Stop()
        {
            AudioSystem3D.StopAudio(this);
        }
    }
}