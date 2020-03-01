﻿using System.Collections.Generic;
using UnityEngine;

//Create enum for curve modes
public enum BezierControlPointMode {
    Free,
    Aligned,
    Mirrored
}

public class BezierSpline : MonoBehaviour {
    [SerializeField] private List<BezierControlPointMode> modes;

    [SerializeField] private List<Vector3> points;

    //Handler functions for private list structure
    //Get number of points
    public int ControlPointCount => points.Count;

    //Return the number of curves in the spline
    public int CurveCount => (points.Count - 1) / 3;

    //Get indexed point
    public Vector3 GetControlPoint(int index) {
        return points[index];
    }

    //Set indexed point
    public void SetControlPoint(int index, Vector3 point) {
        if (index % 3 == 0) {
            var delta = point - points[index];
            if (index > 0) {
                points[index - 1] += delta;
            }

            if (index + 1 < points.Count) {
                points[index + 1] += delta;
            }
        }

        points[index] = point;
        EnforceMode(index);
    }

    //Handlers for bezier control points
    public BezierControlPointMode GetControlPointMode(int index) {
        return modes[(index + 1) / 3];
    }

    public void SetControlPointMode(int index, BezierControlPointMode mode) {
        modes[(index + 1) / 3] = mode;
        EnforceMode(index);
    }

    private void EnforceMode(int index) {
        var modeIndex = (index + 1) / 3;
        var mode      = modes[modeIndex];
        if (mode == BezierControlPointMode.Free || modeIndex == 0 || modeIndex == modes.Count - 1) {
            return;
        }

        var middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (index <= middleIndex) {
            fixedIndex    = middleIndex - 1;
            enforcedIndex = middleIndex + 1;
        } else {
            fixedIndex    = middleIndex + 1;
            enforcedIndex = middleIndex - 1;
        }

        var middle          = points[middleIndex];
        var enforcedTangent = middle - points[fixedIndex];
        if (mode == BezierControlPointMode.Aligned) {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }

        points[enforcedIndex] = middle + enforcedTangent;
    }

    public void Reset() {
        points = new List<Vector3>();
        points.Add(new Vector3(0f, 0f, 3f));
        points.Add(new Vector3(0f, 0f, 6f));
        points.Add(new Vector3(0f, 0f, 9f));
        points.Add(new Vector3(0f, 0f, 12f));

        modes = new List<BezierControlPointMode>();
        modes.Add(BezierControlPointMode.Free);
        modes.Add(BezierControlPointMode.Free);
    }

    public void AddCurve() {
        var point = points[points.Count - 1];
        point.z += 3f;
        points.Add(point);
        point.z += 3f;
        points.Add(point);
        point.z += 3f;
        points.Add(point);

        modes.Add(modes[modes.Count - 1]);
        EnforceMode(points.Count - 4);
    }

    //Get a point t distance along the curve
    public Vector3 GetPoint(float t) {
        int i;
        if (t >= 1f) {
            t = 1f;
            i = points.Count - 4;
        } else {
            t =  Mathf.Clamp01(t) * CurveCount;
            i =  (int) t;
            t -= i;
            i *= 3;
        }

        return transform.TransformPoint(Bezier.GetPoint(
                                            points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }

    //Get the 'velocity' at that point
    public Vector3 GetVelocity(float t) {
        int i;
        if (t >= 1f) {
            t = 1f;
            i = points.Count - 4;
        } else {
            t =  Mathf.Clamp01(t) * CurveCount;
            i =  (int) t;
            t -= i;
            i *= 3;
        }

        return transform.TransformPoint(Bezier.GetFirstDerivative(
                                            points[i], points[i + 1], points[i + 2], points[i + 3], t)) -
               transform.position;
    }

    //Get the direction of the curve at that point
    public Vector3 GetDirection(float t) {
        return GetVelocity(t).normalized;
    }
}