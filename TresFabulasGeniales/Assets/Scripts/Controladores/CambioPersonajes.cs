
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CambioPersonajes : MonoBehaviour
{
    public bool input;

    private MovimientoCharacterController[] personajesMov;
    private Transform[] personajesTrf, detrases;
    private SeguimientoCamara camara;


    // .
    private void Start ()
    {
        personajesMov = GameObject.FindObjectsOfType<MovimientoCharacterController> ();
        personajesTrf = new Transform[2];
        detrases = new Transform[2];
        camara = GameObject.FindObjectOfType<SeguimientoCamara> ();
        personajesTrf[0] = personajesMov[0].transform;
        personajesTrf[1] = personajesMov[1].transform;
        detrases[0] = personajesTrf[0].GetChild (0);
        detrases[1] = personajesTrf[1].GetChild (0);
        personajesMov[1].enabled = false;
    }


    // .
    private void Update ()
    {
        if (input == true && Input.GetButtonDown ("Cambio personaje") == true)
        {
            if (personajesMov[0].enabled == true)
            {
                CambiarA (1);
            }
            else
            {
                CambiarA (0);
            }
        }
    }


    //
    private void CambiarA (int personaje)
    {
        if (personajesMov[personaje].EstaParado () == true)
        {
            camara.objetivo = personajesTrf[personaje];
            camara.detras = detrases[personaje];
            personajesMov[personaje].enabled = false;
            personajesMov[personaje].enabled = true;
        }
    }
}