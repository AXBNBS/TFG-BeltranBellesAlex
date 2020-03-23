
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Ataque : MonoBehaviour
{
    public bool input;

    [SerializeField] private float saltoFrz, aranyazoFrz, rangoAtq;
    private MovimientoHistoria2 movimientoScr;
    private CharacterController characterCtr;
    private float reboteVel, longitudRay;
    private NavMeshAgent agente;
    private bool saltado, aranyado;
    private Animator animador;
    private LayerMask enemigosCap;
    private Transform ataqueCenTrf;

    
    // Inicialización de variables.
    private void Start ()
    {
        movimientoScr = this.GetComponent<MovimientoHistoria2> ();
        characterCtr = this.GetComponent<CharacterController> ();
        reboteVel = +movimientoScr.saltoVel;
        longitudRay = this.transform.localScale.x * characterCtr.radius * 2;
        agente = this.GetComponent<NavMeshAgent> ();
        saltado = false;
        aranyado = false;
        animador = this.GetComponentInChildren<Animator> ();
        enemigosCap = LayerMask.GetMask ("Enemigos");
        ataqueCenTrf = this.transform.GetChild (4);
    }


    // .
    private void Update ()
    {
        if (input == true && movimientoScr.sueleado == true && Input.GetButtonDown ("Atacar") == true && animador.GetCurrentAnimatorStateInfo(0).IsTag ("Ataque") == false) 
        {
            IniciarAtaque ();
        }
        if (animador.GetCurrentAnimatorStateInfo(0).IsTag ("Ataque") == true)
        {
            if (aranyado == false && animador.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f)
            {
                Atacar ();

                aranyado = true;
            }
        }
        else 
        {
            aranyado = false;
        }
    }


    // Hacemos que el avatar rebote tras tocar la cabeza del enemigo y además le cause daño.
    private void OnTriggerStay (Collider other)
    {
        if (saltado == false && movimientoScr.sueleado == false && other.CompareTag ("Rebote") == true && this.name != "Abedul") 
        {
            saltado = true;
            movimientoScr.movimiento.y = reboteVel;

            if (movimientoScr.input == true)
            {
                other.transform.parent.GetComponent<Enemigo>().Danyar (saltoFrz, true);
                characterCtr.Move (new Vector3 (0, reboteVel, 0) * Time.deltaTime);
            }
            else
            {
                //REDUCIR DAÑO DESPUES
                other.transform.parent.GetComponent<Enemigo>().Danyar (saltoFrz, true);

                agente.baseOffset += reboteVel * Time.deltaTime;
            }
        }
    }


    // Al salir del trigger de la cabeza del enemigo que hayamos tocado, comprobamos si este sigue teniendo salud para desactivarlo o no. Si esto se cumple y además el personaje que ha derrotado al enemigo está siendo controlado por la IA, este 
    //buscará el enemigo vivo más cercano para convertirlo en su nuevo blanco.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Rebote") == true) 
        {
            //Enemigo enemigo = other.transform.parent.GetComponent<Enemigo> ();
            
            saltado = false;

            other.transform.parent.GetComponent<Enemigo>().ChecarDerrotado ();
        }
    }


    // Pal debug.
    private void OnDrawGizmos ()
    {
        if (ataqueCenTrf != null) 
        {
            Gizmos.DrawWireSphere (ataqueCenTrf.position, rangoAtq);
        }
    }


    // Activamos el trigger del animador que permite que se reproduzca la animación de atacar.
    public void IniciarAtaque () 
    {
        animador.SetTrigger ("atacando");
    }


    // .
    private void Atacar () 
    {
        Collider[] enemigosCol = Physics.OverlapSphere (ataqueCenTrf.position, rangoAtq, enemigosCap, QueryTriggerInteraction.Ignore);

        foreach (Collider e in enemigosCol) 
        {
            Enemigo enemigo = e.GetComponent<Enemigo> ();

            enemigo.Danyar (aranyazoFrz, false);
            enemigo.ChecarDerrotado ();
        }
        /*RaycastHit rayoDat;

        if (Physics.Raycast (centroTrf.position, -this.transform.right, out rayoDat, longitudRay, enemigosCap, QueryTriggerInteraction.Ignore) == true) 
        {
            Enemigo enemigo = rayoDat.transform.gameObject.GetComponent<Enemigo> ();
            print ("Le dí");

            enemigo.Danyar (aranyazoFrz, false);
            if (enemigo.ChecarDerrotado () == true && movimientoScr.input == false) 
            {
                movimientoScr.PosicionEnemigoCercano ();
            }
        }*/
    }
}