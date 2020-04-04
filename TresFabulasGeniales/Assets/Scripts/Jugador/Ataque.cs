
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;



public class Ataque : MonoBehaviour
{
    public bool input;

    [SerializeField] private float saltoFrz, aranyazoFrz, rangoAtq, cooldownIAMax;
    private MovimientoHistoria2 movimientoScr, companyeroMovScr;
    private CharacterController characterCtr;
    private float reboteVel, cooldownIAAct;
    private NavMeshAgent agente;
    private bool saltado, aranyado;
    private Animator animador;
    private LayerMask golpeablesCap;
    private Transform ataqueCenTrf;

    
    // Inicialización de variables.
    private void Start ()
    {
        GameObject[] avataresObj = GameObject.FindGameObjectsWithTag ("Jugador");

        movimientoScr = this.GetComponent<MovimientoHistoria2> ();
        companyeroMovScr = movimientoScr.gameObject == avataresObj[0] ? avataresObj[1].GetComponent<MovimientoHistoria2> () : avataresObj[0].GetComponent<MovimientoHistoria2> ();
        characterCtr = this.GetComponent<CharacterController> ();
        reboteVel = +movimientoScr.saltoVel;
        agente = this.GetComponent<NavMeshAgent> ();
        saltado = false;
        aranyado = false;
        animador = this.GetComponentInChildren<Animator> ();
        golpeablesCap = LayerMask.GetMask (new string[] { "Enemigos", "Bichos" });
        ataqueCenTrf = this.transform.GetChild (this.transform.childCount - 1);
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
            Vector3 esferaCenSup = new Vector3 (ataqueCenTrf.position.x, characterCtr.bounds.center.y + this.transform.localScale.y * (characterCtr.height / 2 - characterCtr.radius), ataqueCenTrf.position.z);

            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere (esferaCenSup, rangoAtq);
            Gizmos.DrawWireSphere (new Vector3 (esferaCenSup.x, esferaCenSup.y - this.transform.localScale.y * (characterCtr.height - characterCtr.radius * 2), esferaCenSup.z), rangoAtq);
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
        Vector3 esferaCenSup = new Vector3 (ataqueCenTrf.position.x, characterCtr.bounds.center.y + this.transform.localScale.y * (characterCtr.height / 2 - characterCtr.radius), ataqueCenTrf.position.z);
        List<Collider> colliders = Physics.OverlapCapsule (esferaCenSup, new Vector3 (esferaCenSup.x, esferaCenSup.y - this.transform.localScale.y * (characterCtr.height - characterCtr.radius * 2), esferaCenSup.z), rangoAtq, golpeablesCap, 
            QueryTriggerInteraction.Collide).ToList<Collider> ();
        List<Collider> eliminados = new List<Collider> ();
        bool enemigoGol = false;
        List<BichoPegajoso> despegarVio = new List<BichoPegajoso> ();

        colliders = LimpiarLista (colliders);

        if (input == false && colliders.Count > 0) 
        {
            Collider collider = PrimerEnemigo (colliders);

            colliders.Clear ();
            if (collider != null) 
            {
                colliders.Add (collider);
            }
        }

        foreach (Collider e in colliders) 
        {
            Enemigo enemigo = e.GetComponent<Enemigo> ();

            if (enemigo != null) 
            {
                enemigo.Danyar (input == false ? aranyazoFrz / 5 : aranyazoFrz, false);
                enemigo.ChecarDerrotado ();
                eliminados.Add (e);

                enemigoGol = true;
            }
        }

        foreach (Collider c in eliminados) 
        {
            colliders.Remove (c);
        }

        foreach (Collider b in colliders) 
        {
            BichoPegajoso bicho = b.GetComponent<BichoPegajoso> ();

            print (b.name);
            if (bicho != null) 
            {
                if (bicho.pegado == false)
                {
                    bicho.Derrotado ();
                }
                else 
                {
                    if (bicho.pegadoA == companyeroMovScr) 
                    {
                        despegarVio.Add (bicho);
                    }
                }
            }
        }

        companyeroMovScr.Despegar (despegarVio);

        List<BichoPegajoso> bichosPeg = this.GetComponentsInChildren<BichoPegajoso>().ToList<BichoPegajoso> ();

        if (bichosPeg.Count > 0)
        {
            movimientoScr.Despegar (bichosPeg);
        }

        if (input == false && enemigoGol == true)
        {
            this.StartCoroutine ("EsperarYBuscar");
        }
    }


    // Devuelve el primer enemigo que encuentre en una lista llena de colliders, si hay alguno.
    private Collider PrimerEnemigo (List<Collider> colliders) 
    {
        foreach (Collider c in colliders) 
        {
            if (c.GetComponent<Enemigo> () != null) 
            {
                return c;
            }
        }
        return null;
    }


    // Elimina de la lista todo aquel trigger o collider que no esté asociado a un objeto que contenga un script de enemigo o bicho pegajoso.
    private List<Collider> LimpiarLista (List<Collider> colliders) 
    {
        List<Collider> resultado = new List<Collider> ();

        foreach (Collider c in colliders) 
        {
            if (c.isTrigger == true && c.GetComponent<BichoPegajoso> () != null) 
            {
                resultado.Add (c);

                continue;
            }

            if (c.GetComponent<Enemigo> () != null) 
            {
                resultado.Add (c);
            }
        }

        return resultado;
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