using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace MidiPlayerTK
{

    [System.Serializable]
    public class EventMidiClass : UnityEvent<MPTKEvent>
    {
    }

    [System.Serializable]
    public class EventNotesMidiClass : UnityEvent<List<MPTKEvent>>
    {
    }

    [System.Serializable]
    public class EventSynthClass : UnityEvent<string>
    {
    }


    public enum EventEndMidiEnum
    {
        MidiEnd,
        ApiStop,
        Replay,
        Next,
        Previous,
        MidiErr,
        Loop
    }

    /// <summary>
    /// Status of the last midi file loaded
    ///! @li @c      -1: midi file is loading
    ///! @li @c       0: succes, midi file loaded 
    ///! @li @c       1: error, no Midi found
    ///! @li @c       2: error, not a midi file, too short size
    ///! @li @c       3: error, not a midi file, signature MThd not found.
    ///! @li @c       4: error, network error or site not found.
    /// </summary>
    public enum LoadingStatusMidiEnum
    {
        /// <summary>
        /// -1: midi file is loading.
        /// </summary>
        NotYetDefined = -1,

        /// <summary>
        /// 0: succes, midi file loaded.
        /// </summary>
        Success = 0,

        /// <summary>
        /// 1: error, no Midi file found.
        /// </summary>
        NotFound = 1,

        /// <summary>
        /// 2: error, not a midi file, too short size.
        /// </summary>
        TooShortSize = 2,

        /// <summary>
        /// 3: error, not a midi file, signature MThd not found.
        /// </summary>
        NoMThdSignature = 3,

        /// <summary>
        /// 4: error, network error or site not found (MidiExternalPlayer only).
        /// </summary>
        NetworkError = 4,

        /// <summary>
        /// 5: error, midi file corrupted, error detected when loading the midi events.
        /// </summary>
        MidiFileInvalid = 5,

        /// <summary>
        /// 6: SoundFont not loaded.
        /// </summary>
        SoundFontNotLoaded = 6,

        /// <summary>
        /// 7: error, Already playing.
        /// </summary>
        AlreadyPlaying = 7,

        /// <summary>
        /// 8: error, MPTK_MidiName must start with file:// or http:// or https:// (only for MidiExternalPlayer).
        /// </summary>
        MidiNameInvalid = 8,

        /// <summary>
        /// 9: error,  Set MPTK_MidiName by script or in the inspector with Midi Url/path before playing.
        /// </summary>
        MidiNameNotDefined = 9,

    }

    [System.Serializable]
    public class EventStartMidiClass : UnityEvent<string>
    {
    }


    [System.Serializable]
    public class EventEndMidiClass : UnityEvent<string, EventEndMidiEnum>
    {
    }

    static public class ToolsUnityEvent
    {

        static public bool HasEvent(this EventMidiClass evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }

        static public bool HasEvent(this UnityEvent evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }
        static public bool HasEvent(this EventNotesMidiClass evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }

        static public bool HasEvent(this EventStartMidiClass evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }

        static public bool HasEvent(this EventEndMidiClass evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }

        static public bool HasEvent(this EventSynthClass evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }

    }
}
