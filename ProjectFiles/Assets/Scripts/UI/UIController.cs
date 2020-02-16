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
        [SerializeField] private HUD hud;

        private int timer;
        private Rigidbody rb;
        private float v;
        private bool active;
        public int test;
        
        public UIController()
        {

        }

        private void Start()
        {
            Cursor.visible = false;
            escmenu.SetActive(false);
            settingsmenu.SetActive(false);
            active = false;
            timer = 0;
            hud = FindObjectOfType<HUD>();
        }

        // Update is called once per frame
        private void Update()
        {
            timer++;

            if (active)
            {
                Cursor.visible = true;
            }
            else
            {
                Cursor.visible = false;
            }

            if (Input.GetKey(KeyCode.Escape))
            {
                if (!active && timer > 30)
                {
                    escmenu.SetActive(true);
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
        public HUD getHUD()
        {
            if (hud is null)
            {
                hud = FindObjectOfType<HUD>();
            }
            return hud;
        }
    }
}
