using MidiPlayerTK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPTKDemoCatchMusic
{

    public class ControlView : MonoBehaviour
    {
        public MPTKEvent note;
        public MidiStreamPlayer midiStreamPlayer;
        public bool played = false;
        public Material MatPlayed;
        public float zOriginal;

        void Update()
        {
            // The midi event is played with a MidiStreamPlayer when position X < -45 (falling)
            if (!played && transform.position.x < -45f)
            {
                played = true;
                // If original z is not the same, the value will be changed, too bad for the ears ...
                int delta = (int)(zOriginal - transform.position.z);
                note.Value += delta;
                // Now play the control change with a MidiStreamPlayer prefab
                midiStreamPlayer.MPTK_PlayEvent(note);

                gameObject.GetComponent<Renderer>().material = MatPlayed;
            }
            if (transform.position.y < -30f)
            {
                Destroy(this.gameObject);
            }
        }

        void FixedUpdate()
        {
            // Move the note along the X axis
            float translation = Time.fixedDeltaTime * MusicView.Speed;
            transform.Translate(-translation, 0, 0);
        }
    }
}