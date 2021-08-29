using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace MidiPlayerTK
{
    /// <summary>
    /// MIDI command codes. Defined the action to be done with the message: note on/off, change instrument, ...
    /// Depending of the command selected, others properties must be set; Value, Channel, ....
    /// </summary>
    public enum MPTKCommand : byte
    {
        /// <summary>Note Off</summary>
        NoteOff = 0x80,
        /// <summary>Note On. Value contains the note to play between 0 and 127.</summary>
        NoteOn = 0x90,
        /// <summary>Key After-touch</summary>
        KeyAfterTouch = 0xA0,
        /// <summary>Control change. Controller contains iendtify the controller to change (Modulation, Pan, Bank Select ...). Value will contains the value of the controller between 0 and 127.</summary>
        ControlChange = 0xB0,
        /// <summary>Patch change. Value contains patch/preset/instrument value between 0 and 127. </summary>
        PatchChange = 0xC0,
        /// <summary>Channel after-touch</summary>
        ChannelAfterTouch = 0xD0,
        /// <summary>Pitch wheel change</summary>
        PitchWheelChange = 0xE0,
        /// <summary>Sysex message</summary>
        Sysex = 0xF0,
        /// <summary>Eox (comes at end of a sysex message)</summary>
        Eox = 0xF7,
        /// <summary>Timing clock (used when synchronization is required)</summary>
        TimingClock = 0xF8,
        /// <summary>Start sequence</summary>
        StartSequence = 0xFA,
        /// <summary>Continue sequence</summary>
        ContinueSequence = 0xFB,
        /// <summary>Stop sequence</summary>
        StopSequence = 0xFC,
        /// <summary>Auto-Sensing</summary>
        AutoSensing = 0xFE,
        /// <summary>Meta-event</summary>
        MetaEvent = 0xFF,
    }

    /// <summary>
    /// MidiController enumeration
    /// http://www.midi.org/techspecs/midimessages.php#3
    /// </summary>
    public enum MPTKController : byte
    {
        /// <summary>Bank Select (MSB)</summary>
        BankSelect = 0,
        /// <summary>Modulation (MSB)</summary>
        Modulation = 1,
        /// <summary>Breath Controller</summary>
        BreathController = 2,
        /// <summary>Foot controller (MSB)</summary>
        FootController = 4,
        /// <summary>Main volume</summary>
        MainVolume = 7,
        /// <summary>Pan</summary>
        Pan = 10,
        /// <summary>Expression</summary>
        Expression = 11,
        /// <summary>Bank Select LSB ** not implemented **  </summary>
        BankSelectLsb = 32,
        /// <summary>Sustain</summary>
        Sustain = 64, // 0x40
        /// <summary>Portamento On/Off </summary>
        Portamento = 65,
        /// <summary>Sostenuto On/Off</summary>
        Sostenuto = 66,
        /// <summary>Soft Pedal On/Off</summary>
        SoftPedal = 67,
        /// <summary>Legato Footswitch</summary>
        LegatoFootswitch = 68,
        /// <summary>Reset all controllers</summary>
        ResetAllControllers = 121,
        /// <summary>All notes off</summary>
        AllNotesOff = 123,
        /// <summary>All sound off</summary>
        AllSoundOff = 120, // 0x78,
    }

    /// <summary>
    /// MIDI MetaEvent Type
    /// </summary>
    public enum MPTKMeta : byte
    {
        /// <summary>Track sequence number</summary>
        TrackSequenceNumber = 0x00,
        /// <summary>Text event</summary>
        TextEvent = 0x01,
        /// <summary>Copyright</summary>
        Copyright = 0x02,
        /// <summary>Sequence track name</summary>
        SequenceTrackName = 0x03,
        /// <summary>Track instrument name</summary>
        TrackInstrumentName = 0x04,
        /// <summary>Lyric</summary>
        Lyric = 0x05,
        /// <summary>Marker</summary>
        Marker = 0x06,
        /// <summary>Cue point</summary>
        CuePoint = 0x07,
        /// <summary>Program (patch) name</summary>
        ProgramName = 0x08,
        /// <summary>Device (port) name</summary>
        DeviceName = 0x09,
        /// <summary>MIDI Channel (not official?)</summary>
        MidiChannel = 0x20,
        /// <summary>MIDI Port (not official?)</summary>
        MidiPort = 0x21,
        /// <summary>End track</summary>
        EndTrack = 0x2F,
        /// <summary>Set tempo</summary>
        SetTempo = 0x51,
        /// <summary>SMPTE offset</summary>
        SmpteOffset = 0x54,
        /// <summary>Time signature (typo error, deprecated!) </summary>
        TimeSignmature = 0x58,
        /// <summary>Time signature</summary>
        TimeSignature = 0x58,
        /// <summary>Key signature</summary>
        KeySignature = 0x59,
        /// <summary>Sequencer specific</summary>
        SequencerSpecific = 0x7F,
    }

    /// <summary>
    /// Midi Event class for MPTK. Use this class to generate Midi Music with MidiStreamPlayer or to read midi events from a Midi file with MidiLoad 
    /// or to receive midi events from MidiFilePlayer OnEventNotesMidi.
    /// With this class, you can: play and stop a note, change instrument (preset, patch, ...), change some control as modulation
    /// See here https://paxstellar.fr/class-mptkevent
    ///! @code
    ///! 
    ///! // Change instrument to Marimba for channel 0
    ///! NotePlaying = new MPTKEvent() {
    ///!        Command = MPTKCommand.NoteOn,
    ///!        Value = 12, // generally Marimba but depend on the SoundFont selected
    ///!        Channel = 0 }; // Instrument are defined by channel. So at any time, only 16 différents instruments can be used simultaneously.
    ///! midiStreamPlayer.MPTK_PlayEvent(NotePlaying);    
    ///!
    ///! // Play a C5 during one second with the Marimba instrument
    ///! NotePlaying = new MPTKEvent() {
    ///!        Command = MPTKCommand.NoteOn,
    ///!        Value = 60, // play a C5 note
    ///!        Channel = 0,
    ///!        Duration = 1000, // one second
    ///!        Velocity = 100 };
    ///! midiStreamPlayer.MPTK_PlayEvent(NotePlaying);    
    ///! @endcode
    /// </summary>
    public partial class MPTKEvent : ICloneable
    {
        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// Track index of the event in the midi. Track 0 is the first track 'MTrk' read from the midi file.
        /// </summary>
        public long Track;

        /// <summary>
        /// Time in Midi Tick (part of a Beat) of the Event since the start of playing the midi file. This time is independent of the Tempo or Speed. Not used for MidiStreamPlayer.
        /// </summary>
        public long Tick;

        /// <summary>
        /// Event Index in the midi list (defined only when Midi events are read from a Midi stream)
        /// </summary>
        public int Index;

        /// <summary>
        /// V2.86 Time in System.DateTime when the Event is created or read from the Midi file.\n
        /// Not to be confused with properties Tick which is a position inside a Midi file. Sure, the name of this properties was a bad idea, could be renamed ;-)
        /// Can be read from a system thread.
        /// </summary>
        public long TickTime;

        /// <summary>
        /// V2.88 Real time in milliseconds of this event from the start of the midi depending the tempo change.
        /// </summary>
        public float RealTime;

        /// <summary>
        /// Midi Command code. Defined the type of message (Note On, Control Change, Patch Change...)
        /// </summary>
        public MPTKCommand Command;

        /// <summary>
        /// Controller code. When the Command is ControlChange, contains the code fo the controller to change (Modulation, Pan, Bank Select ...).\n
        /// Value properties will contains the value of the controller.
        /// </summary>
        public MPTKController Controller;

        /// <summary>
        /// MetaEvent Code. When the Command is MetaEvent, contains the code of the meta event (Lyric, TimeSignature, ...).\n
        /// Info properties will contains the value of the meta.
        /// </summary>
        public MPTKMeta Meta;

        /// <summary>
        /// Information hold by textual meta event when Command=MetaEvent
        /// </summary>
        public string Info;

        /// <summary>
        /// Contains a value between 0 and 127 in relation with the Command. For:
        ///! @li @c   If Command = NoteOn then Value contains midi note. 60=C5, 61=C5#, ..., 72=C6, ....
        ///! @li @c   If Command = ControlChange then Value contains controller value, see MPTKController
        ///! @li @c   If Command = PatchChange then Value contains patch/preset/instrument value. See the current SoundFont to find value associated to each instrument.
        ///! @li @c   If Command = MetaEvent and Meta = SetTempo then Value contains new Microseconds Per Quarter Note.
        /// </summary>
        public int Value;

        /// <summary>
        /// Midi channel fom 0 to 15 (9 for drum)
        /// </summary>
        public int Channel;

        /// <summary>
        /// Velocity between 0 and 127
        /// </summary>
        public int Velocity;

        /// <summary>
        /// Duration of the note in millisecond. Set -1 to play undefinitely.\n
        /// If Command=MetaEvent and Meta=SetTempo then Duration contains new tempo (quarter per minute).
        /// </summary>
        public long Duration;

        /// <summary>
        /// Short delay before playing the note in millisecond. New with V2.82, works only in Core mode.\n
        /// Apply only on NoteOn event.
        /// </summary>
        public long Delay;

        /// <summary>
        /// Duration of the note in Midi Tick. MidiFilePlayer.MPTK_NoteLength can be used to convert this duration.\n
        /// Not used for MidiStreamPlayer, length is set only when reading a Midi file.
        /// https://en.wikipedia.org/wiki/Note_value
        /// </summary>
        public int Length;

        /// <summary>
        /// Note length as https://en.wikipedia.org/wiki/Note_value
        /// </summary>
        public enum EnumLength { Whole, Half, Quarter, Eighth, Sixteenth }

        /// <summary>
        /// Origin of the message. Midi ID if from Midi Input else zero. V2.83: rename source to Source et set public.
        /// </summary>
        public uint Source;

        /// <summary>
        /// Define an Id associated with this event.\n
        /// Used by MPTK_ClearAllSound to clear only a subset of sound associated with this session when Midi events comes from a Midi file.\n
        /// Could be used also MidiStreamPlayer for your specific need.
        /// </summary>
        public int IdSession;


        /// <summary>
        /// V2.87 Tag information
        /// </summary>
        public object Tag;

        /// <summary>
        /// List of voices associated to this Event for playing a NoteOn event.
        /// </summary>
        public List<fluid_voice> Voices;

        /// <summary>
        /// CHeck if playing of this midi event is over (all voices are OFF)
        /// </summary>
        public bool IsOver
        {
            get
            {
                if (Voices != null)
                {
                    foreach (fluid_voice voice in Voices)
                        if (voice.status != fluid_voice_status.FLUID_VOICE_OFF)
                            return false;
                }
                // All voices are off or empty
                return true;
            }
        }

        public MPTKEvent()
        {
            Command = MPTKCommand.NoteOn;
            // V2.82 set default value
            Duration = -1;
            Channel = 0;
            Delay = 0;
            Velocity = 127; // max
            IdSession = -1;
            TickTime = DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// V2.86 Delta time in system ticks since the creation of this event
        /// </summary>
        public long MPTK_DeltaTimeTick { get { return DateTime.UtcNow.Ticks - TickTime; } }

        /// <summary>
        /// V2.86 Delta time in milliseconds since the creation of this event
        /// </summary>
        public long MPTK_DeltaTimeMillis { get { return MPTK_DeltaTimeTick / fluid_voice.Nano100ToMilli; } }

        /// <summary>
        /// Create a MPTK Midi event from a midi input message
        /// </summary>
        /// <param name="data"></param>
        public MPTKEvent(ulong data)
        {
            Source = (uint)(data & 0xffffffffUL);
            Command = (MPTKCommand)((data >> 32) & 0xFF);
            if (Command < MPTKCommand.Sysex)
            {
                Channel = (int)Command & 0xF;
                Command = (MPTKCommand)((int)Command & 0xF0);
            }
            byte data1 = (byte)((data >> 40) & 0xff);
            byte data2 = (byte)((data >> 48) & 0xff);

            if (Command == MPTKCommand.NoteOn && data2 == 0)
                Command = MPTKCommand.NoteOff;

            //if ((int)Command != 0xFE)
            //    Debug.Log($"{data >> 32:X}");

            switch (Command)
            {
                case MPTKCommand.NoteOn:
                    Value = data1; // Key
                    Velocity = data2;
                    Duration = -1; // no duration are defined in Midi flux
                    break;
                case MPTKCommand.NoteOff:
                    Value = data1; // Key
                    Velocity = data2;
                    break;
                case MPTKCommand.KeyAfterTouch:
                    Value = data1; // Key
                    Velocity = data2;
                    break;
                case MPTKCommand.ControlChange:
                    Controller = (MPTKController)data1;
                    Value = data2;
                    break;
                case MPTKCommand.PatchChange:
                    Value = data1;
                    break;
                case MPTKCommand.ChannelAfterTouch:
                    Value = data1;
                    break;
                case MPTKCommand.PitchWheelChange:
                    Value = data2 << 7 | data1; // Pitch-bend is transmitted with 14-bit precision. 
                    break;
            }
        }

        /// <summary>
        /// Build a packet midi message from a MPTKEvent. Example:  0x00403C90 for a noton (90h, 3Ch note,  40h volume)
        /// </summary>
        /// <returns></returns>
        public ulong ToData()
        {
            ulong data = (ulong)Command | ((ulong)Channel & 0xF);
            switch (Command)
            {
                case MPTKCommand.NoteOn:
                    data |= (ulong)Value << 8 | (ulong)Velocity << 16;
                    break;
                case MPTKCommand.NoteOff:
                    data |= (ulong)Value << 8 | (ulong)Velocity << 16;
                    break;
                case MPTKCommand.KeyAfterTouch:
                    data |= (ulong)Value << 8 | (ulong)Velocity << 16;
                    break;
                case MPTKCommand.ControlChange:
                    data |= (ulong)Controller << 8 | (ulong)Value << 16;
                    break;
                case MPTKCommand.PatchChange:
                    data |= (ulong)Value << 8;
                    break;
                case MPTKCommand.ChannelAfterTouch:
                    data |= (ulong)Value << 8;
                    break;
                case MPTKCommand.PitchWheelChange:
                    // The pitch bender is measured by a fourteen bit value. Center (no pitch change) is 2000H. 
                    // Two data after the command code 
                    //  1) the least significant 7 bits. 
                    //  2) the most significant 7 bits.
                    data |= ((ulong)Value & 0x7F) << 8 | ((ulong)Value & 0x7F00) << 16;
                    break;
            }
            return data;
        }

        /// <summary>
        /// Build a string description of the Midi event. V2.83 removes \n on each returns string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result;
            switch (Command)
            {
                case MPTKCommand.NoteOn:
                    string sDuration = Duration == long.MaxValue ? "Inf." : Duration.ToString();
                    result = string.Format("NoteOn\tTrk:{0:00} Ch:{1:00}\tTick:{2}\tNote:{3}\tDuration:{4,-8}\tVelocity:{5}",
                      Track, Channel, Tick, Value, sDuration, Velocity);
                    break;
                case MPTKCommand.NoteOff:
                    sDuration = Duration == long.MaxValue ? "Inf." : Duration.ToString();
                    result = string.Format("NoteOff\tTrk:{0:00} Ch:{1:00}\tTick:{2}\tNote:{3}\tDuration:{4,-8}\tVelocity:{5}",
                      Track, Channel, Tick, Value, sDuration, Velocity);
                    break;
                case MPTKCommand.PatchChange:
                    result = string.Format("Patch\tTrk:{0:00} Ch:{1:00}\tTick:{2}\tPatch:{3}",
                     Track, Channel, Tick, Value);
                    break;
                case MPTKCommand.ControlChange:
                    result = string.Format("Control\tTrk:{0:00} Ch:{1:00}\tTick:{2}\tValue:{3}\tControler:{4}",
                     Track, Channel, Tick, Value, Controller);
                    break;
                case MPTKCommand.KeyAfterTouch:
                    result = string.Format("KeyAfterTouch\tTrk:{0:00} Ch:{1:00}\tTick:{2}\tKey:{3}\tValue:{4}",
                     Track, Channel, Tick, Value, Controller);
                    break;
                case MPTKCommand.ChannelAfterTouch:
                    result = string.Format("ChannelAfterTouch\tTrk:{0:00} Ch:{1:00}\tTick:{2}\tValue:{3}",
                     Track, Channel, Tick, Value);
                    break;
                case MPTKCommand.PitchWheelChange:
                    result = string.Format("Pitch Wheel\tTrk:{0:00} Ch:{1:00}\tTick:{2}\tValue:{3}",
                     Track, Channel, Tick, Value);
                    break;
                case MPTKCommand.MetaEvent:
                    result = string.Format("Meta\tTrk:{0:00} Ch:{1:00}\tTick:{2}\tValue:{3}",
                      Track, Channel, Tick, Info ?? "Empty info");
                    break;
                case MPTKCommand.AutoSensing:
                    result = string.Format("Auto Sensing");
                    break;
                default:
                    result = string.Format("Unknown Command\t:{0:X2} Ch:{1:00}\tTick:{2}\tNote:{3}\tDuration:{4,2}\tVelocity:{5} source:{6}",
                    (int)Command, Channel, Tick, Value, Duration, Velocity, Source);
                    break;
            }
            return result;
        }
    }
}
