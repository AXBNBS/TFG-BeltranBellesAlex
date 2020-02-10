
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Enemigo : MonoBehaviour
{
    [SerializeField] private float puntosGol;
    [SerializeField] private int saltoDef, aranyazoDef;
    [SerializeField] private int aleatoriedad;
    private bool perseguir, reposicionado;
    private NavMeshAgent agente;
    private Transform objetivo;
    private Vector3 posicionIni;

    
    // .
    private void Start ()
    {
        perseguir = false;
        reposicionado = false;
        agente = this.GetComponent<NavMeshAgent> ();
        posicionIni = this.transform.position;
    }


    // .
    private void Update ()
    {
        if (perseguir == true && this.gameObject.activeSelf == true) 
        {
            if (objetivo.position.y > this.transform.position.y) 
            {
                if (reposicionado == true)
                {
                    agente.SetDestination (new Vector3 (objetivo.position.x + Random.Range (-aleatoriedad, +aleatoriedad), objetivo.transform.position.y, objetivo.position.z + Random.Range (-aleatoriedad, +aleatoriedad)));

                    reposicionado = false;
                }
                else 
                {
                    if (agente.remainingDistance < agente.stoppingDistance && this.IsInvoking () == false) 
                    {
                        this.Invoke ("Reposicionado", 1.75f);
                    }
                }
            }
            else 
            {
                agente.SetDestination (objetivo.position);
            }
        }
    }


    // .
    public void AtacarA (Transform jugador) 
    {
        perseguir = true;
        objetivo = jugador;
    }


    // .
    public void Parar () 
    {
        perseguir = false;

        if (this.gameObject.activeSelf == true) 
        {
            agente.SetDestination (posicionIni);
        }
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


    // .
    private void Reposicionado () 
    {
        reposicionado = true;
    }
}