using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

public class Chunk : MonoBehaviour {
    // data stored in chunk instantiated by chunkster
    public int chunkX, chunkY;

    private Mesh mesh;
    private Dictionary<int, Vector3> triCounts;
    private Dictionary<int, Vector3> left;

    void Start() {
        mesh = this.GetComponent<MeshFilter>().mesh;

        // find edges
        // we know a vertex which is an edge will only
        // be used in two triangles
        var counts = mesh.triangles.GroupBy(x => x)
                         .OrderBy(x => x.Key)
                         .ToDictionary(x => x.Key, x => x.Count());
        
        // triCounts = counts.Where(x => x.Value < 4)
        //                   .ToDictionary(x => x.Key, x => x.Value);

        var edgeVertexIdxs = mesh.triangles.GroupBy     (x => x)
                                           .OrderBy     (x => x.Key)
                                           .ToDictionary(x => x.Key, x => x.Count());
        var cornerVertices = edgeVertexIdxs.Where       (x => x.Value < 3)
                                           .ToDictionary(x => x.Key, x => this.mesh.vertices[x.Key]);
        // includes corners
        // var edgeVertices   = edgeVertexIdxs.Where       (x => x.Value < 4);
                                        //    .ToList      ()
                                        //    .Select      (x => this.mesh.vertices[x.Key])
                                        //    .Distinct    ();
                                        //    .ToDictionary(x => x.Key, x => this.mesh.vertices[x.Key])
        var edgeVertices   = edgeVertexIdxs.Where (x => x.Value < 4)
                                        //    .SelectMany(x => (Vector3) this.mesh.vertices[x.Key]);
                                        // .Select(x => x.Key, x => this.mesh.vertices[x.Key])
                                        .ToDictionary(x => x.Key, x => this.mesh.vertices[x.Key]);
                                        // .Distinct()
                                        // .ToList();
        // Debug.Log(edgeVertices);
        var corners = counts.Where(x => x.Value < 3)
                            .ToDictionary(x => x.Key, x => this.mesh.vertices[x.Key]);
        // var mappedEdges = triCounts.ToDictionary(x => x.Key, x => this.mesh.vertices[x.Key]);

        var edge1 = edgeVertices.Where(v => v.Value.x == 1.0
                                         && v.Value.z == 0.0)
                                .ToDictionary(x => x.Key, x => x.Value);
        // x z
        var edges = new Dictionary<int, Vector3>();
        
        foreach (var corner in corners){
            var edge = edgeVertices.Where(v => v.Value.x == 0
                                            && v.Value.z == 0)
                                   .ToDictionary(x => x.Key, x => x.Value);
            //edges.Add(edge);//
        }
        this.triCounts = edges;

        var orderedCorners = this.orderCorners(corners);

        // could put this in a function but honestly... cba
        var leftMost  = corners.Min(x => x.Value.x);
        var rightMost = corners.Max(x => x.Value.x);
        var botMost   = corners.Max(x => x.Value.y);
        var topMost   = corners.Min(x => x.Value.y);

        var left  = edgeVertices.Where(v => v.Value.x == leftMost ).ToDictionary(x => x.Key, x => x.Value);
        var right = edgeVertices.Where(v => v.Value.x == rightMost).ToDictionary(x => x.Key, x => x.Value);
        var bot   = edgeVertices.Where(v => v.Value.y == botMost  ).ToDictionary(x => x.Key, x => x.Value);
        var top   = edgeVertices.Where(v => v.Value.y == topMost  ).ToDictionary(x => x.Key, x => x.Value);

        this.left = bot;

        foreach (var item in left) {
            Debug.Log(item);
        }
    }

    Dictionary<int, Vector3> findEdge(int x, bool constrainX, int y, bool constrainY) {
        if (constrainX) {

        }
        return null;
    }

    // orders corners into edges;
    // 0: top-left
    // 1: bottom-left
    // 2: top-right
    // 3: bottom-right
    // (might be hacky and gross but so am i xxx)
    List<KeyValuePair<int, Vector3>> orderCorners(Dictionary<int, Vector3> corners) {
        var ordCorners = new List<KeyValuePair<int, Vector3>>();

        var xs = corners.OrderBy(v => v.Value.x).ToList();
        var ys_left  = new List<KeyValuePair<int, Vector3>>() { xs[0], xs[1] }
                      .OrderBy(v => v.Value.y)
                      .ToList();
        var ys_right = new List<KeyValuePair<int, Vector3>>() { xs[2], xs[3] }
                      .OrderBy(v => v.Value.y)
                      .ToList();

        ordCorners.Add(ys_left[0]);
        ordCorners.Add(ys_left[1]);
        ordCorners.Add(ys_right[0]);
        ordCorners.Add(ys_right[1]);

        return ordCorners;
    }

    void Update() {
        // this.mesh.vertices = this.mesh.vertices
        //                     .Cast<Vector3>()
        //                     .Select(x => x + new Vector3(0, 0, 1))
        //                     .ToArray();
        // if (mesh != null) {    
        //     Debug.Log("ainfoasfna");
        //     var vertices = new Vector3[this.mesh.vertices.Length];
        //     Array.Copy(this.mesh.vertices, vertices, this.mesh.vertices.Length);
        //     for (int i = 0; i < this.mesh.vertices.Length / 4; i++) {
        //         // if (i < this.mesh.vertices.Length) {
        //             vertices[i] += new Vector3(0, 0, 1);
        //             // this.mesh.vertices[i] = this.mesh.vertices[i] + new Vector3(0, 0, 1);1
        //         // }
        //     }
        //     this.mesh.vertices = vertices;
        // }

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