using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinousRotation : MonoBehaviour
{
    public float rotationsPerMinute = 10f;

    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(0, 0, 6.0f * rotationsPerMinute * Time.deltaTime);
    }
}
