
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Palanca : MonoBehaviour
{
    [SerializeField] private Transform objetoTrf;
    [SerializeField] private float bajada;
    [SerializeField] private int velocidadMov, colocacionVel;
    [SerializeField] private bool activada, jugadorCer, recolocar;
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


    // Si el jugador está cerca, sólo es un avatar, se pulsa el botón de interactuar y está siendo controlado, iniciaremos el proceso de recolocarlo para que esté en frente y mire correctamente la válvula. Una vez esto pase, la válvula estará activa y
    //la natilla asignada descenderá hasta llegar a su límite, momento en el cual desactivamos el script actual. Durante este proceso, el input del jugador se mantiene desactivado.
    private void Update ()
    {
        if (recolocar == false) 
        {
            if (jugadorCer == true && avataresCer.Count == 1 && Input.GetButtonDown ("Interacción") == true && avataresCer[0].GetComponent<MovimientoHistoria2>().input == true)
            {
                recolocar = true;
                posicionObj = new Vector3 (centroTrg.x, avataresCer[0].transform.position.y, centroTrg.y);
                objetivoY = objetoTrf.position.y - bajada;

                ActivarInput (false);
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
            //print (objetoTrf.position.y);
            //print (objetivoY);
            if (objetoTrf.position.y > objetivoY) 
            {
                objetoTrf.Translate (Time.deltaTime * Vector3.down * velocidadMov, Space.World);
            }
            else 
            {
                ActivarInput (true);

                this.enabled = false;
            }
        }
    }


    // Si el jugador se acerca a la válvula lo suficiente, añadimos su character controller a la lista y activamos el booleano que indica que el jugador está cerca.
    private void OnTriggerEnter (Collider other)
    {
        if (other.isTrigger == false && other.CompareTag ("Jugador") == true) 
        {
            jugadorCer = true;
            
            avataresCer.Add (other.GetComponent<CharacterController> ());
            //print (other.transform.name);
        }
    }


    // Si el jugador se aleja de la válvula lo suficiente para salir del trigger, eliminamos su character controller de la lista y, si no hay ningún avatar más cerca de la válvula, desactivamos el booleano que indica que hay un jugador cerca.
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


    // Coloca al avatar en frente de la palanca, trasladándolo y rotándolo. Una vez se completa este movimiento, la palanca habrá sido activada.
    private void ColocarAvatar () 
    {
        Quaternion rotacionObj = Quaternion.Euler (0, Quaternion.LookRotation(this.transform.position - avataresCer[0].transform.position).eulerAngles.y + 90, 0);

        if (Vector3.Distance (avataresCer[0].transform.position, posicionObj) > 1 || Quaternion.Angle (avataresCer[0].transform.rotation, rotacionObj) > 1) 
        {
            avataresCer[0].transform.rotation = Quaternion.Lerp (avataresCer[0].transform.rotation, rotacionObj, Time.deltaTime * colocacionVel);

            avataresCer[0].Move (Vector3.Lerp (avataresCer[0].transform.position, posicionObj, Time.deltaTime * colocacionVel) - avataresCer[0].transform.position);
        }
        else 
        {
            activada = true;
        }
    }


    // Según se le indique, para o permite el input del cambio de personajes, movimiento y ataque.
    private void ActivarInput (bool activar) 
    {
        CambioDePersonajesYAgrupacion.instancia.input = activar;
        avataresCer[0].GetComponent<MovimientoHistoria2>().input = activar;
        avataresCer[0].GetComponent<Ataque>().input = activar;
    }
}