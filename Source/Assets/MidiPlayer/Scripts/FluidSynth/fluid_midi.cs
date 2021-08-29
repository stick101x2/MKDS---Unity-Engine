using MidiPlayerTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace MidiPlayerTK
{

    /***************************************************************
     *
     *                   CONSTANTS & ENUM
     */



    //public enum fluid_midi_event_type
    //{
    //    /* channel messages */
    //    NOTE_OFF = 0x80,
    //    NOTE_ON = 0x90,
    //    KEY_PRESSURE = 0xa0,
    //    CONTROL_CHANGE = 0xb0,
    //    PROGRAM_CHANGE = 0xc0,
    //    CHANNEL_PRESSURE = 0xd0,
    //    PITCH_BEND = 0xe0,
    //    /* system exclusive */
    //    MIDI_SYSEX = 0xf0,
    //    /* system common - never in midi files */
    //    MIDI_TIME_CODE = 0xf1,
    //    MIDI_SONG_POSITION = 0xf2,
    //    MIDI_SONG_SELECT = 0xf3,
    //    MIDI_TUNE_REQUEST = 0xf6,
    //    MIDI_EOX = 0xf7,
    //    /* system real-time - never in midi files */
    //    MIDI_SYNC = 0xf8,
    //    MIDI_TICK = 0xf9,
    //    MIDI_START = 0xfa,
    //    MIDI_CONTINUE = 0xfb,
    //    MIDI_STOP = 0xfc,
    //    MIDI_ACTIVE_SENSING = 0xfe,
    //    MIDI_SYSTEM_RESET = 0xff,
    //    /* meta event - for midi files only */
    //    MIDI_META_EVENT = 0xff
    //}

    //public enum fluid_midi_control_change
    //{
    //    BANK_SELECT_MSB = 0x00,
    //    MODULATION_MSB = 0x01,
    //    BREATH_MSB = 0x02,
    //    FOOT_MSB = 0x04,
    //    PORTAMENTO_TIME_MSB = 0x05,
    //    DATA_ENTRY_MSB = 0x06,
    //    VOLUME_MSB = 0x07,
    //    BALANCE_MSB = 0x08,
    //    PAN_MSB = 0x0A,
    //    EXPRESSION_MSB = 0x0B,
    //    EFFECTS1_MSB = 0x0C,
    //    EFFECTS2_MSB = 0x0D,
    //    GPC1_MSB = 0x10, /* general purpose controller */
    //    GPC2_MSB = 0x11,
    //    GPC3_MSB = 0x12,
    //    GPC4_MSB = 0x13,
    //    BANK_SELECT_LSB = 0x20,
    //    MODULATION_WHEEL_LSB = 0x21,
    //    BREATH_LSB = 0x22,
    //    FOOT_LSB = 0x24,
    //    PORTAMENTO_TIME_LSB = 0x25,
    //    DATA_ENTRY_LSB = 0x26,
    //    VOLUME_LSB = 0x27,
    //    BALANCE_LSB = 0x28,
    //    PAN_LSB = 0x2A,
    //    EXPRESSION_LSB = 0x2B,
    //    EFFECTS1_LSB = 0x2C,
    //    EFFECTS2_LSB = 0x2D,
    //    GPC1_LSB = 0x30,
    //    GPC2_LSB = 0x31,
    //    GPC3_LSB = 0x32,
    //    GPC4_LSB = 0x33,
    //    SUSTAIN_SWITCH = 0x40,
    //    PORTAMENTO_SWITCH = 0x41,
    //    SOSTENUTO_SWITCH = 0x42,
    //    SOFT_PEDAL_SWITCH = 0x43,
    //    LEGATO_SWITCH = 0x45,
    //    HOLD2_SWITCH = 0x45,
    //    SOUND_CTRL1 = 0x46,
    //    SOUND_CTRL2 = 0x47,
    //    SOUND_CTRL3 = 0x48,
    //    SOUND_CTRL4 = 0x49,
    //    SOUND_CTRL5 = 0x4A,
    //    SOUND_CTRL6 = 0x4B,
    //    SOUND_CTRL7 = 0x4C,
    //    SOUND_CTRL8 = 0x4D,
    //    SOUND_CTRL9 = 0x4E,
    //    SOUND_CTRL10 = 0x4F,
    //    GPC5 = 0x50,
    //    GPC6 = 0x51,
    //    GPC7 = 0x52,
    //    GPC8 = 0x53,
    //    PORTAMENTO_CTRL = 0x54,
    //    EFFECTS_DEPTH1 = 0x5B,
    //    EFFECTS_DEPTH2 = 0x5C,
    //    EFFECTS_DEPTH3 = 0x5D,
    //    EFFECTS_DEPTH4 = 0x5E,
    //    EFFECTS_DEPTH5 = 0x5F,
    //    DATA_ENTRY_INCR = 0x60,
    //    DATA_ENTRY_DECR = 0x61,
    //    NRPN_LSB = 0x62,
    //    NRPN_MSB = 0x63,
    //    RPN_LSB = 0x64,
    //    RPN_MSB = 0x65,
    //    ALL_SOUND_OFF = 0x78,
    //    ALL_CTRL_OFF = 0x79,
    //    LOCAL_CONTROL = 0x7A,
    //    ALL_NOTES_OFF = 0x7B,
    //    OMNI_OFF = 0x7C,
    //    OMNI_ON = 0x7D,
    //    POLY_OFF = 0x7E,
    //    POLY_ON = 0x7F
    //}

    //public enum midi_meta_event
    //{
    //    MIDI_COPYRIGHT = 0x02,
    //    MIDI_TRACK_NAME = 0x03,
    //    MIDI_INST_NAME = 0x04,
    //    MIDI_LYRIC = 0x05,
    //    MIDI_MARKER = 0x06,
    //    MIDI_CUE_POINT = 0x07,
    //    MIDI_EOT = 0x2f,
    //    MIDI_SET_TEMPO = 0x51,
    //    MIDI_SMPTE_OFFSET = 0x54,
    //    MIDI_TIME_SIGNATURE = 0x58,
    //    MIDI_KEY_SIGNATURE = 0x59,
    //    MIDI_SEQUENCER_EVENT = 0x7f
    //}

    //public enum fluid_player_status
    //{
    //    FLUID_PLAYER_READY,
    //    FLUID_PLAYER_PLAYING,
    //    FLUID_PLAYER_DONE
    //};

    //public enum fluid_driver_status
    //{
    //    FLUID_MIDI_READY,
    //    FLUID_MIDI_LISTENING,
    //    FLUID_MIDI_DONE
    //};

    /***************************************************************
     *
     *         TYPE DEFINITIONS & FUNCTION DECLARATIONS
     */

    /* From ctype.h */
    //#define fluid_isascii(c)    (((c) & ~0x7f) == 0)


    //public class fluid_midi_event_t
    //{
    //    /*
    //     * fluid_midi_event_t
    //     */
    //    public uint dtime;       /* Delay (ticks) between this and previous event. midi tracks. */
    //    public fluid_midi_event_type type;       /* MIDI event type */
    //    public byte channel;    /* MIDI channel */
    //    public int param1;      /* First parameter */
    //    public int param2;      /* Second parameter */
    //    /// <summary>
    //    /// MPTK specific - Note duration in ms. Set to -1 to indefinitely
    //    /// </summary>
    //    public int Duration;
    //}
    /*
     * fluid_track_t
     */
    //public class fluid_track_t
    //{
    //    string name;
    //    int num;
    //    fluid_midi_event_t first;
    //    fluid_midi_event_t cur;
    //    fluid_midi_event_t last;
    //    uint ticks;
    //}

    //#define MAX_NUMBER_OF_TRACKS 128
    //#define fluid_track_eot(track)  ((track)->cur == NULL)
}