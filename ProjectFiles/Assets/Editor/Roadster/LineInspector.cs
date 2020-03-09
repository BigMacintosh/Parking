﻿using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Line))]
public class LineInspector : Editor {
    private void OnSceneGUI() {
        //Get the line component and position
        var line          = target as Line;
        var lineTransform = line.transform;

        //Get p0 and p1
        var p0 = lineTransform.TransformPoint(line.p0);
        var p1 = lineTransform.TransformPoint(line.p1);

        //Get orientation of handle
        var handleRotation = Tools.pivotRotation == PivotRotation.Local ? lineTransform.rotation : Quaternion.identity;

        //Draw a line between points
        Handles.color = Color.white;
        Handles.DrawLine(p0, p1);
        Handles.DoPositionHandle(p0, handleRotation);
        Handles.DoPositionHandle(p1, handleRotation);

        //Check to see if positions have change and update the points
        EditorGUI.BeginChangeCheck();
        p0 = Handles.DoPositionHandle(p0, handleRotation);
        if (EditorGUI.EndChangeCheck()) {
            //Enable Undo
            Undo.RecordObject(line, "Move Point");
            EditorUtility.SetDirty(line);
            line.p0 = lineTransform.InverseTransformPoint(p0);
        }

        EditorGUI.BeginChangeCheck();
        p1 = Handles.DoPositionHandle(p1, handleRotation);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(line, "Move Point");
            EditorUtility.SetDirty(line);
            line.p1 = lineTransform.InverseTransformPoint(p1);
        }
    }
}