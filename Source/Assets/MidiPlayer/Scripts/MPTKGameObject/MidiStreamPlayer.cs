﻿//#define DEBUGPERF
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine.Events;
using MEC;
using UnityEngine.UI;

namespace MidiPlayerTK
{
    /// <summary>
    /// The prefab MidiStreamPlayer is useful to play real time music in relation with user actions.\n
    /// Any Midi file is necessary, the notes are generated by your scripts from your own algorithm. Thank to the API of this class.\n
    /// The main function MPTK_PlayEvent() and the class MPTKEvent are able to create all kind of midi events as note-on.\n
    /// All the values must be set in MPTKEvent, command, note value, duration ... for more details look at the class MPTKEvent.\n
    /// A note-on must also stopped: if duration = -1 the note is infinite, it's the goal of MPTK_StopEvent() to stop the note with a note-off.\n
    /// On top of that, the Pro version adds playing chords with MPTK_PlayChordFromRange() and MPTK_PlayChordFromLib().\n
    /// For playing scales, have a look to the class MPTKRangeLib\n
    /// For more information see here https://paxstellar.fr/midi-file-player-detailed-view-2-2/ \n
    /// Also look at the demo TestMidiStream and the code source TestMidiStream.cs.
    ///! @code
    ///
    ///     // Need a reference to the prefab MidiStreamPlayer you have added in your scene hierarchy.
    ///     public MidiStreamPlayer midiStreamPlayer;
    ///     
    ///     // This object will be pass to the MPTK_PlayEvent for playing an event
    ///     MPTKEvent mptkEvent;
    ///     
    ///     new void Start()
    ///     {
    ///         // Find the MidiStreamPlayer. Could be also set directly from the inspector.
    ///         midiStreamPlayer = FindObjectOfType<MidiStreamPlayer>();
    ///     }
    ///
    ///    void Play()
    ///    {
    ///         // Pitch wheel change integrated in the play event
    ///         midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent()
    ///         {
    ///                 Command = MPTKCommand.PitchWheelChange, 
    ///                 Value = (int)PitchChange << 7, 
    ///                 Channel = StreamChannel 
    ///         });
    ///     
    ///         // Play a note
    ///         mptkEvent = new MPTKEvent()
    ///         {
    ///             Channel = 0,    // Between 0 and 15
    ///             Duration = -1,  // Infinite
    ///             Value = 60      // Between 0 and 127, with 60=C5
    ///             Velocity = 100, // Max 127
    ///         };
    ///         midiStreamPlayer.MPTK_PlayEvent(mptkEvent);
    ///     }
    ///     
    ///     // more later .... stop the note
    ///     void Stop()
    ///     {
    ///         midiStreamPlayer.MPTK_StopEvent(mptkEvent);
    ///     }
    ///     
    ///! @endcode
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [HelpURL("https://paxstellar.fr/midi-file-player-detailed-view-2-2/")]
    public partial class MidiStreamPlayer : MidiSynth
    {
        new void Awake()
        {
            base.Awake();
        }

        new void Start()
        {
            try
            {
                MPTK_InitSynth();
                base.Start();
                // Always enabled for midi stream
                MPTK_EnablePresetDrum = true;
                ThreadDestroyAllVoice();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play one midi event with a thread so the call return immediately.
        ///! @snippet MusicView.cs Example PlayNote
        /// </summary>
        public void MPTK_PlayEvent(MPTKEvent evnt)
        {
            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    if (!MPTK_CorePlayer)
                        Routine.RunCoroutine(TheadPlay(evnt), Segment.RealtimeUpdate);
                    else
                    {
                        lock (this) // V2.83
                        {
                            QueueSynthCommand.Enqueue(new SynthCommand() { Command = SynthCommand.enCmd.StartEvent, MidiEvent = evnt });
                        }
                    }

                }
                else
                    Debug.LogWarningFormat("SoundFont not yet loaded, Midi Event cannot be processed Code:{0} Channel:{1}", evnt.Command, evnt.Channel);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play a list of midi events with a thread so the call return immediately.
        /// @snippet TestMidiStream.cs Example MPTK_PlayEvent
        /// </summary>
        public void MPTK_PlayEvent(List<MPTKEvent> events)
        {
            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    if (!MPTK_CorePlayer)
                        Routine.RunCoroutine(TheadPlay(events), Segment.RealtimeUpdate);
                    else
                    {
                        lock (this) // V2.83
                        {
                            foreach (MPTKEvent evnt in events)
                                QueueSynthCommand.Enqueue(new SynthCommand() { Command = SynthCommand.enCmd.StartEvent, MidiEvent = evnt });
                        }
                    }
                }
                else
                    Debug.LogWarningFormat("SoundFont not yet loaded, Midi Events cannot be processed");
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private IEnumerator<float> TheadPlay(MPTKEvent evnt)
        {
            if (evnt != null)
            {
                try
                {
                    //TBR if (!MPTK_PauseOnDistance || MidiPlayerGlobal.MPTK_DistanceToListener(this.transform) <= VoiceTemplate.Audiosource.maxDistance)
                    {
#if DEBUGPERF
                        DebugPerf("-----> Init perf:", 0);
#endif
                        MPTK_PlayDirectEvent(evnt);
#if DEBUGPERF
                        DebugPerf("<---- ClosePerf perf:", 2);
#endif
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
            yield return 0;
        }

        private IEnumerator<float> TheadPlay(List<MPTKEvent> events)
        {
            if (events != null && events.Count > 0)
            {
                try
                {
                    try
                    {
                        //TBR if (!MPTK_PauseOnDistance || MidiPlayerGlobal.MPTK_DistanceToListener(this.transform) <= VoiceTemplate.Audiosource.maxDistance)
                        {
                            PlayEvents(events, true);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        MidiPlayerGlobal.ErrorDetail(ex);
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
            yield return 0;

        }

        /// <summary>
        /// Stop playing the note. All waves associated to the note are stop by sending a noteoff.
        /// </summary>
        /// <param name="pnote"></param>
        public void MPTK_StopEvent(MPTKEvent pnote)
        {
            if (!MPTK_CorePlayer)
                StopEvent(pnote);
            else
            {
                lock (this) // V2.83
                {
                    QueueSynthCommand.Enqueue(new SynthCommand() { Command = SynthCommand.enCmd.StopEvent, MidiEvent = pnote });
                }
            }

            //try
            //{
            //    if (pnote != null && pnote.Voices != null)
            //    {
            //        foreach (fluid_voice voice in pnote.Voices)
            //            if (voice.volenv_section != fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE &&
            //                voice.status != fluid_voice_status.FLUID_VOICE_OFF)
            //                voice.fluid_voice_noteoff();
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    MidiPlayerGlobal.ErrorDetail(ex);
            //}
        }
    }
}

