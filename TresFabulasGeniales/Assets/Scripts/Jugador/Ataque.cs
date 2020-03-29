
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Ataque : MonoBehaviour
{
    public bool input;

    [SerializeField] private float saltoFrz, aranyazoFrz, rangoAtq, cooldownIAMax;
    private MovimientoHistoria2 movimientoScr;
    private CharacterController characterCtr;
    private float reboteVel, longitudRay, cooldownIAAct;
    private NavMeshAgent agente;
    private bool saltado, aranyado;
    private Animator animador;
    private LayerMask enemigosCap, bichosCap;
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
        bichosCap = LayerMask.GetMask ("Bichos");
        if (this.transform.childCount >= 7)
        {
            ataqueCenTrf = this.transform.GetChild (6);
        }
    }


    // Se tienen en cuenta varias cosas relativas a Abedul: bajamos el cooldown de su ataque en caso de que esté siendo controlado por la IA, comprobamos si se cumplen las condiciones para que el jugador pueda iniciar un nuevo ataque, llamamos
    //a la función que detecta y causa daño a los enemigos correspondientes si Abedul está atacando y desactivamos el booleano que indica que ha causado daños si su animación de ataque ha finalizado ya.
    private void Update ()
    {
        cooldownIAAct -= Time.deltaTime;
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


    // Hacemos que Violeta rebote tras tocar la cabeza del enemigo y además le cause daño.
    private void OnTriggerStay (Collider other)
    {
        if (saltado == false && movimientoScr.sueleado == false && other.CompareTag ("Rebote") == true && movimientoScr.movimiento.y < 0 && this.name != "Abedul") 
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
                other.transform.parent.GetComponent<Enemigo>().Danyar (saltoFrz / 5, true);

                movimientoScr.perseguir = false;
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


    // Activamos el trigger del animador que permite que se reproduzca la animación de atacar, tenemos en cuenta el cooldown en el caso de que ataque la IA.
    public void IniciarAtaque () 
    {
        if (input == true || cooldownIAAct < 0) 
        {
            animador.SetTrigger ("atacando");

            cooldownIAAct = cooldownIAMax;
        }
    }


    // Al llegar a un cierto punto de su animación de ataque, miramos si el collider de algún enemigo se encuentra dentro del rango en el que Abedul puede causar daños. En caso afirmativo, este recibe el daño correspondiente. Llamamos también una
    //corrutina que afecta a la IA en el caso de que esta esté controlando a Abedul.
    private void Atacar () 
    {
        Collider[] enemigosCol = Physics.OverlapSphere (ataqueCenTrf.position, rangoAtq, enemigosCap, QueryTriggerInteraction.Ignore);
        Collider[] bichosCol = Physics.OverlapSphere (ataqueCenTrf.position, rangoAtq, bichosCap, QueryTriggerInteraction.Collide);
        BichoPegajoso[] bichosPeg = this.GetComponentsInChildren<BichoPegajoso> ();

        if (input == false && enemigosCol.Length > 0) 
        {
            enemigosCol = new Collider[] { enemigosCol[0] };
        }

        foreach (Collider e in enemigosCol) 
        {
            Enemigo enemigo = e.GetComponent<Enemigo> ();

            enemigo.Danyar (input == false ? aranyazoFrz / 5 : aranyazoFrz, false);
            enemigo.ChecarDerrotado ();
        }

        foreach (Collider b in bichosCol) 
        {
            BichoPegajoso bicho = b.GetComponent<BichoPegajoso> ();

            if (bicho.pegado == false) 
            {
                bicho.Derrotado ();
            }
        }

        if (bichosPeg.Length > 0)
        {
            foreach (BichoPegajoso b in bichosPeg)
            {
                b.SalirVolando ();
            }
            movimientoScr.TodosDespegados ();
        }

        if (input == false && enemigosCol.Length > 0) 
        {
            this.StartCoroutine ("EsperarYBuscar");
        }
    }


    // Corutina usada por la IA de Abedul justo después de causar daños a un enemigo con éxito: se espera un poco y busca a un nuevo enemigo para atacarle.
    private IEnumerator EsperarYBuscar () 
    {
        movimientoScr.descansar = true;

        yield return new WaitForSeconds (1.5f);

        movimientoScr.PosicionEnemigoLejano ();

        movimientoScr.descansar = false;
    }
}