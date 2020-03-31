
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Texto : MonoBehaviour
{
    [SerializeField] private string[] texto;
    private float rotacionVel;
    private Hablar[] jugadores;
    private Hablar jugador;
    private Quaternion rotacionIni, rotacionObj;
    private GameObject panelTxt;


    // Obtenemos las referencias al jugador o jugadores.
    private void Start ()
    {
        rotacionVel = 10;
        jugadores = GameObject.FindObjectsOfType<Hablar> ();
        jugador = jugadores[0];
        rotacionIni = this.transform.rotation;
        panelTxt = GameObject.FindGameObjectWithTag("Interfaz").transform.GetChild(0).GetChild(0).gameObject;
    }


    // .
    private void Update ()
    {
        if (panelTxt.activeSelf == false && Input.GetButtonDown ("Interacción") == true && ((jugadores[0].input == true && jugadores[0].texto != null) || (jugadores.Length > 1 && jugadores[1].input == true && jugadores[1].texto != null))) 
        {
            rotacionObj = Quaternion.Euler (0, Quaternion.LookRotation(jugador.transform.position - this.transform.position).eulerAngles.y + 90, 0);
        }
        if (panelTxt.activeSelf == false)
        {
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionIni, Time.deltaTime * rotacionVel);
        }
        else 
        {
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionObj, Time.deltaTime * rotacionVel);
        }
    }


    // Al entrar en el trigger de un objeto interactuable, el jugador recibe el texto que se mostrará si finalmente interactúa.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            if (jugadores.Length == 1)
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
            }
        }
    }


    // Al salir del trigger de un objeto interactuable, el jugador pierde el texto.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            if (jugadores.Length == 1)
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
            }
        }
    }
}