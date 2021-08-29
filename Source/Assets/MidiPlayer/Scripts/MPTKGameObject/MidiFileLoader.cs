
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using UnityEngine.Events;
using MEC;

namespace MidiPlayerTK
{
    /// <summary>
    /// Script associated to the prefab MidiFileLoader. 
    /// No sequencer, no synthetizer, no music playing capabilities. 
    /// Usefull to load all or part of the Midi events from a Midi and process, transform, write them to what you want. 
    /// List of Midi file must be defined with Midi Player Setup (see Unity menu MPTK).
    ///! @code
    /// // Example of script. See TestMidiFileLoad.cs for a more detailed usage.
    /// // Need of a reference to the Prefab (to be set in the hierarchy)
    /// MidiFileLoader MidiLoader;
    /// 
    /// if (MidiLoader==null)  
    ///    Debug.LogError("TestMidiFileLoad: there is no MidiFileLoader Prefab set in Inspector.");
    ///    
    /// // Defined index (from the Midi list defined in MPTK)
    /// MidiLoader.MPTK_MidiIndex = midiindex;
    /// 
    /// // Load Midi event from the Midi file
    /// MidiLoader.MPTK_Load();
    /// 
    /// // Get the list of events from start to end (in ticks)
    /// List<MPTKEvent> events = MidiLoader.MPTK_ReadMidiEvents(StartTicks, EndTicks);
    ///! @endcode
    /// </summary>
    public class MidiFileLoader : MonoBehaviour
    {
        /// <summary>
        /// Midi name to load. Use the exact name defined in Unity resources folder MidiDB without any path or extension.
        /// Tips: Add Midi files to your project with the Unity menu MPTK or add it directly in the ressource folder and open Midi File Setup to automatically integrate Midi in MPTK.
        /// </summary>
        public string MPTK_MidiName
        {
            get
            {
                //Debug.Log("MPTK_MidiName get " + midiNameToPlay);
                return midiNameToPlay;
            }
            set
            {
                //Debug.Log("MPTK_MidiName set " + value);
                midiIndexToPlay = MidiPlayerGlobal.MPTK_FindMidi(value);
                //Debug.Log("MPTK_MidiName set index= " + midiIndexToPlay);
                midiNameToPlay = value;
            }
        }
        [SerializeField]
        [HideInInspector]
        private string midiNameToPlay;

        /// <summary>
        /// Index Midi. Find the Index of Midi file from the popup in MidiFileLoader inspector.
        /// Tips: Add Midi files to your project with the Unity menu MPTK or add it directly in the ressource folder and open Midi File Setup to automatically integrate Midi in MPTK.
        /// return -1 if not found
        ///! @code
        /// midiFileLoader.MPTK_MidiIndex = 1;
        ///! @endcode
        /// </summary>
        /// <param name="index"></param>
        public int MPTK_MidiIndex
        {
            get
            {
                try
                {
                    //int index = MidiPlayerGlobal.MPTK_FindMidi(MPTK_MidiName);
                    //Debug.Log("MPTK_MidiIndex get " + midiIndexToPlay);
                    return midiIndexToPlay;
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
                return -1;
            }
            set
            {
                try
                {
                    //Debug.Log("MPTK_MidiIndex set " + value);
                    if (value >= 0 && value < MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count)
                    {
                        MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[value];
                        // useless, set when set midi name : 
                        midiIndexToPlay = value;
                    }
                    else
                        Debug.LogWarning("MidiFilePlayer - Set MidiIndex value not valid : " + value);
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private int midiIndexToPlay;

        /// <summary>
        /// Log midi events
        /// </summary>
        public bool MPTK_LogEvents;

        /// <summary>
        /// Should keep note off event Events ? 
        /// </summary>
        public bool MPTK_KeepNoteOff;

        /// <summary>
        /// Should accept change tempo from Midi Events ? 
        /// </summary>
        public bool MPTK_EnableChangeTempo;

        /// <summary>
        /// Initial tempo found in the Midi
        /// </summary>
        public double MPTK_InitialTempo;

        /// <summary>
        /// Duration of the midi. This duration is not constant depending of midi event change tempo inside the midi file.
        /// </summary>
        public TimeSpan MPTK_Duration;

        /// <summary>
        ///V2.88 removed Real Duration of the midi calculated with the midi change tempo events find inside the midi file.
        /// </summary>
        //public TimeSpan MPTK_RealDuration;

        /// <summary>
        /// Duration (milliseconds) of the midi. 
        /// </summary>
        public float MPTK_DurationMS { get { try { if (miditoload != null) return miditoload.MPTK_DurationMS; } catch (System.Exception ex) { MidiPlayerGlobal.ErrorDetail(ex); } return 0f; } }


        /// <summary>
        /// Last tick position in Midi: Time of the last midi event in sequence expressed in number of "ticks". MPTK_TickLast / MPTK_DeltaTicksPerQuarterNote equal the duration time of a quarter-note regardless the defined tempo.
        /// </summary>
        public long MPTK_TickLast;

        /// <summary>
        /// From TimeSignature event: The numerator counts the number of beats in a measure. For example a numerator of 4 means that each bar contains four beats. This is important to know because usually the first beat of each bar has extra emphasis.
        /// https://paxstellar.fr/2020/09/11/midi-timing/
        /// </summary>
        public int MPTK_NumberBeatsMeasure;

        /// <summary>
        /// From TimeSignature event: number of quarter notes in a beat. Equal 2 Power TimeSigDenominator.
        /// https://paxstellar.fr/2020/09/11/midi-timing/
        /// </summary>
        public int MPTK_NumberQuarterBeat;

        /// <summary>
        /// From TimeSignature event: The numerator counts the number of beats in a measure. For example a numerator of 4 means that each bar contains four beats. This is important to know because usually the first beat of each bar has extra emphasis. In MIDI the denominator value is stored in a special format. i.e. the real denominator = 2^[dd]
        /// https://paxstellar.fr/2020/09/11/midi-timing/
        /// </summary>
        public int MPTK_TimeSigNumerator;

        /// <summary>
        /// From TimeSignature event: The denominator specifies the number of quarter notes in a beat. 2 represents a quarter-note, 3 represents an eighth-note, etc. . 
        /// https://paxstellar.fr/2020/09/11/midi-timing/
        /// </summary>
        public int MPTK_TimeSigDenominator;

        /// <summary>
        /// From TimeSignature event: The standard MIDI clock ticks every 24 times every quarter note (crotchet) so a [cc] value of 24 would mean that the metronome clicks once every quarter note. A [cc] value of 6 would mean that the metronome clicks once every 1/8th of a note (quaver).
        /// https://paxstellar.fr/2020/09/11/midi-timing/
        /// </summary>
        public int MPTK_TicksInMetronomeClick;

        /// <summary>
        /// From TimeSignature event: This value specifies the number of 1/32nds of a note happen every MIDI quarter note. It is usually 8 which means that a quarter note happens every quarter note.
        /// https://paxstellar.fr/2020/09/11/midi-timing/
        /// </summary>
        public int MPTK_No32ndNotesInQuarterNote;

        /// <summary>
        /// From the SetTempo event: The tempo is given in micro seconds per quarter beat. 
        /// To convert this to BPM we needs to use the following equation:BPM = 60,000,000/[tt tt tt]
        /// Warning: this value can change during the playing when a change tempo event is find. 
        /// https://paxstellar.fr/2020/09/11/midi-timing/
        /// </summary>
        public int MPTK_MicrosecondsPerQuarterNote;

        /// <summary>
        /// From Midi Header: Delta Ticks Per Quarter Note. 
        /// Represent the duration time in "ticks" which make up a quarter-note. 
        /// For instance, if 96, then a duration of an eighth-note in the file would be 48.
        /// </summary>
        public int MPTK_DeltaTicksPerQuarterNote;

        /// <summary>
        /// Count of track read in the Midi file
        /// </summary>
        public int MPTK_TrackCount;

        private MidiLoad miditoload;


        void Awake()
        {
            //Debug.Log("Awake MidiFilePlayer midiIsPlaying:" + midiIsPlaying);
        }

        void Start()
        {
            //Debug.Log("Start MidiFilePlayer midiIsPlaying:" + midiIsPlaying + " MPTK_PlayOnStart:" + MPTK_PlayOnStart);
        }


        /// <summary>
        /// Load the midi file defined with MPTK_MidiName or MPTK_MidiIndex or from a array of bytes
        /// </summary>
        /// <param name="midiBytesToLoad"></param>
        public void MPTK_Load(byte[] midiBytesToLoad = null)
        {
            try
            {
                // Load description of available soundfont
                //if (MidiPlayerGlobal.ImSFCurrent != null && MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                {
                    if (string.IsNullOrEmpty(MPTK_MidiName))
                        MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[0];
                    int selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == MPTK_MidiName);
                    if (selectedMidi < 0)
                    {
                        Debug.LogWarning("MidiFilePlayer - MidiFile " + MPTK_MidiName + " not found. Try with the first in list.");
                        selectedMidi = 0;
                        MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[0];
                    }

                    try
                    {
                        miditoload = new MidiLoad();

                        // No midi byte array, try to load from MidiFile from resource
                        if (midiBytesToLoad == null || midiBytesToLoad.Length == 0)
                        {
                            TextAsset mididata = Resources.Load<TextAsset>(System.IO.Path.Combine(MidiPlayerGlobal.MidiFilesDB, MPTK_MidiName));
                            midiBytesToLoad = mididata.bytes;
                        }

                        miditoload.KeepNoteOff = MPTK_KeepNoteOff;
                        miditoload.EnableChangeTempo = MPTK_EnableChangeTempo;
                        miditoload.LogEvents = MPTK_LogEvents;
                        miditoload.MPTK_Load(midiBytesToLoad);
                        SetAttributes();
                    }
                    catch (System.Exception ex)
                    {
                        MidiPlayerGlobal.ErrorDetail(ex);
                    }
                }
                //else
                //    Debug.LogWarning(MidiPlayerGlobal.ErrorNoMidiFile);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private void SetAttributes()
        {
            if (miditoload != null)
            {
                MPTK_InitialTempo = miditoload.MPTK_InitialTempo;
                MPTK_Duration = miditoload.MPTK_Duration;
                MPTK_TickLast = miditoload.MPTK_TickLast;
                MPTK_NumberBeatsMeasure = miditoload.MPTK_NumberBeatsMeasure;
                MPTK_NumberQuarterBeat = miditoload.MPTK_NumberQuarterBeat;
                MPTK_TimeSigNumerator = miditoload.MPTK_TimeSigNumerator;
                MPTK_TimeSigDenominator = miditoload.MPTK_TimeSigDenominator;
                MPTK_TicksInMetronomeClick = miditoload.MPTK_TicksInMetronomeClick;
                MPTK_No32ndNotesInQuarterNote = miditoload.MPTK_No32ndNotesInQuarterNote;
                MPTK_MicrosecondsPerQuarterNote = miditoload.MPTK_MicrosecondsPerQuarterNote;
                MPTK_DeltaTicksPerQuarterNote = miditoload.MPTK_DeltaTicksPerQuarterNote;
                MPTK_TrackCount = miditoload.MPTK_TrackCount;
            }
        }
        /// <summary>
        /// Read the list of midi events available in the Midi from a ticks position to an end position.
        /// </summary>
        /// <param name="fromTicks">ticks start</param>
        /// <param name="toTicks">ticks end</param>
        /// <returns></returns>
        public List<MPTKEvent> MPTK_ReadMidiEvents(long fromTicks = 0, long toTicks = long.MaxValue)
        {
            if (miditoload == null)
            {
                NoMidiLoaded("MPTK_ReadMidiEvents");
                return null;
            }
            miditoload.LogEvents = MPTK_LogEvents;
            miditoload.KeepNoteOff = MPTK_KeepNoteOff;
            miditoload.EnableChangeTempo = MPTK_EnableChangeTempo;
            return miditoload.MPTK_ReadMidiEvents(fromTicks, toTicks);
        }

        private void NoMidiLoaded(string action)
        {
            Debug.LogWarning(string.Format("No Midi loaded, {0} canceled", action));
        }
        /// <summary>
        /// Read next Midi from the list of midi defined in MPTK (see Unity menu Midi)
        /// </summary>
        public void MPTK_Next()
        {
            try
            {
                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                {
                    int selectedMidi = 0;
                    //Debug.Log("Next search " + MPTK_MidiName);
                    if (!string.IsNullOrEmpty(MPTK_MidiName))
                        selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == MPTK_MidiName);
                    if (selectedMidi >= 0)
                    {
                        selectedMidi++;
                        if (selectedMidi >= MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count)
                            selectedMidi = 0;
                        MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[selectedMidi];
                        //Debug.Log("Next found " + MPTK_MidiName);
                    }
                }
                else
                    Debug.LogWarning(MidiPlayerGlobal.ErrorNoMidiFile);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Read previous Midi from the list of midi defined in MPTK (see Unity menu Midi)
        /// </summary>
        public void MPTK_Previous()
        {
            try
            {
                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                {
                    int selectedMidi = 0;
                    if (!string.IsNullOrEmpty(MPTK_MidiName))
                        selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == MPTK_MidiName);
                    if (selectedMidi >= 0)
                    {
                        selectedMidi--;
                        if (selectedMidi < 0)
                            selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count - 1;
                        MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[selectedMidi];
                    }
                }
                else
                    Debug.LogWarning(MidiPlayerGlobal.ErrorNoMidiFile);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Return note length as https://en.wikipedia.org/wiki/Note_value 
        /// </summary>
        /// <param name="note"></param>
        /// <returns>MPTKEvent.EnumLength</returns>
        public MPTKEvent.EnumLength MPTK_NoteLength(MPTKEvent note)
        {
            if (miditoload != null)
                return miditoload.NoteLength(note);
            else
                NoMidiLoaded("MPTK_NoteLength");
            return MPTKEvent.EnumLength.Sixteenth;
        }
    }
}

