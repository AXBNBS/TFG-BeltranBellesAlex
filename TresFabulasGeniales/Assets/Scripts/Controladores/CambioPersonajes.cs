
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CambioPersonajes : MonoBehaviour
{
    public bool input;

    private MovimientoCharacterController[] personajesMov;
    private Transform[] personajesTrf, detrases;
    private SeguimientoCamara camara;


    // Inicialización de elementos y desactivación del movimiento de uno de los personajes.
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
        personajesMov[0].input = false;
        camara.objetivo = personajesTrf[1];
        camara.detras = detrases[1];
    }


    // Si se permite el input, y se pulsa el botón/tecla de cambio de personaje, y cambiamos del personaje actual al otro.
    private void Update ()
    {
        if (input == true && Input.GetButtonDown ("Cambio personaje") == true)
        {
            if (personajesMov[0].input == true)
            {
                CambiarA (1, 0);
            }
            else
            {
                CambiarA (0, 1);
            }
        }
    }


    // La cámara pasa a seguir al nuevo personaje y se desactiva el movimiento del otro.
    private void CambiarA (int nuevo, int anterior)
    {
        camara.objetivo = personajesTrf[nuevo];
        camara.detras = detrases[nuevo];
        personajesMov[anterior].input = false;
        personajesMov[nuevo].input = true;
    }
}