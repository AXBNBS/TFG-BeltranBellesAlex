
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent (typeof (SphereCollider))]
[RequireComponent (typeof (CapsuleCollider))]
public class Texto : MonoBehaviour
{
    public bool hablando;
    
    [SerializeField] private string[] texto;
    private float rotacionVel;
    private Hablar[] jugadores;
    private Hablar jugador;
    private Quaternion rotacionIni, rotacionObj;
    private GameObject panelTxt;
    private bool cercano;


    // Obtenemos las referencias al jugador o jugadores.
    private void Start ()
    {
        SphereCollider[] triggers = this.GetComponents<SphereCollider> ();

        rotacionVel = 10;
        jugadores = GameObject.FindObjectsOfType<Hablar> ();
        jugador = jugadores[0];
        rotacionIni = this.transform.rotation;
        panelTxt = GameObject.FindGameObjectWithTag("Interfaz").transform.GetChild(0).GetChild(0).gameObject;
        foreach (SphereCollider t in triggers) 
        {
            t.isTrigger = true;
        }
        this.tag = "Hablable";
        this.gameObject.layer = LayerMask.NameToLayer ("GenteHabladora");
    }


    // Si el jugador está cerca, el panel de texto no está activado, pulsa el botón para hablar y se le permite el input, se fijará una rotación objetivo para que el NPC pueda mirarle y se activará el booleano que indica que está hablando. En 
    //función de si este es verdadero o falso el NPC rotará hacia la rotación que le permita mirar al avatar del jugador o hacia su rotación inicial.
    private void Update ()
    {
        jugador = jugadores[ObtenerIndiceJugador ()];
        if (cercano == true && panelTxt.activeSelf == false && Input.GetButtonDown ("Interacción") == true && (jugador.input == true && jugador.hablables.Contains (this) == true)) 
        {
            rotacionObj = Quaternion.Euler (this.transform.rotation.eulerAngles.x, Quaternion.LookRotation(jugador.transform.position - this.transform.position).eulerAngles.y + 90, this.transform.rotation.eulerAngles.z);
        }
        if (hablando == false)
        {
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionIni, Time.deltaTime * rotacionVel);
            //print (rotacionIni.eulerAngles);
        }
        else 
        {
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionObj, Time.deltaTime * rotacionVel);
            //print (rotacionObj.eulerAngles);
        }
    }


    // Al entrar en el trigger de un objeto interactuable, el jugador recibe el texto que se mostrará si finalmente interactúa.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            /*if (jugadores.Length == 1)
            {
                jugadores[0].texto = texto;
            }
            else 
            {
                if (jugadores[0] == other.GetComponent<Hablar> ())
                {
                    jugadores[0].texto = texto;
                    jugador = jugadores[0];
                }
                else 
                {
                    jugadores[1].texto = texto;
                    jugador = jugadores[1];
                }
            }*/
            cercano = true;
        }
    }


    // Al salir del trigger de un objeto interactuable, el jugador pierde el texto.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            /*if (jugadores.Length == 1)
            {
                jugadores[0].texto = null;
            }
            else 
            {
                if (jugadores[0] == other.GetComponent<Hablar> ())
                {
                    jugadores[0].texto = null;
                }
                else
                {
                    jugadores[1].texto = null;
                }
            }*/
            cercano = false;
        }
    }


    // Desactivamos el booleano que dice que estamos hablando y para evitar problemas impedimos que este se reactive durante un pequeño periodo de tiempo.
    /*public void Esperar () 
    {
        hablando = false;
        esperar = true;

        this.Invoke ("PararEspera", 1);
    }*/


    // Devuelve la array de texto asignada a este NPC.
    public string[] DevolverTexto () 
    {
        return texto;
    }


    // Obtiene el índice de avatar controlado. En caso de que estemos en las historias 1 o 3 este será siempre 0.
    private int ObtenerIndiceJugador () 
    {
        if (jugadores.Length > 1) 
        {
            return (jugadores[0].input == true ? 0 : 1);
        }
        else 
        {
            return 0;
        }
    }


    // Llamada tras un pequeño periodo de tiempo para impedir que se reactive el booleano que nos permite hablar.
    /*private void PararEspera () 
    {
        esperar = false;
    }*/
}