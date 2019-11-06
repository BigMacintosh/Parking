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
    
    public int size = 10;
    
    private int GetVertexStartIndexFor(int x, int y) {
        var idx = x * 6 + y * 6 * size;
        return idx >= 0 && idx < vertices.Length ? idx : -1;
    }

    private void test() {
        Debug.Assert(GetVertexStartIndexFor(0, 0) == 0);
        Debug.Assert(GetVertexStartIndexFor(1, 0) == 6);
        Debug.Assert(GetVertexStartIndexFor(2, 0) == 12);
        Debug.Assert(GetVertexStartIndexFor(0, 1) == size * 6);
        Debug.Assert(GetVertexStartIndexFor(1, 2) == size * 6 + 1 * size);
    }
    
    private void EditVertexAt(int idx, Func<float, float> f) {
        if (idx == 0) {
            Debug.Log("Editing vertex 0");
        }

        if(idx >= 0 && idx <= vertices.Length) {
            var val = f(vertices[idx][2]);
            vertices[idx][2] = val;
        }
    }

    private void EditVerticesFor(int x, int y, Func<float, float> f) {
        var topRight = GetVertexStartIndexFor(x, y);
        EditVertexAt(topRight + 0, f);
        
        var topLeft = GetVertexStartIndexFor(x - 1, y);
        EditVertexAt(topLeft + 2, f);
        EditVertexAt(topLeft + 3, f);
        
        var bottomLeft = GetVertexStartIndexFor(x - 1, y - 1);
        EditVertexAt(bottomLeft + 5, f);
        
        var bottomRight = GetVertexStartIndexFor(x, y - 1);
        EditVertexAt(bottomRight + 1, f);
        EditVertexAt(bottomRight + 4, f);
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

        test();
        
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

        // for (var y = 0; y < size; y++) {
        //     for (var x = 0; x < size; x++) {
        //         SetVerticesFor(x, y, UnityEngine.Random.Range(0.0f, 3.0f));
        //     }
        // }
    }

    int c= 0;
    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 30 == 0) {
            Debug.Log("Round " + c);
            for (var y = 0; y <= size; y++) {
                for (var x = 0; x <= size; x++) {
                    //SetVerticesFor(x, y, UnityEngine.Random.Range(0.0f, 3.0f));
                    IncreaseVerticesFor(x, y, 0.5f);
                }
            }

            UpdateMesh();
            c++;
        }
    }
}
