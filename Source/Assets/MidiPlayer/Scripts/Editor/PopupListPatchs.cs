
using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

namespace MidiPlayerTK
{
    public class PopupListPatchs : PopupWindowContent
    {
        static public CustomStyle MyStyle;
        public int Selected;

        private Vector2 scroller;
        private List<string> Data;
        private GUIContent Content;
        private bool Selectable;

        private int winWidth = 300;
        private int winHeight = 175;
        private GUIStyle CellStyle;
        private GUIStyle TitleStyle;
        public override Vector2 GetWindowSize()
        {
            return new Vector2(winWidth, winHeight);
        }

        public PopupListPatchs(string title, bool pselectable, List<string> data)
        {
            Content = new GUIContent(title);
            Selectable = pselectable;
            if (MyStyle == null) MyStyle = new CustomStyle();
            Data = data;
            CellStyle = MyStyle.LabelList;
            TitleStyle = MyStyle.LabelTitle;
            //winHeight =(int)( Data.Count * CellStyle.CalcHeight(Content,300f)+ TitleStyle.CalcHeight(Content, 300f));
            winHeight = (int)((Data.Count + 2) * CellStyle.lineHeight + TitleStyle.lineHeight);
        }

        public override void OnGUI(Rect rect)
        {
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(Content, TitleStyle);
                if (GUILayout.Button("Close", GUILayout.Width(50), GUILayout.Height(20)))
                    editorWindow.Close();
                GUILayout.EndHorizontal();

                scroller = GUILayout.BeginScrollView(scroller, false, false);
                for (int index = 0; index < Data.Count; index++)
                {
                    if (Selectable)
                    {
                        GUIStyle style = MyStyle.BtStandard;
                        if (Selected == index) style = MyStyle.BtSelected;
                        if (GUILayout.Button(Data[index], style))
                        {
                            Selected = index;
                            editorWindow.Close();
                        }
                    }
                    else
                    {
                        GUILayout.Label(Data[index], CellStyle);
                    }
                }
                GUILayout.EndScrollView();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}