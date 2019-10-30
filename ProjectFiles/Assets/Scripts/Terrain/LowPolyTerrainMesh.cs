using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowPolyTerrainMesh : MonoBehaviour
{
    // a lot from this https://catlikecoding.com/unity/tutorials/procedural-grid/
    
    private MeshFilter mf;
    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;
    
    public int size = 10;
    
    private int GetVertexStartIndexFor(int x, int y) {
        var idx = x * 6 + y * 6 * size;
        return idx >= 0 && idx < size * size * 6 ? idx : -1;
    }
    
    private void IncreaseVerticesFor(int x, int y, float amnt) {
        var topRight = GetVertexStartIndexFor(x, y);
        Debug.Log(topRight);
        if (topRight != -1) {
            vertices[topRight + 0][2] += amnt;
        }
        
        var topLeft = GetVertexStartIndexFor(x - 1, y);
        Debug.Log(topLeft);
        if (topLeft != -1) {
            vertices[topLeft + 2][2] += amnt;
            vertices[topLeft + 3][2] += amnt;
        }
        
        var bottomLeft = GetVertexStartIndexFor(x - 1, y - 1);
        Debug.Log(bottomLeft);
        if (bottomLeft != -1) {
            vertices[bottomLeft + 5][2] += amnt;
        }
        
        var bottomRight = GetVertexStartIndexFor(x, y - 1);
        Debug.Log(bottomRight);
        if (bottomRight != -1) {
            vertices[bottomRight + 1][2] += amnt;
            vertices[bottomRight + 4][2] += amnt;
        }
    }
    
    private void SetVerticesFor(int x, int y, float amnt) {
        var topRight = GetVertexStartIndexFor(x, y);
        if (topRight != -1) {
            vertices[topRight + 0][2] = amnt;
        }
        
        var topLeft = GetVertexStartIndexFor(x - 1, y);
        if (topLeft != -1) {
            vertices[topLeft + 2][2] = amnt;
            vertices[topLeft + 3][2] = amnt;
        }
        
        var bottomLeft = GetVertexStartIndexFor(x - 1, y - 1);
        if (bottomLeft != -1) {
            vertices[bottomLeft + 5][2] = amnt;
        }
        
        var bottomRight = GetVertexStartIndexFor(x, y - 1);
        if (bottomRight != -1) {
            vertices[bottomRight + 1][2] = amnt;
            vertices[bottomRight + 4][2] = amnt;
        }
    }
    
    private void Generate(ref Mesh mesh)
    {
        /*vertices = new Vector3[(int) Mathf.Pow((size + 1), 2)];
        var i = 0;
        for (var y = 0; y < size + 1; y++) {
            for (var x = 0; x < size + 1; x++) {
                vertices[i] = new Vector3(x, y);
                i++;
            }
        }
        vertices[10][2] = -2;
        vertices[20][2] = -2;
        vertices[50][2] = -2;
        vertices[100][2] = -3;
        mesh.vertices = vertices;
        
        triangles = new int[size * size * 6];
        for (int ti = 0, vi = 0, y = 0; y < size; y++, vi++) {
			for (int x = 0; x < size; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 1] = vi + size + 1;
				triangles[ti + 2] = vi + 1;
				triangles[ti + 3] = vi + 1;
				triangles[ti + 4] = vi + size + 1;
				triangles[ti + 5] = vi + size + 2;
				//triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				//triangles[ti + 4] = triangles[ti + 1] = vi + size + 1;
				//triangles[ti + 5] = vi + size + 2;
			}
		}
        mesh.triangles = triangles;
        mesh.RecalculateNormals();*/
        
        //vertices = new Vector3[(int) Mathf.Pow((size + 1), 2)];
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
        
        /*vertices[20][2] = -2;
        vertices[50][2] = -2;
        vertices[150][2] = -2;
        vertices[210][2] = -2;*/
        
        /*IncreaseVerticesFor(0, 0, -0.5f);
        IncreaseVerticesFor(1, 1, -2);
        IncreaseVerticesFor(5, 5, -0.2f);
        IncreaseVerticesFor(6, 5, -0.5f);*/
        
        
        
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

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 30 == 0) {
        for (var y = 0; y < size; y++) {
            for (var x = 0; x < size; x++) {
                SetVerticesFor(x, y, Random.Range(0.0f, 2.0f));
            }
        }
        
        var mf = GetComponent<MeshFilter>();
        var mesh = mf.mesh;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        }
        /*Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        
        for (var i = 0;i < vertices.Length; i++) {
            vertices[i] += normals[i] * Mathf.Sin(Time.time);
        }
        
        mesh.vertices = vertices;*/
    }
}
