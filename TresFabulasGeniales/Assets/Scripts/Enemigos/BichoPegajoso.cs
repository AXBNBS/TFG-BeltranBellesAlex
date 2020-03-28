
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class BichoPegajoso : MonoBehaviour
{
    [SerializeField] private int empuje;
    private NavMeshAgent agente;
    private Transform objetivoTrf, padre;
    private Vector3 posicionIni, offset, puntoVue;
    private bool pegado, volando;


    // .
    private void Start ()
    {
        agente = this.GetComponent<NavMeshAgent> ();
        posicionIni = this.transform.position;
        padre = this.transform.parent;
    }


    // .
    private void Update ()
    {
        PerseguirObjetivo ();
        Pegarse ();
    }


    // .
    public void AtacarA (Transform jugadorTrf) 
    {
        objetivoTrf = jugadorTrf;
    }


    // .
    public void Parar () 
    {
        objetivoTrf = null;
    }


    //
    public void SalirVolando () 
    {
        this.transform.parent = padre;
        pegado = false;
        volando = true;
        puntoVue = this.transform.position - this.transform.forward * empuje;
    }


    // .
    private void PerseguirObjetivo () 
    {
        if (pegado == false)
        {
            if (volando == false)
            {
                if (objetivoTrf == null)
                {
                    agente.SetDestination (posicionIni);
                }
                else
                {
                    agente.SetDestination (objetivoTrf.position);
                }

                agente.velocity = agente.desiredVelocity;
            }
            else 
            {
                agente.SetDestination (puntoVue);
                if (this.IsInvoking ("SalirDeAturdimiento") == false && Vector3.Distance (this.transform.position, puntoVue) < agente.stoppingDistance) 
                {
                    this.Invoke ("SalirDeAturdimiento", 1.5f);
                }
            }
        }
        else 
        {
            this.transform.localPosition = offset;
        }
    }


    // .
    private void Pegarse () 
    {
        if (pegado == false && objetivoTrf != null && Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (objetivoTrf.position.x, objetivoTrf.position.z)) < agente.stoppingDistance) 
        {
            offset = this.transform.position - objetivoTrf.position;
            pegado = true;
            this.transform.parent = objetivoTrf;
            agente.isStopped = true;

            objetivoTrf.GetComponent<MovimientoHistoria2>().Pegado ();
        }
    }


    // .
    private void SalirDeAturdimiento () 
    {
        volando = false;
    }
}