using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.U2D;

public class Paver : MonoBehaviour
{
    [SerializeField] private BezierSpline spline;
    [SerializeField] private float divisions;
    [SerializeField] private float width;
    [SerializeField] private float thickness;
    [SerializeField] private bool signpost = false;
    [SerializeField] private Material surface;
    
    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Vector2> uv;
    
    [ContextMenu("Pave Road")]
    void PaveRoad() 
    {
        Debug.Log("Paving Road");
        Awake();
    }
    
    private void Awake()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uv = new List<Vector2>();
        
        Mesh mesh = new Mesh();
        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();

        populateVertices();
        populateTris();

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        filter.mesh = mesh;
        renderer.material = surface;
        mesh.RecalculateNormals();

        MeshCollider collider = gameObject.AddComponent<MeshCollider>();




    }

    private void populateVertices()
    {
        float stepSize = 1f / (divisions);
        
        for (int p = 0; p < divisions; p++)
        {
            Vector3 roadPoint = spline.GetPoint(p * stepSize);
            
            float velX = spline.GetDirection(p * stepSize).x;
            float velZ = spline.GetDirection(p * stepSize).z;
            float velMag =  (new Vector2(velX, velZ)).magnitude;

            if (signpost)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = roadPoint;
                sphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            float alpha = (Mathf.PI /2) - Mathf.Asin((velZ)/velMag);

            if (velX < 0)
            {
                alpha *= -1;
            }

            Vector3 rightPoint = new Vector3(roadPoint.x + ((width/2)*Mathf.Cos(alpha)),roadPoint.y,roadPoint.z - ((width/2)*Mathf.Sin(alpha))) - this.transform.position;
            Vector3 rightLowerPoint = rightPoint - new Vector3(0f, thickness, 0f);
            
            Vector3 leftPoint = new Vector3(roadPoint.x - ((width/2)*Mathf.Cos(alpha)),roadPoint.y,roadPoint.z + ((width/2)*Mathf.Sin(alpha))) - this.transform.position;
            Vector3 leftLowerPoint = leftPoint - new Vector3(0f, thickness, 0f);

            vertices.Add(leftPoint);
            uv.Add(new Vector2(0f,1 * (p%2)));
            vertices.Add(leftLowerPoint);
            uv.Add(new Vector2(0.9f,0f));
            vertices.Add(rightPoint);
            uv.Add(new Vector2(1f,1 * (p%2)));
            vertices.Add(rightLowerPoint);
            uv.Add(new Vector2(0f,0.9f));

            if (signpost)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = leftPoint + this.transform.position;
                cube.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
            
                cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = rightPoint + this.transform.position;
                cube.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
            }
        }
    }

    private void populateTris()
    {
        //Front surface
        triangles.Add(0);
        triangles.Add(2);
        triangles.Add(1);
            
        triangles.Add(2);
        triangles.Add(3);
        triangles.Add(1);
        
        for (int v = 0; v < vertices.Count - 4; v+=4)
        {
            //Top surface
            triangles.Add(v);
            triangles.Add(v+4);
            triangles.Add(v+6);

            triangles.Add(v + 2);
            triangles.Add(v);
            triangles.Add(v+6);
            
            //Left Surface
            triangles.Add(v);
            triangles.Add(v+1);
            triangles.Add(v+5);
            
            triangles.Add(v+5);
            triangles.Add(v+4);
            triangles.Add(v);
            
            //Right Surface
            triangles.Add(v+2);
            triangles.Add(v+6);
            triangles.Add(v+7);
            
            triangles.Add(v+2);
            triangles.Add(v+7);
            triangles.Add(v+3);

        }
        
        
        //Back Surface
        triangles.Add(vertices.Count-1);
        triangles.Add(vertices.Count-2);
        triangles.Add(vertices.Count-3);
        
        triangles.Add(vertices.Count-2);
        triangles.Add(vertices.Count-4);
        triangles.Add(vertices.Count-3);
    }
    
}
