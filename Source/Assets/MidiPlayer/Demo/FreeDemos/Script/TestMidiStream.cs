//#define MPTK_PRO
//#define DEBUG_MULTI
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;

namespace MidiPlayerTK
{
    public class TestMidiStream : MonoBehaviour
    {

        // MPTK component able to play a stream of midi events
        // Add a MidiStreamPlayer Prefab to your game object and defined midiStreamPlayer in the inspector with this prefab.
        public MidiStreamPlayer midiStreamPlayer;

        [Range(0.05f, 10f)]
        public float Frequency = 1;

        [Range(-10f, 100f)]
        public float NoteDuration = 0;

        [Range(0f, 10f)]
        public float NoteDelay = 0;


        public bool RandomPlay = true;
        public bool DrumKit = false;
        public bool ChordPlay = false;
        public int ArpeggioPlayChord = 0;
        public int DelayPlayScale = 200;
        public bool ChordLibPlay = false;
        public bool RangeLibPlay = false;
        public int CurrentChord;

        [Range(0, 127)]
        public int StartNote = 50;

        [Range(0, 127)]
        public int EndNote = 60;

        [Range(0, 127)]
        public int Velocity = 100;

        [Range(0, 16)]
        public int StreamChannel = 0;

        [Range(0, 127)]
        public int CurrentNote;

        [Range(0, 127)]
        public int StartPreset = 0;

        [Range(0, 127)]
        public int EndPreset = 127;

        [Range(0, 127)]
        public int CurrentPreset;

        [Range(0, 127)]
        public int CurrentPatchDrum;

        [Range(0, 127)]
        public int PanChange;

        [Range(0, 127)]
        public int ModChange;

        const float DEFAULT_PITCH = 64;
        [Range(0, 127)]
        public float PitchChange = DEFAULT_PITCH;
        private float currentVelocityPitch;
        private float LastTimePitchChange;

        [Range(0, 127)]
        public int ExpChange;

        public int CountNoteToPlay = 1;
        public int CountNoteChord = 3;
        public int DegreeChord = 1;
        public int CurrentScale = 0;

        /// <summary>
        /// Current note playing
        /// </summary>
        private MPTKEvent NotePlaying;

        private float LastTimeChange;

        /// <summary>
        /// Popup to select an instrument
        /// </summary>
        private PopupListItem PopPatchInstrument;
        private PopupListItem PopBankInstrument;
        private PopupListItem PopPatchDrum;
        private PopupListItem PopBankDrum;

        // Popup to select a realtime generator
        private PopupListItem[] PopGenerator;
        private int[] indexGenerator;
        private string[] labelGenerator;
        private float[] valueGenerator;
        private const int nbrGenerator = 4;

        // Manage skin
        public CustomStyle myStyle;

        private Vector2 scrollerWindow = Vector2.zero;
        private int buttonWidth = 250;
        private float spaceVertival = 0;
        private float widthFirstCol = 100;
        public bool IsplayingLoopNotes;
        public bool IsplayingLoopPresets;

        private void Awake()
        {
            if (midiStreamPlayer != null)
            {
                // The call of this method can also be defined in the prefab MidiStreamPlayer
                // from the Unity editor inspector. See "On Event Synth Awake". 
                // StartLoadingSynth will be called just before the initialization of the synthesizer.
                // Warning: depending on the starting order of the GameObjects, 
                //          this call may be missed if MidiStreamPlayer is started before TestMidiStream, 
                //          so it is preferable to define this event in the inspector.
                if (!midiStreamPlayer.OnEventSynthAwake.HasEvent())
                    midiStreamPlayer.OnEventSynthAwake.AddListener(StartLoadingSynth);

                // The call of this method can also be defined in the prefab MidiStreamPlayer 
                // from the Unity editor inspector. See "On Event Synth Started.
                // EndLoadingSynth will be called when the synthesizer is ready.
                if (!midiStreamPlayer.OnEventSynthStarted.HasEvent())
                    midiStreamPlayer.OnEventSynthStarted.AddListener(EndLoadingSynth);
            }
            else
                Debug.LogWarning("midiStreamPlayer is not defined. Check in Unity editor inspector of this gameComponent");
        }

        // Use this for initialization
        void Start()
        {
            // Define popup to display to select preset and bank
            PopBankInstrument = new PopupListItem() { Title = "Select A Bank", OnSelect = BankPatchChanged, Tag = "BANK_INST", ColCount = 5, ColWidth = 150, };
            PopPatchInstrument = new PopupListItem() { Title = "Select A Patch", OnSelect = BankPatchChanged, Tag = "PATCH_INST", ColCount = 5, ColWidth = 150, };
            PopBankDrum = new PopupListItem() { Title = "Select A Bank", OnSelect = BankPatchChanged, Tag = "BANK_DRUM", ColCount = 5, ColWidth = 150, };
            PopPatchDrum = new PopupListItem() { Title = "Select A Patch", OnSelect = BankPatchChanged, Tag = "PATCH_DRUM", ColCount = 5, ColWidth = 150, };

            GenModifier.InitListGenerator();
            indexGenerator = new int[nbrGenerator];
            labelGenerator = new string[nbrGenerator];
            valueGenerator = new float[nbrGenerator];
            PopGenerator = new PopupListItem[nbrGenerator];
            for (int i = 0; i < nbrGenerator; i++)
            {
                indexGenerator[i] = GenModifier.RealTimeGenerator[0].Index;
                labelGenerator[i] = GenModifier.RealTimeGenerator[0].Label;
                if (indexGenerator[i] >= 0)
                    valueGenerator[i] = GenModifier.DefaultNormalizedVal((fluid_gen_type)indexGenerator[i]) * 100f;
                PopGenerator[i] = new PopupListItem() { Title = "Select A Generator", OnSelect = GeneratorChanged, Tag = i, ColCount = 3, ColWidth = 250, };
            }
            LastTimeChange = Time.realtimeSinceStartup;
            CurrentNote = StartNote;
            PanChange = 64;
            LastTimeChange = -9999999f;
            PitchChange = DEFAULT_PITCH;
            CountNoteToPlay = 1;
        }

        /// <summary>
        /// The call of this method is defined in MidiPlayerGlobal (it's a son of the prefab MidiStreamPlayer) from the Unity editor inspector. 
        /// The method is called when SoundFont is loaded. We use it only to statistics purpose.
        /// </summary>
        public void EndLoadingSF()
        {
            Debug.Log("End loading SoundFont. Statistics: ");

            Debug.Log("List of presets available");
            foreach (MPTKListItem preset in MidiPlayerGlobal.MPTK_ListPreset)
                Debug.Log($"   [{preset.Index,3:000}] - {preset.Label}");

            Debug.Log("   Time To Load SoundFont: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadSoundFont.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Time To Load Samples: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadWave.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Presets Loaded: " + MidiPlayerGlobal.MPTK_CountPresetLoaded);
            Debug.Log("   Samples Loaded: " + MidiPlayerGlobal.MPTK_CountWaveLoaded);

        }

        public void StartLoadingSynth(string name)
        {
            Debug.LogFormat($"Start loading Synth {name}");
        }

        /// <summary>
        /// This methods is run when the synthesizer is ready if you defined OnEventSynthStarted or set event from Inspector in Unity.
        /// It's a good place to set some synth parameter's as defined preset by channel 
        /// </summary>
        /// <param name="name"></param>
        public void EndLoadingSynth(string name)
        {
            Debug.LogFormat($"Synth {name} is loaded");

            // Set piano (preset 0) to channel 0. Could be different for another SoundFont.
            midiStreamPlayer.MPTK_ChannelPresetChange(0, 0);
            Debug.LogFormat($"Preset {midiStreamPlayer.MPTK_ChannelPresetGetName(0)} defined on channel 0");

            // Set reed organ (preset 20) to channel 1. Could be different for another SoundFont.
            midiStreamPlayer.MPTK_ChannelPresetChange(1, 20);
            Debug.LogFormat($"Preset {midiStreamPlayer.MPTK_ChannelPresetGetName(1)} defined on channel 1");
        }

        public bool testLocalchange = false;
        private void BankPatchChanged(object tag, int index, int indexList)
        {
            switch ((string)tag)
            {
                case "BANK_INST":
                    MidiPlayerGlobal.MPTK_SelectBankInstrument(index);
                    midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.ControlChange, Controller = MPTKController.BankSelect, Value = index, Channel = StreamChannel, });
                    break;

                case "PATCH_INST":
                    CurrentPreset = index;
                    if (testLocalchange)
                        midiStreamPlayer.MPTK_ChannelPresetChange(StreamChannel, index);
                    else
                        midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.PatchChange, Value = index, Channel = StreamChannel, });
                    break;

                case "BANK_DRUM":
                    MidiPlayerGlobal.MPTK_SelectBankDrum(index);
                    midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.ControlChange, Controller = MPTKController.BankSelect, Value = index, Channel = 9, });
                    break;

                case "PATCH_DRUM":
                    CurrentPatchDrum = index;
                    if (testLocalchange)
                        midiStreamPlayer.MPTK_ChannelPresetChange(9, index);
                    else
                        midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.PatchChange, Value = index, Channel = 9 });
                    break;
            }
        }

        private void GeneratorChanged(object tag, int index, int indexList)
        {
            int iGenerator = Convert.ToInt32(tag);
            indexGenerator[iGenerator] = index;
            labelGenerator[iGenerator] = GenModifier.RealTimeGenerator[indexList].Label;
            valueGenerator[iGenerator] = GenModifier.DefaultNormalizedVal((fluid_gen_type)indexGenerator[iGenerator]) * 100f;
            Debug.Log($"indexList:{indexList} indexGenerator:{indexGenerator[iGenerator]} valueGenerator:{valueGenerator[iGenerator]} {labelGenerator[iGenerator]}");
        }

        void OnGUI()
        {
            // Set custom Style.
            if (myStyle == null) myStyle = new CustomStyle();

            // midiStreamPlayer must be defined with the inspector of this gameObject 
            // Otherwise  you could use : midiStreamPlayer fp = FindObjectOfType<MidiStreamPlayer>(); in the Start() method
            if (midiStreamPlayer != null)
            {
                scrollerWindow = GUILayout.BeginScrollView(scrollerWindow, false, false, GUILayout.Width(Screen.width));

                // If need, display the popup  before any other UI to avoid trigger it hidden
                if (HelperDemo.CheckSFExists())
                {
                    PopBankInstrument.Draw(MidiPlayerGlobal.MPTK_ListBank, MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber, myStyle);
                    PopPatchInstrument.Draw(MidiPlayerGlobal.MPTK_ListPreset, CurrentPreset, myStyle);
                    PopBankDrum.Draw(MidiPlayerGlobal.MPTK_ListBank, MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber, myStyle);
                    PopPatchDrum.Draw(MidiPlayerGlobal.MPTK_ListPresetDrum, CurrentPatchDrum, myStyle);

                    for (int i = 0; i < nbrGenerator; i++)
                        PopGenerator[i].Draw(GenModifier.RealTimeGenerator, indexGenerator[i], myStyle);

                    MainMenu.Display("Test Midi Stream - A very simple Generated Music Stream ", myStyle, "https://paxstellar.fr/midi-file-player-detailed-view-2-2/");

                    // Display soundfont available and select a new one
                    GUISelectSoundFont.Display(scrollerWindow, myStyle);

                    //
                    // Select bank & Patch for Instrument
                    // ----------------------------------
                    GUILayout.BeginVertical(myStyle.BacgDemos);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Instrument", myStyle.TitleLabel3, GUILayout.Width(widthFirstCol));

                    // Open the popup to select a bank
                    if (GUILayout.Button(MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber + " - Bank", GUILayout.Width(buttonWidth)))
                        PopBankInstrument.Show = !PopBankInstrument.Show;
                    PopBankInstrument.Position(ref scrollerWindow);

                    // Open the popup to select an instrument
                    if (GUILayout.Button(
                        CurrentPreset.ToString() + " - " +
                        MidiPlayerGlobal.MPTK_GetPatchName(MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber,
                        CurrentPreset),
                        GUILayout.Width(buttonWidth)))
                        PopPatchInstrument.Show = !PopPatchInstrument.Show;
                    PopPatchInstrument.Position(ref scrollerWindow);

                    StreamChannel = (int)Slider("Channel", StreamChannel, 0, 15, true, 100);

                    GUILayout.EndHorizontal();

                    //
                    // Select bank & Patch for Drum
                    // ----------------------------
                    GUILayout.Space(spaceVertival);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Drum", myStyle.TitleLabel3, GUILayout.Width(widthFirstCol));

                    // Open the popup to select a bank for drum
                    if (GUILayout.Button(MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber + " - Bank", GUILayout.Width(buttonWidth)))
                        PopBankDrum.Show = !PopBankDrum.Show;
                    PopBankDrum.Position(ref scrollerWindow);

                    // Open the popup to select an instrument for drum
                    if (GUILayout.Button(
                        CurrentPatchDrum.ToString() + " - " +
                        MidiPlayerGlobal.MPTK_GetPatchName(MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber, CurrentPatchDrum),
                        GUILayout.Width(buttonWidth)))
                        PopPatchDrum.Show = !PopPatchDrum.Show;
                    PopPatchDrum.Position(ref scrollerWindow);

                    GUILayout.Label(" ", GUILayout.Width(42));

                    bool newDrumKit = GUILayout.Toggle(DrumKit, "Activate Drum Mode", GUILayout.Width(buttonWidth * 2));
                    if (newDrumKit != DrumKit)
                    {
                        DrumKit = newDrumKit;
                        // Set canal to dedicated drum canal 9 
                        StreamChannel = DrumKit ? 9 : 0;
                    }
                    GUILayout.EndHorizontal();
                }
                else
                    GUILayout.BeginVertical(myStyle.BacgDemos);

                GUILayout.Space(spaceVertival);

                //
                // Display info and synth stats
                // ----------------------------
                HelperDemo.DisplayInfoSynth(midiStreamPlayer, 500, myStyle);

                GUILayout.Space(spaceVertival);

                //
                // Play one note 
                // --------------
                GUILayout.BeginVertical(myStyle.BacgDemos1);

                GUILayout.BeginHorizontal(GUILayout.Width(350));
                GUILayout.Label("One Shot", myStyle.TitleLabel3, GUILayout.Width(widthFirstCol));
                if (GUILayout.Button("Play", myStyle.BtStandard, GUILayout.Width(buttonWidth * 0.666f)))
                {
                    // Stop current note if playing
                    StopOneNote();
                    // Play one note 
                    PlayOneNote();
                }
                if (GUILayout.Button("Stop", myStyle.BtStandard, GUILayout.Width(buttonWidth * 0.666f)))
                {
                    StopOneNote();
                    StopChord();
                }

                if (GUILayout.Button("Clear", myStyle.BtStandard, GUILayout.Width(buttonWidth * 0.666f)))
                {
                    midiStreamPlayer.MPTK_ClearAllSound(true);
                }

                if (GUILayout.Button("Re-init", myStyle.BtStandard, GUILayout.Width(buttonWidth * 0.666f)))
                {
                    midiStreamPlayer.MPTK_InitSynth();
                    CurrentPreset = CurrentPatchDrum = 0;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.Width(500));

                //if (GUILayout.Button("Test", myStyle.BtStandard, GUILayout.Width(buttonWidth * 0.666f)))
                //{
                //    //midiStreamPlayer.MPTK_KillByExclusiveClass = false;

                //    NotePlaying = new MPTKEvent() { Command = MPTKCommand.NoteOn, Value = 36, Channel = 9, Duration = 1000, Velocity = 10, };// Bass_drum channel 9
                //    midiStreamPlayer.MPTK_PlayEvent(NotePlaying);

                //    NotePlaying = new MPTKEvent() { Command = MPTKCommand.NoteOn, Value = 42, Channel = 9, Duration = 1000, Velocity = 80, };// Closed Hihat channel 9 
                //    midiStreamPlayer.MPTK_PlayEvent(NotePlaying);
                //}

                CurrentNote = (int)Slider("Note", CurrentNote, 0, 127);
                int preset = (int)Slider("Preset", CurrentPreset, 0, 127, true);
                if (preset != CurrentPreset)
                {
                    CurrentPreset = preset;
                    midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent()
                    {
                        Command = MPTKCommand.PatchChange,
                        Value = CurrentPreset,
                        Channel = StreamChannel,
                    });
                }
                NoteDuration = Slider("Duration", NoteDuration, -1f, 10f, true);
                NoteDelay = Slider("Delay", NoteDelay, 0f, 1f, true);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                GUILayout.Space(spaceVertival);

                //
                // Play note loop and preset loop
                // ------------------------------
                GUILayout.BeginVertical(myStyle.BacgDemos1);
                GUILayout.BeginHorizontal(GUILayout.Width(500));
                GUILayout.Label("Loop on Notes and Presets", myStyle.TitleLabel3, GUILayout.Width(220));
                Frequency = Slider("Loop Delay", Frequency, 0.01f, 1f, true);
                GUILayout.Label(" ", myStyle.TitleLabel3, GUILayout.Width(30));
                RandomPlay = GUILayout.Toggle(RandomPlay, "Random Notes", GUILayout.Width(120));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.Width(350));
                GUILayout.Label("Loop Notes", myStyle.TitleLabel3, GUILayout.Width(widthFirstCol));
                if (GUILayout.Button("Start / Stop", IsplayingLoopNotes ? myStyle.BtSelected : myStyle.BtStandard, GUILayout.Width(buttonWidth * 0.666f))) IsplayingLoopNotes = !IsplayingLoopNotes;
                StartNote = (int)Slider("From", StartNote, 0, 127, true);
                EndNote = (int)Slider("To", EndNote, 0, 127, true);
                GUILayout.EndHorizontal();

                GUILayout.Space(spaceVertival);

                GUILayout.BeginHorizontal(GUILayout.Width(350));
                GUILayout.Label("Loop Presets", myStyle.TitleLabel3, GUILayout.Width(widthFirstCol));
                if (GUILayout.Button("Start / Stop", IsplayingLoopPresets ? myStyle.BtSelected : myStyle.BtStandard, GUILayout.Width(buttonWidth * 0.666f))) IsplayingLoopPresets = !IsplayingLoopPresets;
                StartPreset = (int)Slider("From", StartPreset, 0, 127, true);
                EndPreset = (int)Slider("To", EndPreset, 0, 127, true);
                GUILayout.EndHorizontal();


                GUILayout.EndVertical(); // End play note loop and preset loop

                GUILayout.Space(spaceVertival);
#if DEBUG_MULTI
                GUILayout.BeginHorizontal();
                GUILayout.Label(" ", myStyle.TitleLabel3, GUILayout.Width(widthFirstCol));
                CountNoteToPlay = (int)Slider("Play Multiple Notes", CountNoteToPlay, 1, 200, false, 70);
                GUILayout.EndHorizontal();
#endif
                //
                // Build chord and scale (Pro)
                // ---------------------------
                GUILayout.BeginVertical(myStyle.BacgDemos1);
                GUILayout.Label("Build Chord and Range [Availablle with MPTK Pro]", myStyle.TitleLabel3);

                GUILayout.BeginHorizontal();
                GUILayout.Label(" ", myStyle.TitleLabel3, GUILayout.Width(widthFirstCol));

                ChordPlay = GUILayout.Toggle(ChordPlay, "Play Chord From Degree", GUILayout.Width(170));
                if (ChordPlay) { ChordLibPlay = false; RangeLibPlay = false; }

                ChordLibPlay = GUILayout.Toggle(ChordLibPlay, "Play Chord From Lib", GUILayout.Width(170));
                if (ChordLibPlay) { ChordPlay = false; RangeLibPlay = false; }

                RangeLibPlay = GUILayout.Toggle(RangeLibPlay, "Play Range From Lib", GUILayout.Width(170));
                if (RangeLibPlay) { ChordPlay = false; ChordLibPlay = false; }

                GUILayout.EndHorizontal();

                // Build a chord from degree
                if (ChordPlay)
                {
                    BuildChordFromdegree();
                }

                // Build a chord from a library
                if (ChordLibPlay)
                {
                    BuildChordFromLibrary();
                }

                if (RangeLibPlay)
                {
                    BuildRangeFromLib();
                }
                //#else
                //if (ChordPlay || ChordLibPlay || RangeLibPlay)
                //{
                //    GUILayout.BeginVertical(myStyle.BacgDemos1);
                //    GUILayout.Space(spaceVertival);
                //    GUILayout.Label("Chord and Range are available only with MPTK PRO", myStyle.TitleLabel3);
                //    GUILayout.Space(spaceVertival);
                //    GUILayout.EndVertical();
                //}
                //#endif
                GUILayout.EndVertical(); // End build chord and scale (Pro)

                GUILayout.Space(spaceVertival);

                //
                // Change value from Midi Command
                // ------------------------------
                GUILayout.BeginVertical(myStyle.BacgDemos1);
                GUILayout.Label("Real Time Midi Command Change", myStyle.TitleLabel3);

                GUILayout.BeginHorizontal(GUILayout.Width(350));

                // Change volume
                midiStreamPlayer.MPTK_Volume = Slider("Volume", midiStreamPlayer.MPTK_Volume, 0, 1);

                // Change pitch (automatic return to center as a physical keyboard!)
                float pitchChange = Slider("Pitch", PitchChange, 0, 127, true);
                if (pitchChange != PitchChange)
                {
                    LastTimePitchChange = Time.realtimeSinceStartup;
                    PitchChange = pitchChange;
                    midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.PitchWheelChange, Value = (int)PitchChange << 7, Channel = StreamChannel });
                }
                midiStreamPlayer.MPTK_Transpose = (int)Slider("Transpose", midiStreamPlayer.MPTK_Transpose, -24, 24, true);

                GUILayout.EndHorizontal();

                GUILayout.Space(spaceVertival);

                // Change velocity of the note: what force is applied on the key. Change volume and sound of the note.
                GUILayout.BeginHorizontal(GUILayout.Width(350));
                Velocity = (int)Slider("Velocity", (int)Velocity, 0f, 127f);

                // Change left / right stereo
                int panChange = (int)Slider("Panoramic", PanChange, 0, 127, true);
                if (panChange != PanChange)
                {
                    PanChange = panChange;
                    midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.ControlChange, Controller = MPTKController.Pan, Value = PanChange, Channel = StreamChannel });
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(spaceVertival);


                // Change modulation. Often applied a vibrato, this effect is defined in the SoundFont 
                GUILayout.BeginHorizontal(GUILayout.Width(350));
                int modChange = (int)Slider("Modulation", ModChange, 0, 127);
                if (modChange != ModChange)
                {
                    ModChange = modChange;
                    midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.ControlChange, Controller = MPTKController.Modulation, Value = ModChange, Channel = StreamChannel });
                }

                // Change modulation. Often applied volume, this effect is defined in the SoundFont 
                int expChange = (int)Slider("Expression", ExpChange, 0, 127, true);
                if (expChange != ExpChange)
                {
                    ExpChange = expChange;
                    midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.ControlChange, Controller = MPTKController.Expression, Value = ExpChange, Channel = StreamChannel });
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical(); // end ControlChange modification

                GUILayout.Space(spaceVertival);

                //
                // Change value from Generator Synth
                // ---------------------------------
                GUILayout.BeginVertical(myStyle.BacgDemos1);
                GUILayout.Label("Real Time Voice Parameters Change [Availablle with MPTK Pro]. Experimental feature.", myStyle.TitleLabel3);
                float gene;
                for (int i = 0; i < nbrGenerator; i += 2) // 2 generators per line
                {
                    GUILayout.BeginHorizontal(GUILayout.Width(650));

                    for (int j = 0; j < 2; j++) // 2 generators per line
                    {
                        int numGenerator = i + j;
                        // Open the popup to select an instrument
                        if (GUILayout.Button(indexGenerator[numGenerator] + " - " + labelGenerator[numGenerator], GUILayout.Width(buttonWidth)))
                            PopGenerator[numGenerator].Show = !PopGenerator[numGenerator].Show;
                        // Get real time value
                        gene = Slider("Value", valueGenerator[numGenerator], 0f, 100f, true, 50f, 80f);
                        if (indexGenerator[numGenerator] >= 0)
                        {
#if MPTK_PRO
                            // If value is different then applied to the current note playing
                            if (valueGenerator[numGenerator] != gene && NotePlaying != null)
                                NotePlaying.MTPK_ModifySynthParameter((fluid_gen_type)indexGenerator[numGenerator], valueGenerator[numGenerator] / 100f, MPTKModeGeneratorChange.Override);
#endif
                            valueGenerator[numGenerator] = gene;
                        }
                        GUILayout.Label(" ", myStyle.TitleLabel3, GUILayout.Width(60));
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical(); // End change value from Generator Synth

                GUILayout.Space(spaceVertival);

                GUILayout.BeginVertical(myStyle.BacgDemos);
                GUILayout.Label("Go to your Hierarchy, select GameObject MidiStreamPlayer: inspector contains a lot of parameters to control the sound.", myStyle.TitleLabel2);
                GUILayout.EndVertical();

                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Space(spaceVertival);
                GUILayout.Label("MidiStreamPlayer not defined, check hierarchy.", myStyle.TitleLabel3);
            }
        }

        private void BuildRangeFromLib()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("From Lib", myStyle.TitleLabel3, GUILayout.Width(widthFirstCol));
            DelayPlayScale = (int)Slider("Delay (ms)", DelayPlayScale, 100, 1000, false, 70);
            GUILayout.EndHorizontal();
            GUIForScale();
        }

        private void BuildChordFromLibrary()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("From Lib", myStyle.TitleLabel3, GUILayout.Width(widthFirstCol));
            GUILayout.Label("Chord", myStyle.TitleLabel3, GUILayout.MaxWidth(70));

            if (GUILayout.Button("-", GUILayout.Width(30)))
            {
#if MPTK_PRO
                CurrentChord--;
                if (CurrentChord < 0) CurrentChord = MPTKChordLib.Chords.Count - 1;
                Play(true);
#endif
            }

            string strChord = GUILayout.TextField(CurrentChord.ToString(), 2, GUILayout.Width(50));
            int chord = 0;
            try
            {
                chord = Convert.ToInt32(strChord);
            }
            catch (Exception) { }
            if (chord != CurrentChord)
            {
#if MPTK_PRO
                CurrentChord = Mathf.Clamp(chord, 0, MPTKChordLib.Chords.Count - 1);
                Play(true);
#endif
            }

            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
#if MPTK_PRO
                CurrentChord++;
                if (CurrentChord >= MPTKChordLib.Chords.Count) CurrentChord = 0;
                Play(true);
#endif
            }

            if (GUILayout.Button("Play", GUILayout.Width(50)))
            {
                Play(true);
            }
#if MPTK_PRO
            GUILayout.Label($"{MPTKChordLib.Chords[CurrentChord].Name}", myStyle.TitleLabel3, GUILayout.MaxWidth(100));
            GUILayout.Label("See file ChordLib.csv in folder Resources/GeneratorTemplate", myStyle.TitleLabel3, GUILayout.Width(500));
#endif
            GUILayout.EndHorizontal();
        }

        private void BuildChordFromdegree()
        {
            GUILayout.BeginHorizontal(GUILayout.Width(600));
            GUILayout.Label("From Degree", myStyle.TitleLabel3, GUILayout.Width(widthFirstCol));

            int countNote = (int)Slider("Count", CountNoteChord, 2, 17, false, 70);
            if (countNote != CountNoteChord)
            {
                CountNoteChord = countNote;
                Play(true);
            }

            int degreeChord = (int)Slider("Degree", DegreeChord, 1, 7, false, 70);
            if (degreeChord != DegreeChord)
            {
                DegreeChord = degreeChord;
                Play(true);
            }

            ArpeggioPlayChord = (int)Slider("Arpeggio (ms)", ArpeggioPlayChord, 0, 500, false, 70);

            GUILayout.EndHorizontal();
            GUIForScale();
        }

        //#if MPTK_PRO

        /// <summary>
        /// Common UI for building and playing a chord or a scale from the library of scale
        /// See in GUI "Play Chord From Degree" and "Play Range From Lib"
        /// </summary>
        private void GUIForScale()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(" ", myStyle.TitleLabel3, GUILayout.Width(widthFirstCol));
            GUILayout.Label("Scale", myStyle.TitleLabel3, GUILayout.MaxWidth(70));

            // Display and play with the previous scale 
            if (GUILayout.Button("-", GUILayout.Width(30)))
            {
#if MPTK_PRO
                CurrentScale--;
                if (CurrentScale < 0) CurrentScale = MPTKRangeLib.RangeCount - 1;
                // Select the range from a list of range. See file Resources/GeneratorTemplate/GammeDefinition.csv 
                midiStreamPlayer.MPTK_RangeSelected = CurrentScale;
                Play(true);
#endif
            }

            // Seizes the index of the scale
            string strScale = GUILayout.TextField(CurrentScale.ToString(), 2, GUILayout.Width(50));
            int scale = 0;
            try
            {
                scale = Convert.ToInt32(strScale);
            }
            catch (Exception) { }

            if (scale != CurrentScale)
            {
#if MPTK_PRO
                CurrentScale = Mathf.Clamp(scale, 0, MPTKRangeLib.RangeCount - 1);
                // Select the range from a list of range. See file Resources/GeneratorTemplate/GammeDefinition.csv 
                midiStreamPlayer.MPTK_RangeSelected = CurrentScale;
                Play(true);
#endif
            }

            // Display and play with the next scale/range 
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
#if MPTK_PRO
                CurrentScale++;
                if (CurrentScale >= MPTKRangeLib.RangeCount) CurrentScale = 0;
                // Select the range from a list of range. See file Resources/GeneratorTemplate/GammeDefinition.csv 
                midiStreamPlayer.MPTK_RangeSelected = CurrentScale;
                Play(true);
#endif
            }

            // Button play yo play the current range/scale
            if (GUILayout.Button("Play", GUILayout.Width(50)))
            {
                Play(true);
            }
#if MPTK_PRO
            GUILayout.Label($"{midiStreamPlayer.MPTK_RangeName}", myStyle.TitleLabel3, GUILayout.MaxWidth(100));
            GUILayout.Label("See GammeDefinition.csv in folder Resources/GeneratorTemplate", myStyle.TitleLabel3, GUILayout.Width(500));
#endif
            GUILayout.EndHorizontal();
        }
        //#endif

        private float Slider(string title, float val, float min, float max, bool alignright = false, float wiLab = 70, float wiSlider = 100)
        {
            float ret;
            GUILayout.BeginHorizontal();
            GUILayout.Label(title, alignright ? myStyle.LabelRight : myStyle.LabelLeft, GUILayout.Width(wiLab), GUILayout.Height(25));
            GUILayout.Label(Math.Round(val, 2).ToString(), myStyle.LabelRight, GUILayout.Width(30), GUILayout.Height(25));
            ret = GUILayout.HorizontalSlider(val, min, max, myStyle.SliderBar, myStyle.SliderThumb, GUILayout.Width(wiSlider));
            GUILayout.EndHorizontal();
            return ret;
        }

        // Update is called once per frame
        void Update()
        {
            // Checj that SoundFont is loaded and add a little wait (0.5 s by default) because Unity AudioSource need some time to be started
            if (!MidiPlayerGlobal.MPTK_IsReady())
                return;

            if (PitchChange != DEFAULT_PITCH)
            {
                // If user change the pitch, wait 1/2 second before return to median value
                if (Time.realtimeSinceStartup - LastTimePitchChange > 0.5f)
                {
                    PitchChange = Mathf.SmoothDamp(PitchChange, DEFAULT_PITCH, ref currentVelocityPitch, 0.5f, 100, Time.unscaledDeltaTime);
                    if (Mathf.Abs(PitchChange - DEFAULT_PITCH) < 0.1f)
                        PitchChange = DEFAULT_PITCH;
                    //PitchChange = Mathf.Lerp(PitchChange, DEFAULT_PITCH, Time.deltaTime*10f);
                    //Debug.Log("DEFAULT_PITCH " + DEFAULT_PITCH + " " + PitchChange + " " + currentVelocityPitch);
                    midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.PitchWheelChange, Value = (int)PitchChange << 7, Channel = StreamChannel });
                }
            }

            if (midiStreamPlayer != null && (IsplayingLoopPresets || IsplayingLoopNotes))
            {
                float time = Time.realtimeSinceStartup - LastTimeChange;
                if (time > Frequency)
                {
                    // It's time to generate some notes ;-)
                    LastTimeChange = Time.realtimeSinceStartup;


                    for (int indexNote = 0; indexNote < CountNoteToPlay; indexNote++)
                    {
                        if (IsplayingLoopPresets)
                        {
                            if (++CurrentPreset > EndPreset) CurrentPreset = StartPreset;
                            if (CurrentPreset < StartPreset) CurrentPreset = StartPreset;

                            midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent()
                            {
                                Command = MPTKCommand.PatchChange,
                                Value = CurrentPreset,
                                Channel = StreamChannel,
                            });
                        }

                        if (IsplayingLoopNotes)
                        {
                            if (++CurrentNote > EndNote) CurrentNote = StartNote;
                            if (CurrentNote < StartNote) CurrentNote = StartNote;
                        }

                        // Play note or chrod or scale without stopping the current (useful for performance test)
                        Play(false);

                    }
                }
            }
        }

        /// <summary>
        /// Play music depending the parameters set
        /// </summary>
        /// <param name="stopCurrent">stop current note playing</param>
        void Play(bool stopCurrent)
        {
            if (RandomPlay)
            {
                if (StartNote >= EndNote)
                    CurrentNote = StartNote;
                else
                    CurrentNote = UnityEngine.Random.Range(StartNote, EndNote);
            }

#if MPTK_PRO
            if (ChordPlay || ChordLibPlay || RangeLibPlay)
            {
                if (RandomPlay)
                {
                    CountNoteChord = UnityEngine.Random.Range(3, 5);
                    DegreeChord = UnityEngine.Random.Range(1, 8);
                    CurrentChord = UnityEngine.Random.Range(StartNote, EndNote);
                }

                if (stopCurrent)
                    StopChord();

                if (ChordPlay)
                    PlayOneChord();

                if (ChordLibPlay)
                    PlayOneChordFromLib();

                if (RangeLibPlay)
                    PlayScale();
            }
            else
#endif
            {
                if (stopCurrent)
                    StopOneNote();
                PlayOneNote();
            }
        }

#if MPTK_PRO
        MPTKChordBuilder ChordPlaying;
        MPTKChordBuilder ChordLibPlaying;

        /// <summary>
        /// Play note from a scale
        /// </summary>
        private void PlayScale()
        {
            // get the current scale selected
            MPTKRangeLib range = MPTKRangeLib.Range(CurrentScale, true);
            for (int ecart = 0; ecart < range.Count; ecart++)
            {
                NotePlaying = new MPTKEvent()
                {
                    Command = MPTKCommand.NoteOn, // midi command
                    Value = CurrentNote + range[ecart], // from 0 to 127, 48 for C4, 60 for C5, ...
                    Channel = StreamChannel, // from 0 to 15, 9 reserved for drum
                    Duration = DelayPlayScale, // note duration in millisecond, -1 to play undefinitely, MPTK_StopEvent to stop
                    Velocity = Velocity, // from 0 to 127, sound can vary depending on the velocity
                    Delay = ecart * DelayPlayScale, // delau in millisecond before playing the note
                };
                midiStreamPlayer.MPTK_PlayEvent(NotePlaying);
            }
        }

        private void PlayOneChord()
        {
            // Start playing a new chord and save in ChordPlaying to stop it later
            ChordPlaying = new MPTKChordBuilder(true)
            {
                // Parameters to build the chord
                Tonic = CurrentNote,
                Count = CountNoteChord,
                Degree = DegreeChord,

                // Midi Parameters how to play the chord
                Channel = StreamChannel,
                Arpeggio = ArpeggioPlayChord, // delay in milliseconds between each notes of the chord
                Duration = Convert.ToInt64(NoteDuration * 1000f), // millisecond, -1 to play undefinitely
                Velocity = Velocity, // Sound can vary depending on the velocity
                Delay = Convert.ToInt64(NoteDelay * 1000f),
            };
            //Debug.Log(DegreeChord);
            midiStreamPlayer.MPTK_PlayChordFromRange(ChordPlaying);
        }
        private void PlayOneChordFromLib()
        {
            // Start playing a new chord and save in ChordLibPlaying to stop it later
            ChordLibPlaying = new MPTKChordBuilder(true)
            {
                // Parameters to build the chord
                Tonic = CurrentNote,
                FromLib = CurrentChord,

                // Midi Parameters how to play the chord
                Channel = StreamChannel,
                Arpeggio = ArpeggioPlayChord, // delay in milliseconds between each notes of the chord
                Duration = Convert.ToInt64(NoteDuration * 1000f), // millisecond, -1 to play undefinitely
                Velocity = Velocity, // Sound can vary depending on the velocity
                Delay = Convert.ToInt64(NoteDelay * 1000f),
            };
            //Debug.Log(DegreeChord);
            midiStreamPlayer.MPTK_PlayChordFromLib(ChordLibPlaying);
        }

        private void StopChord()
        {
            if (ChordPlaying != null)
                midiStreamPlayer.MPTK_StopChord(ChordPlaying);

            if (ChordLibPlaying != null)
                midiStreamPlayer.MPTK_StopChord(ChordLibPlaying);

        }
#else
        private void PlayScale(){}
        private void PlayOneChord(){}
        private void PlayOneChordFromLib(){}
        private void StopChord(){}
#endif
        //! [Example MPTK_PlayEvent]
        /// <summary>
        /// Send the note to the player. Notes are plays in a thread, so call returns immediately.
        /// The note is stopped automatically after the Duration defined.
        /// </summary>
        /// @snippet TestMidiStream.cs Example MPTK_PlayEvent
        private void PlayOneNote()
        {
            //Debug.Log($"{StreamChannel} {midiStreamPlayer.MPTK_ChannelPresetGetName(StreamChannel)}");
            // Start playing a new note
            NotePlaying = new MPTKEvent()
            {
                Command = MPTKCommand.NoteOn,
                Value = CurrentNote, // note to played, ex 60=C5. Use the method from class HelperNoteLabel to convert to string
                Channel = StreamChannel,
                Duration = Convert.ToInt64(NoteDuration * 1000f), // millisecond, -1 to play undefinitely
                Velocity = Velocity, // Sound can vary depending on the velocity
                Delay = Convert.ToInt64(NoteDelay * 1000f),
            };

#if MPTK_PRO
            // Applied to the current note playing all the real time generators defined
            for (int i = 0; i < nbrGenerator; i++)
                if (indexGenerator[i] >= 0)
                    NotePlaying.MTPK_ModifySynthParameter((fluid_gen_type)indexGenerator[i], valueGenerator[i] / 100f, MPTKModeGeneratorChange.Override);
#endif
            midiStreamPlayer.MPTK_PlayEvent(NotePlaying);
        }
        //! [Example MPTK_PlayEvent]

        private void StopOneNote()
        {
            if (NotePlaying != null)
            {
                //Debug.Log("Stop note");
                // Stop the note (method to simulate a real human on a keyboard : 
                // duration is not known when note is triggered)
                midiStreamPlayer.MPTK_StopEvent(NotePlaying);
                NotePlaying = null;
            }
        }
    }
}