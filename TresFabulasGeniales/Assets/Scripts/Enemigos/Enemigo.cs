
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Enemigo : MonoBehaviour
{
    [SerializeField] private float puntosGol;
    [SerializeField] private int saltoDef, aranyazoDef, aleatoriedad, velocidadMov, velocidadRot;
    private bool perseguir, reposicionado;
    private NavMeshAgent agente;
    private CharacterController personajeCtr;
    private Transform objetivo;
    private Vector3 posicionIni, destinoRnd;

    
    // Inicialización de variables.
    private void Start ()
    {
        perseguir = false;
        reposicionado = false;
        agente = this.GetComponent<NavMeshAgent> ();
        personajeCtr = this.GetComponent<CharacterController> ();
        posicionIni = this.transform.position;
        //agente.updatePosition = false;
        agente.updateRotation = false;
    }


    // Si hay que perseguir a alguien y el enemigo está activo, nos dirigimos directamente al objetivo en caso de que este no esté por encima de nosotros; de lo contrario, el enemigo se mueve de forma aleatoria para tratar de evitar que el jugador
    //caiga sobre él y le haga daño, la nueva posición se definirá cada cierto tiempo.
    private void Update ()
    {
        if (perseguir == true) 
        {
            if (objetivo.position.y <= this.transform.position.y) 
            {
                MoverAgenteYControlador (objetivo.position);
            }
            else 
            {
                MoverAgenteYControlador (destinoRnd);
                if (reposicionado == false)
                {
                    if (this.IsInvoking () == false)
                    {
                        this.Invoke ("Reposicionado", 1.75f);
                    }
                }
                else
                {
                    reposicionado = false;
                }
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

        agente.SetDestination (posicionIni);
    }


    // Se recibe el valor del daño recibido, y si es un salto o no, para tener en cuenta el valor de defensa del enemigo según el tipo de ataque. Además desactivaremos al enemigo en caso de que su salud tras el golpe sea menor de 0.
    public void Danyar (float danyo, bool salto) 
    {
        puntosGol -= salto  == true ? danyo / saltoDef : danyo / aranyazoDef;
    }


    // .
    public bool ChecarDerrotado () 
    {
        if (puntosGol >= 0) 
        {
            return false;
        }
        else
        {
            this.gameObject.SetActive (false);

            return true;
        }
    }


    // Tras cierto tiempo, marcamos como verdadero que el enemigo se ha reposicionado para poder hacerlo de nuevo.
    private void Reposicionado () 
    {
        reposicionado = true;
        destinoRnd = new Vector3 (objetivo.position.x + Random.Range (-aleatoriedad, +aleatoriedad), this.transform.position.y, objetivo.position.z + Random.Range (-aleatoriedad, +aleatoriedad));
    }


    // .
    private void MoverAgenteYControlador (Vector3 objetivo)
    {
        print (objetivo);
        if (Vector3.Distance (this.transform.position, agente.destination) > agente.stoppingDistance) 
        {
            objetivo.y = this.transform.position.y;
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.LookRotation (objetivo - this.transform.position), Time.deltaTime * velocidadRot);

            agente.SetDestination (objetivo);
            personajeCtr.Move (agente.desiredVelocity.normalized * Time.deltaTime * velocidadMov);

            agente.velocity = personajeCtr.velocity;
            this.transform.position = new Vector3 (this.transform.position.x, posicionIni.y, this.transform.position.z);
        }
    }
}