using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;


namespace MidiPlayerTK
{
    /// <summary>
    /// Instrument from a fluid_inst_t
    /// </summary>
    public class HiInstrument
    {
        /// <summary>
        /// unique item id (see int note above)
        /// </summary>
        public int ItemId;
        public string Name;
        public HiZone GlobalZone;
        public HiZone[] Zone;
    }
}
