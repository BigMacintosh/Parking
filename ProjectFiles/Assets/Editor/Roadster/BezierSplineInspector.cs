﻿using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor {
    private const int lineSteps = 10;

    private const float handleSize = 0.04f;
    private const float pickSize   = 0.06f;

    private static readonly Color[] modeColors = {
        Color.white,
        Color.yellow,
        Color.cyan
    };

    private Quaternion handleRotation;
    private Transform  handleTransform;

    private int          selectedIndex = -1;
    private BezierSpline spline;

    private void OnSceneGUI() {
        //Get the bezier component
        spline = target as BezierSpline;

        //Get orientation of handle
        handleTransform = spline.transform;
        handleRotation  = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

        //Display the points
        var p0 = ShowPoint(0);
        for (var i = 1; i < spline.ControlPointCount; i += 3) {
            var p1 = ShowPoint(i);
            var p2 = ShowPoint(i + 1);
            var p3 = ShowPoint(i + 2);

            //Draw lines between points
            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p1, p2);
            Handles.DrawLine(p2, p3);

            //Draw aproximate curve
            Handles.color = Color.white;

            //Draw interpolated bezier curve
//            int lineSteps = 10;
//            Vector3 lineStart = curve.GetPoint(0f);
//            for (int i = 1; i <= lineSteps; i++) {
//                Vector3 lineEnd = curve.GetPoint(i / (float)lineSteps);
//                Handles.DrawLine(lineStart, lineEnd);
//                lineStart = lineEnd;
//            }
            Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
            p0 = p3;
        }
    }


    //Function to display handles of points, display a handle only when selected and aa button otherwise
    private Vector3 ShowPoint(int index) {
        //Get handle location
        var point = handleTransform.TransformPoint(spline.GetControlPoint(index));
        Handles.color = modeColors[(int) spline.GetControlPointMode(index)];
        var size = HandleUtility.GetHandleSize(point);

        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap)) {
            selectedIndex = index;
            Repaint();
        }

        if (selectedIndex == index) {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);

            //Update point location on change
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
            }
        }

        return point;
    }

    //Add a button to add another curve to the spline
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        spline = target as BezierSpline;
        //Only show the selected point in the inspector
        if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount) {
            DrawSelectedPointInspector();
        }

        //Add the button to the inspector
        if (GUILayout.Button("Add Curve")) {
            //Enable undoing adding curves
            Undo.RecordObject(spline, "Add Curve");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
        }
    }


    private void DrawSelectedPointInspector() {
        //Only show the values for the selected point in the insepctor
        GUILayout.Label("Selected Point");
        EditorGUI.BeginChangeCheck();
        var point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(spline, "Move Point");
            EditorUtility.SetDirty(spline);
            spline.SetControlPoint(selectedIndex, point);
        }

        //Allow changing of the curve mode in the inspector
        EditorGUI.BeginChangeCheck();
        var mode = (BezierControlPointMode)
            EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(spline, "Change Point Mode");
            spline.SetControlPointMode(selectedIndex, mode);
            EditorUtility.SetDirty(spline);
        }
    }
}