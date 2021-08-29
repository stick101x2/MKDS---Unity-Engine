#define SHOWDEFAULT
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
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MidiFileLoader))]
    public class MidiFileLoaderEditor : Editor
    {
        private static MidiFileLoader instance;
        private SelectMidiWindow winSelectMidi;

#if SHOWDEFAULT
        private static bool showDefault;
#endif

        void OnEnable()
        {
            try
            {
                instance = (MidiFileLoader)target;

                if (!Application.isPlaying)
                {
                    // Load description of available soundfont
                    if (MidiPlayerGlobal.CurrentMidiSet == null || MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo == null)
                    {
                        MidiPlayerGlobal.InitPath();
                        ToolsEditor.LoadMidiSet();
                        ToolsEditor.CheckMidiSet();
                    }
                }

                if (winSelectMidi != null)
                {
                    //Debug.Log("OnEnable winSelectMidi " + winSelectMidi.Title);
                    winSelectMidi.SelectedItem = instance.MPTK_MidiIndex;
                    winSelectMidi.Repaint();
                    winSelectMidi.Focus();
                }

                //EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            //Debug.Log("EditorApplication_playModeStateChanged " + obj.ToString());
        }

        private void OnDisable()
        {
            try
            {
                if (winSelectMidi != null)
                {
                    //Debug.Log("OnDisable winSelectMidi " + winSelectMidi.Title);
                    winSelectMidi.Close();
                }
            }
            catch (Exception)
            {
            }
        }

        public void InitWinSelectMidi(int selected, Action<object, int> select)
        {
            // Get existing open window or if none, make a new one:
            try
            {
                winSelectMidi = EditorWindow.GetWindow<SelectMidiWindow>(true, "Select a Midi File");
                winSelectMidi.SelectWindow = winSelectMidi;
                //SelectMidiWindow.SelectWindow = winSelectMidi;
                winSelectMidi.OnSelect = select;
                winSelectMidi.SelectedItem = selected;
                winSelectMidi.BackgroundColor = new Color(.8f, .8f, .8f, 1f);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private void MidiChanged(object tag, int midiindex)
        {
            //Debug.Log("MidiChanged " + midiindex + " for " + tag);
            instance.MPTK_MidiIndex = midiindex;
            MidiCommonEditor.SetSceneChangedIfNeed(instance, true);
        }

        public override void OnInspectorGUI()
        {
            try
            {
                GUI.changed = false;
                GUI.color = Color.white;

                if (MidiPlayerGlobal.CurrentMidiSet != null || MidiPlayerGlobal.CurrentMidiSet.MidiFiles == null || MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Select Midi ", "Select Midi File to play"), GUILayout.Width(150));

                    if (GUILayout.Button(new GUIContent(instance.MPTK_MidiIndex + " - " + instance.MPTK_MidiName, "Selected Midi File to load"), GUILayout.Height(30)))
                        InitWinSelectMidi(instance.MPTK_MidiIndex, MidiChanged);
                    EditorGUILayout.EndHorizontal();

                    instance.MPTK_KeepNoteOff = EditorGUILayout.Toggle(new GUIContent("Keep Midi NoteOff", "Keep Midi NoteOff and NoteOn with Velocity=0 (need to restart the playing Midi)"), instance.MPTK_KeepNoteOff);
                    instance.MPTK_LogEvents = EditorGUILayout.Toggle(new GUIContent("Log Midi Events", "Log information about each midi events read."), instance.MPTK_LogEvents);

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("Load", "")))
                        instance.MPTK_Load();
                    if (GUILayout.Button(new GUIContent("Previous", "")))
                    {
                        instance.MPTK_Previous();
                        instance.MPTK_Load();
                    }
                    if (GUILayout.Button(new GUIContent("Next", "")))
                    {
                        instance.MPTK_Next(); EditorGUILayout.EndHorizontal();
                        instance.MPTK_Load();
                    }

                    EditorGUILayout.EndHorizontal();

#if SHOWDEFAULT
                    showDefault = EditorGUILayout.Foldout(showDefault, "Show default editor");
                    if (showDefault)
                    {
                        EditorGUI.indentLevel++;
                        DrawDefaultInspector();
                        EditorGUI.indentLevel--;
                    }
#endif

                }
                else
                {
                    MidiCommonEditor.ErrorNoMidiFile();
                }

                MidiCommonEditor.SetSceneChangedIfNeed(instance, GUI.changed);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }


    }
}
