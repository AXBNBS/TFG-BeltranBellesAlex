
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MovimientoHistoria1 : MonoBehaviour
{
    public bool input;

    [SerializeField] private int movimientoVel, rotacionVel;
    private int horizontalInp, verticalInp, gravedad;
    private Vector3 movimiento;
    private CharacterController personajeCtr;
    private Animator animador;
    private Transform camaraTrf;


    // .
    private void Start ()
    {
        gravedad = -10;
        personajeCtr = this.GetComponent<CharacterController> ();
        animador = this.GetComponentInChildren<Animator> ();
        camaraTrf = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform;
    }


    // .
    private void Update ()
    {
        if (input == false)
        {
            horizontalInp = 0;
            verticalInp = 0;
        }
        else
        {
            horizontalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento horizontal"));
            verticalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento vertical"));
        }
        movimiento.x = 0;
        movimiento.z = 0;

        Mover ();
        Animar ();
    }


    // .
    private void FixedUpdate ()
    {
        AplicarGravedad ();
    }


    // .
    private void Mover () 
    {
        if (horizontalInp != 0 || verticalInp != 0)
        {
            Vector3 relativoCam = (camaraTrf.right * horizontalInp + camaraTrf.forward * verticalInp).normalized * movimientoVel;
            movimiento.x = relativoCam.x;
            movimiento.z = relativoCam.z;
            this.transform.rotation = Quaternion.Slerp (this.transform.rotation, Quaternion.Euler (0, Mathf.Atan2 (movimiento.x, movimiento.z) * Mathf.Rad2Deg + 90, 0), Time.deltaTime * rotacionVel);

            personajeCtr.Move (Time.deltaTime * movimiento);
        }
    }


    // .
    private void Animar () 
    {
        animador.SetBool ("moviendose", (movimiento.x != 0 || movimiento.z != 0));
    }


    // .
    private void AplicarGravedad () 
    {
        if (personajeCtr.isGrounded == true) 
        {
            movimiento.y = -10;
        }
        else 
        {
            movimiento.y += gravedad;
        }
    }
}