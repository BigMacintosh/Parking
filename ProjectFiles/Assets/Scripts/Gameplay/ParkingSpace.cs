using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vehicle;


namespace Gameplay
{
    public delegate void SpaceEnterDelegate(ushort spaceID);
    public delegate void SpaceExitDelegate(ushort spaceID);
    
    public class ParkingSpace : MonoBehaviour
    {
        public ushort SpaceID;
        private Material mat;

        public event SpaceEnterDelegate SpaceEnterEvent;
        public event SpaceExitDelegate SpaceExitEvent;
        
        // Start is called before the first frame update
        void Start()
        {
            mat = GetComponent<Renderer>().material;
            mat.color = new Color(1,1,1,0.3f);
            
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out VehicleDriver driver))
            {
                mat.color = new Color(0,1,0,0.3f);
                Debug.Log("HERE");
                SpaceEnterEvent?.Invoke(SpaceID);
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
                mat.color = new Color(1,1,1,0.3f);
                SpaceExitEvent?.Invoke(SpaceID);
            }
            else if (other.TryGetComponent(out Vehicle.Vehicle v))
            {
            }
            else
            {
            }
        }
        
        // Update is called once per frame
        void Update()
        {
            
        }
    }
}