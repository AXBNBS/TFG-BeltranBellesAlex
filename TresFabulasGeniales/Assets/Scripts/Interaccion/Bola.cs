
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Bola : MonoBehaviour
{
    public bool input;

    [SerializeField] private int movimientoVel;
    private int verticalInp, horizontalInp;
    private Rigidbody cuerpoRig;
    private Transform camara, trigger;
    private MovimientoHistoria3 jugador;
    private Vector3 triggerPos;
    private Quaternion triggerRot;


    // .
    private void Start ()
    {
        cuerpoRig = this.GetComponent<Rigidbody> ();
        camara = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform;
        trigger = this.transform.GetChild (0);
        jugador = GameObject.FindObjectOfType<MovimientoHistoria3> ();
        triggerPos = trigger.position - this.transform.position;
        triggerRot = trigger.rotation;
    }


    // .
    private void FixedUpdate ()
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
        trigger.position = this.transform.position + triggerPos;
        trigger.rotation = triggerRot;

        Mover ();
    }


    // .
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            jugador.bolaSup = trigger;
            input = true;
        }
    }


    // .
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            jugador.bolaSup = null;
            input = false;
            print (cuerpoRig.velocity);
        }
    }


    // .
    private void Mover () 
    {
        if (verticalInp != 0 || horizontalInp != 0) 
        {
            Vector3 relativoCam = camara.TransformDirection(new Vector3 (verticalInp, 0, horizontalInp)).normalized;

            cuerpoRig.AddForce (relativoCam * movimientoVel);
        }
    }
}