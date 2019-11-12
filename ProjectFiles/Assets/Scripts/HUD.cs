using System;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private GameObject car;
    [SerializeField] private Text velocityText;

    private Rigidbody rb;
    private float v;
         
    private void Start()
    {
        rb = car.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        v = (float)Math.Round(rb.velocity.magnitude * 3.6f, 0);
        velocityText.text = "Speed: "+ v + " km/h";
    }
}
