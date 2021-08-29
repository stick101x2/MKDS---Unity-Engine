//#define MPTK_PRO
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;
using UnityEngine.Events;

namespace MidiPlayerTK
{
    public class TestMidiFilePlayerScripting : MonoBehaviour
    {
        /// <summary>
        /// MPTK component able to play a Midi file from your list of Midi file. This PreFab must be present in your scene.
        /// </summary>
        public MidiFilePlayer midiFilePlayer;

        [Header("[Pro] Delay to ramp up volume at startup or down at stop")]
        [Range(0f, 5000f)]
        public float DelayRampMillisecond;

        [Header("Start position in Midi defined in pourcentaage of the whole druration of the Midi")]
        [Range(0f, 100f)]
        public float StartPositionPct;

        [Header("Stop position in Midi defined in pourcentaage of the whole druration of the Midi")]
        [Range(0f, 100f)]
        public float StopPositionPct;

        [Header("Delay to apply random change")]
        [Range(0f, 10f)]
        public float DelayRandomSecond;
        public bool IsRandomPosition;
        public bool IsRandomSpeed;
        public bool IsRandomTranspose;
        public bool IsRandomPlay;

        /// <summary>
        /// When true the transition between two songs is immediate, but a small crossing can occur
        /// </summary>
        public bool IsWaitNotesOff;

        public int CurrentIndexPlaying;

        // Manage skin
        private CustomStyle myStyle;
        private static Color ButtonColor = new Color(.7f, .9f, .7f, 1f);

        private Vector2 scrollerWindow = Vector2.zero;
        private int buttonWidth = 250;
        private PopupListItem PopMidi;

        private string infoMidi;
        private string infoLyrics;
        private string infoCopyright;
        private string infoSeqTrackName;
        private Vector2 scrollPos1 = Vector2.zero;
        private Vector2 scrollPos2 = Vector2.zero;
        private Vector2 scrollPos3 = Vector2.zero;
        private Vector2 scrollPos4 = Vector2.zero;
        private bool toggleChannelDisplay;
        private float LastTimeChange;

        DateTime localStartTimeMidi;
        TimeSpan localDeltaMidi;


        private void Start()
        {
            // if (!HelperDemo.CheckSFExists()) return;

            // Warning: when defined by script, this event is not triggered at first load of MPTK 
            // because MidiPlayerGlobal is loaded before any other gamecomponent
            // To fire this event when application starts, set the event from the inspector.
            if (!MidiPlayerGlobal.OnEventPresetLoaded.HasEvent())
            {
                // To be done in Start event (not Awake)
                MidiPlayerGlobal.OnEventPresetLoaded.AddListener(SoundFontIsReadyEvent);
            }

            PopMidi = new PopupListItem()
            {
                Title = "Select A Midi File",
                OnSelect = MidiChanged,
                Tag = "NEWMIDI",
                ColCount = 3,
                ColWidth = 250,
            };

            // the prefab MidifIlePlayer must be defined in the inspector. You can associated with midiFilePlayer variable
            // with the inspector or by script
            if (midiFilePlayer == null)
            {
                Debug.Log("No MidiFilePLayer defined with the editor inspector, try to find one");
                MidiFilePlayer fp = FindObjectOfType<MidiFilePlayer>();
                if (fp == null)
                    Debug.Log("Can't find a MidiFilePLayer in the Hierarchy. No music will be played");
                else
                {
                    midiFilePlayer = fp;
                }
            }

            if (midiFilePlayer != null)
            {
                // There is two methods to trigger event: 
                //      1) in inpector from the Unity editor 
                //      2) by script, see below 
                // ------------------------------------------

                SetStartEvent();

                // Event trigger when midi file end playing
                if (!midiFilePlayer.OnEventEndPlayMidi.HasEvent())
                {
                    // Set event by script
                    Debug.Log("OnEventEndPlayMidi defined by script");
                    midiFilePlayer.OnEventEndPlayMidi.AddListener(EndPlay);
                }
                else
                    Debug.Log("OnEventEndPlayMidi defined by Unity editor");

                // Event trigger for each group of notes read from midi file
                if (!midiFilePlayer.OnEventNotesMidi.HasEvent())
                {
                    // Set event by scripit
                    Debug.Log("OnEventNotesMidi defined by script");
                    midiFilePlayer.OnEventNotesMidi.AddListener(MidiReadEvents);
                }
                else
                    Debug.Log("OnEventNotesMidi defined by Unity editor");

                InitPlay();
            }
        }

        private void SetStartEvent()
        {
            //! [Example OnEventStartPlayMidi]
            // Event trigger when midi file start playing
            if (!midiFilePlayer.OnEventStartPlayMidi.HasEvent())
            {
                // Set event by script
                Debug.Log("OnEventStartPlayMidi defined by script");
                midiFilePlayer.OnEventStartPlayMidi.AddListener(StartPlay);
            }
            else
                Debug.Log("OnEventStartPlayMidi defined by Unity editor");
            //! [Example OnEventStartPlayMidi]
        }

        /// <summary>
        /// This call is defined from MidiPlayerGlobal event inspector. Run when SF is loaded.
        /// Warning: not triggered at first load of MPTK because MidiPlayerGlobal id load before any other gamecomponent
        /// </summary>
        public void SoundFontIsReadyEvent()
        {
            Debug.LogFormat("End loading SF '{0}', MPTK is ready to play", MidiPlayerGlobal.ImSFCurrent.SoundFontName);
            Debug.Log("   Time To Load SoundFont: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadSoundFont.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Time To Load Samples: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadWave.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Presets Loaded: " + MidiPlayerGlobal.MPTK_CountPresetLoaded);
            Debug.Log("   Samples Loaded: " + MidiPlayerGlobal.MPTK_CountWaveLoaded);
        }

        /// <summary>
        /// Event fired by MidiFilePlayer when a midi is started (set by Unity Editor in MidiFilePlayer Inspector or by script see above)
        /// </summary>
        public void StartPlay(string name)
        {
            infoLyrics = "";
            infoCopyright = "";
            infoSeqTrackName = "";
            localStartTimeMidi = DateTime.Now;
            if (midiFilePlayer != null)
            {
                infoMidi = $"Load time: {midiFilePlayer.midiLoaded.MPTK_LoadTime:F2} milliseconds\n";
                infoMidi += $"Duration: {midiFilePlayer.MPTK_Duration} {midiFilePlayer.MPTK_DurationMS / 1000f:F2} seconds\n";
                infoMidi += $"Track Count: {midiFilePlayer.midiLoaded.MPTK_TrackCount}\n";
                infoMidi += $"Initial Tempo: {midiFilePlayer.midiLoaded.MPTK_InitialTempo:F2}\n";
                infoMidi += $"Delta Ticks Per Quarter: {midiFilePlayer.midiLoaded.MPTK_DeltaTicksPerQuarterNote}\n";
                infoMidi += $"Number Beats Measure: {midiFilePlayer.midiLoaded.MPTK_NumberBeatsMeasure}\n";
                infoMidi += $"Number Quarter Beats: {midiFilePlayer.midiLoaded.MPTK_NumberQuarterBeat}\n";
                infoMidi += $"Count Midi Events: {midiFilePlayer.MPTK_MidiEvents.Count}\n";
                if (StartPositionPct > 0f)
                    midiFilePlayer.MPTK_TickCurrent = (long)((float)midiFilePlayer.MPTK_TickLast * (StartPositionPct / 100f));
            }
            Debug.Log($"Start Play Midi '{name}'  Duration: {midiFilePlayer.MPTK_DurationMS / 1000f:F2} seconds  Load time: {midiFilePlayer.midiLoaded.MPTK_LoadTime:F2} milliseconds");
        }

        /// <summary>
        /// Event fired by MidiFilePlayer when a midi notes are available (set by Unity Editor in MidiFilePlayer Inspector)
        /// </summary>
        public void MidiReadEvents(List<MPTKEvent> events)
        {
            if (StopPositionPct < 100f)
            {
                if (StopPositionPct < StartPositionPct)
                    Debug.LogWarning($"StopPosition ({StopPositionPct} %) is defined before StartPosition ({StartPositionPct} %)");
                if (midiFilePlayer.MPTK_TickCurrent > (long)((float)midiFilePlayer.MPTK_TickLast * (StopPositionPct / 100f)))
                    if (midiFilePlayer.MPTK_Loop)
                        midiFilePlayer.MPTK_RePlay();
                    else
                        midiFilePlayer.MPTK_Next();
            }

            foreach (MPTKEvent midievent in events)
            {
                switch (midievent.Command)
                {
                    case MPTKCommand.NoteOn:
                        //Debug.LogFormat("Note:{0} Velocity:{1} Duration:{2}", midievent.Value, midievent.Velocity, midievent.Duration);
                        break;
                    case MPTKCommand.MetaEvent:
                        switch (midievent.Meta)
                        {
                            case MPTKMeta.TextEvent:
                            case MPTKMeta.Lyric:
                            case MPTKMeta.Marker:
                                // Info from http://gnese.free.fr/Projects/KaraokeTime/Fichiers/karfaq.html and here https://www.mixagesoftware.com/en/midikit/help/HTML/karaoke_formats.html
                                //Debug.Log(midievent.Channel + " " + midievent.Meta + " '" + midievent.Info + "'");
                                string text = midievent.Info.Replace("\\", "\n");
                                text = text.Replace("/", "\n");
                                if (text.StartsWith("@") && text.Length >= 2)
                                {
                                    switch (text[1])
                                    {
                                        case 'K': text = "Type: " + text.Substring(2); break;
                                        case 'L': text = "Language: " + text.Substring(2); break;
                                        case 'T': text = "Title: " + text.Substring(2); break;
                                        case 'V': text = "Version: " + text.Substring(2); break;
                                        default: //I as information, W as copyright, ...
                                            text = text.Substring(2); break;
                                    }
                                    //text += "\n";
                                }
                                infoLyrics += text + "\n";
                                break;

                            case MPTKMeta.Copyright:
                                infoCopyright += midievent.Info + "\n";
                                break;

                            case MPTKMeta.SequenceTrackName:
                                infoSeqTrackName += "Track: " + midievent.Track + " '" + midievent.Info + "'\n";
                                break;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Event fired by MidiFilePlayer when a midi is ended when reach end or stop by MPTK_Stop or Replay with MPTK_Replay
        /// The parameter reason give the origin of the end
        /// </summary>
        public void EndPlay(string name, EventEndMidiEnum reason)
        {
            Debug.LogFormat("End playing midi {0} reason:{1}", name, reason);
        }

        public void InitPlay()
        {
            //midiFilePlayer.MPTK_InitSynth(32);
            if (MidiPlayerGlobal.MPTK_ListMidi != null && MidiPlayerGlobal.MPTK_ListMidi.Count > 0)
            {
                if (IsRandomPlay)
                {
                    // Random select for the Midi
                    int index = UnityEngine.Random.Range(0, MidiPlayerGlobal.MPTK_ListMidi.Count);
                    midiFilePlayer.MPTK_MidiIndex = index;
                    Debug.LogFormat("Random selected midi index{0} name:{1}", index, midiFilePlayer.MPTK_MidiName);
                }
                //GetMidiInfo();
                //midiFilePlayer.MPTK_Play();
                //CurrentIndexPlaying = midiFilePlayer.MPTK_MidiIndex;
            }
        }

        private void MidiChanged(object tag, int midiindex, int indexList)
        {
            Debug.Log("MidiChanged " + midiindex + " for " + tag);
            midiFilePlayer.MPTK_MidiIndex = midiindex;
            midiFilePlayer.MPTK_RePlay();
            // V2.81 Test Play and Pause
            //Debug.Log("MidiChanged **** PlayAndPauseMidi **** " + midiindex + " for " + tag);
            //midiFilePlayer.PlayAndPauseMidi(midiindex, "");//, 5000);
        }

        void OnGUI()
        {
            int spaceV = 10;
            //  if (!HelperDemo.CheckSFExists()) return;

            // Set custom Style. Good for background color 3E619800
            if (myStyle == null)
                myStyle = new CustomStyle();

            if (midiFilePlayer != null)
            {
                scrollerWindow = GUILayout.BeginScrollView(scrollerWindow, false, false, GUILayout.Width(Screen.width));

                // Display popup in first to avoid activate other layout behind
                PopMidi.Draw(MidiPlayerGlobal.MPTK_ListMidi, midiFilePlayer.MPTK_MidiIndex, myStyle);

                MainMenu.Display("Test Midi File Player Scripting - Demonstrate how to use the MPTK API to Play Midi", myStyle, "https://paxstellar.fr/midi-file-player-detailed-view-2/");

                GUISelectSoundFont.Display(scrollerWindow, myStyle);

                //
                // Left column: Midi action
                // ------------------------

                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical(myStyle.BacgDemos, GUILayout.Width(600));
                // Open the popup to select a midi
                if (GUILayout.Button("Current Midi file: '" + midiFilePlayer.MPTK_MidiName + (midiFilePlayer.MPTK_IsPlaying ? "' is playing" : "' is not playing"), GUILayout.Width(500), GUILayout.Height(40)))
                    PopMidi.Show = !PopMidi.Show;
                PopMidi.Position(ref scrollerWindow);

                HelperDemo.DisplayInfoSynth(midiFilePlayer, 600, myStyle);

                GUILayout.Space(spaceV);
                TimeSpan times = TimeSpan.FromMilliseconds(midiFilePlayer.MPTK_Position);
                string playTime = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", times.Hours, times.Minutes, times.Seconds, times.Milliseconds);
                string realDuration = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", midiFilePlayer.MPTK_Duration.Hours, midiFilePlayer.MPTK_Duration.Minutes, midiFilePlayer.MPTK_Duration.Seconds, midiFilePlayer.MPTK_Duration.Milliseconds);

                if (midiFilePlayer.MPTK_IsPlaying)
                    localDeltaMidi = DateTime.Now - localStartTimeMidi;
                GUILayout.Label(string.Format("Real Time: {0} Delta: {1:F3}",
                    string.Format("{0:00}:{1:00}:{2:00}:{3:000}", localDeltaMidi.Hours, localDeltaMidi.Minutes, localDeltaMidi.Seconds, localDeltaMidi.Milliseconds),
                    (times - localDeltaMidi).TotalSeconds),
                    myStyle.TitleLabel3, GUILayout.Width(500));


                GUILayout.BeginHorizontal();
                GUILayout.Label("Time Position: " + playTime + " / " + realDuration, myStyle.TitleLabel3, GUILayout.Width(300));
                double currentPosition = Math.Round(midiFilePlayer.MPTK_Position / 1000d, 2);
                double newPosition = Math.Round(GUILayout.HorizontalSlider((float)currentPosition, 0f, (float)midiFilePlayer.MPTK_Duration.TotalSeconds, GUILayout.Width(150)), 2);
                if (newPosition != currentPosition)
                {
                    if (Event.current.type == EventType.Used)
                    {
                        //Debug.Log("New position " + currentPosition + " --> " + newPosition + " " + Event.current.type);
                        midiFilePlayer.MPTK_Position = newPosition * 1000d;
                    }
                }
                GUILayout.EndHorizontal();


                //Avoid slider with ticks position trigger
                GUILayout.BeginHorizontal();
                GUILayout.Label("Tick Position: " + midiFilePlayer.MPTK_TickCurrent + " / " + midiFilePlayer.MPTK_TickLast, myStyle.TitleLabel3, GUILayout.Width(300));
                long tick = (long)GUILayout.HorizontalSlider((float)midiFilePlayer.MPTK_TickCurrent, 0f, (float)midiFilePlayer.MPTK_TickLast, GUILayout.Width(150));
                if (tick != midiFilePlayer.MPTK_TickCurrent)
                {
                    if (Event.current.type == EventType.Used)
                    {
                        //Debug.Log("New tick " + midiFilePlayer.MPTK_TickCurrent + " --> " + tick + " " + Event.current.type);
                        midiFilePlayer.MPTK_TickCurrent = tick;
                    }
                }
                GUILayout.EndHorizontal();

                //Avoid slider with ticks position trigger
                GUILayout.BeginHorizontal();
                long pos = (long)((float)midiFilePlayer.MPTK_TickLast * (StartPositionPct / 100f));
                GUILayout.Label($"Tick Start Position: {StartPositionPct} % ({pos})", myStyle.TitleLabel3, GUILayout.Width(300));
                StartPositionPct = (long)GUILayout.HorizontalSlider(StartPositionPct, 0f, 100f, GUILayout.Width(150));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                pos = (long)((float)midiFilePlayer.MPTK_TickLast * (StopPositionPct / 100f));
                GUILayout.Label($"Tick Stop Position: {StopPositionPct} % ({pos})", myStyle.TitleLabel3, GUILayout.Width(300));
                StopPositionPct = (long)GUILayout.HorizontalSlider(StopPositionPct, 0f, 100f, GUILayout.Width(150));
                GUILayout.EndHorizontal();

                // Define the global volume
                //GUILayout.Space(spaceV);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Global Volume: " + Math.Round(midiFilePlayer.MPTK_Volume, 2), myStyle.TitleLabel3, GUILayout.Width(220));
                midiFilePlayer.MPTK_Volume = GUILayout.HorizontalSlider(midiFilePlayer.MPTK_Volume * 100f, 0f, 100f, GUILayout.Width(buttonWidth)) / 100f;
                GUILayout.EndHorizontal();

                // Transpose each note
                //GUILayout.Space(spaceV);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Note Transpose: " + midiFilePlayer.MPTK_Transpose, myStyle.TitleLabel3, GUILayout.Width(220));
                midiFilePlayer.MPTK_Transpose = (int)GUILayout.HorizontalSlider((float)midiFilePlayer.MPTK_Transpose, -24f, 24f, GUILayout.Width(buttonWidth));
                GUILayout.EndHorizontal();

                // Transpose each note
                //GUILayout.Space(spaceV);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Speed: " + Math.Round(midiFilePlayer.MPTK_Speed, 2), myStyle.TitleLabel3, GUILayout.Width(220));
                float speed = GUILayout.HorizontalSlider((float)midiFilePlayer.MPTK_Speed, 0.1f, 10f, GUILayout.Width(buttonWidth));
                if (speed != midiFilePlayer.MPTK_Speed)
                {
                    // Avoid event as layout triggered when speed is changed
                    if (Event.current.type == EventType.Used)
                    {
                        //Debug.Log("New speed " + midiFilePlayer.MPTK_Speed + " --> " + speed + " " + Event.current.type);
                        midiFilePlayer.MPTK_Speed = speed;
                    }
                }
                GUILayout.EndHorizontal();


                //GUILayout.Space(spaceV);
                //GUILayout.BeginHorizontal(GUILayout.Width(350));
                //GUILayout.Label("Voices Statistics ", myStyle.TitleLabel3, GUILayout.Width(220));
                //GUILayout.Label(string.Format("Played:{0,-4}   Free:{1,-3}   Active:{2,-3}   Reused:{3} %",
                //    midiFilePlayer.MPTK_StatVoicePlayed, midiFilePlayer.MPTK_StatVoiceCountFree, midiFilePlayer.MPTK_StatVoiceCountActive, Mathf.RoundToInt(midiFilePlayer.MPTK_StatVoiceRatioReused)),
                //    myStyle.TitleLabel3, GUILayout.Width(330));
                //GUILayout.EndHorizontal();

                // Enable or disable channel
                //GUILayout.Space(spaceV);
                //GUILayout.Label("Channel / Preset, enable or disable channel: ", myStyle.TitleLabel3, GUILayout.Width(400));

                toggleChannelDisplay = GUILayout.Toggle(toggleChannelDisplay, "  Display Channels", GUILayout.Width(120));
                //GUILayout.EndHorizontal();

                if (toggleChannelDisplay)
                {
                    GUILayout.BeginVertical();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("Enable All"), GUILayout.Width(100)))
                        for (int channel = 0; channel < midiFilePlayer.MPTK_ChannelCount(); channel++)
                            midiFilePlayer.MPTK_ChannelEnableSet(channel, true);
                    if (GUILayout.Button(new GUIContent("Disable All"), GUILayout.Width(100)))
                        for (int channel = 0; channel < midiFilePlayer.MPTK_ChannelCount(); channel++)
                            midiFilePlayer.MPTK_ChannelEnableSet(channel, false);
                    if (GUILayout.Button(new GUIContent("Default All"), GUILayout.Width(100)))
                        for (int channel = 0; channel < midiFilePlayer.MPTK_ChannelCount(); channel++)
                        {
                            midiFilePlayer.MPTK_ChannelEnableSet(channel, true);
                            midiFilePlayer.MPTK_ChannelForcedPresetSet(channel, -1);
                            midiFilePlayer.MPTK_ChannelVolumeSet(channel, 1);
                        }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Channel  Preset                                 Forced             Count             Enabled               Volume", myStyle.TitleLabel3);
                    GUILayout.EndHorizontal();
                    for (int channel = 0; channel < midiFilePlayer.MPTK_ChannelCount(); channel++)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(string.Format("   {0:00}", channel), myStyle.TitleLabel3, GUILayout.Width(60)); /*2.84*/
                        GUILayout.Label(midiFilePlayer.MPTK_ChannelPresetGetName(channel) ?? "not set", myStyle.TitleLabel3, GUILayout.MaxWidth(150));

                        GUILayout.Label($"{midiFilePlayer.MPTK_ChannelForcedPresetGet(channel)}/{midiFilePlayer.MPTK_ChannelBankGetIndex(channel)}", myStyle.LabelRight/*, GUILayout.Width(80)*/);
                        int forcedPreset = (int)GUILayout.HorizontalSlider(midiFilePlayer.MPTK_ChannelForcedPresetGet(channel), -1f, 127f, myStyle.SliderBar, myStyle.SliderThumb, GUILayout.MaxWidth(50));
                        if (forcedPreset != midiFilePlayer.MPTK_ChannelForcedPresetGet(channel))
                        {
                            midiFilePlayer.MPTK_ChannelForcedPresetSet(channel, forcedPreset);
                        }


                        GUILayout.Label(string.Format("{0,-5}", midiFilePlayer.MPTK_ChannelNoteCount(channel)), myStyle.LabelRight, GUILayout.Width(60));
                        //GUILayout.Label("Enabled", myStyle.LabelRight, GUILayout.Width(60));

                        GUILayout.Label("   ", myStyle.TitleLabel3, GUILayout.Width(60));
                        bool state = GUILayout.Toggle(midiFilePlayer.MPTK_ChannelEnableGet(channel), "", GUILayout.Width(20));
                        if (state != midiFilePlayer.MPTK_ChannelEnableGet(channel))
                        {
                            midiFilePlayer.MPTK_ChannelEnableSet(channel, state);
                            Debug.LogFormat("Channel {0} state:{1}, preset:{2}", channel, state, midiFilePlayer.MPTK_ChannelPresetGetName(channel) ?? "not set"); /*2.84*/
                        }

                        GUILayout.Label($"{Math.Round(midiFilePlayer.MPTK_ChannelVolumeGet(channel), 2)}", myStyle.LabelRight, GUILayout.Width(80));
                        float volume = GUILayout.HorizontalSlider((float)midiFilePlayer.MPTK_ChannelVolumeGet(channel), 0f, 1f, myStyle.SliderBar, myStyle.SliderThumb, GUILayout.MaxWidth(50));
                        if (volume != midiFilePlayer.MPTK_ChannelVolumeGet(channel))
                        {
                            midiFilePlayer.MPTK_ChannelVolumeSet(channel, volume);
                        }

                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                }

                // Random playing ?
                GUILayout.Space(spaceV);
                GUILayout.BeginHorizontal();
                IsRandomPlay = GUILayout.Toggle(IsRandomPlay, "  Random Play Midi", GUILayout.Width(220));

                // Weak device ?
                //midiFilePlayer.MPTK_WeakDevice = GUILayout.Toggle(midiFilePlayer.MPTK_WeakDevice, "Weak Device", GUILayout.Width(220));
                GUILayout.EndHorizontal();
                GUILayout.Space(spaceV);

                // Play/Pause/Stop/Restart actions on midi 
                GUILayout.BeginHorizontal(GUILayout.Width(500));
                if (midiFilePlayer.MPTK_IsPlaying && !midiFilePlayer.MPTK_IsPaused)
                    GUI.color = ButtonColor;
                if (GUILayout.Button(new GUIContent("Play", "")))
                    midiFilePlayer.MPTK_Play();
                GUI.color = Color.white;

                if (midiFilePlayer.MPTK_IsPaused)
                    GUI.color = ButtonColor;
                if (GUILayout.Button(new GUIContent("Pause", "")))
                    if (midiFilePlayer.MPTK_IsPaused)
                        midiFilePlayer.MPTK_UnPause();
                    else
                        midiFilePlayer.MPTK_Pause();
                GUI.color = Color.white;

                if (GUILayout.Button(new GUIContent("Stop", "")))
                    midiFilePlayer.MPTK_Stop();

                if (GUILayout.Button(new GUIContent("Restart", "")))
                    midiFilePlayer.MPTK_RePlay();

                if (GUILayout.Button(new GUIContent("Clear", "")))
                    midiFilePlayer.MPTK_ClearAllSound(true);

                GUILayout.EndHorizontal();

#if MPTK_PRO
                GUILayout.BeginHorizontal(GUILayout.Width(500));
                if (GUILayout.Button(new GUIContent($"Play Ramp-Up {(int)DelayRampMillisecond} ms", "")))
                    midiFilePlayer.MPTK_Play(DelayRampMillisecond);
                if (GUILayout.Button(new GUIContent($"Stop Downward {(int)DelayRampMillisecond} ms", "")))
                    midiFilePlayer.MPTK_Stop(DelayRampMillisecond);
                GUILayout.Label("Delay", myStyle.TitleLabel3, GUILayout.Width(50));
                DelayRampMillisecond = GUILayout.HorizontalSlider(DelayRampMillisecond, 0f, 5000f, myStyle.SliderBar, myStyle.SliderThumb, GUILayout.Width(120));
                GUILayout.EndHorizontal();
#endif
                // Previous and Next button action on midi
                GUILayout.BeginHorizontal(GUILayout.Width(500));
                if (GUILayout.Button(new GUIContent("Previous", "")))
                {
                    if (IsWaitNotesOff)
                        StartCoroutine(NextPreviousWithWait(false));
                    else
                    {
                        midiFilePlayer.MPTK_Previous();
                        CurrentIndexPlaying = midiFilePlayer.MPTK_MidiIndex;
                    }
                }
                if (GUILayout.Button(new GUIContent("Next", "")))
                {
                    if (IsWaitNotesOff)
                        StartCoroutine(NextPreviousWithWait(true));
                    else
                    {
                        midiFilePlayer.MPTK_Next();
                        CurrentIndexPlaying = midiFilePlayer.MPTK_MidiIndex;
                    }
                    //Debug.Log("MPTK_Next - CurrentIndexPlaying " + CurrentIndexPlaying);
                }
                IsWaitNotesOff = GUILayout.Toggle(IsWaitNotesOff, "  Wait all notes off", GUILayout.Width(120));
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                if (!string.IsNullOrEmpty(infoMidi) || !string.IsNullOrEmpty(infoLyrics) || !string.IsNullOrEmpty(infoCopyright) || !string.IsNullOrEmpty(infoSeqTrackName))
                {
                    //
                    // Right Column: midi infomation, lyrics, ...
                    // ------------------------------------------
                    GUILayout.BeginVertical(myStyle.BacgDemos);
                    if (!string.IsNullOrEmpty(infoMidi))
                    {
                        scrollPos1 = GUILayout.BeginScrollView(scrollPos1, false, true);//, GUILayout.Height(heightLyrics));
                        GUILayout.Label(infoMidi, myStyle.TextFieldMultiLine);
                        GUILayout.EndScrollView();
                    }
                    GUILayout.Space(5);
                    if (!string.IsNullOrEmpty(infoLyrics))
                    {
                        GUILayout.Label("Lyrics");
                        //Debug.Log(scrollPos + " " + countline+ " " + myStyle.TextFieldMultiLine.CalcHeight(new GUIContent(lyrics), 400));
                        //float heightLyrics = myStyle.TextFieldMultiLine.CalcHeight(new GUIContent(infoLyrics), 400);
                        //scrollPos.y = - 340;
                        //if (heightLyrics > 200) heightLyrics = 200;
                        scrollPos2 = GUILayout.BeginScrollView(scrollPos2, false, true);//, GUILayout.Height(heightLyrics));
                        GUILayout.Label(infoLyrics, myStyle.TextFieldMultiLine);
                        GUILayout.EndScrollView();
                        //if (GUILayout.Button(new GUIContent("Add", ""))) lyrics += "\ntestest testetst";
                    }
                    GUILayout.Space(5);
                    if (!string.IsNullOrEmpty(infoCopyright))
                    {
                        GUILayout.Label("Copyright");
                        scrollPos3 = GUILayout.BeginScrollView(scrollPos3, false, true);
                        GUILayout.Label(infoCopyright, myStyle.TextFieldMultiLine);
                        GUILayout.EndScrollView();
                    }
                    GUILayout.Space(5);
                    if (!string.IsNullOrEmpty(infoSeqTrackName))
                    {
                        GUILayout.Label("Track Name");
                        scrollPos4 = GUILayout.BeginScrollView(scrollPos4, false, true);
                        GUILayout.Label(infoSeqTrackName, myStyle.TextFieldMultiLine);
                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(myStyle.BacgDemos);
                GUILayout.Label("Go to your Hierarchy, select GameObject MidiFilePlayer: inspector contains a lot of parameters to control the sound.", myStyle.TitleLabel2);
                GUILayout.EndHorizontal();

                GUILayout.EndScrollView();
            }

        }

        /// <summary>
        /// Coroutine: stop current midi playing, wait until all samples are off and go next or previous midi
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public IEnumerator NextPreviousWithWait(bool next)
        {
            midiFilePlayer.MPTK_Stop();

            yield return midiFilePlayer.MPTK_WaitAllNotesOff(midiFilePlayer.IdSession);
            if (next)
                midiFilePlayer.MPTK_Next();
            else
                midiFilePlayer.MPTK_Previous();
            CurrentIndexPlaying = midiFilePlayer.MPTK_MidiIndex;

            yield return 0;
        }

        /// <summary>
        /// Event fired by MidiFilePlayer when a midi is ended (set by Unity Editor in MidiFilePlayer Inspector)
        /// </summary>
        public void RandomPlay()
        {
            if (IsRandomPlay)
            {
                //Debug.Log("Is playing : " + midiFilePlayer.MPTK_IsPlaying);
                int index = UnityEngine.Random.Range(0, MidiPlayerGlobal.MPTK_ListMidi.Count);
                midiFilePlayer.MPTK_MidiIndex = index;
                midiFilePlayer.MPTK_Play();
            }
            else
                midiFilePlayer.MPTK_RePlay();
        }

        //Update is called once per frame
        void Update()
        {
            if (midiFilePlayer != null && midiFilePlayer.MPTK_IsPlaying)
            {
                float time = Time.realtimeSinceStartup - LastTimeChange;
                if (DelayRandomSecond > 0f && time > DelayRandomSecond)
                {
                    // It's time to apply randon change
                    LastTimeChange = Time.realtimeSinceStartup;

                    // Random position
                    if (IsRandomPosition) midiFilePlayer.MPTK_Position = UnityEngine.Random.Range(0f, (float)midiFilePlayer.MPTK_Duration.TotalMilliseconds);

                    // Random Speed
                    if (IsRandomSpeed) midiFilePlayer.MPTK_Speed = UnityEngine.Random.Range(0.1f, 5f);

                    // Random transmpose
                    if (IsRandomTranspose) midiFilePlayer.MPTK_Transpose = UnityEngine.Random.Range(-12, 13);
                }
            }
        }
    }
}