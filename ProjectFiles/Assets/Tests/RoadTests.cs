using System.Collections.Generic;
using EditorTools.Roadster;
using NSubstitute;
using NUnit.Framework;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Tests {
    public class RoadTests {
        private List<Junction> JunctionTest(List<Bounds> road1Boxes, List<Bounds> road2Boxes) {
            IPaver paver1 = Substitute.For<IPaver>();
            IPaver paver2 = Substitute.For<IPaver>();
            paver1.GetDivisionBoundingBoxes().Returns(road1Boxes);
            paver2.GetDivisionBoundingBoxes().Returns(road2Boxes);

            return RoadTools.GetRoadJunctions(paver1, paver2);
        }

        [Test]
        public void GetRoadJunctionsNoIntersectionTest() {
            var junctions = JunctionTest(
                new List<Bounds> {
                    new Bounds(new Vector3 {x = 5, y  = 0, z = 19}, new Vector3 {x = 4, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 9, y  = 0, z = 19}, new Vector3 {x = 4, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 13, y = 0, z = 19}, new Vector3 {x = 4, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 17, y = 0, z = 19}, new Vector3 {x = 4, y = 1, z = 2}),
                },
                new List<Bounds> {
                    new Bounds(new Vector3 {x = 9, y = 0, z = 4.5f},  new Vector3 {x = 2, y = 1, z = 7}),
                    new Bounds(new Vector3 {x = 9, y = 0, z = 11.5f}, new Vector3 {x = 2, y = 1, z = 7}),
                }
            );

            Assert.AreEqual(0, junctions.Count);
        }

        [Test]
        public void GetRoadJunctionsSimpleCrossRoadTest() {
            var junctions = JunctionTest(
                new List<Bounds> {
                    new Bounds(new Vector3 {x = 5, y  = 0, z = 5}, new Vector3 {x = 4, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 9, y  = 0, z = 5}, new Vector3 {x = 4, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 13, y = 0, z = 5}, new Vector3 {x = 4, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 17, y = 0, z = 5}, new Vector3 {x = 4, y = 1, z = 2}),
                },
                new List<Bounds> {
                    new Bounds(new Vector3 {x = 9, y = 0, z = 4.5f},  new Vector3 {x = 2, y = 1, z = 7}),
                    new Bounds(new Vector3 {x = 9, y = 0, z = 11.5f}, new Vector3 {x = 2, y = 1, z = 7}),
                }
            );

            // Check junction was formed
            Assert.AreEqual(1, junctions.Count);

            // Check boxes selected from paver1
            Assert.AreEqual(1, junctions[0].Box1S.Count);
            Assert.AreEqual(
                new List<(int, Bounds)> {
                    (1, new Bounds(new Vector3 {x = 9, y = 0, z = 5}, new Vector3 {x = 4, y = 1, z = 2})),
                },
                junctions[0].Box1S);

            // Check boxes selected from paver2
            Assert.AreEqual(1, junctions[0].Box2S.Count);
            Assert.AreEqual(
                new List<(int, Bounds)> {
                    (0, new Bounds(new Vector3 {x = 9, y = 0, z = 4.5f}, new Vector3 {x = 2, y = 1, z = 7})),
                },
                junctions[0].Box2S);
            
            // Check junction type
            Assert.AreEqual(JunctionType.CrossRoads, junctions[0].JunctionType);
        }

        [Test]
        public void GetRoadJunctionsTJunctionRoadTest() {
            var junctions = JunctionTest(
                new List<Bounds> {
                    new Bounds(new Vector3 {x = 5, y  = 0, z = 2}, new Vector3 {x = 4, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 9, y  = 0, z = 2}, new Vector3 {x = 4, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 13, y = 0, z = 2}, new Vector3 {x = 4, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 17, y = 0, z = 2}, new Vector3 {x = 4, y = 1, z = 2}),
                },
                new List<Bounds> {
                    new Bounds(new Vector3 {x = 9, y = 0, z = 4.5f},  new Vector3 {x = 2, y = 1, z = 7}),
                    new Bounds(new Vector3 {x = 9, y = 0, z = 11.5f}, new Vector3 {x = 2, y = 1, z = 7}),
                }
            );

            // Check junction was formed
            Assert.AreEqual(1, junctions.Count);

            // Check boxes selected from paver1
            Assert.AreEqual(1, junctions[0].Box1S.Count);
            Assert.AreEqual(
                new List<(int, Bounds)> {
                    (1, new Bounds(new Vector3 {x = 9, y = 0, z = 2}, new Vector3 {x = 4, y = 1, z = 2})),
                },
                junctions[0].Box1S);

            // Check boxes selected from paver2
            Assert.AreEqual(1, junctions[0].Box2S.Count);
            Assert.AreEqual(
                new List<(int, Bounds)> {
                    (0, new Bounds(new Vector3 {x = 9, y = 0, z = 4.5f}, new Vector3 {x = 2, y = 1, z = 7})),
                },
                junctions[0].Box2S);
            
            // Check junction type
            Assert.AreEqual(JunctionType.TJunction, junctions[0].JunctionType);
        }

        [Test]
        public void GetRoadJunctionsSimpleCrossRoadWithMoreDivisionsTest() {
            var junctions = JunctionTest(
                new List<Bounds> {
                    new Bounds(new Vector3 {x = 4, y  = 0, z = 5}, new Vector3 {x = 2, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 6, y  = 0, z = 5}, new Vector3 {x = 2, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 8, y  = 0, z = 5}, new Vector3 {x = 2, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 10, y = 0, z = 5}, new Vector3 {x = 2, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 12, y = 0, z = 5}, new Vector3 {x = 2, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 14, y = 0, z = 5}, new Vector3 {x = 2, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 16, y = 0, z = 5}, new Vector3 {x = 2, y = 1, z = 2}),
                    new Bounds(new Vector3 {x = 18, y = 0, z = 5}, new Vector3 {x = 2, y = 1, z = 2}),
                },
                new List<Bounds> {
                    new Bounds(new Vector3 {x = 9, y = 0, z = 3},  new Vector3 {x = 2, y = 1, z = 4}),
                    new Bounds(new Vector3 {x = 9, y = 0, z = 7f}, new Vector3 {x = 2, y = 1, z = 4}),
                    new Bounds(new Vector3 {x = 9, y = 0, z = 11}, new Vector3 {x = 2, y = 1, z = 4}),
                    new Bounds(new Vector3 {x = 9, y = 0, z = 15}, new Vector3 {x = 2, y = 1, z = 4}),
                }
            );

            // Check junction was formed
            Assert.AreEqual(1, junctions.Count);

            // Check boxes selected from paver1
            Assert.AreEqual(4, junctions[0].Box1S.Count);
            Assert.AreEqual(
                new List<(int, Bounds)> {
                    (2, new Bounds(new Vector3 {x = 8, y  = 0, z = 5}, new Vector3 {x = 2, y = 1, z = 2})),
                    (2, new Bounds(new Vector3 {x = 8, y = 0, z = 5}, new Vector3 {x = 2, y = 1, z = 2})),
                    (3, new Bounds(new Vector3 {x = 10, y = 0, z = 5}, new Vector3 {x = 2, y = 1, z = 2})),
                    (3, new Bounds(new Vector3 {x = 10, y = 0, z = 5}, new Vector3 {x = 2, y = 1, z = 2})),
                },
                junctions[0].Box1S);

            // Check boxes selected from paver2
            Assert.AreEqual(4, junctions[0].Box2S.Count);
            Assert.AreEqual(
                new List<(int, Bounds)> {
                    (0, new Bounds(new Vector3 {x = 9, y = 0, z = 3},  new Vector3 {x = 2, y = 1, z = 4})),
                    (1, new Bounds(new Vector3 {x = 9, y = 0, z = 7f}, new Vector3 {x = 2, y = 1, z = 4})),
                    (0, new Bounds(new Vector3 {x = 9, y = 0, z = 3},  new Vector3 {x = 2, y = 1, z = 4})),
                    (1, new Bounds(new Vector3 {x = 9, y = 0, z = 7f}, new Vector3 {x = 2, y = 1, z = 4})),
                },
                junctions[0].Box2S);
            
            // Check junction type
            Assert.AreEqual(JunctionType.CrossRoads, junctions[0].JunctionType);
        }
    }
}