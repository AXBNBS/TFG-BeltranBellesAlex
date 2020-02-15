
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Colgarse : MonoBehaviour
{
    public Balanceo balanceo;

    
    // .
    private void Start ()
    {
        balanceo.Inicializar ();
    }


    // .
    private void Update ()
    {
        this.transform.localPosition = balanceo.Mover (this.transform.localPosition, Time.deltaTime);
    }
}