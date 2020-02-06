using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Object = UnityEngine.Object;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        //  [SerializeField] private GameObject car;
        [SerializeField] private GameObject escmenu;
        [SerializeField] private GameObject settingsmenu;

        private int timer;
        private Rigidbody rb;
        private float v;
        private bool active;
        
        private void Start()
        {
            Cursor.visible = false;
            //   rb = car.GetComponent<Rigidbody>();
            //escmenu = Resources.Load<GameObject>("EscapeMenu");
            //   rb = Resources.Load<Rigidbody>("Car");
            escmenu.SetActive(false);
            settingsmenu.SetActive(false);
            active = false;
            timer = 0;
        }

        // Update is called once per frame
        private void Update()
        {
            timer++;
            //  v = (float)Math.Round(rb.velocity.magnitude * 3.6f, 0);
            //  debugtext.text = "Connected to: " + "serverip" + "\nPlayers connected: " + "playernumber";
            //  velocityText.text = "Speed: "+ v + " km/h";
            if (Input.GetKey(KeyCode.Escape))
            {
                if (!active && timer > 30)
                {
                    escmenu.SetActive(true);
                    Cursor.visible = true;
                    active = true;
                    timer = 0;
                }
                else if (timer > 30)
                {
                    escmenu.SetActive(false);
                    Cursor.visible = false;
                    active = false;
                    timer = 0;
                }
            }

        }
    }
}
