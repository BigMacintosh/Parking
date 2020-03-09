using Game.Core.Driving;
using UnityEngine;
using Transform = UnityEngine.Transform;

namespace Game.Core.Parking {
    public class ParkingSpace : MonoBehaviour, ISpaceColourController, ISpaceTransformController {
        private MaterialPropertyBlock block;
        private Renderer              rend;

        public ParkingSpaceController Controller;

        // Start is called before the first frame update
        private void Start() {
            // Tell the controller that this MonoBehavior is responsible for setting the space colour
            Controller = new ParkingSpaceController {
                ColourController    = this,
                TransformController = this,
            };

            rend  = GetComponent<Renderer>();
            block = new MaterialPropertyBlock();
            Controller.Disable();

            // set ID based upon name (e.g. Space (3) -> 3, Space -> 0)
            var split = name.Split(' ');
            if (split.Length == 1) {
                Controller.SpaceID = 0;
            } else {
                var numStr = split[1].Trim('(', ')');
                var id     = ushort.Parse(numStr);
                Controller.SpaceID = id;
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (!Controller.Enabled) {
                return;
            }

            if (other.TryGetComponent(out VehicleDriver driver)) {
                Controller.OnSpaceEnteredByPlayer();
            } else if (other.TryGetComponent(out Vehicle v)) { }
        }

        private void OnTriggerExit(Collider other) {
            if (!Controller.Enabled) {
                return;
            }

            if (other.TryGetComponent(out VehicleDriver driver)) {
                Controller.OnSpaceLeftByPlayer();
            } else if (other.TryGetComponent(out Vehicle v)) { }
        }

        #region ISpaceColourController implementation

        public void SetColour(Color colour) {
            this.rend.GetPropertyBlock(block);
            this.block.SetColor("_BaseColor", colour);
            this.rend.SetPropertyBlock(block);
        }

        #endregion

        #region ISpaceTransformController implementation

        public Transform GetTransform() {
            return transform;
        }

        #endregion
    }
}