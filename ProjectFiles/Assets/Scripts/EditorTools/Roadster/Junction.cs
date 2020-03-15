using UnityEngine;

namespace EditorTools.Roadster {
    public enum JunctionType {
        TJunction,
        CrossRoads,
    }
    
    public class Junction {
        private Vector3 location;
        private JunctionType junctionType;

        public Junction() {
            
        }
    }
}