using System;
using EditorTools.Chunkster;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Chunkster))]
public class ChunksterInspector : Editor {
    private int selectedTool;

    private void onEnable() { }

    private void OnSceneGUI() {
        // get chosen game object
        var t = target as Chunkster;

        // this has O(n^2) and i am SORRY
        foreach (Transform child in t.transform) {
            var chunk = child.GetComponent<Chunk>();

            switch ((Tool) selectedTool) {
                case Tool.Build:
                    handleToolBuild(t, child, chunk);
                    break;
                case Tool.Destroy:
                    handleToolDestroy(t, child, chunk);
                    break;
            }
        }
    }

    private void handleToolBuild(Chunkster t, Transform child, Chunk chunk) {
        var neighbours = t.GetNeighbourChunks(chunk.chunkX, chunk.chunkY);

        foreach (var n in neighbours) {
            if (!t.ChunkExists(n.Item1, n.Item2, out _)) {
                (var worldX, var worldZ) = t.ChunkCoordToWorld(n.Item1, n.Item2);
                var size = t.baseChunk.GetComponent<Renderer>().bounds.size * 0.5f;
                var pos  = new Vector3(worldX, 0, worldZ);

                // handling button presses
                if (Handles.Button(pos, child.transform.rotation, size.x, size.x, Handles.RectangleHandleCap)) {
                    t.CreateNewChunk(n.Item1, n.Item2);
                }
            }
        }
    }

    private void handleToolDestroy(Chunkster t, Transform child, Chunk chunk) {
        (var worldX, var worldZ) = t.ChunkCoordToWorld(chunk.chunkX, chunk.chunkY);
        var size = t.baseChunk.GetComponent<Renderer>().bounds.size * 0.5f;
        var pos  = new Vector3(worldX, 0, worldZ);

        // handling button presses
        if (Handles.Button(pos, child.transform.rotation, size.x, size.x, Handles.RectangleHandleCap)) {
            t.DeleteChunk(chunk.chunkX, chunk.chunkY);
        }
    }

    public override void OnInspectorGUI() {
        var ToggleButtonStyleNormal  = "button";
        var ToggleButtonStyleToggled = new GUIStyle("button");
        ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;

        DrawDefaultInspector();
        var t = target as Chunkster;

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Edit Mode: ");
        var selected = GUILayout.Toolbar(selectedTool, Enum.GetNames(typeof(Tool)));
        selectedTool = selected;
        EditorGUILayout.HelpBox("Destorying a chunk is permanent!", MessageType.Warning);

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Stitch Seams")) {
            t.StitchChunks();
        }
    }

    private enum Tool {
        None,
        Build,
        Destroy
    }
}