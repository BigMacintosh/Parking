using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowPolyTerrainMesh : MonoBehaviour
{
    // a lot from this https://catlikecoding.com/unity/tutorials/procedural-grid/
    
    private MeshFilter mf;
    public Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;
    
    public int size = 3;
    
    // TODO: Write a proper Unity test class
    
//    private void test() {
//        Debug.Assert(GetVertexStartIndexFor(0, 0) == 0);
//        Debug.Assert(GetVertexStartIndexFor(1, 0) == 6);
//        Debug.Assert(GetVertexStartIndexFor(2, 0) == 12);
//        Debug.Assert(GetVertexStartIndexFor(0, 1) == size * 6);
//        Debug.Assert(GetVertexStartIndexFor(1, 2) == 1 * 6 + 2 * 6 * size);
//        Debug.Log(GetVertexStartIndexFor(1, 2));
//    }
    
    private int? GetVertexStartIndexFor(int x, int y) {
        var idx = x * 6 + y * 6 * size;
        return idx >= 0 && idx < vertices.Length ? idx : (int?) null;
    }
    
    private void EditVertexAt(int idx, Func<float, float> f) {
        if(idx >= 0 && idx <= vertices.Length) {
            var val = f(vertices[idx][2]);
            vertices[idx][2] = val;
        }
    }

    private void EditVerticesFor(int x, int y, Func<float, float> f) {
        if (x < size)
        {
            var topRight = GetVertexStartIndexFor(x, y);
            if (topRight.HasValue)
            {
                int i = topRight.Value + 0;
                EditVertexAt(i, f);
            }

            var bottomRight = GetVertexStartIndexFor(x, y - 1);
            if (bottomRight.HasValue)
            {
                int i = bottomRight.Value + 1;
                EditVertexAt(i, f);
                i = bottomRight.Value + 4;
                EditVertexAt(i, f);
            }
        }

        if (x > 0)
        {
            var topLeft = GetVertexStartIndexFor(x - 1, y);
            if (topLeft.HasValue)
            {
                int i = topLeft.Value + 2;
                EditVertexAt(i, f);
                i = topLeft.Value + 3;
                EditVertexAt(i, f);
            }


            var bottomLeft = GetVertexStartIndexFor(x - 1, y - 1);
            if (bottomLeft.HasValue)
            {
                int i = bottomLeft.Value + 5;
                EditVertexAt(i, f);
            }
        }

        
    }

    private void SetVerticesFor(int x, int y, float amnt) {
        EditVerticesFor(x, y, (z => amnt));
    }
    
    private void IncreaseVerticesFor(int x, int y, float amnt) {
        EditVerticesFor(x, y, (z => z + amnt));
    }
    
    private void Generate(ref Mesh mesh)
    {        
        vertices = new Vector3[size * size * 6];
        triangles = new int[size * size * 6];
        
        var i = 0;
        // where x, y = coordinates of tile
        for (var y = 0; y < size; y++) {
            for (var x = 0; x < size; x++) {
                vertices[i] = new Vector3(x, y);
                triangles[i] = i;
                vertices[i + 1] = new Vector3(x, y + 1);
                triangles[i + 1] = i + 1;
                vertices[i + 2] = new Vector3(x + 1, y);
                triangles[i + 2] = i + 2;
                
                vertices[i + 3] = new Vector3(x + 1, y);
                triangles[i + 3] = i + 3;
                vertices[i + 4] = new Vector3(x, y + 1);
                triangles[i + 4] = i + 4;
                vertices[i + 5] = new Vector3(x + 1, y + 1);
                triangles[i + 5] = i + 5;
                
                i += 6;
            }
        }
        UpdateMesh();
    }
    
    private void UpdateMesh() {
        var mf = GetComponent<MeshFilter>();
        var mesh = mf.mesh;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        var mesh = new Mesh();
        Generate(ref mesh);
        
        mf = GetComponent<MeshFilter>();
        mf.mesh = mesh;
    }
    
    void Update()
    {
        if (Time.frameCount % 30 == 0) {
            for (var y = 0; y <= size; y++) {
                for (var x = 0; x <= size; x++) {
                    SetVerticesFor(x, y, UnityEngine.Random.Range(0.0f, 5.0f));
//                    IncreaseVerticesFor(x, y, 0.5f);
                }
            }
            UpdateMesh();
        }
    }
}
