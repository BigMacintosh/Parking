using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Paver))]
public class PaverInspector : Editor
{
    public override void OnInspectorGUI() {
        Paver paver = (Paver) target;
        DrawDefaultInspector();
        if (GUILayout.Button("Pave Road", EditorStyles.miniButton)) {
            paver.PaveRoad();
        }
    }
}
