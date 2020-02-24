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
        private Material mat;
        public Timer Timer { get; set; }
        public int OccupiedBy { get; set; }


        // Start is called before the first frame update
        void Start()
        {
            mat = GetComponent<Renderer>().material;
            mat.color = new Color(1, 1, 1, 0.3f);
            Timer = new Timer(0);
            OccupiedBy = -1;
            
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
            if (other.TryGetComponent(out VehicleDriver driver))
            {
                mat.color = new Color(1, 1, 0, 0.3f);
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
            mat.color = new Color(0, 1, 0, 0.3f);
        }

        public bool Occupied()
        {
            return OccupiedBy != -1;
        }
    }
}