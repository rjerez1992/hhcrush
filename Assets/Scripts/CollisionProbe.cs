using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionProbe : MonoBehaviour
{
    private int _collisions = 0;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public bool IsColliding() {
        return this._collisions > 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("Probe triggered enter");
        this._collisions++;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //Debug.Log("Probe triggered exited");
        this._collisions--;
    }
}
