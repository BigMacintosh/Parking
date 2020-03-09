using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class Chunkster : MonoBehaviour {
    enum Direction { Up, Down, Left, Right }

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

    public void stitchChunks(int chunkX1, int chunkY1, int chunkX2, int chunkY2) {
        var chunk1 = (Chunk) this.getChunk(chunkX1, chunkY1).GetComponent<Chunk>();
        var chunk2 = (Chunk) this.getChunk(chunkX2, chunkY2).GetComponent<Chunk>();

        
    }
}