﻿using MPTK.NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiPlayerTK
{
    //! @cond NODOC

    /// <summary>
    /// Midi event list (NAUdio format). Internal classe.
    /// </summary>
    public class TrackMidiEvent
    {
        /// <summary>
        /// Track index start from 0
        /// </summary>
        public int IndexTrack;
        public int IndexEvent;
        public long AbsoluteQuantize;
        public float RealTime;
        public MidiEvent Event;
    }

    //! @endcond

}
