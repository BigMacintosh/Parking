using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public interface IPaver {
    List<Bounds> GetDivisionBoundingBoxes();
}

public class Paver : MonoBehaviour, IPaver {
    public List<Vector3> LeftSignPosts  { get; private set; }
    public List<Vector3> RightSignPosts { get; private set; }
    public List<Vector3> RoadPoints     { get; private set; }
    
    [SerializeField] private float        divisions;
    [SerializeField] private float        maxGroundDistance;
    [SerializeField] private float        raiseAboveGround;
    [SerializeField] private bool         signpost;
    [SerializeField] private BezierSpline spline;
    [SerializeField] private Material     surface;
    [SerializeField] private float        thickness;
    [SerializeField] private float        width;

    private List<int>     triangles;
    private List<Vector2> uv;
    private List<Vector3> vertices;

    public void PaveRoad() {
        Pave();
    }

    private void Pave() {
        vertices  = new List<Vector3>();
        triangles = new List<int>();
        uv        = new List<Vector2>();
        LeftSignPosts = new List<Vector3>();
        RightSignPosts = new List<Vector3>();
        RoadPoints = new List<Vector3>();

        var mesh = new Mesh();

        if (!gameObject.TryGetComponent(out MeshRenderer roadRenderer)) {
            roadRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        if (!gameObject.TryGetComponent(out MeshFilter roadFilter)) {
            roadFilter = gameObject.AddComponent<MeshFilter>();
        }

        if (!gameObject.TryGetComponent(out MeshCollider roadCollider)) {
            roadCollider = gameObject.AddComponent<MeshCollider>();
        }

        // get rid of old mesh before recalculating so it doesn't see itself as the floor
        roadCollider.sharedMesh = null;

        PopulateVertices();
        PopulateTris();

        mesh.Clear();
        mesh.vertices         = vertices.ToArray();
        mesh.triangles        = triangles.ToArray();
        mesh.uv               = uv.ToArray();
        roadFilter.mesh       = mesh;
        roadRenderer.material = surface;
        mesh.RecalculateNormals();
        roadCollider.sharedMesh = mesh;
    }

    private void PopulateVertices() {
        var stepSize = 1f / divisions;

        for (var p = 0; p < divisions; p++) {
            var roadPoint = spline.GetPoint(p * stepSize);

            var velX   = spline.GetDirection(p * stepSize).x;
            var velZ   = spline.GetDirection(p * stepSize).z;
            var velMag = new Vector2(velX, velZ).magnitude;

            if (signpost) {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position   = roadPoint;
                sphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            var alpha = Mathf.PI / 2 - Mathf.Asin(velZ / velMag);

            if (velX < 0) {
                alpha *= -1;
            }


            var rightPoint = new Vector3(roadPoint.x + width / 2 * Mathf.Cos(alpha), roadPoint.y,
                                         roadPoint.z - width / 2 * Mathf.Sin(alpha)) - transform.position;
            var signRightPoint = rightPoint;

            if (Physics.Raycast(new Ray(rightPoint + transform.position, Vector3.down), out var rightHit,
                                maxGroundDistance)) {
                rightPoint.y -= rightHit.distance - raiseAboveGround;
                if (signpost) {
                    var cap     = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    var signPos = rightPoint + transform.position;
                    signPos.y                -= rightHit.distance;
                    cap.transform.position   =  signPos;
                    cap.transform.localScale =  new Vector3(0.5f, 0.5f, 0.5f);
                }
            }

            var rightLowerPoint = rightPoint - new Vector3(0f, thickness, 0f);

            var leftPoint = new Vector3(roadPoint.x - width / 2 * Mathf.Cos(alpha), roadPoint.y,
                                        roadPoint.z + width / 2 * Mathf.Sin(alpha)) - transform.position;
            var signLeftPoint = leftPoint;

            if (Physics.Raycast(new Ray(leftPoint + transform.position, Vector3.down), out var leftHit,
                                maxGroundDistance)) {
                leftPoint.y -= leftHit.distance - raiseAboveGround;
                if (signpost) {
                    var cap     = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    var signPos = leftPoint + transform.position;
                    signPos.y                -= leftHit.distance;
                    cap.transform.position   =  signPos;
                    cap.transform.localScale =  new Vector3(0.5f, 0.5f, 0.5f);
                }
            }

            var leftLowerPoint = leftPoint - new Vector3(0f, thickness, 0f);

            var midPoint = roadPoint - transform.position;

            RaycastHit midHit;
            if (Physics.Raycast(new Ray(midPoint + transform.position, Vector3.down), out midHit,
                                maxGroundDistance)) {
                midPoint.y -= midHit.distance - raiseAboveGround;
            }

            var midLowerPoint = midPoint - new Vector3(0f, thickness, 0f);


            vertices.Add(leftPoint);
            uv.Add(new Vector2(0f, 1 * (p % 2)));
            vertices.Add(leftLowerPoint);
            uv.Add(new Vector2(0.9f, 0f));
            
            vertices.Add(midPoint);
            uv.Add(new Vector2(0.5f, 1 * (p %2)));
            vertices.Add(midLowerPoint);
            uv.Add(new Vector2(0.5f, 1 * (p %2)));
            
            vertices.Add(rightPoint);
            uv.Add(new Vector2(1f, 1 * (p % 2)));
            vertices.Add(rightLowerPoint);
            uv.Add(new Vector2(0f, 0.9f));

            if (signpost) {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position   = signLeftPoint + transform.position;
                cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                cube                      = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position   = signRightPoint + transform.position;
                cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
            
            // Collect points for the road detection.
            LeftSignPosts.Add(signLeftPoint);
            RightSignPosts.Add(signRightPoint);
            RoadPoints.Add(midPoint);
        }
    }

    private void PopulateTris() {
        //Front surfaces
        //left half
        triangles.Add(0);
        triangles.Add(2);
        triangles.Add(1);

        triangles.Add(2);
        triangles.Add(3);
        triangles.Add(1);
        
        //Right half
        triangles.Add(2);
        triangles.Add(4);
        triangles.Add(3);

        triangles.Add(4);
        triangles.Add(5);
        triangles.Add(3);

        for (var v = 0; v < vertices.Count - 6; v += 6) {
            //Top surface
            //Left half
            triangles.Add(v);
            triangles.Add(v + 6);
            triangles.Add(v + 8);

            triangles.Add(v + 2);
            triangles.Add(v);
            triangles.Add(v + 8);
            
            //Right half
            triangles.Add(v+2);
            triangles.Add(v + 8);
            triangles.Add(v + 10);

            triangles.Add(v + 4);
            triangles.Add(v + 2);
            triangles.Add(v + 10);

            //Left Road Surface
            triangles.Add(v);
            triangles.Add(v + 1);
            triangles.Add(v + 6);

            triangles.Add(v + 1);
            triangles.Add(v + 7);
            triangles.Add(v+  6);

            //Right Road Surface
            triangles.Add(v + 10);
            triangles.Add(v + 5);
            triangles.Add(v + 4);

            triangles.Add(v + 10);
            triangles.Add(v + 11);
            triangles.Add(v + 5);
        }


        //Back Surface
        //Right Half
        triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 3);
        
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 4);

        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 6);
        triangles.Add(vertices.Count - 5);
        
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 6);
    }

    public List<Bounds> GetDivisionBoundingBoxes() {
        var boundingBoxes = new List<Bounds>();
        Vector3 leftPoint = LeftSignPosts[0];
        Vector3 rightPoint = RightSignPosts[0];
        for (int i = 1; i < LeftSignPosts.Count; i++) {
            var nextLeftPoint = LeftSignPosts[i];
            var nextRightPoint = RightSignPosts[i];

            Vector3 min = VectorTools.GetMinVector(leftPoint, rightPoint, nextLeftPoint, nextRightPoint);
            Vector3 max = VectorTools.GetMaxVector(leftPoint, rightPoint, nextLeftPoint, nextRightPoint);

            Vector3 center = (min + max) / 2;
            Vector3 size = max - min;
            boundingBoxes.Add(new Bounds(center, size));
            
            leftPoint = nextLeftPoint;
            rightPoint = nextRightPoint;
        }
        return boundingBoxes;
    }

    
}