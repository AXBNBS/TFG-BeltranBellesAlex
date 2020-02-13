
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Fundido : MonoBehaviour
{
    public static Fundido instancia;

    private SeguimientoCamara camara;
    private Animator animador;
    private Vector3 posicion;
    private bool cambiarPos;


    // .
    private void Start ()
    {
        instancia = this;
        camara = GameObject.FindObjectOfType<SeguimientoCamara> ();
        animador = this.GetComponent<Animator> ();
    }


    // .
    private void Update ()
    {
        
    }


    // .
    public void FundidoAPosicion (Vector3 pos) 
    {
        animador.SetTrigger ("fundido");

        cambiarPos = true;
        posicion = pos;
    }


    // .
    public void FundidoCompletado () 
    {
        if (cambiarPos == true)
        {
            camara.transform.position = posicion;
            camara.transicionando = false;

            //CambioDePersonajesYAgrupacion.instancia.PermitirInput ();
        }
        else 
        {

        }
    }
}