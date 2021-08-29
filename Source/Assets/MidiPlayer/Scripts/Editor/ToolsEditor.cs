//#define MPTK_PRO
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MidiPlayerTK
{

    //[InitializeOnLoad]
    //class MPTKParameters
    //{
    //    [MenuItem("MPTK/Folders")]
    //    static void Apply()
    //    {
    //        string path = EditorUtility.OpenFolderPanel("DB", "", "");
    //            Debug.Log("path - " + path);
    //    }

    //    static MPTKParameters()
    //    {
    //        //var guids2 = AssetDatabase.FindAssets("co l:concrete l:architecture t:texture2D", ["Assets/MyAwesomeProps"]);
    //        string[] results = UnityEditor.AssetDatabase.FindAssets("Resources");
    //        foreach (string guid in results)
    //        {
    //            Debug.Log("name:Resources - " + UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
    //        }

    //        //string[] folders = UnityEditor.AssetDatabase.GetAllAssetPaths();
    //        //foreach (string folder in folders)
    //        //{
    //        //    Debug.Log("folder - " + folder);
    //        //}


    //        Debug.Log("applicationContentsPath  " + UnityEditor.EditorApplication.applicationContentsPath);
    //        Debug.Log("applicationPath  " + UnityEditor.EditorApplication.applicationPath);
    //        Debug.Log("dataPath  " + Application.dataPath);
    //        Debug.Log("persistentDataPath  " + Application.persistentDataPath);
    //    }
    //}

    public class ToolsEditor
    {
        public static Color ButtonColor = new Color(.7f, .9f, .7f, 1f);
        public static string lastDirectoryMidi = "";
        public static string paxSite = "https://www.paxstellar.com";
        public static string blogSite = "https://paxstellar.fr/midi-player-tool-kit-for-unity-v2/";
        public static string UnitySite = "https://assetstore.unity.com/packages/tools/audio/midi-tool-kit-pro-115331";
#if MPTK_PRO
        public static string version = "2.88 Pro";
#else
        public static string version = "2.88 Free"; 
#endif
        public static string releaseDate = "April, 03 2021";

        public struct DefineColumn
        {
            public float Width;
            public float PositionCaption;
            public string Caption;
        }

        public static Texture2D SetColor(Texture2D tex2, Color32 color)
        {
            var fillColorArray = tex2.GetPixels32();
            for (var i = 0; i < fillColorArray.Length; ++i)
                fillColorArray[i] = color;
            tex2.SetPixels32(fillColorArray);
            tex2.Apply();
            return tex2;
        }

        public static Texture2D MakeTex(int width, int height, Color textureColor, RectOffset border, Color bordercolor)
        {
            int widthInner = width;
            width += border.left;
            width += border.right;

            Color[] pix = new Color[width * (height + border.top + border.bottom)];

            for (int i = 0; i < pix.Length; i++)
            {
                if (i < (border.bottom * width))
                    pix[i] = bordercolor;
                else if (i >= ((border.bottom * width) + (height * width)))  //Border Top
                    pix[i] = bordercolor;
                else
                { //Center of Texture

                    if ((i % width) < border.left) // Border left
                        pix[i] = bordercolor;
                    else if ((i % width) >= (border.left + widthInner)) //Border right
                        pix[i] = bordercolor;
                    else
                        pix[i] = textureColor;    //Color texture
                }
            }

            Texture2D result = new Texture2D(width, height + border.top + border.bottom);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        /// <summary>
        /// Update list SoundFont and Midi
        /// </summary>
        public static void CheckMidiSet()
        {
            // Activate one time to renum all the midi files in resource folder
            //RenumMidiFile();

            if (MidiPlayerGlobal.PathToSoundfonts == null) return;

            //
            // Check Soundfont
            //
            try
            {
                string folder = System.IO.Path.Combine(Application.dataPath + "/", MidiPlayerGlobal.PathToSoundfonts);
                //Debug.Log($"CheckMidiSet {MidiPlayerGlobal.PathToSoundfonts}");
                if (System.IO.Directory.Exists(folder))
                {
                    bool tobesaved = false;
                    string[] fileEntries = System.IO.Directory.GetFiles(folder, "*" + MidiPlayerGlobal.ExtensionSoundFileDot, System.IO.SearchOption.AllDirectories);

                    // Check if sf has been removed by user in resource
                    int isf = 0;
                    while (isf < MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count)
                    {
                        bool found = false;
                        foreach (string filepath in fileEntries)
                        {
                            string filename = System.IO.Path.GetFileNameWithoutExtension(filepath);
                            if (filename == MidiPlayerGlobal.CurrentMidiSet.SoundFonts[isf].Name)
                                found = true;
                        }
                        if (!found)
                        {
                            MidiPlayerGlobal.CurrentMidiSet.SoundFonts.RemoveAt(isf);
                            tobesaved = true;
                        }
                        else
                            isf++;
                    }

                    // Active sound font exists in midiset ?
                    if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.ImSFCurrent != null)
                    {
                        if (MidiPlayerGlobal.CurrentMidiSet.SoundFonts != null && MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Find(s => s.Name == MidiPlayerGlobal.ImSFCurrent.SoundFontName) == null)
                        {
                            // no the current SF has been remove from resource, define first SF  as active or nothing if no SF exists
                            if (MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count >= 0)
                            {
                                MidiPlayerGlobal.CurrentMidiSet.SetActiveSoundFont(0);
                                MidiPlayerGlobal.LoadCurrentSF();
                                //LoadImSF();
                            }
                            else
                                MidiPlayerGlobal.CurrentMidiSet.SetActiveSoundFont(-1);
                            tobesaved = true;
                        }
                    }
                    if (tobesaved)
                        MidiPlayerGlobal.CurrentMidiSet.Save();
                }

            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }

            try
            {
                string pathMidi = System.IO.Path.Combine(Application.dataPath, MidiPlayerGlobal.PathToMidiFile);
                if (!System.IO.Directory.Exists(pathMidi))
                    System.IO.Directory.CreateDirectory(pathMidi);

                RenameExtFileFromMidToBytes();

                //
                // Check Midifile : remove from DB midifile removed from resource
                //
                bool tobesaved = false;
                List<string> midiFiles = GetMidiFilePath();
                int im = 0;
                while (im < MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count)
                {
                    bool found = false;
                    foreach (string filepath in midiFiles)
                    {
                        string filename = System.IO.Path.GetFileNameWithoutExtension(filepath);
                        if (filename == MidiPlayerGlobal.CurrentMidiSet.MidiFiles[im])
                            found = true;
                    }
                    if (!found)
                    {
                        MidiPlayerGlobal.CurrentMidiSet.MidiFiles.RemoveAt(im);
                        tobesaved = true;
                    }
                    else
                        im++;
                }

                //
                // Check Midifile : Add to DB midifile found from resource
                //
                foreach (string pathmidifile in midiFiles)
                {
                    string filename = System.IO.Path.GetFileNameWithoutExtension(pathmidifile);
                    if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == filename) < 0)
                    {
                        tobesaved = true;
                        MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Add(filename);
                    }
                }

                if (tobesaved)
                {
                    MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Sort();
                    MidiPlayerGlobal.CurrentMidiSet.Save();
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        public static void LoadMidiSet()
        {
            try
            {
                MidiPlayerGlobal.CurrentMidiSet = MidiSet.Load(Application.dataPath + "/" + MidiPlayerGlobal.PathToMidiSet);
                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles == null)
                    MidiPlayerGlobal.CurrentMidiSet.MidiFiles = new List<string>();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private static List<string> GetMidiFilePath()
        {
            List<string> paths = new List<string>();
            try
            {
                string folder = Application.dataPath + "/" + MidiPlayerGlobal.PathToMidiFile;
                if (System.IO.Directory.Exists(folder))
                {
                    try
                    {
                        string[] fileEntries = System.IO.Directory.GetFiles(folder, "*" + MidiPlayerGlobal.ExtensionMidiFile, System.IO.SearchOption.AllDirectories);
                        if (fileEntries.Length > 0)
                        {
                            paths = new List<string>(fileEntries);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning("Error GetMidiFilePath GetFiles " + ex.Message);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return paths;
        }
        private static void RenameExtFileFromMidToBytes()
        {
            try
            {
                string folder = Application.dataPath + "/" + MidiPlayerGlobal.PathToMidiFile;
                if (System.IO.Directory.Exists(folder))
                {
                    string[] fileEntries = System.IO.Directory.GetFiles(folder, "*.mid", System.IO.SearchOption.AllDirectories);
                    foreach (string filename in fileEntries)
                    {
                        try
                        {
                            string target = System.IO.Path.ChangeExtension(filename, MidiPlayerGlobal.ExtensionMidiFile);
                            if (File.Exists(target))
                                File.Delete(target);
                            File.Move(filename, target);
                            string meta = Path.ChangeExtension(filename, ".meta");
                            if (File.Exists(meta))
                                File.Delete(meta);
                        }
                        catch (System.Exception ex)
                        {
                            MidiPlayerGlobal.ErrorDetail(ex);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
        /// <summary>
        /// Renum midi files
        /// </summary>
        public static void RenumMidiFile()
        {
            try
            {
                string folder = Application.dataPath + "/" + MidiPlayerGlobal.PathToMidiFile;
                if (System.IO.Directory.Exists(folder))
                {
                    List<string> fileEntries = new List<string>(System.IO.Directory.GetFiles(folder, "*" + MidiPlayerGlobal.ExtensionMidiFile, System.IO.SearchOption.AllDirectories));
                    fileEntries.Sort();
                    int index = 1;
                    foreach (string filename in fileEntries)
                    {
                        try
                        {
                            if (!System.IO.Path.GetFileName(filename).StartsWith("MPTK_Example"))
                            {
                                string target = "";
                                do
                                {
                                    target = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filename), string.Format("MPTK_Example {0:0000}", index++)) + MidiPlayerGlobal.ExtensionMidiFile;
                                } while (File.Exists(target) && index < 10000);
                                if (index < 10000)
                                {
                                    //Debug.Log("MOVE TO " + target);
                                    File.Move(filename, target);
                                    string meta = Path.ChangeExtension(filename, ".meta");
                                    if (File.Exists(meta))
                                        File.Delete(meta);
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            MidiPlayerGlobal.ErrorDetail(ex);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}
