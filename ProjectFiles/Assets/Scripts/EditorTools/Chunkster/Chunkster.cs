using System.Collections.Generic;
using UnityEngine;

public class Chunkster : MonoBehaviour {
    // our base chunk
    public GameObject baseChunk;

    public Dictionary<(int, int), GameObject> chunks;

    [ContextMenu("Join Seams")]
    private void JoinSeams() {}

    (int, int) chunkCoordToWorld(int chunkX, int chunkY) {
        var size = baseChunk.GetComponent<Renderer>().bounds.size;
        return ((int) (chunkX * size.x),
                (int) (chunkY * size.y));
    }

    private void addChunk(int chunkX, int chunkY) {
        (int worldX, int worldY) = chunkCoordToWorld(chunkX, chunkY);
        GameObject chunkNew       = Instantiate(baseChunk,
                                                new Vector3(worldX, worldY, 0),
                                                baseChunk.transform.rotation) as GameObject;
        chunkNew.name             = "baseChunk_(" + chunkX + chunkY + ")";
        chunkNew.transform.parent = gameObject.transform;

        chunks.Add((chunkX, chunkY), chunkNew);
    }

    void Awake() {
        // init. chunk dict.
        chunks = new Dictionary<(int, int), GameObject>();

        // init. centre chunk
        addChunk(0, 0);
        addChunk(1, 0);

        Debug.Log("dog\n");
    }
}