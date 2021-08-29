using MidiPlayerTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace MidiPlayerTK
{
    /// <summary>
    /// Define parameter and information for the SF to avoid loading all a SF 
    /// </summary>
    public class SoundFontInfo
    {
        public string Name;
        public int PatchCount;
        public int WaveCount;
        public long WaveSize;
        //public int xDefaultBankNumber;
        //public int xDrumKitBankNumber;
        /// <summary>
        /// Path + Filename to the original SF2 files.  
        /// SF2 are stored here : Application.persistentDataPath + MidiPlayerGlobal.PathSF2
        /// </summary>
        public string SF2Path;
    }
}
