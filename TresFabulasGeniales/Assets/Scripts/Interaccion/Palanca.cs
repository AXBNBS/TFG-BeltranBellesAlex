
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Palanca : MonoBehaviour
{
    [SerializeField] private Transform objetoTrf;
    [SerializeField] private float bajada;
    [SerializeField] private int velocidadMov, colocacionVel;
    private bool activada, jugadorCer, recolocar;
    private List<CharacterController> avataresCer;
    private Vector2 centroTrg;
    private Vector3 posicionObj;
    private float objetivoY;


    // Inicialización de variables.
    private void Start ()
    {
        Collider[] colliders = this.GetComponents<Collider> ();

        avataresCer = new List<CharacterController> ();
        foreach (Collider c in colliders)
        {
            if (c.isTrigger == true)
            {
                centroTrg = new Vector2 (c.bounds.center.x, c.bounds.center.z);

                break;
            }
        }
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
                posicionObj = new Vector3 (centroTrg.x, avataresCer[0].transform.position.y, centroTrg.y);
                objetivoY = objetoTrf.position.y - bajada;
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
            print (objetoTrf.position.y);
            print (objetivoY);
            if (objetoTrf.position.y > objetivoY) 
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
        if (other.isTrigger == false && other.CompareTag ("Jugador") == true) 
        {
            jugadorCer = true;
            
            avataresCer.Add (other.GetComponent<CharacterController> ());
            //print (other.transform.name);
        }
    }


    // Si el jugador deja de pisar el botón y era necesario mantener este para mover el objeto en cuestión, desactivamos el booleano que inicia el movimiento del objeto asignado, activamos el renderizador que representa al botón sin pulsar y 
    //desactivamos el del botón pulsado. Se activa también el collider del botón cuando sobresale.
    private void OnTriggerExit (Collider other)
    {
        if (other.isTrigger == false && other.CompareTag ("Jugador") == true) 
        {
            avataresCer.Remove (other.GetComponent<CharacterController> ());
            
            if (avataresCer.Count == 0) 
            {
                jugadorCer = false;
            }
        }
    }


    // Coloca al avatar en frente de la palanca, trasladándolo y rotándolo.
    private void ColocarAvatar () 
    {
        Quaternion rotacionObj = Quaternion.Euler (0, Quaternion.LookRotation(this.transform.position - avataresCer[0].transform.position).eulerAngles.y + 90, 0);

        if (Vector3.Distance (avataresCer[0].transform.position, posicionObj) > 1 || Quaternion.Angle (avataresCer[0].transform.rotation, rotacionObj) > 1) 
        {
            avataresCer[0].transform.rotation = Quaternion.Lerp (avataresCer[0].transform.rotation, rotacionObj, Time.deltaTime * colocacionVel);

            avataresCer[0].Move (Vector3.Lerp (avataresCer[0].transform.position, posicionObj, Time.deltaTime * colocacionVel) - avataresCer[0].transform.position);
            //avataresCer[0].transfposition = Vector3.Lerp(avataresCer[0].position, posicionObj, Time.deltaTime * colocacionVel);
        }
        else 
        {
            activada = true;

            CambioDePersonajesYAgrupacion.instancia.PermitirInput ();
        }
    }
}