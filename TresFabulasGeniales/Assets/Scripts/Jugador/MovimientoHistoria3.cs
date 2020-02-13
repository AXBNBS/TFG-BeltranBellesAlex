
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MovimientoHistoria3 : MonoBehaviour
{
    public bool input;

    [SerializeField] private int movimientoVel, rotacionVel, saltoVel;
    [SerializeField] private LayerMask capas;
    private bool saltarInp, cayendo;
    private float horizontalInp, verticalInp, angulo, longitudRay;
    private Rigidbody rigidbody;
    private Vector3 movimiento;
    private Quaternion rotacion;
    private Transform camaraTrf;
    private Animator animator;


    // .
    private void Start ()
    {
        rigidbody = this.GetComponent<Rigidbody> ();
        longitudRay = this.GetComponent<CapsuleCollider>().height / 2 + 0.1f;
    }


    // .
    private void Update ()
    {
        if (input == false)
        {
            horizontalInp = 0;
            verticalInp = 0;
            saltarInp = false;
        }
        else 
        {
            horizontalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento horizontal"));
            verticalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento vertical"));
            saltarInp = Input.GetButtonDown ("Salto");
        }
        //cayendo = Physics.Raycast (this.transform.position, -this.transform.up, );
    }


    // .
    private void FixedUpdate ()
    {
        Mover ();
        Saltar ();
    }


    // .
    private void OnDrawGizmos ()
    {
        Gizmos.DrawRay (this.transform.position, -longitudRay * this.transform.up);
    }


    // .
    private void Mover () 
    {
        if (horizontalInp != 0 || verticalInp != 0) 
        {
            rigidbody.MovePosition (this.transform.position + this.transform.TransformDirection (horizontalInp, 0, verticalInp) * Time.deltaTime * movimientoVel);
        }
    }


    // .
    private void Saltar () 
    {
        if (saltarInp == true && cayendo == false) 
        {
            rigidbody.AddForce (this.transform.up, ForceMode.Impulse);
        }
    }
}