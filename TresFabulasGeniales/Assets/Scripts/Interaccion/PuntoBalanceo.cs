
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PuntoBalanceo : MonoBehaviour
{
    public bool movimientoX;

    
    // .
    private void Start ()
    {
        
    }


    // .
    private void Update ()
    {
        
    }


    // .
    private void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Jugador") 
        {
            other.GetComponent<MovimientoHistoria3>().enganchePnt = this.transform.position;
        }
    }


    // .
    private void OnTriggerExit (Collider other)
    {
        if (other.tag == "Jugador") 
        {
            other.GetComponent<MovimientoHistoria3>().enganchePnt = Vector3.zero;
        }
    }
}