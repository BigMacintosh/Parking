using System.Collections;
using System.Collections.Generic;
using Game;
using Network;
using UnityEngine;
using Vehicle;
using Utils;


namespace Gameplay
{
    public class ParkingSpace : MonoBehaviour
    {
        public event SpaceEnterDelegate SpaceEnterEvent;
        public event SpaceExitDelegate SpaceExitEvent;

        public ushort SpaceID { get; set; }
        private Renderer rend;
        private MaterialPropertyBlock block;
        public Timer Timer { get; set; }
        public int OccupiedBy { get; set; }
        
        private Color inProg = new Color(1,1,0,0.3f);
        private Color claimed = new Color(0, 1, 0, 0.3f);
        private Color empty = new Color(0, 0, 1, 0.3f);
        private Color disable = new Color(1, 1, 1, 0);
        

        public bool Enabled { get; private set; }


        // Start is called before the first frame update
        void Start()
        {
            rend = GetComponent<Renderer>();
            block = new MaterialPropertyBlock();
            SetColour(empty);
            
            Timer = new Timer(0);
            OccupiedBy = -1;
            Disable();
            // set ID based upon name (e.g. Space (3) -> 3, Space -> 0)
            var split = name.Split(' ');

            if (split.Length == 1)
            {
                SpaceID = 0;
            }
            else
            {
                var numStr = split[1].Trim(new char[] {'(', ')'});
                var id = ushort.Parse(numStr);

                SpaceID = id;
            }
        }

        void Update()
        {
            Timer.Update();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!Enabled)
            {
                return;
            }

            if (other.TryGetComponent(out VehicleDriver driver))
            {
                SetColour( inProg);
                SpaceEnterEvent?.Invoke(0, SpaceID);
            }
            else if (other.TryGetComponent(out Vehicle.Vehicle v))
            {
            }
            else
            {
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!Enabled)
            {
                return;
            }
            
            if (other.TryGetComponent(out VehicleDriver driver))
            {
//                mat.color = new Color(1, 1, 1, 0.3f);
                SpaceExitEvent?.Invoke(0, SpaceID);
            }
            else if (other.TryGetComponent(out Vehicle.Vehicle v))
            {
            }
            else
            {
            }
        }

        public void SetOccupied(int playerID)
        {
            OccupiedBy = playerID;
            SetColour(claimed);
        }

        public void SetEmpty()
        {
            OccupiedBy = -1;
            SetColour(empty);
        }

        public bool Occupied()
        {
            return OccupiedBy != -1;
        }
        public void Enable()
        {
            Enabled = true;
            SetEmpty();
        }

        public void Disable()
        {
            Enabled = false;
            SetEmpty();
            SetColour(disable);

        }

        private void SetColour(Color colour)
        {
            rend.GetPropertyBlock(block);
            block.SetColor("_BaseColor", colour);
            rend.SetPropertyBlock(block);
        }
    }
}