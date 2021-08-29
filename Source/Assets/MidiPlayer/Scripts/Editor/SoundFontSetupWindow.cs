//#define MPTK_PRO
using System;
using System.Collections.Generic;
using MPTK.NAudio.Midi;
namespace MidiPlayerTK
{
    //using MonoProjectOptim;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Window editor for the setup of MPTK
    /// </summary>
    public class SoundFontSetupWindow : EditorWindow
    {
        private static SoundFontSetupWindow window;

        Vector2 scrollPosBanks = Vector2.zero;
        Vector2 scrollPosSoundFont = Vector2.zero;

        static float widthLeft = 500 + 30;
        static float widthRight; // calculated

        static float heightLeftTop = 300;
        static float heightRightTop = 400;
        static float heightLeftBottom;  // calculated
        static float heightRightBottom; // calculated

        static float itemHeight = 25;
        static float titleHeight = 18; //label title above list
        static float buttonLargeWidth = 180;
        static float buttonMediumWidth = 60;
        static float buttonHeight = 18;
        static float espace = 5;

        static float xpostitlebox = 2;
        static float ypostitlebox = 5;

        static GUIStyle styleWindow;
        static GUIStyle stylePanel;
        static GUIStyle styleBold;
        static GUIStyle styleRed;
        static GUIStyle styleRichText;
        static GUIStyle styleLabelLeft;
        static GUIStyle styleLabelRight;
        static GUIStyle styleMiniButton;
        static GUIStyle styleListTitle;
        static GUIStyle styleListRow;
        static GUIStyle styleListRowSelected;
        static GUIStyle styleListLabel;
        static GUIStyle styleToggle;

        public static BuilderInfo LogInfo;
#if MPTK_PRO
        Vector2 scrollPosOptim = Vector2.zero;
#endif
        static public bool KeepAllPatchs = false;
        static public bool KeepAllZones = false;
        static public bool RemoveUnusedWaves = false;
        static public bool LogDetailSoundFont = false;

        private Texture buttonIconView;
        private Texture buttonIconHelp;
        private Texture buttonIconSave;
        private Texture buttonIconFolders;
        private Texture buttonIconDelete;

        ToolsEditor.DefineColumn[] columnSF;
        ToolsEditor.DefineColumn[] columnBank;


        // % (ctrl on Windows, cmd on macOS), # (shift), & (alt).
        [MenuItem("MPTK/SoundFont Setup &F", false, 20)]
        public static void Init()
        {
            //Debug.Log("init");
            // Get existing open window or if none, make a new one:
            try
            {
                window = GetWindow<SoundFontSetupWindow>(true, "SoundFont Setup");
                window.minSize = new Vector2(989, 568);
                //Debug.Log(window.position);

                int borderSize = 1; // Border size in pixels
                RectOffset rectBorder = new RectOffset(borderSize, borderSize, borderSize, borderSize);

                styleBold = new GUIStyle(EditorStyles.boldLabel);
                styleBold.fontStyle = FontStyle.Bold;
                styleBold.alignment = TextAnchor.UpperLeft;
                styleBold.normal.textColor = Color.black;

                styleMiniButton = new GUIStyle(EditorStyles.miniButtonMid);
                styleMiniButton.fixedWidth = 16;
                styleMiniButton.fixedHeight = 16;
                float gray1 = 0.5f;
                float gray2 = 0.1f;
                float gray3 = 0.7f;
                float gray4 = 0.65f;
                float gray5 = 0.5f;

                styleWindow = new GUIStyle("box");
                styleWindow.normal.background = ToolsEditor.MakeTex(10, 10, new Color(gray5, gray5, gray5, 1f), rectBorder, new Color(gray2, gray2, gray2, 1f));
                styleWindow.alignment = TextAnchor.MiddleCenter;

                stylePanel = new GUIStyle("box");
                stylePanel.normal.background = ToolsEditor.MakeTex(10, 10, new Color(gray4, gray4, gray4, 1f), rectBorder, new Color(gray2, gray2, gray2, 1f));
                stylePanel.alignment = TextAnchor.MiddleCenter;

                styleListTitle = new GUIStyle("box");
                styleListTitle.normal.background = ToolsEditor.MakeTex(10, 10, new Color(gray1, gray1, gray1, 1f), rectBorder, new Color(gray2, gray2, gray2, 1f));
                styleListTitle.normal.textColor = Color.black;
                styleListTitle.alignment = TextAnchor.MiddleCenter;

                styleListRow = new GUIStyle("box");
                styleListRow.normal.background = ToolsEditor.MakeTex(10, 10, new Color(gray3, gray3, gray3, 1f), rectBorder, new Color(gray2, gray2, gray2, 1f));
                styleListRow.alignment = TextAnchor.MiddleCenter;

                styleListRowSelected = new GUIStyle("box");
                styleListRowSelected.normal.background = ToolsEditor.MakeTex(10, 10, new Color(.6f, .8f, .6f, 1f), rectBorder, new Color(gray2, gray2, gray2, 1f));
                styleListRowSelected.alignment = TextAnchor.MiddleCenter;

                styleListLabel = new GUIStyle("label");
                styleListLabel.alignment = TextAnchor.UpperLeft;
                styleListLabel.normal.textColor = Color.black;

                styleToggle = new GUIStyle("toggle");
                //styleToggle.normal.background = ToolsEditor.MakeTex(10, 10, new Color(gray1, gray1, gray1, 1f), rectBorder, new Color(gray2, gray2, gray2, 1f));
                styleToggle.normal.textColor = Color.black;
                //styleToggle.alignment = TextAnchor.MiddleCenter;

                styleRed = new GUIStyle(EditorStyles.label);
                styleRed.normal.textColor = new Color(188f / 255f, 56f / 255f, 56f / 255f);
                //styleRed.fontStyle = FontStyle.Bold;

                styleRichText = new GUIStyle(EditorStyles.label);
                styleRichText.richText = true;
                styleRichText.alignment = TextAnchor.UpperLeft;
                styleRichText.normal.textColor = Color.black;

                styleLabelRight = new GUIStyle(EditorStyles.label);
                styleLabelRight.alignment = TextAnchor.MiddleRight;
                styleLabelRight.normal.textColor = Color.black;

                styleLabelLeft = new GUIStyle(EditorStyles.label);
                styleLabelLeft.alignment = TextAnchor.MiddleLeft;
                styleLabelLeft.normal.textColor = Color.black;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            //Debug.Log("end init");

        }

        private void OnEnable()
        {
            buttonIconView = Resources.Load<Texture2D>("Textures/eye");
            buttonIconSave = Resources.Load<Texture2D>("Textures/Save_24x24");
            buttonIconFolders = Resources.Load<Texture2D>("Textures/Folders");
            buttonIconDelete = Resources.Load<Texture2D>("Textures/Delete_32x32");
            buttonIconHelp = Resources.Load<Texture2D>("Textures/question-mark");

        }
        /// <summary>
        /// Reload data
        /// </summary>
        private void OnFocus()
        {
            // Load description of available soundfont
            try
            {
                Init();

                MidiPlayerGlobal.InitPath();

                //Debug.Log(MidiPlayerGlobal.ImSFCurrent == null ? "ImSFCurrent is null" : "ImSFCurrent:" + MidiPlayerGlobal.ImSFCurrent.SoundFontName);
                //Debug.Log(MidiPlayerGlobal.CurrentMidiSet == null ? "CurrentMidiSet is null" : "CurrentMidiSet" + MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name);
                //Debug.Log(MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo == null ? "ActiveSounFontInfo is null" : MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name);
                ToolsEditor.LoadMidiSet();
                ToolsEditor.CheckMidiSet();
                // cause catch if call when playing (setup open on run mode)
                try
                {
                    if (!Application.isPlaying)
                        AssetDatabase.Refresh();
                }
                catch (Exception)
                {
                }
                // Exec after Refresh, either cause errror
                if (MidiPlayerGlobal.ImSFCurrent == null)
                    MidiPlayerGlobal.LoadCurrentSF();
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        void OnGUI()
        {
            try
            {
                if (window == null)
                    Init();
                if (LogInfo == null) LogInfo = new BuilderInfo();

                float startx = 5;
                float starty = 7;

                GUI.Box(new Rect(0, 0, window.position.width, window.position.height), "", styleWindow);

                GUIContent content = new GUIContent() { text = "Setup SoundFont - Version " + ToolsEditor.version, tooltip = "" };
                EditorGUI.LabelField(new Rect(startx, starty, 500, itemHeight), content, styleBold);

                content = new GUIContent() { text = "Doc & Contact", tooltip = "Get some help" };

                // Set position of the button
                Rect rect = new Rect(window.position.size.x - buttonLargeWidth - 5, starty, buttonLargeWidth, buttonHeight);
                if (GUI.Button(rect, content))
                    PopupWindow.Show(rect, new AboutMPTK());

                starty += buttonHeight + espace;

                widthRight = window.position.size.x - widthLeft - 2 * espace - startx;
                //widthRight = window.position.size.x / 2f - espace;
                //widthLeft = window.position.size.x / 2f - espace;

                heightLeftBottom = window.position.size.y - heightLeftTop - 3 * espace - starty;
                heightRightBottom = window.position.size.y - heightRightTop - 3 * espace - starty;

                // Display list of soundfont already loaded 
                ShowListSoundFonts(startx, starty, widthLeft, heightLeftTop);

                ShowListBanks(startx + widthLeft + espace, starty, widthRight, heightRightTop);

                ShowExtractOptim(startx + widthLeft + espace, starty + heightRightTop + espace, widthRight, heightRightBottom + espace);

                ShowLogOptim(startx, starty + espace + heightLeftTop, widthLeft, heightLeftBottom + espace);
            }
            catch (ExitGUIException) { }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }


        /// <summary>
        /// Display, add, remove Soundfont
        /// </summary>
        /// <param name="localstartX"></param>
        /// <param name="localstartY"></param>
        private void ShowListSoundFonts(float startX, float startY, float width, float height)
        {
            try
            {
                if (columnSF == null)
                {
                    columnSF = new ToolsEditor.DefineColumn[6];
                    columnSF[0].Width = 215; columnSF[0].Caption = "SoundFont Name"; columnSF[0].PositionCaption = 1f;
                    columnSF[1].Width = 40; columnSF[1].Caption = "Patch"; columnSF[1].PositionCaption = 6f;
                    columnSF[2].Width = 45; columnSF[2].Caption = "Wave"; columnSF[2].PositionCaption = 15f;
                    columnSF[3].Width = 60; columnSF[3].Caption = "Size"; columnSF[3].PositionCaption = 25f;
                    columnSF[4].Width = 85; columnSF[4].Caption = "SoundFont"; columnSF[4].PositionCaption = 5f;
                    columnSF[5].Width = 50; columnSF[5].Caption = "Remove"; columnSF[5].PositionCaption = -7f;
                }

                Rect zone = new Rect(startX, startY, width, height);
                //GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "", stylePanel);
                GUI.color = Color.white;
                float localstartX = 0;
                float localstartY = 0;
                GUIContent content;
                if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.SoundFonts != null && MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count > 0)
                    content = new GUIContent() { text = "SoundFont available", tooltip = "Each SoundFonts contains a set of bank of sound. \nOnly one SoundFont can be active at the same time for the midi player" };
                else
                {
                    content = new GUIContent() { text = "No SoundFont found, click on button 'Add SoundFont'", tooltip = "See the documentation here https://paxstellar.fr/" };
                    MidiPlayerGlobal.ImSFCurrent = null;
                }
                localstartX += xpostitlebox;
                localstartY += ypostitlebox;
                EditorGUI.LabelField(new Rect(startX + localstartX + 5, startY + localstartY, width, titleHeight), content, styleBold);
                localstartY += titleHeight;

#if MPTK_PRO
                if (GUI.Button(new Rect(startX + localstartX + espace, startY + localstartY, buttonLargeWidth, buttonHeight), "Add SoundFont"))
                {
                    if (Application.isPlaying)
                        EditorUtility.DisplayDialog("Add a SoundFont", "This action is not possible when application is running.", "Ok");
                    else
                    {
                        //if (EditorUtility.DisplayDialog("Import SoundFont", "This action could take time, do you confirm ?", "Ok", "Cancel"))
                        {
                            this.AddSoundFont();
                            scrollPosSoundFont = Vector2.zero;
                        }
                    }
                }
#else
                if (GUI.Button(new Rect(startX + localstartX + espace, startY + localstartY, buttonLargeWidth, buttonHeight), "Add a new SoundFont [PRO]"))
                    PopupWindow.Show(new Rect(startX + localstartX, startY + localstartY, buttonLargeWidth, buttonHeight), new GetFullVersion());
#endif
                if (GUI.Button(new Rect(startX + localstartX + width - 65, startY + localstartY - 18, 35, 35), buttonIconHelp))
                {
                    //CreateWave createwave = new CreateWave();
                    //string path = System.IO.Path.Combine(MidiPlayerGlobal.MPTK_PathToResources, "unitySample") + ".wav";
                    ////string path = "unitySample.wav";
                    //HiSample sample = new HiSample();
                    ////sample.LoopStart = sample.LoopEnd = 0;
                    //byte[] data = new byte[10000];
                    //for (int i = 0; i < data.Length; i++) data[i] = (byte)255;
                    //sample.SampleRate = 44100;
                    //sample.End = (uint)data.Length/2;
                    //createwave.Build(path, sample, data);
                    Application.OpenURL("https://paxstellar.fr/setup-mptk-add-soundfonts-v2/");
                }

                localstartY += buttonHeight + espace;

                // Draw title list box
                GUI.Box(new Rect(startX + localstartX + espace, startY + localstartY, width - 35, itemHeight), "", styleListTitle);
                float boxX = startX + localstartX + espace;
                foreach (ToolsEditor.DefineColumn column in columnSF)
                {
                    GUI.Label(new Rect(boxX + column.PositionCaption, startY + localstartY , column.Width, itemHeight), column.Caption, styleLabelLeft);
                    boxX += column.Width;
                }

                localstartY += itemHeight + espace;

                if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.SoundFonts != null && MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count > 0)
                {

                    Rect listVisibleRect = new Rect(startX + localstartX, startY + localstartY - 6, width - 10, height - localstartY);
                    Rect listContentRect = new Rect(0, 0, width - 25, MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count * itemHeight + 5);

                    scrollPosSoundFont = GUI.BeginScrollView(listVisibleRect, scrollPosSoundFont, listContentRect, false, true);
                    float boxY = 0;

                    // Loop on each soundfont
                    for (int i = 0; i < MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count; i++)
                    {
                        SoundFontInfo sf = MidiPlayerGlobal.CurrentMidiSet.SoundFonts[i];
                        bool selected = (MidiPlayerGlobal.ImSFCurrent != null && sf.Name == MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name);

                        // Draw a row
                        GUI.Box(new Rect(espace, boxY , width - 35, itemHeight), "", selected ? styleListRowSelected : styleListRow);

                        // Start content position (from the visible rect)
                        boxX = espace;

                        // col 1 - name
                        float colw = columnSF[0].Width;
                        EditorGUI.LabelField(new Rect(boxX + 1, boxY + 2, colw, itemHeight - 5), sf.Name, styleLabelLeft);
                        boxX += colw;

                        // col 2 - patch count
                        colw = columnSF[1].Width;
                        EditorGUI.LabelField(new Rect(boxX, boxY + 3, colw, itemHeight - 7), sf.PatchCount.ToString(), styleLabelRight);
                        boxX += colw;

                        // col 3 - wave count
                        colw = columnSF[2].Width;
                        EditorGUI.LabelField(new Rect(boxX, boxY + 3, colw, itemHeight - 7), sf.WaveCount.ToString(), styleLabelRight);
                        boxX += colw;

                        // col 4 - size
                        colw = columnSF[3].Width;
                        string sizew = (sf.WaveSize < 1000000) ?
                             Math.Round((double)sf.WaveSize / 1000d).ToString() + " Ko" :
                             Math.Round((double)sf.WaveSize / 1000000d).ToString() + " Mo";
                        EditorGUI.LabelField(new Rect(boxX, boxY + 3, colw, itemHeight - 7), sizew, styleLabelRight);
                        boxX += colw;

                        string textselect = "Select";
                        if (MidiPlayerGlobal.ImSFCurrent != null && sf.Name == MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name)
                            textselect = "Default";// GUI.color = ToolsEditor.ButtonColor;

                        // col 5 - select and remove buttons
                        colw = columnSF[4].Width;
                        boxX += 10;
                        if (GUI.Button(new Rect(boxX, boxY + 3, buttonMediumWidth, buttonHeight), textselect))
                        {
#if MPTK_PRO
                            this.SelectSf(i);
#else
                            PopupWindow.Show(new Rect(boxX, boxY + 3, buttonMediumWidth, buttonHeight), new GetFullVersion());
#endif
                        }
                        boxX += colw;

                        colw = columnSF[5].Width;
                        if (GUI.Button(new Rect(boxX, boxY + 3, 30, buttonHeight), new GUIContent(buttonIconDelete, "Remove SoundFont and samples associated")))
                        {
#if MPTK_PRO
                            if (Application.isPlaying)
                                EditorUtility.DisplayDialog("Remove a SoundFont", "This action is not possible when application is running.", "Ok");
                            else
                            {
                                if (this.DeleteSf(i))
                                    if (MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count > 0)
                                        this.SelectSf(0);
                                    else
                                        MidiPlayerGlobal.ImSFCurrent = null;
                            }
#else
                            PopupWindow.Show(new Rect(boxX, boxY + 3, 30, buttonHeight), new GetFullVersion());
#endif
                        }
                        boxX += colw;

                        boxY += itemHeight - 1;

                    }
                    GUI.EndScrollView();
                }
            }
            catch (ExitGUIException) { }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private void ShowListBanks(float startX, float startY, float width, float height)
        {
            try
            {
                if (columnBank == null)
                {
                    columnBank = new ToolsEditor.DefineColumn[5];
                    columnBank[0].Width = 150; columnBank[0].Caption = "Bank number"; columnBank[0].PositionCaption = 1f;
                    columnBank[1].Width = 60; columnBank[1].Caption = "View"; columnBank[1].PositionCaption = -3f;
                    columnBank[2].Width = 80; columnBank[2].Caption = "Keep"; columnBank[2].PositionCaption = -10f;
                    columnBank[3].Width = 77; columnBank[3].Caption = "Instrument"; columnBank[3].PositionCaption = -25f;
                    columnBank[4].Width = 77; columnBank[4].Caption = "Drum"; columnBank[4].PositionCaption = -6f;
                }

                Rect zone = new Rect(startX, startY, width, height);
                //GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "", stylePanel);
                //GUI.color = Color.white;
                float localstartX = 0;
                float localstartY = 0;
                if (MidiPlayerGlobal.ImSFCurrent != null && MidiPlayerGlobal.ImSFCurrent.Banks != null)
                {
                    GUIContent content = new GUIContent() { text = "Banks available in SoundFont " + MidiPlayerGlobal.ImSFCurrent.SoundFontName, tooltip = "Each bank contains a set of patchs (instrument).\nOnly two banks can be active at the same time : default sound (piano, ...) and drum kit (percussive)" };
                    localstartX += xpostitlebox;
                    localstartY += ypostitlebox;
                    EditorGUI.LabelField(new Rect(startX + localstartX + 5, startY + localstartY, width, titleHeight), content, styleBold);
                    localstartY += titleHeight;

                    // Save selection of banks
                    float btw = 25;
                    if (GUI.Button(new Rect(startX + localstartX + espace, startY + localstartY, btw, buttonHeight), new GUIContent(buttonIconSave, "Save banks configuration")))
                    {
#if MPTK_PRO
                        if (Application.isPlaying)
                            EditorUtility.DisplayDialog("Save Bank Configuration", "This action is not possible when application is running.", "Ok");
                        else
                            SaveBanksConfig();
#endif
                    }

                    btw = 75;
                    float buttonX = startX + localstartX + btw + 4 * espace;
                    EditorGUI.LabelField(new Rect(buttonX, startY + localstartY, btw, buttonHeight), "Keep banks:", styleLabelLeft);
                    buttonX += btw;

                    if (GUI.Button(new Rect(buttonX, startY + localstartY, btw, buttonHeight), new GUIContent("All", "Select all banks to be kept in the SoundFont")))
                    {
                        if (MidiPlayerGlobal.ImSFCurrent != null) MidiPlayerGlobal.ImSFCurrent.SelectAllBanks();
                    }
                    buttonX += btw + espace;

                    if (GUI.Button(new Rect(buttonX, startY + localstartY, btw, buttonHeight), new GUIContent("None", "Unselect all banks to be kept in the SoundFont")))
                    {
                        if (MidiPlayerGlobal.ImSFCurrent != null) MidiPlayerGlobal.ImSFCurrent.UnSelectAllBanks();
                    }
                    buttonX += btw + espace;

                    if (GUI.Button(new Rect(buttonX, startY + localstartY, btw, buttonHeight), new GUIContent("Inverse", "Inverse selection of banks to be kept in the SoundFont")))
                    {
                        if (MidiPlayerGlobal.ImSFCurrent != null) MidiPlayerGlobal.ImSFCurrent.InverseSelectedBanks();
                    }
                    buttonX += btw + espace;

                    localstartY += buttonHeight + espace;

                    // Draw title list box
                    GUI.Box(new Rect(startX + localstartX + espace, startY + localstartY, width - 35, itemHeight), "", styleListTitle);
                    float boxX = startX + localstartX + espace;
                    foreach (ToolsEditor.DefineColumn column in columnBank)
                    {
                        GUI.Label(new Rect(boxX + column.PositionCaption, startY + localstartY, column.Width, itemHeight), column.Caption, styleLabelLeft);
                        boxX += column.Width;
                    }

                    localstartY += itemHeight+ espace;

                    // Count available banks
                    int countBank = 0;
                    foreach (ImBank bank in MidiPlayerGlobal.ImSFCurrent.Banks)
                        if (bank != null) countBank++;
                    Rect listVisibleRect = new Rect(startX + localstartX, startY + localstartY - 6, width - 10, height - localstartY);
                    Rect listContentRect = new Rect(0, 0, width - 25, countBank * itemHeight + 5);

                    scrollPosBanks = GUI.BeginScrollView(listVisibleRect, scrollPosBanks, listContentRect, false, true);

                    float boxY = 0;
                    if (MidiPlayerGlobal.ImSFCurrent != null)
                    {
                        foreach (ImBank bank in MidiPlayerGlobal.ImSFCurrent.Banks)
                        {
                            if (bank != null)
                            {
                                GUI.Box(new Rect(5, boxY , width - 35, itemHeight), "", styleListRow);

                                GUI.color = Color.white;

                                // Start content position (from the visible rect)
                                boxX = espace;

                                // col 0 - bank and patch count
                                float colw = columnBank[0].Width;
                                GUI.Label(new Rect(boxX + 1, boxY , colw, itemHeight), string.Format("Bank [{0,3:000}] Patch:{1,4}", bank.BankNumber, bank.PatchCount), styleLabelLeft);
                                boxX += colw;

                                // col 1 - bt view list of patchs
                                colw = columnBank[1].Width;
                                Rect btrect = new Rect(boxX, boxY + 3, 30, buttonHeight);
                                if (GUI.Button(btrect, new GUIContent(buttonIconView, "See the detail of this bank")))
                                    PopupWindow.Show(btrect, new PopupListPatchs("Patch", false, bank.GetDescription()));
                                boxX += colw;

                                // col 2 - select bank to keep
                                colw = columnBank[2].Width;
                                Rect rect = new Rect(boxX, boxY + 4, colw, buttonHeight);
                                bool newSelect = GUI.Toggle(rect, MidiPlayerGlobal.ImSFCurrent.BankSelected[bank.BankNumber], new GUIContent("", "Keep or remove this bank"), styleToggle);
                                if (newSelect != MidiPlayerGlobal.ImSFCurrent.BankSelected[bank.BankNumber])
                                {
#if MPTK_PRO
                                    MidiPlayerGlobal.ImSFCurrent.BankSelected[bank.BankNumber] = newSelect;
#else
                                    PopupWindow.Show(rect, new GetFullVersion());
#endif
                                }
                                boxX += colw;

                                // col 3 - set default bank for instrument
                                colw = columnBank[3].Width;
                                bool curSelect = MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber == bank.BankNumber;
                                newSelect = GUI.Toggle(new Rect(boxX, boxY + 4, colw, buttonHeight), curSelect, new GUIContent("", "Select this bank as default for playing all instruments except drum"), styleToggle);
                                if (newSelect != curSelect)
                                    MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber = newSelect ? bank.BankNumber : -1;
                                boxX += btw + espace;

                                // col 4 - set default bank for Drum
                                colw = columnBank[4].Width;
                                curSelect = MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber == bank.BankNumber;
                                newSelect = GUI.Toggle(new Rect(boxX, boxY + 4, colw, buttonHeight), curSelect, new GUIContent("", "Select this bank as default for playing drum hit (Channel=9)"), styleToggle);
                                if (newSelect != curSelect)
                                    MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber = newSelect ? bank.BankNumber : -1;
                                boxX += btw + espace;

                                boxY += itemHeight - 1;
                            }
                        }
                    }

                    GUI.EndScrollView();
                }
                else
                    EditorGUI.LabelField(new Rect(startX + xpostitlebox, startY + ypostitlebox, 300, itemHeight), "No SoundFont selected", styleBold);
            }
            catch (ExitGUIException) { }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

#if MPTK_PRO
        private bool SaveBanksConfig()
        {
            string infocheck = this.CheckAndSetBank();
            if (string.IsNullOrEmpty(infocheck))
            {
                // Save MPTK SoundFont : xml only
                this.SaveCurrentIMSF(true);
                AssetDatabase.Refresh();
                return true;
            }
            else
                EditorUtility.DisplayDialog("Save Selected Bank", infocheck, "Ok");
            return false;
        }
#endif
        /// <summary>
        /// Display optimization
        /// </summary>
        /// <param name="localstartX"></param>
        /// <param name="localstartY"></param>
        private void ShowExtractOptim(float localstartX, float localstartY, float width, float height)
        {
            try
            {
                Rect zone = new Rect(localstartX, localstartY, width, height);
                //GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "", stylePanel);
                //GUI.color = Color.white;

                string tooltip = "Remove all banks and Presets not used in the Midi file list";

                GUIContent content;
                if (MidiPlayerGlobal.ImSFCurrent != null)
                {

                    float xpos = localstartX + xpostitlebox + 5;
                    float ypos = localstartY + ypostitlebox;
                    content = new GUIContent() { text = "Extract Patchs & Waves from " + MidiPlayerGlobal.ImSFCurrent.SoundFontName, tooltip = tooltip };
                    EditorGUI.LabelField(new Rect(xpos, ypos, 380 + 85, itemHeight), content, styleBold);
                    ypos += itemHeight;// + espace;

                    float widthCheck = buttonLargeWidth;
                    /*
                    KeepAllZones = GUI.Toggle(new Rect(xpos, ypos, widthCheck, itemHeight), KeepAllZones, new GUIContent("Keep all Zones", "Keep all Waves associated with a Patch regardless of notes and velocities played in Midi files.\n Usefull if you want transpose Midi files."));
                    xpos += widthCheck + espace;
                    KeepAllPatchs = GUI.Toggle(new Rect(xpos, ypos, widthCheck, itemHeight), KeepAllPatchs, new GUIContent("Keep all Patchs", "Keep all Patchs and waves found in the SoundFont selected.\nWarning : a huge volume of files coud be created"));
                    xpos += widthCheck + +2 * espace;
                    */
                    RemoveUnusedWaves = GUI.Toggle(new Rect(xpos, ypos, widthCheck, itemHeight), RemoveUnusedWaves, new GUIContent("Remove unused waves", "If check, keep only waves used by your midi files"), styleToggle);
                    //xpos += widthCheck + espace;
                    ypos += itemHeight;

                    LogDetailSoundFont = GUI.Toggle(new Rect(xpos, ypos, widthCheck, itemHeight), LogDetailSoundFont, new GUIContent("Log SoundFont Detail", "If check, keep only waves used by your midi files"), styleToggle);
                    ypos += itemHeight;

                    // restaure X positio,
                    xpos = localstartX + xpostitlebox + 5;
                    Rect rect1 = new Rect(xpos, ypos, 210, (float)buttonHeight * 2f);
                    Rect rect2 = new Rect(xpos + 210 + 3, ypos, 210, (float)buttonHeight * 2f);
#if MPTK_PRO
                    if (GUI.Button(rect1, new GUIContent("Optimize from Midi file list", "Your list of Midi files will be scanned to identify patchs and zones useful")))
                    {
                        if (Application.isPlaying)
                            EditorUtility.DisplayDialog("Optimization", "This action is not possible when application is running.", "Ok");
                        else
                        {
                            if (SaveBanksConfig())
                            {
                                KeepAllPatchs = false;
                                KeepAllZones = false;
                                this.OptimizeSoundFont();// LogInfo, KeepAllPatchs, KeepAllZones, RemoveUnusedWaves);
                            }
                        }
                    }

                    if (GUI.Button(rect2, new GUIContent("Extract all Patchs & Waves", "All patchs and waves will be extracted from the Soundfile")))
                    {
                        if (Application.isPlaying)
                            EditorUtility.DisplayDialog("Extraction", "This action is not possible when application is running.", "Ok");
                        else
                        {
                            if (SaveBanksConfig())
                            {
                                KeepAllPatchs = true;
                                KeepAllZones = true;
                                this.OptimizeSoundFont();// (LogInfo, KeepAllPatchs, KeepAllZones, RemoveUnusedWaves);
                            }
                        }
                    }
#else
                    if (GUI.Button(rect1, new GUIContent("Optimize from Midi file list [PRO]", "You need to setup some midi files before to launch ths optimization")))
                        PopupWindow.Show(rect1, new GetFullVersion());
                    if (GUI.Button(rect2, new GUIContent("Extract all Patchs & Waves [PRO]", "")))
                        PopupWindow.Show(rect2, new GetFullVersion());

#endif
                }
                else
                {
                    content = new GUIContent() { text = "No SoundFont selected", tooltip = tooltip };
                    EditorGUI.LabelField(new Rect(localstartX + xpostitlebox, localstartY + ypostitlebox, 300, itemHeight), content, styleBold);
                }
            }
            catch (ExitGUIException) { }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Display optimization log
        /// </summary>
        /// <param name="localstartX"></param>
        /// <param name="localstartY"></param>
        private void ShowLogOptim(float localstartX, float localstartY, float width, float height)
        {
            try
            {
                Rect zone = new Rect(localstartX, localstartY, width, height);
                //GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "", stylePanel);
                //GUI.color = Color.white;
                float posx = localstartX;
                GUI.Label(new Rect(posx + espace, localstartY + espace, 40, buttonHeight), new GUIContent("Logs:"), styleBold);
                posx += 40;
                float btw = 25f;
                if (GUI.Button(new Rect(posx + espace, localstartY + espace, btw, buttonHeight), new GUIContent(buttonIconSave, "Save Log")))
                {
                    // Save log file
                    if (LogInfo != null)
                    {
                        string filenamelog = string.Format("SoundFontSetupLog {0} {1} .txt", MidiPlayerGlobal.ImSFCurrent.SoundFontName, DateTime.UtcNow.ToString("yyyy-MM-dd HH-mm-ss"));
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.IO.Path.Combine(Application.persistentDataPath, filenamelog)))
                            foreach (string line in LogInfo.Infos)
                                file.WriteLine(line);
                    }
                }

                if (GUI.Button(new Rect(posx + 2f * espace + btw, localstartY + espace, btw, buttonHeight), new GUIContent(buttonIconFolders, "Open Logs Folder")))
                {
                    Application.OpenURL(Application.persistentDataPath);
                }

                if (GUI.Button(new Rect(posx + 3f * espace + 2f * btw, localstartY + espace, btw, buttonHeight), new GUIContent(buttonIconDelete, "Clear Logs")))
                {
                    LogInfo = new BuilderInfo();
                }
#if MPTK_PRO
                bool displayInfo = MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo != null && MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.PatchCount == 0;
                float heightLine = styleRichText.lineHeight * 1.2f;
                int countLine = (LogInfo != null ? LogInfo.Count : 0) + (displayInfo ? 7 : 0);
                if (countLine > 0)
                {
                    Rect listVisibleRect = new Rect(espace, localstartY + buttonHeight + espace, width - 5, height - 25);
                    Rect listContentRect = new Rect(0, 0, 2 * width, (countLine + 3) * heightLine + 5);

                    scrollPosOptim = GUI.BeginScrollView(listVisibleRect, scrollPosOptim, listContentRect);
                    //Debug.Log(scrollPosOptim);
                    GUI.color = Color.white;
                    float labelY = -heightLine;
                    if (LogInfo != null)
                        foreach (string s in LogInfo.Infos)
                            EditorGUI.LabelField(new Rect(localstartX, labelY += heightLine, width * 2, heightLine), s, styleRichText);

                    if (displayInfo)
                    {
                        float xpos = 0;
                        float ypos = labelY + 2 * heightLine;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 450, itemHeight), "No Patchs and Samples has been yet extracted from the Soundfont.", styleRed); ypos += itemHeight / 1f;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 450, itemHeight), "On the right panel:", styleRed); ypos += itemHeight / 2f;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 450, itemHeight), "   Select banks you want to keep.", styleRed); ypos += itemHeight / 2f;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 450, itemHeight), "   Select default bank for instruments and drums kit.", styleRed); ypos += itemHeight / 1f;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 450, itemHeight), "Click on button:", styleRed); ypos += itemHeight / 2f;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 450, itemHeight), "   'Optimize from Midi file list' to keep only patchs required or", styleRed); ypos += itemHeight / 2f;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 450, itemHeight), "   'Extract all Patchs & Samples' to keep all patchs of selected banks.", styleRed); ypos += itemHeight;
                        scrollPosOptim = new Vector2(0, 1000);
                    }

                    GUI.EndScrollView();
                }
#endif
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}