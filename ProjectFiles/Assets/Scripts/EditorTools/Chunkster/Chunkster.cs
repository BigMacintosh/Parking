using System.Collections.Generic;
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

        createNewChunk(0, 1);
        createNewChunk(1, 1);
    }

    [ContextMenu("Join Seams")]
    private void JoinSeams() {}


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

    // converts a chunkX, chunkY to Unity's world coordinates
    // (note the worldY should actually be used in for the z in a transform)
    (int, int) chunkCoordToWorld(int chunkX, int chunkY) {
        var size = baseChunk.GetComponent<Renderer>().bounds.size;
        Debug.Log(size);
        return ((int) (chunkX * size.x),
                (int) (chunkY * size.z));
    }
    
    // get chunk child from chunk x, y
    GameObject getChunk(int chunkX, int chunkY) {
        foreach (Transform child in this.gameObject.transform) {
            if (child.gameObject.name == this.getChunkId(chunkX, chunkY)) {
                return child.gameObject;
            }
        }
        return null;
    }

    bool chunkExists(int chunkX, int chunkY, out GameObject chunk) {
        foreach (Transform child in this.gameObject.transform) {
            if (child.gameObject.name == this.getChunkId(chunkX, chunkY)) {
                chunk = child.gameObject;
                return true;
            }
        }
        chunk = null;
        return false;
    }

    GameObject createNewChunk(int chunkX, int chunkY) {
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
            return chunkInstantiated;
        }
    }

    void deleteChunk(int chunkX, int chunkY) {
        if (this.chunkExists(chunkX, chunkY, out GameObject chunk)) {
            Destroy(chunk);
        } else {
            Debug.LogWarning("Tried to delete chunk at " + (chunkX, chunkY) + " but it doesn't exist!");
        }
    }
}