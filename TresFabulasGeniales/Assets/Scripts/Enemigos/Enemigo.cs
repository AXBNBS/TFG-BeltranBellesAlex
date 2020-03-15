
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Enemigo : MonoBehaviour
{
    public Transform avatarTrf;
    [HideInInspector] public int indice;

    [SerializeField] private float puntosGol;
    [SerializeField] private int saltoDef, aranyazoDef, aleatoriedad, velocidadMov, velocidadRot, distanciaMaxObj;
    private bool perseguir, reposicionado, objetivo1, alcanzadoObj;
    private Vector3 posicionIni, destinoRnd;
    private NavMeshAgent agente;
    private CharacterController personajeCtr;
    private Transform objetivoTrf, puntoTrf;
    private List<Transform> companyerosCer;
    private AreaEnemiga zona;
    private int indicePnt;
    private Animator animador;

    
    // Inicialización de variables.
    private void Start ()
    {
        perseguir = false;
        reposicionado = false;
        alcanzadoObj = false;
        posicionIni = this.transform.position;
        agente = this.GetComponent<NavMeshAgent> ();
        agente.updatePosition = false;
        agente.updateRotation = false;
        agente.destination = posicionIni;
        personajeCtr = this.GetComponent<CharacterController> ();
        objetivoTrf = null;
        companyerosCer = new List<Transform> ();
        zona = this.transform.parent.GetComponent<AreaEnemiga> ();
        animador = this.GetComponentInChildren<Animator> ();
    }


    // Si hay que perseguir a alguien y el enemigo está activo, nos dirigimos directamente al objetivo en caso de que este no esté por encima de nosotros; de lo contrario, el enemigo se mueve de forma aleatoria para tratar de evitar que el jugador
    //caiga sobre él y le haga daño, la nueva posición se definirá cada cierto tiempo.
    private void Update ()
    {
        alcanzadoObj = Vector3.Distance (this.transform.position, agente.destination) < distanciaMaxObj;
        if (perseguir == true) 
        {
            if (avatarTrf.name == "Abedul" || this.transform.position.y >= objetivoTrf.position.y) 
            {
                MoverAgenteYControlador (puntoTrf.position);
                if (this.IsInvoking () == true) 
                {
                    this.CancelInvoke ("Reposicionado");
                }
            }
            else 
            {
                MoverAgenteYControlador (destinoRnd);

                if (reposicionado == false)
                {
                    if (this.IsInvoking () == false)
                    {
                        destinoRnd = new Vector3 (avatarTrf.position.x + Random.Range (-aleatoriedad, +aleatoriedad), this.transform.position.y, avatarTrf.position.z + Random.Range (-aleatoriedad, +aleatoriedad));

                        this.Invoke ("Reposicionado", Random.Range (1f, 2f));
                    }
                }
                else
                {
                    reposicionado = false;
                }
            }
        }
        else 
        {
            MoverAgenteYControlador (posicionIni);
        }

        Animar ();
    }


    // .
    /*private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("EspacioEnemigo") == true && other.transform.parent != this.transform) 
        {
            companyerosCer.Add (other.transform.parent);
        }
    }


    // .
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("EspacioEnemigo") == true)
        {
            companyerosCer.Remove (other.transform.parent);
        }
    }*/


    // .
    private void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere (agente.destination, 1);
    }


    // Se indica al enemigo que tiene que perseguir/atacar a alguien.
    public void AtacarA (Transform jugador, bool uno) 
    {
        int indiceArr;
        float distanciaChc;

        float distanciaMin = float.MaxValue;

        indicePnt = -1;
        perseguir = true;
        avatarTrf = jugador;
        objetivoTrf = jugador.GetChild (3);
        puntoTrf = this.transform;
        objetivo1 = uno;
        for (int p = 0; p < objetivoTrf.childCount; p += 1)
        {
            indiceArr = objetivo1 == false ? p : p + 4;
            distanciaChc = Vector3.Distance (objetivoTrf.GetChild(p).position, this.transform.position);
            if (zona.tomadosPnt[indiceArr] == false && distanciaChc < distanciaMin)
            {
                distanciaMin = distanciaChc;
                puntoTrf = objetivoTrf.GetChild (p);
                indicePnt = indiceArr;
            }
        }
        if (indicePnt != -1)
        {
            zona.tomadosPnt[indicePnt] = true;
        }
    }


    // El enemigo deja de perseguir a su objetivo y vuelve a su posición inicial, en caso de seguir activo.
    public void Parar () 
    {
        perseguir = false;
        objetivoTrf = null;

        this.CancelInvoke ("Reposicionando");
        /*if (this.gameObject.activeSelf == true) 
        {
            agente.SetDestination (posicionIni);
            print ("jajan't");
        }*/
    }


    // Se recibe el valor del daño recibido, y si es un salto o no, para tener en cuenta el valor de defensa del enemigo según el tipo de ataque. Además desactivaremos al enemigo en caso de que su salud tras el golpe sea menor de 0.
    public void Danyar (float danyo, bool salto) 
    {
        puntosGol -= salto  == true ? danyo / saltoDef : danyo / aranyazoDef;
    }


    // Devuelve true o false dependiendo de si la salud del enemigo está por debajo de 0, y también lo desactiva si es el caso.
    public bool ChecarDerrotado () 
    {
        if (puntosGol >= 0) 
        {
            return false;
        }
        else
        {
            if (indicePnt != -1) 
            {
                zona.tomadosPnt[indicePnt] = false;
            }

            if (avatarTrf.name == "Abedul" || this.transform.position.y >= objetivoTrf.position.y) 
            {
                zona.APor (avatarTrf, objetivo1);
            }

            this.gameObject.SetActive (false);

            return true;
        }
    }


    // Tras cierto tiempo, marcamos como verdadero que el enemigo se ha reposicionado para poder hacerlo de nuevo.
    private void Reposicionado () 
    {
        reposicionado = true;
    }


    // .
    private void MoverAgenteYControlador (Vector3 objetivo)
    {
        objetivo.y = this.transform.position.y;

        /*if (rodear == true)
        {
            float numerador = objetivo1 == false ? indice : indice - zona.perseguidores0;
            float denominador = objetivo1 == false ? zona.perseguidores0 : zona.perseguidores1;

            switch (numerador % 4) 
            {
                case 0:
                    agente.SetDestination (objetivo + (+numerador / denominador * objetivoTrf.forward + (+(denominador - numerador) / denominador * objetivoTrf.right)).normalized * 10);

                    break;
                case 1:
                    agente.SetDestination (objetivo + (-numerador / denominador * objetivoTrf.forward + (+(denominador - numerador) / denominador * objetivoTrf.right)).normalized * 10);
                    print (objetivo + (-numerador / denominador * objetivoTrf.forward + (+(denominador - numerador) / denominador * objetivoTrf.right)).normalized * 10);

                    break;
                case 2:
                    agente.SetDestination (objetivo + (+numerador / denominador * objetivoTrf.forward + (-(denominador - numerador) / denominador * objetivoTrf.right)).normalized * 10);

                    break;
                default:
                    agente.SetDestination (objetivo + (-numerador / denominador * objetivoTrf.forward + (-(denominador - numerador) / denominador * objetivoTrf.right)).normalized * 10);

                    break;
            }
        }
        else 
        {*/
        if (agente.destination != objetivo) 
        {
            agente.SetDestination (objetivo);
        }

        if (alcanzadoObj == false)
        {
            personajeCtr.Move (agente.desiredVelocity.normalized * Time.deltaTime * velocidadMov);

            agente.velocity = personajeCtr.velocity;
            if (agente.velocity != Vector3.zero) 
            {
                this.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler (this.transform.rotation.x, Mathf.Atan2 (personajeCtr.velocity.x, personajeCtr.velocity.z) * Mathf.Rad2Deg, this.transform.rotation.z),
                    Time.deltaTime * velocidadRot);
            }
            this.transform.position = new Vector3 (this.transform.position.x, posicionIni.y, this.transform.position.z);
        }

        agente.nextPosition = this.transform.position;
    }


    // .
    private void Animar () 
    {
        animador.SetBool ("moviendose", !alcanzadoObj);
    }
}