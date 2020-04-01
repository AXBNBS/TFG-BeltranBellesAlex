
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class BichoPegajoso : MonoBehaviour
{
    public bool pegado;
    public MovimientoHistoria2 pegadoA;

    [SerializeField] private int empujeDst, empujeVel, saltoVel, distanciaPar;
    [SerializeField] private float objetivoRnd, saltoRnd;
    private NavMeshAgent agente;
    private Transform objetivoTrf, padre;
    private Vector3 posicionIni, objetivoOff, pegadoOff, puntoVue, escalaIni;
    private bool volando, sueloToc, parado;
    private CharacterController personajeCtr;
    private float puntoVueDst, gravedad, baseOff, fuerzaY;
    private Rigidbody cuerpoRig;


    // .
    private void Start ()
    {
        agente = this.GetComponent<NavMeshAgent> ();
        padre = this.transform.parent;
        posicionIni = this.transform.position;
        escalaIni = this.transform.localScale;
        personajeCtr = this.GetComponent<CharacterController> ();
        gravedad = -10;
        baseOff = agente.baseOffset;
        cuerpoRig = this.GetComponent<Rigidbody> ();
    }


    // .
    private void Update ()
    {
        parado = objetivoTrf == null && Vector3.Distance (this.transform.position, posicionIni) < distanciaPar;

        PerseguirObjetivo ();
    }


    // .
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("CuerpoGato") == true) 
        {
            //print (this.name + ": ME SUSEDIÓ.");
            MovimientoHistoria2 avatarMov = other.transform.parent.parent.GetComponent<MovimientoHistoria2> ();

            cuerpoRig.detectCollisions = false;
            pegado = true;
            agente.enabled = false;
            personajeCtr.enabled = true;
            pegadoA = avatarMov;

            avatarMov.Pegado (this);
            EncontrarHuesoCercano (avatarMov);
        }
    }


    // .
    private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        if (hit.transform.CompareTag ("CuerpoGato") == false) 
        {
            if ((personajeCtr.collisionFlags & CollisionFlags.Sides) != 0)
            {
                puntoVue = this.transform.position;
                //print (this.name + ": he tocado una pared. Mi posición es " + this.transform.position + " y mi nuevo destino es " + puntoVue);
            }
            if ((personajeCtr.collisionFlags & CollisionFlags.Below) != 0) 
            {
                sueloToc = true;
                //print (this.name + ": he tocado el suelo.");
            }
        }
    }


    // .
    public void AtacarA (Transform jugadorTrf) 
    {
        objetivoTrf = jugadorTrf;
        objetivoOff = new Vector3 (Random.Range (-objetivoRnd, +objetivoRnd), 0, Random.Range (-objetivoRnd, +objetivoRnd));
    }


    // .
    public void Parar () 
    {
        objetivoTrf = null;
    }


    // .
    public void SalirVolando () 
    {
        this.transform.parent = padre;
        this.transform.localScale = escalaIni;
        pegado = false;
        volando = true;
        puntoVue = this.transform.position - this.transform.forward * empujeDst;
        puntoVueDst = Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (puntoVue.x, puntoVue.z));
        agente.baseOffset = baseOff;
        fuerzaY = 0;
    }


    // .
    public void Derrotado () 
    {
        this.gameObject.SetActive (false);
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
                    agente.SetDestination (objetivoTrf.position + objetivoOff);
                }

                agente.velocity = agente.baseOffset > baseOff ? agente.desiredVelocity : Vector3.zero;
            }
            else 
            {
                float velocidadMov = Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (puntoVue.x, puntoVue.z)) * empujeVel / puntoVueDst;
                Vector3 movimiento = new Vector3(puntoVue.x - this.transform.position.x, 0, puntoVue.z - this.transform.position.z).normalized * velocidadMov;

                movimiento = new Vector3 (movimiento.x, fuerzaY, movimiento.z) * Time.deltaTime;
                print (this.name + ": me he movido " + movimiento);

                personajeCtr.Move (movimiento);
                print (this.name + ": mi velocidad es " + personajeCtr.velocity);
                if (sueloToc == true && this.IsInvoking ("SalirDeAturdimiento") == false && Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (puntoVue.x, puntoVue.z)) < distanciaPar) 
                {
                    this.Invoke ("SalirDeAturdimiento", 2);
                }
            }

            ControlarGravedad ();
        }
        /*else 
        {
            this.transform.localPosition = pegadoOff;
            this.transform.localRotation = Quaternion.identity;
        }*/
    }


    // .
    /*private void Pegarse () 
    {
        if (pegado == false && volando == false && objetivoTrf != null && Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (agente.destination.x, agente.destination.z)) < agente.stoppingDistance) 
        {
            agente.enabled = false;
            personajeCtr.enabled = true;

            if (this.name == "Bicho pegajoso prototipo (0)") 
            {
                print (this.transform.position);
                personajeCtr.Move ((agente.destination - this.transform.position) * pruebaVel);
                print (this.transform.position);
            }

            personajeCtr.enabled = false;
            agente.enabled = true;
            pegadoOff = this.transform.position - objetivoTrf.position;
            pegado = true;
            this.transform.parent = objetivoTrf;
            agente.isStopped = true;

            objetivoTrf.GetComponent<MovimientoHistoria2>().Pegado ();
        }
    }*/


    // .
    private void SalirDeAturdimiento () 
    {
        volando = false;
        personajeCtr.enabled = false;
        agente.enabled = true;
        cuerpoRig.detectCollisions = true;
        sueloToc = false;
        objetivoOff = new Vector3 (Random.Range (-objetivoRnd, +objetivoRnd), 0, Random.Range (-objetivoRnd, +objetivoRnd));
    }


    // .
    private void ControlarGravedad () 
    {
        if (pegado == false) 
        {
            if (volando == false)
            {
                if (agente.baseOffset > baseOff)
                {
                    agente.baseOffset += Time.deltaTime * fuerzaY;
                    fuerzaY += gravedad;
                }
                else
                {
                    agente.baseOffset = baseOff;
                    fuerzaY = 0;

                    if (parado == false && this.IsInvoking ("Saltar") == false)
                    {
                        this.Invoke ("Saltar", Random.Range (0.3f, 0.4f));
                    }
                }
            }
            else
            {
                if (sueloToc == false)
                {
                    fuerzaY += gravedad;
                }
                else
                {
                    fuerzaY = 0;
                }
            }
        }
    }


    // .
    private void Saltar () 
    {
        fuerzaY = saltoVel + Random.Range (-saltoRnd, +saltoRnd);
        agente.baseOffset += Time.deltaTime * fuerzaY;
    }


    // .
    private void EncontrarHuesoCercano (MovimientoHistoria2 avatarMov) 
    {
        float distanciaChc;

        Transform resultado = null;
        float distanciaMin = float.MaxValue;

        foreach (Transform h in avatarMov.huesos) 
        {
            distanciaChc = Vector3.Distance (this.transform.position, h.position);
            if (distanciaChc < distanciaMin) 
            {
                distanciaMin = distanciaChc;
                resultado = h;
            }
        }
        this.transform.parent = resultado;
    }
}