
using UnityEngine;



public class Bola : MonoBehaviour
{
    public bool input;

    [SerializeField] private int movimientoVel, velocidadMax;
    private int verticalInp, horizontalInp;
    private Rigidbody cuerpoRig;
    private Transform camara, trigger;
    private MovimientoHistoria3 jugador;
    private Vector3 triggerOff;
    private Quaternion triggerRot;


    // Inicialización de variables.
    private void Start ()
    {
        cuerpoRig = this.GetComponent<Rigidbody> ();
        camara = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform;
        trigger = this.transform.GetChild (0);
        jugador = GameObject.FindGameObjectWithTag("Jugador").GetComponent<MovimientoHistoria3> ();
        triggerOff = trigger.position - this.transform.position;
        triggerRot = Quaternion.identity;
    }


    // Obtenemos input del jugador si fuese necesario y ajustamos la posición del trigger superior para se mantenga siempre constante.
    private void Update ()
    {
        if (input == true)
        {
            verticalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento horizontal"));
            horizontalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento vertical"));
        }
        else
        {
            verticalInp = 0;
            horizontalInp = 0;
        }
        trigger.position = this.transform.position + triggerOff;
        trigger.rotation = triggerRot;
    }


    // Aplicamos la fuerza necesaria a la bola si se recibe input y su velocidad no supera el máximo.
    private void FixedUpdate ()
    {
        if (cuerpoRig.velocity.magnitude < velocidadMax && (verticalInp != 0 || horizontalInp != 0)) 
        {
            Mover ();
        }
    }


    // Si el jugador se encuentra en el trigger, guarda una referencia a la parte superior de la bola y su script. Además activamos el input de la misma.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            jugador.bolaSup = trigger;
            jugador.bolaScr = this;
            input = true;
        }
    }


    // Si el jugador sale del trigger, desactivamos el input de la bola.
    /*private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            input = false;
        }
    }*/


    // Se le aplica una fuerza a la bola en la dirección especificada y con un vector relativo a la cámara.
    private void Mover () 
    {
        cuerpoRig.AddForce (camara.TransformDirection(new Vector3 (verticalInp, 0, horizontalInp)).normalized * movimientoVel);
    }
}