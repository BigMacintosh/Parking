using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

public class Chunk : MonoBehaviour {
    // data stored in chunk instantiated by chunkster
    public int chunkX, chunkY;

    private Mesh mesh;
    private Dictionary<int, Vector3> left, right, top, bottom;

    void Start() {
        mesh = this.GetComponent<MeshFilter>().mesh;

        // find occurances of vertex number paired w/ coordinate
        var counts = mesh.triangles.GroupBy(x => x)
                         .OrderBy(x => x.Key)
                         .ToDictionary(x => x.Key, x => x.Count());

        var edgeVertexIdxs = mesh.triangles.GroupBy     (x => x)
                                           .OrderBy     (x => x.Key)
                                           .ToDictionary(x => x.Key, x => x.Count());
        // find edges
        // we know a vertex which is an edge will only
        // be used in two triangles
        var edgeVertices   = edgeVertexIdxs.Where (x => x.Value < 4)
                                           .ToDictionary(x => x.Key, x => this.mesh.vertices[x.Key]);
        // find corners
        var corners = counts.Where(x => x.Value < 3)
                            .ToDictionary(x => x.Key, x => this.mesh.vertices[x.Key]);

        // put all edges in a dict
        // could put this in a function but honestly... cba
        var leftMost  = corners.Min(x => x.Value.x);
        var rightMost = corners.Max(x => x.Value.x);
        var botMost   = corners.Max(x => x.Value.y);
        var topMost   = corners.Min(x => x.Value.y);

        this.left   = edgeVertices.Where(v => v.Value.x == leftMost ).ToDictionary(x => x.Key, x => x.Value);
        this.right  = edgeVertices.Where(v => v.Value.x == rightMost).ToDictionary(x => x.Key, x => x.Value);
        this.bottom = edgeVertices.Where(v => v.Value.y == botMost  ).ToDictionary(x => x.Key, x => x.Value);
        this.top    = edgeVertices.Where(v => v.Value.y == topMost  ).ToDictionary(x => x.Key, x => x.Value);
    }

    void Update() {
        var vertices = new Vector3[this.mesh.vertices.Length];
        Array.Copy(this.mesh.vertices, vertices, this.mesh.vertices.Length);
        foreach (var item in this.left) {
            if (item.Key < vertices.Length) {
                vertices[item.Key] += new Vector3(0, 0, 1);
            }
        }
        this.mesh.vertices = vertices;
    }
}