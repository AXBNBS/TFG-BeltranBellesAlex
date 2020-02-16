
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class YaVeremos : MonoBehaviour
{
    public bool input;

    [SerializeField] private int movimientoVel, rotacionVel, saltoVel;
    [SerializeField] private LayerMask capas;
    [SerializeField] private ForceMode tipoFrz;
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
        camaraTrf = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform;
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
        cayendo = !Physics.Raycast (this.transform.position, -this.transform.up, longitudRay, capas, QueryTriggerInteraction.Ignore);

        Mover ();
        Saltar ();
    }


    // .
    /*private void OnDrawGizmos ()
    {
        Gizmos.DrawRay (this.transform.position, -longitudRay * this.transform.up);
    }*/


    // .
    private void Mover () 
    {
        if (horizontalInp != 0 || verticalInp != 0) 
        {
            Vector3 relativoCam = camaraTrf.right * horizontalInp + camaraTrf.forward * verticalInp;

            relativoCam.y = 0;
            relativoCam = relativoCam.normalized;

            rigidbody.MovePosition (this.transform.position + relativoCam * Time.deltaTime * movimientoVel);

            angulo = Mathf.Atan2 (relativoCam.x, relativoCam.z) * Mathf.Rad2Deg;
            rotacion = Quaternion.Euler (this.transform.rotation.x, angulo, this.transform.rotation.z);
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacion, rotacionVel * Time.deltaTime);
        }
    }


    // .
    private void Saltar () 
    {
        if (saltarInp == true && cayendo == false) 
        {
            rigidbody.AddForce (this.transform.up * saltoVel, tipoFrz);
            rigidbody.velocity = Vector3.zero;
        }
    }
}