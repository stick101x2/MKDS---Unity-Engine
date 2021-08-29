using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace MidiPlayerTK
{

    // specific channel properties
    public class mptk_channel // V2.82 new
    {
        public bool enabled; // V2.82 move from fluid_channel 
        public float volume; // volume for the channel, between 0 and 1
        public int forcedPreset; // forced preset fot his channel
        public int count; // count of noteon for the channel
        //private int channum;
        //private MidiSynth synth;
        public mptk_channel(MidiSynth psynth, int pchanum)
        {
            //synth = psynth;
            //channum = pchanum;
            enabled = true;
            volume = 1f;
            count = 0;
            forcedPreset = -1;
        }
    }

    public class fluid_channel
    {
        public int channum;
        public int banknum;
        public int prognum;
        public HiPreset preset;
        private MidiSynth synth;
        public short key_pressure;
        public short channel_pressure;
        public short pitch_bend;
        public short pitch_wheel_sensitivity;

        // NRPN system 
        //int nrpn_select;
        // cached values of last MSB values of MSB/LSB controllers
        //byte bank_msb;

        // controller values
        public short[] cc;

        // the micro-tuning
        public fluid_tuning tuning;

        /* The values of the generators, set by NRPN messages, or by
         * fluid_synth_set_gen(), are cached in the channel so they can be
         * applied to future notes. They are copied to a voice's generators
         * in fluid_voice_init(), wihich calls fluid_gen_init().  */
        public double[] gens;

        /* By default, the NRPN values are relative to the values of the
         * generators set in the SoundFont. For example, if the NRPN
         * specifies an attack of 100 msec then 100 msec will be added to the
         * combined attack time of the sound font and the modulators.
         *
         * However, it is useful to be able to specify the generator value
         * absolutely, completely ignoring the generators of the sound font
         * and the values of modulators. The gen_abs field, is a boolean
         * flag indicating whether the NRPN value is absolute or not.
         */
        public bool[] gen_abs;

        public fluid_channel()
        {
        }

        public fluid_channel(MidiSynth psynth, int pchanum)
        {
            gens = new double[Enum.GetNames(typeof(fluid_gen_type)).Length];
            gen_abs = new bool[Enum.GetNames(typeof(fluid_gen_type)).Length];
            cc = new short[128];

            synth = psynth;
            channum = pchanum;
            preset = null;
            tuning = null;

            fluid_channel_init();
            fluid_channel_init_ctrl();
        }


        void fluid_channel_init()
        {
            prognum = 0;
            if (MidiPlayerGlobal.ImSFCurrent != null)
            {
                banknum = channum == 9 ? MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber : MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber;
                preset = synth.fluid_synth_find_preset(banknum, prognum);
            }
        }

        void fluid_channel_init_ctrl()
        {
            key_pressure = 0;
            channel_pressure = 0;
            pitch_bend = 0x2000; // Range is 0x4000, pitch bend wheel starts in centered position
            pitch_wheel_sensitivity = 2; // two semi-tones 

            for (int i = 0; i < gens.Length; i++)
            {
                gens[i] = 0.0f;
                gen_abs[i] = false;
            }

            for (int i = 0; i < 128; i++)
            {
                cc[i] = 0;  // SETCC(_c,_n,_v)  _c->cc[_n] = _v
            }

            // Volume / initial attenuation (MSB & LSB) 
            cc[7] = 127;
            cc[39] = 0;

            // Pan (MSB & LSB) 
            cc[10] = 64;
            //? cc[10] = 64;

            // Expression (MSB & LSB) 
            cc[11] = 127;
            cc[43] = 127;
        }


        /*
         * fluid_channel_cc
         */
        public void fluid_channel_cc(MPTKController numController, int valueController)
        {
            cc[(int)numController] = (short)valueController;

            if (synth.VerboseController)
            {
                Debug.LogFormat("ChangeController\tChannel:{0}\tControl:{1}\tValue:{2}", channum, numController, valueController);
            }

            switch (numController)
            {
                case MPTKController.Sustain:
                    {
                        if (valueController < 64)
                        {
                            /*  	printf("** sustain off\n"); */
                            synth.fluid_synth_damp_voices(channum);
                        }
                        else
                        {
                            /*  	printf("** sustain on\n"); */
                        }
                    }
                    break;

                case MPTKController.BankSelect:
                    banknum = valueController;
                    break;

                case MPTKController.BankSelectLsb:
                    {
                        // Not implemented
                        // FIXME: according to the Downloadable Sounds II specification, bit 31 should be set when we receive the message on channel 10 (drum channel)
                        //TBC fluid_channel_set_banknum(chan, (((unsigned int)value & 0x7f) + ((unsigned int)chan->bank_msb << 7)));
                    }
                    break;

                case MPTKController.AllNotesOff:
                    synth.fluid_synth_noteoff(channum, -1);
                    break;

                case MPTKController.AllSoundOff:
                    synth.fluid_synth_soundoff(channum);
                    break;

                case MPTKController.ResetAllControllers:
                    fluid_channel_init_ctrl();
                    synth.fluid_synth_modulate_voices_all(channum);
                    break;

                //case MPTKController.DATA_ENTRY_MSB:
                //    {
                //        //int data = (value << 7) + chan->cc[DATA_ENTRY_LSB];

                ///* SontFont 2.01 NRPN Message (Sect. 9.6, p. 74)  */
                //if ((chan->cc[NRPN_MSB] == 120) && (chan->cc[NRPN_LSB] < 100))
                //{
                //    float val = fluid_gen_scale_nrpn(chan->nrpn_select, data);
                //    FLUID_LOG(FLUID_WARN, "%s: %d: Data = %d, value = %f", __FILE__, __LINE__, data, val);
                //    fluid_synth_set_gen(chan->synth, chan->channum, chan->nrpn_select, val);
                //}
                //    break;
                //}

                //case MPTKController.NRPN_MSB:
                //    cc[(int)MPTKController.NRPN_LSB] = 0;
                //    nrpn_select = 0;
                //    break;

                //case MPTKController.NRPN_LSB:
                //    /* SontFont 2.01 NRPN Message (Sect. 9.6, p. 74)  */
                //    if (cc[(int)MPTKController.NRPN_MSB] == 120)
                //    {
                //        if (value == 100)
                //        {
                //            nrpn_select += 100;
                //        }
                //        else if (value == 101)
                //        {
                //            nrpn_select += 1000;
                //        }
                //        else if (value == 102)
                //        {
                //            nrpn_select += 10000;
                //        }
                //        else if (value < 100)
                //        {
                //            nrpn_select += value;
                //            Debug.LogWarning(string.Format("NRPN Select = {0}", nrpn_select));
                //        }
                //    }
                //    break;

                //case MPTKController.RPN_MSB:
                //    break;

                //case MPTKController.RPN_LSB:
                //    // erase any previously received NRPN message 
                //    cc[(int)MPTKController.NRPN_MSB] = 0;
                //    cc[(int)MPTKController.NRPN_LSB] = 0;
                //    nrpn_select = 0;
                //    break;

                default:
                    if (synth.MPTK_ApplyRealTimeModulator)
                        synth.fluid_synth_modulate_voices(channum, 1, (int)numController);
                    break;
            }
        }


        /*
         * fluid_channel_pitch_bend
         */
        public void fluid_channel_pitch_bend(int val)
        {
            if (synth.VerboseController)
            {
                Debug.LogFormat("PitchChange\tChannel:{0}\tValue:{1}", channum, val);
            }
            pitch_bend = (short)val;
            synth.fluid_synth_modulate_voices(channum, 0, (int)fluid_mod_src.FLUID_MOD_PITCHWHEEL); //STRANGE
        }
    }
}
