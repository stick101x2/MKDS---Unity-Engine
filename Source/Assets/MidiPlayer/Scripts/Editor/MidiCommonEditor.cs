//#define MPTK_PRO
using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MidiPlayerTK
{
    /// <summary>
    /// Inspector for the midi global player component
    /// </summary>
    public class MidiCommonEditor : ScriptableObject
    {
        private TextArea taSequence;
        private TextArea taProgram;
        private TextArea taInstrument;
        private TextArea taText;
        private TextArea taCopyright;
        private SerializedProperty CustomEventSynthAwake;
        private SerializedProperty CustomEventSynthStarted;

        //                                         Level=0            1           2           4             8             
        static private string[] popupQuantization = { "None", "Quarter Note", "Eighth Note", "16th Note", "32th Note", "64th Note", "128th Note" };
        string[] synthRateLabel = new string[] { "Default", "24000 Hz", "36000 Hz", "48000 Hz", "60000 Hz", "72000 Hz", "84000 Hz", "96000 Hz" };
        int[] synthRateIndex = { -1, 0, 1, 2, 3, 4, 5, 6 };

        string[] synthBufferSizeLabel = new string[] { "Default", "64", "128", "256", "512", "1024", "2048" };
        int[] synthBufferSizeIndex = { -1, 0, 1, 2, 3, 4, 5 };

        string[] synthInterpolationLabel = new string[] { "None - efficient but low quality", "Linear - good quality", "Cubic", "7th Order" };
        int[] synthInterpolationIndex = { 0, 1, 2, 3 };

        static public CustomStyle myStyle;

        public void DrawAlertOnDefault()
        {
            if (myStyle == null) myStyle = new CustomStyle();
            EditorGUILayout.LabelField(
                "Changing properties here are without any guarantee!" +
                " To activate full stat, define these scripting symbols: " +
                "DEBUG_PERF_AUDIO, DEBUG_PERF_MIDI, DEBUG_STATUS_STAT"
                , myStyle.LabelAlert);
        }

        static private Texture buttonIconHelp;

        static public bool DrawFoldoutAndHelp(bool state, string title, string urlHelp)
        {
            EditorGUILayout.BeginHorizontal();
            state = EditorGUILayout.Foldout(state, title);
            MidiCommonEditor.DrawHelp(urlHelp);
            EditorGUILayout.EndHorizontal();
            return state;
        }

        static public void DrawHelp(string urlHelp)
        {
            if (buttonIconHelp == null) buttonIconHelp = Resources.Load<Texture2D>("Textures/question-mark");
            if (GUILayout.Button(
                new GUIContent(buttonIconHelp, "Get some help on MPTK web site"),
                EditorStyles.helpBox,
                GUILayout.Width(16f), GUILayout.Height(16f)))
                Application.OpenURL(urlHelp);
        }

        static public void DrawHelpAPI(string urlAPI)
        {
            if (GUILayout.Button(new GUIContent("API", "Get some help on MPTK web site"),
                EditorStyles.helpBox,
                GUILayout.Width(32f), GUILayout.Height(16f)))
                Application.OpenURL("http://autogam.free.fr/MPTK/html/" + urlAPI);
        }

        public void DrawCaption(string title, string urlHelp, string urlAPI)
        {
            if (myStyle == null) myStyle = new CustomStyle();
            EditorGUILayout.BeginVertical(myStyle.LabelTitle);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(title);

            DrawHelp(urlHelp);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (MidiPlayerGlobal.ImSFCurrent != null)
            {
                EditorGUILayout.LabelField(new GUIContent("SoundFont: " + MidiPlayerGlobal.ImSFCurrent.SoundFontName + " loaded", MidiPlayerGlobal.HelpDefSoundFont));
            }
            else if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo != null)
            {
                EditorGUILayout.LabelField(new GUIContent("SoundFont: " + MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name, MidiPlayerGlobal.HelpDefSoundFont));
            }
            else
            {
                ErrorNoSoundFont();
            }
            DrawHelpAPI(urlAPI);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            //EditorGUILayout.Separator();
        }

        public void AllPrefab(MidiSynth instance)
        {
            float volume = EditorGUILayout.Slider(new GUIContent("Volume", "Set global volume for this midi playing"), instance.MPTK_Volume, 0f, 1f);
            if (instance.MPTK_Volume != volume)
                instance.MPTK_Volume = volume;



            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Transpose");
            instance.MPTK_Transpose = EditorGUILayout.IntSlider(instance.MPTK_Transpose, -24, 24);
            EditorGUILayout.EndHorizontal();
            //Debug.Log(instance.GetType());
            string foldoutTitle = $"Show Spatialization Parameters - {instance.MPTK_Spatialize} {Math.Round(instance.distanceToListener, 2)}/{instance.MPTK_MaxDistance}";
            instance.showSpatialization = DrawFoldoutAndHelp(instance.showSpatialization, foldoutTitle, "https://paxstellar.fr/midi-file-player-detailed-view-2/#Foldout-Spatialization-Parameters");
            if (instance.showSpatialization)
            {
                EditorGUI.indentLevel++;
                GUIContent labelSpatialization = new GUIContent("Spatialization", "Enable spatialization effect");
#if MPTK_PRO
                if (!(instance is MidiSpatializer))
                {
#endif
                    bool spatialize = EditorGUILayout.Toggle(labelSpatialization, instance.MPTK_Spatialize);
                    if (instance.MPTK_Spatialize != spatialize)
                        instance.MPTK_Spatialize = spatialize;
#if MPTK_PRO
                }
                else
                {
                    // Need to be forced to true, here to check.
                    EditorGUILayout.LabelField(labelSpatialization, new GUIContent(instance.MPTK_Spatialize ? "True" : "False"));
                }
#endif

                //EditorGUILayout.BeginHorizontal();
                string tooltipDistance = "Playing is paused if distance between AudioListener and this component is greater than MaxDistance";
                float distance = EditorGUILayout.IntField(new GUIContent("Max Distance", tooltipDistance), (int)instance.MPTK_MaxDistance);
                if (instance.MPTK_MaxDistance != distance)
                    instance.MPTK_MaxDistance = distance;

                //Debug.Log("Camera: " + instance.distanceEditorModeOnly);
                EditorGUILayout.LabelField(new GUIContent($"Current distance to Camera: {Math.Round(instance.distanceToListener, 2)}", tooltipDistance));
                //EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
        }

        public void MidiFileParameters(MidiFilePlayer instance)
        {
            instance.MPTK_PlayOnStart = EditorGUILayout.Toggle(new GUIContent("Play At Startup", "Start playing Midi when the application starts"), instance.MPTK_PlayOnStart);
            instance.MPTK_StartPlayAtFirstNote = EditorGUILayout.Toggle(new GUIContent("Start Play From First Note", "Start playing Midi from the first note found in the midi"), instance.MPTK_StartPlayAtFirstNote);
            instance.MPTK_PauseOnFocusLoss = EditorGUILayout.Toggle(new GUIContent("Pause When Focus Loss", "Pause when application loss the focus"), instance.MPTK_PauseOnFocusLoss);

            instance.MPTK_DirectSendToPlayer = EditorGUILayout.Toggle(new GUIContent("Send To Synth", "Midi events are send to the midi player directly"), instance.MPTK_DirectSendToPlayer);

            instance.MPTK_Loop = EditorGUILayout.Toggle(new GUIContent("Loop On Midi", "Enable loop on midi play"), instance.MPTK_Loop);

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.Separator();
                string infotime = "Real time from start and total duration regarding the current tempo";
                EditorGUILayout.LabelField(new GUIContent("Time", infotime), new GUIContent(instance.playTimeEditorModeOnly + " / " + instance.durationEditorModeOnly, infotime));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Position", "Set real time position since the startup regarding the current tempo"));
                float currentPosition = (float)Math.Round(instance.MPTK_Position);
                float newPosition = (float)Math.Round(EditorGUILayout.Slider(currentPosition, 0f, instance.MPTK_DurationMS));
                if (currentPosition != newPosition)
                {
                    // Avoid event as layout triggered when duration is changed
                    if (Event.current.type == EventType.Used)
                    {
                        //Debug.Log("New position " + currentPosition + " --> " + newPosition + " " + Event.current.type);
                        instance.MPTK_Position = newPosition;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Separator();
                string infotick = "Tick count for start and total duration regardless the current tempo";
                EditorGUILayout.LabelField(new GUIContent("Ticks", infotick), new GUIContent(instance.MPTK_TickCurrent + " / " + instance.MPTK_TickLast, infotime));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Position", "Set tick position since the startup regardless the current tempo"));
                long currenttick = instance.MPTK_TickCurrent;
                long ticks = Convert.ToInt64(EditorGUILayout.Slider(currenttick, 0f, (float)instance.MPTK_TickLast));
                if (currenttick != ticks)
                {
                    // Avoid event as layout triggered when duration is changed
                    if (Event.current.type == EventType.Used)
                    {
                        //Debug.Log("New tick " + currenttick + " --> " + ticks + " " + Event.current.type);
                        instance.MPTK_TickCurrent = ticks;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();

                EditorGUILayout.BeginHorizontal();
                if (instance.MPTK_IsPlaying && !instance.MPTK_IsPaused)
                    GUI.color = ToolsEditor.ButtonColor;
                if (GUILayout.Button(new GUIContent("Play", "")))
                    instance.MPTK_Play();
                GUI.color = Color.white;

                if (instance.MPTK_IsPaused)
                    GUI.color = ToolsEditor.ButtonColor;
                if (GUILayout.Button(new GUIContent("Pause", "")))
                    if (instance.MPTK_IsPaused)
                        instance.MPTK_UnPause();
                    else
                        instance.MPTK_Pause();
                GUI.color = Color.white;

                if (GUILayout.Button(new GUIContent("Stop", "")))
                    instance.MPTK_Stop();

                if (GUILayout.Button(new GUIContent("Restart", "")))
                    instance.MPTK_RePlay();
                EditorGUILayout.EndHorizontal();
#if MPTK_PRO
                if (!(instance is MidiExternalPlayer))
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("Previous", "")))
                        instance.MPTK_Previous();
                    if (GUILayout.Button(new GUIContent("Next", "")))
                        instance.MPTK_Next();
                    EditorGUILayout.EndHorizontal();
                }
#endif
            }

            instance.showMidiParameter = DrawFoldoutAndHelp(instance.showMidiParameter, "Show Midi Parameters", "https://paxstellar.fr/midi-file-player-detailed-view-2/#Foldout-Midi-Parameters");
            if (instance.showMidiParameter)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Quantization", ""), GUILayout.Width(150));
                int newLevel = EditorGUILayout.Popup(instance.MPTK_Quantization, popupQuantization);
                if (newLevel != instance.MPTK_Quantization && newLevel >= 0 && newLevel < popupQuantization.Length)
                    instance.MPTK_Quantization = newLevel;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Speed");
                float speed = EditorGUILayout.Slider(instance.MPTK_Speed, 0.1f, 10f);
                //          Debug.Log("New speed " + instance.MPTK_Speed + " --> " + speed + " " + Event.current.type);
                if (speed != instance.MPTK_Speed)
                {
                    //Debug.Log("New speed " + instance.MPTK_Speed + " --> " + speed + " " + Event.current.type);
                    instance.MPTK_Speed = speed;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                instance.MPTK_EnableChangeTempo = EditorGUILayout.Toggle(new GUIContent("Tempo Change", "Enable midi event tempo change when playing."), instance.MPTK_EnableChangeTempo);
                EditorGUILayout.LabelField(new GUIContent("Current:" + Math.Round(instance.MPTK_Tempo, 0), "Current tempo defined in Midi"));
                EditorGUILayout.EndHorizontal();

                instance.MPTK_EnablePresetDrum = EditorGUILayout.Toggle(new GUIContent("Drum Preset Change", "Enable Preset change on the canal 10 for drum. By default disabled, could sometimes create bad sound with midi files not really compliant with the Midi norm."), instance.MPTK_EnablePresetDrum);
                // Moved to synth parameters (V2.85)
                //instance.MPTK_ReleaseSameNote = EditorGUILayout.Toggle(new GUIContent("Release Same Note", "Enable release note if the same note is hit twice on the same channel."), instance.MPTK_ReleaseSameNote);
                //instance.MPTK_KillByExclusiveClass = EditorGUILayout.Toggle(new GUIContent("Kill By Exclusive Class", "Find the exclusive class of the voice. If set, kill all voices that match the exclusive class and are younger than the first voice process created by this noteon event."), instance.MPTK_KillByExclusiveClass);
                instance.MPTK_KeepNoteOff = EditorGUILayout.Toggle(new GUIContent("Keep Midi NoteOff", "Keep Midi NoteOff and NoteOn with Velocity=0 (need to restart the playing Midi)"), instance.MPTK_KeepNoteOff);
                instance.MPTK_LogEvents = EditorGUILayout.Toggle(new GUIContent("Log Midi Events", "Log information about each midi events read."), instance.MPTK_LogEvents);


                EditorGUI.indentLevel--;
            }
        }

        public void MidiFileInfo(MidiFilePlayer instance)
        {
            instance.showMidiInfo = DrawFoldoutAndHelp(instance.showMidiInfo, "Show Midi Info", "https://paxstellar.fr/midi-file-player-detailed-view-2/#Foldout-Midi-Info");
            if (instance.showMidiInfo)
            {
                EditorGUI.indentLevel++;

                if (!string.IsNullOrEmpty(instance.MPTK_SequenceTrackName))
                {
                    if (taSequence == null) taSequence = new TextArea("Sequence");
                    taSequence.Display(instance.MPTK_SequenceTrackName);
                }

                if (!string.IsNullOrEmpty(instance.MPTK_ProgramName))
                {
                    if (taProgram == null) taProgram = new TextArea("Program");
                    taProgram.Display(instance.MPTK_ProgramName);
                }

                if (!string.IsNullOrEmpty(instance.MPTK_TrackInstrumentName))
                {
                    if (taInstrument == null) taInstrument = new TextArea("Instrument");
                    taInstrument.Display(instance.MPTK_TrackInstrumentName);
                }

                if (!string.IsNullOrEmpty(instance.MPTK_TextEvent))
                {
                    if (taText == null) taText = new TextArea("TextEvent");
                    taText.Display(instance.MPTK_TextEvent);
                }

                if (!string.IsNullOrEmpty(instance.MPTK_Copyright))
                {
                    if (taCopyright == null) taCopyright = new TextArea("Copyright");
                    taCopyright.Display(instance.MPTK_Copyright);
                }
                EditorGUI.indentLevel--;
            }
        }

        public void SynthParameters(MidiSynth instance, SerializedObject sobject)
        {
            instance.showSynthParameter = DrawFoldoutAndHelp(instance.showSynthParameter, "Show Synth Parameters", "https://paxstellar.fr/midi-file-player-detailed-view-2/#Foldout-Synth-Parameters");
            if (instance.showSynthParameter)
            {
                EditorGUI.indentLevel++;

                GUIContent labelCore = new GUIContent("Core Player", "Play music with a non Unity thread. Change this properties only when not running");
                string labelRate = "Rate Synth Output";
                string labelBuffer = "Buffer Synth Size";
                string titleCore = instance.MPTK_CorePlayer ? "'Core'" : "'Non Core'";
                string foldoutTitle = $"Show Unity Audio Parameters - {titleCore}";
                instance.showUnitySynthParameter = DrawFoldoutAndHelp(instance.showUnitySynthParameter, foldoutTitle, "https://paxstellar.fr/midi-file-player-detailed-view-2/#Foldout-Audio-Parameters");
                if (instance.showUnitySynthParameter)
                {
                    EditorGUI.indentLevel++;
                    if (myStyle == null) myStyle = new CustomStyle();
                    EditorGUILayout.LabelField("With Core Player checked (fluidsynth mode), the synthesizer is working on a thread apart from the main Unity thread. Accuracy is much better. The legacy mode which is using many AudioSource will be removed with the next major version.", myStyle.LabelGreen);
                    if (!EditorApplication.isPlaying)
                        instance.MPTK_CorePlayer = EditorGUILayout.Toggle(labelCore, instance.MPTK_CorePlayer);
                    else
                        EditorGUILayout.LabelField(labelCore, new GUIContent(instance.MPTK_CorePlayer ? "True" : "False"));

                    if (NoErrorValidator.CantChangeAudioConfiguration)
                    {
                        EditorGUILayout.LabelField("Warning: Audio configuration change is disabled on this platform.", myStyle.LabelAlert);
                    }
                    else if (instance.MPTK_CorePlayer)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Changing synthesizer rate and buffer size can produce unexpected effect according to the hardware. Save your work before!", myStyle.LabelGreen);
                        EditorGUILayout.Space();
                        if (!EditorApplication.isPlaying)
                        {
                            EditorGUILayout.LabelField("Increase the rate to get a better sound but with a cost on performance.", myStyle.LabelGreen);
                            synthRateLabel[0] = "Default: " + AudioSettings.outputSampleRate + " Hz";
                            int indexrate = EditorGUILayout.IntPopup(labelRate, instance.MPTK_IndexSynthRate, synthRateLabel, synthRateIndex);
                            if (indexrate != instance.MPTK_IndexSynthRate)
                                instance.MPTK_IndexSynthRate = indexrate;
                            EditorGUILayout.Space();
                        }
                        else
                            EditorGUILayout.LabelField(labelRate, instance.OutputRate.ToString());

                        if (!EditorApplication.isPlaying)
                        {
                            EditorGUILayout.LabelField("Decrease the buffer size to get a more accurate playing.", myStyle.LabelGreen);
                            int bufferLenght;
                            int numBuffers;
                            AudioSettings.GetDSPBufferSize(out bufferLenght, out numBuffers);
                            synthBufferSizeLabel[0] = "Default: " + bufferLenght;
                            int indexBuffSize = EditorGUILayout.IntPopup(labelBuffer, instance.MPTK_IndexSynthBuffSize, synthBufferSizeLabel, synthBufferSizeIndex);
                            if (indexBuffSize != instance.MPTK_IndexSynthBuffSize)
                                instance.MPTK_IndexSynthBuffSize = indexBuffSize;
                            EditorGUILayout.Space();
                        }
                        else
                            EditorGUILayout.LabelField(labelBuffer, instance.DspBufferSize.ToString());

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Interpolation is the core of the synth process. Linear is a good balacing between quality and performance", myStyle.LabelGreen);
                        instance.InterpolationMethod = (fluid_interp)EditorGUILayout.IntPopup("Interpolation Method", (int)instance.InterpolationMethod, synthInterpolationLabel, synthInterpolationIndex);
                    }
                    else
                        EditorGUILayout.LabelField("Warning: with non core mode, all voices will be played in separate Audio Source. SoundFont synth is not fully implemented. This mode will be removed with a future version.", myStyle.LabelAlert);

                    EditorGUI.indentLevel--;
                }

                instance.MPTK_LogWave = EditorGUILayout.Toggle(new GUIContent("Log Samples", "Log information about sample played for a NoteOn event."), instance.MPTK_LogWave);

                //instance.MPTK_PlayOnlyFirstWave = EditorGUILayout.Toggle(new GUIContent("Play Only First Wave", "Some Instrument in Preset are using more of one wave at the same time. If checked, play only the first wave, usefull on weak device, but sound experience is less good."), instance.MPTK_PlayOnlyFirstWave);
                //instance.MPTK_WeakDevice = EditorGUILayout.Toggle(new GUIContent("Weak Device", "Playing Midi files with WeakDevice activated could cause some bad interpretation of Midi Event, consequently bad sound."), instance.MPTK_WeakDevice);
                instance.MPTK_EnablePanChange = EditorGUILayout.Toggle(new GUIContent("Pan Change", "Enable midi event pan change when playing. Uncheck if you want to manage Pan in your application."), instance.MPTK_EnablePanChange);

                instance.MPTK_ApplyRealTimeModulator = EditorGUILayout.Toggle(new GUIContent("Apply Modulator", "Real-Time change Modulator from Midi and ADSR enveloppe Modulator parameters from SoundFont could have an impact on CPU. Initial value of Modulator set at Note On are keep. Uncheck to gain some % CPU on weak device."), instance.MPTK_ApplyRealTimeModulator);
                instance.MPTK_ApplyModLfo = EditorGUILayout.Toggle(new GUIContent("Apply Mod LFO", "LFO modulation are defined in SoudFont. Uncheck to gain some % CPU on weak device."), instance.MPTK_ApplyModLfo);
                instance.MPTK_ApplyVibLfo = EditorGUILayout.Toggle(new GUIContent("Apply Vib LFO", "LFO vibrato are defined in SoudFont. Uncheck to gain some % CPU on weak device."), instance.MPTK_ApplyVibLfo);

                // Moved from Midi Parameters (V2.85)
                instance.MPTK_ReleaseSameNote = EditorGUILayout.Toggle(new GUIContent("Release Same Note", "Enable release note if the same note is hit twice on the same channel."), instance.MPTK_ReleaseSameNote);
                instance.MPTK_KillByExclusiveClass = EditorGUILayout.Toggle(new GUIContent("Kill By Exclusive Class", "Find the exclusive class of the voice. If set, kill all voices that match the exclusive class and are younger than the first voice process created by this noteon event."), instance.MPTK_KillByExclusiveClass);

                instance.showSoundFontEffect = MidiCommonEditor.DrawFoldoutAndHelp(instance.showSoundFontEffect, "Show SoundFont Effect Parameters", "https://paxstellar.fr/sound-effects/");
                if (instance.showSoundFontEffect)
#if MPTK_PRO
                    CommonProEditor.EffectSoundFontParameters(instance, myStyle);
#else
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("[Available with MPTK Pro] These effects will be applied independently on each voices. Effects values are defined in the SoundFont, so limited modification can't be applied.", myStyle.LabelGreen);
                    EditorGUI.indentLevel--;
                }
#endif

                instance.showUnitySynthEffect = MidiCommonEditor.DrawFoldoutAndHelp(instance.showUnitySynthEffect, "Show Unity Effect Parameters", "https://paxstellar.fr/sound-effects/");
                if (instance.showUnitySynthEffect)
#if MPTK_PRO
                    CommonProEditor.EffectUnityParameters(instance, myStyle);
#else
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("[Available with MPTK Pro] These effects will be applied to all voices processed by the current MPTK gameObject. You can add multiple MPTK gameObjects to apply for different effects.", myStyle.LabelGreen);
                    EditorGUI.indentLevel--;
                }

#endif
                instance.showUnityPerformanceParameter = DrawFoldoutAndHelp(instance.showUnityPerformanceParameter, "Show Performance Parameters", "https://paxstellar.fr/midi-file-player-detailed-view-2/#Foldout-Performance");
                if (instance.showUnityPerformanceParameter)
                {
                    EditorGUI.indentLevel++;
                    instance.waitThreadMidi = EditorGUILayout.IntSlider(new GUIContent("Thread Midi Delay", "Delay in milliseconds between call to the midi sequencer"), instance.waitThreadMidi, 1, 30);
                    instance.MaxDspLoad = EditorGUILayout.IntSlider(new GUIContent("Max Level DSP Load", "When DSP is over the 'Max Level DSP Load' (by default 50%), some actions are taken on current playing voices for better performance"), (int)instance.MaxDspLoad, 0, 100);
                    instance.DevicePerformance = EditorGUILayout.IntSlider(new GUIContent("Device Performance", "Define amount of cleaning of the voice. 1 for weak device and high cleaning. If <=25 some voice could be stopped."), instance.DevicePerformance, 1, 100);
                    instance.MPTK_CutOffVolume = EditorGUILayout.Slider(new GUIContent("Cut Off Volume", "Sample is stopped when amplitude is below this value. Increase for better performance but with degraded quality because sample are stopped earlier"), instance.MPTK_CutOffVolume, 0.01f, 0.5f);
                    EditorGUI.indentLevel--;
                }

                instance.showSynthEvents = MidiCommonEditor.DrawFoldoutAndHelp(instance.showSynthEvents, "Show Synth Unity Events", "https://paxstellar.fr/midi-file-player-detailed-view-2/#Foldout-Synth-Unity-Events");
                if (instance.showSynthEvents)
                {
                    EditorGUI.indentLevel++;
                    if (CustomEventSynthAwake == null)
                        CustomEventSynthAwake = sobject.FindProperty("OnEventSynthAwake");
                    EditorGUILayout.PropertyField(CustomEventSynthAwake);

                    if (CustomEventSynthStarted == null)
                        CustomEventSynthStarted = sobject.FindProperty("OnEventSynthStarted");
                    EditorGUILayout.PropertyField(CustomEventSynthStarted);

                    sobject.ApplyModifiedProperties();
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        public static void ErrorNoSoundFont()
        {
            GUIStyle labError = new GUIStyle("Label");
            //labError.normal.background = SetColor(new Texture2D(2, 2), new Color(0.9f, 0.9f, 0.9f));
            //labError.normal.textColor = new Color(0.8f, 0.1f, 0.1f);
            labError.alignment = TextAnchor.MiddleLeft;
            labError.wordWrap = true;
            labError.fontSize = 10;
            Texture buttonIconFolder = Resources.Load<Texture2D>("Textures/question-mark");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(MidiPlayerGlobal.ErrorNoSoundFont, labError, GUILayout.Height(30f));
            if (GUILayout.Button(new GUIContent(buttonIconFolder, "Help"), GUILayout.Width(20f), GUILayout.Height(20f)))
                Application.OpenURL("https://paxstellar.fr/setup-mptk-quick-start-v2/");
            EditorGUILayout.EndHorizontal();
            //MidiPlayerGlobal.InitPath();
            //ToolsEditor.LoadMidiSet();
            //ToolsEditor.CheckMidiSet();
            //Debug.Log(MidiPlayerGlobal.ErrorNoSoundFont);
        }

        public static void ErrorNoMidiFile()
        {
            GUIStyle labError = new GUIStyle("Label");
            labError.normal.background = SetColor(new Texture2D(2, 2), new Color(0.9f, 0.9f, 0.9f));
            labError.normal.textColor = new Color(0.8f, 0.1f, 0.1f);
            labError.alignment = TextAnchor.MiddleLeft;
            labError.wordWrap = true;
            labError.fontSize = 12;
            Texture buttonIconFolder = Resources.Load<Texture2D>("Textures/question-mark");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(MidiPlayerGlobal.ErrorNoMidiFile, labError, GUILayout.Height(40f));
            if (GUILayout.Button(new GUIContent(buttonIconFolder, "Help"), GUILayout.Width(40f), GUILayout.Height(40f)))
                Application.OpenURL("https://paxstellar.fr/setup-mptk-quick-start-v2/");
            EditorGUILayout.EndHorizontal();
            MidiPlayerGlobal.InitPath();
            ToolsEditor.LoadMidiSet();
            ToolsEditor.CheckMidiSet();
            Debug.Log(MidiPlayerGlobal.ErrorNoMidiFile);
        }

        public static Texture2D SetColor(Texture2D tex2, Color32 color)
        {
            var fillColorArray = tex2.GetPixels32();
            for (var i = 0; i < fillColorArray.Length; ++i)
                fillColorArray[i] = color;
            tex2.SetPixels32(fillColorArray);
            tex2.Apply();
            return tex2;
        }
        public static void SetSceneChangedIfNeed(UnityEngine.Object instance, bool changed)
        {
            if (changed)
            {
                EditorUtility.SetDirty(instance);
                if (!Application.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
        }
    }
}
