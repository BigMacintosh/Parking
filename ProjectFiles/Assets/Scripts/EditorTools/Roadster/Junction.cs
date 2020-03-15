using System.Collections.Generic;
using UnityEngine;

namespace EditorTools.Roadster {
    public enum JunctionType {
        TJunction,
        CrossRoads,
    }
    
    public class Junction {
        private Vector3 location;
        private JunctionType junctionType;
        
        private List<Bounds> boundingBoxes;
        private List<Bounds> box1s;
        private List<Bounds> box2s;

        public Junction() {
            boundingBoxes = new List<Bounds>();
            box1s = new List<Bounds>();
            box2s = new List<Bounds>();
        }


        public void AddBounds(Bounds box1, Bounds box2) {
            box1s.Add(box1);
            box2s.Add(box2);

            box1.Encapsulate(box2);
            boundingBoxes.Add(box1);
        }
    }
}