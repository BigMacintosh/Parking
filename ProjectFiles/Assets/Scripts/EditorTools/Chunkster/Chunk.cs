using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EditorTools.Chunkster {

    [ExecuteInEditMode]
    public class Chunk : MonoBehaviour {
        // data stored in chunk instantiated by chunkster
        public  int                      chunkX, chunkY;
        public  bool                     edgedRecently;
        private Dictionary<int, Vector3> left, right, top, bottom;

        private Mesh mesh;

        private void Awake() {
        #if UNITY_EDITOR
            mesh = GetComponent<MeshFilter>().sharedMesh;
        #else
            // play mode
            mesh = this.GetComponent<MeshFilter>().mesh;
        #endif
        }

        // don't look at this
        public void RefreshEdges() {
            edgedRecently = true;

            // find occurances of vertex number paired w/ coordinate
            var counts = mesh.triangles.GroupBy(x => x)
                             .OrderBy(x => x.Key)
                             .ToDictionary(x => x.Key, x => x.Count());

            var edgeVertexIdxs = mesh.triangles.GroupBy(x => x)
                                     .OrderBy(x => x.Key)
                                     .ToDictionary(x => x.Key, x => x.Count());
            // find edges
            // we know a vertex which is an edge will only
            // be used in two triangles
            var edgeVertices = edgeVertexIdxs.Where(x => x.Value < 4)
                                             .ToDictionary(x => x.Key, x => mesh.vertices[x.Key]);
            // find corners
            var corners = counts.Where(x => x.Value < 3)
                                .ToDictionary(x => x.Key, x => mesh.vertices[x.Key]);

            // put all edges in a dict
            // could put this in a function but honestly... cba
            var leftMost  = corners.Min(x => x.Value.x);
            var rightMost = corners.Max(x => x.Value.x);
            var botMost   = corners.Max(x => x.Value.y);
            var topMost   = corners.Min(x => x.Value.y);

            left   = edgeVertices.Where(v => v.Value.x == leftMost).ToDictionary(x => x.Key, x => x.Value);
            right  = edgeVertices.Where(v => v.Value.x == rightMost).ToDictionary(x => x.Key, x => x.Value);
            bottom = edgeVertices.Where(v => v.Value.y == botMost).ToDictionary(x => x.Key, x => x.Value);
            top    = edgeVertices.Where(v => v.Value.y == topMost).ToDictionary(x => x.Key, x => x.Value);
        }

        // will only be accurate if the edges have been refreshed
        public Dictionary<int, Vector3> GetMeshEdge(Vector3 direction) {
            if (direction == Vector3.up) {
                return top;
            }

            if (direction == Vector3.left) {
                return left;
            }

            if (direction == Vector3.right) {
                return right;
            }

            if (direction == Vector3.down) {
                return bottom;
            }

            return null;
        }

        public void UpdateMeshReference() {
            mesh = GetComponent<MeshFilter>().sharedMesh;
        }

        public bool HasPolybrushMesh() {
            UpdateMeshReference();
            return mesh.name.StartsWith("Polybrush");
        }

        // updates mesh with given edge
        public void UpdateEdge(Dictionary<int, Vector3> edge) {
            var vertices = new Vector3[mesh.vertices.Length];
            Array.Copy(mesh.vertices, vertices, mesh.vertices.Length);
            foreach (var item in edge) {
                vertices[item.Key] = item.Value;
            }

            mesh.vertices = vertices;
        }
    }
}