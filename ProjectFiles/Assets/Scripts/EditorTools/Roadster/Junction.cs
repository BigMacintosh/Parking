using System;
using System.Collections.Generic;
using UnityEngine;

namespace EditorTools.Roadster {
    public enum JunctionType {
        TJunction,
        CrossRoads,
    }
    
    public class Junction {
        public JunctionType JunctionType { get; set; }
        public List<(int, Bounds)> Box1S { get; private set; }
        public List<(int, Bounds)> Box2S { get; private set; }
        
        private Vector3 location;
        private List<Bounds> boundingBoxes;
        
        public Junction(JunctionType junctionType) {
            boundingBoxes = new List<Bounds>();
            Box1S = new List<(int, Bounds)>();
            Box2S = new List<(int, Bounds)>();
            this.JunctionType = junctionType;
        }


        public void AddBounds(Bounds box1, int i, Bounds box2, int j) {
            Box1S.Add((i, box1));
            Box2S.Add((j, box2));

            box1.Encapsulate(box2);
            boundingBoxes.Add(box1);
        }
    }
}