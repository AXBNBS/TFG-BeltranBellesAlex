
using UnityEngine;
using UnityEngine.AI;



[RequireComponent (typeof (SphereCollider))]
[RequireComponent (typeof (CapsuleCollider))]
public class Texto : MonoBehaviour
{
    public bool hablando;
    
    [SerializeField] private string[] texto;
    [SerializeField] private bool moviendose, hablarCmbRot, gira;
    [SerializeField] private float hablarRotX, hablarRotZ;
    [SerializeField] private GameObject bocadilloPrf;
    private bool cercano, esperar;
    private float rotacionVel;
    private Quaternion rotacionIni, rotacionObj;
    private Hablar jugador;
    private Hablar[] jugadores;
    private GameObject panelTxt, bocadilloObj;
    private NavMeshAgent agente;
    private Animator animador;
    private Transform modeloTrf, camaraTrf;


    // Obtenemos las referencias al jugador o jugadores.
    private void Start ()
    {
        CapsuleCollider capsula = this.GetComponent<CapsuleCollider> ();
        SphereCollider[] triggers = this.GetComponents<SphereCollider> ();

        rotacionVel = 5;
        jugadores = GameObject.FindObjectsOfType<Hablar> ();
        panelTxt = GameObject.FindGameObjectWithTag("Interfaz").transform.GetChild(0).GetChild(0).gameObject;
        bocadilloObj = GameObject.Instantiate (bocadilloPrf, new Vector3 (capsula.bounds.center.x, capsula.bounds.max.y + 5, capsula.bounds.center.z), Quaternion.identity);
        bocadilloObj.transform.parent = this.transform;
        if (moviendose == true) 
        {
            agente = this.GetComponent<NavMeshAgent> ();
        }
        animador = this.GetComponentInChildren<Animator> ();
        modeloTrf = animador.transform;
        rotacionIni = modeloTrf.rotation;
        camaraTrf = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform;
        foreach (SphereCollider t in triggers) 
        {
            t.isTrigger = true;
        }
        this.tag = "Hablable";
        this.gameObject.layer = LayerMask.NameToLayer ("GenteHabladora");

        bocadilloObj.SetActive (false);
    }


    // Si el jugador está cerca, el panel de texto no está activado, pulsa el botón para hablar y se le permite el input, se fijará una rotación objetivo para que el NPC pueda mirarle y se activará el booleano que indica que está hablando. En 
    //función de si este es verdadero o falso el NPC rotará hacia la rotación que le permita mirar al avatar del jugador o hacia su rotación inicial. Animamos también el NPC según si este está hablando o no.
    private void Update ()
    {
        jugador = jugadores[ObtenerIndiceJugador ()];
        if (cercano == true && esperar == false && panelTxt.activeSelf == false && Input.GetButtonDown ("Interacción") == true && jugador.hablables.Contains (this) == true) 
        {
            if (hablarCmbRot == false) 
            {
                rotacionObj = Quaternion.Euler (modeloTrf.rotation.eulerAngles.x, Quaternion.LookRotation(jugador.transform.position - this.transform.position).eulerAngles.y + 90, modeloTrf.rotation.eulerAngles.z);
            }
            else 
            {
                rotacionObj = Quaternion.Euler (hablarRotX, Quaternion.LookRotation(jugador.transform.position - this.transform.position).eulerAngles.y + 90, hablarRotZ);
            }
            if (moviendose == true) 
            {
                agente.enabled = false;
            }
        }
        if (gira == true && (moviendose == false || agente.enabled == false)) 
        {
            if (hablando == false)
            {
                modeloTrf.rotation = Quaternion.Lerp (modeloTrf.rotation, rotacionIni, Time.deltaTime * rotacionVel);
                //print (rotacionIni.eulerAngles);
            }
            else
            {
                modeloTrf.rotation = Quaternion.Lerp (modeloTrf.rotation, rotacionObj, Time.deltaTime * rotacionVel);
                //print (rotacionObj.eulerAngles);
            }
        }

        Animar ();
        if (bocadilloObj.activeSelf == true) 
        {
            bocadilloObj.transform.LookAt (camaraTrf);
        }
    }


    // Al acercarse el jugador a un objeto interactuable o un NPC, este objeto tiene en cuenta que el jugador se encuentra cerca.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            cercano = true;
        }
    }


    // Al alejarse el jugador a un objeto interactuable o un NPC, este objeto tiene en cuenta que el jugador acaba de alejarse.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            cercano = false;

            bocadilloObj.SetActive (false);
        }
    }


    // Desactivamos el booleano que dice que estamos hablando y para evitar problemas impedimos que este se reactive durante un pequeño periodo de tiempo.
    public void FinalizarConversacion () 
    {
        hablando = false;
        esperar = true;
        if (moviendose == true) 
        {
            agente.enabled = true;
        }

        this.Invoke ("AcabarEspera", 0.1f);
    }


    // Devuelve la array de texto asignada a este NPC.
    public string[] DevolverTexto () 
    {
        return texto;
    }


    // Activamos el icono sobre la cabeza del NPC o no según se nos indique.
    public void ActivarIcono (bool activar) 
    {
        bocadilloObj.SetActive (activar);
    }


    // Obtiene el índice de avatar controlado. En caso de que estemos en las historias 1 o 3 este será siempre 0.
    private int ObtenerIndiceJugador () 
    {
        if (jugadores.Length > 1) 
        {
            return (jugadores[0].input == true ? 0 : 1);
        }
        else 
        {
            return 0;
        }
    }


    // Llamada tras un pequeño periodo de tiempo para impedir que se reactive el booleano que nos permite hablar.
    private void AcabarEspera () 
    {
        esperar = false;
    }


    // Se anima al NPC de forma que su animación varíe si está hablando con el avatar o no.
    private void Animar () 
    {
        animador.SetBool ("hablando", hablando);
    }
}