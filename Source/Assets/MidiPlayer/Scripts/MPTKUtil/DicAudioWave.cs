using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MidiPlayerTK
{
    /// <summary>
    /// Dictionnary of wave associated with AudioClip
    /// </summary>
    public class DicAudioWave
    {
        private static Dictionary<string, HiSample> dicWave;
        public static void Init()
        {
            dicWave = new Dictionary<string, HiSample>();
        }

        public static void Add(HiSample smpl)
        {
            HiSample c;
            try
            {
                if (!dicWave.TryGetValue(smpl.Name, out c))
                {
                    dicWave.Add(smpl.Name, smpl);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
        public static bool Exist(string name)
        {
            try
            {
                HiSample c;
                return dicWave.TryGetValue(name, out c);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return false;
        }
        public static HiSample Get(string name)
        {
            try
            {
                HiSample c;
                dicWave.TryGetValue(name, out c);
                return c;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return null;
        }
        public static HiSample GetWave(string name)
        {
            try
            {
                HiSample c;

                dicWave.TryGetValue(name, out c);
                return c;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return null;
        }
    }
}
