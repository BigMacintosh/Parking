using System;
using System.Collections.Generic;
using System.Linq;
using Game.Entity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EditorTools.Roadster {
    public class Roads {
        private List<Paver> roads;
        private Vector3     previousPoint;

        private readonly World   world;
        private readonly List<Vector3> roadPoints;

        public Roads(World world) {
            this.world = world;
            roads      = Object.FindObjectsOfType<Paver>().ToList();
            roadPoints = new List<Vector3>();
            foreach (var points in roads.Select(road => road.RoadPoints)) {
                roadPoints.AddRange(points);
            }
        }

        private List<Junction> GetAllRoadJunctions() {
            var junctions = new List<Junction>();

            // Determine which roads intersect and form junctions.
            foreach (var road in roads) {
                foreach (var otherRoad in roads) {
                    if (road == otherRoad) continue;
                    junctions.AddRange(GetRoadJunctions(road, otherRoad));
                }
            }
            return junctions;
        }

        private List<Junction> GetRoadJunctions(Paver road1, Paver road2) {
            List<int> road1Overlaps = new List<int>();
            List<int> road2Overlaps = new List<int>();
            
            var boxes1 = road1.GetDivisionBoundingBoxes();
            var boxes2 = road2.GetDivisionBoundingBoxes();

            for (int i = 0; i < boxes1.Capacity; i++) {
                for (int j = 0; j < boxes2.Capacity; j++) {
                    if (boxes1[i].Intersects(boxes2[j])) {
                        road1Overlaps.Add(i);
                        road2Overlaps.Add(j);
                    }
                }
            }
            
            
            
            return new List<Junction>();
        }

        private Vector3 GetClosestPoint(int playerID) {
            var pos = world.Players[playerID].GetPosition().Transform.Position;
            if (roadPoints.Count == 0) {
                // TODO: change this to something that isn't the base exception.
                throw new Exception("no road points");
            }

            var closestPoint         = roadPoints[0];
            var closestPointDistance = Vector3.Distance(pos, roadPoints[0]);
            foreach (var point in roadPoints) {
                var pointDistance = Vector3.Distance(pos, point);
                if (pointDistance > closestPointDistance) continue;
                closestPoint         = point;
                closestPointDistance = pointDistance;
            }

            return closestPoint;
        }
    }
}