using System;
using System.Collections.Generic;
using System.Linq;
using Game.Entity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Roadster {
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
            foreach (var road in roads) {
                var points = road.GetRoadPoints();
                roadPoints.AddRange(points);
            }
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