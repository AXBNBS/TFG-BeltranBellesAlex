
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class BichoPegajoso : MonoBehaviour
{
    public bool pegado;

    [SerializeField] private int empujeDst, empujeVel, saltoVel, pruebaVel;
    [SerializeField] private float aleatoriedad;
    private NavMeshAgent agente;
    private Transform objetivoTrf, padre;
    private Vector3 posicionIni, objetivoOff, pegadoOff, puntoVue;
    private bool volando;
    private CharacterController personajeCtr;
    private float puntoVueDst, gravedad, baseOff, fuerzaY;


    // .
    private void Start ()
    {
        agente = this.GetComponent<NavMeshAgent> ();
        posicionIni = this.transform.position;
        padre = this.transform.parent;
        personajeCtr = this.GetComponent<CharacterController> ();
        gravedad = -10;
        baseOff = agente.baseOffset;
    }


    // .
    private void Update ()
    {
        PerseguirObjetivo ();
        Pegarse ();
    }


    // .
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("CuerpoGato") == true) 
        {
            MovimientoHistoria2 avatarMov = objetivoTrf.GetComponent<MovimientoHistoria2> ();

            pegado = true;
            agente.isStopped = true;
            //this.transform.parent = objetivoTrf;

            avatarMov.Pegado ();
            EncontrarHuesoCercano (avatarMov);

            //pegadoOff = this.transform.position - this.transform.parent.position;
        }
    }


    // .
    public void AtacarA (Transform jugadorTrf) 
    {
        objetivoTrf = jugadorTrf;
        objetivoOff = new Vector3 (Random.Range (-aleatoriedad, +aleatoriedad), 0, Random.Range (-aleatoriedad, +aleatoriedad));
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
        pegado = false;
        volando = true;
        puntoVue = this.transform.position - this.transform.forward * empujeDst;
        puntoVueDst = Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (puntoVue.x, puntoVue.z));
        agente.enabled = false;
        personajeCtr.enabled = true;
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
                DarSaltitos ();

                agente.velocity = agente.baseOffset > baseOff ? agente.desiredVelocity : Vector3.zero;
            }
            else 
            {
                personajeCtr.Move ((puntoVue - this.transform.position).normalized * Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (puntoVue.x, puntoVue.z)) * empujeVel / puntoVueDst * 
                    Time.deltaTime);
                if (this.IsInvoking ("SalirDeAturdimiento") == false && Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (puntoVue.x, puntoVue.z)) < agente.stoppingDistance) 
                {
                    this.Invoke ("SalirDeAturdimiento", 1.5f);
                }
            }
        }
        else 
        {
            /*this.transform.localPosition = pegadoOff;
            this.transform.localRotation = Quaternion.identity;*/
        }
    }


    // .
    private void Pegarse () 
    {
        if (pegado == false && volando == false && objetivoTrf != null && Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (agente.destination.x, agente.destination.z)) < agente.stoppingDistance) 
        {
            /*agente.enabled = false;
            personajeCtr.enabled = true;

            if (this.name == "Bicho pegajoso prototipo (0)") 
            {
                print (this.transform.position);
                personajeCtr.Move ((agente.destination - this.transform.position) * pruebaVel);
                print (this.transform.position);
            }

            personajeCtr.enabled = false;
            agente.enabled = true;*/
            /*pegadoOff = this.transform.position - objetivoTrf.position;
            pegado = true;
            this.transform.parent = objetivoTrf;
            agente.isStopped = true;

            objetivoTrf.GetComponent<MovimientoHistoria2>().Pegado ();*/
        }
    }


    // .
    private void SalirDeAturdimiento () 
    {
        volando = false;
        personajeCtr.enabled = false;
        agente.enabled = true;
    }


    // .
    private void DarSaltitos () 
    {
        if (agente.baseOffset > baseOff)
        {
            agente.baseOffset += Time.deltaTime * fuerzaY;
            fuerzaY += gravedad;
        }
        else 
        {
            agente.baseOffset = baseOff;
            if (this.IsInvoking ("Saltar") == false && Vector3.Distance (this.transform.position, agente.destination) > agente.stoppingDistance) 
            {
                this.Invoke ("Saltar", Random.Range (0.3f, 0.4f));
            }
        }
    }


    // .
    private void Saltar () 
    {
        fuerzaY = saltoVel;
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