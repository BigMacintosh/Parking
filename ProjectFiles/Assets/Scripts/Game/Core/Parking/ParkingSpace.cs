using Game.Core.Driving;
using Network;
using UnityEngine;
using Utils;

namespace Game.Core.Parking {
    public class ParkingSpace : MonoBehaviour {
        // Delegates
        public event SpaceEnterDelegate SpaceEnterEvent;
        public event SpaceExitDelegate  SpaceExitEvent;

        // Public fields
        public ushort SpaceID    { get; private set; }
        public int    OccupiedBy { get; private set; }
        public bool   Enabled    { get; private set; }
        public Timer  Timer      { get; set; }

        // Private Fields
        private readonly Color claimed = new Color(0, 1, 0, 0.3f);
        private readonly Color disable = new Color(1, 1, 1, 0);
        private readonly Color empty   = new Color(0, 0, 1, 0.3f);
        private readonly Color inProg  = new Color(1, 1, 0, 0.3f);

        private MaterialPropertyBlock block;
        private Renderer              rend;


        // Start is called before the first frame update
        private void Start() {
            rend  = GetComponent<Renderer>();
            block = new MaterialPropertyBlock();
            SetColour(empty);

            Timer      = new Timer(0);
            OccupiedBy = -1;
            Disable();
            // set ID based upon name (e.g. Space (3) -> 3, Space -> 0)
            var split = name.Split(' ');

            if (split.Length == 1) {
                SpaceID = 0;
            } else {
                var numStr = split[1].Trim('(', ')');
                var id     = ushort.Parse(numStr);

                SpaceID = id;
            }
        }

        private void Update() {
            Timer.Update();
        }

        private void OnTriggerEnter(Collider other) {
            if (!Enabled) {
                return;
            }

            if (other.TryGetComponent(out VehicleDriver driver)) {
                SetColour(inProg);
                SpaceEnterEvent?.Invoke(0, SpaceID);
            } else if (other.TryGetComponent(out Vehicle v)) { }
        }

        private void OnTriggerExit(Collider other) {
            if (!Enabled) {
                return;
            }

            if (other.TryGetComponent(out VehicleDriver driver)) {
//                mat.color = new Color(1, 1, 1, 0.3f);
                SpaceExitEvent?.Invoke(0, SpaceID);
            } else if (other.TryGetComponent(out Vehicle v)) { }
        }

        public void SetOccupied(int playerID) {
            OccupiedBy = playerID;
            SetColour(claimed);
        }

        public void SetEmpty() {
            OccupiedBy = -1;
            SetColour(empty);
        }

        public bool Occupied() {
            return OccupiedBy != -1;
        }

        public void Enable() {
            Enabled = true;
            SetEmpty();
        }

        public void Disable() {
            Enabled = false;
            SetEmpty();
            SetColour(disable);
        }

        private void SetColour(Color colour) {
            rend.GetPropertyBlock(block);
            block.SetColor("_BaseColor", colour);
            rend.SetPropertyBlock(block);
        }
    }
}