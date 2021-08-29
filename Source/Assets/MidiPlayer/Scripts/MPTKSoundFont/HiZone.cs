using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace MidiPlayerTK
{
    /// <summary>
    /// Cover fluid_inst_zone_t and fluid_preset_zone_t
    /// </summary>
    public class HiZone
    {
        /// <summary>
        /// unique item id (see int note above)
        /// </summary>
        public int ItemId;

        /// <summary>
        /// Instrument defined in this zone (only for preset zone)
        /// </summary>
        public HiInstrument Instrument;

        //public string Name;
        //public fluid_sample_t sample;
        /// <summary>
        /// Index to the sample (only for instrument zone)
        /// </summary>
        public int Index;
        public int KeyLo;
        public int KeyHi;
        public int VelLo;
        public int VelHi;
        //public fluid_gen_t[] gen;
        public HiGen[] gens;
        public HiMod[] mods; /* List of modulators */

        public HiZone()
        {
            //sample = null;
            Index = -1;
            KeyLo = 0;
            KeyHi = 128;
            VelLo = 0;
            VelHi = 128;
        }
    }
}
