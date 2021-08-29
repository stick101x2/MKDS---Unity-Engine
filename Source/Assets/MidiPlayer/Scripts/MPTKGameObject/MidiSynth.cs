//#define MPTK_PRO
//#define DEBUG_PERF_NOTEON // warning: generate heavy cpu use
//#define DEBUG_PERF_AUDIO 
//#define DEBUG_PERF_MIDI
//#define DEBUG_STATUS_STAT // also in HelperDemo.cs

#if UNITY_IOS
#define CANT_CHANGE_AUDIO_CONFIG
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;
using MEC;
using System.Runtime.InteropServices;
using System.Threading;

#if MPTK_PRO && UNITY_ANDROID && UNITY_OBOE
using Oboe.Stream;
#endif

namespace MidiPlayerTK
{
    public enum fluid_loop
    {
        FLUID_UNLOOPED = 0,
        FLUID_LOOP_DURING_RELEASE = 1,
        FLUID_NOTUSED = 2,
        FLUID_LOOP_UNTIL_RELEASE = 3
    }

    public enum fluid_synth_status
    {
        FLUID_SYNTH_CLEAN,
        FLUID_SYNTH_PLAYING,
        FLUID_SYNTH_QUIET,
        FLUID_SYNTH_STOPPED
    }

    // Flags to choose the interpolation method 
    public enum fluid_interp
    {
        None, // no interpolation: Fastest, but questionable audio quality
        Linear, // Straight-line interpolation: A bit slower, reasonable audio quality
        Cubic, // Fourth-order interpolation: Requires 50 % of the whole DSP processing time, good quality 
        Order7,
    }

    /// <summary>
    /// Contains all the functions to build a wave table synth: load SoundFont and samples, process midi event, play voices, controllers, generators ...\n 
    /// This class is inherited by others class to build these prefabs: MidiStreamPlayer, MidiFilePlayer, MidiInReader.\n
    /// It is not recommended to instanciate directly this class, rather add the prefabs to the hierarchy of yours scenes.
    /// </summary>
#if MPTK_PRO && UNITY_ANDROID && UNITY_OBOE
    public partial class MidiSynth : MonoBehaviour, IMixerProcessor
    {
#else
    public partial class MidiSynth : MonoBehaviour
    {
#endif
        //! @cond NODOC
        //[HideInInspector] // defined at startup by script
        public AudioSource CoreAudioSource;

        [HideInInspector] // defined at startup by script
        public AudioReverbFilter ReverbFilter;

        [HideInInspector] // defined at startup by script
        public AudioChorusFilter ChorusFilter;

        /// <summary>
        /// Time in millisecond from the start of play
        /// </summary>
        protected double timeMidiFromStartPlay = 0d;

        [HideInInspector] // defined in custom inspector
        [Range(1, 30)]
        public int waitThreadMidi = 10;

        [HideInInspector] // defined in custom inspector
        [Range(1, 100)]
        public int DevicePerformance = 40;

        /// <summary>
        /// Time in millisecond for the current midi playing position
        /// </summary>
        protected double lastTimeMidi = 0d;

        public System.Diagnostics.Stopwatch watchPerfMidi;

        [HideInInspector] // defined in custom inspector
        [Range(0, 100)]
        public float MaxDspLoad = 40f;

        // has the synth module been initialized? 
        private static int lastIdSynth;
        public int IdSynth;
        public int IdSession;

        //! @endcond

        //[HideInInspector]
        /// <summary>
        /// Preset are often composed with 2 or more samples. Classically for left and right channel. Check this to play only the first sample found
        /// </summary>  
        public bool playOnlyFirstWave;

        /// <summary>
        /// Should accept change Preset for Drum canal 10 ? \n
        /// Disabled by default. Could sometimes create bad sound with midi files not really compliant with the Midi norm.
        /// </summary>
        //[HideInInspector]
        public bool MPTK_EnablePresetDrum;

        /// <summary>
        /// V2.83. If the same note is hit twice on the same channel, then the older voice process is advanced to the release stage.\n
        /// It's the default Midi processing. 
        /// </summary>
        //[HideInInspector]
        public bool MPTK_ReleaseSameNote = true;

        /// <summary>
        /// V2.83 Find the exclusive class of this voice. If set, kill all voices that match the exclusive class\n 
        /// and are younger than the first voice process created by this noteon event.
        /// </summary>
        //[HideInInspector]
        public bool MPTK_KillByExclusiveClass = true;

        /// <summary>
        /// Multiplier to increase or decrease the default release time defined in the SoundFont.\n
        /// Recommended values between 0.1 and 2. Default is 1 (no modification of the release time)
        /// </summary>
        [Tooltip("Modify the default value of the release time")]
        [Range(0.1f, 2f)]
        public float MPTK_ReleaseTimeMod = 1f;

        /// <summary>
        /// When amplitude is below this value the playing of sample is stopped (voice_off). \n
        /// Can be increase for better performance but with degraded quality because sample could be stopped earlier.
        /// Remember: Amplitude can varying between 0 and 1.
        /// </summary>
        [Range(0.01f, 0.5f)]
        [Tooltip("Sample is stopped when amplitude is below this value")]
        public float MPTK_CutOffVolume = 0.05f; //V2.872 from 0.1f to 0.05f // replace amplitude_that_reaches_noise_floor 

        /// <summary>
        /// V2.873 - A lean startup of the volume of the synth is usefull to avoid weird sound at the beguinning of the application (in some cases).\n
        /// This parameter define the speed of the increase of the volume of the audio source.\n
        /// Set to 1 for an immediate full volume at start.
        /// </summary>
        [Range(0.001f, 1f)]
        [Tooltip("Lean startup of the volume of the synth is usefull to avoid weird sound at the beguinning of the application. Set to 1 for an immediate full volume at startup.")]
        public float MPTK_LeanSynthStarting = 0.05f;

        /// <summary>
        /// Voice buffering is important to get better performance. But you can disable this fonction with this parameter.
        /// </summary>
        [Tooltip("Enable bufferring Voice to enhance performance.")]
        public bool MPTK_AutoBuffer = true;

        /// <summary>
        /// Free voices older than MPTK_AutoCleanVoiceLimit are removed when count is over than MPTK_AutoCleanVoiceTime
        /// </summary>
        [Tooltip("Auto Clean Voice Greater Than")]
        [Range(0, 1000)]
        public int MPTK_AutoCleanVoiceLimit;

        [Tooltip("Auto Clean Voice Older Than (millisecond)")]
        [Range(1000, 100000)]
        public float MPTK_AutoCleanVoiceTime;

        /// <summary>
        /// Apply real time modulatoreffect defined in the SoundFont: pitch bend, control change, enveloppe modulation
        /// </summary>
        [HideInInspector] // defined in custom inspector
        public bool MPTK_ApplyRealTimeModulator;

        /// <summary>
        /// Apply LFO effect defined in the SoundFont
        /// </summary>
        [HideInInspector] // defined in custom inspector
        public bool MPTK_ApplyModLfo;

        /// <summary>
        /// Apply vibrato effect defined in the SoundFont
        /// </summary>
        [HideInInspector] // defined in custom inspector
        public bool MPTK_ApplyVibLfo;

        //! @cond NODOC

        [Header("DSP Statistics")]
        public float StatDspLoadPCT;
        public float StatDspLoadMIN;
        public float StatDspLoadMAX;
        public float StatDspLoadAVG;

        public float StatUILatencyLAST;
        public MovingAverage StatSynthLatency;
        public float StatSynthLatencyLAST;
        public float StatSynthLatencyAVG;
        public float StatSynthLatencyMIN;
        public float StatSynthLatencyMAX;


        //public float StatDspLoadLongAVG;
        public MovingAverage StatDspLoadMA;
        //public MovingAverage StatDspLoadLongMA;

        [Header("Midi Sequencer Statistics")]

        /// <summary>
        /// Delta time in milliseconds between calls of the Midi sequencer
        /// </summary>
        public double StatDeltaThreadMidiMS = 0d;
        public double StatDeltaThreadMidiMAX;
        public double StatDeltaThreadMidiMIN;
        public float StatDeltaThreadMidiAVG;
        public MovingAverage StatDeltaThreadMidiMA;

        /// <summary>
        /// Time to read Midi Events
        /// </summary>
        public float StatReadMidiMS;

        /// <summary>
        /// Time to enqueue Midi events to the Unity thread
        /// </summary>
        public float StatEnqueueMidiMS;

        /// <summary>
        /// Time to process Midi event (create voice)
        /// </summary>
        public float StatProcessMidiMS;
        public float StatProcessMidiMAX;

        [Header("Midi Synth Statistics")]

        /// <summary>
        /// Delta time in milliseconds between call to the Midi Synth (OnAudioFilterRead). \n
        /// This value is constant during playing. Directly related to the buffer size and the synth rate values.
        /// </summary>
        public double StatDeltaAudioFilterReadMS;

        /// <summary>
        /// Time in milliseconds for the whole Midi Synth processing (OnAudioFilterRead)
        /// </summary>
        public float StatAudioFilterReadMS;
        public double StatAudioFilterReadMAX;
        public double StatAudioFilterReadMIN;
        public float StatAudioFilterReadAVG;
        public MovingAverage StatAudioFilterReadMA;

        /// <summary>
        /// Time to process samples in active list of voices
        /// </summary>
        public float StatSampleWriteMS;
        public float StatSampleWriteAVG;
        public MovingAverage StatSampleWriteMA;
        /// <summary>
        /// Time to process active and free voices
        /// </summary>
        public float StatProcessListMS;
        public float StatProcessListAVG;
        public MovingAverage StatProcessListMA;

#if DEBUG_PERF_AUDIO
        private System.Diagnostics.Stopwatch watchPerfAudio = new System.Diagnostics.Stopwatch(); // High resolution time
#endif

#if DEBUG_STATUS_STAT
        [Header("Voice Status Count Clean / On / Sustain / Off / Release")]
        /// <summary>
        /// Voice Status Count 
        /// </summary>
        public int[] StatusStat;
#endif

        /// <summary>
        /// Time in millisecond for the last OnAudioFilter
        /// </summary>
        protected double lastTimePlayCore = 0d;

        private System.Diagnostics.Stopwatch watchOnAudioFilterRead = new System.Diagnostics.Stopwatch();

        protected System.Diagnostics.Stopwatch watchMidi = new System.Diagnostics.Stopwatch();
        protected System.Diagnostics.Stopwatch pauseMidi = new System.Diagnostics.Stopwatch();
        private long EllapseMidi;
        private Thread midiThread;

#if DEBUG_PERF_NOTEON
        private float perf_time_cumul;
        private List<string> perfs;
        private System.Diagnostics.Stopwatch watchPerfNoteOn = new System.Diagnostics.Stopwatch(); // High resolution time
#endif

        //! @endcond

        /// <summary>
        /// If true then Midi events are read and play from a dedicated thread.\n
        /// If false, MidiSynth will use AudioSource gameobjects to play sound.\n
        /// This properties must be defined before running the application from the inspector.\n
        /// The default is true.\n 
        /// Warning: The non core mode player (MPTK_CorePlayer=false) will be removed with the next major version (V3)
        /// </summary>
        [HideInInspector]
        public bool MPTK_CorePlayer;

        /// <summary>
        /// Current synth rate defined.
        /// </summary>
        public int MPTK_SynthRate
        {
            get { return (int)OutputRate; }
        }

        /// <summary>
        /// Set or Get sample rate output of the synth. -1:default, 0:24000, 1:36000, 2:48000, 3:60000, 4:72000, 5:84000, 6:96000.\n 
        /// It's better to stop playing before changing on fly to avoid bad noise.
        /// </summary>
        public int MPTK_IndexSynthRate
        {
            get { return indexSynthRate; }
            set
            {

                indexSynthRate = value;
#if CANT_CHANGE_AUDIO_CONFIG
                Debug.Log("Can't change audio configuration on this device");
#else
                if (VerboseSynth)
                    Debug.Log("MPTK_ChangeSynthRate " + indexSynthRate);
                if (indexSynthRate < 0)
                {
                    // No change
                    OnAudioConfigurationChanged(false);
                }
                else
                {
                    if (indexSynthRate > 6) indexSynthRate = 6;
                    if (CoreAudioSource != null) CoreAudioSource.Stop();
                    int sampleRate = 24000 + (indexSynthRate * 12000);
                    if (VerboseSynth)
                        Debug.Log("Change Sample Rate:" + sampleRate);
                    AudioConfiguration ac = AudioSettings.GetConfiguration();
                    ac.sampleRate = sampleRate;
                    AudioSettings.Reset(ac);
                    //Debug.Log("New OutputRate:" + OutputRate);
                    if (ActiveVoices != null)
                        for (int i = 0; i < ActiveVoices.Count; i++)
                            ActiveVoices[i].output_rate = OutputRate;
                    if (FreeVoices != null)
                        for (int i = 0; i < FreeVoices.Count; i++)
                            FreeVoices[i].output_rate = OutputRate;
                    if (CoreAudioSource != null) CoreAudioSource.Play();
                }
#endif
            }
        }

        [SerializeField]
        [HideInInspector]
        private int indexSynthRate = -1;

        private int[] tabDspBufferSize = new int[] { 64, 128, 256, 512, 1024, 2048 };

        /// <summary>
        /// Set or Get sample rate output of the synth. -1:default, 0:24000, 1:36000, 2:48000, 3:60000, 4:72000, 5:84000, 6:96000.\n 
        /// It's better to stop playing before changing on fly to avoid bad noise.
        /// </summary>
        public int MPTK_IndexSynthBuffSize
        {
            get { return indexBuffSize; }
            set
            {
                indexBuffSize = value;
#if CANT_CHANGE_AUDIO_CONFIG
                Debug.Log("Can't change audio configuration on this device");
#else
                if (VerboseSynth) Debug.Log("MPTK_IndexSynthBuffSize " + indexBuffSize);
                if (indexBuffSize < 0)
                {
                    // No change
                    OnAudioConfigurationChanged(false);
                }
                else
                {
                    if (indexBuffSize > 5) indexBuffSize = 5;
                    if (CoreAudioSource != null) CoreAudioSource.Stop();
                    int bufferSize = tabDspBufferSize[indexBuffSize];
                    if (VerboseSynth)
                        Debug.Log("Change Buffer Size:" + bufferSize);
                    AudioConfiguration ac = AudioSettings.GetConfiguration();
                    ac.dspBufferSize = bufferSize;
                    AudioSettings.Reset(ac);
                    //if (ActiveVoices != null)
                    //    for (int i = 0; i < ActiveVoices.Count; i++)
                    //        ActiveVoices[i].output_rate = OutputRate;
                    //if (FreeVoices != null)
                    //    for (int i = 0; i < FreeVoices.Count; i++)
                    //        FreeVoices[i].output_rate = OutputRate;
                    if (CoreAudioSource != null) CoreAudioSource.Play();
                }
#endif
            }
        }

        [SerializeField]
        [HideInInspector]
        private int indexBuffSize = -1;


        /// <summary>
        /// If true (default) then Midi events are sent automatically to the midi player.\n
        /// Set to false if you want to process events without playing sound. \n
        /// OnEventNotesMidi Unity Event can be used to process each notes.
        /// </summary>
        [HideInInspector]
        public bool MPTK_DirectSendToPlayer;

        /// <summary>
        /// Should accept change tempo from Midi Events ? 
        /// </summary>
        [HideInInspector]
        public bool MPTK_EnableChangeTempo;

        /// <summary>
        /// If MPTK_Spatialize is enabled, the volume of the audio source depends on the distance between the audio source and the listener. \n
        /// Beyong this distance, the volume is set to 0 and the midi player is paused. No effect if MPTK_Spatialize is disabled.
        /// </summary>
        [HideInInspector]
        public float MPTK_MaxDistance
        {
            get
            {
                return maxDistance;
            }
            set
            {
                try
                {
                    maxDistance = value;
                    SetSpatialization();
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        protected void SetSpatialization()
        {
            //Debug.Log("Set Max Distance " + maxDistance);
            if (MPTK_CorePlayer)
            {
                if (CoreAudioSource != null)
                {
                    if (MPTK_Spatialize)
                    {
                        CoreAudioSource.maxDistance = maxDistance;
                        CoreAudioSource.spatialBlend = 1f;
                        CoreAudioSource.spatialize = true;
                        CoreAudioSource.spatializePostEffects = true;
                        CoreAudioSource.loop = true;
                        CoreAudioSource.volume = 1f;
                        if (!CoreAudioSource.isPlaying)
                            CoreAudioSource.Play();
                    }
                    else
                    {
                        CoreAudioSource.spatialBlend = 0f;
                        CoreAudioSource.spatialize = false;
                        CoreAudioSource.spatializePostEffects = false;
                    }
                }
            }
            else
            {
                AudiosourceTemplate.Audiosource.maxDistance = maxDistance;
                if (ActiveVoices != null)
                    for (int i = 0; i < ActiveVoices.Count; i++)
                    {
                        fluid_voice voice = ActiveVoices[i];
                        if (voice.VoiceAudio != null)
                            voice.VoiceAudio.Audiosource.maxDistance = maxDistance;
                    }
                if (FreeVoices != null)
                    for (int i = 0; i < FreeVoices.Count; i++)
                    {
                        fluid_voice voice = FreeVoices[i];
                        if (voice.VoiceAudio != null)
                            voice.VoiceAudio.Audiosource.maxDistance = maxDistance;
                    }
            }
        }

        [SerializeField]
        [HideInInspector]
        private float maxDistance;

        /// <summary>
        /// [obsolete] replaced by MPTK_Spatialize"); V2.83
        /// </summary>
        [HideInInspector]
        public bool MPTK_PauseOnDistance
        {
            get { Debug.LogWarning("MPTK_PauseOnDistance is obsolete, replaced by MPTK_Spatialize"); return spatialize; }
            set { Debug.LogWarning("MPTK_PauseOnDistance is obsolete, replaced by MPTK_Spatialize"); spatialize = value; }
        }
        /// <summary>
        /// Should the Spatialization effect must be enabled?\n
        /// See here how to setup spatialization with Unity https://paxstellar.fr/midi-file-player-detailed-view-2/#Foldout-Spatialization-Parameters
        /// </summary>
        [HideInInspector]
        public bool MPTK_Spatialize
        {
            get { return spatialize; }
            set
            {
                spatialize = value;
                SetSpatialization();
            }
        }

        /// <summary>
        /// Contains each MidiSynth for each channel when the prefab MidiSpatializer is used and IsMidiChannelSpace=true.\n
        /// Warning: only one MidiSpatializer can be used in a hierarchy.
        /// </summary>
        static public MidiFilePlayer[] SpatialSynths;
        protected bool IsMidiChannelSpace = false; // for internal use

        private int dedicatedChannel = -1;

        /// <summary>
        /// Dedicated Channel for this MidiSynth when the prefab MidiSpatializer is used.\n
        /// The MidiSynth reader (from a midi file) has no channel because no voice is played, so DedicatedChannel is set to -1
        /// </summary>
        public int MPTK_DedicatedChannel
        {
            get
            {
                return dedicatedChannel;
            }
        }

        /// <summary>
        /// Should change pan from Midi Events or from SoundFont ?\n
        /// Pan is disabled when Spatialization is activated.
        /// </summary>
        [HideInInspector]
        public bool MPTK_EnablePanChange;

        /// <summary>
        /// Volume of midi playing. \n
        /// Must be >=0 and <= 1
        /// </summary>
        [HideInInspector]
        public float MPTK_Volume
        {
            get { return volumeGlobal; }
            set
            {
                if (value >= 0f && value <= 1f) volumeGlobal = value; else Debug.LogWarning($"MidiFilePlayer - Set Volume value {value} not valid, must be between 0 and 1");
            }
        }

        [SerializeField]
        [HideInInspector]
        private float volumeGlobal = 0.5f;

        [HideInInspector]
        protected float volumeStartStop = 1f;

        /// <summary>
        /// Transpose note from -24 to 24
        /// </summary>
        [HideInInspector]
        public int MPTK_Transpose
        {
            get { return transpose; }
            set { if (value >= -24 && value <= 24f) transpose = value; else Debug.LogWarning("MidiFilePlayer - Set Transpose value not valid : " + value); }
        }

        /// <summary>
        /// Log for each wave to be played
        /// </summary>
        [HideInInspector]
        public bool MPTK_LogWave;

        [Header("Voice Statistics")]

        /// <summary>
        /// Count of the active voices (playing) - Readonly
        /// </summary>
        public int MPTK_StatVoiceCountActive;

        /// <summary>
        /// Count of the free voices for reusing on need.\n
        /// Voice older than AutoCleanVoiceTime are removed but only when count is over than AutoCleanVoiceLimit - Readonly
        /// </summary>
        public int MPTK_StatVoiceCountFree;

        /// <summary>
        /// Percentage of voice reused during the synth life. 0: any reuse, 100:all voice reused (unattainable, of course!)
        /// </summary>
        public float MPTK_StatVoiceRatioReused;

        /// <summary>
        /// Count of voice played since the start of the synth
        /// </summary>
        public int MPTK_StatVoicePlayed;

        //! @cond NODOC

        /*protected*/
        public MidiLoad midiLoaded;
        protected bool sequencerPause = false;
        protected double SynthElapsedMilli;
        protected float timeToPauseMilliSeconde = -1f;

        [SerializeField]
        [HideInInspector]
        protected bool playPause = false;
        /// <summary>
        /// Distance to the listener.\n
        /// Calculated only if MPTK_PauseOnDistance = true
        /// </summary>
        [HideInInspector]
        public float distanceToListener;

        [SerializeField]
        [HideInInspector]
        public int transpose = 0;

        public mptk_channel[] MptkChannels;          /** MPTK properties for channels */
        public fluid_channel[] Channels;          /** the channels */
        private List<fluid_voice> ActiveVoices;              /** the synthesis processes */

        private List<fluid_voice> FreeVoices;              /** the synthesis processes */
        //public ConcurrentQueue<MPTKEvent> QueueEvents;
        protected Queue<SynthCommand> QueueSynthCommand;
        protected Queue<List<MPTKEvent>> QueueMidiEvents;

        public class SynthCommand
        {
            public enum enCmd { StartEvent, StopEvent, ClearAllVoices, NoteOffAll }
            public enCmd Command;
            public int IdSession; // V2.84
            public MPTKEvent MidiEvent;
        }

        /* fluid_settings_old_t settings_old;  the old synthesizer settings */
        //TBC fluid_settings_t* settings;         /** the synthesizer settings */

        //int polyphony;                     /** maximum polyphony */
        [HideInInspector]
        public int FLUID_BUFSIZE = 64; // was a const

        [HideInInspector]
        public fluid_interp InterpolationMethod = fluid_interp.Linear;

        //[Tooltip("Force voice off (kill at once note) when the same note is played")]
        //public bool ForceVoiceOff;

        [HideInInspector]
        public float gain = 1f;

        //public const uint DRUM_INST_MASK = 0x80000000;
        [Header("Enable Debug Log")]

        public bool VerboseSynth;
        public bool VerboseVoice;
        public bool VerboseGenerator;
        public bool VerboseCalcGen;
        public bool VerboseController;
        public bool VerboseEnvVolume;
        public bool VerboseEnvModulation;
        public bool VerboseFilter;
        public bool VerboseVolume;

        [HideInInspector]
        public float OutputRate;

        [HideInInspector]
        public int DspBufferSize;

        //public int midi_channels = 16;                 /** the number of MIDI channels (>= 16) */
        //int audio_channels;                /** the number of audio channels (1 channel=left+right) */

        // the number of (stereo) 'sub'groups from the synth. Typically equal to audio_channels.
        // Only one used with Unity
        // int audio_groups;                 

        //int effects_channels = 2;              /** the number of effects channels (= 2) */

        private fluid_synth_status state = fluid_synth_status.FLUID_SYNTH_CLEAN;                /** the synthesizer state -  */ // V2.83 set to private
        //uint ticks;                /** the number of audio samples since the start */

        //the start in msec, as returned by system clock 
        //uint start;

        // How many audio buffers are used? (depends on nr of audio channels / groups)
        // Only one buffer used with Unity
        //int nbuf;     

        [Header("Attributes below applies only with AudioSource mode (Core Audio unchecked)")]

        public VoiceAudioSource AudiosourceTemplate;

        [Tooltip("Apply only with AudioSource mode (no Core Audio)")]
        public bool AdsrSimplified;

        //! @endcond

        /// <summary>
        /// Should play on a weak device (cheaper smartphone) ? Apply only with AudioSource mode (MPTK_CorePlayer=False).\n
        /// Playing Midi files with WeakDevice activated could cause some bad interpretation of Midi Event, consequently bad sound.
        /// </summary>
        [Tooltip("Apply only with AudioSource mode (no Core Audio)")]
        public bool MPTK_WeakDevice;

        /// <summary>
        /// [Only when CorePlayer=False] Define a minimum release time at noteoff in 100 iem nanoseconds.\n
        /// Default 50 ms is a good tradeoff. Below some unpleasant sound could be heard. Useless when MPTK_CorePlayer is true.
        /// </summary>
        [Range(0, 5000000)]
        [Tooltip("Apply only with AudioSource mode (no Core Audio)")]
        public uint MPTK_ReleaseTimeMin = 500000;

        //[Tooltip("Only for no Core Audio")]
        //[Range(0.00f, 5.0f)]
        //public float LfoAmpFreq = 1f;

        //[Tooltip("Only for no Core Audio")]
        //[Range(0.01f, 5.0f)]
        //public float LfoVibFreq = 1f;

        //[Tooltip("Only for no Core Audio")]
        //[Range(0.01f, 5.0f)]
        //public float LfoVibAmp = 1f;

        //[Tooltip("Only for no Core Audio")]
        //[Range(0.01f, 5.0f)]
        //public float LfoToFilterMod = 1f;

        //[Tooltip("Only for no Core Audio")]
        //[Range(0.01f, 5.0f)]
        //public float FilterEnvelopeMod = 1f;



        //[Tooltip("Only for no Core Audio")]
        //[Range(0f, 1f)]
        //public float ReverbMix = 0f;

        //[Tooltip("Only for no Core Audio")]
        //[Range(0f, 1f)]
        //public float ChorusMix = 0f;

        //! @cond NODOC

        [Range(0, 100)]
        [Tooltip("Smooth Volume Change")]
        public int DampVolume = 0; // default value=5
        //! @endcond


        //[Header("Events associated to the synth")]
        [HideInInspector]
        /// <summary>
        /// Unity event fired at awake of the synthesizer. Name of the gameobject component is passed as a parameter.
        ///! @code
        /// ...
        /// if (!midiStreamPlayer.OnEventSynthAwake.HasEvent())
        ///    midiStreamPlayer.OnEventSynthAwake.AddListener(StartLoadingSynth);
        /// ...
        /// public void StartLoadingSynth(string name)
        /// {
        ///     Debug.LogFormat("Synth {0} loading", name);
        /// }
        ///! @endcode
        /// </summary>
        public EventSynthClass OnEventSynthAwake;

        [HideInInspector]
        /// <summary>
        /// Unity event fired at start of the synthesizer. Name of the gameobject component is passed as a parameter.
        ///! @code
        /// ...
        /// if (!midiStreamPlayer.OnEventStartSynth.HasEvent())
        ///    midiStreamPlayer.OnEventStartSynth.AddListener(EndLoadingSynth);
        /// ...
        /// public void EndLoadingSynth(string name)
        /// {
        ///    Debug.LogFormat("Synth {0} loaded", name);
        ///    midiStreamPlayer.MPTK_PlayEvent(
        ///       new MPTKEvent() { Command = MPTKCommand.PatchChange, Value = CurrentPatchInstrument, Channel = StreamChannel});
        /// }
        ///! @endcode
        /// </summary>
        public EventSynthClass OnEventSynthStarted;

        private float[] left_buf;
        private float[] right_buf;

        //int cur;                           /** the current sample in the audio buffers to be output */
        //int dither_index;		/* current index in random dither value buffer: fluid_synth_(write_s16|dither_s16) */


        //fluid_tuning_t[][] tuning;           /** 128 banks of 128 programs for the tunings */
        //fluid_tuning_t cur_tuning;         /** current tuning in the iteration */

        // The midi router. Could be done nicer.
        //Indicates, whether the audio thread is currently running.Note: This simple scheme does -not- provide 100 % protection against thread problems, for example from MIDI thread and shell thread
        //fluid_mutex_t busy;
        //fluid_midi_router_t* midi_router;


        //default modulators SF2.01 page 52 ff:
        //There is a set of predefined default modulators. They have to be explicitly overridden by the sound font in order to turn them off.

        private static HiMod default_vel2att_mod = new HiMod();        /* SF2.01 section 8.4.1  */
        private static HiMod default_vel2filter_mod = new HiMod();     /* SF2.01 section 8.4.2  */
        private static HiMod default_at2viblfo_mod = new HiMod();      /* SF2.01 section 8.4.3  */
        private static HiMod default_mod2viblfo_mod = new HiMod();     /* SF2.01 section 8.4.4  */
        private static HiMod default_att_mod = new HiMod();            /* SF2.01 section 8.4.5  */
        private static HiMod default_pan_mod = new HiMod();            /* SF2.01 section 8.4.6  */
        private static HiMod default_expr_mod = new HiMod();           /* SF2.01 section 8.4.7  */
        private static HiMod default_reverb_mod = new HiMod();         /* SF2.01 section 8.4.8  */
        private static HiMod default_chorus_mod = new HiMod();         /* SF2.01 section 8.4.9  */
        private static HiMod default_pitch_bend_mod = new HiMod();     /* SF2.01 section 8.4.10 */

        private int countvoiceReused;

        //! @cond NODOC

        [HideInInspector]
        public bool showMidiInfo;
        [HideInInspector]
        public bool showSynthParameter;
        [HideInInspector]
        public bool showSpatialization;
        [HideInInspector]
        public bool showUnitySynthParameter;
        [HideInInspector]
        public bool showUnityPerformanceParameter;
        [HideInInspector]
        public bool showSoundFontEffect;
        [HideInInspector]
        public bool showUnitySynthEffect;
        [HideInInspector]
        public bool showMidiParameter;
        [HideInInspector]
        public bool showSynthEvents;
        [HideInInspector]
        public bool showEvents;
        [HideInInspector]
        public bool showDefault;
        [HideInInspector]
        public bool spatialize;


        //! @endcond

        /* reverb presets */
        //        static fluid_revmodel_presets_t revmodel_preset[] = {
        //	/* name */    /* roomsize */ /* damp */ /* width */ /* level */
        //	{ "Test 1",          0.2f,      0.0f,       0.5f,       0.9f },
        //    { "Test 2",          0.4f,      0.2f,       0.5f,       0.8f },
        //    { "Test 3",          0.6f,      0.4f,       0.5f,       0.7f },
        //    { "Test 4",          0.8f,      0.7f,       0.5f,       0.6f },
        //    { "Test 5",          0.8f,      1.0f,       0.5f,       0.5f },
        //    { NULL, 0.0f, 0.0f, 0.0f, 0.0f }
        //};

        // From fluid_sys.c - fluid_utime() returns the time in micro seconds. this time should only be used to measure duration(relative times). 
        //double fluid_utime()
        //{
        //    //fprintf(stderr, "fluid_cpu_frequency:%f fluid_utime:%f\n", fluid_cpu_frequency, rdtsc() / fluid_cpu_frequency);

        //    //return (rdtsc() / fluid_cpu_frequency);
        //    return AudioSettings.dspTime;
        //}

        // returns the current time in milliseconds. This time should only be used in relative time measurements.
        //int fluid_curtime()
        //{
        //    // replace GetTickCount() :Retrieves the number of milliseconds that have elapsed since the system was started, up to 49.7 days.
        //    return System.Environment.TickCount;
        //}

        public void Awake()
        {
            // for test
            //Time.timeScale = 0;


            IdSynth = lastIdSynth++;
            if (VerboseSynth) Debug.Log($"Awake MidiSynth IdSynth:{IdSynth}");
            try
            {
                OnEventSynthAwake.Invoke(this.name);
                MidiPlayerGlobal.InitPath();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }

            // V2.83 Move these init from Start to Awake
            if (!MPTK_CorePlayer && AudiosourceTemplate == null)
            {
                if (VerboseSynth)
                    Debug.LogWarningFormat("AudiosourceTemplate not defined in the {0} inspector, search one", this.name);
                AudiosourceTemplate = FindObjectOfType<VoiceAudioSource>();
                //if (AudiosourceTemplate == null)
                //{
                //    Debug.LogErrorFormat("No VoiceAudioSource template found for the audiosource synth {0}", this.name);
                //}
            }

            // V2.83 Move these init from Start to Awake
            if (CoreAudioSource == null)
            {
                //if (VerboseSynth) Debug.LogWarningFormat("CoreAudioSource not defined in the {0} inspector, search one", this.name);
                CoreAudioSource = GetComponent<AudioSource>();
                if (CoreAudioSource == null)
                {
                    Debug.LogErrorFormat("No AudioSource defined in the MPTK prefab '{0}'", this.name);
                }
            }
        }

        public void Start()
        {

            left_buf = new float[FLUID_BUFSIZE];
            right_buf = new float[FLUID_BUFSIZE];

            if (VerboseSynth) Debug.Log($"Start MidiSynth IdSynth:{IdSynth}");
            try
            {
#if MPTK_PRO
                BuildChannelSynth();
#endif

                Routine.RunCoroutine(ThreadLeanStartAudio(CoreAudioSource), Segment.RealtimeUpdate);

#if CANT_CHANGE_AUDIO_CONFIG
                // Get default value defined with Unity: Edit / Project Settings / Audio
                AudioConfiguration GetConfiguration = AudioSettings.GetConfiguration();
                OutputRate = GetConfiguration.sampleRate;
                DspBufferSize = GetConfiguration.dspBufferSize;
#else
                AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
#endif

                MPTK_IndexSynthRate = indexSynthRate;

                fluid_dsp_float.fluid_dsp_float_config();

#if !UNITY_ANDROID
                if (VerboseSynth)
                    InfoAudio();
#endif
                /* The number of buffers is determined by the higher number of nr
                 * groups / nr audio channels.  If LADSPA is unused, they should be
                 * the same. */
                //nbuf = audio_channels;
                //if (audio_groups > nbuf)
                //{
                //    nbuf = audio_groups;
                //}

                /* as soon as the synth is created it starts playing. */
                // Too soon state = fluid_synth_status.FLUID_SYNTH_PLAYING;

#if MPTK_PRO
                InitEffect();
#endif
                //cur = FLUID_BUFSIZE;
                //dither_index = 0;

                /* FIXME */
                //start = (uint)(DateTime.UtcNow.Ticks / fluid_voice.Nano100ToMilli); // milliseconds:  fluid_curtime();

#if MPTK_PRO && UNITY_ANDROID && UNITY_OBOE
                InitOboe();
#endif

                OnEventSynthStarted.Invoke(this.name);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

      

        public IEnumerator<float> ThreadLeanStartAudio(AudioSource audioSource)
        {
            audioSource.volume = 0f;
            while (audioSource.volume < 1f)
            {
                audioSource.volume += MPTK_LeanSynthStarting;
                yield return 0;// Routine.WaitForSeconds(.01f);
            }
            yield return 0;
        }

        /// <summary>
        /// Get current audio configuration
        /// </summary>
        /// <param name="deviceWasChanged"></param>
        void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
            AudioConfiguration GetConfiguration = AudioSettings.GetConfiguration();
            OutputRate = GetConfiguration.sampleRate;
            DspBufferSize = GetConfiguration.dspBufferSize;
            if (VerboseSynth)
            {
                Debug.Log("OnAudioConfigurationChanged - " + (deviceWasChanged ? "Device was changed" : "Reset was called"));
                Debug.Log("   dspBufferSize:" + DspBufferSize);
                Debug.Log("   OutputRate:" + OutputRate);
            }
        }

        private void InfoAudio()
        {
            int bufferLenght;
            int numBuffers;
            // Two methods
            AudioSettings.GetDSPBufferSize(out bufferLenght, out numBuffers);
            AudioConfiguration ac = AudioSettings.GetConfiguration();
            Debug.Log("------InfoAudio------");
            Debug.Log("  " + (MPTK_CorePlayer ? "Core Player Activated" : "AudioSource Player Activated"));
            Debug.Log("  bufferLenght:" + bufferLenght + " 2nd method: " + ac.dspBufferSize);
            Debug.Log("  numBuffers:" + numBuffers);
            Debug.Log("  outputSampleRate:" + AudioSettings.outputSampleRate + " 2nd method: " + ac.sampleRate);
            Debug.Log("  speakerMode:" + AudioSettings.speakerMode);
            Debug.Log("---------------------");
        }

        /// <summary>
        /// Initialize the synthetizer: channel, voices, modulator.\n
        /// It's not usefull to call this method if you are using prefabs (MidiFilePlayer, MidiStreamPlayer, ...).\n
        /// Each gameObjects created from these prefabs have their own, autonomous and isolated synth.
        /// </summary>
        /// <param name="channelCount">Number of channel to create, default 16. Any other values are experimental!</param>
        public void MPTK_InitSynth(int channelCount = 16)
        {
            fluid_voice.LastId = 0;

            if (channelCount > 32)
                channelCount = 32;

            if (MptkChannels == null)
            {
                MptkChannels = new mptk_channel[32];
                for (int i = 0; i < MptkChannels.Length; i++)
                    MptkChannels[i] = new mptk_channel(this, i);
            }

            Channels = new fluid_channel[channelCount];
            for (int i = 0; i < Channels.Length; i++)
                Channels[i] = new fluid_channel(this, i);

            if (VerboseSynth)
                Debug.Log($"MPTK_InitSynth. IdSynth:{IdSynth}, Channels:{Channels.Length}");

            if (ActiveVoices == null)
                ActiveVoices = new List<fluid_voice>();

            FreeVoices = new List<fluid_voice>();
            QueueSynthCommand = new Queue<SynthCommand>();
            QueueMidiEvents = new Queue<List<MPTKEvent>>();

            fluid_conv.fluid_conversion_config();

            //TBC fluid_dsp_float_config();
            //fluid_sys_config();
            //init_dither(); // pour fluid_synth_write_s16 ?

            /* SF2.01 page 53 section 8.4.1: MIDI Note-On Velocity to Initial Attenuation */
            fluid_mod_set_source1(default_vel2att_mod, /* The modulator we are programming here */
                (int)fluid_mod_src.FLUID_MOD_VELOCITY,    /* Source. VELOCITY corresponds to 'index=2'. */
                (int)fluid_mod_flags.FLUID_MOD_GC           /* Not a MIDI continuous controller */
                | (int)fluid_mod_flags.FLUID_MOD_CONCAVE    /* Curve shape. Corresponds to 'type=1' */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR   /* Polarity. Corresponds to 'P=0' */
                | (int)fluid_mod_flags.FLUID_MOD_NEGATIVE   /* Direction. Corresponds to 'D=1' */
            );
            fluid_mod_set_source2(default_vel2att_mod, 0, 0); /* No 2nd source */
            fluid_mod_set_dest(default_vel2att_mod, (int)fluid_gen_type.GEN_ATTENUATION);  /* Target: Initial attenuation */
            fluid_mod_set_amount(default_vel2att_mod, 960.0f);          /* Modulation amount: 960 */

            /* SF2.01 page 53 section 8.4.2: MIDI Note-On Velocity to Filter Cutoff
             * Have to make a design decision here. The specs don't make any sense this way or another.
             * One sound font, 'Kingston Piano', which has been praised for its quality, tries to
             * override this modulator with an amount of 0 and positive polarity (instead of what
             * the specs say, D=1) for the secondary source.
             * So if we change the polarity to 'positive', one of the best free sound fonts works...
             */
            fluid_mod_set_source1(default_vel2filter_mod, (int)fluid_mod_src.FLUID_MOD_VELOCITY, /* Index=2 */
                (int)fluid_mod_flags.FLUID_MOD_GC                        /* CC=0 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                  /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_NEGATIVE                /* D=1 */
            );
            fluid_mod_set_source2(default_vel2filter_mod, (int)fluid_mod_src.FLUID_MOD_VELOCITY, /* Index=2 */
                (int)fluid_mod_flags.FLUID_MOD_GC                                 /* CC=0 */
                | (int)fluid_mod_flags.FLUID_MOD_SWITCH                           /* type=3 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                         /* P=0 */
                                                                                  // do not remove       | FLUID_MOD_NEGATIVE                         /* D=1 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                         /* D=0 */
            );
            fluid_mod_set_dest(default_vel2filter_mod, (int)fluid_gen_type.GEN_FILTERFC);        /* Target: Initial filter cutoff */
            fluid_mod_set_amount(default_vel2filter_mod, -2400);

            /* SF2.01 page 53 section 8.4.3: MIDI Channel pressure to Vibrato LFO pitch depth */
            fluid_mod_set_source1(default_at2viblfo_mod, (int)fluid_mod_src.FLUID_MOD_CHANNELPRESSURE, /* Index=13 */
                (int)fluid_mod_flags.FLUID_MOD_GC                        /* CC=0 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                  /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                /* D=0 */
            );
            fluid_mod_set_source2(default_at2viblfo_mod, 0, 0); /* no second source */
            fluid_mod_set_dest(default_at2viblfo_mod, (int)fluid_gen_type.GEN_VIBLFOTOPITCH);        /* Target: Vib. LFO => pitch */
            fluid_mod_set_amount(default_at2viblfo_mod, 50);

            /* SF2.01 page 53 section 8.4.4: Mod wheel (Controller 1) to Vibrato LFO pitch depth */
            fluid_mod_set_source1(default_mod2viblfo_mod, 1, /* Index=1 */
                (int)fluid_mod_flags.FLUID_MOD_CC                        /* CC=1 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                  /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                /* D=0 */
            );
            fluid_mod_set_source2(default_mod2viblfo_mod, 0, 0); /* no second source */
            fluid_mod_set_dest(default_mod2viblfo_mod, (int)fluid_gen_type.GEN_VIBLFOTOPITCH);        /* Target: Vib. LFO => pitch */
            fluid_mod_set_amount(default_mod2viblfo_mod, 50);

            /* SF2.01 page 55 section 8.4.5: MIDI continuous controller 7 to initial attenuation*/
            fluid_mod_set_source1(default_att_mod, 7,                     /* index=7 */
                (int)fluid_mod_flags.FLUID_MOD_CC                              /* CC=1 */
                | (int)fluid_mod_flags.FLUID_MOD_CONCAVE                       /* type=1 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                      /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_NEGATIVE                      /* D=1 */
            );
            fluid_mod_set_source2(default_att_mod, 0, 0);                 /* No second source */
            fluid_mod_set_dest(default_att_mod, (int)fluid_gen_type.GEN_ATTENUATION);         /* Target: Initial attenuation */
            fluid_mod_set_amount(default_att_mod, 960.0f);                 /* Amount: 960 */

            /* SF2.01 page 55 section 8.4.6 MIDI continuous controller 10 to Pan Position */
            fluid_mod_set_source1(default_pan_mod, 10,                    /* index=10 */
                (int)fluid_mod_flags.FLUID_MOD_CC                              /* CC=1 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                        /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_BIPOLAR                       /* P=1 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                      /* D=0 */
            );
            fluid_mod_set_source2(default_pan_mod, 0, 0);                 /* No second source */
            fluid_mod_set_dest(default_pan_mod, (int)fluid_gen_type.GEN_PAN);

            // Target: pan - Amount: 500. The SF specs $8.4.6, p. 55 syas: "Amount = 1000 tenths of a percent". 
            // The center value (64) corresponds to 50%, so it follows that amount = 50% x 1000/% = 500. 
            fluid_mod_set_amount(default_pan_mod, 500.0f);

            /* SF2.01 page 55 section 8.4.7: MIDI continuous controller 11 to initial attenuation*/
            fluid_mod_set_source1(default_expr_mod, 11,                     /* index=11 */
                (int)fluid_mod_flags.FLUID_MOD_CC                              /* CC=1 */
                | (int)fluid_mod_flags.FLUID_MOD_CONCAVE                       /* type=1 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                      /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_NEGATIVE                      /* D=1 */
            );
            fluid_mod_set_source2(default_expr_mod, 0, 0);                 /* No second source */
            fluid_mod_set_dest(default_expr_mod, (int)fluid_gen_type.GEN_ATTENUATION);         /* Target: Initial attenuation */
            fluid_mod_set_amount(default_expr_mod, 960.0f);                 /* Amount: 960 */

            /* SF2.01 page 55 section 8.4.8: MIDI continuous controller 91 to Reverb send */
            fluid_mod_set_source1(default_reverb_mod, 91,                 /* index=91 */
                (int)fluid_mod_flags.FLUID_MOD_CC                              /* CC=1 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                        /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                      /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                      /* D=0 */
            );
            fluid_mod_set_source2(default_reverb_mod, 0, 0);              /* No second source */
            fluid_mod_set_dest(default_reverb_mod, (int)fluid_gen_type.GEN_REVERBSEND);       /* Target: Reverb send */
            fluid_mod_set_amount(default_reverb_mod, 200);                /* Amount: 200 ('tenths of a percent') */

            /* SF2.01 page 55 section 8.4.9: MIDI continuous controller 93 to Reverb send */
            fluid_mod_set_source1(default_chorus_mod, 93,                 /* index=93 */
                (int)fluid_mod_flags.FLUID_MOD_CC                              /* CC=1 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                        /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                      /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                      /* D=0 */
            );
            fluid_mod_set_source2(default_chorus_mod, 0, 0);              /* No second source */
            fluid_mod_set_dest(default_chorus_mod, (int)fluid_gen_type.GEN_CHORUSSEND);       /* Target: Chorus */
            fluid_mod_set_amount(default_chorus_mod, 200);                /* Amount: 200 ('tenths of a percent') */

            /* SF2.01 page 57 section 8.4.10 MIDI Pitch Wheel to Initial Pitch ... */
            fluid_mod_set_source1(default_pitch_bend_mod, (int)fluid_mod_src.FLUID_MOD_PITCHWHEEL, /* Index=14 */
                (int)fluid_mod_flags.FLUID_MOD_GC                              /* CC =0 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                        /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_BIPOLAR                       /* P=1 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                      /* D=0 */
            );
            fluid_mod_set_source2(default_pitch_bend_mod, (int)fluid_mod_src.FLUID_MOD_PITCHWHEELSENS,  /* Index = 16 */
                (int)fluid_mod_flags.FLUID_MOD_GC                                        /* CC=0 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                                  /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                                /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                                /* D=0 */
            );
            fluid_mod_set_dest(default_pitch_bend_mod, (int)fluid_gen_type.GEN_PITCH);                 /* Destination: Initial pitch */
            fluid_mod_set_amount(default_pitch_bend_mod, 12700.0f);                 /* Amount: 12700 cents */

            MPTK_ResetStat();
            state = fluid_synth_status.FLUID_SYNTH_PLAYING;
        }

        /// <summary>
        /// Start the Midi sequencer: each midi events are read and play in a dedicated thread.\n
        /// This thread is automatically started by prefabs MidiFilePlayer, MidiListPlayer, MidiExternalPlayer.
        /// </summary>
        public void MPTK_StartSequencerMidi()
        {
            if (VerboseSynth) Debug.LogFormat("MPTK_InitSequencerMidi {0} {1}", this.name, "thread is " + (midiThread == null ? "null" : "alive:" + midiThread.IsAlive));

            if (midiThread == null || !midiThread.IsAlive)
            {
                if (VerboseSynth) Debug.Log($"MPTK_InitSequencerMidi {this.name} {IdSynth}");
                midiThread = new Thread(ThreadMidiPlayer);
                midiThread.Start();
            }
            else if (VerboseSynth) Debug.LogFormat("MPTK_InitSequencerMidi: thread is already alive");
        }

        /// <summary>
        /// Stop processing samples by the synth and the Midi sequencer.
        /// </summary>
        public void MPTK_StopSynth()
        {
            state = fluid_synth_status.FLUID_SYNTH_STOPPED;
        }

        /// <summary>
        /// Clear all sound by sending note off. \n
        /// That could take some seconds because release time for sample need to be played.
        ///! @code
        ///  if (GUILayout.Button("Clear"))
        ///     midiStreamPlayer.MPTK_ClearAllSound(true);
        ///! @endcode       
        /// </summary>
        /// <param name="destroyAudioSource">usefull only in non core mode</param>
        /// <param name="_idSession">clear only for sample playing with this session, -1 for all (default)</param>
        public void MPTK_ClearAllSound(bool destroyAudioSource = false, int _idSession = -1)
        {
            Routine.RunCoroutine(ThreadClearAllSound(true), Segment.RealtimeUpdate);
        }

        public IEnumerator<float> ThreadClearAllSound(bool destroyAudioSource = false, int _idSession = -1)
        {
#if DEBUGNOTE
            numberNote = -1;
#endif
            MPTK_ResetStat();
            //Debug.Log($" >>> {DateTime.Now} ThreadClearAllSound {_idSession}");
            if (MPTK_CorePlayer)
            {
                if (SpatialSynths != null && dedicatedChannel == -1) // apply only for the MidiSynth reader
                {
                    foreach (MidiFilePlayer mfp in SpatialSynths)
                        if (mfp.QueueSynthCommand != null)
                            mfp.QueueSynthCommand.Enqueue(new SynthCommand() { Command = SynthCommand.enCmd.NoteOffAll, IdSession = _idSession });
                    // Could be gard to synch all synth ! prefer a robust solution ...
                    // V2.84 yield return Timing.WaitForSeconds(0.5f);
                }
                else
                {

                    if (QueueSynthCommand != null)
                        QueueSynthCommand.Enqueue(new SynthCommand() { Command = SynthCommand.enCmd.NoteOffAll, IdSession = _idSession });
                    // V2.84 yield return Timing.WaitUntilDone(Timing.RunCoroutine(ThreadWaitAllStop()), false);
                }
            }
            else
            {
                for (int i = 0; i < ActiveVoices.Count; i++)
                {
                    fluid_voice voice = ActiveVoices[i];
                    if (voice != null && (voice.status == fluid_voice_status.FLUID_VOICE_ON || voice.status == fluid_voice_status.FLUID_VOICE_SUSTAINED))
                    {
                        //Debug.LogFormat("ReleaseAll {0} / {1}", voice.IdVoice, ActiveVoices.Count);
                        yield return Routine.WaitUntilDone(Routine.RunCoroutine(voice.Release(), Segment.RealtimeUpdate));
                    }
                }
                if (destroyAudioSource)
                {
                    yield return Routine.WaitUntilDone(Routine.RunCoroutine(ThreadDestroyAllVoice(), Segment.RealtimeUpdate), false);
                }
            }

            //Debug.Log($" <<< {DateTime.Now} ThreadClearAllSound {_idSession}");

            yield return 0;
        }


        /// <summary>
        /// Wait until all notes are off.\n
        /// That could take some seconds due to the samples release time.\n
        /// Therefore, the method exit after a timeout of 3 seconds.\n
        /// *** Use this method only as a coroutine ***
        ///! @code
        ///     // Call this method with: StartCoroutine(NextPreviousWithWait(false)); 
        ///     // See TestMidiFilePlayerScripting.cs
        ///     public IEnumerator NextPreviousWithWait(bool next)
        ///     {
        ///         midiFilePlayer.MPTK_Stop();
        ///         yield return midiFilePlayer.MPTK_WaitAllNotesOff(midiFilePlayer.IdSession);
        ///         if (next)
        ///             midiFilePlayer.MPTK_Next();
        ///         else
        ///             midiFilePlayer.MPTK_Previous();
        ///         CurrentIndexPlaying = midiFilePlayer.MPTK_MidiIndex;
        ///     yield return 0;
        ///}
        ///! @endcode
        /// </summary>
        /// <param name="_idSession">clear only for samples playing with this session, -1 for all</param>
        /// <returns></returns>
        public IEnumerator MPTK_WaitAllNotesOff(int _idSession = -1) // V2.84: new param idsession and CoRoutine compatible
        {
            //Debug.Log($"<<< {DateTime.Now} MPTK_WaitAllNotesOff {_idSession}");
            //yield return Timing.WaitUntilDone(Timing.RunCoroutine(ThreadWaitAllStop(_idSession)), false);
            int count = 999;
            DateTime start = DateTime.Now;
            //Debug.Log($"ThreadWaitAllStop " + start);
            if (ActiveVoices != null)
            {
                while (count != 0 && (DateTime.Now - start).TotalMilliseconds < 3000d)
                {
                    count = 0;
                    foreach (fluid_voice voice in ActiveVoices)
                        if (voice != null &&
                           (_idSession == -1 || voice.IdSession == _idSession) &&
                           (voice.status == fluid_voice_status.FLUID_VOICE_ON || voice.status == fluid_voice_status.FLUID_VOICE_SUSTAINED))
                            count++;
                    //Debug.LogFormat("   ThreadReleaseAll\t{0}\t{1}\t{2}/{3}", start, (DateTime.Now - start).TotalMilliseconds, count, ActiveVoices.Count);
                    yield return new WaitForSeconds(.1f);
                }
            }
            //Debug.Log($"<<< {DateTime.Now} MPTK_WaitAllNotesOff {_idSession}");
            yield return 0;
        }


        // Nothing to document after this line
        //! @cond NODOC

        public IEnumerator<float> ThreadWaitAllStop(int _idSession = -1)
        {
            int count = 999;
            DateTime start = DateTime.Now;
            //Debug.Log($"ThreadWaitAllStop " + start);
            if (ActiveVoices != null)
            {
                while (count != 0 && (DateTime.Now - start).TotalMilliseconds < 3000d)
                {
                    count = 0;
                    foreach (fluid_voice voice in ActiveVoices)
                        if (voice != null &&
                           (_idSession == -1 || voice.IdSession == _idSession) &&
                           (voice.status == fluid_voice_status.FLUID_VOICE_ON || voice.status == fluid_voice_status.FLUID_VOICE_SUSTAINED))
                            count++;
                    //Debug.LogFormat("   ThreadReleaseAll\t{0}\t{1}\t{2}/{3}", start, (DateTime.Now - start).TotalMilliseconds, count, ActiveVoices.Count);
                    yield return Routine.WaitForSeconds(0.2f);
                }
            }
            Debug.Log($"ThreadWaitAllStop end - {DateTime.Now} count:{count}");
            yield return 0;

        }


        /// Remove AudioSource not playing
        /// </summary>
        protected IEnumerator<float> ThreadDestroyAllVoice()
        {
            //Debug.Log("ThreadDestroyAllVoice");
            try
            {
                //VoiceAudioSource[] voicesList = GetComponentsInChildren<VoiceAudioSource>();
                //Debug.LogFormat("DestroyAllVoice {0}", (voicesList != null ? voicesList.Length.ToString() : "no voice found"));
                //if (voicesList != null)
                //{
                //    foreach (VoiceAudioSource voice in voicesList)
                //        try
                //        {
                //            //Debug.Log("Destroy " + voice.IdVoice + " " + (voice.Audiosource.clip != null ? voice.Audiosource.clip.name : "no clip"));
                //            //Don't delete audio source template
                //            if (voice.name.StartsWith("VoiceAudioId_"))
                //                Destroy(voice.gameObject);
                //        }
                //        catch (System.Exception ex)
                //        {
                //            MidiPlayerGlobal.ErrorDetail(ex);
                //        }
                //    Voices.Clear();
                //}
                if (ActiveVoices != null)
                {
                    if (MPTK_CorePlayer)
                        QueueSynthCommand.Enqueue(new SynthCommand() { Command = SynthCommand.enCmd.ClearAllVoices });
                    else
                    {
                        for (int i = 0; i < ActiveVoices.Count; i++)
                        {
                            try
                            {
                                fluid_voice voice = ActiveVoices[i];
                                if (voice != null && voice.VoiceAudio != null)
                                {
                                    //Debug.Log("Destroy " + voice.IdVoice + " " + (voice.VoiceAudio.Audiosource.clip != null ? voice.VoiceAudio.Audiosource.clip.name : "no clip"));
                                    //Don't delete audio source template
                                    if (voice.VoiceAudio.name.StartsWith("VoiceAudioId_"))
                                        Destroy(voice.VoiceAudio.gameObject);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                MidiPlayerGlobal.ErrorDetail(ex);
                            }
                        }
                        ActiveVoices.Clear();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            yield return 0;
        }

        void OnApplicationQuit()
        {
            //Debug.Log("MidiSynth OnApplicationQuit " + Time.time + " seconds");
            state = fluid_synth_status.FLUID_SYNTH_STOPPED;
        }

        private void OnApplicationPause(bool pause)
        {
            //Debug.Log("MidiSynth OnApplicationPause " + pause);
        }

        protected void ResetMidi()
        {
            timeMidiFromStartPlay = 0d;
            lastTimeMidi = 0d;
            watchMidi.Reset();
            watchMidi.Start();
            if (midiLoaded != null) midiLoaded.StartMidi();
        }

        //! @endcond

        /// <summary>
        /// Reset voices statistics 
        /// </summary>
        public void MPTK_ResetStat()
        {
            MPTK_StatVoicePlayed = 0;
            countvoiceReused = 0;
            MPTK_StatVoiceRatioReused = 0;
            //        lastTimePlayCore = 0d;
            StatDspLoadPCT = 0f;
            StatDspLoadMIN = float.MaxValue;
            StatDspLoadMAX = 0f;

            if (MptkChannels != null)
                foreach (mptk_channel mptkChannel in MptkChannels)
                    mptkChannel.count = 0;

            StatSynthLatency = new MovingAverage();
            StatSynthLatencyAVG = 0f;
            StatSynthLatencyMIN = float.MaxValue;
            StatSynthLatencyMAX = 0f;

#if DEBUG_PERF_AUDIO
            StatDspLoadMA = new MovingAverage();
            //StatDspLoadLongMA = new MovingAverage();
            StatAudioFilterReadMIN = double.MaxValue;
            StatAudioFilterReadMAX = 0;
            StatAudioFilterReadMA = new MovingAverage();
            StatSampleWriteMA = new MovingAverage();
            StatProcessListMA = new MovingAverage();
#endif
#if DEBUG_PERF_MIDI
            StatDeltaThreadMidiMIN = double.MaxValue;
            StatDeltaThreadMidiMAX = 0;
            StatDeltaThreadMidiMA = new MovingAverage();
            StatProcessMidiMAX = 0f; ;
            watchPerfMidi = new System.Diagnostics.Stopwatch();
#endif
        }

        /*
         * fluid_mod_set_source1
         */
        void fluid_mod_set_source1(HiMod mod, int src, int flags)
        {
            mod.Src1 = (byte)src;
            mod.Flags1 = (byte)flags;
        }

        /*
         * fluid_mod_set_source2
         */
        void fluid_mod_set_source2(HiMod mod, int src, int flags)
        {
            mod.Src2 = (byte)src;
            mod.Flags2 = (byte)flags;
        }

        /*
         * fluid_mod_set_dest
         */
        void fluid_mod_set_dest(HiMod mod, int dest)
        {
            mod.Dest = (byte)dest;
        }

        /*
         * fluid_mod_set_amount
         */
        void fluid_mod_set_amount(HiMod mod, float amount)
        {
            mod.Amount = amount;
        }

        /// <summary>
        /// Enable or disable a channel.
        /// </summary>
        /// <param name="channel">must be between 0 and 15</param>
        /// <param name="enable">true to enable</param>
        public void MPTK_ChannelEnableSet(int channel, bool enable)
        {
            if (MptkChannels != null && channel >= 0 && channel < MptkChannels.Length)
                MptkChannels[channel].enabled = enable;
        }

        /// <summary>
        /// Is channel is enabled or disabled.
        /// </summary>
        /// <param name="channel">channel, must be between 0 and 15</param>
        /// <returns>true if channel is enabled</returns>
        public bool MPTK_ChannelEnableGet(int channel)
        {
            if (MptkChannels != null && channel >= 0 && channel < MptkChannels.Length)
                return MptkChannels[channel].enabled;
            else
                return false;
        }

        /// <summary>
        /// Get count of notes played since the start of the Midi.
        /// </summary>
        /// <param name="channel">must be between 0 and 15</param>
        public int MPTK_ChannelNoteCount(int channel)
        {
            if (MptkChannels != null && channel >= 0 && channel < MptkChannels.Length)
                return MptkChannels[channel].count;
            else
                return 0;
        }

        /// <summary>
        /// Set the volume for a channel (between 0 and 1). New with V2.82, works only in Core mode.
        /// </summary>
        /// <param name="channel">must be between 0 and 15</param>
        /// <param name="volume">volume for the channel, must be between 0 and 1</param>
        public void MPTK_ChannelVolumeSet(int channel, float volume)
        {
            if (MptkChannels != null && channel >= 0 && channel < MptkChannels.Length)
                MptkChannels[channel].volume = volume;
            for (int i = 0; i < ActiveVoices.Count; i++)
            {
                fluid_voice voice = ActiveVoices[i];
                if (voice.chan == channel)
                    voice.fluid_voice_update_param((int)fluid_gen_type.GEN_PAN);
            }
        }

        /// <summary>
        /// Get the volume of the channel
        /// </summary>
        /// <param name="channel">must be between 0 and 15</param>
        /// <returns>volume of the channel, between 0 and 1</returns>
        public float MPTK_ChannelVolumeGet(int channel)
        {
            if (MptkChannels != null && channel >= 0 && channel < MptkChannels.Length)
                return MptkChannels[channel].volume;
            else
                return 0f;
        }

        /// <summary>
        /// Get channel preset indx.
        /// </summary>
        /// <param name="channel">must be between 0 and 15</param>
        public int MPTK_ChannelPresetGetIndex(int channel)
        {
            if (CheckParamChannel(channel))
                return Channels[channel].prognum;
            else
                return -1;
        }

        /// <summary>
        /// Get channel bank.
        /// </summary>
        /// <param name="channel">must be between 0 and 15</param>
        public int MPTK_ChannelBankGetIndex(int channel)
        {
            if (CheckParamChannel(channel))
                return Channels[channel].banknum;
            else
                return -1;
        }

        /// <summary>
        /// Get channel current preset name.
        /// </summary>
        /// <param name="channel">must be between 0 and 15</param>
        public string MPTK_ChannelPresetGetName(int channel)
        {
            try
            {
                if (CheckParamChannel(channel) && Channels[channel].preset != null)
                    return Channels[channel].preset.Name;
                return "No preset find";

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get channel count. The midi norm is 16, but MPTK can manage up to 32 channels.
        /// </summary>
        /// <param name="channel">must be between 0 and 15</param>
        /// <returns>channel count</returns>
        public int MPTK_ChannelCount()
        {
            if (Channels != null /* v2.83 && CheckParamChannel(0)*/)
                return Channels.Length;
            return 0;
        }

        private bool CheckParamChannel(int channel)
        {
            if (Channels == null)
                return false; // V2.83

            if (channel < 0 || channel >= Channels.Length)
            {
                //Debug.LogWarningFormat("MPTK_ChannelEnable: channels are not created");
                return false;
            }
            //if (channel < 0 || channel >= Channels.Length)
            //{
            //    //Debug.LogWarningFormat("MPTK_ChannelEnable: incorrect value for channel {0}", channel);
            //    return false;
            //}
            if (Channels[channel] == null)
            {
                //Debug.LogWarningFormat("MPTK_ChannelEnable: channel {0} is not defined", channel);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Set forced preset on the channel. Midi will allways playing with this preset even if a Midi Preset Change message is received.\n
        /// Set to -1 to disable this behavior.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>preset index, -1 if not set</returns>
        ///! @code
        /// GUILayout.Label($"{midiFilePlayer.MPTK_ChannelForcedPresetGet(channel)}/{midiFilePlayer.MPTK_ChannelBankGetIndex(channel)}");
        /// int forced = 
        ///       (int)GUILayout.HorizontalSlider(
        ///             midiFilePlayer.MPTK_ChannelForcedPresetGet(channel),  
        ///             -1f, 127f);
        /// if (forced != midiFilePlayer.MPTK_ChannelForcedPresetGet(channel))
        ///    midiFilePlayer.MPTK_ChannelForcedPresetSet(channel, forcedPreset);
        ///! @endcode
        public int MPTK_ChannelForcedPresetGet(int channel)
        {
            if (CheckParamChannel(channel))
            {
                return MptkChannels[channel].forcedPreset;
            }
            return -1;
        }
        /// <summary>
        /// Set forced preset on the channel. Midi will allways playing with this preset even if a Midi Preset Change message is received.\n
        /// Set to -1 to disable this behavior.
        /// </summary>
        /// <param name="channel">0 to 15 channel</param>
        /// <param name="preset">0 to 127 preset</param>
        /// <returns></returns>
        ///! @code
        /// GUILayout.Label($"{midiFilePlayer.MPTK_ChannelForcedPresetGet(channel)}/{midiFilePlayer.MPTK_ChannelBankGetIndex(channel)}");
        /// int forced = 
        ///       (int)GUILayout.HorizontalSlider(
        ///             midiFilePlayer.MPTK_ChannelForcedPresetGet(channel),  
        ///             -1f, 127f);
        /// if (forced != midiFilePlayer.MPTK_ChannelForcedPresetGet(channel))
        ///    midiFilePlayer.MPTK_ChannelForcedPresetSet(channel, forcedPreset);
        ///! @endcode
        public bool MPTK_ChannelForcedPresetSet(int channel, int preset)
        {
            if (CheckParamChannel(channel))
            {
                // Take the default bank for this channel or a new bank
                int bank = Channels[channel].banknum;

                ImSoundFont sfont = MidiPlayerGlobal.ImSFCurrent;
                if (sfont == null)
                    Debug.LogWarningFormat("MPTK_ChannelForcedPreset: no soundfont defined");
                else if (bank < 0 || bank >= sfont.Banks.Length)
                    Debug.LogWarningFormat($"MPTK_ChannelForcedPreset: bank {bank} is outside the limits [0 - {sfont.Banks.Length}] for sfont {sfont.SoundFontName}");
                else if (sfont.Banks[bank] == null || sfont.Banks[bank].defpresets == null)
                    Debug.LogWarningFormat($"MPTK_ChannelForcedPreset: bank {bank} is not defined with sfont {sfont.SoundFontName}");
                else if (preset < -1 || preset >= sfont.Banks[bank].defpresets.Length)
                    Debug.LogWarningFormat($"MPTK_ChannelForcedPreset: preset {preset} is outside the limits [0 - {sfont.Banks[bank].defpresets.Length}] for sfont {sfont.SoundFontName} and bank {bank}");
                else
                {
                    bool found = true;
                    if (preset != -1 && sfont.Banks[bank].defpresets[preset] == null)
                    {
                        found = false;
                        // search for a preset available
                        for (int p = preset + 1; p < sfont.Banks[bank].defpresets.Length; p++)
                            if (sfont.Banks[bank].defpresets[p] != null)
                            {
                                preset = p;
                                found = true;
                                break;
                            }
                        if (!found)
                            for (int p = preset - 1; p >= 0; p--)
                                if (sfont.Banks[bank].defpresets[p] != null)
                                {
                                    preset = p;
                                    found = true;
                                    break;
                                }
                        if (VerboseVoice && found)
                            Debug.LogFormat($"MPTK_ChannelForcedPreset: preset not found, set close preset {preset} ");
                    }
                    if (!found)
                        Debug.LogWarningFormat($"MPTK_ChannelForcedPreset: preset {preset} is not defined with sfont {sfont.SoundFontName} and bank {bank}");
                    else
                    {
                        MptkChannels[channel].forcedPreset = preset; // set to -1 to disable forced preset
                        if (preset >= 0)
                            fluid_synth_program_change(channel, preset);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Change the preset and bank for the channel. \n
        /// When playing a Midi file, the preset is set by channel with the Midi message Patch Change. \n
        /// The bank is changed with a ControlChange Midi message.  \n
        /// The new value of the bank is local for the channel, the preset list is not updated.\n
        /// To change globally the bank, use instead the global methods: MidiPlayerGlobal.MPTK_SelectBankInstrument or MidiPlayerGlobal.MPTK_SelectBankDrum
        /// </summary>
        /// <param name="channel">0 to 15. There is 16 channels available in the Midi norm.</param>
        /// <param name="preset">The count of presets is dependant of the soundfont selected</param>
        /// <param name="newbank">optionnal, use the default bank defined globally</param>
        /// <returns>true if preset change is done</returns>
        public bool MPTK_ChannelPresetChange(int channel, int preset, int newbank = -1)
        {
            if (CheckParamChannel(channel))
            {
                // Take the default bank for this channel or a new bank
                int bank = newbank < 0 ? Channels[channel].banknum : newbank;

                ImSoundFont sfont = MidiPlayerGlobal.ImSFCurrent;
                if (sfont == null)
                    Debug.LogWarningFormat("MPTK_ChannelPresetChange: no soundfont defined");
                else if (bank < 0 || bank >= sfont.Banks.Length)
                    Debug.LogWarningFormat($"MPTK_ChannelPresetChange: bank {bank} is outside the limits [0 - {sfont.Banks.Length}] for sfont {sfont.SoundFontName}");
                else if (sfont.Banks[bank] == null || sfont.Banks[bank].defpresets == null)
                    Debug.LogWarningFormat($"MPTK_ChannelPresetChange: bank {bank} is not defined with sfont {sfont.SoundFontName}");
                else if (preset < 0 || preset >= sfont.Banks[bank].defpresets.Length)
                    Debug.LogWarningFormat($"MPTK_ChannelPresetChange: preset {preset} is outside the limits [0 - {sfont.Banks[bank].defpresets.Length}] for sfont {sfont.SoundFontName} and bank {bank}");
                else if (sfont.Banks[bank].defpresets[preset] == null)
                    Debug.LogWarningFormat($"MPTK_ChannelPresetChange: preset {preset} is not defined with sfont {sfont.SoundFontName} and bank {bank}");
                else
                {
                    Channels[channel].banknum = bank;
                    fluid_synth_program_change(channel, preset);
                    return true;
                }
            }
            return false;
        }

        //! @cond NODOC

        /// <summary>
        /// Allocate a synthesis voice. This function is called by a soundfont's preset in response to a noteon event.\n
        /// The returned voice comes with default modulators installed(velocity-to-attenuation, velocity to filter, ...)\n
        /// Note: A single noteon event may create any number of voices, when the preset is layered. Typically 1 (mono) or 2 (stereo).
        /// </summary>
        public fluid_voice fluid_synth_alloc_voice(HiSample hiSample, int chan, int _idSession, int key, int vel)
        {
            fluid_voice voice = null;
            fluid_channel MidiChannel = null;
            MPTK_StatVoicePlayed++;

            /*   fluid_mutex_lock(synth.busy); /\* Don't interfere with the audio thread *\/ */
            /*   fluid_mutex_unlock(synth.busy); */

            // check if there's an available free voice with same sample and same session
            for (int indexVoice = 0; indexVoice < FreeVoices.Count;)
            {
                fluid_voice v = FreeVoices[indexVoice];
                if (v.sample.Name == hiSample.Name && _idSession == v.IdSession)
                {
                    voice = v;
                    FreeVoices.RemoveAt(indexVoice);
                    countvoiceReused++;
                    if (VerboseVoice) Debug.Log($"Voice {voice.IdVoice} - Reuse - Sample:'{hiSample.Name}'");
                    break;
                }
                indexVoice++;
            }

#if DEBUG_PERF_NOTEON
            DebugPerf("After find existing voice:");
#endif
            // No found existing voice, instanciate a new one
            if (voice == null)
            {
                if (VerboseVoice) Debug.Log($"Voice idSession:{_idSession} idVoice:{fluid_voice.LastId} - Create - Sample:'{hiSample.Name}' Rate:{hiSample.SampleRate} hz");

                voice = new fluid_voice(this);
                voice.IdSession = _idSession;

                if (MPTK_CorePlayer)
                {
                    // Play voice with OnAudioFilterRead
                    // --------------------------------------

                    if (MidiPlayerGlobal.ImSFCurrent.LiveSF)
                    {
                        if (hiSample.Data == null)
                        {
                            hiSample.Data = MidiPlayerGlobal.ImSFCurrent.SampleData;
                            //    if (VerboseVoice) Debug.LogFormat("Load sample {0} rate:{1} length:{2} ko ", hiSample.Name, hiSample.SampleRate, hiSample.End - hiSample.Start + 1);
                            //    hiSample.Data = new float[hiSample.End - hiSample.Start + 1];
                            //    for (uint i = hiSample.Start; i <= hiSample.End; i++)
                            //    {
                            //        short s = (short)((MidiPlayerGlobal.ImSFCurrent.HiSf.SampleData[(i * 2) + 1] << 8) | MidiPlayerGlobal.ImSFCurrent.HiSf.SampleData[(i * 2)]);
                            //        // convert to range from -1 to (just below) 1
                            //        hiSample.Data[i - hiSample.Start] = s / 32768.0F;
                            //    }
                        }
                        voice.sample = hiSample;
                    }
                    else
                    {
                        voice.VoiceAudio = null;
                        voice.sample = DicAudioWave.GetWave(hiSample.Name);
                    }
                    // Can't load wave out of the main Unity thread
                    //if (voice.sample == null)
                    //{
                    //    MidiPlayerGlobal.LoadWave(hiSample);
                    //    voice.sample = hiSample;
                    //}

                    if (voice.sample == null)
                    {
                        Debug.LogWarningFormat("fluid_synth_alloc_voice - Clip {0} data not loaded", hiSample.Name);
                        return null;
                    }
                    // Debug.LogFormat("fluid_synth_alloc_voice - load wave from dict. {0} Length:{1} SynthSampleRate:{2}", hiSample.Name, voice.sample.Data.Length, sample_rate);
                }
                else
                {
                    // Play each voice with a dedicated AudioSource (legacy mode)
                    // ----------------------------------------------------------
                    AudioClip clip = DicAudioClip.Get(hiSample.Name);
                    if (clip == null)
                    {
                        string path = MidiPlayerGlobal.WavePath + "/" + System.IO.Path.GetFileNameWithoutExtension(hiSample.Name);
                        AudioClip ac = Resources.Load<AudioClip>(path);
                        if (ac != null)
                        {
                            //Debug.Log("Wave load " + path);
                            DicAudioClip.Add(hiSample.Name, ac);
                        }
                        clip = DicAudioClip.Get(hiSample.Name);
                        if (clip == null || clip.loadState != AudioDataLoadState.Loaded)
                        {
                            Debug.LogWarningFormat("fluid_synth_alloc_voice - Clip {0} not found", hiSample.Name);
                            return null;
                        }
                        else if (clip.loadState != AudioDataLoadState.Loaded)
                        {
                            Debug.LogWarningFormat("fluid_synth_alloc_voice - Clip {0} not ready to play {1}", hiSample.Name, clip.loadState);
                            return null;
                        }
                    }
                    voice.sample = hiSample;
                    voice.VoiceAudio = Instantiate<VoiceAudioSource>(AudiosourceTemplate);
                    voice.VoiceAudio.fluidvoice = voice;
                    voice.VoiceAudio.synth = this;
                    voice.VoiceAudio.transform.position = AudiosourceTemplate.transform.position;
                    voice.VoiceAudio.transform.SetParent(AudiosourceTemplate.transform.parent);
                    voice.VoiceAudio.name = "VoiceAudioId_" + voice.IdVoice;
                    voice.VoiceAudio.Audiosource.clip = clip;
                    // seems to have no effect, issue open with Unity
                    voice.VoiceAudio.hideFlags = VerboseVoice ? HideFlags.None : HideFlags.HideInHierarchy;
                }

#if DEBUG_PERF_NOTEON
                DebugPerf("After instanciate voice:");
#endif
            }
            //else if (VerboseVoice) Debug.Log($"Voice idSession:{_idSession} idVoice:{fluid_voice.LastId} - Reuse - Sample:'{hiSample.Name}' Rate:{hiSample.SampleRate} hz");

            // Apply change on each voice
            if (MPTK_CorePlayer)
            {
                // Done with ThreadCorePlay in MidiFilePlayer
            }
            else
            {
                // Legacy mode, will be removed
                if (voice.VoiceAudio != null)
                    voice.VoiceAudio.Audiosource.spatialBlend = MPTK_Spatialize ? 1f : 0f;
                MoveVoiceToFree();
                if (MPTK_AutoBuffer)
                    AutoCleanVoice(DateTime.UtcNow.Ticks);
            }

            if (chan < 0 || chan >= Channels.Length)
            {
                Debug.LogFormat("Channel out of range chan:{0}", chan);
                chan = 0;
            }
            MidiChannel = Channels[chan];

            // Defined default voice value. Called also when a voice is reused.
            voice.fluid_voice_init(MptkChannels[chan], MidiChannel, key, vel/*, gain*/);

#if DEBUG_PERF_NOTEON
            DebugPerf("After fluid_voice_init:");
#endif
            /* add the default modulators to the synthesis process. */
            voice.mods = new List<HiMod>();
            voice.fluid_voice_add_mod(MidiSynth.default_vel2att_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT);    /* SF2.01 $8.4.1  */
            voice.fluid_voice_add_mod(MidiSynth.default_vel2filter_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT); /* SF2.01 $8.4.2  */
            voice.fluid_voice_add_mod(MidiSynth.default_at2viblfo_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT);  /* SF2.01 $8.4.3  */
            voice.fluid_voice_add_mod(MidiSynth.default_mod2viblfo_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT); /* SF2.01 $8.4.4  */
            voice.fluid_voice_add_mod(MidiSynth.default_att_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT);        /* SF2.01 $8.4.5  */
            voice.fluid_voice_add_mod(MidiSynth.default_pan_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT);        /* SF2.01 $8.4.6  */
            voice.fluid_voice_add_mod(MidiSynth.default_expr_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT);       /* SF2.01 $8.4.7  */
            voice.fluid_voice_add_mod(MidiSynth.default_reverb_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT);     /* SF2.01 $8.4.8  */
            voice.fluid_voice_add_mod(MidiSynth.default_chorus_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT);     /* SF2.01 $8.4.9  */
            voice.fluid_voice_add_mod(MidiSynth.default_pitch_bend_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT); /* SF2.01 $8.4.10 */
#if DEBUG_PERF_NOTEON
            DebugPerf("After fluid_voice_add_mod:");
#endif

            ActiveVoices.Add(voice);
            voice.IndexActive = ActiveVoices.Count - 1;

            MPTK_StatVoiceCountActive = ActiveVoices.Count;
            MPTK_StatVoiceCountFree = FreeVoices.Count;
            //if (countvoiceAllocated > 0) cant'be zero
            MPTK_StatVoiceRatioReused = (countvoiceReused * 100f) / MPTK_StatVoicePlayed;
            return voice;
        }

        public void fluid_synth_kill_by_exclusive_class(fluid_voice new_voice)
        {
            //fluid_synth_t* synth
            /** Kill all voices on a given channel, which belong into
                excl_class.  This function is called by a SoundFont's preset in
                response to a noteon event.  If one noteon event results in
                several voice processes (stereo samples), ignore_ID must name
                the voice ID of the first generated voice (so that it is not
                stopped). The first voice uses ignore_ID=-1, which will
                terminate all voices on a channel belonging into the exclusive
                class excl_class.
            */

            //int i;
            int excl_class = (int)new_voice.gens[(int)fluid_gen_type.GEN_EXCLUSIVECLASS].Val;
            /* Check if the voice belongs to an exclusive class. In that case, previous notes from the same class are released. */

            /* Excl. class 0: No exclusive class */
            if (excl_class == 0)
            {
                return;
            }

            //  FLUID_LOG(FLUID_INFO, "Voice belongs to exclusive class (class=%d, ignore_id=%d)", excl_class, ignore_ID);

            /* Kill all notes on the same channel with the same exclusive class */

            for (int i = 0; i < ActiveVoices.Count; i++)
            {
                fluid_voice voice = ActiveVoices[i];
                /* Existing voice does not play? Leave it alone. */
                if (!(voice.status == fluid_voice_status.FLUID_VOICE_ON) || voice.status == fluid_voice_status.FLUID_VOICE_SUSTAINED)
                {
                    continue;
                }

                /* An exclusive class is valid for a whole channel (or preset). Is the voice on a different channel? Leave it alone. */
                if (voice.chan != new_voice.chan)
                {
                    continue;
                }

                /* Existing voice has a different (or no) exclusive class? Leave it alone. */
                if ((int)voice.gens[(int)fluid_gen_type.GEN_EXCLUSIVECLASS].Val != excl_class)
                {
                    continue;
                }

                /* Existing voice is a voice process belonging to this noteon event (for example: stereo sample)?  Leave it alone. */
                if (voice.IdVoice == new_voice.IdVoice)
                {
                    continue;
                }

                //    FLUID_LOG(FLUID_INFO, "Releasing previous voice of exclusive class (class=%d, id=%d)",
                //     (int)_GEN(existing_voice, GEN_EXCLUSIVECLASS), (int)fluid_voice_get_id(existing_voice));
                //Debug.Log($"{voice.key} {voice.SampleName} ");

                voice.fluid_voice_kill_excl();
            }
        }
        /// <summary>
        ///  Start a synthesis voice. This function is called by a soundfont's preset in response to a noteon event after the voice  has been allocated with fluid_synth_alloc_voice() and initialized.
        /// Exclusive classes are processed here.
        /// </summary>
        /// <param name="synth"></param>
        /// <param name="voice"></param>

        //public void fluid_synth_start_voice(fluid_voice voice)
        //{
        //    //fluid_synth_t synth
        //    /*   fluid_mutex_lock(synth.busy); /\* Don't interfere with the audio thread *\/ */
        //    /*   fluid_mutex_unlock(synth.busy); */

        //    /* Find the exclusive class of this voice. If set, kill all voices
        //     * that match the exclusive class and are younger than the first
        //     * voice process created by this noteon event. */
        //    fluid_synth_kill_by_exclusive_class(voice);

        //    /* Start the new voice */
        //    voice.fluid_voice_start();
        //}

        public HiPreset fluid_synth_find_preset(int banknum, int prognum)
        {
            ImSoundFont sfont = MidiPlayerGlobal.ImSFCurrent;
            if (banknum >= 0 && banknum < sfont.Banks.Length &&
                sfont.Banks[banknum] != null &&
                sfont.Banks[banknum].defpresets != null &&
                prognum < sfont.Banks[banknum].defpresets.Length &&
                sfont.Banks[banknum].defpresets[prognum] != null)
                return sfont.Banks[banknum].defpresets[prognum];

            // Not find, return the first available
            foreach (ImBank bank in sfont.Banks)
                if (bank != null)
                    foreach (HiPreset preset in bank.defpresets)
                        if (preset != null)
                            return preset;
            return null;
        }

        public void synth_noteon(MPTKEvent note)
        {
            if (note.Tag != null && note.Tag.GetType() == typeof(long))
                StatUILatencyLAST = (float)(DateTime.UtcNow.Ticks - (long)note.Tag) / (float)fluid_voice.Nano100ToMilli;

            HiSample hiSample;
            fluid_voice voice;
            List<HiMod> mod_list = new List<HiMod>();

            int key = note.Value + MPTK_Transpose;
            int vel = note.Velocity;
            HiPreset preset;

            //DebugPerf("Begin synth_noteon:");
            MptkChannels[note.Channel].count++;

            if (!MptkChannels[note.Channel].enabled)
            {
                if (MPTK_LogWave)
                    Debug.LogFormat("Channel {0} disabled, cancel playing note: {1}", note.Channel, note.Value);
                return;
            }

            // Use the preset defined in the channel
            preset = Channels[note.Channel].preset;
            if (preset == null)
            {
                if (MPTK_LogWave)
                    Debug.LogWarningFormat("No preset associated to this channel {0}, set first preset, note: {1}", note.Channel, note.Value);
                fluid_synth_program_change(note.Channel, 0);
                preset = Channels[note.Channel].preset;
                if (preset == null)
                {
                    Debug.LogWarningFormat("No preset associated to this channel {0}, cancel playing note: {1}", note.Channel, note.Value);
                    return;
                }
            }

            // If the same note is hit twice on the same channel, then the older voice process is advanced to the release stage.  
            if (MPTK_ReleaseSameNote)
                fluid_synth_release_voice_on_same_note(note.Channel, key);

            ImSoundFont sfont = MidiPlayerGlobal.ImSFCurrent;
            note.Voices = new List<fluid_voice>();

            // run thru all the zones of this preset 
            foreach (HiZone preset_zone in preset.Zone)
            {
                // check if the note falls into the key and velocity range of this preset 
                if ((preset_zone.KeyLo <= key) &&
                    (preset_zone.KeyHi >= key) &&
                    (preset_zone.VelLo <= vel) &&
                    (preset_zone.VelHi >= vel))
                {
                    if (preset_zone.Index >= 0)
                    {
                        HiInstrument inst = sfont.HiSf.inst[preset_zone.Index];
                        HiZone global_inst_zone = inst.GlobalZone;

                        // run thru all the zones of this instrument */
                        foreach (HiZone inst_zone in inst.Zone)
                        {

                            if (inst_zone.Index < 0 || inst_zone.Index >= sfont.HiSf.Samples.Length)
                                continue;

                            // make sure this instrument zone has a valid sample
                            hiSample = sfont.HiSf.Samples[inst_zone.Index];
                            if (hiSample == null)
                                continue;

                            // check if the note falls into the key and velocity range of this instrument

                            if ((inst_zone.KeyLo <= key) &&
                                (inst_zone.KeyHi >= key) &&
                                (inst_zone.VelLo <= vel) &&
                                (inst_zone.VelHi >= vel))
                            {
                                //
                                // Found a sample to play
                                //
                                //Debug.Log("   Found Instrument '" + inst.name + "' index:" + inst_zone.index + " '" + sfont.hisf.Samples[inst_zone.index].Name + "'");
                                //DebugPerf("After found instrument:");
                                //if (MidiPlayerGlobal.ImSFCurrent.LiveSF)
                                //{
                                //    //voice.sample.Data = sfont.HiSf.SampleData;
                                //}
                                voice = fluid_synth_alloc_voice(hiSample, note.Channel, note.IdSession, key, vel);
#if DEBUG_PERF_NOTEON
                                DebugPerf("After fluid_synth_alloc_voice:");
#endif
                                if (voice == null) return;

                                voice.MptkEvent = note;
                                note.Voices.Add(voice);
                                voice.Duration = note.Duration; // only for information, not used

                                // V2.82: can be set to -1
                                voice.DurationTick = note.Duration >= 0 ? note.Duration * fluid_voice.Nano100ToMilli : -1;

                                //
                                // Instrument level - Generator
                                // ----------------------------

                                // Global zone

                                // SF 2.01 section 9.4 'bullet' 4: A generator in a local instrument zone supersedes a global instrument zone generator.  
                                // Both cases supersede the default generator. The generator not defined in this instrument do nothing, leave it at the default.

                                if (global_inst_zone != null && global_inst_zone.gens != null)
                                    foreach (HiGen gen in global_inst_zone.gens)
                                    {
                                        //fluid_voice_gen_set(voice, i, global_inst_zone.gen[i].val);
                                        voice.gens[(int)gen.type].Val = gen.Val;
                                        voice.gens[(int)gen.type].flags = fluid_gen_flags.GEN_SET_INSTRUMENT;
                                    }

                                // Local zone
                                if (inst_zone.gens != null && inst_zone.gens != null)
                                    foreach (HiGen gen in inst_zone.gens)
                                    {
                                        //fluid_voice_gen_set(voice, i, global_inst_zone.gen[i].val);
                                        voice.gens[(int)gen.type].Val = gen.Val;
                                        voice.gens[(int)gen.type].flags = fluid_gen_flags.GEN_SET_INSTRUMENT;
                                    }

                                //
                                // Instrument level - Modulators
                                // -----------------------------

                                /// Global zone
                                mod_list = new List<HiMod>();
                                if (global_inst_zone != null && global_inst_zone.mods != null)
                                {
                                    foreach (HiMod mod in global_inst_zone.mods)
                                        mod_list.Add(mod);
                                    //HiMod.DebugLog("      Instrument Global Mods ", global_inst_zone.mods);
                                }
                                //HiMod.DebugLog("      Instrument Local Mods ", inst_zone.mods);

                                // Local zone
                                if (inst_zone.mods != null)
                                    foreach (HiMod mod in inst_zone.mods)
                                    {
                                        // 'Identical' modulators will be deleted by setting their list entry to NULL.  The list length is known. 
                                        // NULL entries will be ignored later.  SF2.01 section 9.5.1 page 69, 'bullet' 3 defines 'identical'.

                                        foreach (HiMod mod1 in mod_list)
                                        {
                                            // fluid_mod_test_identity(mod, mod_list[i]))
                                            if ((mod1.Dest == mod.Dest) &&
                                                (mod1.Src1 == mod.Src1) &&
                                                (mod1.Src2 == mod.Src2) &&
                                                (mod1.Flags1 == mod.Flags1) &&
                                                (mod1.Flags2 == mod.Flags2))
                                            {
                                                mod1.Amount = mod.Amount;
                                                break;
                                            }
                                        }
                                    }

                                // Add instrument modulators (global / local) to the voice.
                                // Instrument modulators -supersede- existing (default) modulators.  SF 2.01 page 69, 'bullet' 6
                                foreach (HiMod mod1 in mod_list)
                                    voice.fluid_voice_add_mod(mod1, fluid_voice_addorover_mod.FLUID_VOICE_OVERWRITE);

                                //
                                // Preset level - Generators
                                // -------------------------

                                //  Local zone
                                if (preset_zone.gens != null)
                                    foreach (HiGen gen in preset_zone.gens)
                                    {
                                        //fluid_voice_gen_incr(voice, i, preset.global_zone.gen[i].val);
                                        //if (gen.type==fluid_gen_type.GEN_VOLENVATTACK)
                                        voice.gens[(int)gen.type].Val += gen.Val;
                                        voice.gens[(int)gen.type].flags = fluid_gen_flags.GEN_SET_PRESET;
                                    }

                                // Global zone
                                if (preset.GlobalZone != null && preset.GlobalZone.gens != null)
                                {
                                    foreach (HiGen gen in preset.GlobalZone.gens)
                                    {
                                        // If not incremented in local, increment in global
                                        if (voice.gens[(int)gen.type].flags != fluid_gen_flags.GEN_SET_PRESET)
                                        {
                                            //fluid_voice_gen_incr(voice, i, preset.global_zone.gen[i].val);
                                            voice.gens[(int)gen.type].Val += gen.Val;
                                            voice.gens[(int)gen.type].flags = fluid_gen_flags.GEN_SET_PRESET;
                                        }
                                    }
                                }

                                //
                                // Preset level - Modulators
                                // -------------------------

                                // Global zone
                                mod_list = new List<HiMod>();
                                if (preset.GlobalZone != null && preset.GlobalZone.mods != null)
                                {
                                    foreach (HiMod mod in preset.GlobalZone.mods)
                                        mod_list.Add(mod);
                                    //HiMod.DebugLog("      Preset Global Mods ", preset.global_zone.mods);
                                }
                                //HiMod.DebugLog("      Preset Local Mods ", preset_zone.mods);

                                // Local zone
                                if (preset_zone.mods != null)
                                    foreach (HiMod mod in preset_zone.mods)
                                    {
                                        // 'Identical' modulators will be deleted by setting their list entry to NULL.  The list length is known. 
                                        // NULL entries will be ignored later.  SF2.01 section 9.5.1 page 69, 'bullet' 3 defines 'identical'.

                                        foreach (HiMod mod1 in mod_list)
                                        {
                                            // fluid_mod_test_identity(mod, mod_list[i]))
                                            if ((mod1.Dest == mod.Dest) &&
                                                (mod1.Src1 == mod.Src1) &&
                                                (mod1.Src2 == mod.Src2) &&
                                                (mod1.Flags1 == mod.Flags1) &&
                                                (mod1.Flags2 == mod.Flags2))
                                            {
                                                mod1.Amount = mod.Amount;
                                                break;
                                            }
                                        }
                                    }

                                // Add preset modulators (global / local) to the voice.
                                foreach (HiMod mod1 in mod_list)
                                    if (mod1.Amount != 0d)
                                        // Preset modulators -add- to existing instrument default modulators.  
                                        // SF2.01 page 70 first bullet on page 
                                        voice.fluid_voice_add_mod(mod1, fluid_voice_addorover_mod.FLUID_VOICE_ADD);

#if DEBUG_PERF_NOTEON
                                DebugPerf("After genmod init:");
#endif
                                // Find the exclusive class of this voice. If set, kill all voices that match the exclusive class 
                                // and are younger than the first voice process created by this noteon event.
                                if (MPTK_KillByExclusiveClass)
                                    fluid_synth_kill_by_exclusive_class(voice);

                                /* Start the new voice */
                                voice.fluid_voice_start(note);

#if DEBUG_PERF_NOTEON
                                DebugPerf("After fluid_voice_start:");
#endif

                                if (MPTK_LogWave)
                                    Debug.LogFormat("NoteOn [C:{0:00} B:{1:000} P:{2:000}]\t{3,-21}\tKey:{4,-3}({5})\tVel:{6,-3}\tDuration:{7:0.000}\tInstr:{8,-21}\t\tSample:{9,-21}\tAtt:{10:0.00}\tPan:{11:0.00}",
                                    note.Channel + 1, Channels[note.Channel].banknum, Channels[note.Channel].prognum, preset.Name,
                                    key,
                                    HelperNoteLabel.LabelFromMidi(key),
                                    vel, note.Duration,
                                    inst.Name,
                                    sfont.HiSf.Samples[inst_zone.Index].Name,
                                    fluid_conv.fluid_atten2amp(voice.attenuation),
                                    voice.pan
                                );

                                if (VerboseGenerator)
                                    foreach (HiGen gen in voice.gens)
                                        if (gen != null && gen.flags > 0)
                                            Debug.LogFormat("Gen Id:{1,-50}\t{0}\tValue:{2:0.00}\tMod:{3:0.00}\tflags:{4,-50}", (int)gen.type, gen.type, gen.Val, gen.Mod, gen.flags);

                                /* Store the ID of the first voice that was created by this noteon event.
                                 * Exclusive class may only terminate older voices.
                                 * That avoids killing voices, which have just been created.
                                 * (a noteon event can create several voice processes with the same exclusive
                                 * class - for example when using stereo samples)
                                 */
                            }
                            if (playOnlyFirstWave && note.Voices.Count > 0)
                                return;
                        }
                    }

                }
            }
#if DEBUG_PERF_NOTEON
            DebugPerf("After synth_noteon:");
#endif
            if (MPTK_LogWave && note.Voices.Count == 0)
                Debug.LogFormat("NoteOn [{0:00} {1:000} {2:000}]\t{3,-21}\tKey:{4,-3}\tVel:{5,-3}\tDuration:{6:0.000}\tInstr:{7,-21}",
                note.Channel, Channels[note.Channel].banknum, Channels[note.Channel].prognum, preset.Name, key, vel, note.Duration, "*** no wave found ***");
        }

        // If the same note is hit twice on the same channel, then the older voice process is advanced to the release stage.  
        // Using a mechanical MIDI controller, the only way this can happen is when the sustain pedal is held.
        // In this case the behaviour implemented here is natural for many instruments.  
        // Note: One noteon event can trigger several voice processes, for example a stereo sample.  Don't release those...
        void fluid_synth_release_voice_on_same_note(int chan, int key)
        {
            for (int i = 0; i < ActiveVoices.Count; i++)
            {
                fluid_voice voice = ActiveVoices[i];
                if (voice.chan == chan && voice.key == key)
                //&& (fluid_voice_get_id(voice) != synth->noteid))
                {
                    //if (ForceVoiceOff)
                    //voice.fluid_voice_off();
                    //else
                    voice.fluid_voice_noteoff(true);

                    if (VerboseVoice) Debug.Log($"Voice {voice.IdVoice} - Same note, send note off");
                    // can't break, beacause need to search in case of multi sample
                }
            }
        }

        public void fluid_synth_allnotesoff()
        {
            for (int chan = 0; chan < Channels.Length; chan++)
                fluid_synth_noteoff(chan, -1);
        }

        public void fluid_synth_noteoff(int pchan, int pkey)
        {
            for (int i = 0; i < ActiveVoices.Count; i++)
            {
                fluid_voice voice = ActiveVoices[i];
                // A voice is 'ON', if it has not yet received a noteoff event. Sending a noteoff event will advance the envelopes to  section 5 (release). 
                //#define _ON(voice)  ((voice)->status == FLUID_VOICE_ON && (voice)->volenv_section < FLUID_VOICE_ENVRELEASE)
                if (voice.status == fluid_voice_status.FLUID_VOICE_ON &&
                    voice.volenv_section < fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE &&
                    voice.chan == pchan &&
                    (pkey == -1 || voice.key == pkey))
                {
                    //fluid_global.FLUID_LOG(fluid_log_level.FLUID_INFO, "noteoff chan:{0} key:{1} vel:{2} time{3}", voice.chan, voice.key, voice.vel, (fluid_curtime() - start) / 1000.0f);
                    voice.fluid_voice_noteoff();
                }
            }
        }

        public void fluid_synth_soundoff(int pchan)
        {
            for (int i = 0; i < ActiveVoices.Count; i++)
            {
                fluid_voice voice = ActiveVoices[i];
                // A voice is 'ON', if it has not yet received a noteoff event. Sending a noteoff event will advance the envelopes to  section 5 (release). 
                //#define _ON(voice)  ((voice)->status == FLUID_VOICE_ON && (voice)->volenv_section < FLUID_VOICE_ENVRELEASE)
                if (voice.status == fluid_voice_status.FLUID_VOICE_ON &&
                    voice.volenv_section < fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE &&
                    voice.chan == pchan)
                {
                    //fluid_global.FLUID_LOG(fluid_log_level.FLUID_INFO, "noteoff chan:{0} key:{1} vel:{2} time{3}", voice.chan, voice.key, voice.vel, (fluid_curtime() - start) / 1000.0f);
                    voice.fluid_voice_off();
                }
            }
        }

        /*
         * fluid_synth_damp_voices
         */
        public void fluid_synth_damp_voices(int pchan)
        {
            for (int i = 0; i < ActiveVoices.Count; i++)
            {
                fluid_voice voice = ActiveVoices[i];
                //#define _SUSTAINED(voice)  ((voice)->status == FLUID_VOICE_SUSTAINED)
                if (voice.chan == pchan && voice.status == fluid_voice_status.FLUID_VOICE_SUSTAINED)
                    voice.fluid_voice_noteoff(true);
            }
        }

        /*
         * fluid_synth_cc - call directly
         */
        public void fluid_synth_cc(int chan, MPTKController num, int val)
        {
            /*   fluid_mutex_lock(busy); /\* Don't interfere with the audio thread *\/ */
            /*   fluid_mutex_unlock(busy); */

            /* check the ranges of the arguments */
            //if ((chan < 0) || (chan >= midi_channels))
            //{
            //    FLUID_LOG(FLUID_WARN, "Channel out of range");
            //    return FLUID_FAILED;
            //}
            //if ((num < 0) || (num >= 128))
            //{
            //    FLUID_LOG(FLUID_WARN, "Ctrl out of range");
            //    return FLUID_FAILED;
            //}
            //if ((val < 0) || (val >= 128))
            //{
            //    FLUID_LOG(FLUID_WARN, "Value out of range");
            //    return FLUID_FAILED;
            //}

            /* set the controller value in the channel */
            Channels[chan].fluid_channel_cc(num, val);
        }

        /// <summary>
        /// tell all synthesis activ voices on this channel to update their synthesis parameters after a control change.
        /// </summary>
        /// <param name="chan"></param>
        /// <param name="is_cc"></param>
        /// <param name="ctrl"></param>
        public void fluid_synth_modulate_voices(int chan, int is_cc, int ctrl)
        {
            for (int i = 0; i < ActiveVoices.Count; i++)
            {
                fluid_voice voice = ActiveVoices[i];
                if (voice.chan == chan && voice.status != fluid_voice_status.FLUID_VOICE_OFF)
                    voice.fluid_voice_modulate(is_cc, ctrl);
            }
        }

        /// <summary>
        /// Tell all synthesis processes on this channel to update their synthesis parameters after an all control off message (i.e. all controller have been reset to their default value).
        /// </summary>
        /// <param name="chan"></param>
        public void fluid_synth_modulate_voices_all(int chan)
        {
            for (int i = 0; i < ActiveVoices.Count; i++)
            {
                fluid_voice voice = ActiveVoices[i];
                if (voice.chan == chan)
                    voice.fluid_voice_modulate_all();
            }
        }

        /*
         * fluid_synth_program_change
         */
        public void fluid_synth_program_change(int pchan, int prognum)
        {
            fluid_channel channel;
            HiPreset preset;
            int banknum;
            if (MptkChannels[pchan].forcedPreset >= 0)
                prognum = MptkChannels[pchan].forcedPreset;
            channel = Channels[pchan];
            banknum = channel.banknum; //fluid_channel_get_banknum
            channel.prognum = prognum; // fluid_channel_set_prognum
            if (VerboseVoice) Debug.LogFormat("ProgramChange\tChannel:{0}\tBank:{1}\tPreset:{2}", pchan, banknum, prognum);
            preset = fluid_synth_find_preset(banknum, prognum);
            channel.preset = preset; // fluid_channel_set_preset
        }

        /*
         * fluid_synth_pitch_bend
         */
        void fluid_synth_pitch_bend(int chan, int val)
        {
            if (MPTK_ApplyRealTimeModulator)
            {
                /*   fluid_mutex_lock(busy); /\* Don't interfere with the audio thread *\/ */
                /*   fluid_mutex_unlock(busy); */

                /* check the ranges of the arguments */
                if (chan < 0 || chan >= Channels.Length)
                {
                    Debug.LogFormat("Channel out of range chan:{0}", chan);
                    return;
                }

                /* set the pitch-bend value in the channel */
                Channels[chan].fluid_channel_pitch_bend(val);
            }
        }

        /// <summary>
        /// Play a list of Midi events 
        /// </summary>
        /// <param name="midievents">List of Midi events to play</param>
        /// <param name="playNoteOff"></param>
        protected void PlayEvents(List<MPTKEvent> midievents, bool playNoteOff = true)
        {
            if (MidiPlayerGlobal.MPTK_SoundFontLoaded == false)
                return;

            if (midievents != null)
            {
                foreach (MPTKEvent note in midievents)
                {
                    MPTK_PlayDirectEvent(note, playNoteOff);
                }
            }
        }
#if DEBUGNOTE
        public int numberNote = -1;
        public int startNote;
        public int countNote;
#endif
        /// <summary>
        /// Play one Midi event
        /// @snippet MusicView.cs Example PlayNote
        /// </summary>
        /// <param name="midievent"></param>
        protected void StopEvent(MPTKEvent midievent)
        {
            try
            {
                if (midievent != null && midievent.Voices != null)
                {
                    for (int i = 0; i < midievent.Voices.Count; i++)
                    {
                        fluid_voice voice = midievent.Voices[i];
                        if (voice.volenv_section != fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE &&
                            voice.status != fluid_voice_status.FLUID_VOICE_OFF)
                            voice.fluid_voice_noteoff();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// V2.86 Play immediately one Midi event.\n
        /// Like MPTK_PlayEvent but take time to process the event before returning to the caller.
        /// @snippet MusicView.cs Example MPTK_PlayEvent
        /// </summary>
        /// <param name="midievent"></param>
        public void MPTK_PlayDirectEvent(MPTKEvent midievent, bool playNoteOff = true)
        {
            //Debug.Log($">>> PlayEvent IdSynth:'{this.IdSynth}'");

            try
            {
                if (MidiPlayerGlobal.ImSFCurrent == null)
                {
                    Debug.Log("No SoundFont selected for MPTK_PlayNote ");
                    return;
                }

#if DEBUG_PERF_NOTEON
                DebugPerf("-----> Init perf:", 0);
#endif
                //Debug.Log(midievent.ToString());
                switch (midievent.Command)
                {
                    case MPTKCommand.NoteOn:
                        if (midievent.Velocity != 0)
                        {
#if DEBUGNOTE
                            numberNote++;
                            if (numberNote < startNote || numberNote > startNote + countNote - 1) return;
#endif
                            //if (note.Channel==4)
                            synth_noteon(midievent);
                        }
                        else
                        {
                            //Debug.Log("PlayEvent: NoteOn velocity=0 " + midievent.Value);
                            fluid_synth_noteoff(midievent.Channel, midievent.Value);
                        }
                        break;

                    case MPTKCommand.NoteOff:
                        if (playNoteOff)
                            fluid_synth_noteoff(midievent.Channel, midievent.Value);
                        break;

                    case MPTKCommand.ControlChange:
                        //if (midievent.Controller == MPTKController.Modulation) Debug.Log("midievent.Controller Modulation " + midievent.Value);
                        if (MPTK_ApplyRealTimeModulator)
                            Channels[midievent.Channel].fluid_channel_cc(midievent.Controller, midievent.Value); // replace of fluid_synth_cc(note.Channel, note.Controller, (int)note.Value);
                        break;

                    case MPTKCommand.PatchChange:
                        if (midievent.Channel != 9 || MPTK_EnablePresetDrum == true)
                            fluid_synth_program_change(midievent.Channel, midievent.Value);
                        break;

                    case MPTKCommand.PitchWheelChange:
                        fluid_synth_pitch_bend(midievent.Channel, midievent.Value);
                        break;
                }
#if DEBUG_PERF_NOTEON
                DebugPerf("<---- ClosePerf perf:", 2);
#endif
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            //Debug.Log($"<<< PlayEvent IdSynth:'{this.IdSynth}'");
        }

#if MPTK_PRO && UNITY_ANDROID && UNITY_OBOE
        public unsafe void OnAudioData(AudioStream audioStream, void* dataArray, int numFrames)
        {
            float* data = (float*)dataArray;
            DspBufferSize = numFrames;
#else
        private void OnAudioFilterRead(float[] data, int channels)
        {
            // data.Length == DspBufferSize * channels (so, in general the dobble
            //Debug.Log($"OnAudioFilterRead {IdSynth} length:{data.Length} channels:{channels} DspBufferSize:{DspBufferSize}");
#endif
            //This uses the Unity specific float method we added to get the buffer
            if (MPTK_CorePlayer && state == fluid_synth_status.FLUID_SYNTH_PLAYING)
            {
                long ticks = System.DateTime.UtcNow.Ticks;
                //Debug.Log($"{data[0]} {data[1]} ");
                if (lastTimePlayCore == 0d)
                {
                    lastTimePlayCore = AudioSettings.dspTime * 1000d;
                    return;
                }


                watchOnAudioFilterRead.Reset();
                watchOnAudioFilterRead.Start();

                SynthElapsedMilli = AudioSettings.dspTime * 1000d;


                StatDeltaAudioFilterReadMS = SynthElapsedMilli - lastTimePlayCore;
                //Debug.Log(deltaTimeCore);
                lastTimePlayCore = SynthElapsedMilli;

#if MPTK_PRO
                StartEvent();
#endif

                lock (this)
                {
                    ProcessQueueCommand();

#if DEBUG_PERF_AUDIO
                    watchPerfAudio.Reset();
                    watchPerfAudio.Start();
#endif
                    MoveVoiceToFree();
                    if (MPTK_AutoBuffer)
                        AutoCleanVoice(ticks);
                    MPTK_StatVoiceCountActive = ActiveVoices.Count;
                    MPTK_StatVoiceCountFree = FreeVoices.Count;

#if DEBUG_PERF_AUDIO
                    watchPerfAudio.Stop();
                    StatProcessListMS = (float)watchPerfAudio.ElapsedTicks / ((float)System.Diagnostics.Stopwatch.Frequency / 1000f);
                    StatProcessListMA.Add(Convert.ToInt32(StatProcessListMS * 1000f));
                    StatProcessListAVG = StatProcessListMA.Average / 1000f;

                    watchPerfAudio.Reset();
                    watchPerfAudio.Start();
#endif
                    int block = 0;
                    while (block < DspBufferSize)
                    {
                        Array.Clear(left_buf, 0, FLUID_BUFSIZE);
                        Array.Clear(right_buf, 0, FLUID_BUFSIZE);

                        float[] reverb_buf = null;
                        float[] chorus_buf = null;
#if MPTK_PRO
                        PrepareBufferEffect(out reverb_buf, out chorus_buf);
#endif
                        WriteAllSamples(ticks, reverb_buf, chorus_buf);

                        //Debug.Log("   block:" + block + " j start:" + ((block + 0) * 2) + " j end:" + ((block + FLUID_BUFSIZE-1) * 2) + " data.Length:" + data.Length );

#if MPTK_PRO
                        ProcessEffect(reverb_buf, chorus_buf);
#endif

                        float vol = MPTK_Volume * volumeStartStop;

                        for (int i = 0; i < FLUID_BUFSIZE; i++)
                        {
                            int j = (block + i) * 2;
                            data[j] = left_buf[i] * vol;
                            data[j + 1] = right_buf[i] * vol;
                        }
                        block += FLUID_BUFSIZE;
                    }

                    StatAudioFilterReadMS = ((float)watchOnAudioFilterRead.ElapsedTicks) / ((float)System.Diagnostics.Stopwatch.Frequency / 1000f);
                    StatDspLoadPCT = StatDeltaAudioFilterReadMS > 0f ? (StatAudioFilterReadMS * 100f) / (float)StatDeltaAudioFilterReadMS : 0f;
#if DEBUG_PERF_AUDIO
                    StatAudioFilterReadMA.Add(Convert.ToInt32(StatAudioFilterReadMS * 1000f));
                    if (StatAudioFilterReadMS > StatAudioFilterReadMAX) StatAudioFilterReadMAX = StatAudioFilterReadMS;
                    if (StatAudioFilterReadMS < StatAudioFilterReadMIN) StatAudioFilterReadMIN = StatAudioFilterReadMS;
                    StatAudioFilterReadAVG = StatAudioFilterReadMA.Average / 1000f;

                    watchPerfAudio.Stop();
                    StatSampleWriteMS = (float)watchPerfAudio.ElapsedTicks / ((float)System.Diagnostics.Stopwatch.Frequency / 1000f);
                    StatSampleWriteMA.Add(Convert.ToInt32(StatSampleWriteMS * 1000f));
                    StatSampleWriteAVG = StatSampleWriteMA.Average / 1000f;

                    StatDspLoadMA.Add(Convert.ToInt32(StatDspLoadPCT * 1000f));
                    //StatDspLoadLongMA.Add(Convert.ToInt32(StatDspLoadPCT * 1000f));
                    if (StatDspLoadPCT > StatDspLoadMAX) StatDspLoadMAX = StatDspLoadPCT;
                    if (StatDspLoadPCT < StatDspLoadMIN) StatDspLoadMIN = StatDspLoadPCT;
                    StatDspLoadAVG = StatDspLoadMA.Average / 1000f;
                    //StatDspLoadLongAVG = StatDspLoadLongMA.Average / 1000f;
#endif
                }
            }
        }

        private void WriteAllSamples(long ticks, float[] reverb_buf, float[] chorus_buf)
        {
            for (int i = 0; i < ActiveVoices.Count; i++)
            {
                fluid_voice voice = ActiveVoices[i];
                //Debug.Log("voice.TimeAtStart :" + voice.TimeAtStart + " System.DateTime.UtcNow.Ticks:" + System.DateTime.UtcNow.Ticks);
                try
                {
                    if (voice.LatenceTick < 0)
                    {
                        voice.LatenceTick = voice.MptkEvent.MPTK_DeltaTimeTick;
                        StatSynthLatency.Add(Convert.ToInt32(voice.LatenceTick));
                        StatSynthLatencyLAST = (float)voice.LatenceTick / (float)fluid_voice.Nano100ToMilli;
                        if (StatSynthLatencyLAST > StatSynthLatencyMAX) StatSynthLatencyMAX = StatSynthLatencyLAST;
                        if (StatSynthLatencyLAST < StatSynthLatencyMIN) StatSynthLatencyMIN = StatSynthLatencyLAST;
                        StatSynthLatencyAVG = (float)StatSynthLatency.Average / (float)fluid_voice.Nano100ToMilli;
                    }

                    if (voice.TimeAtStart <= ticks &&
                       (voice.status == fluid_voice_status.FLUID_VOICE_ON || voice.status == fluid_voice_status.FLUID_VOICE_SUSTAINED))
                        voice.fluid_voice_write(ticks, left_buf, right_buf, reverb_buf, chorus_buf);

                }
                catch (Exception ex)
                {
                    if (VerboseSynth)
                        Debug.LogWarning(ex.Message);
                }
            }
        }

        private void ProcessQueueCommand()
        {
            try
            {
                //if (QueueSynthCommand != null)
                while (QueueSynthCommand.Count > 0)
                {
                    SynthCommand action = QueueSynthCommand.Dequeue();
                    if (action != null)
                    {
                        switch (action.Command)
                        {
                            case SynthCommand.enCmd.StartEvent:
                                MPTK_PlayDirectEvent(action.MidiEvent);
                                break;
                            case SynthCommand.enCmd.StopEvent:
                                StopEvent(action.MidiEvent);
                                break;
                            case SynthCommand.enCmd.ClearAllVoices:
                                ActiveVoices.Clear();
                                break;
                            case SynthCommand.enCmd.NoteOffAll:
                                for (int i = 0; i < ActiveVoices.Count; i++)
                                {
                                    fluid_voice voice = ActiveVoices[i];
                                    if ((voice.IdSession == action.IdSession || action.IdSession == -1) &&
                                        (voice.status == fluid_voice_status.FLUID_VOICE_ON || voice.status == fluid_voice_status.FLUID_VOICE_SUSTAINED))
                                    {
                                        //Debug.LogFormat("ReleaseAll {0} / {1}", voice.IdVoice, ActiveVoices.Count);
                                        voice.fluid_voice_noteoff(true);
                                    }
                                }
                                break;
                        }
                    }
                    else
                        Debug.LogWarning($"OnAudioFilterRead SynthCommand null");
                }
            }
            catch (Exception ex)
            {
                if (VerboseSynth)
                    Debug.LogWarning(ex.Message);
            }
        }

        public void MoveVoiceToFree(fluid_voice v)
        {
            ActiveVoices.RemoveAt(v.IndexActive);
            FreeVoices.Add(v);
        }

        public void DebugVoice()
        {
            foreach (fluid_voice v in ActiveVoices)
            {
                Debug.LogFormat("", v.LastTimeWrite);
            }
        }

        private void MoveVoiceToFree()
        {
#if DEBUG_STATUS_STAT
            // 0: fluid_voice_status.FLUID_VOICE_CLEAN,
            // 1: fluid_voice_status.FLUID_VOICE_ON,
            // 2: fluid_voice_status.FLUID_VOICE_SUSTAINED,
            // 3: fluid_voice_status.FLUID_VOICE_OFF
            // 4: fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE
            StatusStat = new int[(int)fluid_voice_status.FLUID_VOICE_OFF + 2];
#endif
            bool firstToKill = false;
            int countActive = ActiveVoices.Count;
            for (int indexVoice = 0; indexVoice < ActiveVoices.Count;)
            {
                try
                {
                    fluid_voice voice = ActiveVoices[indexVoice];
#if DEBUG_STATUS_STAT
                    if (voice.volenv_section == fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE)
                        StatusStat[(int)fluid_voice_status.FLUID_VOICE_OFF + 1]++;
                    else
                        StatusStat[(int)voice.status]++;
#endif

                    if (StatDspLoadPCT > MaxDspLoad)
                    {
                        if (VerboseSynth) voice.DebugVolEnv($"DSP {StatDspLoadPCT} > {MaxDspLoad} OVERLOAD");
                        // Check if there is voice wich are sustained: Midi message ControlChange with Sustain (64)
                        if (voice.status == fluid_voice_status.FLUID_VOICE_SUSTAINED)
                        {
                            if (VerboseSynth) voice.DebugVolEnv("  Send noteoff sustained");
                            voice.fluid_voice_noteoff(true);
                        }

                        if (voice.volenv_section == fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE)
                        {
                            if (VerboseSynth) voice.DebugVolEnv("  Reduce release time");
                            // reduce release time
                            float count = voice.volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].count;
                            count *= (float)DevicePerformance / 100f;
                            //if (indexVoice == 0) Debug.Log(voice.volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].count + " --> " + count);
                            voice.volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].count = (uint)count;
                        }

                        if (!firstToKill && DevicePerformance <= 25) // V2.82 Try to stop one older voice (the first in the list of active voice)
                        {
                            if (voice.volenv_section == fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD ||
                                voice.volenv_section == fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN)
                            {
                                firstToKill = true;
                                if (VerboseSynth) voice.DebugVolEnv("  Send noteoff at sustain");
                                voice.fluid_voice_noteoff(true);
                            }
                        }
                    }

                    if (voice.status == fluid_voice_status.FLUID_VOICE_OFF)
                    {
                        if (VerboseVoice) Debug.Log($"Voice {voice.IdVoice} - Voice Off, move to FreeVoices");

                        ActiveVoices.RemoveAt(indexVoice);
                        if (MPTK_AutoBuffer)
                            FreeVoices.Add(voice);
                    }
                    else
                    {
                        indexVoice++;
                    }

                }
                catch (Exception ex)
                {
                    if (VerboseSynth)
                        Debug.LogWarning(ex.Message);
                }
            }

#if DEBUG_STATUS_STAT
            if (StatDspLoadPCT > MaxDspLoad)
            {
                Debug.Log(Math.Round(SynthElapsedMilli, 2) +

                    " deltaTimeCore:" + Math.Round(StatDeltaAudioFilterReadMS, 2) +
                    " timeToProcessAudio:" + Math.Round(StatAudioFilterReadMS, 2) +
                    " dspLoad:" + Math.Round(StatDspLoadPCT, 2) +
                    " Active:" + countActive +
                    //" Sustained:" + countSustained +
                    " Clean:" + StatusStat[(int)fluid_voice_status.FLUID_VOICE_CLEAN] +
                    " On:" + StatusStat[(int)fluid_voice_status.FLUID_VOICE_ON] +
                    " Sust:" + StatusStat[(int)fluid_voice_status.FLUID_VOICE_SUSTAINED] +
                    " Off:" + StatusStat[(int)fluid_voice_status.FLUID_VOICE_OFF] +
                    " Released:" + StatusStat[(int)fluid_voice_status.FLUID_VOICE_OFF + 1]);
            }
#endif
        }

        private void AutoCleanVoice(long ticks)
        {
            for (int indexVoice = 0; indexVoice < FreeVoices.Count;)
            {
                try
                {
                    if (FreeVoices.Count > MPTK_AutoCleanVoiceLimit)
                    {
                        fluid_voice voice = FreeVoices[indexVoice];
                        // Is it an older voice ?
                        //if ((Time.realtimeSinceStartup * 1000d - v.TimeAtStart) > AutoCleanVoiceTime)
                        if (((ticks - voice.TimeAtStart) / fluid_voice.Nano100ToMilli) > MPTK_AutoCleanVoiceTime)
                        {
                            if (VerboseVoice) Debug.LogFormat("Remove voice total:{0} id:{1} start:{2}", FreeVoices.Count, voice.IdVoice, (ticks - voice.TimeAtStart) / fluid_voice.Nano100ToMilli);
                            FreeVoices.RemoveAt(indexVoice);
                            if (voice.VoiceAudio != null) Destroy(voice.VoiceAudio.gameObject);
                        }
                        else
                            indexVoice++;
                    }
                    else
                        break;
                }
                catch (Exception ex)
                {
                    if (VerboseSynth)
                        Debug.LogWarning(ex.Message);
                }
            }
        }

        private void ThreadMidiPlayer()
        {
            if (VerboseSynth) Debug.Log($"START ThreadMidiPlayer IdSynth:{IdSynth} state:{state}");
            if (MPTK_DedicatedChannel < 0)
            {
                while (state == fluid_synth_status.FLUID_SYNTH_PLAYING)
                {
                    System.Threading.Thread.Sleep(waitThreadMidi);
                    double nowMs = (double)watchMidi.ElapsedTicks / ((double)System.Diagnostics.Stopwatch.Frequency / 1000d);
                    StatDeltaThreadMidiMS = nowMs - lastTimeMidi;
                    /*if (miditoplay.ReadyToPlay)*/
                    //Debug.Log($"ThreadMidiPlayer IdSynth:'{this.IdSynth}' watchMidi:{Math.Round(nowMs, 2)} lastTimeMidi:{Math.Round(lastTimeMidi, 2)} timeMidiFromStartPlay:{Math.Round(timeMidiFromStartPlay, 2)}  delta:{Math.Round(StatDeltaThreadMidiMS, 2)}");
                    lastTimeMidi = nowMs;

#if DEBUG_PERF_MIDI
                    if (StatDeltaThreadMidiMS > StatDeltaThreadMidiMAX) StatDeltaThreadMidiMAX = StatDeltaThreadMidiMS;
                    if (StatDeltaThreadMidiMS < StatDeltaThreadMidiMIN) StatDeltaThreadMidiMIN = StatDeltaThreadMidiMS;
                    StatDeltaThreadMidiMA.Add(Convert.ToInt32(StatDeltaThreadMidiMS * 1000f));
                    StatDeltaThreadMidiAVG = StatDeltaThreadMidiMA.Average / 1000f;
#endif

                    if (midiLoaded != null)
                    {
                        if (!sequencerPause)
                        {
                            if (midiLoaded.ReadyToPlay)
                            {
                                lock (this)
                                {
                                    timeMidiFromStartPlay += StatDeltaThreadMidiMS;
                                    PlayMidi();
                                }
                            }
                        }
                        else
                        {
                            //Debug.Log(lastTimeMidi + " " + timeToPauseMilliSeconde + " " + pauseMidi.ElapsedMilliseconds);
                            if (timeToPauseMilliSeconde > -1f)
                            {
                                if (pauseMidi.ElapsedMilliseconds > timeToPauseMilliSeconde)
                                {
                                    if (timeMidiFromStartPlay <= 0d) watchMidi.Reset(); // V2.82
                                    watchMidi.Start();
                                    pauseMidi.Stop();
                                    //Debug.Log("Pause ended: " + lastTimeMidi + " " + timeToPauseMilliSeconde + " pauseMidi:" + pauseMidi.ElapsedMilliseconds + " watchMidi:" + watchMidi.ElapsedMilliseconds);
                                    playPause = false;
                                    sequencerPause = false; // V2.82
                                }
                            }
                        }
                    }
                }
            }
            if (VerboseSynth) Debug.Log($"STOP ThreadMidiPlayer IdSynth:{IdSynth} state:{state}");

            midiThread.Abort();
        }


        void PlayMidi()
        {
#if DEBUG_PERF_MIDI
            watchPerfMidi.Reset();
            watchPerfMidi.Start();
#endif

            //EllapseMidi = watchMidi.ElapsedTicks / ((float)System.Diagnostics.Stopwatch.Frequency / 1000f);
            EllapseMidi = watchMidi.ElapsedMilliseconds;
            // Read midi events until this time
            List<MPTKEvent> midievents = midiLoaded.fluid_player_callback((int)EllapseMidi, IdSession);

#if DEBUG_PERF_MIDI
            StatReadMidiMS = (float)watchPerfMidi.ElapsedTicks / ((float)System.Diagnostics.Stopwatch.Frequency / 1000f);
            watchPerfMidi.Reset();
            watchPerfMidi.Start();
#endif

            // Play notes read from the midi file
            if (midievents != null && midievents.Count > 0)
            {
                //lock (this) // V2.83 - no there is already a lock around PlayMidi()
                {
                    QueueMidiEvents.Enqueue(midievents);
                }

#if DEBUG_PERF_MIDI
                StatEnqueueMidiMS = (float)watchPerfMidi.ElapsedTicks / ((float)System.Diagnostics.Stopwatch.Frequency / 1000f);
                watchPerfMidi.Reset();
                watchPerfMidi.Start();
#endif

                if (MPTK_DirectSendToPlayer)
                {
                    foreach (MPTKEvent midievent in midievents)
                    {
                        try
                        {
#if MPTK_PRO
                            if (SpatialSynths != null)
                            {
                                SpatialSynths[midievent.Channel].MPTK_PlayDirectEvent((MPTKEvent)midievent/*.Clone()*/, false);
                            }
                            else
#endif
                                MPTK_PlayDirectEvent(midievent, false);

                        }
                        catch (Exception ex)
                        {
                            Debug.Log($"ThreadMidiPlayer IdSynth:'{this.IdSynth}' {ex.Message}");
                        }
                    }
#if DEBUG_PERF_MIDI
                    StatProcessMidiMS = (float)watchPerfMidi.ElapsedTicks / ((float)System.Diagnostics.Stopwatch.Frequency / 1000f);
                    if (StatProcessMidiMS > StatProcessMidiMAX) StatProcessMidiMAX = StatProcessMidiMS;
                    watchPerfMidi.Reset();
#endif
                }
            }
        }

        //! @endcond

#if DEBUG_PERF_NOTEON
        float perf_time_last;
        public void DebugPerf(string info, int mode = 1)
        {
            // Init
            if (mode == 0)
            {
                watchPerfNoteOn.Reset();
                watchPerfNoteOn.Start();
                perfs = new List<string>();
                perf_time_cumul = 0;
                perf_time_last = 0;
            }

            if (perfs != null)
            {
                //Debug.Log(watchPerfNoteOn.ElapsedTicks+ " " + System.Diagnostics.Stopwatch .IsHighResolution+ " " + System.Diagnostics.Stopwatch.Frequency);
                float now = (float)watchPerfNoteOn.ElapsedTicks / ((float)System.Diagnostics.Stopwatch.Frequency / 1000f);
                perf_time_cumul = now;
                float delta = now - perf_time_last;
                perf_time_last = now;
                string perf = string.Format("{0,-30} \t\t delta:{1:F6} ms \t cumul:{2:F6} ms ", info, delta, perf_time_cumul);
                perfs.Add(perf);
            }

            // Close
            if (mode == 2)
            {
                foreach (string perf in perfs)
                    Debug.Log(perf);
                //Debug.Log(perfs.Last());
            }
        }
#endif
    }
}
