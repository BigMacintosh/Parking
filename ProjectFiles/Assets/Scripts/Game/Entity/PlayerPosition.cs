using UnityEngine;
using Utils;

namespace Game.Entity {
    public struct PlayerPosition {
        public ObjectTransform Transform;
        public Vector3         Velocity;
        public Vector3         AngularVelocity;
    }
}