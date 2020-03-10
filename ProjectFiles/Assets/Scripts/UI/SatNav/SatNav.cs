using System;
using System.Collections.Generic;
using System.Linq;
using Game.Entity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI.SatNav {
    public class SatNav {
        private List<Vector3> roadPoints;
        private List<Paver> roads;
        private GameObject closestPointMarker;
        private Vector3 previousPoint;
        private ClientWorld world;
        
        public SatNav(ClientWorld world) {
            this.world = world;
            roads = Object.FindObjectsOfType<Paver>().ToList();
            roadPoints = new List<Vector3>();
            foreach (var road in roads) {
                var points = road.GetRoadPoints();
                roadPoints.AddRange(points);
            }
            closestPointMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }
        
        public void Update() {
            // Get closest point
            var point = GetClosestPoint();
            if (point == previousPoint) return;
            previousPoint = point;
            if (Physics.Raycast(new Ray(point, Vector3.down), out var pointHit,
                                100)) {
                point.y -= pointHit.distance - 4;
            }
            
            // Move marker to closest point
            closestPointMarker.transform.position = point;
        }

        private Vector3 GetClosestPoint() {
            var myPos = world.GetMyPosition().Pos;
            if (roadPoints.Count == 0) {
                // TODO: change this to something that isn't the base exception.
                throw new Exception("no road points");
            }
            var closestPoint = roadPoints[0];
            var closestPointDistance = Vector3.Distance(myPos, roadPoints[0]);
            foreach (var point in roadPoints) {
                var pointDistance = Vector3.Distance(myPos, point);
                if (pointDistance > closestPointDistance) continue;
                closestPoint = point;
                closestPointDistance = pointDistance;
            }

            return closestPoint;
        }
    }
}