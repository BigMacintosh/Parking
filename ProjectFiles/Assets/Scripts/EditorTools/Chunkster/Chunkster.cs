using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EditorTools.Chunkster {
    [ExecuteInEditMode]
// we store our chunk objects as children of this script
// and then manipulate them via referencing children
// i am sorry, but this was the most error-free way
// even if it is slow
    public class Chunkster : MonoBehaviour {
        // TODO: remove this cus unity has this in Vector3 oops
        private enum Direction {
            Up,
            Down,
            Left,
            Right
        }

        // our base chunk
        public GameObject baseChunk;

        private void Awake() {
            // add centre chunk if it doesn't exist
            if (!ChunkExists(0, 0, out _)) {
                CreateNewChunk(0, 0);
            }
        }

        private (int, int) AddDirection(int chunkX, int chunkY, Direction dir) {
            switch (dir) {
                case Direction.Up:    return (chunkX, chunkY + 1);
                case Direction.Down:  return (chunkX, chunkY - 1);
                case Direction.Left:  return (chunkX         - 1, chunkY);
                case Direction.Right: return (chunkX         + 1, chunkY);
                default:              return (chunkX, chunkY);
            }
        }

        private string GetChunkId(int chunkX, int chunkY) {
            return baseChunk.name + "_" + chunkX + "_" + chunkY;
        }

        public List<(int, int)> GetAllChunks() {
            var chunks = new List<(int, int)>();
            foreach (Transform child in gameObject.transform) {
                var chunk = child.GetComponent<Chunk>();
                chunks.Add((chunk.chunkX, chunk.chunkY));
            }

            return chunks;
        }

        public List<(int, int)> GetNeighbourChunks(int chunkX, int chunkY) {
            var xs = Enum.GetValues(typeof(Direction))
                         .Cast<Direction>()
                         .Select(x => AddDirection(chunkX, chunkY, x))
                         .ToList();
            return xs;
        }

        // converts a chunkX, chunkY to Unity's world coordinates
        // (note the worldY should actually be used in for the z in a transform)
        public (int, int) ChunkCoordToWorld(int chunkX, int chunkY) {
            var size = baseChunk.GetComponent<Renderer>().bounds.size;
            return ((int) (chunkX * size.x),
                    (int) (chunkY * size.z));
        }

        // get chunk child from chunk x, y
        public GameObject GetChunk(int chunkX, int chunkY) {
            foreach (Transform child in gameObject.transform) {
                if (child.gameObject.name == GetChunkId(chunkX, chunkY)) {
                    return child.gameObject;
                }
            }

            return null;
        }

        public bool ChunkExists(int chunkX, int chunkY, out GameObject chunk) {
            foreach (Transform child in gameObject.transform) {
                if (child.gameObject.name == GetChunkId(chunkX, chunkY)) {
                    chunk = child.gameObject;
                    return true;
                }
            }

            chunk = null;
            return false;
        }

        public GameObject CreateNewChunk(int chunkX, int chunkY) {
            if (ChunkExists(chunkX, chunkY, out _)) {
                Debug.LogWarning("Tried to create new chunk at " + (chunkX, chunkY) + " but one already exists!");
                return null;
            }

            var (worldX, worldY) = ChunkCoordToWorld(chunkX, chunkY);
            var chunkInstantiated = Instantiate(baseChunk,
                                                new Vector3(worldX, 0, worldY),
                                                baseChunk.transform.rotation);
            chunkInstantiated.name             = GetChunkId(chunkX, chunkY);
            chunkInstantiated.transform.parent = gameObject.transform;
            var chunk = chunkInstantiated.GetComponent<Chunk>();
            chunk.chunkX = chunkX;
            chunk.chunkY = chunkY;

            return chunkInstantiated;
        }

        public void DeleteChunk(int chunkX, int chunkY) {
            if (ChunkExists(chunkX, chunkY, out var chunk)) {
                DestroyImmediate(chunk);
            } else {
                Debug.LogWarning("Tried to delete chunk at " + (chunkX, chunkY) + " but it doesn't exist!");
            }
        }

        // stitches together seams between ALL chunks
        public void StitchChunks() {
            foreach (Transform child in gameObject.transform) {
                var chunk = child.GetComponent<Chunk>();

                // if we stitch together the bottom and the right for all chunks
                // we will eventually stitch together every chunk
                var (bottomX, bottomY) = AddDirection(chunk.chunkX, chunk.chunkY, Direction.Down);
                var (rightX, rightY)   = AddDirection(chunk.chunkX, chunk.chunkY, Direction.Right);

                /*if (!chunk.edgedRecently)*/
                chunk.RefreshEdges();

                if (ChunkExists(bottomX, bottomY, out var chunkBot)) {
                    var chunkBotData = chunkBot.GetComponent<Chunk>();
                    /*if (!chunkBotData.edgedRecently)*/
                    chunkBotData.RefreshEdges();
                    StitchChunkPair(chunk, Vector3.up, chunkBotData, Vector3.down);
                }

                if (ChunkExists(rightX, rightY, out var chunkRight)) {
                    var chunkRightData = chunkRight.GetComponent<Chunk>();
                    /*if (!chunkRightData.edgedRecently)*/
                    chunkRightData.RefreshEdges();
                    StitchChunkPair(chunk, Vector3.right, chunkRightData, Vector3.left);
                }

                chunk.edgedRecently = false;
            }

            // final lighting recalculation (sloooow)
            // TODO: make this properly work
            foreach (Transform child in gameObject.transform) {
                var mesh = child.GetComponent<MeshFilter>().sharedMesh;
                mesh.RecalculateNormals();
            }
        }

        // sorts edge dict. and places into list
        // of (k, v) pairs.
        // sorts by x,y depending on which is varying
        private List<(int, Vector3)> SortEdge(Dictionary<int, Vector3> edge, Vector3 direction) {
            // true if we want to sort by x
            // if it's false we want to sort by y (probs)
            var sortX = direction.x == 0;

            var ordered = edge.OrderBy(x => sortX ? x.Value.x : x.Value.z)
                              .Select(x => (x.Key, x.Value))
                              .ToList();
            return ordered;
        }

        // TODO: one day deal with corners
        // ...but today will not be that day :)
        public void StitchChunkPair(Chunk chunk1, Vector3 chunkEdge1, Chunk chunk2, Vector3 chunkEdge2) {
            if (chunk1.HasPolybrushMesh() && chunk2.HasPolybrushMesh()) {
                var edge1        = chunk1.GetMeshEdge(chunkEdge1);
                var edge2        = chunk2.GetMeshEdge(chunkEdge2);

                var edge1New     = new Dictionary<int, Vector3>();
                var edge2New     = new Dictionary<int, Vector3>();
                var orderedEdge1 = SortEdge(edge1, chunkEdge1);
                var orderedEdge2 = SortEdge(edge2, chunkEdge2);

                for (var i = 0; i < orderedEdge1.Count; i++) {
                    var vec1 = orderedEdge1[i].Item2;
                    var vec2 = orderedEdge2[i].Item2;
                    // var avgZ = (vec1.z + vec2.z) / 2;

                    // edge1New[orderedEdge1[i].Item1] = new Vector3(vec1.x, vec1.y, avgZ);
                    // edge2New[orderedEdge2[i].Item1] = new Vector3(vec2.x, vec2.y, avgZ);

                    var avgY = (vec1.y + vec2.y) / 2;

                    edge1New[orderedEdge1[i].Item1] = new Vector3(vec1.x, avgY, vec1.z);
                    edge2New[orderedEdge2[i].Item1] = new Vector3(vec2.x, avgY, vec2.z);
                }

                chunk1.UpdateEdge(edge1New);
                chunk2.UpdateEdge(edge2New);
            } else {
                // if we try to stitch a mesh which is not polybrush, we will
                // be manipulating the shared mesh of all base terrain objects.
                // so let's not do that
                Debug.LogWarning("Tried to stitch mesh of "
                               + chunk1.gameObject.name + " & " + chunk2.gameObject.name
                               + " but couldn't as it is not a Polybrush mesh instance!");
            }
        }
    }
}