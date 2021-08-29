using System.Collections.Generic;
using UnityEngine;

namespace Reign.Audio
{
    public class AudioListener3D : MonoBehaviour
    {
        internal new Transform transform;
        internal Dictionary<AudioSource3D, UnityEngine.AudioSource> sources;

        private void Start()
        {
            transform = GetComponent<Transform>();
            sources = new Dictionary<AudioSource3D, UnityEngine.AudioSource>();
            AudioSystem3D.AddAudioListener(this);
        }

        private void OnDestroy()
        {
            AudioSystem3D.RemoveAudioListener(this);
        }
    }
}