#define DEBUG_START_MIDIx
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
    /// This class, associated to the prefab MidiFilePlayer, is  able to play music from Midi file.\n 
    /// Midi files must be added from the Unity menu MPTK in the Unity editor.\n
    /// There is no need to writing a script. For a simple usage, all the job can be done in the prefab inspector.\n
    /// For more information see here https://paxstellar.fr/midi-file-player-detailed-view-2/\n
    /// But to more specific interaction, this class can be useful. See example below wich select randomly a Midi to play.
    ///! @code
    ///     // Example of script. See TestMidiFilePlayerScripting.cs for a more detailed usage.
    ///
    ///     // Need a reference to the prefab MidiFilePlayer you have added in your scene hierarchy.
    ///     MidiFilePlayer midiFilePlayer = FindObjectOfType<MidiFilePlayer>();
    /// 
    ///     // Random select for the Midi
    ///     int index = UnityEngine.Random.Range(0, MidiPlayerGlobal.MPTK_ListMidi.Count);
    ///     midiFilePlayer.MPTK_MidiIndex = index;
    /// 
    ///     // Play! How make more simple?
    ///     midiFilePlayer.MPTK_Play();
    /// 
    ///! @endcode
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [HelpURL("https://paxstellar.fr/midi-file-player-detailed-view-2/")]
    public partial class MidiFilePlayer : MidiSynth
    {
        /// <summary>
        /// Select a Midi to play ny its name.\n
        /// Use the exact name as seen in the Midi setup windows (Unity menu MPTK/ without any path or extension.\n
        /// Tips: Add Midi files to your project with the Unity menu MPTK.
        ///! @code
        /// midiFilePlayer.MPTK_MidiName = "Albinoni - Adagio";
        /// midiFilePlayer.MPTK_Play();
        ///! @endcode
        /// </summary>
        virtual public string MPTK_MidiName
        {
            get
            {
                return midiNameToPlay;
            }
            set
            {
                midiIndexToPlay = MidiPlayerGlobal.MPTK_FindMidi(value);
                midiNameToPlay = value;
            }
        }
        [SerializeField]
        [HideInInspector]
        protected string midiNameToPlay;

        /// <summary>
        /// Select a Midi file to play by its Index.\n
        /// The Index of a Midi file is displayed in the popup from the MidiFilePlayer inspector and in the window "Midi File Setup" from the MPTK menu in the editor.\n
        ///! @code
        /// midiFilePlayer.MPTK_MidiIndex = 33;
        /// midiFilePlayer.MPTK_Play();
        ///! @endcode        
        /// </summary>
        /// <param name="index">Index of the Midi, start from 0</param>
        public int MPTK_MidiIndex
        {
            get
            {
                return midiIndexToPlay;
            }
            set
            {
                ///! @code
                /// midiFilePlayer.MPTK_MidiIndex = 1;
                ///! @endcode
                try
                {
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
        /// Should the Midi start playing when application starts ?
        /// </summary>
        [HideInInspector]
        public bool MPTK_PlayOnStart { get { return playOnStart; } set { playOnStart = value; } }

        /// <summary>
        /// Should the midi start playing at the first note found ? 
        /// </summary>
        [HideInInspector]
        public bool MPTK_StartPlayAtFirstNote;

        /// <summary>
        /// Should automatically restart playing when Midi reaches the end ? The midi doesn't need to be reload.
        /// </summary>
        [HideInInspector]
        public bool MPTK_Loop { get { return loop; } set { loop = value; } }

        /// <summary>
        /// Get the current tempo from the Midi file (independent from MPTK_Speed). 
        /// Return QuarterPerMinuteValue similar to BPM (Beat Per Measure)
        /// </summary>
        public double MPTK_Tempo { get { if (midiLoaded != null) return midiLoaded.MPTK_CurrentTempo; else return 0d; } }


        public string MPTK_SequenceTrackName { get { return midiLoaded != null ? midiLoaded.SequenceTrackName : ""; } }
        public string MPTK_ProgramName { get { return midiLoaded != null ? midiLoaded.ProgramName : ""; } }
        public string MPTK_TrackInstrumentName { get { return midiLoaded != null ? midiLoaded.TrackInstrumentName : ""; } }
        public string MPTK_TextEvent { get { return midiLoaded != null ? midiLoaded.TextEvent : ""; } }
        public string MPTK_Copyright { get { return midiLoaded != null ? midiLoaded.Copyright : ""; } }

        /// <summary>
        /// Speed of playing. 
        /// Between 0.1 (10%) to 10 (1000%).<BR>
        /// Set to 1 for normal speed. 
        /// </summary>
        public float MPTK_Speed
        {
            get
            {
                //Debug.Log("get speed " + speed );
                return speed;
            }
            set
            {
                try
                {
                    if (value != speed)
                    {
                        //Debug.Log("set speed " + value);
                        if (value >= 0.1f && value <= 10f)
                        {
                            speed = value;
                            if (midiLoaded != null)
                                midiLoaded.ChangeSpeed(speed);
                        }
                        else
                            Debug.LogWarning("MidiFilePlayer - Set Speed value not valid : " + value);
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        /// <summary>
        /// Set or get the current position in the Midi when playing in milliseconds. 
        /// Warning: if you want to set the start position, set MPTK_Position inside the processing of the event OnEventStartPlayMidi 
        /// because MPTK_Play() reset the start position to 0.
        /// Other possibility to change the position in the Midi is to use the property MPTK_TickCurrent: set or get the position in tick 
        /// More info here https://paxstellar.fr/2020/09/11/midi-timing/
        ///! @code
        /// Move forward one second
        /// midiFilePlayer.MPTK_Position = midiFilePlayer.MPTK_Position + 1000d;
        ///! @endcode
        /// </summary>
        public double MPTK_Position
        {
            get
            {
                // V2.88 return midiLoaded != null ? midiLoaded.MPTK_ConvertTickToTime(MPTK_TickCurrent) : 0;
                return MPTK_LastEventPlayed != null ? MPTK_LastEventPlayed.RealTime : 0;
            }
            set
            {
                try
                {
                    if (midiLoaded != null)
                    {
                        midiLoaded.fluid_player_seek((int)midiLoaded.MPTK_SearchTickFromTime(value));
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private float speed = 1f;

        /// <summary>
        /// Is Midi file playing is paused ?
        /// </summary>
        public bool MPTK_IsPaused { get { return playPause; } }

        /// <summary>
        /// Is Midi file is playing ?
        /// </summary>
        public bool MPTK_IsPlaying { get { return midiIsPlaying; } }

        /// <summary>
        /// Get detailed information about the midi playing. This readonly properties is available only when a Midi is playing.<BR> 
        /// Rather use the method MPTK_Load() to get information about a Midi before playing. V2.82.
        /// </summary>
        public MidiLoad MPTK_MidiLoaded { get { return midiLoaded; } }


        //! @cond NODOC
        /// <summary>
        /// Value updated only when playing in Unity (for inspector refresh)
        /// </summary>
        public string durationEditorModeOnly;
        //! @endcond

        /// <summary>
        /// Duration (TimeSpan) of the midi.
        /// </summary>
        public TimeSpan MPTK_Duration { get { try { if (midiLoaded != null) return midiLoaded.MPTK_Duration; } catch (System.Exception ex) { MidiPlayerGlobal.ErrorDetail(ex); } return TimeSpan.Zero; } }

        /// <summary>
        /// Duration (milliseconds) of the midi. 
        /// </summary>
        public float MPTK_DurationMS { get { try { if (midiLoaded != null) return midiLoaded.MPTK_DurationMS; } catch (System.Exception ex) { MidiPlayerGlobal.ErrorDetail(ex); } return 0f; } }

        /// <summary>
        /// Last tick position in Midi: Value of the tick for the last midi event in sequence expressed in number of "ticks". MPTK_TickLast / MPTK_DeltaTicksPerQuarterNote equal the duration time of a quarter-note regardless the defined tempo.
        /// </summary>
        public long MPTK_TickLast { get { return midiLoaded != null ? midiLoaded.MPTK_TickLast : 0; } }

        /// <summary>
        /// Tick position for the first note found
        /// </summary>
        public long MPTK_TickFirstNote { get { return midiLoaded != null ? midiLoaded.MPTK_TickFirstNote : 0; } }

        /// <summary>
        /// Position in millisecond for the first note found
        /// </summary>
        public double MPTK_PositionFirstNote { get { return midiLoaded != null ? midiLoaded.MPTK_PositionFirstNote : 0; } }

        /// <summary>
        /// Count of track read in the Midi file
        /// </summary>
        public int MPTK_TrackCount { get { return midiLoaded != null ? midiLoaded.MPTK_TrackCount : 0; } }


        /// <summary>
        /// Set or get the current tick position in the Midi when playing. 
        /// Midi tick is an easy way to identify a position in a song independently of the time which could vary with tempo change. 
        /// The count of ticks for a quarter is constant all along a Midi: see properties MPTK_DeltaTicksPerQuarterNote. 
        /// Example: with a time signature of 4/4 the ticks length of a bar is 4 * MPTK_DeltaTicksPerQuarterNote.
        /// Warning: if you want to set the start position, set MPTK_TickCurrent inside the processing of the event OnEventStartPlayMidi 
        /// because MPTK_Play() reset the start position to 0.
        /// Other possibility to change the position in the Midi is to use the property MPTK_Position: set or get the position in milliseconds 
        /// but tempo change event will impact also this time.
        /// More info here https://paxstellar.fr/2020/09/11/midi-timing/
        ///! @code
        /// Move forward one quarter
        /// midiFilePlayer.MPTK_TickCurrent = midiFilePlayer.MPTK_TickCurrent + midiFilePlayer.MPTK_DeltaTicksPerQuarterNote;
        ///! @endcode
        /// </summary>
        public long MPTK_TickCurrent
        {
            get
            {
                return midiLoaded != null ? midiLoaded.MPTK_TickCurrent : 0;
            }
            set
            {
                try
                {
                    if (midiLoaded != null)
                    {
                        //Debug.Log("Set MPTK_TickCurrent:" + value);

                        long position = value;
                        if (position < 0) position = 0;
                        if (position > MPTK_TickLast) position = MPTK_TickLast;
                        //MPTK_Position = miditoplay.MPTK_ConvertTickToTime(position);
                        midiLoaded.fluid_player_seek((int)position);
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        /// <summary>
        /// Last Midi event read or played
        /// </summary>
        public MPTKEvent MPTK_LastEventPlayed
        {
            get
            {
                return midiLoaded?.MPTK_LastEventPlayed;
            }
        }

        /// <summary>
        /// Lenght in millisecond of a quarter. Obviously depends on the current tempo.
        /// </summary>
        public double MPTK_PulseLenght { get { try { if (midiLoaded != null) return midiLoaded.MPTK_PulseLenght; } catch (System.Exception ex) { MidiPlayerGlobal.ErrorDetail(ex); } return 0d; } }

        //! @cond NODOC
        /// <summary>
        /// Updated only when playing in Unity (for inspector refresh)
        /// </summary>
        public string playTimeEditorModeOnly;
        //! @endcond

        /// <summary>
        /// Time from the start of playing the current midi
        /// </summary>
        public TimeSpan MPTK_PlayTime { get { try { return TimeSpan.FromMilliseconds(timeMidiFromStartPlay); } catch (System.Exception ex) { MidiPlayerGlobal.ErrorDetail(ex); } return TimeSpan.Zero; } }

        /// <summary>
        /// Log midi events
        /// </summary>
        public bool MPTK_LogEvents
        {
            get { return logEvents; }
            set { logEvents = value; }
        }

        /// <summary>
        /// Should keep note off event Events from the Midi file ? 
        /// </summary>
        public bool MPTK_KeepNoteOff
        {
            get { return keepNoteOff; }
            set { keepNoteOff = value; }
        }

        /// <summary>
        /// Status of the last midi loaded. The status is updated in a coroutine, so the status can change at each frame.
        /// </summary>
        public LoadingStatusMidiEnum MPTK_StatusLastMidiLoaded;


        /// <summary>
        /// Define unity event to trigger when notes available from the Midi file.
        ///! @code
        /// MidiFilePlayer midiFilePlayer = FindObjectOfType<MidiFilePlayer>(); 
        ///         ...
        /// if (!midiFilePlayer.OnEventNotesMidi.HasEvent())
        /// {
        ///    // No listener defined, set now by script. NotesToPlay will be called for each new notes read from Midi file
        ///    midiFilePlayer.OnEventNotesMidi.AddListener(NotesToPlay);
        /// }
        ///         ...
        /// public void NotesToPlay(List<MPTKEvent> notes)
        /// {
        ///    Debug.Log(notes.Count);
        ///    foreach (MPTKEvent midievent in notes)
        ///    {
        ///         ...
        ///    }
        /// }
        ///! @endcode
        /// </summary>
        [HideInInspector]
        public EventNotesMidiClass OnEventNotesMidi;


        /// <summary>
        /// Define unity event to trigger at start of playing the Midi.
        ///! @code
        ///! MidiFilePlayer midiFilePlayer = FindObjectOfType<MidiFilePlayer>();
        ///         ...
        /// if (!midiFilePlayer.OnEventStartPlayMidi.HasEvent())
        /// {
        ///    // No listener defined, set now by script. StartPlay will be called.
        ///    midiFilePlayer.OnEventStartPlayMidi.AddListener(StartPlay);
        /// }
        ///         ...
        /// public void StartPlay(string midiname)
        /// {
        ///    Debug.LogFormat("Start playing midi {0}", midiname);
        /// }
        ///! @endcode
        ///! </summary>
        [HideInInspector]
        public EventStartMidiClass OnEventStartPlayMidi;

        /// <summary>
        /// Define unity event to trigger at end of playing the midi.
        ///! @code
        /// MidiFilePlayer midiFilePlayer = FindObjectOfType<MidiFilePlayer>();
        ///         ...
        /// if (!midiFilePlayer.OnEventEndPlayMidi.HasEvent())
        /// {
        ///    // No listener defined, set now by script. EndPlay will be called.
        ///    midiFilePlayer.OnEventEndPlayMidi.AddListener(EndPlay);
        /// }
        ///         ...
        /// public void EndPlay(string midiname, EventEndMidiEnum reason)
        /// {
        ///    Debug.LogFormat("End playing midi {0} reason:{1}", midiname, reason);
        /// }
        ///! @endcode
        /// </summary>
        [HideInInspector]
        public EventEndMidiClass OnEventEndPlayMidi;

        /// <summary>
        /// Level of quantization : 
        ///! @li @c     0 = None 
        ///! @li @c     1 = Quarter Note
        ///! @li @c     2 = Eighth Note
        ///! @li @c     3 = 16th Note
        ///! @li @c     4 = 32th Note
        ///! @li @c     5 = 64th Note
        ///! @li @c     6 = 128th Note
        /// </summary>
        public int MPTK_Quantization
        {
            get { return quantization; }
            set
            {
                try
                {
                    if (value >= 0 && value <= 6)
                    {
                        quantization = value;
                        midiLoaded.ChangeQuantization(quantization);
                    }
                    else
                        Debug.LogWarning("MidiFilePlayer - Set Quantization value not valid : " + value);
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }
        [SerializeField]
        [HideInInspector]
        private int quantization = 0;


        [SerializeField]
        [HideInInspector]
        private bool playOnStart = false, replayMidi = false, stopMidi = false,
            midiIsPlaying = false, loop = false,
            logEvents = false, keepNoteOff = false, needDelayToStart = false, needDelayToStop = false /*v2.84*/ ;

        //private float delayToStopMilliseconds = 100f;

        private float timeAtNeedToStopSecond = 0f;
        private float delayNeedToStopSecond = 0f;

        [SerializeField]
        [HideInInspector]
        public bool nextMidi = false, prevMidi = false;

        //[SerializeField]
        //[HideInInspector]
        //protected bool playPause = false;

        [Range(0, 100)]
        private float delayMilliSeconde = 15f;  // only with AudioSource mode (non core)

        private double lastMidiTimePlayAS = 0d;
        protected double timeAtStartMidi = 0d;

        /// <summary>
        /// [DEPRECATED] Get all the raw midi events available in the midi file.\n
        /// Use rather the class MidiLoad.
        ///! @code
        /// MidiLoad MidiLoaded = new MidiLoad();
        /// MidiLoaded.MPTK_Load(midiindex);
        /// List<MPTKEvent> events = MidiLoaded.MPTK_ReadMidiEvents();
        ///! @endcode
        /// </summary>
        public List<TrackMidiEvent> MPTK_MidiEvents
        {
            get
            {
                return midiLoaded != null ? midiLoaded.MPTK_MidiEvents : null;
            }
        }

        /// <summary>
        /// Delta Ticks Per Quarter Note. Indicate the duration time in "ticks" which make up a quarter-note.\n 
        /// For instance, if 96, then a duration of an eighth-note in the file would be 48.\n
        /// More info here https://paxstellar.fr/2020/09/11/midi-timing/\n
        ///! @code
        /// Move forward one quarter
        /// midiFilePlayer.MPTK_TickCurrent = midiFilePlayer.MPTK_TickCurrent + midiFilePlayer.MPTK_DeltaTicksPerQuarterNote;
        ///! @endcode
        /// </summary>
        public int MPTK_DeltaTicksPerQuarterNote
        {
            get
            {
                int DeltaTicksPerQuarterNote = 0;
                try
                {
                    DeltaTicksPerQuarterNote = midiLoaded.MPTK_DeltaTicksPerQuarterNote;
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
                return DeltaTicksPerQuarterNote;
            }
        }

        new void Awake()
        {
            //Debug.Log("Awake MidiFilePlayer midiIsPlaying:" + midiIsPlaying);
            AwakeMidiFilePlayer();
        }

        protected void AwakeMidiFilePlayer()
        {
            //Debug.Log("AwakeMidiFilePlayer MidiFilePlayer midiIsPlaying:" + midiIsPlaying);
            midiIsPlaying = false;
            //midiFilter= GetComponent<MidiFilter>();
            //if (midiFilter == null)
            //    Debug.Log("no midiFilter");
            //else
            //    Debug.Log("midiFilter " /*+ midiFilter.Tracks.Count*/);
            base.Awake();
        }

        new void Start()
        {
            //Debug.Log("Start MidiFilePlayer midiIsPlaying:" + midiIsPlaying + " MPTK_PlayOnStart:" + MPTK_PlayOnStart);
            StartMidiFilePlayer();
        }

        protected void StartMidiFilePlayer()
        {
            //Debug.Log("StartMidiFilePlayer MidiFilePlayer midiIsPlaying:" + midiIsPlaying + " MPTK_PlayOnStart:" + MPTK_PlayOnStart);
            base.Start();
            try
            {
                //Debug.Log("   midiIsPlaying:" + midiIsPlaying + " MPTK_PlayOnStart:" + MPTK_PlayOnStart);
                if (MPTK_PlayOnStart)
                {
                    Routine.RunCoroutine(TheadPlayIfReady(), Segment.RealtimeUpdate);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private void OnDestroy()
        {
            //Debug.Log("OnDestroy " + Time.time + " seconds");
            // MPTK_Stop(); this call launch a coroutine, not a good idea when scene is closing!
            // Extract of MPTK_Stop:
            if (midiLoaded != null)
            {
                midiLoaded.ReadyToPlay = false;
                midiIsPlaying = false;
                playPause = false;
                stopMidi = true;
            }
            MPTK_StopSynth();
        }

        void OnApplicationQuit()
        {
            //Debug.Log("OnApplicationQuit " + Time.time + " seconds");
            MPTK_Stop();
            MPTK_StopSynth();
        }

        private void OnApplicationPause(bool pause)
        {
            //Debug.Log("MidiFilePlayer OnApplicationPause " + pause);
            if (pause && MPTK_PauseOnFocusLoss)
                watchMidi.Stop();
            else if (!watchMidi.IsRunning)
                watchMidi.Start();
        }

        void OnApplicationFocus(bool hasFocus)
        {
            //Debug.Log("MidiFilePlayer OnApplicationFocus " + hasFocus);
            if (!hasFocus && MPTK_PauseOnFocusLoss)
                watchMidi.Stop();
            else if (!watchMidi.IsRunning)
                watchMidi.Start();
        }

        protected IEnumerator<float> TheadPlayIfReady()
        {
            while (!MidiPlayerGlobal.MPTK_SoundFontLoaded)
                yield return Routine.WaitForSeconds(0.2f);

            // Wait a few of millisecond to let app to start (usefull when play on start)
            yield return Routine.WaitForSeconds(0.2f);

            MPTK_Play();
        }

        /// <summary>
        /// Play the midi file defined with MPTK_MidiName or MPTK_MidiIndex
        /// </summary>
        public virtual void MPTK_Play()
        {
            try
            {
                // V2.82 removed from here
                //MPTK_InitSynth();
                //MPTK_StartSequencerMidi();

                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    // V2.82 playPause = false; UnPause if paused
                    if (MPTK_IsPaused)
                        MPTK_UnPause();
                    else if (!MPTK_IsPlaying)
                    {
                        // V2.82 moved here
                        MPTK_InitSynth();
                        MPTK_StartSequencerMidi();

                        // Load description of available soundfont
                        if (MidiPlayerGlobal.ImSFCurrent != null && MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                        {
                            if (VerboseSynth) Debug.Log(MPTK_MidiName);
                            if (string.IsNullOrEmpty(MPTK_MidiName))
                                MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[0];
                            int selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == MPTK_MidiName);
                            if (selectedMidi < 0)
                            {
                                Debug.LogWarning("MidiFilePlayer - MidiFile " + MPTK_MidiName + " not found. Trying with the first in list.");
                                selectedMidi = 0;
                                MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[0];
                            }

                            if (MPTK_CorePlayer)
                                Routine.RunCoroutine(ThreadCorePlay().CancelWith(gameObject), Segment.RealtimeUpdate);
                            else
                                Routine.RunCoroutine(ThreadPlay(null).CancelWith(gameObject), Segment.RealtimeUpdate);
                        }
                        else
                            Debug.LogWarning(MidiPlayerGlobal.ErrorNoMidiFile);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Stop playing
        /// </summary>
        public void MPTK_Stop()
        {
            if (midiLoaded != null)
            {
                midiLoaded.ReadyToPlay = false;
                midiIsPlaying = false;
                playPause = false;
                stopMidi = true;
            }
            Routine.RunCoroutine(ThreadClearAllSound(true, IdSession), Segment.RealtimeUpdate);
        }

        /// <summary>
        /// Restart playing of the current midi file
        /// </summary>
        public void MPTK_RePlay()
        {
            try
            {
                playPause = false;
                if (midiIsPlaying)
                {
                    ThreadClearAllSound(true, IdSession);
                    replayMidi = true;
                }
                else
                    MPTK_Play();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Should the Midi playing must be paused when the application lost the focus?
        /// </summary>
        [HideInInspector]
        public bool MPTK_PauseOnFocusLoss;

        /// <summary>
        /// Pause the current playing
        /// </summary>
        /// <param name="timeToPauseMS">time to pause in milliseconds. default or < 0 : indefinitely</param>
        public void MPTK_Pause(float timeToPauseMS = -1f)
        {
            try
            {
                if (MPTK_CorePlayer && timeToPauseMS > 0f)
                {
                    // Pause with no time limit
                    pauseMidi.Reset();
                    pauseMidi.Start();
                }
                timeToPauseMilliSeconde = timeToPauseMS;
                watchMidi.Stop();
                playPause = true;
                Routine.RunCoroutine(ThreadClearAllSound(), Segment.RealtimeUpdate);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// UnPause the current playing
        /// </summary>
        public void MPTK_UnPause()
        {
            try
            {
                if (MPTK_CorePlayer)
                {
                    if (timeMidiFromStartPlay <= 0d) watchMidi.Reset(); // V2.82
                    watchMidi.Start();
                    playPause = false;
                }
                else
                {
                    playPause = false;
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play next Midi from the list of midi defined in MPTK (see Unity menu Midi)
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
                        nextMidi = true;
                        MPTK_RePlay();
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
        /// Play previous Midi from the list of midi defined in MPTK (see Unity menu Midi)
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
                        prevMidi = true;
                        MPTK_RePlay();
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
            if (midiLoaded != null)
                return midiLoaded.NoteLength(note);
            return MPTKEvent.EnumLength.Sixteenth;
        }

        /// <summary>
        /// Load the midi file defined with MPTK_MidiName or MPTK_MidiIndex. It's an optional action before playing a midi file witk MPTK_Play()<BR>
        /// Use this method to get all Midi events before start playing.
        ///! @code
        /// private void GetMidiInfo()
        /// {
        ///    MidiLoad midiloaded = midiFilePlayer.MPTK_Load();
        ///    if (midiloaded != null)
        ///    {
        ///       infoMidi = "Duration: " + midiloaded.MPTK_Duration.TotalSeconds + " seconds\n";
        ///       infoMidi += "Tempo: " + midiloaded.MPTK_InitialTempo + "\n";
        ///       List<MPTKEvent> listEvents = midiloaded.MPTK_ReadMidiEvents();
        ///       infoMidi += "Count Midi Events: " + listEvents.Count + "\n";
        ///       Debug.Log(infoMidi);
        ///    }
        /// }
        ///! @endcode        
        /// </summary>        
        /// <returns>MidiLoad to access all the properties of the midi loaded</returns>
        public MidiLoad MPTK_Load()
        {
            MidiLoad miditoload = new MidiLoad();

            if (string.IsNullOrEmpty(MPTK_MidiName))
            {
                Debug.LogWarning("MPTK_Load: midi name not defined");
                return null;
            }

            TextAsset mididata = Resources.Load<TextAsset>(System.IO.Path.Combine(MidiPlayerGlobal.MidiFilesDB, MPTK_MidiName));
            if (mididata == null || mididata.bytes == null || mididata.bytes.Length == 0)
            {
                Debug.LogWarning("MPTK_Load: error when loading midi " + MPTK_MidiName);
                return null;
            }

            miditoload.KeepNoteOff = false;
            miditoload.LogEvents = MPTK_LogEvents;
            miditoload.MPTK_Load(mididata.bytes);

            return miditoload;
        }
        //protected IEnumerator<float> TestFrameDelay()
        //{
        //    double deltaTime = 0;
        //    do
        //    {
        //        deltaTime = (Time.realtimeSinceStartup - lastTimePlay) * 1000d;
        //        timeFromStartPlay += deltaTime;
        //        Debug.Log("   deltaTime:" + Math.Round(deltaTime, 3));

        //        lastTimePlay = Time.realtimeSinceStartup;

        //        if (stopMidi)
        //        {
        //            break;
        //        }

        //        if (delayMilliSeconde > 0)
        //            yield return Timing.WaitForSeconds(delayMilliSeconde / 1000F);
        //        else
        //            yield return -1;

        //    }
        //    while (true);
        //}

        //! @cond NODOC

        /// <summary>
        /// Read and play Midi event from the Unity Main Thread
        /// </summary>
        /// <param name="midiBytesToPlay"></param>
        /// <returns></returns>
        /*protected */
        public IEnumerator<float> ThreadPlay(byte[] midiBytesToPlay = null, float fromPosition = 0, float toPosition = 0)
        {
            double deltaTime = 0;
            midiIsPlaying = true;
            stopMidi = false;
            replayMidi = false;
            bool first = true;
            string currentMidiName = "";
            //Debug.Log("Start play");
            try
            {
                midiLoaded = new MidiLoad();

                // No midi byte array, try to load from MidiFilesDN from resource
                if (midiBytesToPlay == null || midiBytesToPlay.Length == 0)
                {
                    currentMidiName = MPTK_MidiName;
                    TextAsset mididata = Resources.Load<TextAsset>(System.IO.Path.Combine(MidiPlayerGlobal.MidiFilesDB, currentMidiName));
                    midiBytesToPlay = mididata.bytes;
                }

                midiLoaded.KeepNoteOff = MPTK_KeepNoteOff;
                midiLoaded.EnableChangeTempo = MPTK_EnableChangeTempo;
                midiLoaded.MPTK_Load(midiBytesToPlay);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }

            if (midiLoaded != null)
            {
                // Clear all sound from a previous midi
                yield return Routine.WaitUntilDone(Routine.RunCoroutine(ThreadClearAllSound(true), Segment.RealtimeUpdate), false);

                try
                {
                    midiLoaded.ChangeSpeed(MPTK_Speed);
                    midiLoaded.ChangeQuantization(MPTK_Quantization);

                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }

                lastMidiTimePlayAS = Time.realtimeSinceStartup;
                timeMidiFromStartPlay = fromPosition;

                //if (MPTK_Spatialize)
                SetSpatialization();
                //else MPTK_MaxDistance = 500;

                MPTK_ResetStat();

                timeAtStartMidi = (System.DateTime.UtcNow.Ticks / 10000D);
                ResetMidi();

                // Call Event StartPlayMidi
                try
                {
                    OnEventStartPlayMidi.Invoke(currentMidiName);
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }

                //
                // Read and play Midi event from the Unity Main Thread
                // --------------------------------------------------
                do
                {
                    midiLoaded.LogEvents = MPTK_LogEvents;

                    if (MPTK_Spatialize)
                    {
                        distanceToListener = MidiPlayerGlobal.MPTK_DistanceToListener(this.transform);
                        if (distanceToListener > MPTK_MaxDistance)
                        {
                            playPause = true;
                            timeToPauseMilliSeconde = -1f;
                        }
                        else
                            playPause = false;
                    }

                    if (playPause)
                    {
                        deltaTime = (Time.realtimeSinceStartup - lastMidiTimePlayAS) * 1000d;
                        lastMidiTimePlayAS = Time.realtimeSinceStartup;
                        //Debug.Log("pause " + timeToPauseMilliSeconde+ " " + deltaTime);
                        yield return Routine.WaitForSeconds(0.2f);
                        if (midiLoaded.EndMidiEvent || replayMidi || stopMidi)
                        {
                            break;
                        }
                        if (timeToPauseMilliSeconde > -1f)
                        {
                            timeToPauseMilliSeconde -= (float)deltaTime;
                            if (timeToPauseMilliSeconde <= 0f)
                                playPause = false;
                        }
                        continue;
                    }

                    if (!first)
                    {
                        deltaTime = (Time.realtimeSinceStartup - lastMidiTimePlayAS) * 1000d;

                        if (deltaTime < delayMilliSeconde)
                        {
                            yield return -1;
                            continue;
                        }
                        timeMidiFromStartPlay += deltaTime;
                    }
                    else
                    {
                        timeMidiFromStartPlay = fromPosition;
                        first = false;
                    }

                    lastMidiTimePlayAS = Time.realtimeSinceStartup;

                    //Debug.Log("---------------- " /*+ timeFromStartPlay */+ "   deltaTime:" + Math.Round(deltaTime, 3) /*+ "   " + System.DateTime.UtcNow.Millisecond*/);

                    // Read midi events until this time
                    List<MPTKEvent> midievents = midiLoaded.fluid_player_callback((int)timeMidiFromStartPlay, IdSession);

                    if (midiLoaded.EndMidiEvent || replayMidi || stopMidi || (toPosition > 0 && toPosition > fromPosition && MPTK_Position > toPosition))
                    {
                        break;
                    }

                    // Play notes read from the midi file
                    if (midievents != null && midievents.Count > 0)
                    {
                        // Call event with these midi events
                        try
                        {
                            if (OnEventNotesMidi != null)
                                OnEventNotesMidi.Invoke(midievents);
                        }
                        catch (System.Exception ex)
                        {
                            MidiPlayerGlobal.ErrorDetail(ex);
                        }

                        float beforePLay = Time.realtimeSinceStartup;
                        //Debug.Log("---------------- play count:" + midievents.Count);
                        if (MPTK_DirectSendToPlayer)
                        {
                            foreach (MPTKEvent midievent in midievents)
                            {
                                MPTK_PlayDirectEvent(midievent, false);
                            }
                        }
                        //Debug.Log("---------------- played count:" + midievents.Count + " Start:" + timeFromStartPlay + " Delta:" + Math.Round(deltaTime, 3) + " Elapsed:" + Math.Round((Time.realtimeSinceStartup - beforePLay) * 1000f,3));
                    }

                    if (Application.isEditor)
                    {
                        TimeSpan times = TimeSpan.FromMilliseconds(MPTK_Position);
                        playTimeEditorModeOnly = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", times.Hours, times.Minutes, times.Seconds, times.Milliseconds);
                        durationEditorModeOnly = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", MPTK_Duration.Hours, MPTK_Duration.Minutes, MPTK_Duration.Seconds, MPTK_Duration.Milliseconds);
                    }

                    if (delayMilliSeconde > 0)
                        yield return Routine.WaitForSeconds(delayMilliSeconde / 1000F);
                    else
                        yield return -1;


                }
                while (true);
            }
            else
                Debug.LogWarning("MidiFilePlayer/ThreadPlay - Midi Load error");

            midiIsPlaying = false;

            try
            {
                EventEndMidiEnum reason = EventEndMidiEnum.MidiEnd;
                if (nextMidi)
                {
                    reason = EventEndMidiEnum.Next;
                    nextMidi = false;
                }
                else if (prevMidi)
                {
                    reason = EventEndMidiEnum.Previous;
                    prevMidi = false;
                }
                else if (stopMidi)
                    reason = EventEndMidiEnum.ApiStop;
                else if (replayMidi)
                    reason = EventEndMidiEnum.Replay;
                OnEventEndPlayMidi.Invoke(currentMidiName, reason);

                if ((MPTK_Loop || replayMidi) && !stopMidi)
                    MPTK_Play();
                //stopMidiToPlay = false;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            //Debug.Log("Stop play");
        }

        public IEnumerator<float> ThreadCorePlay(byte[] midiBytesToPlay = null, float fromPosition = 0, float toPosition = 0)
        {
            StartPlaying();
            string currentMidiName = MPTK_MidiName;
            //Debug.Log("Start play " + fromPosition + " " + toPosition);
            try
            {
                midiLoaded = new MidiLoad();

                // No midi byte array, try to load from MidiFilesDN from resource
                if (midiBytesToPlay == null || midiBytesToPlay.Length == 0)
                {
                    TextAsset mididata = Resources.Load<TextAsset>(System.IO.Path.Combine(MidiPlayerGlobal.MidiFilesDB, currentMidiName));
                    midiBytesToPlay = mididata.bytes;
                }

                midiLoaded.KeepNoteOff = MPTK_KeepNoteOff;
                midiLoaded.EnableChangeTempo = MPTK_EnableChangeTempo;
                midiLoaded.LogEvents = MPTK_LogEvents;
                if (!midiLoaded.MPTK_Load(midiBytesToPlay))
                    midiLoaded = null;
#if DEBUG_START_MIDI
                Debug.Log("After load midi " + (double)watchStartMidi.ElapsedTicks / ((double)System.Diagnostics.Stopwatch.Frequency / 1000d));
#endif
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }

            Routine.RunCoroutine(ThreadInternalMidiPlaying(currentMidiName, fromPosition, toPosition).CancelWith(gameObject), Segment.RealtimeUpdate);
            yield return 0;
        }

        protected void StartPlaying()
        {
#if DEBUG_START_MIDI
            System.Diagnostics.Stopwatch watchStartMidi = new System.Diagnostics.Stopwatch();
            watchStartMidi.Start();
#endif
            midiIsPlaying = true;
            stopMidi = false;
            replayMidi = false;
            needDelayToStop = false;
        }

        protected IEnumerator<float> ThreadInternalMidiPlaying(string currentMidiName, float fromPosition = 0, float toPosition = 0)
        {
            if (midiLoaded != null)
            {
                // Clear all sound from a previous midi - v2.71 wait until all notes are stopped
                // V2.84 yield return Timing.WaitUntilDone(Timing.RunCoroutine(ThreadClearAllSound(true)), false);
                //Timing.RunCoroutine(ThreadClearAllSound(true));
                // V2.84
                Routine.RunCoroutine(ThreadClearAllSound(true, IdSession), Segment.RealtimeUpdate);

#if DEBUG_START_MIDI
                Debug.Log("After clear sound " +(double)watchStartMidi.ElapsedTicks / ((double)System.Diagnostics.Stopwatch.Frequency / 1000d));
#endif
                try
                {
                    midiLoaded.ChangeSpeed(MPTK_Speed);
                    midiLoaded.ChangeQuantization(MPTK_Quantization);

                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }

                SetSpatialization();

                MPTK_ResetStat();
                timeAtStartMidi = (System.DateTime.UtcNow.Ticks / 10000D);
                ResetMidi();

                do
                {
                    //Debug.Log(miditoplay.MPTK_TickFirstNote);
                    if (fromPosition > 0)
                        MPTK_Position = fromPosition;
                    else if (MPTK_StartPlayAtFirstNote && midiLoaded.MPTK_TickFirstNote > 0)
                        MPTK_TickCurrent = midiLoaded.MPTK_TickFirstNote;

                    // Call Event StartPlayMidi - v2.71 move after the do
                    try
                    {
                        if (SpatialSynths != null)
                            // Send to the channel synth
                            foreach (MidiFilePlayer mfp in SpatialSynths)
                                mfp.OnEventStartPlayMidi.Invoke(currentMidiName);

                        OnEventStartPlayMidi.Invoke(currentMidiName);
                    }
                    catch (System.Exception ex)
                    {
                        MidiPlayerGlobal.ErrorDetail(ex);
                    }

                    volumeStartStop = 1f;
                    IdSession++;
                    midiLoaded.ReadyToPlay = true; //miditoplay.ReadyToStarted = true; // V2.84 from below


#if DEBUG_START_MIDI
                    Debug.Log("Just before playing " + (double)watchStartMidi.ElapsedTicks / ((double)System.Diagnostics.Stopwatch.Frequency / 1000d));
#endif
                    do
                    {
                        midiLoaded.LogEvents = MPTK_LogEvents;
                        midiLoaded.EnableChangeTempo = MPTK_EnableChangeTempo;

                        if (MPTK_Spatialize)
                        {
                            distanceToListener = MidiPlayerGlobal.MPTK_DistanceToListener(this.transform);
                            if (distanceToListener > MPTK_MaxDistance)
                                MPTK_Pause();
                            else if (playPause)
                                MPTK_UnPause();
                        }

                        if (needDelayToStart && delayNeedToStopSecond > 0f)
                        {
                            float pct = (timeAtNeedToStopSecond - Time.realtimeSinceStartup) / delayNeedToStopSecond;
                            //Debug.Log($"{DateTime.UtcNow.ToLongTimeString()} {timeAtNeedToStopSecond - Time.realtimeSinceStartup} {delayNeedToStopSecond} {pct}");
                            if (pct > 0f)
                                volumeStartStop = 1 - pct; // pct start at 1 and go to 0, we need start to 0 to 1
                            else
                            {
                                needDelayToStart = false;
                            }
                        }

                        if (needDelayToStop)
                        {
                            float pct = (timeAtNeedToStopSecond - Time.realtimeSinceStartup) / delayNeedToStopSecond;
                            //Debug.Log($"{DateTime.UtcNow.ToLongTimeString()} {timeAtNeedToStopSecond - Time.realtimeSinceStartup} {delayNeedToStopSecond} {pct}");
                            if (pct > 0f)
                                volumeStartStop = pct; // pct start at 1 and go to 0
                            else
                            {
                                MPTK_Stop();
                            }
                        }

                        if (playPause)
                        {
                            //Debug.Log("paused");
                            midiLoaded.ReadyToPlay = false;
                            sequencerPause = true;
                        }
                        else
                        {
                            midiLoaded.ReadyToPlay = true;
                            sequencerPause = false;
                        }

                        if (midiLoaded.EndMidiEvent || replayMidi || stopMidi || (toPosition > 0 && toPosition > fromPosition && MPTK_Position > toPosition))
                        {
                            midiLoaded.ReadyToPlay = false;
                            break;
                        }

                        try
                        {
                            while (QueueMidiEvents != null && QueueMidiEvents.Count > 0)
                            {
                                List<MPTKEvent> midievents = QueueMidiEvents.Dequeue();
                                if (midievents != null && midievents.Count > 0)
                                {
                                    if (SpatialSynths != null)
                                    {
                                        if (midievents.Count == 1)
                                        {
                                            SpatialSynths[midievents[0].Channel].OnEventNotesMidi.Invoke(midievents);
                                        }
                                        else
                                        {
                                            // Send to the channel synth
                                            List<MPTKEvent> channelEvent = new List<MPTKEvent>();
                                            foreach (MPTKEvent midievent in midievents)
                                            {
                                                if (SpatialSynths[midievent.Channel].OnEventNotesMidi != null)
                                                {
                                                    channelEvent.Clear();
                                                    channelEvent.Add(midievent);
                                                    SpatialSynths[midievent.Channel].OnEventNotesMidi.Invoke(channelEvent);
                                                }
                                            }
                                        }
                                    }
                                    // Send also to the midi reader
                                    if (OnEventNotesMidi != null)
                                        OnEventNotesMidi.Invoke(midievents);
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            MidiPlayerGlobal.ErrorDetail(ex);
                        }

                        if (Application.isEditor)
                        {
                            TimeSpan times = TimeSpan.FromMilliseconds(MPTK_Position);
                            playTimeEditorModeOnly = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", times.Hours, times.Minutes, times.Seconds, times.Milliseconds);
                            durationEditorModeOnly = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", MPTK_Duration.Hours, MPTK_Duration.Minutes, MPTK_Duration.Seconds, MPTK_Duration.Milliseconds);
                        }
                        yield return Routine.WaitForSeconds(delayMilliSeconde / 1000F);
                    }
                    while (true);

                    yield return Routine.WaitForSeconds(delayMilliSeconde / 1000F);
                    if (MPTK_Loop)
                    {
                        midiLoaded.EndMidiEvent = false;
                        midiLoaded.ClearMetaText();
                        ResetMidi();
                        OnEventEndPlayMidi.Invoke(currentMidiName, EventEndMidiEnum.Loop);
                    }
                }
                while (MPTK_Loop && !stopMidi && !replayMidi);
            }
            else
                Debug.LogWarning("MidiFilePlayer/ThreadPlay - Midi Load error");


            midiIsPlaying = false;
            try
            {
                EventEndMidiEnum reason = EventEndMidiEnum.MidiEnd;
                if (midiLoaded == null)
                {
                    reason = EventEndMidiEnum.MidiErr;
                    MPTK_StatusLastMidiLoaded = LoadingStatusMidiEnum.MidiFileInvalid;
                }
                else if (nextMidi)
                {
                    reason = EventEndMidiEnum.Next;
                    nextMidi = false;
                }
                else if (prevMidi)
                {
                    reason = EventEndMidiEnum.Previous;
                    prevMidi = false;
                }
                else if (stopMidi)
                    reason = EventEndMidiEnum.ApiStop;
                else if (replayMidi)
                    reason = EventEndMidiEnum.Replay;

                if (SpatialSynths != null)
                    // Send to the channel synth
                    foreach (MidiFilePlayer mfp in SpatialSynths)
                        mfp.OnEventEndPlayMidi.Invoke(currentMidiName, reason);

                try
                {
                    OnEventEndPlayMidi.Invoke(currentMidiName, reason);

                }
                catch (Exception)
                {

                    throw;
                }
                if (replayMidi && !stopMidi) MPTK_Play();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        //! @endcond
    }
}

