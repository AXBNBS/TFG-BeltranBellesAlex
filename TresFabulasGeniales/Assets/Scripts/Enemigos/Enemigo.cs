
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

    
    // Inicialización de variables.
    private void Start ()
    {
        perseguir = false;
        reposicionado = false;
        agente = this.GetComponent<NavMeshAgent> ();
        posicionIni = this.transform.position;
    }


    // Si hay que perseguir a alguien y el enemigo está activo, nos dirigimos directamente al objetivo en caso de que este no esté por encima de nosotros; de lo contrario, el enemigo se mueve de forma aleatoria para tratar de evitar que el jugador
    //caiga sobre él y le haga daño, la nueva posición se definirá cada cierto tiempo.
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
                    if (this.IsInvoking () == false && agente.remainingDistance < agente.stoppingDistance) 
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


    // Se indica al enemigo que tiene que perseguir/atacar a alguien.
    public void AtacarA (Transform jugador) 
    {
        perseguir = true;
        objetivo = jugador;
    }


    // El enemigo deja de perseguir a su objetivo y vuelve a su posición inicial, en caso de seguir activo.
    public void Parar () 
    {
        perseguir = false;

        if (this.gameObject.activeSelf == true) 
        {
            agente.SetDestination (posicionIni);
        }
    }


    // Se recibe el valor del daño recibido, y si es un salto o no, para tener en cuenta el valor de defensa del enemigo según el tipo de ataque. Además desactivaremos al enemigo en caso de que su salud tras el golpe sea menor de 0.
    public void Danyar (int danyo, bool salto) 
    {
        puntosGol -= salto ? danyo / saltoDef : danyo / aranyazoDef;

        if (puntosGol < 0) 
        {
            this.gameObject.SetActive (false);
        }
    }


    // Tras cierto tiempo, marcamos como verdadero que el enemigo se ha reposicionado para poder hacerlo de nuevo.
    private void Reposicionado () 
    {
        reposicionado = true;
    }
}