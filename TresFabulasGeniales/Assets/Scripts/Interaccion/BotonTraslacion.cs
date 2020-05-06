
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BotonTraslacion : MonoBehaviour
{
    public Transform objetoTrf;

    [SerializeField] private bool mantener;
    [SerializeField] private Vector3 posicionFin;
    [SerializeField] private int velocidadMov;
    [SerializeField] private BotonTraslacion igual;
    private Vector3 posicionIni, desplazamiento, traslacion;
    private bool[] positivoDsp;
    [SerializeField] private bool pulsado, limiteAlc;
    private Renderer[] renderizadores;
    private BoxCollider cajaCol;


    // Inicialización de variables.
    private void Start ()
    {
        posicionIni = objetoTrf.position;
        desplazamiento = (posicionFin - posicionIni).normalized;
        //print (desplazamiento);
        positivoDsp = new bool[] { Mathf.Sign (desplazamiento.x) == +1, Mathf.Sign (desplazamiento.y) == +1, Mathf.Sign (desplazamiento.z) == +1 };
        limiteAlc = true;
        renderizadores = this.GetComponentsInChildren<Renderer> ();
        renderizadores[1].enabled = false;
        cajaCol = renderizadores[0].GetComponent<BoxCollider> ();
    }


    // Si el movimiento está permitido en las circunstancias actuales, trasladamos el objeto en el sentido que corresponda.
    private void FixedUpdate ()
    {
        if (limiteAlc == false && MovimientoPermitido () == true) 
        {
            traslacion = (pulsado == true ? desplazamiento : -desplazamiento) * velocidadMov;
            /*if (plataformaScr != null) 
            {
                plataformaScr.negativaY = traslacion.y < 0;
            }*/

            objetoTrf.Translate (traslacion * Time.deltaTime, Space.World);
            //}
            /*else 
            {
                plataformaCueRig.MovePosition (plataformaCueRig.transform.position + traslacion * Time.deltaTime);
            }*/
        }
        /*else 
        {
            if (plataforma != null) 
            {
                plataforma.movimiento = Vector3.zero;
            }
        }*/
    }


    // Si el jugador pisa el botón, activamos el booleano que inicia el movimiento del objeto asignado, desactivamos el renderizador que representa al botón sin pulsar y activamos el del botón pulsado. Se desactiva también el collider del botón
    //cuando sobresale.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            //print ("Weno sí.");
            pulsado = true;
            renderizadores[1].enabled = true;
            renderizadores[0].enabled = false;
            cajaCol.enabled = false;
            if (igual != null && igual.pulsado == true) 
            {
                limiteAlc = igual.limiteAlc;
            }
            else 
            {
                limiteAlc = false;
            }
        }
    }


    // Si el jugador deja de pisar el botón y era necesario mantener este para mover el objeto en cuestión, desactivamos el booleano que inicia el movimiento del objeto asignado, activamos el renderizador que representa al botón sin pulsar y 
    //desactivamos el del botón pulsado. Se activa también el collider del botón cuando sobresale.
    private void OnTriggerExit (Collider other)
    {
        if (mantener == true && other.CompareTag ("Jugador") == true) 
        {
            //print ("Weno no.");
            pulsado = false;
            renderizadores[0].enabled = true;
            renderizadores[1].enabled = false;
            cajaCol.enabled = true;
            if (igual != null && igual.pulsado == true)
            {
                limiteAlc = igual.limiteAlc;
            }
            else 
            {
                limiteAlc = false;
            }
        }
    }


    // Obtiene una nueva posición para el objeto que se ha de mover aplicando el desplazamiento necesario en el sentido que corresponda y devuelve true si todavía no se han sobrepasado los límites de la posición inicial o final y por tanto 
    //aún se permite moverlo, o false en el caso contrario.
    private bool MovimientoPermitido () 
    {
        Vector3 nuevaPos = objetoTrf.position;

        if (pulsado == false) 
        {
            nuevaPos -= desplazamiento;
            if ((desplazamiento.y != 0 && (positivoDsp[1] == true ? nuevaPos.y < posicionIni.y : nuevaPos.y > posicionIni.y)) || (desplazamiento.x != 0 && (positivoDsp[0] == true ? nuevaPos.x < posicionIni.x : nuevaPos.x > posicionIni.x)) ||
                (desplazamiento.z != 0 && (positivoDsp[2] == true ? nuevaPos.z < posicionIni.z : nuevaPos.z > posicionIni.z))) 
            {
                limiteAlc = true;
                if (igual != null)
                {
                    igual.limiteAlc = true;
                }

                return false;
            }
        }
        else 
        {
            nuevaPos += desplazamiento;
            if ((desplazamiento.x != 0 && (positivoDsp[0] == true ? nuevaPos.x > posicionFin.x : nuevaPos.x < posicionFin.x)) || (desplazamiento.y != 0 && (positivoDsp[1] == true ? nuevaPos.y > posicionFin.y : nuevaPos.y < posicionFin.y)) ||
                (desplazamiento.z != 0 && (positivoDsp[2] == true ? nuevaPos.z > posicionFin.z : nuevaPos.z < posicionFin.z)))
            {
                limiteAlc = true;
                if (igual != null)
                {
                    igual.limiteAlc = true;
                }

                return false;
            }
        }

        return true;
    }
}