using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public GameObject car;
    private Rigidbody rb;
    private float v;
    public Text velocityText;
         
    // Start is called before the first frame update
    void Start()
    {
        rb = car.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        v = (float)Math.Round(rb.velocity.magnitude * 3.6f, 0);
        velocityText.text = "Speed: "+ v + " km/h";
    }
}
