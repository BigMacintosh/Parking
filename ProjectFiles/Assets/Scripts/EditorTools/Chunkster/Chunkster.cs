using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class Chunkster : MonoBehaviour {
    // TODO: remove this cus unity has this in Vector3 oops
    public enum Direction { Up, Down, Left, Right }

    // our base chunk
    public GameObject baseChunk;
    

    void Awake() {
        // add centre chunk if it doesn't exist
        if (!this.chunkExists(0, 0, out _)) {
            createNewChunk(0, 0);
        }
    }

    (int, int) addDirection(int chunkX, int chunkY, Direction dir) {
        switch (dir) {
            case Direction.Up:    return (chunkX,     chunkY + 1);
            case Direction.Down:  return (chunkX,     chunkY - 1);
            case Direction.Left:  return (chunkX - 1, chunkY    );
            case Direction.Right: return (chunkX + 1, chunkY    );
            default:              return (chunkX    , chunkY    );
        }
    }

    private string getChunkId(int chunkX, int chunkY) {
        return this.baseChunk.name + "_" + chunkX + "_" + chunkY;
    }

    public List<(int, int)> getAllChunks() {
        List<(int, int)> chunks = new List<(int, int)>();
        foreach (Transform child in this.gameObject.transform) {
            var chunk = (Chunk) child.GetComponent<Chunk>();
            chunks.Add((chunk.chunkX, chunk.chunkY));
        }
        return chunks;
    }

    public List<(int, int)> getNeighbourChunks(int chunkX, int chunkY) {
        // foreach (Direction dir in Enum.GetValues(typeof(Direction))) {
        //     (neighChunkX, neighChunkY) = this.addDirection(chunkX, chunkY, dir);
        // }
        // return null;
        var xs = Enum.GetValues(typeof(Direction))
                     .Cast<Direction>()
                     .Select(x => this.addDirection(chunkX, chunkY, x))
                     .ToList();
        return xs;
    }

    // converts a chunkX, chunkY to Unity's world coordinates
    // (note the worldY should actually be used in for the z in a transform)
    public (int, int) chunkCoordToWorld(int chunkX, int chunkY) {
        var size = baseChunk.GetComponent<Renderer>().bounds.size;
        return ((int) (chunkX * size.x),
                (int) (chunkY * size.z));
    }
    
    // get chunk child from chunk x, y
    public GameObject getChunk(int chunkX, int chunkY) {
        foreach (Transform child in this.gameObject.transform) {
            if (child.gameObject.name == this.getChunkId(chunkX, chunkY)) {
                return child.gameObject;
            }
        }
        return null;
    }

    public bool chunkExists(int chunkX, int chunkY, out GameObject chunk) {
        foreach (Transform child in this.gameObject.transform) {
            if (child.gameObject.name == this.getChunkId(chunkX, chunkY)) {
                chunk = child.gameObject;
                return true;
            }
        }
        chunk = null;
        return false;
    }

    public GameObject createNewChunk(int chunkX, int chunkY) {
        if (this.chunkExists(chunkX, chunkY, out _)) {
            Debug.LogWarning("Tried to create new chunk at " + (chunkX, chunkY) + " but one already exists!");
            return null;
        } else {
            (int worldX, int worldY)           = this.chunkCoordToWorld(chunkX, chunkY);
            GameObject chunkInstantiated       = Instantiate(this.baseChunk,
                                                             new Vector3(worldX, 0, worldY),
                                                             this.baseChunk.transform.rotation) as GameObject;
            chunkInstantiated.name             = this.getChunkId(chunkX, chunkY);
            chunkInstantiated.transform.parent = gameObject.transform;
            var chunk = (Chunk) chunkInstantiated.GetComponent<Chunk>();
            chunk.chunkX = chunkX;
            chunk.chunkY = chunkY;

            return chunkInstantiated;
        }
    }

    public void deleteChunk(int chunkX, int chunkY) {
        if (this.chunkExists(chunkX, chunkY, out GameObject chunk)) {
            DestroyImmediate(chunk);
        } else {
            Debug.LogWarning("Tried to delete chunk at " + (chunkX, chunkY) + " but it doesn't exist!");
        }
    }

    // stitches together seams between chunks
    // (takes average of touching vertices)
    public void stitchChunks() {
        foreach (Transform child in this.gameObject.transform) {
            var chunk = (Chunk) child.GetComponent<Chunk>();
            
            var (bottomX, bottomY) = this.addDirection(chunk.chunkX, chunk.chunkY, Direction.Down );
            var (rightX,   rightY) = this.addDirection(chunk.chunkX, chunk.chunkY, Direction.Right);

            /*if (!chunk.edgedRecently)*/          chunk.RefreshEdges();

            if (this.chunkExists(bottomX, bottomY, out GameObject chunkBot)) {
                var chunkBotData = (Chunk) chunkBot.GetComponent<Chunk>();
                /*if (!chunkBotData.edgedRecently)*/   chunkBotData.RefreshEdges();
                stitchChunkPair(chunk, Vector3.down, chunkBotData, Vector3.up);
            }
            if (this.chunkExists(rightX, rightY, out GameObject chunkRight)) {
                var chunkRightData = (Chunk) chunkRight.GetComponent<Chunk>();
                /*if (!chunkRightData.edgedRecently)*/ chunkRightData.RefreshEdges();
                stitchChunkPair(chunk, Vector3.right, chunkRightData, Vector3.left);
            }

            chunk.edgedRecently = false;
        }

        // final lighting recalculation (sloooow)
        // TODO: make this properly work
        foreach (Transform child in this.gameObject.transform) {
            var mesh = child.GetComponent<MeshFilter>().sharedMesh;
            mesh.RecalculateNormals();
        }
    }

    // sorts edge dict. and places into list
    // of (k, v) pairs.
    // sorts by x,y depending on which is varying
    List<(int, Vector3)> sortEdge(Dictionary<int, Vector3> edge, Vector3 direction) {
        // true if we want to sort by x
        // if it's false we want to sort by y (probs)
        bool sortX = direction.x == 0;
        Debug.Log(sortX);

        var ordered = edge.OrderBy(x => sortX ? x.Value.x : x.Value.y)
                          .Select (x => (x.Key, x.Value))
                          .ToList ();
        // var ordered = edge.OrderBy(x => x.Value.x)
        //                   .Select (x => (x.Key, x.Value))
        //                   .ToList ();
        return ordered;
    }

    // TODO: one day deal with corners
    // ...but today will not be that day :)
    public void stitchChunkPair(Chunk chunk1, Vector3 chunkEdge1, Chunk chunk2, Vector3 chunkEdge2) {
        if (chunk1.HasPolybrushMesh() && chunk2.HasPolybrushMesh()) {

            // TODO: corners repeated twice?

            // chunk1.RefreshEdges();
            // chunk2.RefreshEdges();

            var edge1 = chunk1.GetMeshEdge(chunkEdge1);
            var edge2 = chunk2.GetMeshEdge(chunkEdge2);

            //edge1 = edge1.ToDictionary(x => x.Key, x => x.Value + new Vector3(0, 0, 1));
            //edge2 = edge2.ToDictionary(x => x.Key, x => x.Value + new Vector3(0, 0, 1));

            // var edgeShared = new Dictionary<int, Vector3>();
            var edge1New = new Dictionary<int, Vector3>();
            var edge2New = new Dictionary<int, Vector3>();

            var orderedEdge1 = sortEdge(edge1, chunkEdge1);
            var orderedEdge2 = sortEdge(edge2, chunkEdge2);

            Debug.Log(orderedEdge1.Count + ", " + orderedEdge2.Count);

            for (int i = 0; i < orderedEdge1.Count; i++) {
                var vec1 = orderedEdge1[i].Item2;
                var vec2 = orderedEdge2[i].Item2;
                Debug.Log("(" + vec1.x + "," + vec1.y + "," + vec1.z
                        + " === "
                        + "(" + vec2.x + "," + vec2.y + "," + vec2.z + ")");
                var avgZ = (vec1.z + vec2.z) / 2;

                edge1New[orderedEdge1[i].Item1] = new Vector3(vec1.x, vec1.y, avgZ);
                edge2New[orderedEdge2[i].Item1] = new Vector3(vec2.x, vec2.y, avgZ);
            }

            // is there any guarantee the vertices are ordered in the same way??
/* 
            foreach (var edge1Vtx in edge1){
                var edge2Vtx = edge1[edge1Vtx.Key];

                // take average of vals
                //var avgVtx = (edge1Vtx.Value + edge1Vtx.Value) / 2;
                var avgVtx = new Vector3(0, 2, 0);

                edgeShared[edge1Vtx.Key] = avgVtx;
            } */

            // sort edge1 and edge2 by x/y
            // go thru
            /* edge1.OrderBy(x => x.Value.x)
                 .Select (x => (x.Key, x.Value))
                 .ToList ();
            edge2.OrderBy(x => x.Value.x)
                 .Select (x => (x.Key, x.Value))
                 .ToList (); */

            // go through values in edge1 & edge2
            // sequentially by key val.
            // probs. not the best way to do this...
            /* int lastEdge2Idx = 0;
            for (int i = 0; i <= edge1.Keys.Max(); i++) {
                // find next key in edge1
                if (edge1.ContainsKey(i)) {
                    // find next key in edge2
                    while (!edge2.ContainsKey(lastEdge2Idx)) {
                        lastEdge2Idx++;
                        if (lastEdge2Idx > edge2.Keys.Max()) {
                            break;
                        }
                    }

                    // these two should be matched?
                    //var avgVec = (edge1[i] + edge2[lastEdge2Idx]) / 2;
                    //var avgVec = (new Vector3(edge1[i].x, ))
                    var avgZ = (edge1[i].z + edge2[lastEdge2Idx].z) / 2;
                    //var avgVec = edge1[i];
                    //var avgVec = new Vector3(0, 1, 0);
                    //  edge1New[i]            = edge1[i] + new Vector3(0, 0, 2);
                    //  edge2New[lastEdge2Idx] = edge2[lastEdge2Idx] + new Vector3(0, 0, 2);
                    edge1New[i]            = new Vector3(edge1[i].x,            edge1[i].y,            avgZ);
                    edge2New[lastEdge2Idx] = new Vector3(edge2[lastEdge2Idx].x, edge2[lastEdge2Idx].y, avgZ);

                    lastEdge2Idx++;
                }

            } */

            // foreach (var item in edge1){
            //     edge1New[item.Key] = item.Value + new Vector3(0, 0, 2);
            // }

            chunk1.UpdateEdge(edge1New);
            chunk2.UpdateEdge(edge2New);
        } else {
            Debug.LogWarning("Tried to stitch mesh of "
                           + chunk1.gameObject.name + " & " + chunk2.gameObject.name
                           + " but couldn't as it is not a Polybrush mesh instance!");
        }
    }
}