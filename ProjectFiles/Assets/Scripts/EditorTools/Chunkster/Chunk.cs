using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

[ExecuteInEditMode]
public class Chunk : MonoBehaviour {
    // data stored in chunk instantiated by chunkster
    public int chunkX, chunkY;

    private Mesh mesh;
    private Dictionary<int, Vector3> left, right, top, bottom;
    public bool edgedRecently = false;

    void Awake() {
        #if UNITY_EDITOR
            // var meshFilter = this.GetComponent<MeshFilter>();
            // var meshCopy   = Mesh.Instantiate(meshFilter.sharedMesh) as Mesh;
            // mesh = meshFilter.mesh = meshCopy;
            mesh = this.GetComponent<MeshFilter>().sharedMesh;
            //mesh = meshCopy;
        #else
            // play mode
            mesh = this.GetComponent<MeshFilter>().mesh;
        #endif
    }

    public void RefreshEdges() {
        this.edgedRecently = true;

        // find occurances of vertex number paired w/ coordinate
        var counts = mesh.triangles.GroupBy(x => x)
                         .OrderBy          (x => x.Key)
                         .ToDictionary     (x => x.Key, x => x.Count());

        var edgeVertexIdxs = mesh.triangles.GroupBy     (x => x)
                                           .OrderBy     (x => x.Key)
                                           .ToDictionary(x => x.Key, x => x.Count());
        // find edges
        // we know a vertex which is an edge will only
        // be used in two triangles
        var edgeVertices   = edgeVertexIdxs.Where       (x => x.Value < 4)
                                           .ToDictionary(x => x.Key, x => this.mesh.vertices[x.Key]);
        // find corners
        var corners = counts.Where       (x => x.Value < 3)
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

    // will only be accurate if the edges have been refreshed
    public Dictionary<int, Vector3> GetMeshEdge(Vector3 direction) {
        if       (direction == Vector3.up) {
            if(this.top == null)
                Debug.Log("nulll");
            Debug.Log("umm" + direction + top);
            return this.top;
        } else if(direction == Vector3.left) {
            return this.left;
        } else if(direction == Vector3.right) {
            return this.right;
        } else if(direction == Vector3.down) {
            return this.bottom;
        } else {
            return null;
        }
    }

    public bool HasPolybrushMesh() {
        UpdateMeshReference();
        return this.mesh.name.StartsWith("Polybrush");
    }

    public void UpdateMeshReference() {
        this.mesh = this.GetComponent<MeshFilter>().sharedMesh;
    }

    // updates mesh with edge
    public void UpdateEdge(Dictionary<int, Vector3> edge) {
        var vertices = new Vector3[this.mesh.vertices.Length];
        Array.Copy(this.mesh.vertices, vertices, this.mesh.vertices.Length);
        foreach (var item in edge){
            vertices[item.Key] = item.Value;//new Vector3(2, 0, 0);//item.Value;
        }
        this.mesh.vertices = vertices;

        //var meshFilter = this.GetComponent<MeshFilter>();
        //meshFilter.mesh.SetVertices(vertices, 0, vertices.Length);
        //meshFilter.mesh.vertices = vertices;
        //meshFilter.mesh.RecalculateNormals();
        //meshFilter.mesh = this.mesh;
    }

    void Update() {
        // var vertices = new Vector3[this.mesh.vertices.Length];
        // Array.Copy(this.mesh.vertices, vertices, this.mesh.vertices.Length);
        // foreach (var item in this.left) {
        //     if (item.Key < vertices.Length) {
        //         vertices[item.Key] += new Vector3(0, 0, 1);
        //     }
        // }
        // this.mesh.vertices = vertices;
    }
}