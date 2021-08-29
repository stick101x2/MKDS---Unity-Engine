using System;
using System.Collections.Generic;
using System.IO;
using MPTK.NAudio.Midi;
namespace MidiPlayerTK
{
    //using MonoProjectOptim;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Window editor for the setup of MPTK
    /// </summary>
    public class MidiFileSetupWindow : EditorWindow
    {

        private static MidiFileSetupWindow window;

        static Vector2 scrollPosMidiFile = Vector2.zero;
        static Vector2 scrollPosAnalyze = Vector2.zero;

        static float widthLeft = 500;
        static float widthRight;

        static float heightList;
        static float titleHeight = 18; //label title above list

        static int itemHeight = 25;
        static int buttonWidth = 150;
        static int buttonShortWidth = 50;
        static float buttonLargeWidth = 170;
        static int buttonHeight = 18;
        static float espace = 5;

        static float xpostitlebox = 2;
        static float ypostitlebox = 5;

        static string midifile;

        static GUIStyle styleWindow;
        static GUIStyle stylePanel;
        static GUIStyle styleBold;
        static GUIStyle styleRed;
        static GUIStyle styleRichText;
        static GUIStyle styleRichTextBorder;
        static GUIStyle styleLabelUpperLeft;
        static GUIStyle styleLabelLeft;
        static GUIStyle styleLabelRight;
        static GUIStyle styleLabelCenter;
        static GUIStyle styleMiniButton;
        static GUIStyle styleListTitle;
        static GUIStyle styleListRow;
        static GUIStyle styleListRowSelected;
        static GUIStyle styleListLabel;
        static GUIStyle styleToggle;

        static public CustomStyle myStyle;
        static private List<string> infoEvents;
        static public int PageToDisplay = 0;
        const int MAXLINEPAGE = 100;
        static bool withMeta = false, withNoteOn = false, withNoteOff = false, withPatchChange = false;
        static bool withControlChange = false, withAfterTouch = false, withOthers = false;

        static int IndexEditItem;

        //private Texture buttonIconView;
        private Texture buttonIconHelp;
        private Texture buttonIconDelete;

        // % (ctrl on Windows, cmd on macOS), # (shift), & (alt).
        [MenuItem("MPTK/Midi File Setup &M", false, 10)]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            try
            {
                window = GetWindow<MidiFileSetupWindow>(true, "Midi File Setup");
                window.minSize = new Vector2(828, 400);

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
                styleToggle.hover.textColor = Color.black;
                styleToggle.active.textColor = Color.black;
                styleToggle.focused.textColor = Color.black;
                //styleToggle.alignment = TextAnchor.MiddleCenter;

                styleRed = new GUIStyle(EditorStyles.label);
                styleRed.normal.textColor = new Color(188f / 255f, 56f / 255f, 56f / 255f);
                //styleRed.fontStyle = FontStyle.Bold;

                styleRichText = new GUIStyle(EditorStyles.label);
                styleRichText.richText = true;
                styleRichText.alignment = TextAnchor.UpperLeft;
                styleRichText.normal.textColor = Color.black;

                styleRichTextBorder = new GUIStyle(EditorStyles.label);
                styleRichTextBorder.richText = true;
                styleRichTextBorder.alignment = TextAnchor.MiddleLeft;
                styleRichTextBorder.normal.textColor = Color.black;
                styleRichTextBorder.normal.background = ToolsEditor.MakeTex(500, 600, new Color(gray5, gray5, gray5, 1f), rectBorder, new Color(gray2, gray2, gray2, 1f));

                styleLabelRight = new GUIStyle(EditorStyles.label);
                styleLabelRight.alignment = TextAnchor.MiddleRight;
                styleLabelRight.normal.textColor = Color.black;

                styleLabelCenter = new GUIStyle(EditorStyles.label);
                styleLabelCenter.alignment = TextAnchor.MiddleCenter;
                styleLabelCenter.normal.textColor = Color.black;

                styleLabelLeft = new GUIStyle(EditorStyles.label);
                styleLabelLeft.alignment = TextAnchor.MiddleLeft;
                styleLabelLeft.normal.textColor = Color.black;

                styleLabelUpperLeft = new GUIStyle(EditorStyles.label);
                styleLabelUpperLeft.alignment = TextAnchor.UpperLeft;
                styleLabelUpperLeft.normal.textColor = Color.black;
                styleLabelUpperLeft.hover.textColor = Color.black;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private void OnEnable()
        {
            //buttonIconView = Resources.Load<Texture2D>("Textures/eye");
            buttonIconDelete = Resources.Load<Texture2D>("Textures/Delete_32x32");
            buttonIconHelp = Resources.Load<Texture2D>("Textures/question-mark");
        }

        private void OnLostFocus()
        {
#if UNITY_2017_1_OR_NEWER
            // Trig an  error before v2017...
            if (Application.isPlaying)
            {
                window.Close();
            }
#endif
        }
        private void OnFocus()
        {
            // Load description of available soundfont
            try
            {
                //Debug.Log("OnFocus");
                Init();
                MidiPlayerGlobal.InitPath();
                ToolsEditor.LoadMidiSet();
                ToolsEditor.CheckMidiSet();
                AssetDatabase.Refresh();
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
                if (window == null) Init();
                float startx = 5;
                float starty = 7;
                //Log.Write("test");

                //if (myStyle == null)                    myStyle = new CustomStyle();
                GUI.Box(new Rect(0, 0, window.position.width, window.position.height), "", styleWindow);

                GUIContent content = new GUIContent() { text = "Setup Midi files - Version " + ToolsEditor.version, tooltip = "" };
                EditorGUI.LabelField(new Rect(startx, starty, 500, itemHeight), content, styleBold);

                content = new GUIContent() { text = "Doc & Contact", tooltip = "Get some help" };
                Rect rect = new Rect(window.position.size.x - buttonWidth - 5, starty, buttonWidth, buttonHeight);
                if (GUI.Button(rect, content))
                    PopupWindow.Show(rect, new AboutMPTK());

                starty += buttonHeight + espace;

                widthRight = window.position.size.x - widthLeft - 2 * espace - startx;
                heightList = window.position.size.y - 3 * espace - starty;

                ShowListMidiFiles(startx, starty, widthLeft, heightList);
                ShowMidiAnalyse(startx + widthLeft + espace, starty, widthRight, heightList);

            }
            catch (ExitGUIException) { }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        ToolsEditor.DefineColumn[] columnSF;
        private static Rect listMidiVisibleRect;

        /// <summary>
        /// Display, add, remove Midi file
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        private void ShowListMidiFiles(float startX, float startY, float width, float height)
        {
            try
            {
                Event e = Event.current;
                if (e.type == EventType.KeyDown)
                {
                    //Debug.Log("Ev.KeyDown: " + e);
                    if (e.keyCode == KeyCode.DownArrow || e.keyCode == KeyCode.UpArrow || e.keyCode == KeyCode.End || e.keyCode == KeyCode.Home)
                    {
                        if (e.keyCode == KeyCode.End)
                            IndexEditItem = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count - 1;

                        if (e.keyCode == KeyCode.Home)
                            IndexEditItem = 0;

                        if (e.keyCode == KeyCode.DownArrow)
                        {
                            IndexEditItem++;
                            if (IndexEditItem >= MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count)
                                IndexEditItem = 0;
                        }

                        if (e.keyCode == KeyCode.UpArrow)
                        {
                            IndexEditItem--;
                            if (IndexEditItem < 0)
                                IndexEditItem = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count - 1;
                        }

                        SetMidiSelectedVisible();
                        ReadEvents();
                        GUI.changed = true;
                        Repaint();
                    }
                }
                if (columnSF == null)
                {
                    columnSF = new ToolsEditor.DefineColumn[2];
                    columnSF[0].Width = 60; columnSF[0].Caption = "Index"; columnSF[0].PositionCaption = 10f;
                    columnSF[1].Width = 300; columnSF[1].Caption = "Midi Name"; columnSF[1].PositionCaption = 5f;
                    //columnSF[2].Width = 70; columnSF[2].Caption = "Read"; columnSF[2].PositionCaption = 0f;
                    //columnSF[3].Width = 60; columnSF[3].Caption = "Remove"; columnSF[3].PositionCaption = -9f;
                }

                GUI.Box(new Rect(startX, startY, width, height), "", stylePanel);

                float localstartX = 0;
                float localstartY = 0;

                GUIContent content = new GUIContent()
                {
                    text = MidiPlayerGlobal.CurrentMidiSet.MidiFiles == null || MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count == 0 ?
                                        "No Midi file available" : "Midi files available",
                    tooltip = ""
                };

                localstartX += xpostitlebox;
                localstartY += ypostitlebox;
                GUI.Label(new Rect(startX + localstartX + 5, startY + localstartY, 160, titleHeight), content, styleBold);

                string searchMidi = EditorGUI.TextField(new Rect(startX + localstartX + 5 + 170+ espace, startY + localstartY-2, 225, titleHeight), "Search in list:");
                if (!string.IsNullOrEmpty(searchMidi))
                {
                    int index = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s.ToLower().Contains(searchMidi.ToLower()));
                    if (index>=0)
                    {
                        IndexEditItem = index;
                        SetMidiSelectedVisible();
                        ReadEvents();
                    }

                }

                localstartY += titleHeight;


                if (GUI.Button(new Rect(startX + localstartX + width - 65, startY + localstartY - 18, 35, 35), buttonIconHelp))
                    Application.OpenURL("https://paxstellar.fr/setup-mptk-add-midi-files-v2/");

                if (GUI.Button(new Rect(startX + localstartX + espace, startY + localstartY, buttonLargeWidth, buttonHeight),
                                "Add a Midi File"))
                    AddMidifile();

                if (GUI.Button(new Rect(startX + localstartX + 2 * espace + buttonLargeWidth, startY + localstartY, buttonLargeWidth, buttonHeight),
                                "Add All Midi from a Folder"))
                    AddMidiFromFolder();

                if (IndexEditItem >= 0 && IndexEditItem < MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count)
                    if (GUI.Button(new Rect(startX + localstartX + 7 * espace + 2 * buttonLargeWidth, startY + localstartY, 30, buttonHeight),
                        new GUIContent(buttonIconDelete, $"Remove {MidiPlayerGlobal.CurrentMidiSet.MidiFiles[IndexEditItem]}")))
                    {
                        if (EditorUtility.DisplayDialog(
                            "Remove Midi File",
                            $"Remove {MidiPlayerGlobal.CurrentMidiSet.MidiFiles[IndexEditItem]} ?",
                            "ok", "cancel"))
                        {
                            DeleteResource(MidiLoad.BuildOSPath(MidiPlayerGlobal.CurrentMidiSet.MidiFiles[IndexEditItem]));
                            AssetDatabase.Refresh();
                            ToolsEditor.LoadMidiSet();
                            ToolsEditor.CheckMidiSet();
                            AssetDatabase.Refresh();
                        }
                    }

                localstartY += buttonHeight + espace;

                // Draw title list box
                GUI.Box(new Rect(startX + localstartX + espace, startY + localstartY, width - 35, itemHeight), "", styleListTitle);
                float boxX = startX + localstartX + espace;
                foreach (ToolsEditor.DefineColumn column in columnSF)
                {
                    GUI.Label(new Rect(boxX + column.PositionCaption, startY + localstartY, column.Width, itemHeight), column.Caption, styleLabelLeft);
                    boxX += column.Width;
                }

                localstartY += itemHeight + espace;

                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null)
                {
                    listMidiVisibleRect = new Rect(startX + localstartX, startY + localstartY - 6, width - 10, height - localstartY);
                    Rect listMidiContentRect = new Rect(0, 0, width - 25, MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count * itemHeight + 5);

                    scrollPosMidiFile = GUI.BeginScrollView(listMidiVisibleRect, scrollPosMidiFile, listMidiContentRect, false, true);
                    //Debug.Log($"scrollPosMidiFile:{scrollPosMidiFile.y} listVisibleRect:{listMidiVisibleRect.height} listContentRect:{listMidiContentRect.height}");
                    float boxY = 0;

                    // Loop on each midi
                    // -----------------
                    for (int i = 0; i < MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count; i++)
                    {
                        boxX = 5;

                        if (GUI.Button(new Rect(espace, boxY, width - 35, itemHeight), "", IndexEditItem == i ? styleListRowSelected : styleListRow))
                        {
                            IndexEditItem = i;
                            ReadEvents();
                        }

                        // col 0 - Index
                        float colw = columnSF[0].Width;
                        EditorGUI.LabelField(new Rect(boxX + 1, boxY + 2, colw, itemHeight - 5), i.ToString(), styleLabelCenter);
                        boxX += colw;

                        // col 1 - Name
                        colw = columnSF[1].Width;
                        content = new GUIContent() { text = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[i], tooltip = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[i] };
                        EditorGUI.LabelField(new Rect(boxX + 5, boxY + 2, colw, itemHeight - 5), content, styleLabelLeft);
                        boxX += colw;

                        // col 2 - Select
                        //colw = columnSF[2].Width;
                        //if (GUI.Button(new Rect(boxX, boxY + 3, 30, buttonHeight), new GUIContent(buttonIconView, "Read Midi events")))
                        //{
                        //    IndexEditItem = i;
                        //    ReadEvents();
                        //}
                        //boxX += colw;

                        // col 3 - remove
                        //colw = columnSF[3].Width;
                        //if (GUI.Button(new Rect(boxX, boxY + 3, 30, buttonHeight), new GUIContent(buttonIconDelete, "Remove Midi File")))
                        //{
                        //    DeleteResource(MidiLoad.BuildOSPath(MidiPlayerGlobal.CurrentMidiSet.MidiFiles[i]));
                        //    AssetDatabase.Refresh();
                        //    ToolsEditor.LoadMidiSet();
                        //    ToolsEditor.CheckMidiSet();
                        //    AssetDatabase.Refresh();
                        //}
                        //boxX += colw;

                        boxY += itemHeight - 1;
                    }
                    GUI.EndScrollView();
                }
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Display analyse of midifile
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        private void ShowMidiAnalyse(float startX, float startY, float width, float height)
        {
            try
            {
                GUI.Box(new Rect(startX, startY, width, height), "", stylePanel);

                if (infoEvents != null && infoEvents.Count > 0)
                {
                    float posx = startX + espace;
                    float posy = startY + espace;
                    int toggleLargeWidth = 70;
                    int toggleSmallWidth = 55;

                    if (GUI.Button(new Rect(posx, posy, buttonShortWidth, buttonHeight), "All"))
                    {
                        withMeta = withNoteOn = withNoteOff = withControlChange = withPatchChange = withAfterTouch = withOthers = true;
                        ReadEvents();
                    }
                    posx += buttonShortWidth + espace;

                    if (GUI.Button(new Rect(posx, posy, buttonShortWidth, buttonHeight), "None"))
                    {
                        withMeta = withNoteOn = withNoteOff = withControlChange = withPatchChange = withAfterTouch = withOthers = false;
                        ReadEvents();
                    }
                    posx += buttonShortWidth + espace;

                    bool filter = GUI.Toggle(new Rect(posx, posy, toggleSmallWidth, buttonHeight), withMeta, "Meta", styleToggle);
                    if (filter != withMeta)
                    {
                        withMeta = filter;
                        ReadEvents();
                    }

                    posx += toggleSmallWidth + espace;

                    filter = GUI.Toggle(new Rect(posx, posy, toggleLargeWidth, buttonHeight), withNoteOn, "Note On", styleToggle);
                    if (filter != withNoteOn)
                    {
                        withNoteOn = filter;
                        ReadEvents();
                    }
                    posx += toggleLargeWidth + espace;

                    filter = GUI.Toggle(new Rect(posx, posy, toggleLargeWidth, buttonHeight), withNoteOff, "Note Off", styleToggle);
                    if (filter != withNoteOff)
                    {
                        withNoteOff = filter;
                        ReadEvents();
                    }
                    posx += toggleLargeWidth + espace;

                    filter = GUI.Toggle(new Rect(posx, posy, toggleLargeWidth, buttonHeight), withControlChange, "Control", styleToggle);
                    if (filter != withControlChange)
                    {
                        withControlChange = filter;
                        ReadEvents();
                    }
                    posx += toggleLargeWidth + espace;

                    filter = GUI.Toggle(new Rect(posx, posy, toggleSmallWidth, buttonHeight), withPatchChange, "Patch", styleToggle);
                    if (filter != withPatchChange)
                    {
                        withPatchChange = filter;
                        ReadEvents();
                    }
                    posx += toggleSmallWidth + espace;

                    filter = GUI.Toggle(new Rect(posx, posy, toggleSmallWidth, buttonHeight), withAfterTouch, "Touch", styleToggle);
                    if (filter != withAfterTouch)
                    {
                        withAfterTouch = filter;
                        ReadEvents();
                    }
                    posx += toggleSmallWidth + espace;

                    filter = GUI.Toggle(new Rect(posx, posy, toggleSmallWidth, buttonHeight), withOthers, "Others", styleToggle);
                    if (filter != withOthers)
                    {
                        withOthers = filter;
                        ReadEvents();
                    }
                    posx += toggleSmallWidth + espace;

                    if (PageToDisplay < 0) PageToDisplay = 0;
                    if (PageToDisplay * MAXLINEPAGE > infoEvents.Count) PageToDisplay = infoEvents.Count / MAXLINEPAGE;

                    string infoToDisplay = "";
                    for (int i = PageToDisplay * MAXLINEPAGE; i < (PageToDisplay + 1) * MAXLINEPAGE; i++)
                        if (i < infoEvents.Count)
                        {
                            if (i < 11)
                                // The ten first lines are global info on the midi, not events
                                infoToDisplay += infoEvents[i] + "\n";
                            else
                                infoToDisplay += /*(i - 10).ToString() + " " +*/ infoEvents[i] + "\n";
                        }

                    posx = startX + espace;
                    posy = startY + espace + buttonHeight + espace;

                    if (GUI.Button(new Rect(posx, posy, buttonShortWidth, buttonHeight), "<<")) PageToDisplay = 0;

                    posx += buttonShortWidth + espace;
                    if (GUI.Button(new Rect(posx, posy, buttonShortWidth, buttonHeight), "<")) PageToDisplay--;

                    posx += buttonShortWidth + espace;
                    GUI.Label(new Rect(posx, posy, buttonWidth / 2, buttonHeight),
                        "Page " + (PageToDisplay + 1).ToString() + " / " + (infoEvents.Count / MAXLINEPAGE + 1).ToString(), styleLabelLeft);

                    posx += buttonWidth / 2 + espace;
                    if (GUI.Button(new Rect(posx, posy, buttonShortWidth, buttonHeight), ">")) PageToDisplay++;

                    posx += buttonShortWidth + espace;
                    if (GUI.Button(new Rect(posx, posy, buttonShortWidth, buttonHeight), ">>")) PageToDisplay = infoEvents.Count / MAXLINEPAGE;

                    float wList = widthRight - 6 * espace;
                    Rect listVisibleRect = new Rect(
                        startX + espace,
                        startY + 2 * buttonHeight + 3 * espace,
                        wList + 4 * espace,
                        heightList - 2 * buttonHeight - 4 * espace);

                    Rect listContentRect = new Rect(
                        0,
                        0,
                        wList,
                        MAXLINEPAGE * styleRichTextBorder.lineHeight + espace);

                    Rect fondRect = new Rect(
                        startX + espace,
                        startY + 2 * buttonHeight + 3 * espace,
                        wList - 0 * espace,
                        heightList - 2 * buttonHeight - 4 * espace);

                    GUI.Box(fondRect, "", stylePanel);

                    scrollPosAnalyze = GUI.BeginScrollView(listVisibleRect, scrollPosAnalyze, listContentRect);
                    GUI.Box(listContentRect, infoToDisplay, styleLabelUpperLeft);

                    GUI.EndScrollView();
                }
                else
                {
                    GUIContent content = new GUIContent() { text = "No Midi file analysed", tooltip = "" };
                    EditorGUI.LabelField(new Rect(startX + xpostitlebox, startY + ypostitlebox, 300, itemHeight), content, styleBold);
                }
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        static private void ReadEvents()
        {
            midifile = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[IndexEditItem];
            infoEvents = MidiScan.GeneralInfo(midifile, withNoteOn, withNoteOff, withControlChange, withPatchChange, withAfterTouch, withMeta, withOthers);
            infoEvents.Insert(0, "Open midi file: " + midifile);
            infoEvents.Insert(0, "DB Midi index: " + IndexEditItem);
            PageToDisplay = 0;
            scrollPosAnalyze = Vector2.zero;
        }


        /// <summary>
        /// Add a new Midi file from desktop
        /// </summary>
        private static void AddMidifile()
        {
            try
            {
                string selectedFile = EditorUtility.OpenFilePanelWithFilters("Open and import Midi file", ToolsEditor.lastDirectoryMidi, new string[] { "Midi files", "mid,midi", "Karoke files", "kar", "All", "*" });
                if (!string.IsNullOrEmpty(selectedFile))
                {
                    // selectedFile contins also the folder 
                    ToolsEditor.lastDirectoryMidi = Path.GetDirectoryName(selectedFile);
                    InsertMidiFIle(selectedFile);
                }
                AssetDatabase.Refresh();
                ToolsEditor.LoadMidiSet();
                ToolsEditor.CheckMidiSet();
                AssetDatabase.Refresh();
                ReadEvents();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }


        /// <summary>
        /// Add Midi files from a folder
        /// </summary>
        private static void AddMidiFromFolder()
        {
            try
            {
                string selectedFolder = EditorUtility.OpenFolderPanel("Import Midi from a folder", ToolsEditor.lastDirectoryMidi, "");
                if (!string.IsNullOrEmpty(selectedFolder))
                {
                    ToolsEditor.lastDirectoryMidi = Path.GetDirectoryName(selectedFolder);
                    string[] files = Directory.GetFiles(selectedFolder);
                    foreach (string file in files)
                        if (file.EndsWith(".mid") || file.EndsWith(".midi"))
                            InsertMidiFIle(file);
                }
                AssetDatabase.Refresh();
                ToolsEditor.LoadMidiSet();
                ToolsEditor.CheckMidiSet();
                AssetDatabase.Refresh();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private static void InsertMidiFIle(string selectedFile)
        {
            // Build path to midi folder 
            string pathMidiFile = Path.Combine(Application.dataPath, MidiPlayerGlobal.PathToMidiFile);

            if (!Directory.Exists(pathMidiFile))
                Directory.CreateDirectory(pathMidiFile);

            MidiLoad midifile;
            try
            {
                midifile = new MidiLoad();
                if (!midifile.MPTK_LoadFile(selectedFile))
                {
                    EditorUtility.DisplayDialog("Midi Not Loaded", "Try to open " + selectedFile + "\nbut this file seems not a valid midi file", "ok");
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat("{0} {1}", selectedFile, ex.Message);
                return;
            }

            string filename = Path.GetFileNameWithoutExtension(selectedFile);
            //foreach (char c in filename) Debug.Log(string.Format("{0} {1}", c, (int)c));
            foreach (char i in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(i, '_');
            }
            string filenameToSave = Path.Combine(pathMidiFile, filename + MidiPlayerGlobal.ExtensionMidiFile);

            filenameToSave = filenameToSave.Replace('(', '_');
            filenameToSave = filenameToSave.Replace(')', '_');
            filenameToSave = filenameToSave.Replace('#', '_');
            filenameToSave = filenameToSave.Replace('$', '_');

            // Create a copy of the midi file in MPTK resources
            File.Copy(selectedFile, filenameToSave, true);

            if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles == null)
                MidiPlayerGlobal.CurrentMidiSet.MidiFiles = new List<string>();

            // Add midi file to the list
            string midiname = Path.GetFileNameWithoutExtension(selectedFile);
            if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == midiname) < 0)
            {
                MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Add(midiname);
                MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Sort();
                MidiPlayerGlobal.CurrentMidiSet.Save();
            }
            IndexEditItem = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == midiname);

            SetMidiSelectedVisible();

            Debug.Log($"Midi file '{midiname}' added with success, " +
                      $"Index: {IndexEditItem}, " +
                      $"Duration: {midifile.MPTK_DurationMS / 1000f} second, " +
                      $"Track count:{midifile.MPTK_TrackCount}, " +
                      $"Initial Tempo:{midifile.MPTK_InitialTempo}"
                      );
        }

        private static void SetMidiSelectedVisible()
        {
            if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
            {
                float contentHeight = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count * itemHeight;
                scrollPosMidiFile.y = contentHeight * ((float)IndexEditItem / (float)MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count) - listMidiVisibleRect.height / 2f;
            }
        }

        static private void DeleteResource(string filepath)
        {
            try
            {
                Debug.Log("Delete " + filepath);
                File.Delete(filepath);
                // delete also meta
                string meta = filepath + ".meta";
                Debug.Log("Delete " + meta);
                File.Delete(meta);

            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }

}