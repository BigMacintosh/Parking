using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(Chunkster))]
public class ChunksterInspector : Editor {
    enum Tool { None, Build, Destroy }
    private int selectedTool = 0;

    private void onEnable() {}
    private void OnSceneGUI() {
        // get chosen game object
        Chunkster t = target as Chunkster;

        // this has O(n^2) and i am SORRY
        foreach (Transform child in t.transform) {
            var chunk = (Chunk) child.GetComponent<Chunk>();

            switch((Tool) selectedTool) {
                case Tool.Build:
                        this.handleToolBuild(t, child, chunk);
                        break;
                    case Tool.Destroy:
                        this.handleToolDestroy(t, child, chunk);
                        break;
                    default:
                        break;
            }
        }
    }

    private void handleToolBuild(Chunkster t, Transform child, Chunk chunk) {
        var neighbours = t.getNeighbourChunks(chunk.chunkX, chunk.chunkY);

        foreach (var n in neighbours) {
            if (!t.chunkExists(n.Item1, n.Item2, out _)) {
                (int worldX, int worldZ) = t.chunkCoordToWorld(n.Item1, n.Item2);
                var size                 = t.baseChunk.GetComponent<Renderer>().bounds.size * 0.5f;
                var pos                  = new Vector3(worldX, 0, worldZ);

                // handling button presses
                if (Handles.Button(pos, child.transform.rotation, size.x,  size.x, Handles.RectangleHandleCap)) {
                    t.createNewChunk(n.Item1, n.Item2);
                }
            }
        }
    }

    private void handleToolDestroy(Chunkster t, Transform child, Chunk chunk) {
        (int worldX, int worldZ) = t.chunkCoordToWorld(chunk.chunkX, chunk.chunkY);
        var size                 = t.baseChunk.GetComponent<Renderer>().bounds.size * 0.5f;
        var pos                  = new Vector3(worldX, 0, worldZ);

        // handling button presses
        if (Handles.Button(pos, child.transform.rotation, size.x,  size.x, Handles.RectangleHandleCap)) {
            t.deleteChunk(chunk.chunkX, chunk.chunkY);
        }
    }

    public override void OnInspectorGUI() {
        var ToggleButtonStyleNormal = "button";
        var ToggleButtonStyleToggled = new GUIStyle("button");
        ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;

        DrawDefaultInspector();
        Chunkster t = target as Chunkster;

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Edit Mode: ");
        int selected = GUILayout.Toolbar(selectedTool, Enum.GetNames(typeof(Tool)));
        selectedTool = selected;
        EditorGUILayout.HelpBox("Destorying a chunk is permanent!", MessageType.Warning);

        EditorGUILayout.EndVertical();
        
        if (GUILayout.Button("Stitch Seams")) {
            t.stitchChunks();
        }
    }
}