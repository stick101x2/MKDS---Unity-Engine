using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using MPTK.NAudio.Midi;
using System;
using System.IO;

namespace MidiPlayerTK
{
    /// <summary>
    /// Scan a midifile and return information
    /// </summary>
    public class MidiScan
    {
        /// <summary>
        /// Return information about a midifile : patch change, copyright, ...
        /// </summary>
        /// <param name="pathfilename"></param>
        /// <param name="Info"></param>
        static public List<string> GeneralInfo(string pathfilename, bool withNoteOn, bool withNoteOff, bool withControlChange, bool withPatchChange, bool withAfterTouch, bool withMeta, bool withOthers)
        {
            List<string> Info = new List<string>();
            try
            {
                int NumberBeatsMeasure;
                int NumberQuarterBeat;
                MidiLoad midifile = new MidiLoad();
                midifile.KeepNoteOff = withNoteOff;
                midifile.MPTK_Load(pathfilename);
                if (midifile != null)
                {
                    Info.Add(string.Format("Format: {0}", midifile.midifile.FileFormat));
                    Info.Add(string.Format("Tracks: {0}", midifile.midifile.Tracks));
                    Info.Add(string.Format("Events count: {0}", midifile.MPTK_MidiEvents.Count()));
                    Info.Add(string.Format("Duration: {0} ({1} seconds) {2} Ticks", midifile.MPTK_Duration, midifile.MPTK_Duration.TotalSeconds, midifile.MPTK_TickLast));
                    Info.Add($"Track Count: {midifile.MPTK_TrackCount}");
                    Info.Add(string.Format("Initial Tempo: {0,0:F2} BPM", midifile.MPTK_InitialTempo));
                    Info.Add(string.Format("Beats in a measure: {0}", midifile.MPTK_NumberBeatsMeasure));
                    Info.Add(string.Format("Quarters count in a beat:{0}", midifile.MPTK_NumberQuarterBeat));
                    Info.Add(string.Format("Ticks per Quarter Note: {0}", midifile.midifile.DeltaTicksPerQuarterNote));
                    Info.Add("");
                    //if (false)
                    {
                        foreach (TrackMidiEvent trackEvent in midifile.MPTK_MidiEvents)
                        {
                            switch (trackEvent.Event.CommandCode)
                            {
                                case MidiCommandCode.NoteOn:
                                    if (withNoteOn)
                                    {
                                        NoteOnEvent noteon = (NoteOnEvent)trackEvent.Event;
                                        if (noteon.OffEvent != null)
                                        {
                                            Info.Add(BuildInfoTrack(trackEvent) + string.Format("NoteOn {0,3} ({1,3}) Len:{2,3} Vel:{3,3}", noteon.NoteName, noteon.NoteNumber, noteon.NoteLength, noteon.Velocity));
                                        }
                                        else if (withNoteOff)
                                        {
                                            // It's a noteoff
                                            //NoteEvent noteoff = (NoteEvent)noteon.OffEvent;
                                            Info.Add(BuildInfoTrack(trackEvent) + string.Format("NoteOff {0,3} ({1,3}) Len:{2,3} Vel:{3,3}", noteon.NoteName, noteon.NoteNumber, noteon.NoteLength, noteon.Velocity));
                                        }
                                    }
                                    break;
                                case MidiCommandCode.NoteOff:
                                    if (withNoteOff)
                                    {
                                        NoteEvent noteoff = (NoteEvent)trackEvent.Event;
                                        Info.Add(BuildInfoTrack(trackEvent) + string.Format("NoteOff {0,3} ({1,3}) Vel:{2,3}", noteoff.NoteName, noteoff.NoteNumber, noteoff.Velocity));
                                    }
                                    break;
                                case MidiCommandCode.PitchWheelChange:
                                    if (withOthers)
                                    {
                                        PitchWheelChangeEvent aftertouch = (PitchWheelChangeEvent)trackEvent.Event;
                                        Info.Add(BuildInfoTrack(trackEvent) + string.Format("PitchWheelChange {0,3}", aftertouch.Pitch));
                                    }
                                    break;
                                case MidiCommandCode.KeyAfterTouch:
                                    if (withAfterTouch)
                                    {
                                        NoteEvent aftertouch = (NoteEvent)trackEvent.Event;
                                        Info.Add(BuildInfoTrack(trackEvent) + string.Format("KeyAfterTouch {0,3} ({1,3}) Pressure:{2,3}", aftertouch.NoteName, aftertouch.NoteNumber, aftertouch.Velocity));
                                    }
                                    break;
                                case MidiCommandCode.ChannelAfterTouch:
                                    if (withAfterTouch)
                                    {
                                        ChannelAfterTouchEvent aftertouch = (ChannelAfterTouchEvent)trackEvent.Event;
                                        Info.Add(BuildInfoTrack(trackEvent) + string.Format("ChannelAfterTouch Pressure:{0,3}", aftertouch.AfterTouchPressure));
                                    }
                                    break;
                                case MidiCommandCode.ControlChange:
                                    if (withControlChange)
                                    {
                                        ControlChangeEvent controlchange = (ControlChangeEvent)trackEvent.Event;
                                        Info.Add(BuildInfoTrack(trackEvent) + string.Format("ControlChange {0,3} ({1,3}) Value:{2,3}", controlchange.Controller, controlchange.Controller, controlchange.ControllerValue));
                                    }
                                    break;
                                case MidiCommandCode.PatchChange:
                                    if (withPatchChange)
                                    {
                                        PatchChangeEvent change = (PatchChangeEvent)trackEvent.Event;
                                        Info.Add(BuildInfoTrack(trackEvent) + string.Format("PatchChange {0,3:000} {1}", change.Patch, PatchChangeEvent.GetPatchName(change.Patch)));
                                    }
                                    break;
                                case MidiCommandCode.MetaEvent:
                                    if (withMeta)
                                    {
                                        MetaEvent meta = (MetaEvent)trackEvent.Event;
                                        switch (meta.MetaEventType)
                                        {
                                            case MetaEventType.SetTempo:
                                                TempoEvent tempo = (TempoEvent)meta;
                                                Info.Add(BuildInfoTrack(trackEvent) + string.Format("SetTempo Tempo:{0} MicrosecondsPerQuarterNote:{1}", Math.Round(tempo.Tempo, 0), tempo.MicrosecondsPerQuarterNote));
                                                //tempo.Tempo
                                                break;

                                            case MetaEventType.TimeSignature:
                                                TimeSignatureEvent timesig = (TimeSignatureEvent)meta;
                                                // Numerator: counts the number of beats in a measure. 
                                                // For example a numerator of 4 means that each bar contains four beats. 

                                                // Denominator: number of quarter notes in a beat.0=ronde, 1=blanche, 2=quarter, 3=eighth, etc. 
                                                // Set default value
                                                NumberBeatsMeasure = timesig.Numerator;
                                                NumberQuarterBeat = System.Convert.ToInt32(Mathf.Pow(2, timesig.Denominator));
                                                Info.Add(BuildInfoTrack(trackEvent) + string.Format("TimeSignature Beats Measure:{0} Beat Quarter:{1}", NumberBeatsMeasure, NumberQuarterBeat));
                                                break;

                                            default:
                                                string text = meta is TextEvent ? " '" + ((TextEvent)meta).Text + "'" : "";
                                                Info.Add(BuildInfoTrack(trackEvent) + meta.MetaEventType.ToString() + text);
                                                break;

                                        }
                                    }
                                    break;

                                default:
                                    // Other midi event
                                    if (withOthers)
                                    {
                                        Info.Add(BuildInfoTrack(trackEvent) + string.Format(" {0} ({1})", trackEvent.Event.CommandCode, (int)trackEvent.Event.CommandCode));
                                    }
                                    break;
                            }
                        }
                    }
                    //else DebugMidiSorted(midifile.MidiSorted);
                }
                else
                {
                    Info.Add("Error reading midi file");
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return Info;
        }
        static public void DebugMidiSorted(List<TrackMidiEvent> MidiSorted)
        {
            foreach (TrackMidiEvent midievent in MidiSorted)
            {
                //string info = string.Format("Track:{0} Channel:{1,2:00} Command:{2} AbsoluteTime:{3:0000000} ", midievent.IndexTrack, midievent.Event.Channel, midievent.Event.CommandCode, midievent.Event.AbsoluteTime);
                //switch (midievent.Event.CommandCode)
                //{
                //    case MidiCommandCode.NoteOn:
                //        NoteOnEvent noteon = (NoteOnEvent)midievent.Event;
                //        if (noteon.Velocity == 0)
                //            info += string.Format(" Velocity 0");
                //        if (noteon.OffEvent == null)
                //            info += string.Format(" OffEvent null");
                //        else
                //            info += string.Format(" OffEvent.DeltaTimeChannel:{0:0000.00} ", noteon.OffEvent.DeltaTime);
                //        break;
                //}
                Debug.Log(string.Format("[{0,5:00000}] [T:{1,2:00} C:{2,2:00}] ", midievent.Event.AbsoluteTime, midievent.IndexTrack, midievent.Event.Channel - 1 /*2.84*/) + midievent.Event.ToString());
            }
        }

        private static string BuildInfoTrack(TrackMidiEvent e)
        {
            return $"[I:{e.IndexEvent:00000} A:{e.Event.AbsoluteTime:00000} D:{e.Event.DeltaTime:0000} R:{e.RealTime / 1000f:F2}] [Track:{e.IndexTrack:00} Channel:{e.Event.Channel - 1:00}] ";

            //return string.Format("[{0,5:000000}] [T:{1,2:00} C:{2,2:00} D:{3,4:0000}] ", e.Event.AbsoluteTime, e.IndexTrack, e.Event.Channel - 1 /*2.84*/, e.Event.DeltaTime, e.RealTime);
        }
    }
}

