using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MidiPlayerTK
{
    /// <summary>
    /// Preset from a ImSoundFont
    /// </summary>
    public class HiPreset
    {
        /// <summary>
        /// unique item id (see int note above)
        /// </summary>
        public int ItemId;
        public string Name;

        /// <summary>
        /// the bank number
        /// </summary>
        public int Bank;
        public int Num;
        public uint Libr;           /* Not used (preserved) */
        public uint Genre;      /* Not used (preserved) */
        public uint Morph;      /* Not used (preserved) */

        public HiZone GlobalZone;
        public HiZone[] Zone;
        public string Description()
        {
            return string.Format(" {0,3:000} {1}", Num, Name);
        }
    }
}
