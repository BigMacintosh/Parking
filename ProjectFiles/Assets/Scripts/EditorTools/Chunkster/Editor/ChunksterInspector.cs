using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Chunkster))]
public class ChunksterInspector : Editor {
    private void onEnable() {}
    private void OnSceneGUI() {
        // get chosen game object
        Chunkster t = target as Chunkster;

        // this has O(n^2) and i am SORRY
        foreach (Transform child in t.transform) {
            var chunk = (Chunk) child.GetComponent<Chunk>();
            var neighbours = t.getNeighbourChunks(chunk.chunkX, chunk.chunkY);

            foreach (var n in neighbours) {
                if (!t.chunkExists(n.Item1, n.Item2, out _)) {
                    var verts = this.getVertsForChunk(t, n.Item1, n.Item2);
                    Handles.DrawSolidRectangleWithOutline(verts, new Color(0.5f, 0.5f, 0.5f, 0.1f), new Color(0, 0, 0, 1)); 
                }
            }
        }
    }

    public override void OnInspectorGUI() {}

    private Vector3[] getVertsForChunk(Chunkster t, int chunkX, int chunkY) {
        var size                 = t.baseChunk.GetComponent<Renderer>().bounds.size * 0.5f;
        (int worldX, int worldY) = t.chunkCoordToWorld(chunkX, chunkY);

        Vector3[] verts = new Vector3[]
        {
            new Vector3(worldX - size.x, t.transform.position.y, worldY - size.z),
            new Vector3(worldX - size.x, t.transform.position.y, worldY + size.z),
            new Vector3(worldX + size.x, t.transform.position.y, worldY + size.z),
            new Vector3(worldX + size.x, t.transform.position.y, worldY - size.z)
        };

        return verts;
    }
}