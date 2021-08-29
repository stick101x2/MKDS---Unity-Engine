using MidiPlayerTK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Demo CatchMusic
/// </summary>
namespace MPTKDemoCatchMusic
{
    
    /// <summary>
    /// Defined behavior of a note
    /// </summary>
    public class NoteView : MonoBehaviour
    {
        public static bool FirstNotePlayed = false;
        public MPTKEvent note;
        public MidiStreamPlayer midiStreamPlayer;
        public bool played = false;
        public Material MatPlayed;
        public float zOriginal;
        // 
        /// <summary>
        /// Update
        ///! @code
        /// midiFilePlayer.MPTK_PlayNote(note);
        /// FirstNotePlayed = true;
        ///! @endcode
        /// </summary>
        public void Update()
        {
            // The midi event is played with a MidiStreamPlayer when position X < -45 (falling)
            if (!played && transform.position.x < -45f)
            {
                played = true;
                // If original z is not the same, the value will be changed, too bad for the ears ...
                int delta = (int)(zOriginal - transform.position.z);
                //Debug.Log($"Note:{note.Value} Z:{transform.position.z:F1} DeltaZ:{delta} Travel Time:{note.MPTK_DeltaTimeMillis} ms");
                //! [Example PlayNote]
                note.Value += delta; // change the original note
                // Now play the note with a MidiStreamPlayer prefab
                midiStreamPlayer.MPTK_PlayEvent(note);
                //! [Example PlayNote]
                FirstNotePlayed = true;

                gameObject.GetComponent<Renderer>().material = MatPlayed;// .color = Color.red;
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