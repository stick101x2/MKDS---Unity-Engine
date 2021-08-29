//#define MPTK_PRO
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;

namespace MidiPlayerTK
{
    public class GUISelectSoundFont : MonoBehaviour
    {
        static public List<MPTKListItem> SoundFonts = null;
        static private PopupListItem PopSoundFont;
        static private int selectedSf;
        static private Texture buttonIconNote;

        static private void SoundFontChanged(object tag, int midiindex, int indexList)
        {
#if MPTK_PRO
            Debug.Log("SoundFontChanged " + midiindex);
            MidiPlayerGlobal.MPTK_SelectSoundFont(MidiPlayerGlobal.MPTK_ListSoundFont[midiindex]);
            selectedSf = midiindex;
#else
            Debug.Log("Can't change of SoundFont with Free version of MPTK");
#endif

            // return true;
        }

        static public void Display(Vector2 scrollerWindow, CustomStyle myStyle)
        {
            SoundFonts = new List<MPTKListItem>();
            if (MidiPlayerGlobal.MPTK_ListSoundFont == null) return;
            foreach (string name in MidiPlayerGlobal.MPTK_ListSoundFont)
            {
                if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo != null && name == MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name)
                    selectedSf = SoundFonts.Count;
                SoundFonts.Add(new MPTKListItem() { Index = SoundFonts.Count, Label = name });
            }

            if (PopSoundFont == null)
                PopSoundFont = new PopupListItem()
                {
                    Title = "Select A SoundFont",
                    OnSelect = SoundFontChanged,
                    ColCount = 1,
                    ColWidth = 500,
                };

            if (SoundFonts != null)
            {
                PopSoundFont.Draw(SoundFonts, selectedSf, myStyle);
                GUILayout.BeginHorizontal(myStyle.BacgDemos);

                if (buttonIconNote == null)
                    buttonIconNote = Resources.Load<Texture2D>("Textures/Note");
                if (GUILayout.Button(new GUIContent(buttonIconNote, "Select A SoundFont"), GUILayout.Width(60), GUILayout.Height(60)))
                    PopSoundFont.Show = !PopSoundFont.Show;
                GUILayout.Space(20);
                if (MidiPlayerGlobal.ImSFCurrent != null)
                {
                    if (MidiPlayerGlobal.ImSFCurrent.LiveSF)
                        GUILayout.Label("Live SoundFont: " + MidiPlayerGlobal.ImSFCurrent.SoundFontName, myStyle.TitleLabel2, GUILayout.Height(60));
                    else
                        GUILayout.Label("Current SoundFont: " + MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name, myStyle.TitleLabel2, GUILayout.Height(60));
                    GUILayout.Label(string.Format("Load Time:{0} s    Samples:{1} s    Count Presets:{2}   Samples:{3}",
                        Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadSoundFont.TotalSeconds, 3),
                        Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadWave.TotalSeconds, 3),
                        MidiPlayerGlobal.MPTK_CountPresetLoaded,
                        MidiPlayerGlobal.MPTK_CountWaveLoaded),
                        myStyle.TitleLabel2, GUILayout.Height(60));
                }
                else
                    GUILayout.Label("No SoundFont loaded", myStyle.TitleLabel2, GUILayout.Height(60));

                GUILayout.EndHorizontal();

                PopSoundFont.Position(ref scrollerWindow);
            }
            else
            {
                GUILayout.Label("No SoundFont found");
            }
        }
    }
}