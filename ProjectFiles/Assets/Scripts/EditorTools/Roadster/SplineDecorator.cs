﻿using UnityEngine;

public class SplineDecorator : MonoBehaviour {
    public int frequency;

    public Transform[] items;

    public bool lookForward;

    public BezierSpline spline;

    private void Awake() {
        if (frequency <= 0 || items == null || items.Length == 0) {
            return;
        }

        var stepSize = 1f / (frequency * items.Length);
        for (int p = 0, f = 0; f < frequency; f++) {
            for (var i = 0; i < items.Length; i++, p++) {
                var item     = Instantiate(items[i]);
                var position = spline.GetPoint(p * stepSize);
                item.transform.localPosition = position;
                if (lookForward) {
                    item.transform.LookAt(position + spline.GetDirection(p * stepSize));
                }

                item.transform.parent = transform;
            }
        }
    }
}