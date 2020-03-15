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

        private readonly World         world;
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
            var junctions = new List<Junction>();

            var boxes1 = road1.GetDivisionBoundingBoxes();
            var boxes2 = road2.GetDivisionBoundingBoxes();

            var i = 0;
            var j = 0;

            while (i < boxes1.Count) {
                while (j < boxes2.Count) {
                    if (boxes1[i].Intersects(boxes2[j])) {
                        // We found the start of a junction, now find the whole thing...
                        var junc             = new Junction();
                        var previousIMatches = new Queue<int>();
                        var currentIMatches  = new Queue<int>();
                        while (true) {
                            // Check if a box intersects
                            if (boxes1[i].Intersects(boxes2[j])) {
                                junc.AddBounds(boxes1[i], boxes2[j]);
                                currentIMatches.Enqueue(j);

                                // move j forward
                                if (previousIMatches.Count != 0) j = previousIMatches.Dequeue();
                                else {
                                    j++;
                                    if (j == boxes2.Count) {
                                        // At the end of boxes2 so reset j back to the start of the previous
                                        // j matches and increment i
                                        i++;
                                        if (i == boxes1.Count) break; // We've got to the end of both arrays.
                                        previousIMatches = currentIMatches;
                                        currentIMatches  = new Queue<int>();
                                        j                = previousIMatches.Dequeue();
                                    }
                                }
                            } else {
                                // Boxes do not intersect.
                                if (previousIMatches.Count == 0 && currentIMatches.Count == 0) break;
                                if (previousIMatches.Count == 0) {
                                    // We have no more j to look at on this row.
                                    // Move i forward.
                                    i++;
                                    // Transfer queues
                                    previousIMatches = currentIMatches;
                                    currentIMatches  = new Queue<int>();
                                    j                = previousIMatches.Dequeue();
                                } else {
                                    j = previousIMatches.Dequeue();
                                }
                            }
                        }

                        junctions.Add(junc);
                    }

                    j++;
                }

                i++;
            }

            return junctions;
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