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

            if (!chunk.edgedRecently)          chunk.RefreshEdges();

            if (this.chunkExists(bottomX, bottomY, out GameObject chunkBot)) {
                var chunkBotData = (Chunk) chunkBot.GetComponent<Chunk>();
                if (!chunkBotData.edgedRecently)   chunkBotData.RefreshEdges();
                stitchChunkPair(chunk, Vector3.down, chunkBotData, Vector3.up);
            }
            if (this.chunkExists(rightX, rightY, out GameObject chunkRight)) {
                var chunkRightData = (Chunk) chunkRight.GetComponent<Chunk>();
                if (!chunkRightData.edgedRecently) chunkRightData.RefreshEdges();
                stitchChunkPair(chunk, Vector3.right, chunkRightData, Vector3.left);
            }

            chunk.edgedRecently = false;
        }
    }

    public void stitchChunkPair(Chunk chunk1, Vector3 chunkEdge1, Chunk chunk2, Vector3 chunkEdge2) {
        // TODO: corners repeated twice?

        chunk1.RefreshEdges();
        chunk2.RefreshEdges();

        var edge1 = chunk1.GetMeshEdge(chunkEdge1);
        var edge2 = chunk2.GetMeshEdge(chunkEdge2);

        //edge1 = edge1.ToDictionary(x => x.Key, x => x.Value + new Vector3(0, 0, 1));
        Debug.Log("ugh");
        //edge2 = edge2.ToDictionary(x => x.Key, x => x.Value + new Vector3(0, 0, 1));

        var edgeShared = new Dictionary<int, Vector3>();

        foreach (var edge1Vtx in edge1){
            var edge2Vtx = edge1[edge1Vtx.Key];

            // take average of vals
            var avgVtx = (edge1Vtx.Value + edge1Vtx.Value) / 2;

            edgeShared[edge1Vtx.Key] = avgVtx;
        }

        Debug.Log("ugh2");

        Debug.Log("ugh");

        chunk1.UpdateEdge(edgeShared);
        chunk2.UpdateEdge(edgeShared);
    }
}