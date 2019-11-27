using System;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private Text velocityText;
    private Rigidbody rb;
    private float v;
    private bool isSet;

    public Rigidbody Car {private get; set;}

    // Update is called once per frame
    private void Update()
    {
        v = (float)Math.Round(Car.velocity.magnitude * 3.6f, 0);
            velocityText.text = "Speed: "+ v + " km/h";
    }

}
