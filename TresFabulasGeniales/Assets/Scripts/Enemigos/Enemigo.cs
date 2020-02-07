
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Enemigo : MonoBehaviour
{
    [SerializeField] private float puntosGol;
    [SerializeField] private int saltoDef, aranyazoDef;
    private bool perseguir;
    private NavMeshAgent agente;
    private Transform objetivo;
    private Vector3 posicionIni;

    
    // .
    private void Start ()
    {
        perseguir = false;
        agente = this.GetComponent<NavMeshAgent> ();
        posicionIni = this.transform.position;
    }


    // .
    private void Update ()
    {
        if (perseguir == true) 
        {
            agente.SetDestination (objetivo.position);
        }
    }


    // .
    public void AtacarA (Transform jugador) 
    {
        perseguir = true;
        objetivo = jugador.transform;
    }


    // .
    public void Parar () 
    {
        perseguir = false;

        agente.SetDestination (posicionIni);
    }


    // .
    public void Danyar (int danyo, bool salto) 
    {
        puntosGol -= salto ? danyo / saltoDef : danyo / aranyazoDef;

        if (puntosGol < 0) 
        {
            this.gameObject.SetActive (false);
        }
    }
}