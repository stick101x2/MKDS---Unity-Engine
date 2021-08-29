using MidiPlayerTK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MidiPlayerTK
{
    public class SceneHandler : MonoBehaviour
    {
        // Manage skin
        public CustomStyle myStyle;
        //public Texture Header;
        public Button MsgScene;
        //private float widthColTitle = 200;
        private float widthColDescription = 300;
        private float widthColSceneName = 200;
        private float widthColScript = 200;
        private float widthColGo = 200;
        private float widthColVersion = 70;
        private float height = 70;
        private Vector2 scrollerWindow = Vector2.zero;

        public class Demonstrator
        {
            public string Title;
            public string Description;
            public string SceneName;
            public string ScripName;
            public string Version;
            public bool Pro;
            public static List<Demonstrator> Demos;

            public static void Load()
            {
                try
                {
                    Demos = new List<Demonstrator>();
                    TextAsset mytxtData = Resources.Load<TextAsset>("DemosList");
                    string text = System.Text.Encoding.UTF8.GetString(mytxtData.bytes);
                    text = text.Replace("\n", "");
                    string[] listDemos = text.Split('\r');
                    if (listDemos != null)
                    {
                        foreach (string demo in listDemos)
                        {
                            string[] colmuns = demo.Split(';');
                            if (colmuns.Length >= 5)
                                Demos.Add(new Demonstrator()
                                {
                                    Title = colmuns[0],
                                    Description = colmuns[1],
                                    SceneName = colmuns[2],
                                    ScripName = colmuns[3],
                                    Version = colmuns[4],
                                    Pro = colmuns[4] == "Pro" ? true : false
                                });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log("Error loading demonstrator " + ex.Message);
                    throw;
                }
            }
        }
        // Use this for initialization
        void Start()
        {
            Demonstrator.Load();
        }

        void OnGUI()
        {
            //Debug.Log(SceneManager.GetActiveScene().name);
            // Set custom Style. Good for background color 3E619800
            if (myStyle == null) myStyle = new CustomStyle();

            scrollerWindow = GUILayout.BeginScrollView(scrollerWindow, false, false, GUILayout.Width(Screen.width));

            // GUILayout.BeginHorizontal();
            //GUILayout.Space(20);
            // GUILayout.Label(Header, GUILayout.Width(Header.width));
            //GUILayout.Space(40);
            //if (!MsgScene.gameObject.activeInHierarchy)
            {
                MainMenu.Display("MPTK Demonstration - Have a look to the demo scenes and the documentation!", myStyle);

                //GUI.color = Color.black;
                //GUILayout.BeginVertical();
                //GUILayout.Label("MPTK Demonstration - Have a look to the demo scenes and the documentation!", myStyle.TitleLabel1, GUILayout.Height(Header.height / 2));
                //GUI.color = Color.white;
                //GUILayout.Space(20);
                //if (GUILayout.Button("MPTK WebSite: quick start, API, doc, ...", myStyle.BtSelected, GUILayout.Width(400), GUILayout.Height(Header.height / 2 - 20)))
                //    Application.OpenURL("https://paxstellar.fr/midi-player-tool-kit-for-unity-v2/");
                //GUILayout.EndVertical();
            }

            //GUILayout.EndHorizontal();
            GUILayout.Space(20);
            bool header = true;
            if (!MsgScene.gameObject.activeInHierarchy)
            {
                foreach (Demonstrator demo in Demonstrator.Demos)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    //GUILayout.Label(demo.Title, header ? myStyle.LabelTitle : myStyle.LabelZone, GUILayout.Width(widthColTitle), GUILayout.Height(height));
                    if (!header)
                    {
                        if (GUILayout.Button(demo.Title, myStyle.BtSelected, GUILayout.Width(widthColGo), GUILayout.Height(height)))
                        {
                            //for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++) Debug.Log(SceneUtility.GetScenePathByBuildIndex(i));

                            int index = SceneUtility.GetBuildIndexByScenePath(demo.SceneName);
                            if (index < 0)
                            {
                                if (demo.Pro)
                                    Debug.Log($"The demo {demo.SceneName} is available only with MPTK Pro.");
                                else
                                    Debug.LogWarning($"Scene {demo.SceneName} not found");
                                MsgScene.gameObject.SetActive(true);

                            }
                            else
                            {
                                //SceneManager.UnloadScene(SceneManager.GetActiveScene().buildIndex);
                                // Load the demo
                                SceneManager.LoadScene(index, LoadSceneMode.Single);
                            }
                        }
                    }
                    else
                        GUILayout.Label("Load", myStyle.LabelTitle, GUILayout.Width(widthColGo), GUILayout.Height(height));

                    GUILayout.Label(demo.Description, header ? myStyle.LabelTitle : myStyle.LabelZone, GUILayout.Width(widthColDescription), GUILayout.Height(height));
                    GUILayout.Label(demo.SceneName, header ? myStyle.LabelTitle : myStyle.LabelZone, GUILayout.Width(widthColSceneName), GUILayout.Height(height));
                    GUILayout.Label(demo.ScripName, header ? myStyle.LabelTitle : myStyle.LabelZone, GUILayout.Width(widthColScript), GUILayout.Height(height));
                    GUILayout.Label(demo.Version, header ? myStyle.LabelTitle : myStyle.LabelZoneCentered, GUILayout.Width(widthColVersion), GUILayout.Height(height));
                    GUILayout.EndHorizontal();
                    header = false;
                }
            }
            GUILayout.EndScrollView();
        }
    }
}