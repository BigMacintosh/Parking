using System;
using System.Collections.Generic;
using Game.Core.Camera;
using UI;
using UI.Minimap;
using UnityEngine;

namespace Game.Core.Driving {
    public class Vehicle : MonoBehaviour {
        // Public Fields
        public List<DriveWheel>  driveWheels;
        public VehicleProperties vehicleProperties;
        public LayerMask collisionMask;

        // Private Fields
        private Color colourToSet;
        private UIController  uiController;
        private VehicleDriver driver;
        private MaterialPropertyBlock bodyMaterialBlock;
        
        
        public void Start() {
            bodyMaterialBlock = new MaterialPropertyBlock();
            Debug.Log($"{colourToSet.ToString()}");
            _setBodyColour(colourToSet);
        }

        public void SetBodyColour(Color colour) {
            colourToSet = colour;
            Debug.Log("test");
        }
        
        private void _setBodyColour(Color colour) {
            var renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var childRenderer in renderers) {
                Debug.Log(childRenderer.name);
                if (childRenderer.name.Equals("Car")) {
                    Debug.Log("setting colour");
                    // childRenderer.materials[0].color = colour; 
                    childRenderer.materials[0].SetColor("_BaseColor", colour);
                    childRenderer.materials[0].SetColor("Color_EF46C5D1", colour);
                    
                //     childRenderer.GetPropertyBlock(bodyMaterialBlock);
                //     bodyMaterialBlock.SetColor("_BaseColor", colour);
                //     childRenderer.SetPropertyBlock(bodyMaterialBlock);
                }
            }
        }

        public void SetControllable() {
            // Add DriveController to car
            driver = gameObject.AddComponent<VehicleDriver>();

            driver.accel       = 20;
            driver.maxSpeed    = 50;
            driver.driveWheels = driveWheels;
            driver.maxSteer    = 30;
            driver.driftFactor = 3f;
            driver.mask        = collisionMask;
            driver.setAcceptInput(true);

            // Set camera to follow car
            var camera = FindObjectOfType<CameraFollowController>();
            camera.ObjectToFollow = transform;

            var minimap = FindObjectOfType<MinimapScroller>();
            minimap.ObjectToFollow = transform;

            //Link with UIController
            uiController         = FindObjectOfType<UIController>();
            uiController.Vehicle = this;
        }
    }

    // Stores properties for each type of car
    // Allows drive controller to change based on different properties.
    [Serializable]
    public class VehicleProperties {
        // Scale from 1 - 10
        [field: SerializeField] public int SpeedRating    { get; private set; }
        [field: SerializeField] public int SteeringRating { get; private set; }
        [field: SerializeField] public int WeightRating   { get; private set; }
    }
}