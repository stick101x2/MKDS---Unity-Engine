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
    public class MenuShortcut : EditorWindow
    {

        // Add a menu item to create MidiFilePlayer GameObjects.
        // Priority 1 ensures it is grouped with the other menu items of the same kind
        // and propagated to the hierarchy dropdown and hierarch context menus.
        [MenuItem("GameObject/MPTK/MidiFilePlayer", false, 10)]
        static void CreateMidiFilePlayerGameObject(MenuCommand menuCommand)
        {
            Object prefab = AssetDatabase.LoadAssetAtPath("Assets/MidiPlayer/Prefab/MidiFilePlayer.prefab", typeof(GameObject));
            if (prefab == null)
                Debug.LogWarning("Prefab MidiFilePlayer not found");
            else
            {
                GameObject go = PrefabUtility.InstantiateAttachedAsset(prefab) as GameObject;
                // Ensure it gets reparented if this was a context click (otherwise does nothing)
                GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
                // Register the creation in the undo system
                Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
                Selection.activeObject = go;
            }
        }

        // Add a menu item to create MidiStreamPlayer GameObjects.
        // Priority 1 ensures it is grouped with the other menu items of the same kind
        // and propagated to the hierarchy dropdown and hierarch context menus.
        [MenuItem("GameObject/MPTK/MidiStreamPlayer", false, 10)]
        static void CreateMidiStreamPlayerGameObject(MenuCommand menuCommand)
        {
            Object prefab = AssetDatabase.LoadAssetAtPath("Assets/MidiPlayer/Prefab/MidiStreamPlayer.prefab", typeof(GameObject));
            if (prefab == null)
                Debug.LogWarning("Prefab MidiStreamPlayer not found");
            else
            {
                GameObject go = PrefabUtility.InstantiateAttachedAsset(prefab) as GameObject;
                // Ensure it gets reparented if this was a context click (otherwise does nothing)
                GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
                // Register the creation in the undo system
                Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
                Selection.activeObject = go;
            }
        }

    }
}