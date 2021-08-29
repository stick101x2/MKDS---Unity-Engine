﻿using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

namespace MidiPlayerTK
{
    public class AboutMPTK : PopupWindowContent
    {

        private int winWidth = 365;
        private int winHeight = 175;
        public override Vector2 GetWindowSize()
        {
            return new Vector2(winWidth, winHeight);
        }

        public override void OnGUI(Rect rect)
        {
            try
            {
                float xCol0 = 5;
                float xCol1 = 20;
                float xCol2 = 120;
                float yStart = 5;
                float ySpace = 18;
                float colWidth = 230;
                float colHeight = 17;

                GUIStyle style = new GUIStyle("Label");
                style.fontSize = 16;
                style.fontStyle = FontStyle.Bold;

                try
                {
                    int sizePicture = 90;
                    Texture aTexture = Resources.Load<Texture>("Logo_MPTK");
                    EditorGUI.DrawPreviewTexture(new Rect(winWidth - sizePicture - 5, yStart, sizePicture, sizePicture), aTexture);
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
                GUIContent cont = new GUIContent("Midi Player ToolKit (MPTK)");
                EditorGUI.LabelField(new Rect(xCol0, yStart, 300, 30), cont, style);
                EditorGUI.LabelField(new Rect(xCol0, yStart + 8, 300, colHeight), "_________________________________");

                yStart += 15;
                EditorGUI.LabelField(new Rect(xCol1, yStart += ySpace, colWidth, colHeight), "Version:");
                EditorGUI.LabelField(new Rect(xCol2, yStart, colWidth, colHeight), ToolsEditor.version);

                EditorGUI.LabelField(new Rect(xCol1, yStart += ySpace, colWidth, colHeight), "Release:");
                EditorGUI.LabelField(new Rect(xCol2, yStart, colWidth, colHeight), ToolsEditor.releaseDate);

                yStart += 15;
                EditorGUI.LabelField(new Rect(xCol0, yStart += ySpace, colWidth, colHeight), "Developed by Thierry Bachmann");
                EditorGUI.LabelField(new Rect(xCol1, yStart += ySpace, colWidth, colHeight), "Email:");
                EditorGUI.TextField(new Rect(xCol2, yStart, colWidth, colHeight), "thierry.bachmann@gmail.com");
                EditorGUI.LabelField(new Rect(xCol1, yStart += ySpace, colWidth, colHeight), "Website:");
                EditorGUI.TextField(new Rect(xCol2, yStart, colWidth, colHeight), ToolsEditor.paxSite);

                yStart += 30;
                colWidth = 110;
                int space = 8;
                if (GUI.Button(new Rect(xCol0, yStart, colWidth, colHeight), "Open Web Site"))
                {
                    Application.OpenURL(ToolsEditor.paxSite);
                }
                if (GUI.Button(new Rect(xCol0 + colWidth + space, yStart, colWidth, colHeight), "Help"))
                {
                    Application.OpenURL(ToolsEditor.blogSite);
                }

                if (GUI.Button(new Rect(xCol0 + colWidth + space + colWidth + space, yStart, colWidth, colHeight), "Get Full Version"))
                {
                    Application.OpenURL(ToolsEditor.UnitySite);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}