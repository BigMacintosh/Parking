using Network;
using UnityEngine;
using Utils;
using Transform = UnityEngine.Transform;

namespace Game.Core.Parking {
    // The Controller exists to make the Parking Space state and logic separate from the MonoBehaviour.
    // This is turn prevents MonoBehaviours from being propagated into the ParkingSpaceManager.
    // As a result both classes can actually be unit tested.
    public class ParkingSpaceController {
        public event SpaceEnterDelegate SpaceEnterEvent;
        public event SpaceExitDelegate  SpaceExitEvent;

        public ushort SpaceID    { get; set; }
        public int    OccupiedBy { get; private set; }
        public bool   Enabled    { get; private set; }
        public Timer  Timer      { get; set; }

        public Transform Transform => TransformController.GetTransform();
        
        public ISpaceColourController    ColourController    { private get; set; }
        public ISpaceTransformController TransformController { private get; set; }

        private readonly Color claimed = new Color(0, 1, 0, 0.3f);
        private readonly Color disable = new Color(1, 1, 1, 0);
        private readonly Color empty   = new Color(0, 0, 1, 0.3f);
        private readonly Color inProg  = new Color(1, 1, 0, 0.3f);

        public ParkingSpaceController() {
            Timer = new Timer(0);
        }

        public void OnSpaceEnteredByPlayer() {
            ColourController.SetColour(inProg);
            SpaceEnterEvent?.Invoke(0, SpaceID);
        }

        public void OnSpaceLeftByPlayer() {
            SpaceExitEvent?.Invoke(0, SpaceID);
        }

        public void SetOccupied(int playerID) {
            OccupiedBy = playerID;
            ColourController.SetColour(claimed);
        }

        public void SetEmpty() {
            OccupiedBy = -1;
            ColourController.SetColour(empty);
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
            ColourController.SetColour(disable);
        }
    }
}