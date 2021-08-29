//#define DEBUG_STATUS_STAT // also in MidiSynth.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MidiPlayerTK
{
    public class HelperDemo
    {

        public int Midi;
        public string Label;
        //public float Ratio;
        //public float Frequence;

        static List<HelperDemo> ListNote;
        static List<HelperDemo> ListEcart;
        //static public float _ratioHalfTone = 0.0594630943592952645618252949463f;

        static public void DisplayInfoSynth(MidiSynth synth, int width, CustomStyle myStyle)
        {
            string info;
            GUILayout.BeginHorizontal(GUILayout.Width(width));
            GUILayout.Label("Synthesizer statistics", myStyle.TitleLabel3, GUILayout.Width(150));
            if (GUILayout.Button("Reset Stat", GUILayout.Width(100)))
            {
                synth.MPTK_ResetStat();
            }
            GUILayout.EndHorizontal();
            info = string.Format("Mode:\t{0}\tRate:{1}\tBuffer:{2}\tDSP:{3,-5:F2} ms\n", synth.MPTK_CorePlayer ? "Core" : "AudioSource", synth.OutputRate, synth.DspBufferSize, Math.Round(synth.StatDeltaAudioFilterReadMS, 2));
            info += string.Format("Voice:\tPlayed:{0,-4}\tFree:{1,-4}\tActive:{2,-4}\tReused:{3} %\n",
                synth.MPTK_StatVoicePlayed, synth.MPTK_StatVoiceCountFree,
                synth.MPTK_StatVoiceCountActive, Mathf.RoundToInt(synth.MPTK_StatVoiceRatioReused));

#if DEBUG_STATUS_STAT
            if (synth.StatusStat != null && synth.StatusStat.Length >= (int)fluid_voice_status.FLUID_VOICE_OFF + 2)
            {
                info += string.Format("\t\tSustain:{0,-4}\tRelease:{1,-4}\n\n",
                    synth.StatusStat[(int)fluid_voice_status.FLUID_VOICE_SUSTAINED],
                    synth.StatusStat[(int)fluid_voice_status.FLUID_VOICE_OFF + 1]
                );
            }
#endif
            if (synth.StatAudioFilterReadMA != null)
            {
                info += string.Format("Stat Synth:\tSample:{0,-5:F2} ms\tMini:{1,-5:F2}\tMaxi:{2,-5:F2}\tAvg:{3,-5:F2}\n",
                    Math.Round(synth.StatAudioFilterReadMS, 2),
                    synth.StatAudioFilterReadMIN < double.MaxValue ? Math.Round(synth.StatAudioFilterReadMIN, 2) : 0,
                    Math.Round(synth.StatAudioFilterReadMAX, 2),
                    Math.Round(synth.StatAudioFilterReadAVG, 2));
            }

            if (synth.StatDspLoadMAX != 0f)
                info += string.Format("\tLoad:{0} %\tMini:{1,-5:F2}\tMaxi:{2,-5:F2}\tAvg:{3,-5:F2}",//\tLong Avg:{3,-5:F2}",
                    Math.Round(synth.StatDspLoadPCT, 2),
                    Math.Round(synth.StatDspLoadMIN, 2),
                    Math.Round(synth.StatDspLoadMAX, 2),
                    Math.Round(synth.StatDspLoadAVG, 2));
                    //Math.Round(synth.StatDspLoadLongAVG, 1));
            else
                info += string.Format("\tDSP Load:{0} % ", Math.Round(synth.StatDspLoadPCT, 1));

            if (synth.StatDspLoadPCT >= 100f)
                info += string.Format("\n\t<color=red>\tDSP Load over 100%</color>");
            else if (synth.StatDspLoadPCT >= synth.MaxDspLoad)
                info += string.Format("\n\t<color=orange>\tDSP Load over {0}%</color>", synth.MaxDspLoad);
            else info += "\n";

            // Available only when a file Midi reader is enabled
            if (synth.StatDeltaThreadMidiMA != null && synth.StatDeltaThreadMidiMIN < double.MaxValue)
            {
                info += string.Format("\nStat Sequencer:\tDelta:{0,-5:F2} ms\tMini:{1,-5:F2}\tMaxi:{2,-5:F2}\tAvg:{3,-5:F2}\n",
                    Math.Round(synth.StatDeltaThreadMidiMS, 2),
                    synth.StatDeltaThreadMidiMIN < double.MaxValue ? Math.Round(synth.StatDeltaThreadMidiMIN, 2) : 0,
                    Math.Round(synth.StatDeltaThreadMidiMAX, 2),
                    Math.Round(synth.StatDeltaThreadMidiAVG, 2));
                info += string.Format("\tRead:{0,-5:F2} ms\tTreat:{1,-5:F2}\tMaxi:{2,-5:F2}",
                    Math.Round(synth.StatReadMidiMS, 2),
                    Math.Round(synth.StatProcessMidiMS, 2),
                    Math.Round(synth.StatProcessMidiMAX, 2));
            }

            //#if !UNITY_ANDROID
            //#endif
            GUILayout.Label(info, myStyle.TextFieldMultiCourier);
        }


        static public bool CheckSFExists()
        {
            if (MidiPlayerGlobal.ImSFCurrent == null || !MidiPlayerGlobal.MPTK_SoundFontLoaded)
            {
                //Debug.Log(MidiPlayerGlobal.ErrorNoSoundFont);
                return false;
            }
            return true;
        }

        static public string LabelFromMidi(int midi)
        {
            if (midi < 0 || midi >= ListNote.Count)
                return "xx";
            else
                return ListNote[midi].Label;
        }

        static public string LabelFromEcart(int ecart)
        {
            if (ecart < 0 || ecart >= 12)
                return "xx";
            else
                return ListEcart[ecart].Label;
        }
        static public void InitNote()
        {
            ListEcart = new List<HelperDemo>();
            ListEcart.Add(new HelperDemo() { Label = "C", Midi = 0, });
            ListEcart.Add(new HelperDemo() { Label = "C#", Midi = 1, });
            ListEcart.Add(new HelperDemo() { Label = "D", Midi = 2, });
            ListEcart.Add(new HelperDemo() { Label = "D#", Midi = 3, });
            ListEcart.Add(new HelperDemo() { Label = "E", Midi = 4, });
            ListEcart.Add(new HelperDemo() { Label = "F", Midi = 5, });
            ListEcart.Add(new HelperDemo() { Label = "F#", Midi = 6, });
            ListEcart.Add(new HelperDemo() { Label = "G", Midi = 7, });
            ListEcart.Add(new HelperDemo() { Label = "G#", Midi = 8, });
            ListEcart.Add(new HelperDemo() { Label = "A", Midi = 9, });
            ListEcart.Add(new HelperDemo() { Label = "A#", Midi = 10, });
            ListEcart.Add(new HelperDemo() { Label = "B", Midi = 11, });

            ListNote = new List<HelperDemo>();
            ListNote.Add(new HelperDemo() { Label = "C0", Midi = 0, });
            ListNote.Add(new HelperDemo() { Label = "C0#", Midi = 1, });
            ListNote.Add(new HelperDemo() { Label = "D0", Midi = 2, });
            ListNote.Add(new HelperDemo() { Label = "D0#", Midi = 3, });
            ListNote.Add(new HelperDemo() { Label = "E0", Midi = 4, });
            ListNote.Add(new HelperDemo() { Label = "F0", Midi = 5, });
            ListNote.Add(new HelperDemo() { Label = "F0#", Midi = 6, });
            ListNote.Add(new HelperDemo() { Label = "G0", Midi = 7, });
            ListNote.Add(new HelperDemo() { Label = "G0#", Midi = 8, });
            ListNote.Add(new HelperDemo() { Label = "A0", Midi = 9, });
            ListNote.Add(new HelperDemo() { Label = "A0#", Midi = 10, });
            ListNote.Add(new HelperDemo() { Label = "B0", Midi = 11, });
            ListNote.Add(new HelperDemo() { Label = "C1", Midi = 12, });
            ListNote.Add(new HelperDemo() { Label = "C1#", Midi = 13, });
            ListNote.Add(new HelperDemo() { Label = "D1", Midi = 14, });
            ListNote.Add(new HelperDemo() { Label = "D1#", Midi = 15, });
            ListNote.Add(new HelperDemo() { Label = "E1", Midi = 16, });
            ListNote.Add(new HelperDemo() { Label = "F1", Midi = 17, });
            ListNote.Add(new HelperDemo() { Label = "F1#", Midi = 18, });
            ListNote.Add(new HelperDemo() { Label = "G1", Midi = 19, });
            ListNote.Add(new HelperDemo() { Label = "G1#", Midi = 20, });
            ListNote.Add(new HelperDemo() { Label = "A1", Midi = 21, });
            ListNote.Add(new HelperDemo() { Label = "A1#", Midi = 22, });
            ListNote.Add(new HelperDemo() { Label = "B1", Midi = 23, });
            ListNote.Add(new HelperDemo() { Label = "C2", Midi = 24, });
            ListNote.Add(new HelperDemo() { Label = "C2#", Midi = 25, });
            ListNote.Add(new HelperDemo() { Label = "D2", Midi = 26, });
            ListNote.Add(new HelperDemo() { Label = "D2#", Midi = 27, });
            ListNote.Add(new HelperDemo() { Label = "E2", Midi = 28, });
            ListNote.Add(new HelperDemo() { Label = "F2", Midi = 29, });
            ListNote.Add(new HelperDemo() { Label = "F2#", Midi = 30, });
            ListNote.Add(new HelperDemo() { Label = "G2", Midi = 31, });
            ListNote.Add(new HelperDemo() { Label = "G2#", Midi = 32, });
            ListNote.Add(new HelperDemo() { Label = "A2", Midi = 33, });
            ListNote.Add(new HelperDemo() { Label = "A2#", Midi = 34, });
            ListNote.Add(new HelperDemo() { Label = "B2", Midi = 35, });
            ListNote.Add(new HelperDemo() { Label = "C3", Midi = 36, });
            ListNote.Add(new HelperDemo() { Label = "C3#", Midi = 37, });
            ListNote.Add(new HelperDemo() { Label = "D3", Midi = 38, });
            ListNote.Add(new HelperDemo() { Label = "D3#", Midi = 39, });
            ListNote.Add(new HelperDemo() { Label = "E3", Midi = 40, });
            ListNote.Add(new HelperDemo() { Label = "F3", Midi = 41, });
            ListNote.Add(new HelperDemo() { Label = "F3#", Midi = 42, });
            ListNote.Add(new HelperDemo() { Label = "G3", Midi = 43, });
            ListNote.Add(new HelperDemo() { Label = "G3#", Midi = 44, });
            ListNote.Add(new HelperDemo() { Label = "A3", Midi = 45, });
            ListNote.Add(new HelperDemo() { Label = "A3#", Midi = 46, });
            ListNote.Add(new HelperDemo() { Label = "B3", Midi = 47, });
            ListNote.Add(new HelperDemo() { Label = "C4", Midi = 48, });
            ListNote.Add(new HelperDemo() { Label = "C4#", Midi = 49, });
            ListNote.Add(new HelperDemo() { Label = "D4", Midi = 50, });
            ListNote.Add(new HelperDemo() { Label = "D4#", Midi = 51, });
            ListNote.Add(new HelperDemo() { Label = "E4", Midi = 52, });
            ListNote.Add(new HelperDemo() { Label = "F4", Midi = 53, });
            ListNote.Add(new HelperDemo() { Label = "F4#", Midi = 54, });
            ListNote.Add(new HelperDemo() { Label = "G4", Midi = 55, });
            ListNote.Add(new HelperDemo() { Label = "G4#", Midi = 56, });
            ListNote.Add(new HelperDemo() { Label = "A4", Midi = 57, });
            ListNote.Add(new HelperDemo() { Label = "A4#", Midi = 58, });
            ListNote.Add(new HelperDemo() { Label = "B4", Midi = 59, });
            ListNote.Add(new HelperDemo() { Label = "C5", Midi = 60, });
            ListNote.Add(new HelperDemo() { Label = "C5#", Midi = 61, });
            ListNote.Add(new HelperDemo() { Label = "D5", Midi = 62, });
            ListNote.Add(new HelperDemo() { Label = "D5#", Midi = 63, });
            ListNote.Add(new HelperDemo() { Label = "E5", Midi = 64, });
            ListNote.Add(new HelperDemo() { Label = "F5", Midi = 65, });
            ListNote.Add(new HelperDemo() { Label = "F5#", Midi = 66, });
            ListNote.Add(new HelperDemo() { Label = "G5", Midi = 67, });
            ListNote.Add(new HelperDemo() { Label = "G5#", Midi = 68, });
            ListNote.Add(new HelperDemo() { Label = "A5", Midi = 69, });
            ListNote.Add(new HelperDemo() { Label = "A5#", Midi = 70, });
            ListNote.Add(new HelperDemo() { Label = "B5", Midi = 71, });
            ListNote.Add(new HelperDemo() { Label = "C6", Midi = 72, });
            ListNote.Add(new HelperDemo() { Label = "C6#", Midi = 73, });
            ListNote.Add(new HelperDemo() { Label = "D6", Midi = 74, });
            ListNote.Add(new HelperDemo() { Label = "D6#", Midi = 75, });
            ListNote.Add(new HelperDemo() { Label = "E6", Midi = 76, });
            ListNote.Add(new HelperDemo() { Label = "F6", Midi = 77, });
            ListNote.Add(new HelperDemo() { Label = "F6#", Midi = 78, });
            ListNote.Add(new HelperDemo() { Label = "G6", Midi = 79, });
            ListNote.Add(new HelperDemo() { Label = "G6#", Midi = 80, });
            ListNote.Add(new HelperDemo() { Label = "A6", Midi = 81, });
            ListNote.Add(new HelperDemo() { Label = "A6#", Midi = 82, });
            ListNote.Add(new HelperDemo() { Label = "B6", Midi = 83, });
            ListNote.Add(new HelperDemo() { Label = "C7", Midi = 84, });
            ListNote.Add(new HelperDemo() { Label = "C7#", Midi = 85, });
            ListNote.Add(new HelperDemo() { Label = "D7", Midi = 86, });
            ListNote.Add(new HelperDemo() { Label = "D7#", Midi = 87, });
            ListNote.Add(new HelperDemo() { Label = "E7", Midi = 88, });
            ListNote.Add(new HelperDemo() { Label = "F7", Midi = 89, });
            ListNote.Add(new HelperDemo() { Label = "F7#", Midi = 90, });
            ListNote.Add(new HelperDemo() { Label = "G7", Midi = 91, });
            ListNote.Add(new HelperDemo() { Label = "G7#", Midi = 92, });
            ListNote.Add(new HelperDemo() { Label = "A7", Midi = 93, });
            ListNote.Add(new HelperDemo() { Label = "A7#", Midi = 94, });
            ListNote.Add(new HelperDemo() { Label = "B7", Midi = 95, });
            ListNote.Add(new HelperDemo() { Label = "C8", Midi = 96, });
            ListNote.Add(new HelperDemo() { Label = "C8#", Midi = 97, });
            ListNote.Add(new HelperDemo() { Label = "D8", Midi = 98, });
            ListNote.Add(new HelperDemo() { Label = "D8#", Midi = 99, });
            ListNote.Add(new HelperDemo() { Label = "E8", Midi = 100, });
            ListNote.Add(new HelperDemo() { Label = "F8", Midi = 101, });
            ListNote.Add(new HelperDemo() { Label = "F8#", Midi = 102, });
            ListNote.Add(new HelperDemo() { Label = "G8", Midi = 103, });
            ListNote.Add(new HelperDemo() { Label = "G8#", Midi = 104, });
            ListNote.Add(new HelperDemo() { Label = "A8", Midi = 105, });
            ListNote.Add(new HelperDemo() { Label = "A8#", Midi = 106, });
            ListNote.Add(new HelperDemo() { Label = "B8", Midi = 107, });
            ListNote.Add(new HelperDemo() { Label = "C9", Midi = 108, });
            ListNote.Add(new HelperDemo() { Label = "C9#", Midi = 109, });
            ListNote.Add(new HelperDemo() { Label = "D9", Midi = 110, });
            ListNote.Add(new HelperDemo() { Label = "D9#", Midi = 111, });
            ListNote.Add(new HelperDemo() { Label = "E9", Midi = 112, });
            ListNote.Add(new HelperDemo() { Label = "F9", Midi = 113, });
            ListNote.Add(new HelperDemo() { Label = "F9#", Midi = 114, });
            ListNote.Add(new HelperDemo() { Label = "G9", Midi = 115, });
            ListNote.Add(new HelperDemo() { Label = "G9#", Midi = 116, });
            ListNote.Add(new HelperDemo() { Label = "A9", Midi = 117, });
            ListNote.Add(new HelperDemo() { Label = "A9#", Midi = 118, });
            ListNote.Add(new HelperDemo() { Label = "B9", Midi = 119, });
            ListNote.Add(new HelperDemo() { Label = "C10", Midi = 120, });
            ListNote.Add(new HelperDemo() { Label = "C10#", Midi = 121, });
            ListNote.Add(new HelperDemo() { Label = "D10", Midi = 122, });
            ListNote.Add(new HelperDemo() { Label = "D10#", Midi = 123, });
            ListNote.Add(new HelperDemo() { Label = "E10", Midi = 124, });
            ListNote.Add(new HelperDemo() { Label = "F10", Midi = 125, });
            ListNote.Add(new HelperDemo() { Label = "F10#", Midi = 126, });
            ListNote.Add(new HelperDemo() { Label = "G10", Midi = 127, });

            //ListNote[60].Ratio = 1f; // C3
            //ListNote[60].Frequence = 261.626f; // C3

            //foreach (HelperNote hn in ListNote)
            //{
            //    hn.Ratio = Mathf.Pow(_ratioHalfTone, hn.Midi);
            //    hn.Frequence = ListNote[48].Frequence * hn.Ratio;
            //    //Debug.Log("Position:" + hn.Position +" Hauteur:" + hn.Hauteur +" Label:" + hn.Label +" Ratio:" + hn.Ratio +" Frequence:" + hn.Frequence);
            //}
        }
    }
}
