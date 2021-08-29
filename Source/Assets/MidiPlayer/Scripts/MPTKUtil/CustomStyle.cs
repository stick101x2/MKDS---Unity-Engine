using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MidiPlayerTK
{
    public class CustomStyle
    {
        public GUIStyle BackgPopupList;
        public GUIStyle BacgDemos;
        public GUIStyle BacgDemos1;
        public GUIStyle BackgMidiList;
        public GUIStyle TextFieldMultiLine;
        public GUIStyle TextFieldMultiCourier;
        public GUIStyle VScroll;
        public GUIStyle HScroll;
        public GUIStyle TitleLabel1;
        public GUIStyle TitleLabel2;
        public GUIStyle TitleLabel2Centered;
        public GUIStyle TitleLabel3;
        public GUIStyle LabelRight;
        public GUIStyle LabelCentered;
        public GUIStyle LabelAlert;
        public GUIStyle LabelGreen;
        public GUIStyle LabelLeft;
        public GUIStyle KeyWhite;
        public GUIStyle KeyBlack;
        public GUIStyle BtStandard;
        public GUIStyle BtSelected;
        public GUIStyle ItemSelected;
        public GUIStyle ItemNotSelected;
        public GUIStyle ItemNotSelectedCentered;
        public GUIStyle BlueText;
        public GUIStyle LabelZone;
        public GUIStyle LabelList;
        public GUIStyle LabelZoneCentered;
        public GUIStyle LabelTitle;
        public GUIStyle SliderBar;
        public GUIStyle SliderThumb;
        public GUIStyle BtTransparent;
        public GUIStyle BtListNormal;

        public Color ButtonColor = new Color(.7f, .9f, .7f, 1f);

        /// <summary>
        /// Set custom Style. Good for background color 3E619800
        /// </summary>
        public CustomStyle()
        {
            Texture2D gray = Resources.Load<Texture2D>("Textures/gray");
            Texture2D white = Resources.Load<Texture2D>("Textures/white");
            Texture2D black = Resources.Load<Texture2D>("Textures/black");
            Texture2D green = Resources.Load<Texture2D>("Textures/green");
            Texture2D greendark = Resources.Load<Texture2D>("Textures/greendark");
            Texture2D greenback = Resources.Load<Texture2D>("Textures/greenback");
            Texture2D greenlight = Resources.Load<Texture2D>("Textures/greenlight");
            
            BtStandard = new GUIStyle("Button");

            BtSelected = new GUIStyle("Button");
            BtSelected.fontStyle = FontStyle.Bold;
            BtSelected.normal.textColor = new Color(0.5f, 0.9f, 0.5f);
            BtSelected.hover.textColor = BtSelected.normal.textColor;
            BtSelected.active.textColor = BtSelected.normal.textColor;

            BtTransparent = new GUIStyle("Button");
            BtTransparent.normal.background = null;
            BtTransparent.active.background = null;
            BtTransparent.alignment = TextAnchor.UpperLeft;
            BtTransparent.border = new RectOffset(0, 0, 0, 0);
            BtTransparent.padding = new RectOffset(0, 0, 0, 0);


            BtListNormal = new GUIStyle("Button");
            BtListNormal.normal.background = null;
            BtListNormal.active.background = null;
            BtListNormal.alignment = TextAnchor.UpperLeft;
            BtListNormal.border = new RectOffset(1, 1, 1, 1);
            BtListNormal.padding = new RectOffset(0, 0, 0, 0);


            ItemSelected = new GUIStyle("label");
            ItemSelected.normal.background = greenlight;
            ItemSelected.alignment = TextAnchor.UpperLeft;

            ItemNotSelected = new GUIStyle("label");
            ItemNotSelected.alignment = TextAnchor.UpperLeft;

            ItemNotSelectedCentered = new GUIStyle("label");
            ItemNotSelectedCentered.alignment = TextAnchor.UpperCenter;

            BackgPopupList = new GUIStyle("box"); // Issue with window: become transparent when get focus.
            BackgPopupList.normal.background = Resources.Load<Texture2D>("Textures/window");

            BackgMidiList = new GUIStyle("textField");

            BacgDemos = new GUIStyle("box");
            BacgDemos.normal.background = gray;// SetColor(new Texture2D(2, 2), new Color(.3f, .4f, .2f, 1f));// Issue with window: become transparent when get focus.
            BacgDemos.normal.textColor = Color.black;

            BacgDemos1 = new GUIStyle("box");
            BacgDemos1.normal.background = greenback; //SetColor(new Texture2D(2, 2), new Color(.3f, .5f, .2f, 1f));// Issue with window: become transparent when get focus.
            BacgDemos1.normal.textColor = Color.black;

            VScroll = new GUIStyle("verticalScrollbar");

            HScroll = new GUIStyle("horizontalScrollbar");

            TitleLabel1 = new GUIStyle("label");
            TitleLabel1.fontSize = 20;
            TitleLabel1.alignment = TextAnchor.MiddleLeft;

            TitleLabel2 = new GUIStyle("label");
            TitleLabel2.fontSize = 16;
            TitleLabel2.alignment = TextAnchor.MiddleLeft;

            TitleLabel2Centered = new GUIStyle("label");
            TitleLabel2Centered.fontSize = 16;
            TitleLabel2Centered.alignment = TextAnchor.MiddleCenter;

            TitleLabel3 = new GUIStyle("label");
            TitleLabel3.alignment = TextAnchor.UpperLeft;
            TitleLabel3.fontSize = 14;

            LabelRight = new GUIStyle("label");
            LabelRight.alignment = TextAnchor.UpperRight;
            LabelRight.fontSize = 14;

            LabelLeft = new GUIStyle("label");
            LabelLeft.alignment = TextAnchor.UpperLeft;
            LabelLeft.fontSize = 14;

            LabelCentered = new GUIStyle("Label");
            LabelCentered.alignment = TextAnchor.MiddleCenter;

            LabelAlert = new GUIStyle("Label");
            LabelAlert.alignment = TextAnchor.MiddleLeft;
            LabelAlert.wordWrap = true;
            LabelAlert.normal.textColor = new Color(0.6f, 0.1f, 0.1f);
            LabelAlert.fontSize = 12;

            LabelGreen = new GUIStyle("Label");
            LabelGreen.alignment = TextAnchor.MiddleLeft;
            LabelGreen.wordWrap = true;
            LabelGreen.normal.textColor = new Color(0f, 0.5f, 0f);
            LabelGreen.fontSize = 12;

            SliderBar = new GUIStyle("horizontalslider");
            SliderBar.alignment = TextAnchor.LowerLeft;
            SliderBar.margin = new RectOffset(4, 4, 10, 4);
            SliderThumb = new GUIStyle("horizontalsliderthumb");
            SliderThumb.alignment = TextAnchor.LowerLeft;

            KeyWhite = new GUIStyle("Button");
            KeyWhite.normal.background = white;
            KeyWhite.normal.textColor = new Color(0.1f, 0.1f, 0.1f);
            KeyWhite.alignment = TextAnchor.UpperCenter;
            KeyWhite.fontSize = 8;

            KeyBlack = new GUIStyle("Button");
            KeyBlack.normal.background = black;
            KeyBlack.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            KeyBlack.alignment = TextAnchor.UpperCenter;
            KeyBlack.fontSize = 8;


            TextFieldMultiLine = new GUIStyle("textField");
            TextFieldMultiLine.alignment = TextAnchor.UpperLeft;
            TextFieldMultiLine.wordWrap = true;

            TextFieldMultiCourier = new GUIStyle("textField");
            TextFieldMultiCourier.alignment = TextAnchor.UpperLeft;
            TextFieldMultiCourier.wordWrap = true;
            TextFieldMultiCourier.richText = true;
            TextFieldMultiCourier.font = Resources.Load<Font>("Courier");

            LabelList = new GUIStyle("label");
            LabelList.alignment = TextAnchor.MiddleLeft;
            LabelList.normal.textColor = Color.black;
            LabelList.wordWrap = false;
            LabelList.fontSize = 12;
            LabelList.border = new RectOffset(0, 0, 0, 0);
            LabelList.padding = new RectOffset(0, 0, 0, 0);

            LabelZone = new GUIStyle("textField");
            LabelZone.alignment = TextAnchor.MiddleLeft;
            LabelZone.normal.background = green;
            LabelZone.normal.textColor = Color.black;
            LabelZone.wordWrap = true;
            LabelZone.fontSize = 14;

            LabelZoneCentered = new GUIStyle("textField");
            LabelZoneCentered.alignment = TextAnchor.MiddleCenter;
            LabelZoneCentered.normal.background = green;
            LabelZoneCentered.normal.textColor = Color.black;
            LabelZoneCentered.wordWrap = true;
            LabelZoneCentered.fontSize = 14;

            LabelTitle = new GUIStyle("textField");
            LabelTitle.alignment = TextAnchor.MiddleCenter;
            LabelTitle.normal.background = greendark;
            LabelTitle.normal.textColor = Color.black;
            LabelTitle.wordWrap = true;
            LabelTitle.fontSize = 14;

            BlueText = new GUIStyle("textArea");
            BlueText.normal.textColor = new Color(0, 0, 0.99f);
            BlueText.alignment = TextAnchor.UpperLeft;
            //styleDragZone.border = new RectOffset(2, 2, 2, 2);
        }

        /// <summary>
        /// Used to define color of GUI ----  Issue: become transparent when get focus. Prefer loading texture from resource
        /// </summary>
        /// <param name="tex2"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public Texture2D SetColor(Texture2D tex2, Color32 color)
        {
            var fillColorArray = tex2.GetPixels32();
            for (var i = 0; i < fillColorArray.Length; ++i)
                fillColorArray[i] = color;
            tex2.SetPixels32(fillColorArray);
            tex2.Apply();
            return tex2;
        }
     
    }
}
