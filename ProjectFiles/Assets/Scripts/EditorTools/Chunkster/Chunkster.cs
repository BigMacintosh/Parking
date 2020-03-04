using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class Chunkster : MonoBehaviour {
    enum Direction { Up, Down, Left, Right }

    // our base chunk
    public GameObject baseChunk;
    public Dictionary<(int, int), GameObject> chunks;
    // folder in which to save chunk prefabs
    public string chunkFolderPath;

    (int, int) addDirection(int chunkX, int chunkY, Direction dir) {
        switch (dir) {
            case Direction.Up:    return (chunkX,     chunkY + 1);
            case Direction.Down:  return (chunkX,     chunkY - 1);
            case Direction.Left:  return (chunkX - 1, chunkY    );
            case Direction.Right: return (chunkX + 1, chunkY    );
            default:              return (chunkX    , chunkY    );
        }
    }

    (int, int) chunkCoordToWorld(int chunkX, int chunkY) {
        var size = baseChunk.GetComponent<Renderer>().bounds.size;
        return ((int) (chunkX * size.x),
                (int) (chunkY * size.y));
    }


    [ContextMenu("Join Seams")]
    private void JoinSeams() {}


    // private GameObject instantiateChunk(int chunkX, int chunkY) {
    //     if (chunks.ContainsKey((chunkX, chunkY))) {
    //         (int worldX, int worldY)  = chunkCoordToWorld(chunkX, chunkY);
    //         GameObject chunkNew       = Instantiate(baseChunk,
    //                                                 new Vector3(worldX, worldY, 0),
    //                                                 baseChunk.transform.rotation) as GameObject;
    //         chunkNew.name             = "baseChunk_(" + chunkX + chunkY + ")";
    //         chunkNew.transform.parent = gameObject.transform;

    //         return chunkNew;
    //     } else {
    //         Debug.Log("Tried to instantiate chunk which doesn't exist for coordinates (" + chunkX + ", " + chunkY + ")");
    //         return null;
    //     }
    // }

    // private void addNewChunk(int chunkX, int chunkY) {
    //     if (chunks.ContainsKey((chunkX, chunkY))) {
    //         Debug.Log("Tried to create new chunk at (" + chunkX + ", " + chunkY + ") but one already exists!");
    //     } else {
    //         var chunkNew = instantiateChunk(chunkX, chunkY);
    //         chunks.Add((chunkX, chunkY), chunkNew);
    //     }
    // }

    void Awake() {
        // init. chunk dict.
        chunks = new Dictionary<(int, int), GameObject>();        

        // init. centre chunk
        // addNewChunk(0, 0);
        // addNewChunk(1, 0);
        // addNewChunk(0, 0);
        createNewChunk(0, 0);

        Debug.Log("dog\n");
    }

    private string getChunkId(int chunkX, int chunkY) {
        return this.baseChunk.name + "_" + chunkX + "_" + chunkY;
    }

    private string getChunkPrefabPath(int chunkX, int chunkY) {
        return this.chunkFolderPath + this.getChunkId(chunkX, chunkY);
    }

    // create a new chunk at given coordinates
    // we must instantiate the chunk when created; we cannot
    // copy the base prefab without doing so
    void createNewChunk(int chunkX, int chunkY) {
        // check if chunk exists in dict or if chunk exists in files
        GameObject chunkLoaded = this.loadChunk(chunkX, chunkY);
        if (this.chunks.ContainsKey((chunkX, chunkY)) || chunkLoaded != null) {
            Debug.LogWarning("Tried to create new chunk at " + (chunkX, chunkY) + ", but one already exists!");
        } else {
            // instantiate the object
            var chunkInstantiated = this.instantiateChunk(this.baseChunk, chunkX, chunkY);
            // add it to the dict
            this.chunks.Add((chunkX, chunkY), chunkInstantiated);
            // let's save it whilst we're at it
            this.saveChunk(chunkX, chunkY);
        }
    }

    // puts a loaded prefab chunk into the world
    GameObject instantiateChunk(GameObject chunk, int chunkX, int chunkY) {
        // the loaded prefab should have the correct values
        // but let's not make any assumptions.
        (int worldX, int worldY)           = this.chunkCoordToWorld(chunkX, chunkY);
        GameObject chunkInstantiated       = Instantiate(chunk,
                                                         new Vector3(worldX, worldY, 0),
                                                         chunk.transform.rotation) as GameObject;
        chunkInstantiated.name             = this.getChunkId(chunkX, chunkY);
        chunkInstantiated.transform.parent = gameObject.transform;
        return chunkInstantiated;
    }

    // loads a chunk prefab from resources and returns it
    GameObject loadChunk(int chunkX, int chunkY) {
        GameObject chunkLoaded = Resources.Load(this.getChunkPrefabPath(chunkX, chunkY)) as GameObject;
        if (chunkLoaded == null)
            Debug.LogWarning("Tried to load non-existing chunk at " + (chunkX, chunkY) + " from disk!");
        return chunkLoaded;
    }

    void loadChunkIntoDict(int chunkX, int chunkY) {
        GameObject chunkLoaded = loadChunk(chunkX, chunkY);
        if (chunkLoaded != null) {
            // TODO: what if chunk already (for some reason) exists?
            this.chunks.Add((chunkX, chunkY), chunkLoaded);
        }

        // delete already instantiated chunk
        foreach (Transform child in transform) {
            if (child.name == getChunkId(chunkX, chunkY)) {
                
            }
        }
    }

    // saves a chunk instance from dict -> resources
    void saveChunk(int chunkX, int chunkY) {
        if (this.chunks.TryGetValue((chunkX, chunkY), out GameObject chunkToSave)) {
            // TODO: what if we have the same pathname as another prefab?
            PrefabUtility.SaveAsPrefabAssetAndConnect(chunkToSave,
                                                      this.getChunkPrefabPath(chunkX, chunkY),
                                                      InteractionMode.AutomatedAction);
        } else {
            Debug.LogWarning("Tried to save non-existing chunk at " + (chunkX, chunkY) + " to disk!");
        }
    }

    void loadChunks() {
        foreach ((int, int) loc in this.chunks.Keys) {
            var loadedChunk = this.loadChunk(loc.Item1, loc.Item2);
            this.instantiateChunk(loadedChunk, loc.Item1, loc.Item2);
        }
    }

    void saveChunks() {
        foreach ((int, int) loc in this.chunks.Keys) {
            this.saveChunk(loc.Item1, loc.Item2);
        }
    }

    void deleteChunk() {}
}