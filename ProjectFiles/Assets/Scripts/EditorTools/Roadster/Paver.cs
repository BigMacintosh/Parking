﻿using System.Collections.Generic;
using UnityEngine;

public class Paver : MonoBehaviour {
    [SerializeField] private float         divisions;
    [SerializeField] private float         maxGroundDistance;
    [SerializeField] private float         raiseAboveGround;
    [SerializeField] private bool          signpost;
    [SerializeField] private BezierSpline  spline;
    [SerializeField] private Material      surface;
    [SerializeField] private float         thickness;
    private                  List<int>     triangles;
    private                  List<Vector2> uv;

    private                  List<Vector3> vertices;
    [SerializeField] private float         width;

    [ContextMenu("Pave Road")]
    private void PaveRoad() {
        Debug.Log("Paving Road");
        Awake();
    }

    private void Awake() {
        vertices  = new List<Vector3>();
        triangles = new List<int>();
        uv        = new List<Vector2>();

        var mesh     = new Mesh();
        var filter   = gameObject.AddComponent<MeshFilter>();
        var renderer = gameObject.AddComponent<MeshRenderer>();

        populateVertices();
        populateTris();

        mesh.Clear();
        mesh.vertices     = vertices.ToArray();
        mesh.triangles    = triangles.ToArray();
        mesh.uv           = uv.ToArray();
        filter.mesh       = mesh;
        renderer.material = surface;
        mesh.RecalculateNormals();

        var collider = gameObject.AddComponent<MeshCollider>();
    }

    private void populateVertices() {
        var stepSize = 1f / divisions;

        for (var p = 0; p < divisions; p++) {
            var roadPoint = spline.GetPoint(p * stepSize);

            var velX   = spline.GetDirection(p * stepSize).x;
            var velZ   = spline.GetDirection(p * stepSize).z;
            var velMag = new Vector2(velX, velZ).magnitude;

            if (signpost) {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position   = roadPoint;
                sphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            var alpha = Mathf.PI / 2 - Mathf.Asin(velZ / velMag);

            if (velX < 0) {
                alpha *= -1;
            }


            var rightPoint = new Vector3(roadPoint.x + width / 2 * Mathf.Cos(alpha), roadPoint.y,
                                         roadPoint.z - width / 2 * Mathf.Sin(alpha)) - transform.position;
            var signRightPoint = rightPoint;

            RaycastHit rightHit;
            if (Physics.Raycast(new Ray(rightPoint + transform.position, Vector3.down), out rightHit,
                                maxGroundDistance)) {
                rightPoint.y -= rightHit.distance - raiseAboveGround;
                if (signpost) {
                    var cap     = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    var signPos = rightPoint + transform.position;
                    signPos.y                -= rightHit.distance;
                    cap.transform.position   =  signPos;
                    cap.transform.localScale =  new Vector3(0.5f, 0.5f, 0.5f);
                }
            }

            var rightLowerPoint = rightPoint - new Vector3(0f, thickness, 0f);

            var leftPoint = new Vector3(roadPoint.x - width / 2 * Mathf.Cos(alpha), roadPoint.y,
                                        roadPoint.z + width / 2 * Mathf.Sin(alpha)) - transform.position;
            var signLeftPoint = leftPoint;

            RaycastHit leftHit;
            if (Physics.Raycast(new Ray(leftPoint + transform.position, Vector3.down), out leftHit,
                                maxGroundDistance)) {
                leftPoint.y -= leftHit.distance - raiseAboveGround;
                if (signpost) {
                    var cap     = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    var signPos = leftPoint + transform.position;
                    signPos.y                -= leftHit.distance;
                    cap.transform.position   =  signPos;
                    cap.transform.localScale =  new Vector3(0.5f, 0.5f, 0.5f);
                }
            }

            var leftLowerPoint = leftPoint - new Vector3(0f, thickness, 0f);

            vertices.Add(leftPoint);
            uv.Add(new Vector2(0f, 1 * (p % 2)));
            vertices.Add(leftLowerPoint);
            uv.Add(new Vector2(0.9f, 0f));
            vertices.Add(rightPoint);
            uv.Add(new Vector2(1f, 1 * (p % 2)));
            vertices.Add(rightLowerPoint);
            uv.Add(new Vector2(0f, 0.9f));

            if (signpost) {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position   = signLeftPoint + transform.position;
                cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                cube                      = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position   = signRightPoint + transform.position;
                cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
        }
    }

    private void populateTris() {
        //Front surface
        triangles.Add(0);
        triangles.Add(2);
        triangles.Add(1);

        triangles.Add(2);
        triangles.Add(3);
        triangles.Add(1);

        for (var v = 0; v < vertices.Count - 4; v += 4) {
            //Top surface
            triangles.Add(v);
            triangles.Add(v + 4);
            triangles.Add(v + 6);

            triangles.Add(v + 2);
            triangles.Add(v);
            triangles.Add(v + 6);

            //Left Surface
            triangles.Add(v);
            triangles.Add(v + 1);
            triangles.Add(v + 5);

            triangles.Add(v + 5);
            triangles.Add(v + 4);
            triangles.Add(v);

            //Right Surface
            triangles.Add(v + 2);
            triangles.Add(v + 6);
            triangles.Add(v + 7);

            triangles.Add(v + 2);
            triangles.Add(v + 7);
            triangles.Add(v + 3);
        }


        //Back Surface
        triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 3);

        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
    }
}