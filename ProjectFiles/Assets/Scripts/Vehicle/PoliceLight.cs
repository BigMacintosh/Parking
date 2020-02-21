using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PoliceLight : MonoBehaviour
{
    [SerializeField]private float speed = 1f;
    // Start is called before the first frame update
    void Start()
    {
     GetComponent<Animator>().SetFloat("LightSpeed", speed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
