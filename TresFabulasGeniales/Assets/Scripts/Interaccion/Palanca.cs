
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Palanca : MonoBehaviour
{
    [SerializeField] private Transform objetoTrf;
    [SerializeField] private float finalY;
    [SerializeField] private int velocidadMov, colocacionVel;
    private bool activada, jugadorCer, recolocar;
    private List<Transform> avataresCer;
    private Vector2 centroTrg;
    private Vector3 posicionObj;


    // Inicialización de variables.
    private void Start ()
    {
        Collider[] colliders = this.GetComponents<Collider> ();

        avataresCer = new List<Transform> ();
        foreach (Collider c in colliders) 
        {
            if (c.isTrigger == true) 
            {
                centroTrg = new Vector2 (c.bounds.center.x, c.bounds.center.z);

                break;
            }
        }
        /*desplazamiento = (posicionFin - posicionIni).normalized;
        positivoDsp = new bool[] { Mathf.Sign (desplazamiento.x) == +1, Mathf.Sign (desplazamiento.y) == +1, Mathf.Sign (desplazamiento.z) == +1 };
        renderizadores = this.GetComponentsInChildren<Renderer> ();
        renderizadores[1].enabled = false;
        cajaCol = renderizadores[0].GetComponent<BoxCollider> ();*/
    }


    // Si el movimiento está permitido en las circunstancias actuales, trasladamos el objeto en el sentido que corresponda.
    private void Update ()
    {
        if (recolocar == false) 
        {
            if (jugadorCer == true && avataresCer.Count == 1 && Input.GetButtonDown ("Interacción") == true)
            {
                CambioDePersonajesYAgrupacion.instancia.PararInput ();

                recolocar = true;
                posicionObj = new Vector3 (centroTrg.x, avataresCer[0].position.y, centroTrg.y);
            }
        }
        else 
        {
            if (activada == false) 
            {
                ColocarAvatar ();
            }
        }

        if (activada == true) 
        {
            if (objetoTrf.position.y > finalY) 
            {
                objetoTrf.Translate (Time.deltaTime * Vector3.down * velocidadMov, Space.World);
            }
            else 
            {
                this.enabled = false;
            }
        }
    }


    // Si el jugador pisa el botón, activamos el booleano que inicia el movimiento del objeto asignado, desactivamos el renderizador que representa al botón sin pulsar y activamos el del botón pulsado. Se desactiva también el collider del botón
    //cuando sobresale.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            jugadorCer = true;
            
            avataresCer.Add (other.transform);
        }
    }


    // Si el jugador deja de pisar el botón y era necesario mantener este para mover el objeto en cuestión, desactivamos el booleano que inicia el movimiento del objeto asignado, activamos el renderizador que representa al botón sin pulsar y 
    //desactivamos el del botón pulsado. Se activa también el collider del botón cuando sobresale.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            avataresCer.Remove (other.transform);
            
            if (avataresCer.Count == 0) 
            {
                jugadorCer = false;
            }
        }
    }


    // .
    private void ColocarAvatar () 
    {
        Quaternion rotacionObj = Quaternion.Euler (0, Quaternion.LookRotation(this.transform.position - avataresCer[0].position).eulerAngles.y + 90, 0);

        if (Vector3.Distance (avataresCer[0].position, posicionObj) > 1 || Quaternion.Angle (avataresCer[0].rotation, rotacionObj) > 1) 
        {
            avataresCer[0].position = Vector3.Lerp (avataresCer[0].position, posicionObj, Time.deltaTime * colocacionVel);
            avataresCer[0].rotation = Quaternion.Lerp (avataresCer[0].rotation, rotacionObj, Time.deltaTime * colocacionVel);
        }
        else 
        {
            activada = true;

            CambioDePersonajesYAgrupacion.instancia.PermitirInput ();
        }
    }
}