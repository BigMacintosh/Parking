using UnityEngine;

namespace Game.Core.Driving {
    public struct VehicleInputState {
        // TODO: these could be bools rather than floats, we don't need to support controllers
        public float Drive;
        public float Turn;
        public float Jump;
        public float Drift;

        public override string ToString() {
            return $"<{GetType().Name} Drive: {Drive:02f}, Turn: {Turn:02f}, Jump: {Jump:02f}, Drift: {Drift:02f}>";
        }

        public static VehicleInputState GetInputs() {
            return new VehicleInputState {
                Drive = Input.GetAxis("Vertical"),
                Turn  = Input.GetAxis("Horizontal"),
                Jump  = Input.GetAxis("Jump"),
                Drift = Input.GetAxis("Drift")
            };
        }
    }
}