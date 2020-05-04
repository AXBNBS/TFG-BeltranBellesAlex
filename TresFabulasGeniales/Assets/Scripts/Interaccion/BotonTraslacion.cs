
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BotonTraslacion : MonoBehaviour
{
    [SerializeField] private Transform objetoTrf;
    [SerializeField] private bool mantener;
    [SerializeField] private Vector3 posicionFin;
    [SerializeField] private int velocidadMov;
    private Vector3 posicionIni, desplazamiento;
    private bool[] positivoDsp;
    private bool mover;
    private Renderer[] renderizadores;
    private BoxCollider cajaCol;


    // Inicialización de variables.
    private void Start ()
    {
        posicionIni = objetoTrf.position;
        desplazamiento = (posicionFin - posicionIni).normalized;
        positivoDsp = new bool[] { Mathf.Sign (desplazamiento.x) == +1, Mathf.Sign (desplazamiento.y) == +1, Mathf.Sign (desplazamiento.z) == +1 };
        renderizadores = this.GetComponentsInChildren<Renderer> ();
        renderizadores[1].enabled = false;
        cajaCol = renderizadores[0].GetComponent<BoxCollider> ();
    }


    // Si el movimiento está permitido en las circunstancias actuales, trasladamos el objeto en el sentido que corresponda.
    private void Update ()
    {
        if (MovimientoPermitido () == true) 
        {
            objetoTrf.Translate ((mover == true ? desplazamiento : -desplazamiento) * Time.deltaTime * velocidadMov, Space.World);
        }
    }


    // Si el jugador pisa el botón, activamos el booleano que inicia el movimiento del objeto asignado, desactivamos el renderizador que representa al botón sin pulsar y activamos el del botón pulsado. Se desactiva también el collider del botón
    //cuando sobresale.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            mover = true;
            renderizadores[1].enabled = true;
            renderizadores[0].enabled = false;
            cajaCol.enabled = false;
        }
    }


    // Si el jugador deja de pisar el botón y era necesario mantener este para mover el objeto en cuestión, desactivamos el booleano que inicia el movimiento del objeto asignado, activamos el renderizador que representa al botón sin pulsar y 
    //desactivamos el del botón pulsado. Se activa también el collider del botón cuando sobresale.
    private void OnTriggerExit (Collider other)
    {
        if (mantener == true && other.CompareTag ("Jugador") == true) 
        {
            mover = false;
            renderizadores[0].enabled = true;
            renderizadores[1].enabled = false;
            cajaCol.enabled = true;
        }
    }


    // Obtiene una nueva posición para el objeto que se ha de mover aplicando el desplazamiento necesario en el sentido que corresponda y devuelve true si todavía no se han sobrepasado los límites de la posición inicial o final y por tanto 
    //aún se permite moverlo, o false en el caso contrario.
    private bool MovimientoPermitido () 
    {
        Vector3 nuevaPos = objetoTrf.position;

        if (mover == false) 
        {
            nuevaPos -= desplazamiento;

            if (desplazamiento.x != 0 && (positivoDsp[0] == true ? nuevaPos.x < posicionIni.x : nuevaPos.x > posicionIni.x)) 
            {
                return false;
            }
            if (desplazamiento.y != 0 && (positivoDsp[1] == true ? nuevaPos.y < posicionIni.y : nuevaPos.y > posicionIni.y))
            {
                return false;
            }
            if (desplazamiento.z != 0 && (positivoDsp[2] == true ? nuevaPos.z < posicionIni.z : nuevaPos.z > posicionIni.z))
            {
                return false;
            }
        }
        else 
        {
            nuevaPos += desplazamiento;

            if (desplazamiento.x != 0 && (positivoDsp[0] == true ? nuevaPos.x > posicionFin.x : nuevaPos.x < posicionFin.x))
            {
                return false;
            }
            if (desplazamiento.y != 0 && (positivoDsp[1] == true ? nuevaPos.y > posicionFin.y : nuevaPos.y < posicionFin.y))
            {
                return false;
            }
            if (desplazamiento.z != 0 && (positivoDsp[2] == true ? nuevaPos.z > posicionFin.z : nuevaPos.z < posicionFin.z))
            {
                return false;
            }
        }

        return true;
    }
}