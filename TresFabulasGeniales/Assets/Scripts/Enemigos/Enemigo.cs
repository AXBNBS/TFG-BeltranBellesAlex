
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Enemigo : MonoBehaviour
{
    [HideInInspector] public int indice;
    
    [SerializeField] private float puntosGol;
    [SerializeField] private int saltoDef, aranyazoDef, aleatoriedad, velocidadMov, velocidadRot, distanciaMaxObj;
    private bool perseguir, reposicionado, objetivo1, apartandose, apartandoseUltFrm;
    private NavMeshAgent agente;
    private CharacterController personajeCtr;
    private Transform objetivoTrf, puntoTrf;
    private Vector3 posicionIni, destinoRnd;
    private List<Transform> companyerosCer;
    private AreaEnemiga zona;

    
    // Inicialización de variables.
    private void Start ()
    {
        perseguir = false;
        reposicionado = false;
        apartandose = false;
        agente = this.GetComponent<NavMeshAgent> ();
        personajeCtr = this.GetComponent<CharacterController> ();
        posicionIni = this.transform.position;
        agente.updatePosition = false;
        agente.updateRotation = false;
        companyerosCer = new List<Transform> ();
        zona = this.transform.parent.GetComponent<AreaEnemiga> ();
    }


    // Si hay que perseguir a alguien y el enemigo está activo, nos dirigimos directamente al objetivo en caso de que este no esté por encima de nosotros; de lo contrario, el enemigo se mueve de forma aleatoria para tratar de evitar que el jugador
    //caiga sobre él y le haga daño, la nueva posición se definirá cada cierto tiempo.
    private void Update ()
    {
        if (perseguir == true) 
        {
            if (apartandose == false) 
            {
                MoverAgenteYControlador (puntoTrf.position);
            }
            else 
            {
                MoverAgenteYControlador (destinoRnd);

                if (reposicionado == false)
                {
                    if (this.IsInvoking () == false)
                    {
                        this.Invoke ("Reposicionado", Random.Range (1f, 2f));
                    }
                }
                else
                {
                    reposicionado = false;
                }
            }
        }
    }


    // .
    private void OnTriggerEnter (Collider other)
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
    }


    // .
    private void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere (agente.destination, 1);
    }


    // Se indica al enemigo que tiene que perseguir/atacar a alguien.
    public void AtacarA (Transform jugador, bool uno) 
    {
        if (agente.destination == destinoRnd) 
        {
            print(jugador.name);
            int indiceArr;
            float distanciaChc;

            int tomado = -1;
            float distanciaMin = float.MaxValue;

            perseguir = true;
            objetivoTrf = jugador.GetChild (3);
            puntoTrf = this.transform;
            objetivo1 = uno;
            for (int p = 0; p < objetivoTrf.childCount; p += 1)
            {
                indiceArr = objetivo1 == false ? p : p + 4;
                distanciaChc = Vector3.Distance(objetivoTrf.GetChild(p).position, this.transform.position);
                if (zona.tomadosPnt[indiceArr] == false && distanciaChc < distanciaMin)
                {
                    distanciaMin = distanciaChc;
                    puntoTrf = objetivoTrf.GetChild(p);
                    tomado = p;
                }
            }
            if (tomado != -1)
            {
                zona.tomadosPnt[tomado] = true;
            }
        }
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
            this.gameObject.SetActive (false);

            return true;
        }
    }


    // Tras cierto tiempo, marcamos como verdadero que el enemigo se ha reposicionado para poder hacerlo de nuevo.
    private void Reposicionado () 
    {
        reposicionado = true;
        destinoRnd = new Vector3 (objetivoTrf.position.x + Random.Range (-aleatoriedad, +aleatoriedad), this.transform.position.y, objetivoTrf.position.z + Random.Range (-aleatoriedad, +aleatoriedad));
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
            agente.SetDestination (objetivo);
        //}

        if (Vector3.Distance (this.transform.position, agente.destination) > distanciaMaxObj)
        {
            personajeCtr.Move (agente.desiredVelocity.normalized * Time.deltaTime * velocidadMov);

            agente.velocity = personajeCtr.velocity;
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler (this.transform.rotation.x, Mathf.Atan2 (personajeCtr.velocity.x, personajeCtr.velocity.z) * Mathf.Rad2Deg, this.transform.rotation.z), 
                Time.deltaTime * velocidadRot);
            this.transform.position = new Vector3 (this.transform.position.x, posicionIni.y, this.transform.position.z);
        }

        agente.nextPosition = this.transform.position;
    }
}